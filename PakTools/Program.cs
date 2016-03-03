using System;
using System.IO;

namespace PakTools
{
    class Program
    {
        public static string OutPath;

        static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("PakTools.exe unpack <path_to_pak> <output_folder>");
                return;
            }

            // unpack "C:\Program Files (x86)\Steam\SteamApps\common\The Park\AtlanticIslandPark\Content\Paks\AtlanticIslandPark-WindowsNoEditor.pak.bak" "C:\Program Files (x86)\Steam\SteamApps\common\The Park"

            FileStream fs = new FileStream(args[1], FileMode.Open);
            OutPath = args[2];
            PakFile pack = new PakFile();
            pack.DeSerialize(fs);
        }
    }
}
