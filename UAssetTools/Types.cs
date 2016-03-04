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

        public void DeSerialize(FileStream fs)
        {
            ExportCount = ReadInt32(fs);
            NameCount = ReadInt32(fs);
        }

        public void Serialize(FileStream fs)
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

        public void DeSerialize(FileStream fs)
        {
            Major = ReadInt16(fs);
            Minor = ReadInt16(fs);
            Patch = ReadInt16(fs);
            Changelist = ReadInt32(fs);
            Branch = ReadString(fs);
        }

        public void Serialize(FileStream fs)
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

        public void DeSerialize(FileStream fs)
        {
            int nTextureTypesCount = ReadInt32(fs);
            if (nTextureTypesCount > 0)
                throw new Exception("TextureAllocations not supported!");
        }

        public void Serialize(FileStream fs)
        {
            WriteInt32(fs, 0); // nTextureTypesCount
        }
    }

    public class Name : BinaryHelper
    {
        public Int32 ComparisonIndex;
        public UInt32 Number;

        public void DeSerialize(FileStream fs)
        {
            ComparisonIndex = ReadInt32(fs);
            Number = ReadUInt32(fs);
        }

        public void Serialize(FileStream fs)
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

        public void DeSerialize(FileStream fs)
        {
            ClassPackage.DeSerialize(fs);
            ClassName.DeSerialize(fs);
            OuterIndex = ReadInt32(fs);
            ObjectName.DeSerialize(fs);
        }

        public void Serialize(FileStream fs)
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

        public object Object;
        public byte[] TailSomething;

        public Int64 SerialSizeOffset;
        public Int64 SerialOffsetOffset;

        public ObjectExport()
        {
            ObjectName = new Name();
        }

        public void DeSerialize(FileStream fs)
        {
            ClassIndex = ReadInt32(fs);
            SuperIndex = ReadInt32(fs);
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
            if (FileSummary.FileVersionUE4 < 365)
                throw new Exception("This version not supported!");
            bNotForEditorGame = ReadBool(fs);
        }

        public void Serialize(FileStream fs)
        {
            WriteInt32(fs, ClassIndex);
            WriteInt32(fs, SuperIndex);
            WriteInt32(fs, OuterIndex);
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
        }

        public void Correction(FileStream fs, Int32 SerialSize, Int32 SerialOffset)
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

        public void DeSerialize(FileStream fs)
        {
            if (FileSummary.FileVersionUE4 < 368)
                throw new Exception("This version not supported!");
            Flags = ReadInt32(fs);
            HistoryType = ReadByte(fs);
            if (HistoryType != 0 && HistoryType != 255)
                throw new Exception("Other types not supported!");
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

        public void Serialize(FileStream fs)
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

        public void DeSerialize(FileStream fs)
        {
            X = ReadInt32(fs);
            Y = ReadInt32(fs);
        }

        public void Serialize(FileStream fs)
        {
            WriteInt32(fs, X);
            WriteInt32(fs, Y);
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
        public bool bPayloadInline;

        public Int64 BulkDataOffsetInFileOffset;

        public void DeSerialize(FileStream fs)
        {
            BulkDataFlags = ReadUInt32(fs);

            bPayloadInline = ((BulkDataFlags & 0x01) != 0x01);

            ElementCount = ReadInt32(fs);
            BulkDataSizeOnDisk = ReadInt32(fs);
            BulkDataOffsetInFile = ReadInt64(fs);

            if (bPayloadInline)
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
        }

        public void Serialize(FileStream fs)
        {
            WriteUInt32(fs, BulkDataFlags);
            WriteInt32(fs, BulkData.Length); // ElementCount == BulkData.Length ???
            WriteInt32(fs, BulkData.Length);
            if (bPayloadInline)
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

        public void DeSerialize(FileStream fs)
        {
            bCooked = ReadBool(fs);
            BulkData.DeSerialize(fs);
            SizeX = ReadInt32(fs);
            SizeY = ReadInt32(fs);
        }

        public void Serialize(FileStream fs)
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

        public void DeSerialize(FileStream fs)
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

        public void Serialize(FileStream fs)
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

        public void DeSerialize(FileStream fs)
        {
            NumFormats = ReadInt32(fs);
            if (NumFormats != 1)
                throw new Exception("Many NumFormats not supported!");
            Name.DeSerialize(fs);
            string sName = PackageReader.NameMap[Name.ComparisonIndex];
            BulkData.DeSerialize(fs);
        }

        public void Serialize(FileStream fs)
        {
            WriteInt32(fs, NumFormats);
            Name.Serialize(fs);
            BulkData.Serialize(fs);
        }
    }
}
