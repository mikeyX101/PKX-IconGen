using System;

namespace PKXIconGen.Core.Exceptions;

public class TextureNameNotFoundException : Exception
{
    private const string BASE_MESSAGE = "Name ({0}) not found in NameMap";
    private string? Name { get; } = "Unknown Name";
    public override string Message => string.Format(BASE_MESSAGE, Name);

    public TextureNameNotFoundException()
    {
        
    }

    public TextureNameNotFoundException(string? name)
    {
        Name = name;
    }
}