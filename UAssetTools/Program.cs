using System;
using System.Collections.Generic;
using System.IO;

namespace UAssetTools
{
    public class TextInfo
    {
        public string File;
        public string Key;
        public string Text;

        public TextInfo(string file, string key, string text)
        {
            File = file;
            Key = key;
            Text = text;
        }
    }

    public class PictureInfo
    {
        public string File;
        public string Hash;

        public PictureInfo(string file, string hash)
        {
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
            string[] current_files = System.IO.Directory.GetFiles(path);
            for (int i = 0; i < current_files.Length; i++)
            {
                if (Path.GetExtension(current_files[i]) != ".uasset")
                    continue;
                files.Add(current_files[i]);
            }
            string[] current_dirs = System.IO.Directory.GetDirectories(path);
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
                Texts.Add(new TextInfo(file, key, text));
            }
            sr.Close();
            return Texts;
        }

        static void WriteDDSHeader(FileStream fs, int nWidth, int nHeight, int nVersion)
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

        static void WriteDDSHeaderBGRA(FileStream fs, int nWidth, int nHeight)
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

            header[88] = 0x20;
            header[89] = 0x00;
            header[90] = 0x00;
            header[91] = 0x00;

            // R
            header[92] = 0x00;
            header[93] = 0xFF;
            header[94] = 0x00;
            header[95] = 0x00;

            // G
            header[96] = 0x00;
            header[97] = 0x00;
            header[98] = 0xFF;
            header[99] = 0x00;

            // B
            header[100] = 0x00;
            header[101] = 0x00;
            header[102] = 0x00;
            header[103] = 0xFF;

            // A
            header[104] = 0xFF;
            header[105] = 0x00;
            header[106] = 0x00;
            header[107] = 0x00;

            header[109] = 0x10;

            fs.Write(header, 0, header.Length);
        }

        static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("UAssetTools.exe extract_texts <game_path> <path_to_texts_file>");
                //Console.WriteLine("UAssetTools.exe replace_texts <game_path> <path_to_texts_file>");
                Console.WriteLine("UAssetTools.exe extract_textures <game_path> <path_to_extracted_textures>");
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
                                asset.OpenPackageFile(files[i]);
                                for (int j = 0; j < PackageReader.Texts.Count; j++)
                                    Texts.Add(new TextInfo(files[i], PackageReader.Texts[j].Key, PackageReader.Texts[j].Value));
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Cannot read file, reason: " + ex.Message);
                            }
                        }
                        StreamWriter sw = new StreamWriter(args[2]);
                        for (int i = 0; i < Texts.Count; i++)
                        {
                            sw.WriteLine("[" + Texts[i].File.Replace(args[1] + "\\", "") + "][" + Texts[i].Key + "]");
                            sw.WriteLine(Texts[i].Text);
                            sw.WriteLine();
                        }
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
                                    if (sPixelFormat.Substring(0, 6) == "PF_DXT")
                                    {
                                        int nVersion = int.Parse(sPixelFormat.Substring(6));
                                        FileStream fs = new FileStream(Path.Combine(args[2], (Pictures.Count + 1) + " - " + Path.GetFileNameWithoutExtension(files[i])) + ".dds", System.IO.FileMode.Create);
                                        WriteDDSHeader(fs, texture.Data.Mips[0].SizeX, texture.Data.Mips[0].SizeY, nVersion);
                                        fs.Write(texture.Data.Mips[0].BulkData.BulkData, 0, texture.Data.Mips[0].BulkData.BulkData.Length);
                                        fs.Close();
                                        Pictures.Add(new PictureInfo(files[i], ""));
                                    }
                                    else if (sPixelFormat == "PF_B8G8R8A8")
                                    {
                                        FileStream fs = new FileStream(Path.Combine(args[2], (Pictures.Count + 1) + " - " + Path.GetFileNameWithoutExtension(files[i])) + ".dds", System.IO.FileMode.Create);
                                        WriteDDSHeaderBGRA(fs, texture.Data.Mips[0].SizeX, texture.Data.Mips[0].SizeY);
                                        fs.Write(texture.Data.Mips[0].BulkData.BulkData, 0, texture.Data.Mips[0].BulkData.BulkData.Length);
                                        fs.Close();
                                        Pictures.Add(new PictureInfo(files[i], ""));
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
                            sw.WriteLine("[" + (i + 1) + "][" + Pictures[i].File.Replace(args[1] + "\\", "") + "][" + Pictures[i].Hash + "]");
                        }
                        sw.Close();
                    }
                    break;
                default:
                    Console.WriteLine("Unknown operation!");
                    break;
            }
        }
    }
}
