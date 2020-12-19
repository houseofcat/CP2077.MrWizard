using CP2077.Models.Save;
using HouseofCat.Compression;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CP2077.Utilities
{
    public static class SaveHelper
    {
        private static readonly LZ4CodecProvider _lz4CodecProvider = new LZ4CodecProvider(K4os.Compression.LZ4.LZ4Level.L00_FAST);

        public static SaveGame DecompressSave(Stream input)
        {
            var saveGame = ReadCompressedSave(input);

            // Marking rest of the content for the metadata.
            byte[] buffer = new byte[input.Length - input.Position];
            input.Read(buffer, 0, buffer.Length);
            saveGame.SaveMetadata.RestOfContent = buffer;

            return saveGame;
        }

        public static void WriteCompressedSave(SaveGame saveGame)
        {
            WriteDecompressedSaveFile(saveGame);
            WriteDecompressedMetadataFile(saveGame.SaveMetadata);
        }

        public static async Task CompressSaveGameFileAsync(string inputFileName, string metadataFileNamePath)
        {
            var metadata = await ReadSaveMetadataAsync(metadataFileNamePath);

            //var dataToCompress = new List<byte[]>();
            //using (var memoryStream = new MemoryStream())
            //{
            //    using (var writer = new BinaryWriter(memoryStream, Encoding.UTF8))
            //    {
            //        using (var stream = File.OpenRead(inputFileName))
            //        {
            //            using (var reader = new BinaryReader(stream, Encoding.UTF8, true))
            //            {
            //                long remainingBytes = reader.BaseStream.Length;
            //                while (remainingBytes > 262144)
            //                {
            //                    var uncompressedBytes = reader.ReadBytes(262144);
            //                    dataToCompress.Add(uncompressedBytes);
            //                    remainingBytes -= 262144;
            //                }
            //                var lastBytes = reader.ReadBytes((int)remainingBytes);
            //                dataToCompress.Add(lastBytes);

            //            }
            //        }
            //        writer.Write(metadata.FirstHeaderBytes);
            //        writer.Write(metadata.Skipped);
            //        writer.Write(metadata.SecondFileHeaderBytes);
            //        writer.Write(dataToCompress.Count);
            //        writer.Write(metadata.HeaderSize);
            //        int offset = metadata.HeaderSize;
            //        int index = 0;

            //        foreach (var bytesToCompress in dataToCompress)
            //        {
            //            var target = new byte[LZ4Codec.MaximumOutputSize(bytesToCompress.Length)];
            //            int actualSize = LZ4Codec.Encode(bytesToCompress, target, compressionLevel);
            //            var compressedData = new byte[actualSize];
            //            int fakeSize = actualSize + 8;
            //            Array.Copy(target, compressedData, actualSize);
            //            Span<byte> outputData = new byte[bytesToCompress.Length];
            //            Span<byte> inputData = compressedData;
            //            int bytesDecoded = LZ4Codec.Decode(inputData, outputData);
            //            writer.Write(fakeSize);//CompressedChunkSize
            //            writer.Write(bytesDecoded);//DecompressedChunkSize
            //            offset = offset + fakeSize;
            //            if (index < dataToCompress.Count - 1)
            //            {
            //                writer.Write(offset);//EndOfChunkOffset
            //            }
            //            else
            //            {
            //                writer.Write(0);
            //            }
            //            index++;
            //        }
            //        writer.Write(MetaInformation.TrailingFileHeaderContent);
            //        foreach (var bytesToCompress in dataToCompress)
            //        {
            //            writer.Write(new byte[] { 52, 90, 76, 88 });
            //            var target = new byte[LZ4Codec.MaximumOutputSize(bytesToCompress.Length)];
            //            int actualSize = LZ4Codec.Encode(bytesToCompress, target, compressionLevel);
            //            var compressedData = new byte[actualSize];
            //            int fakeSize = actualSize + 8;
            //            Array.Copy(target, compressedData, actualSize);
            //            Span<byte> outputData = new byte[bytesToCompress.Length];
            //            Span<byte> inputData = compressedData;
            //            int bytesDecoded = LZ4Codec.Decode(inputData, outputData);
            //            if (bytesDecoded != bytesToCompress.Length)
            //            {
            //                int a = 1;
            //            }
            //            writer.Write(bytesDecoded);
            //            writer.Write(compressedData);
            //        }
            //        writer.Write(MetaInformation.RestOfContent, 0, MetaInformation.RestOfContent.Length - 8);
            //        writer.Write(offset);
            //        writer.Write(new byte[] { 0x45, 0x4E, 0x4F, 0x44 });
            //        using (var fileStream = File.Create($"{Constants.FileStructure.OUTPUT_FOLDER_NAME}\\{MetaInformation.FileGuid}_{Constants.FileStructure.RECOMPRESSED_SUFFIX}.bin"))
            //        {
            //            memoryStream.Seek(0, SeekOrigin.Begin);
            //            memoryStream.CopyTo(fileStream);
            //        }
            //    }
            //}
        }

        private static async Task<SaveMetadata> ReadSaveMetadataAsync(string metadatFileNamePath)
        {
            var metaText = await File.ReadAllTextAsync(metadatFileNamePath);
            return JsonSerializer.Deserialize<SaveMetadata>(metaText);
        }

        public static void WriteDecompressedSave(SaveGame saveGame)
        {
            WriteDecompressedSaveFile(saveGame);
            WriteDecompressedMetadataFile(saveGame.SaveMetadata);
        }

        private static void WriteDecompressedSaveFile(SaveGame saveGame)
        {
            using var stream = new FileStream(
                DecompressedSaveFileNamePath(saveGame.SaveMetadata.FileGuid),
                FileMode.Create);

            foreach (var chunk in saveGame.SaveGameChunk)
            {
                stream.Write(chunk.DecompressedData, 0, chunk.DecompressedData.Length);
            }
        }

        private static readonly string DecompressedSaveFileNamePathTemplate = $"{0}\\{1}_{2}{3}";
        private static string DecompressedSaveFileNamePath(Guid fileId)
        {
            return string.Format(
                DecompressedSaveFileNamePathTemplate,
                Constants.FileStructure.OUTPUT_FOLDER_NAME,
                fileId,
                Constants.FileStructure.UNCOMPRESSED_SUFFIX,
                Constants.FileStructure.SAVE_FILE_ENDING);
        }

        private static void WriteDecompressedMetadataFile(SaveMetadata metadata)
        {
            File.WriteAllText(
                DecompressedMetaFileNamePath(metadata.FileGuid),
                JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = true }));
        }

        private static readonly string DecompressedMetaFileNamePathTemplate = $"{0}\\{1}_{2}{3}";
        private static string DecompressedMetaFileNamePath(Guid fileId)
        {
            return string.Format(
                DecompressedMetaFileNamePathTemplate,
                Constants.FileStructure.OUTPUT_FOLDER_NAME,
                fileId,
                Constants.FileStructure.METAINFORMATION_SUFFIX,
                Constants.FileStructure.METAINFORMATION_FILE_ENDING);
        }

        public static SaveGame ReadCompressedSave(Stream input)
        {
            var metadata = ReadSaveMetadata(input);

            return ReadCompressedSaveFromStream(input, metadata);
        }

        public static SaveMetadata ReadSaveMetadata(Stream input)
        {
            var metadata = new SaveMetadata();
            using var reader = new BinaryReader(input, Encoding.UTF8, true);

            var resumePosition = reader.BaseStream.Position;
            metadata.FirstFileHeaderMarker = reader.ReadString(4);
            reader.BaseStream.Position = resumePosition;
            metadata.FirstHeaderBytes = reader.ReadBytes(4);
            if (metadata.FirstFileHeaderMarker != Constants.SaveFile.FIRST_FILE_HEADER_STRING)
            {
                throw new InvalidOperationException();
            }

            metadata.Skipped = reader.ReadBytes(21); // Currently Unknown data
            resumePosition = reader.BaseStream.Position;
            metadata.SecondFileHeaderMarker = reader.ReadString(4);
            reader.BaseStream.Position = resumePosition;
            metadata.SecondFileHeaderBytes = reader.ReadBytes(4);
            if (metadata.SecondFileHeaderMarker != Constants.SaveFile.SECOND_FILE_HEADER_STRING)
            {
                throw new InvalidOperationException();
            }
            metadata.ChunkCount = reader.ReadInt32();
            metadata.HeaderSize = reader.ReadInt32();
            return metadata;
        }

        public static SaveGame ReadCompressedSaveFromStream(Stream inputStream, SaveMetadata metadata)
        {
            var saveGameChunk = new SaveGameChunk[metadata.ChunkCount];
            using (var reader = new BinaryReader(inputStream, Encoding.UTF8, true))
            {
                for (int i = 0; i < metadata.ChunkCount; i++)
                {
                    saveGameChunk[i] = new SaveGameChunk
                    {
                        CompressedChunkSize = reader.ReadInt32(),
                        DecompressedChunkSize = reader.ReadInt32(),
                        EndOfChunkOffset = reader.ReadInt32()
                    };

                }
            }

            metadata.PartialHeaderSize = metadata.HeaderSize - (inputStream.Position);
            metadata.TrailingFileHeaderContent = new byte[metadata.PartialHeaderSize];

            inputStream.Read(metadata.TrailingFileHeaderContent);
            inputStream.Position = metadata.HeaderSize;

            //var data = new byte[metadata.HeaderSize + saveGameChunk.Sum(c => c.DecompressedChunkSize)];
            foreach (var chunk in saveGameChunk)
            {
                ReadCompressedChunkDataFromStream(inputStream, chunk);
            }

            return new SaveGame
            {
                SaveGameChunk = saveGameChunk,
                SaveMetadata = metadata,
            };
        }

        public static void ReadCompressedChunkDataFromStream(Stream inputStream, SaveGameChunk data)
        {
            data.CompressedData = new byte[data.CompressedChunkSize - 8];
            data.DecompressedData = new byte[data.DecompressedChunkSize];

            inputStream.Read(data.Skipped);
            inputStream.Read(data.CompressedData);

            var decodedCount = _lz4CodecProvider.Decode(data.CompressedData, data.DecompressedData);

            // Extra validation
            if (decodedCount != data.DecompressedChunkSize) throw new InvalidOperationException("Bytes decoded don't match bytes to be decompressed.");
            if (inputStream.Position != data.EndOfChunkOffset || data.EndOfChunkOffset != 0) throw new InvalidOperationException("EndPosition is not at the end of the current chunk of data.");
        }
    }
}
