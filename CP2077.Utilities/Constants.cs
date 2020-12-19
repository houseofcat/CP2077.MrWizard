namespace CP2077.Utilities
{
    public static class Constants
    {
        public struct SaveFile
        {
            public const string FIRST_FILE_HEADER_STRING = "VASC";
            public const string SECOND_FILE_HEADER_STRING = "FZLC";
        }

        public struct FileStructure
        {
            public const string OUTPUT_FOLDER_NAME = "Output";
            public const string UNCOMPRESSED_SUFFIX = "uncompressed";
            public const string METAINFORMATION_SUFFIX = "metainf";
            public const string RECOMPRESSED_SUFFIX = "recompressed";
            public const string SAVE_FILE_ENDING = ".bin";
            public const string METAINFORMATION_FILE_ENDING = ".json";
        }
    }
}
