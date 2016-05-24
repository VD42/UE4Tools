using System;
using System.Collections.Generic;
using Helpers;
using System.IO;

namespace UAssetTools
{
    public class PackageReader
    {
        public FFileSummary PackageFileSummary;
        public static List<String> NameMap;
        public static List<FObjectImport> ImportMap;
        public List<FObjectExport> ExportMap;
        public List<List<Int32>> DependsMap;

        public Int64 NameOffset;
        public Int64 ImportOffset;
        public Int64 ExportOffset;
        public Int64 BulkDataStartOffset;
        public Int64 TotalHeaderSize;
        public Int64 DependsOffset;
        public Int64 AssetRegistryDataOffset;

        public static List<TextInfo> Texts;
        public static List<KeyValuePair<string, string>> TextsToReplace;

        public static bool bEnableSoftMode;

        public PackageReader()
        {
            PackageFileSummary = new FFileSummary();
            NameMap = new List<string>();
            ImportMap = new List<FObjectImport>();
            ExportMap = new List<FObjectExport>();
            DependsMap = new List<List<Int32>>();

            Texts = new List<TextInfo>();
            TextsToReplace = new List<KeyValuePair<string, string>>();

            bEnableSoftMode = false;
        }

        public void ReadOrSavePackageFile(string filename, FileMode mode)
        {
            FileStream fs = new FileStream(filename, mode);
            FArchive ar = new FArchive(fs, mode == FileMode.Open ? FArchive.Type.Read : FArchive.Type.Write);
            PackageFileSummary.Serialize(ar);
            SerializeNameMap(ar);
            SerializeImportMap(ar);
            SerializeExportMap(ar);
            SerializeDependsMap(ar);
            Int32 nAssetRegistryData = 0;
            nAssetRegistryData.Serialize(ar);
            if (nAssetRegistryData != 0)
                throw new Exception("Not supported!");
            SerializeProperties(ar);
            if (ar.IsWriting())
            {
                BulkDataStartOffset = ar.Position();
                for (int i = 0; i < UntypedBulkData.BulkStorage.Count; i++)
                    ar.Write(UntypedBulkData.BulkStorage[i]);
                PackageFileSummary.Correction(
                    ar,
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
                PackageFileSummary.Tag.Serialize(ar);
            }
            fs.Close();
        }

        public void OpenPackageFile(string filename)
        {
            ReadOrSavePackageFile(filename, FileMode.Open);
        }

        public void SerializeNameMap(FArchive ar)
        {
            if (ar.IsReading())
            {
                if (PackageFileSummary.NameCount > 0)
                {
                    ar.Seek(PackageFileSummary.NameOffset);
                    NameMap.Capacity = PackageFileSummary.NameCount;
                    for (int i = 0; i < PackageFileSummary.NameCount; i++)
                        NameMap[i].Serialize(ar);
                }
            }
            else if (ar.IsWriting())
            {
                NameOffset = ar.Position();
                for (int i = 0; i < NameMap.Count; i++)
                    NameMap[i].Serialize(ar);
            }
        }

        public void SerializeImportMap(FArchive ar)
        {
            if (ar.IsReading())
            {
                if (PackageFileSummary.ImportCount > 0)
                {
                    ar.Seek(PackageFileSummary.ImportOffset);
                    ImportMap.Capacity = PackageFileSummary.ImportCount;
                    for (int i = 0; i < PackageFileSummary.ImportCount; i++)
                        ImportMap[i].Serialize(ar);
                }
            }
            else if (ar.IsWriting())
            {
                ImportOffset = ar.Position();
                for (int i = 0; i < ImportMap.Count; i++)
                    ImportMap[i].Serialize(ar);
            }
        }

        public void SerializeExportMap(FArchive ar)
        {
            if (ar.IsReading())
            {
                if (PackageFileSummary.ExportCount > 0)
                {
                    ar.Seek(PackageFileSummary.ExportOffset);
                    ExportMap.Capacity = PackageFileSummary.ExportCount;
                    for (int i = 0; i < PackageFileSummary.ExportCount; i++)
                        ExportMap[i].Serialize(ar);
                }
            }
            else if (ar.IsWriting())
            {
                ExportOffset = ar.Position();
                for (int i = 0; i < ExportMap.Count; i++)
                    ExportMap[i].Serialize(ar);
            }
        }

        public void SerializeDependsMap(FArchive ar)
        {
            if (ar.IsReading())
            {
                ar.Seek(PackageFileSummary.DependsOffset);
                DependsMap.Capacity = PackageFileSummary.ExportCount;
                for (int i = 0; i < PackageFileSummary.ExportCount; i++)
                    DependsMap[i].Serialize(ar);
            }
            else if (ar.IsWriting())
            {
                DependsOffset = ar.Position();
                for (int i = 0; i < DependsMap.Count; i++)
                    DependsMap[i].Serialize(ar);
            }
        }

        public void SerializeProperties(FArchive ar)
        {
            for (int i = 0; i < ExportMap.Count; i++)
            {
                Int64 SerialOffset = 0;

                if (ar.IsReading())
                {
                    if (ar.Position() != ExportMap[i].SerialOffset)
                        throw new Exception("Bad read?");
                    ar.Seek(ExportMap[i].SerialOffset);
                }
                else if (ar.IsWriting())
                {
                    SerialOffset = ar.Position();
                }

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
                                if (ar.IsReading())
                                    ExportMap[i].Object = new Texture2D();
                                ((Texture2D)ExportMap[i].Object).Serialize(ar);
                                break;
                            case "SoundWave":
                                if (ar.IsReading())
                                    ExportMap[i].Object = new SoundWave();
                                ((SoundWave)ExportMap[i].Object).Serialize(ar);
                                break;
                            case "UserDefinedEnum":
                                if (ar.IsReading())
                                    ExportMap[i].Object = new UserDefinedEnum();
                                ((UserDefinedEnum)ExportMap[i].Object).Serialize(ar);
                                break;
                            case "DataTable":
                                if (ar.IsReading())
                                    ExportMap[i].Object = new DataTable();
                                ((DataTable)ExportMap[i].Object).Serialize(ar);
                                break;
                            case "Font":
                                if (ar.IsReading())
                                    ExportMap[i].Object = new Font();
                                ((Font)ExportMap[i].Object).Serialize(ar);
                                break;
                            case "FontBulkData":
                                if (ar.IsReading())
                                    ExportMap[i].Object = new FontBulkData();
                                ((FontBulkData)ExportMap[i].Object).Serialize(ar);
                                break;
                            default:
                                if (!bEnableSoftMode)
                                    throw new Exception("Unknown object name!");
                                if (ar.IsReading())
                                    ExportMap[i].Object = new RawObject();
                                ((RawObject)ExportMap[i].Object).Serialize(ar);
                                break;
                        }
                        break;
                    default:
                        if (!bEnableSoftMode)
                            throw new Exception("Unknown class name!");
                        if (ar.IsReading())
                            ExportMap[i].Object = new RawObject();
                        ((RawObject)ExportMap[i].Object).Serialize(ar);
                        break;
                }

                if (ar.IsReading())
                {
                    if (ar.Position() < ExportMap[i].SerialOffset + ExportMap[i].SerialSize)
                    {
                        if (!bEnableSoftMode)
                            throw new Exception("Bad read!");
                        ExportMap[i].TailSomething = ar.Read((Int32)(ExportMap[i].SerialOffset + ExportMap[i].SerialSize - ar.Position()));
                    }
                    else if (ar.Position() > ExportMap[i].SerialOffset + ExportMap[i].SerialSize)
                    {
                        if (!bEnableSoftMode)
                            throw new Exception("Realy bad read!!!");
                    }
                }
                else if (ar.IsWriting())
                {
                    Int64 SerialSize = ar.Position() - SerialOffset;
                    ExportMap[i].Correction(ar, (Int32)SerialSize, (Int32)SerialOffset);
                }
            }
        }

        public void SavePackageFile(string filename)
        {
            UntypedBulkData.BulkStorage = new List<byte[]>();
            ReadOrSavePackageFile(filename, FileMode.Create);
        }
    }
}
