using System;
using System.Collections.Generic;
using Helpers;
using System.IO;

namespace UAssetTools
{
    public class FName
    {
        public Int32 ComparisonIndex;
        public UInt32 Number;

        public void Serialize(FArchive ar)
        {
            ComparisonIndex.Serialize(ar);
            Number.Serialize(ar);
        }

        public override string ToString()
        {
            return PackageReader.NameMap[ComparisonIndex];
        }
    }

    public class FGenerationInfo
    {
        public Int32 ExportCount;
        public Int32 NameCount;

        public void Serialize(FArchive ar)
        {
            ExportCount.Serialize(ar);
            NameCount.Serialize(ar);
        }
    }

    public class FEngineVersion
    {
        public Int16 Major;
        public Int16 Minor;
        public Int16 Patch;
        public Int32 Changelist;
        public string Branch;

        public void Serialize(FArchive ar)
        {
            Major.Serialize(ar);
            Minor.Serialize(ar);
            Patch.Serialize(ar);
            Changelist.Serialize(ar);
            Branch.Serialize(ar);
        }
    }

    public class FTextureType
    {
        public void Serialize(FArchive ar)
        {
        }
    }

    public class FTextureAllocations
    {
        public List<FTextureType> TextureTypes;

        public FTextureAllocations()
        {
            TextureTypes = new List<FTextureType>();
        }

        public void Serialize(FArchive ar)
        {
            TextureTypes.Serialize(ar);
            if (TextureTypes.Count > 0)
                throw new Exception("TextureAllocations not supported!");
        }
    }

    public class FObjectImport
    {
        public FName ClassPackage;
        public FName ClassName;
        public Int32 OuterIndex;
        public FName ObjectName;

        public FObjectImport()
        {
            ClassPackage = new FName();
            ClassName = new FName();
            ObjectName = new FName();
        }

        public void Serialize(FArchive ar)
        {
            ClassPackage.Serialize(ar);
            ClassName.Serialize(ar);
            OuterIndex.Serialize(ar);
            ObjectName.Serialize(ar);
        }
    }

    public class FObjectExport
    {
        public Int32 ClassIndex;
        public Int32 SuperIndex;
        public Int32 OuterIndex;
        public FName ObjectName;
        public UInt32 ObjectFlags;
        public Int32 SerialSize;
        public Int32 SerialOffset;
        public bool bForcedExport;
        public bool bNotForClient;
        public bool bNotForServer;
        public Guid PackageGuid;
        public UInt32 PackageFlags;
        public bool bNotForEditorGame;

        public object Object;
        public byte[] TailSomething;

        public Int64 SerialSizeOffset;
        public Int64 SerialOffsetOffset;

        public FObjectExport()
        {
            ObjectName = new FName();
        }

        public void Serialize(FArchive ar)
        {
            ClassIndex.Serialize(ar);
            SuperIndex.Serialize(ar);
            OuterIndex.Serialize(ar);
            ObjectName.Serialize(ar);
            ObjectFlags.Serialize(ar); // need add Load flag? and check bIsAsset?
            ar.SavePosition("SerialSize"); SerialSize.Serialize(ar);
            ar.SavePosition("SerialOffset"); SerialOffset.Serialize(ar);
            bForcedExport.Serialize(ar);
            bNotForClient.Serialize(ar);
            bNotForServer.Serialize(ar);
            PackageGuid.Serialize(ar);
            PackageFlags.Serialize(ar);
            if (FFileSummary.FileVersionUE4 < 365)
                throw new Exception("This version not supported!");
            bNotForEditorGame.Serialize(ar);
        }

        public void Correction(FArchive ar, Int32 SerialSize, Int32 SerialOffset)
        {
            ar.WriteToPosition("SerialSize", SerialSize);
            ar.WriteToPosition("SerialOffset", SerialOffset);
        }
    }

    public class FText
    {
        public Int32 Flags;
        public Byte HistoryType;
        public String Namespace;
        public String Key;
        public String SourceStringRaw;

        public void Serialize(FArchive ar)
        {
            if (FFileSummary.FileVersionUE4 < 368)
                throw new Exception("This version not supported!");
            Flags.Serialize(ar);
            HistoryType.Serialize(ar);
            if (HistoryType != 0 && HistoryType != 255)
                throw new Exception("Other types not supported!");
            if (HistoryType == 0)
            {
                Namespace.Serialize(ar);
                Key.Serialize(ar);
                SourceStringRaw.Serialize(ar);
            }
            else
            {
                Namespace = "";
                Key = "";
                SourceStringRaw = "";
            }

            if (ar.IsReading())
            {
                if (Key != "" && SourceStringRaw != "")
                {
                    PackageReader.Texts.Add(new TextInfo("", Namespace, Key, CRC32.StrCrc32(SourceStringRaw), SourceStringRaw));
                    for (int i = 0; i < PackageReader.TextsToReplace.Count; i++)
                    {
                        if (PackageReader.TextsToReplace[i].Key == Key)
                            SourceStringRaw = PackageReader.TextsToReplace[i].Value;
                    }
                }
            }
        }
    }

    public class FIntPoint
    {
        public Int32 X;
        public Int32 Y;

        public void Serialize(FArchive ar)
        {
            X.Serialize(ar);
            Y.Serialize(ar);
        }
    }

    public class FUntypedBulkData
    {
        public static List<byte[]> BulkStorage;

        public UInt32 BulkDataFlags;
        public Int32 ElementCount;
        public Int32 BulkDataSizeOnDisk;
        public Int64 BulkDataOffsetInFile;
        public Byte[] BulkData;
        public Byte[] BulkDataDecompressed;
        public Boolean bPayloadAtEndOfFile;
        public Boolean bCompressed;

        public Int64 PackageFileTag;
        public Int64 CompressionChunkSize;
        public Int64 CompressedSize;
        public Int64 UncompressedSize;
        public Int64 TotalChunkCount;
        public List<KeyValuePair<Int64, Int64>> ChanksInfo;

        public Int64 BulkDataOffsetInFileOffset;

        public FUntypedBulkData()
        {
            ChanksInfo = new List<KeyValuePair<long, long>>();
            BulkDataDecompressed = new Byte[0];
        }

        public void Serialize(FArchive ar)
        {
            BulkDataFlags.Serialize(ar);

            bPayloadAtEndOfFile = ((BulkDataFlags & 0x01) == 0x01);
            bCompressed = ((BulkDataFlags & 0x02) == 0x02);

            int nElementCount = BulkDataDecompressed.Length;
            nElementCount.Serialize(ar);

            if (ar.IsReading())
            {
                BulkDataSizeOnDisk.Serialize(ar);
                BulkDataOffsetInFile.Serialize(ar);

                if (!bPayloadAtEndOfFile)
                {
                    BulkData = ar.Read(BulkDataSizeOnDisk);
                }
                else
                {
                    Int64 nCurOffset = ar.Position();
                    ar.Seek(BulkDataOffsetInFile + FFileSummary.BulkDataStartOffset);
                    BulkData = ar.Read(BulkDataSizeOnDisk);
                    ar.Seek(nCurOffset);
                }

                if (bCompressed)
                {
                    MemoryStream ms = new MemoryStream(BulkData);
                    FArchive ar2 = new FArchive(ms, FArchive.Type.Read);
                    PackageFileTag.Serialize(ar2);
                    CompressionChunkSize.Serialize(ar2);
                    CompressedSize.Serialize(ar2);
                    UncompressedSize.Serialize(ar2);
                    TotalChunkCount = (UncompressedSize + CompressionChunkSize - 1) / CompressionChunkSize;
                    for (int i = 0; i < TotalChunkCount; i++)
                    {
                        ChanksInfo.Add(new KeyValuePair<Int64, Int64>());
                        ChanksInfo[i].Key.Serialize(ar2);
                        ChanksInfo[i].Value.Serialize(ar2);
                    }
                    BulkDataDecompressed = new byte[ElementCount];
                    MemoryStream ms_decompressed = new MemoryStream(BulkDataDecompressed);
                    Int64 CurrentOffset = ar2.Position();
                    for (int i = 0; i < ChanksInfo.Count; i++)
                    {
                        ms.Seek(CurrentOffset + 2, SeekOrigin.Begin);
                        System.IO.Compression.DeflateStream fs_decompressed = new System.IO.Compression.DeflateStream(ms, System.IO.Compression.CompressionMode.Decompress, true);
                        fs_decompressed.CopyTo(ms_decompressed);
                        fs_decompressed.Close();
                        CurrentOffset += ChanksInfo[i].Key;
                    }
                }
                else
                {
                    BulkDataDecompressed = BulkData;
                }
            }
            else if (ar.IsWriting())
            {
                if (bCompressed)
                {
                    MemoryStream ms = new MemoryStream();
                    FArchive ar2 = new FArchive(ms, FArchive.Type.Write);
                    PackageFileTag.Serialize(ar2);
                    CompressionChunkSize.Serialize(ar2);

                    Int64 nTotalSize = 0;
                    ar2.SavePosition("nTotalSize"); nTotalSize.Serialize(ar2);
                    BulkDataDecompressed.Length.Serialize(ar2);

                    TotalChunkCount = (BulkDataDecompressed.Length + CompressionChunkSize - 1) / CompressionChunkSize;

                    for (int j = 0; j < TotalChunkCount; j++)
                    {
                        ar2.SavePosition("ChanksOffsetInfo" + j);
                        Int64 tmp = 0;
                        tmp.Serialize(ar);
                        tmp.Serialize(ar);
                    }

                    int nCurrentOffset = 0;
                    int i = 0;
                    while (nCurrentOffset < BulkDataDecompressed.Length)
                    {
                        ms.Seek(0, SeekOrigin.End);
                        Int64 CurrentCompressionChunkSize = CompressionChunkSize;
                        if (BulkDataDecompressed.Length - nCurrentOffset < CurrentCompressionChunkSize)
                            CurrentCompressionChunkSize = BulkDataDecompressed.Length - nCurrentOffset;
                        MemoryStream ms_src = new MemoryStream(BulkDataDecompressed, nCurrentOffset, (int)CurrentCompressionChunkSize);
                        nCurrentOffset += (int)CurrentCompressionChunkSize;
                        Int64 nCurFsOffset = ms.Position;
                        ((UInt16)0x9C78).Serialize(ar2);
                        System.IO.Compression.DeflateStream ms_compressed = new System.IO.Compression.DeflateStream(ms, System.IO.Compression.CompressionLevel.Optimal, true);
                        ms_src.CopyTo(ms_compressed);
                        ms_compressed.Close();
                        Adler32Computer ad32 = new Adler32Computer();
                        byte[] BufForAdlerCalc = new byte[ms_src.Length];
                        ms_src.Seek(0, SeekOrigin.Begin);
                        ms_src.Read(BufForAdlerCalc, 0, BufForAdlerCalc.Length);
                        ad32.Update(BufForAdlerCalc, 0, BufForAdlerCalc.Length);
                        byte[] AdlerRes = BitConverter.GetBytes(ad32.Checksum);
                        AdlerRes[3].Serialize(ar2);
                        AdlerRes[2].Serialize(ar2);
                        AdlerRes[1].Serialize(ar2);
                        AdlerRes[0].Serialize(ar2);
                        Int64 nNewFsOffset = ms.Position;
                        ar2.WriteToPosition("ChanksOffsetInfo" + i, nNewFsOffset - nCurFsOffset);
                        nTotalSize += nNewFsOffset - nCurFsOffset;
                        CurrentCompressionChunkSize.Serialize(ar2);
                        i++;
                    }

                    ar2.WriteToPosition("nTotalSize", nTotalSize);

                    BulkData = new byte[ms.Length];
                    ms.Seek(0, SeekOrigin.Begin);
                    ms.Read(BulkData, 0, BulkData.Length);
                }

                BulkData.Length.Serialize(ar);
                if (!bPayloadAtEndOfFile)
                {
                    (ar.Position() + 8).Serialize(ar);
                    ar.Write(BulkData);
                }
                else
                {
                    Int64 nTotalLength = 0;
                    for (int i = 0; i < BulkStorage.Count; i++)
                        nTotalLength += BulkStorage[i].LongLength;
                    nTotalLength.Serialize(ar);
                    BulkStorage.Add(BulkData);
                }
            }
        }
    }

    public class FTexture2DMipMap
    {
        public Boolean bCooked;
        public FUntypedBulkData BulkData;
        public Int32 SizeX;
        public Int32 SizeY;

        public FTexture2DMipMap()
        {
            BulkData = new FUntypedBulkData();
        }

        public void Serialize(FArchive ar)
        {
            bCooked.Serialize(ar);
            BulkData.Serialize(ar);
            SizeX.Serialize(ar);
            SizeY.Serialize(ar);
        }
    }

    public class FTexturePlatformData
    {
        public Int32 SizeX;
        public Int32 SizeY;
        public Int32 NumSlices;
        public string PixelFormatString;
        public Int32 FirstMipToSerialize;
        public List<FTexture2DMipMap> Mips;

        public FTexturePlatformData()
        {
            Mips = new List<FTexture2DMipMap>();
        }

        public void Serialize(FArchive ar)
        {
            SizeX.Serialize(ar);
            SizeY.Serialize(ar);
            NumSlices.Serialize(ar);
            PixelFormatString.Serialize(ar);
            FirstMipToSerialize.Serialize(ar);
            Mips.Serialize(ar);
        }
    }

    public class FFormatContainer
    {
        public Int32 NumFormats;
        public FName Name;
        public FUntypedBulkData BulkData;

        public FFormatContainer()
        {
            Name = new FName();
            BulkData = new FUntypedBulkData();
        }

        public void Serialize(FArchive ar)
        {
            NumFormats.Serialize(ar);
            if (NumFormats != 1)
                throw new Exception("Many NumFormats not supported!");
            Name.Serialize(ar);
            BulkData.Serialize(ar);
        }
    }

    public class FGuidCustomVersion_DEPRECATED
    {
        public Guid Key;
        public Int32 Version;
        public String FriendlyName;

        public void Serialize(FArchive ar)
        {
            Key.Serialize(ar);
            Version.Serialize(ar);
            FriendlyName.Serialize(ar);
        }
    }

    public class FPropertyTag
    {
        public FName Name;
        public FName Type;
        public Int32 Size;
        public Int32 ArrayIndex;
        public FName StructName;
        public Guid StructGuid;
        public Byte BoolVal;
        public FName EnumName;
        public FName InnerType;

        public Int32 ClassIndex;

        public Int64 SizeOffset;

        public FPropertyTag()
        {
            Name = new FName();
            Type = new FName();
            StructName = new FName();
            EnumName = new FName();
            InnerType = new FName();
        }

        public void Serialize(FArchive ar)
        {
            Name.Serialize(ar);
            if (Name.ToString() == "None")
                return;
            Type.Serialize(ar);
            ar.SavePosition("SizeOffset"); Size.Serialize(ar);
            ArrayIndex.Serialize(ar);
            if (Type.ToString() == "StructProperty")
            {
                StructName.Serialize(ar);
                if (FFileSummary.FileVersionUE4 < 441)
                    throw new Exception("This version not supported!");
                StructGuid.Serialize(ar);
            }
            else if (Type.ToString() == "BoolProperty")
            {
                BoolVal.Serialize(ar);
            }
            else if (Type.ToString() == "ByteProperty")
            {
                EnumName.Serialize(ar);
            }
            else if (Type.ToString() == "ArrayProperty")
            {
                if (FFileSummary.FileVersionUE4 < 282)
                    throw new Exception("This version not supported!");
                InnerType.Serialize(ar);
            }
        }

        public class FFontCharacter
        {
            public Int32 StartU;
            public Int32 StartV;
            public Int32 USize;
            public Int32 VSize;
            public Byte TextureIndex;
            public Int32 VerticalOffset;

            public void Serialize(FArchive ar)
            {
                StartU.Serialize(ar);
                StartV.Serialize(ar);
                USize.Serialize(ar);
                VSize.Serialize(ar);
                TextureIndex.Serialize(ar);
                VerticalOffset.Serialize(ar);
            }
        }
    }
}
