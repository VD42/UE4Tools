using System;
using System.Collections.Generic;
using Helpers;
using System.IO;

namespace UAssetTools
{
    public class PackageReader : BinaryHelper
    {
        public FileSummary PackageFileSummary;
        public static List<string> NameMap;
        public static List<ObjectImport> ImportMap;
        public List<ObjectExport> ExportMap;
        public List<List<Int32>> DependsMap;

        public Int64 NameOffset;
        public Int64 ImportOffset;
        public Int64 ExportOffset;
        public Int64 BulkDataStartOffset;
        public Int64 TotalHeaderSize;
        public Int64 DependsOffset;
        public Int64 AssetRegistryDataOffset;

        public static List<KeyValuePair<string, string>> Texts;
        public static List<KeyValuePair<string, string>> TextsToReplace;

        public static bool bEnableSoftMode;

        public PackageReader()
        {
            PackageFileSummary = new FileSummary();
            NameMap = new List<string>();
            ImportMap = new List<ObjectImport>();
            ExportMap = new List<ObjectExport>();
            DependsMap = new List<List<Int32>>();

            Texts = new List<KeyValuePair<string, string>>();
            TextsToReplace = new List<KeyValuePair<string, string>>();

            bEnableSoftMode = false;
        }

        public void OpenPackageFile(string filename)
        {
            FileStream fs = new FileStream(filename, FileMode.Open);
            PackageFileSummary.DeSerialize(fs);
            DeSerializeNameMap(fs);
            DeSerializeImportMap(fs);
            DeSerializeExportMap(fs);
            DeSerializeDependsMap(fs);
            int AssetRegistryData = ReadInt32(fs);
            if (AssetRegistryData != 0)
                throw new Exception("Not supported!");
            ReadProperties(fs);
            fs.Close();
        }

        public void DeSerializeNameMap(FileStream fs)
        {
            if (PackageFileSummary.NameCount > 0)
            {
                fs.Seek(PackageFileSummary.NameOffset, SeekOrigin.Begin);
                for (int i = 0; i < PackageFileSummary.NameCount; i++)
                    NameMap.Add(ReadString(fs));
            }
        }

        public void DeSerializeImportMap(FileStream fs)
        {
            if (PackageFileSummary.ImportCount > 0)
            {
                fs.Seek(PackageFileSummary.ImportOffset, SeekOrigin.Begin);
                for (int i = 0; i < PackageFileSummary.ImportCount; i++)
                {
                    ImportMap.Add(new ObjectImport());
                    ImportMap[i].DeSerialize(fs);
                }
            }
        }

        public void DeSerializeExportMap(FileStream fs)
        {
            if (PackageFileSummary.ExportCount > 0)
            {
                fs.Seek(PackageFileSummary.ExportOffset, SeekOrigin.Begin);
                for (int i = 0; i < PackageFileSummary.ExportCount; i++)
                {
                    ExportMap.Add(new ObjectExport());
                    ExportMap[i].DeSerialize(fs);
                }
            }
        }

        public void DeSerializeDependsMap(FileStream fs)
        {
            fs.Seek(PackageFileSummary.DependsOffset, SeekOrigin.Begin);
            for (int i = 0; i < PackageFileSummary.ExportCount; i++)
            {
                DependsMap.Add(new List<Int32>());
                Int32 Count = ReadInt32(fs);
                for (int j = 0; j < Count; j++)
                    DependsMap[i].Add(ReadInt32(fs));
            }
        }

        public void ReadProperties(FileStream fs)
        {
            for (int i = 0; i < ExportMap.Count; i++)
            {
                if (fs.Position != ExportMap[i].SerialOffset)
                    throw new Exception("Bad read?");
                fs.Seek(ExportMap[i].SerialOffset, SeekOrigin.Begin);

                Int32 ClassIndex = 0;

                if (ExportMap[i].ClassIndex < 0)
                    ClassIndex = ExportMap[i].ClassIndex;
                else if (ExportMap[i].ClassIndex > 0)
                    ClassIndex = ExportMap[ExportMap[i].ClassIndex - 1].ClassIndex; // ???
                else
                    throw new Exception("Not supported!");

                string sClassName = PackageReader.NameMap[ImportMap[-ClassIndex - 1].ClassName.ComparisonIndex];
                string sObjectName = PackageReader.NameMap[ImportMap[-ClassIndex - 1].ObjectName.ComparisonIndex];

                switch (sClassName)
                {
                    case "Class":
                        switch (sObjectName)
                        {
                            case "Texture2D":
                                ExportMap[i].Object = new Texture2D();
                                ((Texture2D)ExportMap[i].Object).DeSerialize(fs);
                                break;
                            case "SoundWave":
                                ExportMap[i].Object = new SoundWave();
                                ((SoundWave)ExportMap[i].Object).DeSerialize(fs);
                                break;
                            case "UserDefinedEnum":
                                ExportMap[i].Object = new UserDefinedEnum();
                                ((UserDefinedEnum)ExportMap[i].Object).DeSerialize(fs);
                                break;
                            case "DataTable":
                                ExportMap[i].Object = new DataTable();
                                ((DataTable)ExportMap[i].Object).DeSerialize(fs);
                                break;
                            //case "Function":
                            //    ExportMap[i].Object = new Function();
                            //    ((Function)ExportMap[i].Object).DeSerialize(fs);
                            //    break;
                            default:
                                if (!bEnableSoftMode)
                                    throw new Exception("Unknown object name!");
                                ExportMap[i].Object = new RawObject();
                                ((RawObject)ExportMap[i].Object).DeSerialize(fs);
                                break;
                        }
                        break;
                    default:
                        if (!bEnableSoftMode)
                            throw new Exception("Unknown class name!");
                        ExportMap[i].Object = new RawObject();
                        ((RawObject)ExportMap[i].Object).DeSerialize(fs);
                        break;
                }

                if (fs.Position < ExportMap[i].SerialOffset + ExportMap[i].SerialSize)
                {
                    if (!bEnableSoftMode)
                        throw new Exception("Bad read!");
                    ExportMap[i].TailSomething = new byte[ExportMap[i].SerialOffset + ExportMap[i].SerialSize - fs.Position];
                    fs.Read(ExportMap[i].TailSomething, 0, ExportMap[i].TailSomething.Length);
                }
            }
        }

        public void SavePackageFile(string filename)
        {
            FileStream fs = new FileStream(filename, FileMode.Create);
            UntypedBulkData.BulkStorage = new List<byte[]>();
            PackageFileSummary.Serialize(fs);
            SerializeNameMap(fs);
            SerializeImportMap(fs);
            SerializeExportMap(fs);
            SerializeDependsMap(fs);
            WriteAssetRegistryData(fs);
            WriteProperties(fs);
            WriteBulkData(fs);
            PackageFileSummary.Correction(
                fs,
                (Int32)TotalHeaderSize,
                NameMap.Count,
                (Int32)NameOffset,
                ExportMap.Count,
                (Int32)ExportOffset,
                ImportMap.Count,
                (Int32)ImportOffset,
                (Int32)BulkDataStartOffset,
                (Int32)DependsOffset,
                (Int32)AssetRegistryDataOffset,
                (Int32)AssetRegistryDataOffset
            );
            WriteInt32(fs, PackageFileSummary.Tag);
            fs.Close();
        }

        public void SerializeNameMap(FileStream fs)
        {
            NameOffset = fs.Position;
            for (int i = 0; i < NameMap.Count; i++)
                WriteString(fs, NameMap[i]);
        }

        public void SerializeImportMap(FileStream fs)
        {
            ImportOffset = fs.Position;
            for (int i = 0; i < ImportMap.Count; i++)
                ImportMap[i].Serialize(fs);
        }

        public void SerializeExportMap(FileStream fs)
        {
            ExportOffset = fs.Position;
            for (int i = 0; i < ExportMap.Count; i++)
                ExportMap[i].Serialize(fs);
        }

        public void SerializeDependsMap(FileStream fs)
        {
            DependsOffset = fs.Position;
            for (int i = 0; i < PackageFileSummary.ExportCount; i++)
            {
                WriteInt32(fs, DependsMap[i].Count);
                for (int j = 0; j < DependsMap[i].Count; j++)
                    WriteInt32(fs, DependsMap[i][j]);
            }
        }
        public void WriteAssetRegistryData(FileStream fs)
        {
            AssetRegistryDataOffset = fs.Position;
            WriteInt32(fs, 0); // not supported
        }

        public void WriteProperties(FileStream fs)
        {
            TotalHeaderSize = fs.Position;

            for (int i = 0; i < ExportMap.Count; i++)
            {
                Int64 SerialOffset = fs.Position;

                Int32 ClassIndex = 0;

                if (ExportMap[i].ClassIndex < 0)
                    ClassIndex = ExportMap[i].ClassIndex;
                else if (ExportMap[i].ClassIndex > 0)
                    ClassIndex = ExportMap[ExportMap[i].ClassIndex - 1].ClassIndex; // ???
                else
                    throw new Exception("Not supported!");

                string sClassName = PackageReader.NameMap[ImportMap[-ClassIndex - 1].ClassName.ComparisonIndex];
                string sObjectName = PackageReader.NameMap[ImportMap[-ClassIndex - 1].ObjectName.ComparisonIndex];

                switch (sClassName)
                {
                    case "Class":
                        switch (sObjectName)
                        {
                            case "Texture2D":
                                ((Texture2D)ExportMap[i].Object).Serialize(fs);
                                break;
                            case "SoundWave":
                                ((SoundWave)ExportMap[i].Object).Serialize(fs);
                                break;
                            case "UserDefinedEnum":
                                ((UserDefinedEnum)ExportMap[i].Object).Serialize(fs);
                                break;
                            case "DataTable":
                                ((DataTable)ExportMap[i].Object).Serialize(fs);
                                break;
                            default:
                                if (!bEnableSoftMode)
                                    throw new Exception("Unknown object name!");
                                fs.Write(ExportMap[i].TailSomething, 0, ExportMap[i].TailSomething.Length);
                                break;
                        }
                        break;
                    default:
                        if (!bEnableSoftMode)
                            throw new Exception("Unknown class name!");
                        fs.Write(ExportMap[i].TailSomething, 0, ExportMap[i].TailSomething.Length);
                        break;
                }

                Int64 SerialSize = fs.Position - SerialOffset;
                ExportMap[i].Correction(fs, (Int32)SerialSize, (Int32)SerialOffset);
            }
        }

        public void WriteBulkData(FileStream fs)
        {
            BulkDataStartOffset = fs.Position;
            for (int i = 0; i < UntypedBulkData.BulkStorage.Count; i++)
                fs.Write(UntypedBulkData.BulkStorage[i], 0, UntypedBulkData.BulkStorage[i].Length);
        }
    }
}
