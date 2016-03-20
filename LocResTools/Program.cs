using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Helpers;

namespace LocResTools
{
    class Program
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

        public static LocRes ReadLocResSource(string filename)
        {
            LocRes lr = new LocRes();
            StreamReader sr = new StreamReader(filename);
            int nCurrentNamespace = 0;
            int nMode = 0;
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                if (line.Length > 5 && line.Substring(0, 3) == "=>[")
                {
                    if (nMode == 1)
                        lr.Namespaces[nCurrentNamespace].Value[lr.Namespaces[nCurrentNamespace].Value.Count - 1].String = lr.Namespaces[nCurrentNamespace].Value[lr.Namespaces[nCurrentNamespace].Value.Count - 1].String.Substring(0, lr.Namespaces[nCurrentNamespace].Value[lr.Namespaces[nCurrentNamespace].Value.Count - 1].String.Length - 4);

                    // read string
                    line = line.Substring(3);
                    string Key = line.Substring(0, line.IndexOf(']'));
                    line = line.Substring(line.IndexOf('[') + 1);
                    UInt32 Hash = UInt32.Parse(line.Substring(0, line.IndexOf(']')));
                    lr.Namespaces[nCurrentNamespace].Value.Add(new StringInfo(Key, Hash, ""));
                    nMode = 1;
                    continue;
                }
                if (line.Length > 3 && line.Substring(0, 3) == "=>{")
                {
                    if (nMode == 1)
                        lr.Namespaces[nCurrentNamespace].Value[lr.Namespaces[nCurrentNamespace].Value.Count - 1].String = lr.Namespaces[nCurrentNamespace].Value[lr.Namespaces[nCurrentNamespace].Value.Count - 1].String.Substring(0, lr.Namespaces[nCurrentNamespace].Value[lr.Namespaces[nCurrentNamespace].Value.Count - 1].String.Length - 4);

                    // read namespace
                    string Namespace = line.Substring(3, line.IndexOf('}') - 3);
                    if (Namespace == "[END]")
                        break;
                    nCurrentNamespace = lr.Namespaces.Count;
                    lr.Namespaces.Add(new KeyValuePair<string, List<StringInfo>>(Namespace, new List<StringInfo>()));
                    nMode = 0;
                    continue;
                }
                if (nMode == 1)
                    lr.Namespaces[nCurrentNamespace].Value[lr.Namespaces[nCurrentNamespace].Value.Count - 1].String += line + "\r\n";
            }
            sr.Close();
            return lr;
        }

        public static void WriteLocResSource(string filename, LocRes lr)
        {
            StreamWriter sw = new StreamWriter(filename);
            for (int i = 0; i < lr.Namespaces.Count; i++)
            {
                sw.WriteLine("=>{" + lr.Namespaces[i].Key + "}");
                sw.WriteLine();
                for (int j = 0; j < lr.Namespaces[i].Value.Count; j++)
                {
                    sw.WriteLine("=>[" + lr.Namespaces[i].Value[j].Key + "][" + lr.Namespaces[i].Value[j].Hash + "]");
                    sw.WriteLine(lr.Namespaces[i].Value[j].String);
                    sw.WriteLine();
                }
            }
            sw.WriteLine("=>{[END]}");
            sw.Close();
        }

        static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("LocResTools.exe unpack <path_to_locres> <output_file_with_texts>");
                Console.WriteLine("LocResTools.exe pack <path_to_locres> <input_file_with_texts>");
                Console.WriteLine("LocResTools.exe repack_old <file_with_texts_new_format> <file_with_texts_old_format>");
                return;
            }

            switch (args[0])
            {
                // unpack "C:\Program Files (x86)\Steam\SteamApps\common\The Park\AtlanticIslandPark\Content\Localization\Game\en\Game.locres" "C:\Program Files (x86)\Steam\SteamApps\common\The Park\AtlanticIslandPark\Content\Localization\Game\en\TEST_Game.locres.txt"
                case "unpack":
                    {
                        Stream fs = new FileStream(args[1], FileMode.Open);
                        LocRes lr = new LocRes();
                        lr.DeSerialize(fs);
                        fs.Close();
                        StreamWriter sw = new StreamWriter(args[2]);
                        for (int i = 0; i < lr.Namespaces.Count; i++)
                        {
                            sw.WriteLine("=>{" + lr.Namespaces[i].Key + "}");
                            sw.WriteLine();
                            for (int j = 0; j < lr.Namespaces[i].Value.Count; j++)
                            {
                                sw.WriteLine("=>[" + lr.Namespaces[i].Value[j].Key + "][" + lr.Namespaces[i].Value[j].Hash + "]");
                                sw.WriteLine(lr.Namespaces[i].Value[j].String);
                                sw.WriteLine();
                            }
                        }
                        sw.WriteLine("=>{[END]}");
                        sw.Close();
                    }
                    break;
                // pack "C:\Program Files (x86)\Steam\SteamApps\common\The Park\AtlanticIslandPark\Content\Localization\Game\en\Game.locres" "C:\Program Files (x86)\Steam\SteamApps\common\The Park\AtlanticIslandPark\Content\Localization\Game\en\TEST_Game.locres.txt"
                case "pack":
                    {
                        LocRes lr = new LocRes();
                        StreamReader sr = new StreamReader(args[2]);
                        int nCurrentNamespace = 0;
                        int nMode = 0;
                        while (!sr.EndOfStream)
                        {
                            string line = sr.ReadLine();
                            if (line.Length > 5 && line.Substring(0, 3) == "=>[")
                            {
                                if (nMode == 1)
                                    lr.Namespaces[nCurrentNamespace].Value[lr.Namespaces[nCurrentNamespace].Value.Count - 1].String = lr.Namespaces[nCurrentNamespace].Value[lr.Namespaces[nCurrentNamespace].Value.Count - 1].String.Substring(0, lr.Namespaces[nCurrentNamespace].Value[lr.Namespaces[nCurrentNamespace].Value.Count - 1].String.Length - 4);

                                // read string
                                line = line.Substring(3);
                                string Key = line.Substring(0, line.IndexOf(']'));
                                line = line.Substring(line.IndexOf('[') + 1);
                                UInt32 Hash = UInt32.Parse(line.Substring(0, line.IndexOf(']')));
                                lr.Namespaces[nCurrentNamespace].Value.Add(new StringInfo(Key, Hash, ""));
                                nMode = 1;
                                continue;
                            }
                            if (line.Length > 3 && line.Substring(0, 3) == "=>{")
                            {
                                if (nMode == 1)
                                    lr.Namespaces[nCurrentNamespace].Value[lr.Namespaces[nCurrentNamespace].Value.Count - 1].String = lr.Namespaces[nCurrentNamespace].Value[lr.Namespaces[nCurrentNamespace].Value.Count - 1].String.Substring(0, lr.Namespaces[nCurrentNamespace].Value[lr.Namespaces[nCurrentNamespace].Value.Count - 1].String.Length - 4);

                                // read namespace
                                string Namespace = line.Substring(3, line.IndexOf('}') - 3);
                                if (Namespace == "[END]")
                                    break;
                                nCurrentNamespace = lr.Namespaces.Count;
                                lr.Namespaces.Add(new KeyValuePair<string, List<StringInfo>>(Namespace, new List<StringInfo>()));
                                nMode = 0;
                                continue;
                            }
                            if (nMode == 1)
                                lr.Namespaces[nCurrentNamespace].Value[lr.Namespaces[nCurrentNamespace].Value.Count - 1].String += line + "\r\n";
                        }
                        sr.Close();
                        Stream fs = new FileStream(args[1], FileMode.Create);
                        lr.Serialize(fs);
                        fs.Close();
                    }
                    break;
                // repack_old "C:\Program Files (x86)\Steam\SteamApps\common\The Park\AtlanticIslandPark\Content\Localization\Game\en\RU_Game.locres.txt" "C:\Program Files (x86)\Steam\SteamApps\common\The Park\AtlanticIslandPark\Content\Localization\Game\en\Subtitles.txt"
                case "repack_old":
                    {
                        LocRes lr = new LocRes();
                        StreamReader sr = new StreamReader(args[1]);
                        int nCurrentNamespace = 0;
                        int nMode = 0;
                        while (!sr.EndOfStream)
                        {
                            string line = sr.ReadLine();
                            if (line.Length > 5 && line.Substring(0, 3) == "=>[")
                            {
                                if (nMode == 1)
                                    lr.Namespaces[nCurrentNamespace].Value[lr.Namespaces[nCurrentNamespace].Value.Count - 1].String = lr.Namespaces[nCurrentNamespace].Value[lr.Namespaces[nCurrentNamespace].Value.Count - 1].String.Substring(0, lr.Namespaces[nCurrentNamespace].Value[lr.Namespaces[nCurrentNamespace].Value.Count - 1].String.Length - 4);

                                // read string
                                line = line.Substring(3);
                                string Key = line.Substring(0, line.IndexOf(']'));
                                line = line.Substring(line.IndexOf('[') + 1);
                                UInt32 Hash = UInt32.Parse(line.Substring(0, line.IndexOf(']')));
                                lr.Namespaces[nCurrentNamespace].Value.Add(new StringInfo(Key, Hash, ""));
                                nMode = 1;
                                continue;
                            }
                            if (line.Length > 3 && line.Substring(0, 3) == "=>{")
                            {
                                if (nMode == 1)
                                    lr.Namespaces[nCurrentNamespace].Value[lr.Namespaces[nCurrentNamespace].Value.Count - 1].String = lr.Namespaces[nCurrentNamespace].Value[lr.Namespaces[nCurrentNamespace].Value.Count - 1].String.Substring(0, lr.Namespaces[nCurrentNamespace].Value[lr.Namespaces[nCurrentNamespace].Value.Count - 1].String.Length - 4);

                                // read namespace
                                string Namespace = line.Substring(3, line.IndexOf('}') - 3);
                                if (Namespace == "[END]")
                                    break;
                                nCurrentNamespace = lr.Namespaces.Count;
                                lr.Namespaces.Add(new KeyValuePair<string, List<StringInfo>>(Namespace, new List<StringInfo>()));
                                nMode = 0;
                                continue;
                            }
                            if (nMode == 1)
                                lr.Namespaces[nCurrentNamespace].Value[lr.Namespaces[nCurrentNamespace].Value.Count - 1].String += line + "\r\n";
                        }
                        sr.Close();

                        // replace
                        List<TextInfo> Texts = ReadTexts(args[2]);
                        for (int i = 0; i < Texts.Count; i++)
                        {
                            for (int j = 0; j < lr.Namespaces[0].Value.Count; j++)
                            {
                                if (lr.Namespaces[0].Value[j].Key == Texts[i].Key)
                                    lr.Namespaces[0].Value[j].String = Texts[i].Text;
                            }
                        }

                        StreamWriter sw = new StreamWriter(args[1]);
                        for (int i = 0; i < lr.Namespaces.Count; i++)
                        {
                            sw.WriteLine("=>{" + lr.Namespaces[i].Key + "}");
                            sw.WriteLine();
                            for (int j = 0; j < lr.Namespaces[i].Value.Count; j++)
                            {
                                sw.WriteLine("=>[" + lr.Namespaces[i].Value[j].Key + "][" + lr.Namespaces[i].Value[j].Hash + "]");
                                sw.WriteLine(lr.Namespaces[i].Value[j].String);
                                sw.WriteLine();
                            }
                        }
                        sw.WriteLine("=>{[END]}");
                        sw.Close();
                    }
                    break;
                // correction "C:\Program Files (x86)\Steam\SteamApps\common\The Park\AtlanticIslandPark\Content\Localization\Game\en\EN_Game.locres.txt" "C:\Program Files (x86)\Steam\SteamApps\common\The Park\AtlanticIslandPark\Content\Localization\Game\en\RU_Game.locres.txt"
                case "correction":
                    {
                        LocRes lr_en = ReadLocResSource(args[1]);
                        LocRes lr_ru = ReadLocResSource(args[2]);

                        for (int i = 0; i < lr_en.Namespaces[0].Value.Count; i++)
                        {
                            for (int j = i + 1; j < lr_en.Namespaces[0].Value.Count; j++)
                            {
                                if (lr_en.Namespaces[0].Value[i].Key == lr_en.Namespaces[0].Value[j].Key)
                                {
                                    lr_en.Namespaces[0].Value.RemoveAt(j);
                                    j--;
                                }
                            }
                        }

                        for (int i = 0; i < lr_en.Namespaces[0].Value.Count; i++)
                        {
                            if (lr_en.Namespaces[0].Value[i].Hash != CRC32.StrCrc32(lr_en.Namespaces[0].Value[i].String))
                                lr_en.Namespaces[0].Value[i].Hash = CRC32.StrCrc32(lr_en.Namespaces[0].Value[i].String);
                        }

                        WriteLocResSource(args[1], lr_en);

                        for (int i = 0; i < lr_en.Namespaces[0].Value.Count; i++)
                        {
                            for (int j = 0; j < lr_ru.Namespaces[0].Value.Count; j++)
                            {
                                if (lr_en.Namespaces[0].Value[i].Key == lr_ru.Namespaces[0].Value[j].Key)
                                    lr_en.Namespaces[0].Value[i].String = lr_ru.Namespaces[0].Value[j].String;
                            }
                        }

                        WriteLocResSource(args[2], lr_en);
                    }
                    break;
                default:
                    Console.WriteLine("Unknown operation!");
                    break;
            }
        }
    }
}
