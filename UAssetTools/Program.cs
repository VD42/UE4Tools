using System;
using System.Collections.Generic;
using System.IO;

namespace UAssetTools
{
    public class TextInfo
    {
        public string File;
        public string Namespace;
        public string Key;
        public UInt32 Hash;
        public string Text;

        public TextInfo(string file, string ns, string key, UInt32 hash, string text)
        {
            File = file;
            Namespace = ns;
            Key = key;
            Hash = hash;
            Text = text;
        }
    }

    public class PictureInfo
    {
        public int Index;
        public string File;
        public string Hash;

        public PictureInfo(int index, string file, string hash)
        {
            Index = index;
            File = file;
            Hash = hash;
        }
    }

    class Program
    {
        public static void GetFiles(string path, ref List<string> files)
        {
            if (Path.GetFileName(path) == ".git")
                return;
            string[] current_files = Directory.GetFiles(path);
            for (int i = 0; i < current_files.Length; i++)
            {
                if (Path.GetExtension(current_files[i]) != ".uasset" && Path.GetExtension(current_files[i]) != ".umap")
                    continue;
                files.Add(current_files[i]);
            }
            string[] current_dirs = Directory.GetDirectories(path);
            for (int i = 0; i < current_dirs.Length; i++)
                GetFiles(current_dirs[i], ref files);
        }

        public static List<TextInfo> ReadTexts(string filename)
        {
            List<TextInfo> Texts = new List<TextInfo>();
            StreamReader sr = new StreamReader(filename);
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                if (line.Trim() == "")
                    continue;
                line = line.Substring(line.IndexOf('[') + 1);
                string file = line.Substring(0, line.IndexOf(']'));
                line = line.Substring(line.IndexOf('[') + 1);
                string key = line.Substring(0, line.IndexOf(']'));
                string text = sr.ReadLine();
                Texts.Add(new TextInfo(file, "", key, 0, text));
            }
            sr.Close();
            return Texts;
        }

        public static List<PictureInfo> ReadPictures(string filename)
        {
            List<PictureInfo> Pictures = new List<PictureInfo>();
            StreamReader sr = new StreamReader(filename);
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                if (line.Trim() == "")
                    continue;
                line = line.Substring(line.IndexOf('[') + 1);
                int index = int.Parse(line.Substring(0, line.IndexOf(']')));
                line = line.Substring(line.IndexOf('[') + 1);
                string file = line.Substring(0, line.IndexOf(']'));
                string hash = "";
                if (line.IndexOf('[') > -1)
                {
                    line = line.Substring(line.IndexOf('[') + 1);
                    hash = line.Substring(0, line.IndexOf(']'));
                }
                Pictures.Add(new PictureInfo(index, file, hash));
            }
            sr.Close();
            return Pictures;
        }

        static void WriteDDSHeader(Stream fs, int nWidth, int nHeight, int nVersion)
        {
            byte[] header = new byte[128];

            header[0] = 0x44;
            header[1] = 0x44;
            header[2] = 0x53;
            header[3] = 0x20;

            header[4] = 0x7C;

            header[8] = 0x07;
            header[9] = 0x10;

            header[12] = (byte)(nHeight & 0xFF);
            header[13] = (byte)((nHeight >> 8) & 0xFF);
            header[14] = (byte)((nHeight >> 16) & 0xFF);
            header[15] = (byte)((nHeight >> 24) & 0xFF);

            header[16] = (byte)(nWidth & 0xFF);
            header[17] = (byte)((nWidth >> 8) & 0xFF);
            header[18] = (byte)((nWidth >> 16) & 0xFF);
            header[19] = (byte)((nWidth >> 24) & 0xFF);

            header[76] = 0x20;

            header[80] = 0x04;

            header[84] = 0x44;
            header[85] = 0x58;
            header[86] = 0x54;
            header[87] = (byte)(0x30 + nVersion);

            header[109] = 0x10;

            fs.Write(header, 0, header.Length);
        }

        static void WriteDDSHeaderATI2(Stream fs, int nWidth, int nHeight)
        {
            byte[] header = new byte[128];

            header[0] = 0x44;
            header[1] = 0x44;
            header[2] = 0x53;
            header[3] = 0x20;

            header[4] = 0x7C;

            header[8] = 0x07;
            header[9] = 0x10;

            header[12] = (byte)(nHeight & 0xFF);
            header[13] = (byte)((nHeight >> 8) & 0xFF);
            header[14] = (byte)((nHeight >> 16) & 0xFF);
            header[15] = (byte)((nHeight >> 24) & 0xFF);

            header[16] = (byte)(nWidth & 0xFF);
            header[17] = (byte)((nWidth >> 8) & 0xFF);
            header[18] = (byte)((nWidth >> 16) & 0xFF);
            header[19] = (byte)((nWidth >> 24) & 0xFF);

            header[76] = 0x20;

            header[80] = 0x04;

            header[84] = 0x41;
            header[85] = 0x54;
            header[86] = 0x49;
            header[87] = 0x32;

            header[109] = 0x10;

            fs.Write(header, 0, header.Length);
        }

        static void WriteDDSHeaderBGRA(Stream fs, int nWidth, int nHeight)
        {
            byte[] header = new byte[128];

            header[0] = 0x44;
            header[1] = 0x44;
            header[2] = 0x53;
            header[3] = 0x20;

            header[4] = 0x7C;

            header[8] = 0x07;
            header[9] = 0x10;

            header[12] = (byte)(nHeight & 0xFF);
            header[13] = (byte)((nHeight >> 8) & 0xFF);
            header[14] = (byte)((nHeight >> 16) & 0xFF);
            header[15] = (byte)((nHeight >> 24) & 0xFF);

            header[16] = (byte)(nWidth & 0xFF);
            header[17] = (byte)((nWidth >> 8) & 0xFF);
            header[18] = (byte)((nWidth >> 16) & 0xFF);
            header[19] = (byte)((nWidth >> 24) & 0xFF);

            header[76] = 0x20;

            header[80] = 0x41;

            header[88] = 0x20;
            header[89] = 0x00;
            header[90] = 0x00;
            header[91] = 0x00;

            // R
            header[92] = 0x00;
            header[93] = 0x00;
            header[94] = 0xFF;
            header[95] = 0x00;

            // G
            header[96] = 0x00;
            header[97] = 0xFF;
            header[98] = 0x00;
            header[99] = 0x00;

            // B
            header[100] = 0xFF;
            header[101] = 0x00;
            header[102] = 0x00;
            header[103] = 0x00;

            // A
            header[104] = 0x00;
            header[105] = 0x00;
            header[106] = 0x00;
            header[107] = 0xFF;

            header[109] = 0x10;

            fs.Write(header, 0, header.Length);
        }

        public static void WriteDDSHeaderG8(Stream fs, int nWidth, int nHeight)
        {
            byte[] header = new byte[128];

            header[0] = 0x44;
            header[1] = 0x44;
            header[2] = 0x53;
            header[3] = 0x20;

            header[4] = 0x7C;

            header[8] = 0x07;
            header[9] = 0x10;

            header[12] = (byte)(nHeight & 0xFF);
            header[13] = (byte)((nHeight >> 8) & 0xFF);
            header[14] = (byte)((nHeight >> 16) & 0xFF);
            header[15] = (byte)((nHeight >> 24) & 0xFF);

            header[16] = (byte)(nWidth & 0xFF);
            header[17] = (byte)((nWidth >> 8) & 0xFF);
            header[18] = (byte)((nWidth >> 16) & 0xFF);
            header[19] = (byte)((nWidth >> 24) & 0xFF);

            header[76] = 0x20;

            header[80] = 0x42;

            header[88] = 0x08;
            header[89] = 0x00;
            header[90] = 0x00;
            header[91] = 0x00;

            // R
            header[92] = 0x00;
            header[93] = 0x00;
            header[94] = 0x00;
            header[95] = 0x00;

            // G
            header[96] = 0x00;
            header[97] = 0x00;
            header[98] = 0x00;
            header[99] = 0xFF;

            // B
            header[100] = 0x00;
            header[101] = 0x00;
            header[102] = 0x00;
            header[103] = 0x00;

            // A
            header[104] = 0x00;
            header[105] = 0x00;
            header[106] = 0x00;
            header[107] = 0x00;

            header[109] = 0x10;

            fs.Write(header, 0, header.Length);
        }

        public static void CreatePath(string FilePath)
        {
            string DirPath = Path.GetDirectoryName(FilePath);
            if (!Directory.Exists(DirPath))
            {
                CreatePath(DirPath);
                Directory.CreateDirectory(DirPath);
            }
        }

        static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("UAssetTools.exe extract_texts <game_path> <path_to_texts_file>");
                //Console.WriteLine("UAssetTools.exe replace_texts <game_path> <path_to_texts_file>");
                Console.WriteLine("UAssetTools.exe bruteforce_texts <game_path> <path_to_texts_file>");
                Console.WriteLine("UAssetTools.exe extract_textures <game_path> <path_to_extracted_textures>");
                Console.WriteLine("UAssetTools.exe extract_convert_textures <game_path> <path_to_extracted_and_converted_textures>");
                Console.WriteLine("UAssetTools.exe restore_textures <game_path> <path_to_extracted_and_converted_textures>");
                Console.WriteLine("UAssetTools.exe font_unpack <asset_file_path> <path_to_font>");
                Console.WriteLine("UAssetTools.exe font_pack <asset_file_path> <path_to_font>");
                return;
            }

            switch (args[0])
            {
                case "extract_texts":
                    // extract_texts "C:\Program Files (x86)\Steam\SteamApps\common\The Park" "C:\Program Files (x86)\Steam\SteamApps\common\The Park\Workspace\texts.txt"
                    {
                        List<String> files = new List<string>();
                        GetFiles(args[1], ref files);
                        List<TextInfo> Texts = new List<TextInfo>();
                        for (int i = 0; i < files.Count; i++)
                        {
                            Console.WriteLine("[" + (i + 1) + " of " + files.Count + "] " + files[i]);
                            PackageReader asset = new PackageReader();
                            try
                            {
                                PackageReader.bEnableSoftMode = true;
                                asset.OpenPackageFile(files[i]);
                                PackageReader.bEnableSoftMode = false;
                                for (int j = 0; j < PackageReader.Texts.Count; j++)
                                {
                                    Texts.Add(PackageReader.Texts[j]);
                                    if (Texts[Texts.Count - 1].Namespace != "")
                                        throw new Exception("Only empty supported!");
                                    Texts[Texts.Count - 1].File = files[i];
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Cannot read file, reason: " + ex.Message);
                            }
                        }
                        for (int i = 0; i < Texts.Count; i++)
                        {
                            for (int j = i + 1; j < Texts.Count; j++)
                            {
                                if (Texts[i].Key == Texts[j].Key)
                                {
                                    Texts.RemoveAt(j);
                                    j--;
                                }
                            }
                        }
                        StreamWriter sw = new StreamWriter(args[2]);
                        sw.WriteLine("=>{}");
                        sw.WriteLine();
                        for (int i = 0; i < Texts.Count; i++)
                        {
                            sw.WriteLine("=>[" + Texts[i].Key + "][" + Texts[i].Hash + "]");
                            sw.WriteLine(Texts[i].Text);
                            sw.WriteLine();
                        }
                        sw.WriteLine("=>{[END]}");
                        sw.Close();
                    }
                    break;
                case "replace_texts":
                    // replace_texts "C:\Program Files (x86)\Steam\SteamApps\common\The Park" "C:\Program Files (x86)\Steam\SteamApps\common\The Park\Workspace\texts_rus.txt"
                    {
                        Console.WriteLine("This method for text localization is deprecated! See about .locres-file!");
                        List<TextInfo> Texts = ReadTexts(args[2]);
                        for (int i = 0; i < Texts.Count; i++)
                        {
                            Console.WriteLine("[" + (i + 1) + " of " + Texts.Count + "][" + Texts[i].Key + "]: " + Path.Combine(args[1], Texts[i].File));
                            PackageReader asset = new PackageReader();
                            PackageReader.TextsToReplace.Add(new KeyValuePair<string, string>(Texts[i].Key, Texts[i].Text));
                            asset.OpenPackageFile(Path.Combine(args[1], Texts[i].File));
                            asset.SavePackageFile(Path.Combine(args[1], Texts[i].File));
                        }
                    }
                    break;
                case "bruteforce_texts":
                    // bruteforce_texts "C:\Program Files (x86)\Steam\SteamApps\common\The Park" "C:\Program Files (x86)\Steam\SteamApps\common\The Park\Workspace\texts.txt"
                    {
                        List<String> files = new List<string>();
                        GetFiles(args[1], ref files);
                        List<TextInfo> Texts = new List<TextInfo>();
                        for (int i = 0; i < files.Count; i++)
                        {
                            Console.WriteLine("[" + (i + 1) + " of " + files.Count + "] " + files[i]);
                            PackageReader asset = new PackageReader();
                            try
                            {
                                FileStream fs = new FileStream(files[i], FileMode.Open);
                                FArchive ar = new FArchive(fs, FArchive.Type.Read);
                                asset.PackageFileSummary.Serialize(ar);
                                asset.SerializeNameMap(ar);
                                asset.SerializeExportMap(ar);

                                bool bFoundTextProperty = false;
                                byte[] TextPropertySignature = new byte[8];
                                for (int j = 0; j < PackageReader.NameMap.Count; j++)
                                {
                                    if (PackageReader.NameMap[j] == "TextProperty")
                                    {
                                        byte[] TextPropertySignaturePart = BitConverter.GetBytes(j);
                                        for (int k = 0; k < TextPropertySignaturePart.Length; k++)
                                            TextPropertySignature[k] = TextPropertySignaturePart[k];
                                        bFoundTextProperty = true;
                                        break;
                                    }
                                }

                                if (!bFoundTextProperty)
                                {
                                    fs.Close();
                                    continue;
                                }

                                for (int j = 0; j < asset.ExportMap.Count; j++)
                                {
                                    fs.Seek(asset.ExportMap[j].SerialOffset, SeekOrigin.Begin);
                                    for (int k = 0; k < asset.ExportMap[j].SerialSize - 7; k++)
                                    {
                                        fs.Seek(asset.ExportMap[j].SerialOffset + k, SeekOrigin.Begin);
                                        byte[] Test = new byte[8];
                                        fs.Read(Test, 0, 8);
                                        if (!System.Collections.StructuralComparisons.StructuralEqualityComparer.Equals(Test, TextPropertySignature))
                                            continue;
                                        try
                                        {
                                            fs.Seek(8, SeekOrigin.Current);
                                            Text t = new Text();
                                            t.DeSerialize(fs);
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.WriteLine("Cannot read file, reason: " + ex.Message);
                                        }
                                    }
                                }

                                fs.Close();

                                for (int j = 0; j < PackageReader.Texts.Count; j++)
                                {
                                    Texts.Add(PackageReader.Texts[j]);
                                    if (Texts[Texts.Count - 1].Namespace != "")
                                        throw new Exception("Only empty supported!");
                                    Texts[Texts.Count - 1].File = files[i];
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Cannot read file, reason: " + ex.Message);
                            }
                        }
                        for (int i = 0; i < Texts.Count; i++)
                        {
                            for (int j = i + 1; j < Texts.Count; j++)
                            {
                                if (Texts[i].Key == Texts[j].Key)
                                {
                                    Texts.RemoveAt(j);
                                    j--;
                                }
                            }
                        }
                        StreamWriter sw = new StreamWriter(args[2]);
                        sw.WriteLine("=>{}");
                        sw.WriteLine();
                        for (int i = 0; i < Texts.Count; i++)
                        {
                            sw.WriteLine("=>[" + Texts[i].Key + "][" + Texts[i].Hash + "]");
                            sw.WriteLine(Texts[i].Text);
                            sw.WriteLine();
                        }
                        sw.WriteLine("=>{[END]}");
                        sw.Close();
                    }
                    break;
                case "extract_textures":
                    // extract_textures "C:\Program Files (x86)\Steam\SteamApps\common\The Park" "C:\Program Files (x86)\Steam\SteamApps\common\The Park\Workspace\Textures"
                    {
                        List<String> files = new List<string>();
                        GetFiles(args[1], ref files);
                        List<PictureInfo> Pictures = new List<PictureInfo>();
                        for (int i = 0; i < files.Count; i++)
                        {
                            Console.WriteLine("[" + (i + 1) + " of " + files.Count + "] " + files[i]);
                            PackageReader asset = new PackageReader();
                            try
                            {
                                asset.OpenPackageFile(files[i]);
                                for (int j = 0; j < asset.ExportMap.Count; j++)
                                {
                                    if (PackageReader.NameMap[PackageReader.ImportMap[-asset.ExportMap[j].ClassIndex - 1].ObjectName.ComparisonIndex] == "Texture2D")
                                    {
                                        if (asset.ExportMap.Count != 1)
                                            throw new Exception("Only one export supported!");
                                    }
                                }
                                if (asset.ExportMap.Count > 0 && PackageReader.NameMap[PackageReader.ImportMap[-asset.ExportMap[0].ClassIndex - 1].ObjectName.ComparisonIndex] == "Texture2D")
                                {
                                    Texture2D texture = (Texture2D)asset.ExportMap[0].Object;
                                    string sPixelFormat = PackageReader.NameMap[texture.PixelFormatName1.ComparisonIndex];
                                    if (sPixelFormat.Length >= 6 && sPixelFormat.Substring(0, 6) == "PF_DXT")
                                    {
                                        int nVersion = int.Parse(sPixelFormat.Substring(6));
                                        FileStream fs = new FileStream(Path.Combine(args[2], (Pictures.Count + 1) + " - " + Path.GetFileNameWithoutExtension(files[i]) + ".dds"), FileMode.Create);
                                        WriteDDSHeader(fs, texture.Data.Mips[0].SizeX, texture.Data.Mips[0].SizeY, nVersion);
                                        fs.Write(texture.Data.Mips[0].BulkData.BulkData, 0, texture.Data.Mips[0].BulkData.BulkData.Length);
                                        Pictures.Add(new PictureInfo(Pictures.Count + 1, files[i], ""));
                                        fs.Close();
                                    }
                                    else if (sPixelFormat == "PF_B8G8R8A8")
                                    {
                                        FileStream fs = new FileStream(Path.Combine(args[2], (Pictures.Count + 1) + " - " + Path.GetFileNameWithoutExtension(files[i]) + ".dds"), FileMode.Create);
                                        WriteDDSHeaderBGRA(fs, texture.Data.Mips[0].SizeX, texture.Data.Mips[0].SizeY);
                                        fs.Write(texture.Data.Mips[0].BulkData.BulkData, 0, texture.Data.Mips[0].BulkData.BulkData.Length);
                                        Pictures.Add(new PictureInfo(Pictures.Count + 1, files[i], ""));
                                        fs.Close();
                                    }
                                    else if (sPixelFormat == "PF_BC5")
                                    {
                                        FileStream fs = new FileStream(Path.Combine(args[2], (Pictures.Count + 1) + " - " + Path.GetFileNameWithoutExtension(files[i]) + ".dds"), FileMode.Create);
                                        WriteDDSHeaderATI2(fs, texture.Data.Mips[0].SizeX, texture.Data.Mips[0].SizeY);
                                        fs.Write(texture.Data.Mips[0].BulkData.BulkData, 0, texture.Data.Mips[0].BulkData.BulkData.Length);
                                        Pictures.Add(new PictureInfo(Pictures.Count + 1, files[i], ""));
                                        fs.Close();
                                    }
                                    else if (sPixelFormat == "PF_G8")
                                    {
                                        FileStream fs = new FileStream(Path.Combine(args[2], (Pictures.Count + 1) + " - " + Path.GetFileNameWithoutExtension(files[i]) + ".dds"), FileMode.Create);
                                        WriteDDSHeaderG8(fs, texture.Data.Mips[0].SizeX, texture.Data.Mips[0].SizeY);
                                        fs.Write(texture.Data.Mips[0].BulkData.BulkData, 0, texture.Data.Mips[0].BulkData.BulkData.Length);
                                        Pictures.Add(new PictureInfo(Pictures.Count + 1, files[i], ""));
                                        fs.Close();
                                    }
                                    else
                                    {
                                        throw new Exception("Other formats not supported!");
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Cannot read file, reason: " + ex.Message);
                            }
                        }
                        StreamWriter sw = new StreamWriter(Path.Combine(args[2], "_textures.txt"));
                        for (int i = 0; i < Pictures.Count; i++)
                        {
                            sw.WriteLine("[" + Pictures[i].Index + "][" + Pictures[i].File.Replace(args[1] + "\\", "") + "]");
                        }
                        sw.Close();
                    }
                    break;
                case "extract_convert_textures":
                    // extract_convert_textures "C:\Program Files (x86)\Steam\SteamApps\common\The Park" "C:\Program Files (x86)\Steam\SteamApps\common\The Park\Workspace\ConvertedTextures"
                    {
                        List<PictureInfo> Pictures = ReadPictures(Path.Combine(args[2], "_textures.txt"));
                        for (int i = 0; i < Pictures.Count; i++)
                        {
                            Console.WriteLine("[" + (i + 1) + " of " + Pictures.Count + "] " + Pictures[i].File);
                            PackageReader asset = new PackageReader();
                            try
                            {
                                asset.OpenPackageFile(Path.Combine(args[1], Pictures[i].File));
                                for (int j = 0; j < asset.ExportMap.Count; j++)
                                {
                                    if (PackageReader.NameMap[PackageReader.ImportMap[-asset.ExportMap[j].ClassIndex - 1].ObjectName.ComparisonIndex] == "Texture2D")
                                    {
                                        if (asset.ExportMap.Count != 1)
                                            throw new Exception("Only one export supported!");
                                    }
                                }
                                if (asset.ExportMap.Count > 0 && PackageReader.NameMap[PackageReader.ImportMap[-asset.ExportMap[0].ClassIndex - 1].ObjectName.ComparisonIndex] == "Texture2D")
                                {
                                    Texture2D texture = (Texture2D)asset.ExportMap[0].Object;
                                    string sPixelFormat = PackageReader.NameMap[texture.PixelFormatName1.ComparisonIndex];
                                    if (sPixelFormat.Length >= 6 && sPixelFormat.Substring(0, 6) == "PF_DXT")
                                    {
                                        int nVersion = int.Parse(sPixelFormat.Substring(6));
                                        MemoryStream ms = new MemoryStream();
                                        WriteDDSHeader(ms, texture.Data.Mips[0].SizeX, texture.Data.Mips[0].SizeY, nVersion);
                                        ms.Write(texture.Data.Mips[0].BulkData.BulkData, 0, texture.Data.Mips[0].BulkData.BulkData.Length);
                                        ms.Seek(0, SeekOrigin.Begin);
                                        ImageMagick.MagickReadSettings settings = new ImageMagick.MagickReadSettings();
                                        settings.Format = ImageMagick.MagickFormat.Dds;
                                        ImageMagick.MagickImage image = new ImageMagick.MagickImage(ms, settings);
                                        FileStream fs = new FileStream(Path.Combine(args[2], Pictures[i].Index + " - " + Path.GetFileNameWithoutExtension(Pictures[i].File) + ".png"), FileMode.Create);
                                        image.Write(fs, ImageMagick.MagickFormat.Png);
                                        fs.Seek(0, SeekOrigin.Begin);
                                        Pictures[i].Hash = BitConverter.ToString(System.Security.Cryptography.SHA256.Create().ComputeHash(fs)).Replace("-", "");
                                        fs.Close();
                                    }
                                    else if (sPixelFormat == "PF_B8G8R8A8")
                                    {
                                        MemoryStream ms = new MemoryStream();
                                        WriteDDSHeaderBGRA(ms, texture.Data.Mips[0].SizeX, texture.Data.Mips[0].SizeY);
                                        ms.Write(texture.Data.Mips[0].BulkData.BulkData, 0, texture.Data.Mips[0].BulkData.BulkData.Length);
                                        ms.Seek(0, SeekOrigin.Begin);
                                        ImageMagick.MagickReadSettings settings = new ImageMagick.MagickReadSettings();
                                        settings.Format = ImageMagick.MagickFormat.Dds;
                                        ImageMagick.MagickImage image = new ImageMagick.MagickImage(ms, settings);
                                        FileStream fs = new FileStream(Path.Combine(args[2], Pictures[i].Index + " - " + Path.GetFileNameWithoutExtension(Pictures[i].File) + ".png"), FileMode.Create);
                                        image.Write(fs, ImageMagick.MagickFormat.Png);
                                        fs.Seek(0, SeekOrigin.Begin);
                                        Pictures[i].Hash = BitConverter.ToString(System.Security.Cryptography.SHA256.Create().ComputeHash(fs)).Replace("-", "");
                                        fs.Close();
                                    }
                                    else if (sPixelFormat == "PF_BC5")
                                    {
                                        MemoryStream ms = new MemoryStream();
                                        WriteDDSHeaderATI2(ms, texture.Data.Mips[0].SizeX, texture.Data.Mips[0].SizeY);
                                        ms.Write(texture.Data.Mips[0].BulkData.BulkData, 0, texture.Data.Mips[0].BulkData.BulkData.Length);
                                        ms.Seek(0, SeekOrigin.Begin);
                                        ImageMagick.MagickReadSettings settings = new ImageMagick.MagickReadSettings();
                                        settings.Format = ImageMagick.MagickFormat.Dds;
                                        ImageMagick.MagickImage image = new ImageMagick.MagickImage(ms, settings);
                                        FileStream fs = new FileStream(Path.Combine(args[2], Pictures[i].Index + " - " + Path.GetFileNameWithoutExtension(Pictures[i].File) + ".png"), FileMode.Create);
                                        image.Write(fs, ImageMagick.MagickFormat.Png);
                                        fs.Seek(0, SeekOrigin.Begin);
                                        Pictures[i].Hash = BitConverter.ToString(System.Security.Cryptography.SHA256.Create().ComputeHash(fs)).Replace("-", "");
                                        fs.Close();
                                    }
                                    else if (sPixelFormat == "PF_G8")
                                    {
                                        MemoryStream ms = new MemoryStream();
                                        WriteDDSHeaderG8(ms, texture.Data.Mips[0].SizeX, texture.Data.Mips[0].SizeY);
                                        ms.Write(texture.Data.Mips[0].BulkData.BulkData, 0, texture.Data.Mips[0].BulkData.BulkData.Length);
                                        ms.Seek(0, SeekOrigin.Begin);
                                        ImageMagick.MagickReadSettings settings = new ImageMagick.MagickReadSettings();
                                        settings.Format = ImageMagick.MagickFormat.Dds;
                                        ImageMagick.MagickImage image = new ImageMagick.MagickImage(ms, settings);
                                        FileStream fs = new FileStream(Path.Combine(args[2], Pictures[i].Index + " - " + Path.GetFileNameWithoutExtension(Pictures[i].File) + ".png"), FileMode.Create);
                                        image.Write(fs, ImageMagick.MagickFormat.Png);
                                        fs.Seek(0, SeekOrigin.Begin);
                                        Pictures[i].Hash = BitConverter.ToString(System.Security.Cryptography.SHA256.Create().ComputeHash(fs)).Replace("-", "");
                                        fs.Close();
                                    }
                                    else
                                    {
                                        throw new Exception("Other formats not supported!");
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Cannot read file, reason: " + ex.Message);
                            }
                        }
                        StreamWriter sw = new StreamWriter(Path.Combine(args[2], "_textures.txt"));
                        for (int i = 0; i < Pictures.Count; i++)
                        {
                            sw.WriteLine("[" + Pictures[i].Index + "][" + Pictures[i].File + "][" + Pictures[i].Hash + "]");
                        }
                        sw.Close();
                    }
                    break;
                case "restore_textures":
                    // restore_textures "C:\Program Files (x86)\Steam\SteamApps\common\The Park" "C:\Program Files (x86)\Steam\SteamApps\common\The Park\Workspace\ConvertedTextures"
                    {
                        List<PictureInfo> Pictures = ReadPictures(Path.Combine(args[2], "_textures.txt"));
                        for (int i = 0; i < Pictures.Count; i++)
                        {
                            Console.WriteLine("[" + (i + 1) + " of " + Pictures.Count + "] " + Pictures[i].File);
                            if (!File.Exists(Path.Combine(args[2], Pictures[i].Index + " - " + Path.GetFileNameWithoutExtension(Pictures[i].File) + ".png")))
                            {
                                Console.WriteLine("Not found, skip.");
                                continue;
                            }
                            byte[] PictureBytes = File.ReadAllBytes(Path.Combine(args[2], Pictures[i].Index + " - " + Path.GetFileNameWithoutExtension(Pictures[i].File) + ".png"));
                            string Hash = BitConverter.ToString(System.Security.Cryptography.SHA256.Create().ComputeHash(PictureBytes, 0, PictureBytes.Length)).Replace("-", "");
                            if (Hash == Pictures[i].Hash)
                            {
                                Console.WriteLine("Not changed, skip.");
                                continue;
                            }
                            PackageReader asset = new PackageReader();
                            asset.OpenPackageFile(Path.Combine(args[1], Pictures[i].File));
                            Texture2D texture = (Texture2D)asset.ExportMap[0].Object;
                            string sPixelFormat = PackageReader.NameMap[texture.PixelFormatName1.ComparisonIndex];
                            if (sPixelFormat.Length >= 6 && sPixelFormat.Substring(0, 6) == "PF_DXT")
                            {
                                int nVersion = int.Parse(sPixelFormat.Substring(6));
                                int n4x4Count = 0;
                                for (int j = 0; j < texture.Data.Mips.Count; j++)
                                {
                                    int nWidth = texture.Data.Mips[j].SizeX;
                                    int nHeight = texture.Data.Mips[j].SizeY;
                                    if (nWidth == 4 && nHeight == 4)
                                        n4x4Count++;
                                    ImageMagick.MagickImage image = new ImageMagick.MagickImage(Path.Combine(args[2], Pictures[i].Index + " - " + Path.GetFileNameWithoutExtension(Pictures[i].File) + ".png"));
                                    if (n4x4Count == 2)
                                    {
                                        image.Resize(2, 2);
                                        image.Resize(4, 4);
                                    }
                                    if (n4x4Count == 3)
                                    {
                                        image.Resize(1, 1);
                                        image.Resize(4, 4);
                                    }
                                    else
                                    {
                                        image.Resize(nWidth, nHeight);
                                    }

                                    /*
                                    MemoryStream ms = new MemoryStream();
                                    image.Write(ms, ImageMagick.MagickFormat.Dds); // so sad, ImageMagic has bug here :(
                                    ms.Seek(87, SeekOrigin.Begin);
                                    int nCurrentVersion = ms.ReadByte() - 0x30;
                                    if (nCurrentVersion != nVersion)
                                        throw new Exception("Bad conversion!");
                                    byte[] BulkData = new byte[ms.Length - 124 - 4];
                                    ms.Seek(124 + 4, SeekOrigin.Begin);
                                    ms.Read(BulkData, 0, BulkData.Length);
                                    texture.Data.Mips[j].BulkData.BulkData = BulkData;
                                    */

                                    string temp1 = Path.GetTempFileName();
                                    File.Move(temp1, temp1 + ".png");
                                    image.Write(temp1 + ".png");
                                    string temp2 = Path.GetTempFileName();
                                    File.Move(temp2, temp2 + ".dds");
                                    System.Diagnostics.Process nvdxt = System.Diagnostics.Process.Start(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase), "ThirdParty\\nvdxt.exe"), "-file \"" + (temp1 + ".png") + "\" -dxt" + nVersion + " -nomipmap -quality_production -output \"" + (temp2 + ".dds") + "\"");
                                    nvdxt.WaitForExit();
                                    FileStream fs = new FileStream(temp2 + ".dds", FileMode.Open);
                                    fs.Seek(87, SeekOrigin.Begin);
                                    int nCurrentVersion = fs.ReadByte() - 0x30;
                                    if (nCurrentVersion != nVersion)
                                        throw new Exception("Bad conversion!");
                                    fs.Seek(124 + 4, SeekOrigin.Begin);
                                    byte[] BulkData = new byte[fs.Length - 124 - 4];
                                    fs.Read(BulkData, 0, BulkData.Length);
                                    texture.Data.Mips[j].BulkData.BulkData = BulkData;
                                    fs.Close();
                                    File.Delete(temp1 + ".png");
                                    File.Delete(temp2 + ".dds");
                                }
                            }
                            else
                            {
                                throw new Exception("Other formats not supported!");
                            }
                            CreatePath(Path.Combine(args[2], "Result", Pictures[i].File));
                            asset.SavePackageFile(Path.Combine(args[2], "Result", Pictures[i].File));
                        }
                    }
                    break;
                case "font_unpack":
                    // font_unpack "C:\Program Files (x86)\Steam\steamapps\common\The Park\AtlanticIslandPark\Content\UI\Fonts\MISPROJE.uasset" "C:\Program Files (x86)\Steam\steamapps\common\The Park\AtlanticIslandPark\Content\UI\Fonts\MISPROJE.ttf"
                    {
                        PackageReader asset = new PackageReader();
                        asset.OpenPackageFile(args[1]);
                        for (int i = 0; i < asset.ExportMap.Count; i++)
                        {
                            if (asset.ExportMap[i].Object.GetType() == typeof(FontBulkData))
                            {
                                FontBulkData font = (FontBulkData)asset.ExportMap[i].Object;
                                FileStream fs = new FileStream(args[2], FileMode.Create);
                                if (font.BulkData.BulkDataDecompressed != null)
                                    fs.Write(font.BulkData.BulkDataDecompressed, 0, font.BulkData.BulkDataDecompressed.Length);
                                else
                                    fs.Write(font.BulkData.BulkData, 0, font.BulkData.BulkData.Length);
                                fs.Close();
                            }
                        }
                    }
                    break;
                case "font_pack":
                    // font_pack "C:\Program Files (x86)\Steam\steamapps\common\The Park\AtlanticIslandPark\Content\UI\Fonts\MISPROJE.uasset" "C:\Program Files (x86)\Steam\steamapps\common\The Park\AtlanticIslandPark\Content\UI\Fonts\MISPROJE.ttf"
                    {
                        PackageReader asset = new PackageReader();
                        asset.OpenPackageFile(args[1]);
                        for (int i = 0; i < asset.ExportMap.Count; i++)
                        {
                            if (asset.ExportMap[i].Object.GetType() == typeof(FontBulkData))
                            {
                                FontBulkData font = (FontBulkData)asset.ExportMap[i].Object;
                                FileStream fs = new FileStream(args[2], FileMode.Open);
                                if (font.BulkData.BulkDataDecompressed != null)
                                {
                                    font.BulkData.BulkDataDecompressed = new byte[fs.Length];
                                    fs.Read(font.BulkData.BulkDataDecompressed, 0, font.BulkData.BulkDataDecompressed.Length);
                                }
                                else
                                {
                                    font.BulkData.BulkData = new byte[fs.Length];
                                    fs.Read(font.BulkData.BulkData, 0, font.BulkData.BulkData.Length);
                                }
                                fs.Close();
                            }
                        }
                        asset.SavePackageFile(args[1]);
                    }
                    break;
                default:
                    Console.WriteLine("Unknown operation!");
                    break;
            }
        }
    }
}
