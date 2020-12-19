using System.Linq;

namespace CP2077.Models.Save
{
    public class SaveGame
    {
        public SaveMetadata SaveMetadata { get; set; }
        public SaveGameChunk[] SaveGameChunk { get; set; }

        private int _curPos = 0;

        public SaveGameChunk ReadNextDataChunk()
        {
            if (_curPos >= SaveGameChunk.Length)
            { return null; }
            else
            { return SaveGameChunk[_curPos++]; }
        }

        public void Reset() => _curPos = 0;

        public int TotalCompressedSize => SaveGameChunk.Sum(sgc => sgc.CompressedChunkSize);
        public int TotalDecompressedSize => SaveGameChunk.Sum(sgc => sgc.DecompressedChunkSize);
    }
}
