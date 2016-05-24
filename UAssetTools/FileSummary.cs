using System;
using System.Collections.Generic;
using Helpers;
using System.IO;

namespace UAssetTools
{
    public class FFileSummary
    {
        public Int32 Tag;
        public Int32 LegacyFileVersion;
        public Int32 LegacyUE3Version;
        public static Int32 FileVersionUE4;
        public Int32 FileVersionLicenseeUE4;
        public List<FGuidCustomVersion_DEPRECATED> VersionArray;
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
        public List<FGenerationInfo> Generations;
        public FEngineVersion SavedByEngineVersion;
        public FEngineVersion CompatibleWithEngineVersion;
        public UInt32 CompressionFlags;
        public UInt32 PackageSource;
        public FTextureAllocations TextureAllocations;
        public Int32 AssetRegistryDataOffset;
        public static Int64 BulkDataStartOffset;
        public Int32 WorldTileInfoDataOffset;
        public List<Int32> ChunkIDs;

        public FFileSummary()
        {
            VersionArray = new List<FGuidCustomVersion_DEPRECATED>();
            Generations = new List<FGenerationInfo>();
            SavedByEngineVersion = new FEngineVersion();
            CompatibleWithEngineVersion = new FEngineVersion();
            TextureAllocations = new FTextureAllocations();
            ChunkIDs = new List<Int32>();
            BulkDataStartOffset = 0;
        }

        public void Serialize(FArchive ar)
        {
            Tag.Serialize(ar);
            if (Tag != -1641380927)
                throw new Exception("This is not uasset!");
            LegacyFileVersion.Serialize(ar);
            if (LegacyFileVersion != -5)
                throw new Exception("LegacyFileVersion != -5 not supported!");
            LegacyUE3Version.Serialize(ar);
            FileVersionUE4.Serialize(ar);
            FileVersionLicenseeUE4.Serialize(ar);
            VersionArray.Serialize(ar);
            ar.SavePosition("TotalHeaderSize"); TotalHeaderSize.Serialize(ar);
            FolderName.Serialize(ar);
            PackageFlags.Serialize(ar);
            if ((PackageFlags & 0x80000000) != 0x80000000)
                throw new Exception("Flag must be set!");
            ar.SavePosition("NameCount"); NameCount.Serialize(ar);
            ar.SavePosition("NameOffset"); NameOffset.Serialize(ar);
            if (FileVersionUE4 >= 459) // VER_UE4_SERIALIZE_TEXT_IN_PACKAGES
            {
                GatherableTextDataCount.Serialize(ar);
                if (GatherableTextDataCount > 0)
                    throw new Exception("GatherableTextDataCount not supported!");
                GatherableTextDataOffset.Serialize(ar);
            }
            ar.SavePosition("ExportCount"); ExportCount.Serialize(ar);
            ar.SavePosition("ExportOffset"); ExportOffset.Serialize(ar);
            ar.SavePosition("ImportCount"); ImportCount.Serialize(ar);
            ar.SavePosition("ImportOffset"); ImportOffset.Serialize(ar);
            ar.SavePosition("DependsOffset"); DependsOffset.Serialize(ar);
            if (FileVersionUE4 < 384)
                throw new Exception("This version not supported!");
            StringAssetReferencesCount.Serialize(ar);
            if (StringAssetReferencesCount > 0)
                throw new Exception("StringAssetReferences not supported!");
            ar.SavePosition("StringAssetReferencesOffset"); StringAssetReferencesOffset.Serialize(ar);
            ThumbnailTableOffset.Serialize(ar);
            if (ThumbnailTableOffset > 0)
                throw new Exception("ThumbnailTableOffset not supported!");
            Guid.Serialize(ar);
            Generations.Serialize(ar);
            if (FileVersionUE4 < 336)
                throw new Exception("This version not supported!");
            SavedByEngineVersion.Serialize(ar);
            if (FileVersionUE4 < 444)
                throw new Exception("This version not supported!");
            CompatibleWithEngineVersion.Serialize(ar);
            CompressionFlags.Serialize(ar);
            int nCompressedChunksCount = 0;
            nCompressedChunksCount.Serialize(ar);
            if (nCompressedChunksCount > 0)
                throw new Exception("CompressedChunks not supported!");
            PackageSource.Serialize(ar);
            int nAdditionalPackagesToCookCount = 0;
            nAdditionalPackagesToCookCount.Serialize(ar);
            if (nAdditionalPackagesToCookCount > 0)
                throw new Exception("AdditionalPackagesToCook not supported!");
            TextureAllocations.Serialize(ar);
            ar.SavePosition("AssetRegistryDataOffset"); AssetRegistryDataOffset.Serialize(ar);
            ar.SavePosition("BulkDataStartOffset"); BulkDataStartOffset.Serialize(ar);
            if (FileVersionUE4 < 224)
                throw new Exception("This version not supported!");
            WorldTileInfoDataOffset.Serialize(ar);
            if (WorldTileInfoDataOffset > 0)
                throw new Exception("WorldTileInfoDataOffset not supported!");
            if (FileVersionUE4 < 326)
                throw new Exception("This version not supported!");
            ChunkIDs.Serialize(ar);
        }

        public void Correction(FArchive ar, Int32 TotalHeaderSize, Int32 NameCount, Int32 NameOffset, Int32 ExportCount, Int32 ExportOffset, Int32 ImportCount, Int32 ImportOffset, Int32 BulkDataStartOffset, Int32 DependsOffset, Int32 StringAssetReferencesOffset, Int32 AssetRegistryDataOffset)
        {
            ar.WriteToPosition("TotalHeaderSize", TotalHeaderSize);
            ar.WriteToPosition("NameCount", NameCount);
            ar.WriteToPosition("NameOffset", NameOffset);
            ar.WriteToPosition("ExportCount", ExportCount);
            ar.WriteToPosition("ExportOffset", ExportOffset);
            ar.WriteToPosition("ImportCount", ImportCount);
            ar.WriteToPosition("ImportOffset", ImportOffset);
            ar.WriteToPosition("BulkDataStartOffset", BulkDataStartOffset);
            ar.WriteToPosition("DependsOffset", DependsOffset);
            ar.WriteToPosition("StringAssetReferencesOffset", StringAssetReferencesOffset);
            ar.WriteToPosition("AssetRegistryDataOffset", AssetRegistryDataOffset);
            ar.WriteToPosition("BulkDataStartOffset", BulkDataStartOffset);
            ar.WriteToPosition("BulkDataStartOffset", BulkDataStartOffset);
        }
    }
}
