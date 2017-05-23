using System;
using System.Collections.Generic;
using Helpers;
using System.IO;

namespace UAssetTools
{
    public class GenerationInfo : BinaryHelper
    {
        Int32 ExportCount;
        Int32 NameCount;

        public void DeSerialize(Stream fs)
        {
            ExportCount = ReadInt32(fs);
            NameCount = ReadInt32(fs);
        }

        public void Serialize(Stream fs)
        {
            WriteInt32(fs, ExportCount);
            WriteInt32(fs, NameCount);
        }
    }

    public class EngineVersion : BinaryHelper
    {
        public Int16 Major;
        public Int16 Minor;
        public Int16 Patch;
        public Int32 Changelist;
        public string Branch;

        public void DeSerialize(Stream fs)
        {
            Major = ReadInt16(fs);
            Minor = ReadInt16(fs);
            Patch = ReadInt16(fs);
            Changelist = ReadInt32(fs);
            Branch = ReadString(fs);
        }

        public void Serialize(Stream fs)
        {
            WriteInt16(fs, Major);
            WriteInt16(fs, Minor);
            WriteInt16(fs, Patch);
            WriteInt32(fs, Changelist);
            WriteString(fs, Branch);
        }
    }

    public class TextureType
    {

    }

    public class TextureAllocations : BinaryHelper
    {
        public List<TextureType> TextureTypes;

        public TextureAllocations()
        {
            TextureTypes = new List<TextureType>();
        }

        public void DeSerialize(Stream fs)
        {
            int nTextureTypesCount = ReadInt32(fs);
            if (nTextureTypesCount > 0)
                throw new Exception("TextureAllocations not supported!");
        }

        public void Serialize(Stream fs)
        {
            WriteInt32(fs, 0); // nTextureTypesCount
        }
    }

    public class Name : BinaryHelper
    {
        public Int32 ComparisonIndex;
        public UInt32 Number;

        public void DeSerialize(Stream fs)
        {
            ComparisonIndex = ReadInt32(fs);
            Number = ReadUInt32(fs);
        }

        public void Serialize(Stream fs)
        {
            WriteInt32(fs, ComparisonIndex);
            WriteUInt32(fs, Number);
        }
    }

    public class ObjectImport : BinaryHelper
    {
        public Name ClassPackage;
        public Name ClassName;
        public Int32 OuterIndex;
        public Name ObjectName;

        public ObjectImport()
        {
            ClassPackage = new Name();
            ClassName = new Name();
            ObjectName = new Name();
        }

        public void DeSerialize(Stream fs)
        {
            ClassPackage.DeSerialize(fs);
            ClassName.DeSerialize(fs);
            OuterIndex = ReadInt32(fs);
            ObjectName.DeSerialize(fs);
        }

        public void Serialize(Stream fs)
        {
            ClassPackage.Serialize(fs);
            ClassName.Serialize(fs);
            WriteInt32(fs, OuterIndex);
            ObjectName.Serialize(fs);
        }
    }

    public class ObjectExport : BinaryHelper
    {
        public Int32 ClassIndex;
        public Int32 SuperIndex;
        public Int32 TemplateIndex;
        public Int32 OuterIndex;
        public Name ObjectName;
        public UInt32 ObjectFlags;
        public Int32 SerialSize;
        public Int32 SerialOffset;
        public bool bForcedExport;
        public bool bNotForClient;
        public bool bNotForServer;
        public Guid PackageGuid;
        public UInt32 PackageFlags;
        public bool bNotForEditorGame;
        public bool bIsAsset;
        public Int32 FirstExportDependency;
        public Int32 SerializationBeforeSerializationDependencies;
        public Int32 CreateBeforeSerializationDependencies;
        public Int32 SerializationBeforeCreateDependencies;
        public Int32 CreateBeforeCreateDependencies;

        public object Object;
        public byte[] TailSomething;

        public Int64 SerialSizeOffset;
        public Int64 SerialOffsetOffset;

        public ObjectExport()
        {
            ObjectName = new Name();
        }

        public void DeSerialize(Stream fs)
        {
            ClassIndex = ReadInt32(fs);
            SuperIndex = ReadInt32(fs);
            // if (FileSummary.bUnversioned || FileSummary.FileVersionUE4 >= 508) // VER_UE4_TemplateIndex_IN_COOKED_EXPORTS
            //     TemplateIndex = ReadInt32(fs);
            OuterIndex = ReadInt32(fs);
            ObjectName.DeSerialize(fs);
            ObjectFlags = ReadUInt32(fs); // need add Load flag? and check bIsAsset?
            SerialSize = ReadInt32(fs);
            SerialOffset = ReadInt32(fs);
            bForcedExport = ReadBool(fs);
            bNotForClient = ReadBool(fs);
            bNotForServer = ReadBool(fs);
            PackageGuid = ReadGuid(fs);
            PackageFlags = ReadUInt32(fs);
            if (!FileSummary.bUnversioned && FileSummary.FileVersionUE4 < 365)
                throw new Exception("This version not supported!");
            bNotForEditorGame = ReadBool(fs);
            if (FileSummary.bUnversioned || FileSummary.FileVersionUE4 >= 485) // VER_UE4_COOKED_ASSETS_IN_EDITOR_SUPPORT
                bIsAsset = ReadBool(fs);
            /*if (FileSummary.bUnversioned || FileSummary.FileVersionUE4 >= 507) // VER_UE4_PRELOAD_DEPENDENCIES_IN_COOKED_EXPORTS
            {
                FirstExportDependency = ReadInt32(fs);
                SerializationBeforeSerializationDependencies = ReadInt32(fs);
                CreateBeforeSerializationDependencies = ReadInt32(fs);
                SerializationBeforeCreateDependencies = ReadInt32(fs);
                CreateBeforeCreateDependencies = ReadInt32(fs);
            }*/
        }

        public void Serialize(Stream fs)
        {
            WriteInt32(fs, ClassIndex);
            WriteInt32(fs, SuperIndex);
            if (FileSummary.bUnversioned || FileSummary.FileVersionUE4 >= 508) // VER_UE4_TemplateIndex_IN_COOKED_EXPORTS
                throw new Exception("TemplateIndex not supported for write!");
            ObjectName.Serialize(fs);
            WriteUInt32(fs, ObjectFlags);
            SerialSizeOffset = fs.Position; WriteInt32(fs, 0); // POST: SerialSize
            SerialOffsetOffset = fs.Position; WriteInt32(fs, 0); // POST: SerialOffset
            WriteBool(fs, bForcedExport);
            WriteBool(fs, bNotForClient);
            WriteBool(fs, bNotForServer);
            WriteGuid(fs, PackageGuid);
            WriteUInt32(fs, PackageFlags);
            WriteBool(fs, bNotForEditorGame);
            if (FileSummary.bUnversioned || FileSummary.FileVersionUE4 >= 485) // VER_UE4_COOKED_ASSETS_IN_EDITOR_SUPPORT
                throw new Exception("bIsAsset not supported for write!");
            if (FileSummary.bUnversioned || FileSummary.FileVersionUE4 >= 507) // VER_UE4_PRELOAD_DEPENDENCIES_IN_COOKED_EXPORTS
                throw new Exception("PRELOAD_DEPENDENCIES not supported for write!");
        }

        public void Correction(Stream fs, Int32 SerialSize, Int32 SerialOffset)
        {
            Int64 nCurrentPosition = fs.Position;

            fs.Seek(SerialSizeOffset, SeekOrigin.Begin);
            WriteInt32(fs, SerialSize);

            fs.Seek(SerialOffsetOffset, SeekOrigin.Begin);
            WriteInt32(fs, SerialOffset);

            fs.Seek(nCurrentPosition, SeekOrigin.Begin);
        }
    }

    public class Text : BinaryHelper
    {
        public Int32 Flags;
        public byte HistoryType;
        public string Namespace;
        public string Key;
        public string SourceStringRaw;

        public void DeSerialize(Stream fs)
        {
            if (!FileSummary.bUnversioned && FileSummary.FileVersionUE4 < 368)
                throw new Exception("This version not supported!");
            Flags = ReadInt32(fs);
            HistoryType = ReadByte(fs);
            //if (HistoryType != 0 && HistoryType != 255)
            //    throw new Exception("Other types not supported!");
            if (HistoryType == 0)
            {
                Namespace = ReadString(fs);
                Key = ReadString(fs);
                SourceStringRaw = ReadString(fs);
            }
            else
            {
                Namespace = "";
                Key = "";
                SourceStringRaw = "";
            }

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

        public void Serialize(Stream fs)
        {
            WriteInt32(fs, Flags);
            WriteByte(fs, HistoryType);
            if (HistoryType == 0)
            {
                WriteString(fs, Namespace);
                WriteString(fs, Key);
                WriteString(fs, SourceStringRaw);
            }
        }
    }

    public class IntPoint : BinaryHelper
    {
        public Int32 X;
        public Int32 Y;

        public void DeSerialize(Stream fs)
        {
            X = ReadInt32(fs);
            Y = ReadInt32(fs);
        }

        public void Serialize(Stream fs)
        {
            WriteInt32(fs, X);
            WriteInt32(fs, Y);
        }
    }

    public class Adler32Computer
    {
        private int a = 1;
        private int b = 0;

        public int Checksum
        {
            get
            {
                return ((b * 65536) + a);
            }
        }

        private static readonly int Modulus = 65521;

        public void Update(byte[] data, int offset, int length)
        {
            for (int counter = 0; counter < length; ++counter)
            {
                a = (a + (data[offset + counter])) % Modulus;
                b = (b + a) % Modulus;
            }
        }
    }

    public class UntypedBulkData : BinaryHelper
    {
        public static List<byte[]> BulkStorage;

        public UInt32 BulkDataFlags;
        public Int32 ElementCount;
        public Int32 BulkDataSizeOnDisk;
        public Int64 BulkDataOffsetInFile;
        public byte[] BulkData;
        public byte[] BulkDataDecompressed;
        public bool bPayloadAtEndOfFile;
        public bool bCompressed;

        public Int64 PackageFileTag;
        public Int64 CompressionChunkSize;
        public Int64 CompressedSize;
        public Int64 UncompressedSize;
        public Int64 TotalChunkCount;
        public List<KeyValuePair<Int64, Int64>> ChanksInfo;

        public Int64 BulkDataOffsetInFileOffset;

        public UntypedBulkData()
        {
            ChanksInfo = new List<KeyValuePair<long, long>>();
        }

        public void DeSerialize(Stream fs)
        {
            BulkDataFlags = ReadUInt32(fs);

            bPayloadAtEndOfFile = ((BulkDataFlags & 0x01) == 0x01);
            bCompressed = ((BulkDataFlags & 0x02) == 0x02);

            ElementCount = ReadInt32(fs);
            BulkDataSizeOnDisk = ReadInt32(fs);
            BulkDataOffsetInFile = ReadInt64(fs);

            if (!bPayloadAtEndOfFile)
            {
                BulkData = new byte[BulkDataSizeOnDisk];
                fs.Read(BulkData, 0, BulkDataSizeOnDisk);
            }
            else
            {
                Int64 nCurOffset = fs.Position;
                fs.Seek(BulkDataOffsetInFile + FileSummary.BulkDataStartOffset, SeekOrigin.Begin);
                BulkData = new byte[BulkDataSizeOnDisk];
                fs.Read(BulkData, 0, BulkDataSizeOnDisk);
                fs.Seek(nCurOffset, SeekOrigin.Begin);
            }

            if (bCompressed)
            {
                MemoryStream ms = new MemoryStream(BulkData);
                PackageFileTag = ReadInt64(ms);
                CompressionChunkSize = ReadInt64(ms);
                CompressedSize = ReadInt64(ms);
                UncompressedSize = ReadInt64(ms);
                TotalChunkCount = (UncompressedSize + CompressionChunkSize - 1) / CompressionChunkSize;
                for (int i = 0; i < TotalChunkCount; i++)
                {
                    ChanksInfo.Add(new KeyValuePair<long, long>(ReadInt64(ms), ReadInt64(ms)));
                }
                BulkDataDecompressed = new byte[ElementCount];
                MemoryStream ms_decompressed = new MemoryStream(BulkDataDecompressed);
                Int64 CurrentOffset = ms.Position;
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

        public void Serialize(Stream fs)
        {
            bPayloadAtEndOfFile = ((BulkDataFlags & 0x01) == 0x01);
            bCompressed = ((BulkDataFlags & 0x02) == 0x02);

            if (bCompressed)
            {
                MemoryStream ms = new MemoryStream();
                WriteInt64(ms, PackageFileTag);
                WriteInt64(ms, CompressionChunkSize);

                Int64 CompressedSizeOffset = ms.Position; WriteInt64(ms, 0);
                WriteInt64(ms, BulkDataDecompressed.Length);

                TotalChunkCount = (BulkDataDecompressed.Length + CompressionChunkSize - 1) / CompressionChunkSize;

                List<Int64> ChanksOffsetInfo = new List<Int64>();

                for (int j = 0; j < TotalChunkCount; j++)
                {
                    ChanksOffsetInfo.Add(ms.Position);
                    WriteInt64(ms, 0);
                    WriteInt64(ms, 0);
                }

                int nCurrentOffset = 0;
                int i = 0;
                Int64 nTotalSize = 0;
                while (nCurrentOffset < BulkDataDecompressed.Length)
                {
                    ms.Seek(0, SeekOrigin.End);
                    Int64 CurrentCompressionChunkSize = CompressionChunkSize;
                    if (BulkDataDecompressed.Length - nCurrentOffset < CurrentCompressionChunkSize)
                        CurrentCompressionChunkSize = BulkDataDecompressed.Length - nCurrentOffset;
                    MemoryStream ms_src = new MemoryStream(BulkDataDecompressed, nCurrentOffset, (int)CurrentCompressionChunkSize);
                    nCurrentOffset += (int)CurrentCompressionChunkSize;
                    Int64 nCurFsOffset = ms.Position;
                    WriteUInt16(ms, 0x9C78);
                    System.IO.Compression.DeflateStream ms_compressed = new System.IO.Compression.DeflateStream(ms, System.IO.Compression.CompressionLevel.Optimal, true);
                    ms_src.CopyTo(ms_compressed);
                    ms_compressed.Close();
                    Adler32Computer ad32 = new Adler32Computer();
                    byte[] BufForAdlerCalc = new byte[ms_src.Length];
                    ms_src.Seek(0, SeekOrigin.Begin);
                    ms_src.Read(BufForAdlerCalc, 0, BufForAdlerCalc.Length);
                    ad32.Update(BufForAdlerCalc, 0, BufForAdlerCalc.Length);
                    byte[] AdlerRes = BitConverter.GetBytes(ad32.Checksum);
                    WriteByte(ms, AdlerRes[3]);
                    WriteByte(ms, AdlerRes[2]);
                    WriteByte(ms, AdlerRes[1]);
                    WriteByte(ms, AdlerRes[0]);
                    Int64 nNewFsOffset = ms.Position;
                    ms.Seek(ChanksOffsetInfo[i], SeekOrigin.Begin);
                    WriteInt64(ms, nNewFsOffset - nCurFsOffset);
                    nTotalSize += nNewFsOffset - nCurFsOffset;
                    WriteInt64(ms, CurrentCompressionChunkSize);
                    i++;
                }

                ms.Seek(CompressedSizeOffset, SeekOrigin.Begin);
                WriteInt64(ms, nTotalSize);

                BulkData = new byte[ms.Length];
                ms.Seek(0, SeekOrigin.Begin);
                ms.Read(BulkData, 0, BulkData.Length);
            }

            WriteUInt32(fs, BulkDataFlags);
            WriteInt32(fs, BulkDataDecompressed.Length);
            WriteInt32(fs, BulkData.Length);
            if (!bPayloadAtEndOfFile)
            {
                WriteInt64(fs, fs.Position + 8);
                fs.Write(BulkData, 0, BulkData.Length);
            }
            else
            {
                Int64 nTotalLength = 0;
                for (int i = 0; i < BulkStorage.Count; i++)
                    nTotalLength += BulkStorage[i].LongLength;
                WriteInt64(fs, nTotalLength);
                BulkStorage.Add(BulkData);
            }
        }
    }

    public class Texture2DMipMap : BinaryHelper
    {
        public bool bCooked;
        public UntypedBulkData BulkData;
        public Int32 SizeX;
        public Int32 SizeY;

        public Texture2DMipMap()
        {
            BulkData = new UntypedBulkData();
        }

        public void DeSerialize(Stream fs)
        {
            bCooked = ReadBool(fs);
            BulkData.DeSerialize(fs);
            SizeX = ReadInt32(fs);
            SizeY = ReadInt32(fs);
        }

        public void Serialize(Stream fs)
        {
            WriteBool(fs, bCooked);
            BulkData.Serialize(fs);
            WriteInt32(fs, SizeX);
            WriteInt32(fs, SizeY);
        }
    }

    public class TexturePlatformData : BinaryHelper
    {
        public Int32 SizeX;
        public Int32 SizeY;
        public Int32 NumSlices;
        public string PixelFormatString;
        public Int32 FirstMipToSerialize;
        public Int32 NumMips;
        public List<Texture2DMipMap> Mips;

        public TexturePlatformData()
        {
            Mips = new List<Texture2DMipMap>();
        }

        public void DeSerialize(Stream fs)
        {
            SizeX = ReadInt32(fs);
            SizeY = ReadInt32(fs);
            NumSlices = ReadInt32(fs);
            PixelFormatString = ReadString(fs);
            FirstMipToSerialize = ReadInt32(fs);
            NumMips = ReadInt32(fs);
            for (int i = 0; i < NumMips; i++)
            {
                Mips.Add(new Texture2DMipMap());
                Mips[i].DeSerialize(fs);
            }
        }

        public void Serialize(Stream fs)
        {
            WriteInt32(fs, SizeX);
            WriteInt32(fs, SizeY);
            WriteInt32(fs, NumSlices);
            WriteString(fs, PixelFormatString);
            WriteInt32(fs, FirstMipToSerialize);
            WriteInt32(fs, Mips.Count);
            for (int i = 0; i < NumMips; i++)
                Mips[i].Serialize(fs);
        }
    }

    public class FormatContainer : BinaryHelper
    {
        public Int32 NumFormats;
        public Name Name;
        public UntypedBulkData BulkData;

        public FormatContainer()
        {
            Name = new Name();
            BulkData = new UntypedBulkData();
        }

        public void DeSerialize(Stream fs)
        {
            NumFormats = ReadInt32(fs);
            if (NumFormats != 1)
                throw new Exception("Many NumFormats not supported!");
            Name.DeSerialize(fs);
            string sName = PackageReader.NameMap[Name.ComparisonIndex];
            BulkData.DeSerialize(fs);
        }

        public void Serialize(Stream fs)
        {
            WriteInt32(fs, NumFormats);
            Name.Serialize(fs);
            BulkData.Serialize(fs);
        }
    }

    public class CompositeFont : StructProperty
    {
        public void DeSerialize(Stream fs)
        {
            base.DeSerialize(fs);
        }
    }

    public class Typeface : StructProperty
    {
        public void DeSerialize(Stream fs)
        {
            base.DeSerialize(fs);
        }
    }

    public class FontData : StructProperty
    {
        public void DeSerialize(Stream fs)
        {
            base.DeSerialize(fs);
        }
    }

    public class FontImportOptionsData : StructProperty
    {
        public void DeSerialize(Stream fs)
        {
            base.DeSerialize(fs);
        }
    }

    public class FontCharacter : StructProperty
    {
        public Int32 StartU;
        public Int32 StartV;
        public Int32 USize;
        public Int32 VSize;
        public byte TextureIndex;
        public Int32 VerticalOffset;

        public void DeSerialize(Stream fs)
        {
            StartU = ReadInt32(fs);
            StartV = ReadInt32(fs);
            USize = ReadInt32(fs);
            VSize = ReadInt32(fs);
            TextureIndex = ReadByte(fs);
            VerticalOffset = ReadInt32(fs);
        }

        public void Serialize(Stream fs)
        {
            WriteInt32(fs, StartU);
            WriteInt32(fs, StartV);
            WriteInt32(fs, USize);
            WriteInt32(fs, VSize);
            WriteByte(fs, TextureIndex);
            WriteInt32(fs, VerticalOffset);
        }
    }
}
