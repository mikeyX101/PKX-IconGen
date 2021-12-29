namespace PKXIconGen.Core.Data.Blender
{
    // https://docs.blender.org/api/current/bpy.types.Light.html#bpy.types.Light.type
    public enum LightType : byte
    {
        Point,
        Sun,
        Spot,
        Area
    }

    public static class LightTypeExtensions
    {
        public static string GetBlenderName(this LightType type)
        {
            return type.ToString().ToUpper();
        }
    }
}
