using System;

namespace CP2077.Models.Save
{
    public class SaveGameChunk
    {
        public Guid Id { get; private set; }
        public int CompressedChunkSize { get; set; }
        public int DecompressedChunkSize { get; set; }
        public int EndOfChunkOffset { get; set; }
        public byte[] Skipped { get; private set; }
        public byte[] DecompressedData { get; set; }
        public byte[] CompressedData { get; set; }

        public SaveGameChunk()
        {
            Id = Guid.NewGuid();
            Skipped = new byte[8];
        }
    }
}
