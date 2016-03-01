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

            FileStream fs = new FileStream(args[1], FileMode.Open);
            OutPath = args[2];
            PakFile pack = new PakFile();
            pack.DeSerialize(fs);
        }
    }
}
