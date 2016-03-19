using System;
using System.Collections.Generic;
using System.IO;

namespace PakTools
{
    class Program
    {
        public static string OutPath;
        public static string OutPrefix;

        public static void GetFiles(string path, ref List<string> files)
        {
            if (Path.GetFileName(path) == ".git")
                return;
            string[] current_files = Directory.GetFiles(path);
            for (int i = 0; i < current_files.Length; i++)
                files.Add(current_files[i]);
            string[] current_dirs = Directory.GetDirectories(path);
            for (int i = 0; i < current_dirs.Length; i++)
                GetFiles(current_dirs[i], ref files);
        }

        static void Main(string[] args)
        {
            if (args.Length != 3 && args.Length != 4)
            {
                Console.WriteLine("PakTools.exe unpack <path_to_pak> <output_folder>");
                Console.WriteLine("PakTools.exe pack <path_to_pak> <input_folder> <mount_pount>");
                return;
            }
            switch (args[0])
            {
                case "unpack":
                    // unpack "C:\Program Files (x86)\Steam\SteamApps\common\The Park\AtlanticIslandPark\Content\Paks\AtlanticIslandPark-WindowsNoEditor.pak.bak" "C:\Program Files (x86)\Steam\SteamApps\common\The Park"
                    // unpack "C:\Program Files (x86)\Steam\steamapps\common\The Park\Workspace\Russian.pak" "C:\Program Files (x86)\Steam\steamapps\common\The Park\Workspace\unpack_test"
                    {
                        FileStream fs = new FileStream(args[1], FileMode.Open);
                        OutPath = args[2];
                        PakFile pack = new PakFile();
                        pack.DeSerialize(fs);
                        fs.Close();
                    }
                    break;
                case "pack":
                    // pack "C:\Program Files (x86)\Steam\steamapps\common\The Park\Workspace\AtlanticIslandPark-Russian.pak" "C:\Program Files (x86)\Steam\steamapps\common\The Park\Workspace\result" "../../../"
                    {
                        FileStream fs = new FileStream(args[1], FileMode.Create);
                        OutPath = args[2];
                        OutPrefix = args[3];
                        PakFile pack = new PakFile();
                        pack.Serialize(fs);
                        fs.Close();
                    }
                    break;
                default:
                    Console.WriteLine("Unknown operation!");
                    break;
            }
        }
    }
}
