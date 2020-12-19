using System.IO;

namespace CP2077.Utilities
{
    public static class Extensions
    {
        public static string ReadString(this BinaryReader reader, int count)
        {
            return new string(reader.ReadChars(count));
        }
    }
}
