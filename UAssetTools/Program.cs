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

        static void WriteDDSHeader(FileStream fs, int nWidth, int nHeight)
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
            header[87] = 0x35;

            header[109] = 0x10;

            fs.Write(header, 0, header.Length);
        }

        static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("UAssetTools.exe extract_texts <game_path> <path_to_texts_file>");
                return;
            }

            switch (args[0])
            {
                case "extract_texts":
                    List<String> files = new List<string>();
                    GetFiles(args[1], ref files);
                    List<TextInfo> Texts = new List<TextInfo>();
                    for (int i = 0; i < files.Count; i++)
                    {
                        Console.WriteLine("[" + i + " of " + files.Count + "] " + files[i]);
                        PackageReader asset = new PackageReader();
                        try
                        {
                            asset.OpenPackageFile(files[i]);
                            for (int j = 0; j < PackageReader.Texts.Count; j++)
                                Texts.Add(new TextInfo(files[i], PackageReader.Texts[j].Key, PackageReader.Texts[j].Value));
                        }
                        catch (Exception ex)
                        {
                            //if (files[i].IndexOf("bogeyman_01_weapon_n.uasset") > -1)
                            //    Console.WriteLine("Cannot read file " + files[i] + ": " + ex.Message);
                            //if (files[i].IndexOf("Lorraine_Anger_Subtitles.uasset") > -1)
                            //    Console.WriteLine("Cannot read file " + files[i] + ": " + ex.Message);
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
                    break;
                default:
                    Console.WriteLine("Unknown operation!");
                    Console.WriteLine("UAssetTools.exe extract_texts <game_path> <output_folder>");
                    break;
            }

            /*
            if (false)
            {
                List<String> files = new List<string>();
                GetFiles("C:\\Program Files (x86)\\Steam\\steamapps\\common\\The Park - un", ref files);
                for (int i = 0; i < files.Count; i++)
                {
                    PackageReader asset = new PackageReader();
                    try
                    {
                        asset.OpenPackageFile(files[i]);
                        if (asset.ExportMap[0].Object.GetType() != typeof(Texture2D))
                            continue;
                        FileStream fs = new FileStream(Path.Combine("C:\\Users\\VD42\\Documents\\Res", Path.GetFileName(files[i]) + ".dds"), FileMode.Create);
                        WriteDDSHeader(fs, ((Texture2D)asset.ExportMap[0].Object).Data.Mips[0].SizeX, ((Texture2D)asset.ExportMap[0].Object).Data.Mips[0].SizeY);
                        fs.Write(((Texture2D)asset.ExportMap[0].Object).Data.Mips[0].BulkData.BulkData, 0, ((Texture2D)asset.ExportMap[0].Object).Data.Mips[0].BulkData.BulkData.Length);
                        fs.Close();
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            string sObjectName = PackageReader.NameMap[asset.ImportMap[-asset.ExportMap[0].ClassIndex - 1].ObjectName.ComparisonIndex];
                            if (sObjectName == "Texture2D")
                            {
                                int j = 0;
                                j++;
                            }
                        }
                        catch (Exception ex2)
                        {
                            Console.WriteLine(ex2);
                        }
                        Console.WriteLine(files[i]);
                        Console.WriteLine(ex);
                        Console.WriteLine();
                    }
                }
                Console.ReadKey();
                return;
            }


            PackageReader pr = new PackageReader();
            //pr.OpenPackageFile("C:\\Program Files (x86)\\Steam\\steamapps\\common\\The Park - un\\AtlanticIslandPark\\Content\\Sound\\voiceover\\Voice_Over_NEW\\en\\cinematic_VO\\Lorraine_new_haventseenmrbear.uasset");
            //pr.OpenPackageFile("C:\\Program Files (x86)\\Steam\\steamapps\\common\\The Park - un\\AtlanticIslandPark\\Content\\UI\\MainMenu\\MainMenu.uasset");
            //pr.OpenPackageFile("C:\\Program Files (x86)\\Steam\\steamapps\\common\\The Park - un\\AtlanticIslandPark\\Content\\Gameplay\\Moving\\FerrisWheel\\SpeedDirectionEnum.uasset");
            //pr.OpenPackageFile("C:\\Program Files (x86)\\Steam\\steamapps\\common\\The Park\\AtlanticIslandPark\\Content\\Environment\\Props\\Textures\\T_chad_plaque_D.uasset");
            //pr.OpenPackageFile("C:\\Program Files (x86)\\Steam\\steamapps\\common\\The Park - un\\AtlanticIslandPark\\Content\\Assets\\Effects\\Textures\\T_Clouds_01.uasset");
            //pr.SavePackageFile("C:\\Program Files (x86)\\Steam\\steamapps\\common\\The Park - un\\AtlanticIslandPark\\Content\\Assets\\Effects\\Textures\\T_Clouds_01_out.uasset");

            pr.OpenPackageFile("C:\\Program Files (x86)\\Steam\\steamapps\\common\\The Park\\AtlanticIslandPark\\Content\\Environment\\Props\\Textures\\T_flyer_D.uasset");
            FileStream fs2 = new FileStream("C:\\Users\\VD42\\Documents\\T_flyer_D.uasset.dds", FileMode.Open);
            byte[] dds = new byte[fs2.Length - 124 - 4];
            fs2.Seek(124 + 4, SeekOrigin.Begin);
            fs2.Read(dds, 0, dds.Length);
            ((Texture2D)pr.ExportMap[0].Object).Data.Mips[0].BulkData.BulkData = dds;
            pr.SavePackageFile("C:\\Users\\VD42\\Documents\\T_flyer_D.uasset");

            */
            /*if (pr.ExportMap[0].Object.GetType() == typeof(Texture2D))
            {
                FileStream fs = new FileStream("C:\\Users\\VD42\\Documents\\res.dds", System.IO.FileMode.Create);
                WriteDDSHeader(fs, ((Texture2D)pr.ExportMap[0].Object).Data.Mips[0].SizeX, ((Texture2D)pr.ExportMap[0].Object).Data.Mips[0].SizeY);
                fs.Write(((Texture2D)pr.ExportMap[0].Object).Data.Mips[0].BulkData.BulkData, 0, ((Texture2D)pr.ExportMap[0].Object).Data.Mips[0].BulkData.BulkData.Length);
                fs.Close();
            }*/
        }
    }
}
