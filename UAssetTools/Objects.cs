using System;
using System.Collections.Generic;
using Helpers;
using System.IO;

namespace UAssetTools
{
    public class Object : BinaryHelper
    {
        public StructProperty Properties;

        public Object()
        {
            Properties = new StructProperty();
        }

        public virtual void DeSerialize(Stream fs)
        {
            Properties.DeSerialize(fs);
        }

        public virtual void Serialize(Stream fs)
        {
            Properties.Serialize(fs);
        }
    }

    public class Texture2D : Object
    {
        public Int32 Something1; // ???
        public Int16 Something2; // ???

        public byte GlobalStripFlags;
        public byte ClassStripFlags;

        public Name PixelFormatName1;
        public Name PixelFormatName2;
        public Int32 SkipOffset;

        public TexturePlatformData Data;

        public Int64 SkipOffsetOffset;

        public Texture2D()
        {
            PixelFormatName1 = new Name();
            PixelFormatName2 = new Name();
            Data = new TexturePlatformData();
        }

        public override void DeSerialize(Stream fs)
        {
            base.DeSerialize(fs);

            Something1 = ReadInt32(fs); // ???
            Something2 = ReadInt16(fs); // ???

            if (FileSummary.FileVersionUE4 < 214)
                throw new Exception("This version not supported!");
            GlobalStripFlags = ReadByte(fs);
            ClassStripFlags = ReadByte(fs);
            bool bCooked = ReadBool(fs);
            if (!bCooked)
                throw new Exception("This flag must be set!");
            PixelFormatName1.DeSerialize(fs);
            string sPixelFormatName1 = PackageReader.NameMap[PixelFormatName1.ComparisonIndex];
            SkipOffset = ReadInt32(fs);
            Data.DeSerialize(fs);
            PixelFormatName2.DeSerialize(fs);
            string sPixelFormatName2 = PackageReader.NameMap[PixelFormatName2.ComparisonIndex];
            if (sPixelFormatName2 != "None")
                throw new Exception("Skipping not supported!");
        }

        public override void Serialize(Stream fs)
        {
            base.Serialize(fs);

            WriteInt32(fs, Something1); // ???
            WriteInt16(fs, Something2); // ???

            WriteByte(fs, GlobalStripFlags);
            WriteByte(fs, ClassStripFlags);
            WriteBool(fs, true);
            PixelFormatName1.Serialize(fs);
            SkipOffsetOffset = fs.Position; WriteInt32(fs, 0); // POST: SkipOffset
            Data.Serialize(fs);

            Int64 nCurrentPosition = fs.Position;
            fs.Seek(SkipOffsetOffset, SeekOrigin.Begin);
            WriteInt32(fs, (Int32)nCurrentPosition);
            fs.Seek(nCurrentPosition, SeekOrigin.Begin);

            PixelFormatName2.Serialize(fs);
        }

        public void Correction(Stream fs, Int32 SkipOffset)
        {
            Int64 nCurrentPosition = fs.Position;

            fs.Seek(SkipOffsetOffset, SeekOrigin.Begin);
            WriteInt32(fs, SkipOffset);

            fs.Seek(nCurrentPosition, SeekOrigin.Begin);
        }
    }

    public class SoundWave : Object
    {
        public Int32 Something; // ???

        public bool bCooked;
        public Name CompressionName;
        public FormatContainer CompressedFormatData;
        public Guid CompressedDataGuid;

        public SoundWave()
        {
            CompressionName = new Name();
            CompressedFormatData = new FormatContainer();
        }

        public override void DeSerialize(Stream fs)
        {
            base.DeSerialize(fs);

            Something = ReadInt32(fs); // ???

            bCooked = ReadBool(fs);
            if (!bCooked)
                throw new Exception("This flag must be set!");
            if (FileSummary.FileVersionUE4 < 392)
                throw new Exception("This version not supported!");
            CompressionName.DeSerialize(fs);
            string sCompressionName = PackageReader.NameMap[CompressionName.ComparisonIndex];
            CompressedFormatData.DeSerialize(fs);
            CompressedDataGuid = ReadGuid(fs);
        }

        public override void Serialize(Stream fs)
        {
            base.Serialize(fs);

            WriteInt32(fs, Something); // ???

            WriteBool(fs, bCooked);
            CompressionName.Serialize(fs);
            CompressedFormatData.Serialize(fs);
            WriteGuid(fs, CompressedDataGuid);
        }
    }

    public class Enum : Object
    {
        public Int64 Something; // ???

        public Int32 Count;
        public List<Name> Names;
        public byte EnumTypeByte;

        public Enum()
        {
            Names = new List<Name>();
        }

        public override void DeSerialize(Stream fs)
        {
            base.DeSerialize(fs);

            Something = ReadInt64(fs); // ???

            if (FileSummary.FileVersionUE4 >= 463)
                throw new Exception("This version not supported!");
            Count = ReadInt32(fs);
            for (int i = 0; i < Count; i++)
            {
                Names.Add(new Name());
                Names[i].DeSerialize(fs);
                string sName = PackageReader.NameMap[Names[i].ComparisonIndex];
            }
            if (FileSummary.FileVersionUE4 < 390)
                throw new Exception("This version not supported!");
            EnumTypeByte = ReadByte(fs);
        }

        public override void Serialize(Stream fs)
        {
            base.Serialize(fs);

            WriteInt64(fs, Something); // ???

            WriteInt32(fs, Count);
            for (int i = 0; i < Names.Count; i++)
                Names[i].Serialize(fs);
            WriteByte(fs, EnumTypeByte);
        }
    }

    public class UserDefinedEnum : Enum
    {
        public override void DeSerialize(Stream fs)
        {
            base.DeSerialize(fs);
        }

        public override void Serialize(Stream fs)
        {
            base.Serialize(fs);
        }
    }

    public class DataTable : Object
    {
        public Int32 Something; // ???

        public Int32 NumRows;
        public List<KeyValuePair<Name, StructProperty>> RowMap;

        public DataTable()
        {
            RowMap = new List<KeyValuePair<Name, StructProperty>>();
        }

        public override void DeSerialize(Stream fs)
        {
            base.DeSerialize(fs);

            Something = ReadInt32(fs); // ???

            // ???
            /*
            string sObjectName = "";
            for (int i = 0; i < Properties.Properties.Count; i++)
            {
                if (PackageReader.NameMap[Properties.Properties[i].Key.Name.ComparisonIndex] == "RowStruct")
                {
                    sObjectName = PackageReader.NameMap[PackageReader.ImportMap[-(Int32)Properties.Properties[i].Value - 1].ObjectName.ComparisonIndex];
                }
            }
            if (sObjectName == "")
                throw new Exception("RowStruct property not found!");
            */

            NumRows = ReadInt32(fs);

            for (int i = 0; i < NumRows; i++)
            {
                Name RowName = new Name();
                RowName.DeSerialize(fs);
                string sRowName = PackageReader.NameMap[RowName.ComparisonIndex];
                StructProperty RowData = new StructProperty();
                RowData.DeSerialize(fs);
                RowMap.Add(new KeyValuePair<Name, StructProperty>(RowName, RowData));
            }
        }

        public override void Serialize(Stream fs)
        {
            base.Serialize(fs);

            WriteInt32(fs, Something); // ???

            WriteInt32(fs, RowMap.Count);
            for (int i = 0; i < NumRows; i++)
            {
                RowMap[i].Key.Serialize(fs);
                RowMap[i].Value.Serialize(fs);
            }
        }
    }

    public class RawObject : Object
    {
        public override void DeSerialize(Stream fs)
        {
            // Nothing
        }

        public override void Serialize(Stream fs)
        {
            // Nothing
        }
    }

    public class Field : Object
    {
        public Int32 Something; // ???

        public Int32 Next;

        public override void DeSerialize(Stream fs)
        {
            base.DeSerialize(fs);

            Something = ReadInt32(fs); // ???

            Next = ReadInt32(fs);
        }

        public override void Serialize(Stream fs)
        {
            base.Serialize(fs);

            WriteInt32(fs, Something); // ???

            WriteInt32(fs, Next);
        }
    }

    public class Struct : Field
    {
        public Int32 SuperStruct;
        public Int32 Children;
        public Int32 BytecodeBufferSize;
        public Int32 SerializedScriptSize;
        public byte[] SerializedScript;

        public override void DeSerialize(Stream fs)
        {
            base.DeSerialize(fs);
            SuperStruct = ReadInt32(fs);
            Children = ReadInt32(fs);
            BytecodeBufferSize = ReadInt32(fs);
            SerializedScriptSize = ReadInt32(fs);

            // may be changed in future
            SerializedScript = new byte[SerializedScriptSize];
            fs.Read(SerializedScript, 0, SerializedScript.Length);

            // try find texts
            try
            {
                for (int i = 0; i < SerializedScript.Length; i++)
                {
                    if (i < SerializedScript.Length - 1 && SerializedScript[i] == 0x29 && SerializedScript[i + 1] == 0x1F) // seems like Text!
                    {
                        int nStringStart = i + 2;
                        int nStringEnd = i + 2;
                        for (int j = nStringStart; j < SerializedScript.Length; j++)
                        {
                            if (SerializedScript[j] == 0x00)
                            {
                                nStringEnd = j;
                                break;
                            }
                        }
                        int nKeyStart = nStringEnd + 2;
                        int nKeyEnd = nStringEnd + 2;
                        for (int j = nKeyStart; j < SerializedScript.Length; j++)
                        {
                            if (SerializedScript[j] == 0x00)
                            {
                                nKeyEnd = j;
                                break;
                            }
                        }
                        int nNamespaceStart = nKeyEnd + 2;
                        int nNamespaceEnd = nKeyEnd + 2;
                        for (int j = nNamespaceStart; j < SerializedScript.Length; j++)
                        {
                            if (SerializedScript[j] == 0x00)
                            {
                                nNamespaceEnd = j;
                                break;
                            }
                        }
                        string sString = System.Text.Encoding.ASCII.GetString(SerializedScript, nStringStart, nStringEnd - nStringStart);
                        string sKey = System.Text.Encoding.ASCII.GetString(SerializedScript, nKeyStart, nKeyEnd - nKeyStart);
                        string sNamespace = System.Text.Encoding.ASCII.GetString(SerializedScript, nNamespaceStart, nNamespaceEnd - nNamespaceStart);

                        if (sKey != "")
                            PackageReader.Texts.Add(new TextInfo("", sNamespace, sKey, CRC32.StrCrc32(sString), sString));

                        i = nNamespaceEnd + 1;
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }

        public override void Serialize(Stream fs)
        {
            base.Serialize(fs);

            WriteInt32(fs, SuperStruct);
            WriteInt32(fs, Children);

            // may be changed in future
            WriteInt32(fs, BytecodeBufferSize);
            WriteInt32(fs, SerializedScriptSize);
            fs.Write(SerializedScript, 0, SerializedScript.Length);
        }
    }

    public class Function : Struct
    {
        public UInt32 FunctionFlags;
        public Int32 EventGraphFunction;
        public Int32 EventGraphCallOffset;

        public override void DeSerialize(Stream fs)
        {
            base.DeSerialize(fs);

            FunctionFlags = ReadUInt32(fs);
            if ((FunctionFlags & 0x00000040) == 0x00000040)
                throw new Exception("This flag not supported!");
            if (FileSummary.FileVersionUE4 < 451)
                throw new Exception("This version not supported!");
            EventGraphFunction = ReadInt32(fs);
            EventGraphCallOffset = ReadInt32(fs);
        }

        public override void Serialize(Stream fs)
        {
            base.Serialize(fs);

            WriteUInt32(fs, FunctionFlags);
            WriteInt32(fs, EventGraphFunction);
            WriteInt32(fs, EventGraphCallOffset);
        }
    }

    public class Font : Object
    {
        public Int32 Something; // ???

        public Dictionary<UInt16, UInt16> CharRemap;

        public Font()
        {
            CharRemap = new Dictionary<UInt16, UInt16>();
        }

        public override void DeSerialize(Stream fs)
        {
            base.DeSerialize(fs);

            Something = ReadInt32(fs); // ???

            Int32 NumElements = ReadInt32(fs);
            for (int i = 0; i < NumElements; i++)
                CharRemap.Add(ReadUInt16(fs), ReadUInt16(fs));

            if (FileSummary.FileVersionUE4 < 411)
                throw new Exception("This version not supported!");
        }

        public override void Serialize(Stream fs)
        {
            base.Serialize(fs);

            WriteInt32(fs, Something); // ???

            WriteInt32(fs, CharRemap.Count);
            foreach (UInt16 key in CharRemap.Keys)
            {
                WriteUInt16(fs, key);
                WriteUInt16(fs, CharRemap[key]);
            }
        }
    }

    public class FontBulkData : Object
    {
        public UntypedBulkData BulkData;

        public Int32 Something; // ???

        public FontBulkData()
        {
            BulkData = new UntypedBulkData();
        }

        public override void DeSerialize(Stream fs)
        {
            base.DeSerialize(fs);

            Something = ReadInt32(fs); // ???

            BulkData.DeSerialize(fs);
        }

        public override void Serialize(Stream fs)
        {
            base.Serialize(fs);

            WriteInt32(fs, Something); // ???

            BulkData.Serialize(fs);
        }
    }
}
