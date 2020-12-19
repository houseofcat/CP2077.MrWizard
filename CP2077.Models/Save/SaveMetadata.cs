using System;

namespace CP2077.Models.Save
{
    public class SaveMetadata
    {
        public int ChunkCount { get; set; }
        public int HeaderSize { get; set; }
        public long PartialHeaderSize { get; set; }
        public string FirstFileHeaderMarker { get; set; }
        public byte[] FirstHeaderBytes { get; set; }
        public string SecondFileHeaderMarker { get; set; }
        public byte[] SecondFileHeaderBytes { get; set; }
        public byte[] Skipped { get; set; }
        public byte[] TrailingFileHeaderContent { get; set; }
        public byte[] RestOfContent { get; set; }
        public Guid FileGuid { get; set; }
    }
}
