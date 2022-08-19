#region License
/*  PKX-IconGen.AvaloniaUI - Avalonia user interface for PKX-IconGen.Core
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
using System.Threading;
using JetBrains.Annotations;
using PKXIconGen.Core.Services;
using ReactiveUI;

namespace PKXIconGen.AvaloniaUI.ViewModels
{
    public class TextureDownloadWindowViewModel : WindowViewModelBase, IDisposable
    {
        private bool downloading;
        public bool Downloading
        {
            get => downloading;
            set => this.RaiseAndSetIfChanged(ref downloading, value);
        }
        
        private double progress;
        public double Progress
        {
            get => progress;
            set => this.RaiseAndSetIfChanged(ref progress, value);
        }

        private string statusText = "";
        public string StatusText
        {
            get => statusText;
            set => this.RaiseAndSetIfChanged(ref statusText, value);
        }

        private string AssetsPath { get; init; }
        
        public TextureDownloadWindowViewModel(string assetsPath)
        {
            Downloading = false;
            Progress = 0;
            StatusText = "Standing by...";

            AssetsPath = assetsPath;
        }

        private CancellationTokenSource? cancelDownloadTokenSource;
        [UsedImplicitly]
        public async void Download()
        {
            DisposeCancelDownloadRenderToken();
            cancelDownloadTokenSource = new CancellationTokenSource();

            Downloading = true;
            Progress = 0;
            StatusText = "Downloading... 0%";
            using TexturesInstaller installer = new(AssetsPath, p =>
            {
                Progress = p;
                StatusText = $"Downloading... {Progress:##0.#}%"; // Lots of reallocation, but it's not the end of the world
            });
            try
            {
                if (await installer.DownloadAsync(cancelDownloadTokenSource.Token))
                {
                    Progress = 100; // In case file already exists
                    StatusText = "Installing...";
                    await installer.ExtractAsync();
                }
            }
            catch (OperationCanceledException) { }
            finally
            {
                EndDownload();
            }
        }

        [UsedImplicitly]
        public void Cancel() => EndDownload();

        private void EndDownload()
        {
            Downloading = false;
            StatusText = "Finished!";
            DisposeCancelDownloadRenderToken();
        }
        
        private void DisposeCancelDownloadRenderToken()
        {
            // Make sure tokens are canceled
            if (cancelDownloadTokenSource != null)
            {
                cancelDownloadTokenSource.Cancel();
                cancelDownloadTokenSource.Dispose();
                cancelDownloadTokenSource = null;
            }
        }
        
        public void Dispose()
        {
            DisposeCancelDownloadRenderToken();
        }
    }
}
