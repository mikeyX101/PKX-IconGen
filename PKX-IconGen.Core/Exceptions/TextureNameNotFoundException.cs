using System;

namespace PKXIconGen.Core.Exceptions;

public class TextureNameNotFoundException : Exception
{
    private const string BASE_MESSAGE = "Name ({0}) not found in NameMap";
    public string? Name { get; init; } = "Unknown Name";
    public override string Message => string.Format(BASE_MESSAGE, Name);
    
    public TextureNameNotFoundException() : base() {}

    public TextureNameNotFoundException(string? name) : base()
    {
        Name = name;
    }
}