using System;
using System.IO;
using System.Text.Json;
using PKXIconGen.Core.Data;
using PKXIconGen.Core.Exceptions;

namespace PKXIconGen.Core;

public static class NameMap
{
    // Dispose manually after all operations or application close
    private static JsonDocument? NamesMap { get; set; } = null;
    private static Game NameMapGame { get; set; } = Game.Undefined;
    public static void LoadNamesMap(Game forGame)
    {
        if (forGame == NameMapGame)
        {
            return;
        }
        
        CleanUp();
            
        if (forGame == Game.Undefined)
        {
            NameMapGame = Game.Undefined;
            return;
        }

        string? nameMapPath = forGame switch 
        {
            Game.PokemonColosseum => Paths.ColoNameMapFile,
            Game.PokemonXDGaleOfDarkness => Paths.XDNameMapFile,
            _ => null
        };
        if (nameMapPath is null)
        {
            return;
        }

        if (!File.Exists(nameMapPath))
        {
            Console.Error.WriteLine("Map for " + forGame + " not found: " + nameMapPath);
            CoreManager.Logger.Error("Map for {ForGame} not found : {MapPath}", forGame, nameMapPath);
            return;
        }

        using FileStream file = File.Open(nameMapPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        NamesMap = JsonDocument.Parse(file, new JsonDocumentOptions { MaxDepth = 4 });
        NameMapGame = forGame;
    }

    public static TextureNames? GetTextureNames(string? japaneseName)
    {
        if (NameMapGame == Game.Undefined || japaneseName is null)
        {
            return null;
        }

        if (NamesMap is null)
        {
            throw new InvalidOperationException("No NameMap has been loaded, load with NameMap.LoadNamesMap()");
        }

        if (NamesMap.RootElement.TryGetProperty(japaneseName, out JsonElement prop))
        {
            try
            {
                return prop.Deserialize<TextureNames>();
            }
            catch (JsonException)
            {
                CoreManager.Logger.Warning("Got property from NameMap for {game}, but the name provided didn't exist: {name}", NameMapGame, japaneseName);
                return null;
            }
        }
        
        throw new TextureNameNotFoundException(japaneseName);
    }
    
    public static void CleanUp()
    {
        NamesMap?.Dispose();
        NamesMap = null;
    }
}