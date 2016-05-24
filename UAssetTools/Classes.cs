using System;
using System.Collections.Generic;
using Helpers;
using System.IO;

namespace UAssetTools
{
    public class UClass : UStruct
    {
        //public StructProperty Properties;

        public UClass()
        {
            //Properties = new StructProperty();
        }

        public override void Serialize(FArchive ar)
        {
            //Properties.DeSerialize(fs);
            base.Serialize(ar);
        }
    }

    public class UTexture2D : UClass
    {
        public Int32 Something1; // ???
        public Int16 Something2; // ???

        public byte GlobalStripFlags;
        public byte ClassStripFlags;

        public FName PixelFormatName1;
        public FName PixelFormatName2;
        public Int32 SkipOffset;

        public FTexturePlatformData Data;

        public Int64 SkipOffsetOffset;

        public UTexture2D()
        {
            PixelFormatName1 = new FName();
            PixelFormatName2 = new FName();
            Data = new FTexturePlatformData();
        }

        public override void Serialize(FArchive ar)
        {
            base.Serialize(ar);

            Something1.Serialize(ar); // ???
            Something2.Serialize(ar); // ???

            if (FFileSummary.FileVersionUE4 < 214)
                throw new Exception("This version not supported!");
            GlobalStripFlags.Serialize(ar);
            ClassStripFlags.Serialize(ar);
            Boolean bCooked = true;
            bCooked.Serialize(ar);
            if (!bCooked)
                throw new Exception("This flag must be set!");
            PixelFormatName1.Serialize(ar);
            ar.SavePosition("SkipOffset"); SkipOffset.Serialize(ar);
            Data.Serialize(ar);
            Int64 nCurrentPosition = ar.Position();
            ar.WriteToPosition("SkipOffset", (Int32)nCurrentPosition);
            PixelFormatName2.Serialize(ar);
            if (PixelFormatName2.ToString() != "None")
                throw new Exception("Skipping not supported!");
        }
    }

    public class USoundWave : UClass
    {
        public Int32 Something; // ???

        public FName CompressionName;
        public FFormatContainer CompressedFormatData;
        public Guid CompressedDataGuid;

        public USoundWave()
        {
            CompressionName = new FName();
            CompressedFormatData = new FFormatContainer();
        }

        public override void Serialize(FArchive ar)
        {
            base.Serialize(ar);

            Something.Serialize(ar); // ???

            Boolean bCooked = true;
            bCooked.Serialize(ar);
            if (!bCooked)
                throw new Exception("This flag must be set!");
            if (FFileSummary.FileVersionUE4 < 392)
                throw new Exception("This version not supported!");
            CompressionName.Serialize(ar);
            CompressedFormatData.Serialize(ar);
            CompressedDataGuid.Serialize(ar);
        }
    }

    public class UEnum : UClass
    {
        public Int64 Something; // ???

        public Int32 Count;
        public List<FName> Names;
        public Byte EnumTypeByte;

        public UEnum()
        {
            Names = new List<FName>();
        }

        public override void Serialize(FArchive ar)
        {
            base.Serialize(ar);

            Something.Serialize(ar); // ???

            if (FFileSummary.FileVersionUE4 >= 463)
                throw new Exception("This version not supported!");
            Names.Serialize(ar);
            if (FFileSummary.FileVersionUE4 < 390)
                throw new Exception("This version not supported!");
            EnumTypeByte.Serialize(ar);
        }
    }

    public class UUserDefinedEnum : UEnum
    {
        public override void Serialize(FArchive ar)
        {
            base.Serialize(ar);
        }
    }

    public class UDataTable : UClass
    {
        public Int32 Something; // ???

        public Int32 NumRows;
        //public List<KeyValuePair<FName, StructProperty>> RowMap;

        public UDataTable()
        {
            //RowMap = new List<KeyValuePair<FName, StructProperty>>();
        }

        public override void Serialize(FArchive ar)
        {
            base.Serialize(ar);
            /*
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
            /*
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
            */
        }

        /*public override void Serialize(Stream fs)
        {
            base.Serialize(fs);

            WriteInt32(fs, Something); // ???

            WriteInt32(fs, RowMap.Count);
            for (int i = 0; i < NumRows; i++)
            {
                RowMap[i].Key.Serialize(fs);
                RowMap[i].Value.Serialize(fs);
            }
        }*/
    }

    public class URawClass : UClass
    {
        public override void Serialize(FArchive ar)
        {
            // Nothing
        }
    }

    public class UFont : UClass
    {
        public Int32 Something; // ???

        public Dictionary<UInt16, UInt16> CharRemap;

        public UFont()
        {
            CharRemap = new Dictionary<UInt16, UInt16>();
        }

        public override void Serialize(FArchive ar)
        {
            base.Serialize(ar);

            Something.Serialize(ar); // ???

            CharRemap.Serialize(ar);

            if (FFileSummary.FileVersionUE4 < 411)
                throw new Exception("This version not supported!");
        }
    }

    public class UFontBulkData : UClass
    {
        public FUntypedBulkData BulkData;

        public Int32 Something; // ???

        public UFontBulkData()
        {
            BulkData = new FUntypedBulkData();
        }

        public override void Serialize(FArchive ar)
        {
            base.Serialize(ar);

            Something.Serialize(ar); // ???

            BulkData.Serialize(ar);
        }
    }
}
