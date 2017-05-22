using System;
using System.Collections.Generic;
using Helpers;
using System.IO;

namespace UAssetTools
{
    public class GuidCustomVersion_DEPRECATED : BinaryHelper
    {
        public Guid Key;
        public Int32 Version;
        public string FriendlyName;

        public void DeSerialize(Stream fs)
        {
            Key = ReadGuid(fs);
            Version = ReadInt32(fs);
            FriendlyName = ReadString(fs);
        }

        public void Serialize(Stream fs)
        {
            WriteGuid(fs, Key);
            WriteInt32(fs, Version);
            WriteString(fs, FriendlyName);
        }
    }

    public class FileSummary : BinaryHelper
    {
        public Int32 Tag;
        public Int32 LegacyFileVersion;
        public Int32 LegacyUE3Version;
        public static Int32 FileVersionUE4;
        public Int32 FileVersionLicenseeUE4;
        public static bool bUnversioned;
        public List<GuidCustomVersion_DEPRECATED> VersionArray;
        public Int32 TotalHeaderSize;
        public string FolderName;
        public UInt32 PackageFlags;
        public Int32 NameCount;
        public Int32 NameOffset;
        public Int32 GatherableTextDataCount;
        public Int32 GatherableTextDataOffset;
        public Int32 ExportCount;
        public Int32 ExportOffset;
        public Int32 ImportCount;
        public Int32 ImportOffset;
        public Int32 DependsOffset;
        public Int32 StringAssetReferencesCount;
        public Int32 StringAssetReferencesOffset;
        public Int32 ThumbnailTableOffset;
        public Guid Guid;
        public List<GenerationInfo> Generations;
        public EngineVersion SavedByEngineVersion;
        public EngineVersion CompatibleWithEngineVersion;
        public UInt32 CompressionFlags;
        public UInt32 PackageSource;
        public TextureAllocations TextureAllocations;
        public Int32 AssetRegistryDataOffset;
        public static Int64 BulkDataStartOffset;
        public Int32 WorldTileInfoDataOffset;
        public List<Int32> ChunkIDs;

        public Int64 TotalHeaderSizeOffset;
        public Int64 NameCountOffset;
        public Int64 NameOffsetOffset;
        public Int64 ExportCountOffset;
        public Int64 ExportOffsetOffset;
        public Int64 ImportCountOffset;
        public Int64 ImportOffsetOffset;
        public Int64 BulkDataStartOffsetOffset;
        public Int64 DependsOffsetOffset;
        public Int64 StringAssetReferencesOffsetOffset;
        public Int64 AssetRegistryDataOffsetOffset;

        public FileSummary()
        {
            VersionArray = new List<GuidCustomVersion_DEPRECATED>();
            Generations = new List<GenerationInfo>();
            SavedByEngineVersion = new EngineVersion();
            CompatibleWithEngineVersion = new EngineVersion();
            TextureAllocations = new TextureAllocations();
            ChunkIDs = new List<int>();
            BulkDataStartOffset = 0;
        }

        public void DeSerialize(Stream fs)
        {
            fs.Seek(0, SeekOrigin.Begin);
            Tag = ReadInt32(fs);
            if (Tag != -1641380927)
                throw new Exception("This is not uasset!");
            LegacyFileVersion = ReadInt32(fs);
            if (LegacyFileVersion < -7)
                throw new Exception("LegacyFileVersion < -7 not supported!");
            LegacyUE3Version = ReadInt32(fs);
            FileVersionUE4 = ReadInt32(fs);
            FileVersionLicenseeUE4 = ReadInt32(fs);
            bUnversioned = (FileVersionUE4 == 0 && FileVersionLicenseeUE4 == 0);
            int nVersionsCount = ReadInt32(fs);
            for (int i = 0; i < nVersionsCount; i++)
            {
                VersionArray.Add(new GuidCustomVersion_DEPRECATED());
                VersionArray[i].DeSerialize(fs);
            }
            TotalHeaderSize = ReadInt32(fs);
            FolderName = ReadString(fs);
            PackageFlags = ReadUInt32(fs);
            if ((PackageFlags & 0x80000000) != 0x80000000)
                throw new Exception("Flag must be set!");
            NameCount = ReadInt32(fs);
            NameOffset = ReadInt32(fs);
            if (bUnversioned || FileVersionUE4 >= 459) // VER_UE4_SERIALIZE_TEXT_IN_PACKAGES
            {
                GatherableTextDataCount = ReadInt32(fs);
                if (GatherableTextDataCount > 0)
                    throw new Exception("GatherableTextDataCount not supported!");
                GatherableTextDataOffset = ReadInt32(fs);
            }
            ExportCount = ReadInt32(fs);
            ExportOffset = ReadInt32(fs);
            ImportCount = ReadInt32(fs);
            ImportOffset = ReadInt32(fs);
            DependsOffset = ReadInt32(fs);
            if (!bUnversioned && FileVersionUE4 < 384)
                throw new Exception("This version not supported!");
            StringAssetReferencesCount = ReadInt32(fs);
            if (StringAssetReferencesCount > 0)
                throw new Exception("StringAssetReferences not supported!");
            StringAssetReferencesOffset = ReadInt32(fs);
            ThumbnailTableOffset = ReadInt32(fs);
            if (ThumbnailTableOffset > 0)
                throw new Exception("ThumbnailTableOffset not supported!");
            Guid = ReadGuid(fs);
            Int32 GenerationCount = ReadInt32(fs);
            for (int i = 0; i < GenerationCount; i++)
            {
                Generations.Add(new GenerationInfo());
                Generations[i].DeSerialize(fs);
            }
            if (!bUnversioned && FileVersionUE4 < 336)
                throw new Exception("This version not supported!");
            SavedByEngineVersion.DeSerialize(fs);
            if (!bUnversioned && FileVersionUE4 < 444)
                throw new Exception("This version not supported!");
            CompatibleWithEngineVersion.DeSerialize(fs);
            CompressionFlags = ReadUInt32(fs);
            int nCompressedChunksCount = ReadInt32(fs);
            if (nCompressedChunksCount > 0)
                throw new Exception("CompressedChunks not supported!");
            PackageSource = ReadUInt32(fs);
            int nAdditionalPackagesToCookCount = ReadInt32(fs);
            if (nAdditionalPackagesToCookCount > 0)
                throw new Exception("AdditionalPackagesToCook not supported!");
            TextureAllocations.DeSerialize(fs);
            AssetRegistryDataOffset = ReadInt32(fs);
            BulkDataStartOffset = ReadInt64(fs);
            if (!bUnversioned && FileVersionUE4 < 224)
                throw new Exception("This version not supported!");
            WorldTileInfoDataOffset = ReadInt32(fs);
            if (WorldTileInfoDataOffset > 0)
                throw new Exception("WorldTileInfoDataOffset not supported!");
            if (!bUnversioned && FileVersionUE4 < 326)
                throw new Exception("This version not supported!");
            int nChunkIDsCount = ReadInt32(fs);
            for (int i = 0; i < nChunkIDsCount; i++)
                ChunkIDs.Add(ReadInt32(fs));
        }

        public void Serialize(Stream fs)
        {
            WriteInt32(fs, Tag);
            WriteInt32(fs, LegacyFileVersion);
            WriteInt32(fs, LegacyUE3Version);
            WriteInt32(fs, FileVersionUE4);
            WriteInt32(fs, FileVersionLicenseeUE4);
            bool bUnversioned = (FileVersionUE4 == 0 && FileVersionLicenseeUE4 == 0);
            if (bUnversioned)
                throw new Exception("FileVersionUE4 == 0 and FileVersionLicenseeUE4 == 0 not supported for write!");
            WriteInt32(fs, VersionArray.Count);
            for (int i = 0; i < VersionArray.Count; i++)
                VersionArray[i].Serialize(fs);
            TotalHeaderSizeOffset = fs.Position; WriteInt32(fs, 0); // POST: TotalHeaderSize
            WriteString(fs, FolderName);
            WriteUInt32(fs, PackageFlags);
            NameCountOffset = fs.Position; WriteInt32(fs, 0); // POST: NameCount
            NameOffsetOffset = fs.Position; WriteInt32(fs, 0); // POST: NameOffset
            if (FileVersionUE4 >= 459) // VER_UE4_SERIALIZE_TEXT_IN_PACKAGES
            {
                WriteInt32(fs, 0); // GatherableTextDataCount
                WriteInt32(fs, 0); // GatherableTextDataOffset
            }
            ExportCountOffset = fs.Position; WriteInt32(fs, 0); // POST: ExportCount
            ExportOffsetOffset = fs.Position; WriteInt32(fs, 0); // POST: ExportOffset
            ImportCountOffset = fs.Position; WriteInt32(fs, 0); // POST: ImportCount
            ImportOffsetOffset = fs.Position; WriteInt32(fs, 0); // POST: ImportOffset
            DependsOffsetOffset = fs.Position; WriteInt32(fs, 0); // POST: DependsOffset
            WriteInt32(fs, 0); //StringAssetReferencesCount, not supported
            StringAssetReferencesOffsetOffset = fs.Position; WriteInt32(fs, StringAssetReferencesOffset); // POST: StringAssetReferencesOffset
            WriteInt32(fs, 0); // ThumbnailTableOffset, not supported
            WriteGuid(fs, Guid);
            WriteInt32(fs, Generations.Count);
            for (int i = 0; i < Generations.Count; i++)
                Generations[i].Serialize(fs);
            SavedByEngineVersion.Serialize(fs);
            CompatibleWithEngineVersion.Serialize(fs);
            WriteUInt32(fs, CompressionFlags);
            WriteInt32(fs, 0); // nCompressedChunksCount
            WriteUInt32(fs, PackageSource);
            WriteInt32(fs, 0); // nAdditionalPackagesToCookCount
            TextureAllocations.Serialize(fs);
            AssetRegistryDataOffsetOffset = fs.Position; WriteInt32(fs, 0); // POST: AssetRegistryDataOffset
            BulkDataStartOffsetOffset = fs.Position; WriteInt64(fs, 0); // POST: BulkDataStartOffset
            WriteInt32(fs, WorldTileInfoDataOffset);
            WriteInt32(fs, ChunkIDs.Count);
            for (int i = 0; i < ChunkIDs.Count; i++)
               WriteInt32(fs, ChunkIDs[i]);
        }

        public void Correction(Stream fs, Int32 TotalHeaderSize, Int32 NameCount, Int32 NameOffset, Int32 ExportCount, Int32 ExportOffset, Int32 ImportCount, Int32 ImportOffset, Int32 BulkDataStartOffset, Int32 DependsOffset, Int32 StringAssetReferencesOffset, Int32 AssetRegistryDataOffset)
        {
            Int64 nCurrentPosition = fs.Position;

            fs.Seek(TotalHeaderSizeOffset, SeekOrigin.Begin);
            WriteInt32(fs, TotalHeaderSize);

            fs.Seek(NameCountOffset, SeekOrigin.Begin);
            WriteInt32(fs, NameCount);

            fs.Seek(NameOffsetOffset, SeekOrigin.Begin);
            WriteInt32(fs, NameOffset);

            fs.Seek(ExportCountOffset, SeekOrigin.Begin);
            WriteInt32(fs, ExportCount);

            fs.Seek(ExportOffsetOffset, SeekOrigin.Begin);
            WriteInt32(fs, ExportOffset);

            fs.Seek(ImportCountOffset, SeekOrigin.Begin);
            WriteInt32(fs, ImportCount);

            fs.Seek(ImportOffsetOffset, SeekOrigin.Begin);
            WriteInt32(fs, ImportOffset);

            fs.Seek(BulkDataStartOffsetOffset, SeekOrigin.Begin);
            WriteInt32(fs, BulkDataStartOffset);

            fs.Seek(DependsOffsetOffset, SeekOrigin.Begin);
            WriteInt32(fs, DependsOffset);

            fs.Seek(StringAssetReferencesOffsetOffset, SeekOrigin.Begin);
            WriteInt32(fs, StringAssetReferencesOffset);

            fs.Seek(AssetRegistryDataOffsetOffset, SeekOrigin.Begin);
            WriteInt32(fs, AssetRegistryDataOffset);

            fs.Seek(nCurrentPosition, SeekOrigin.Begin);
        }
    }
}
