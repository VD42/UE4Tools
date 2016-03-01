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

        public virtual void DeSerialize(FileStream fs)
        {
            Properties.DeSerialize(fs);
        }

        public virtual void Serialize(FileStream fs)
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

        public override void DeSerialize(FileStream fs)
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

        public override void Serialize(FileStream fs)
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

        public void Correction(FileStream fs, Int32 SkipOffset)
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

        public override void DeSerialize(FileStream fs)
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

        public override void DeSerialize(FileStream fs)
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
    }

    public class UserDefinedEnum : Enum
    {
        public override void DeSerialize(FileStream fs)
        {
            base.DeSerialize(fs);
        }
    }
}
