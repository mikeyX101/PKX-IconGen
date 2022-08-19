#region License
/*  PKX-IconGen.Core - Pokemon Icon Generator for GCN/WII Pokemon games
    Copyright (C) 2021-2022 Samuel Caron/mikeyX#4697

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CG.Web.MegaApiClient;

namespace PKXIconGen.Core.Services
{
    public class TexturesInstaller : IDisposable
    {
        public const string MEGA_FOLDER_URL = "https://mega.nz/folder/9ZZmUa5b#y157hcF9D7i0REq6RyaqEg";
        private static string ZipTarget => Path.Combine(Paths.TempFolder, "hd_textures.zip");
        private string ZipExtractTarget => Path.Combine(AssetsPath, "icon-gen", "hd-mikeyx");

        private Task LoginTask { get; }
        private readonly IMegaApiClient MegaClient = new MegaApiClient();
        private string AssetsPath { get; }
        private Action<double>? OnProgress { get; }

        public TexturesInstaller(string assetsPath, Action<double>? onProgress = null)
        {
            AssetsPath = assetsPath;
            OnProgress = onProgress;

            LoginTask = LoginAsync();
        }

        private async Task LoginAsync()
        {
            await MegaClient.LoginAnonymousAsync();
        }

        public async Task<bool> DownloadAsync(CancellationToken token = default)
        {
            static IEnumerable<byte> CleanHash(byte[] hash)
            {
                return Encoding.UTF8.GetBytes(
                    BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant()
                );
            }

            await LoginTask;
            token.ThrowIfCancellationRequested();

            Uri folderLink = new(MEGA_FOLDER_URL);
            INode[] nodes = (await MegaClient.GetNodesFromLinkAsync(folderLink)).ToArray();

            INode? texturesZip = nodes.SingleOrDefault(n => n.Name == "latest.zip");
            if (texturesZip is null)
            {
                CoreManager.Logger.Error("Can't find latest.zip node");
                return false;
            }

            INode? texturesZipHash = nodes.SingleOrDefault(n => n.Name == "latest.zip.sha256");
            if (texturesZipHash is null)
            {
                CoreManager.Logger.Error("Can't find latest.zip.sha256 node");
                return false;
            }

            await using Stream hashDownloadStream = await MegaClient.DownloadAsync(texturesZipHash, null, token);
            byte[] expectedHash = new byte[64];
            _ = await hashDownloadStream.ReadAsync(expectedHash, 0, 64, token);
            token.ThrowIfCancellationRequested();

            if (File.Exists(ZipTarget))
            {
                await using FileStream existingFileStream = File.OpenRead(ZipTarget);
                using SHA256 fileSHA256 = SHA256.Create();
                IEnumerable<byte> fileHash = CleanHash(await fileSHA256.ComputeHashAsync(existingFileStream, token));

                if (expectedHash.SequenceEqual(fileHash))
                {
                    CoreManager.Logger.Information("File found and has same hash");
                    return true;
                }

                CoreManager.Logger.Information("File found but hash didn't match");
            }

            CoreManager.Logger.Information("Downloading textures...");
            IProgress<double>? progressHandler = OnProgress is not null ? new Progress<double>(OnProgress) : null;
            await using Stream downloadStream = await MegaClient.DownloadAsync(texturesZip, progressHandler, token);
            // File should be around 200MB, which is reasonable enough to have in memory to compute hash instantly
            await using Stream memoryStream = new MemoryStream((int)texturesZip.Size);
            await downloadStream.CopyToAsync(memoryStream, token);

            using SHA256 downloadSHA256 = SHA256.Create();
            memoryStream.Position = 0;
            IEnumerable<byte> computedHash = CleanHash(await downloadSHA256.ComputeHashAsync(memoryStream, token));
            token.ThrowIfCancellationRequested();

            if (!expectedHash.SequenceEqual(computedHash))
            {
                CoreManager.Logger.Error("Downloaded archive doesn't match expected hash");
                return false;
            }

            await using FileStream fileStream = File.OpenWrite(ZipTarget);
            memoryStream.Position = 0;
            await memoryStream.CopyToAsync(fileStream, token);

            CoreManager.Logger.Information("Downloaded textures successfully");
            return true;
        }

        public Task ExtractAsync(CancellationToken token = default)
        {
            return Task.Run(() =>
            {
                if (!Directory.Exists(ZipExtractTarget))
                {
                    Directory.CreateDirectory(ZipExtractTarget);
                }

                CoreManager.Logger.Information("Extracting textures...");
                ZipFile.ExtractToDirectory(ZipTarget, ZipExtractTarget, true);
                CoreManager.Logger.Information("Extracted textures successfully");
            }, token);
        }

        public void Dispose()
        {
            MegaClient.Logout();

            LoginTask.Dispose();
            GC.SuppressFinalize(this);
            GC.Collect(GC.GetGeneration(this));
        }
    }
}