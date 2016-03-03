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
        static void Main(string[] args)
        {
            //UInt32 Completed = CRC32.StrCrc32("Quit Game");
            //return;
            if (args.Length != 3)
            {
                Console.WriteLine("LocResTools.exe unpack <path_to_locres> <output_file>");
                Console.WriteLine("LocResTools.exe pack <path_to_locres> <input_file>");
                return;
            }

            switch (args[0])
            {
                // unpack "C:\Program Files (x86)\Steam\SteamApps\common\The Park\Engine\Content\Localization\Engine\en\Engine.locres" "C:\Program Files (x86)\Steam\SteamApps\common\The Park\Engine\Content\Localization\Engine\en\Engine.locres.txt"
                case "unpack":
                    {
                        FileStream fs = new FileStream(args[1], FileMode.Open);
                        LocRes lr = new LocRes();
                        lr.DeSerialize(fs);
                        fs.Close();
                        StreamWriter sw = new StreamWriter(args[2]);
                        for (int i = 0; i < lr.Namespaces.Count; i++)
                        {
                            sw.WriteLine("{" + lr.Namespaces[i].Key + "}");
                            sw.WriteLine();
                            for (int j = 0; j < lr.Namespaces[i].Value.Count; j++)
                            {
                                sw.WriteLine("[" + lr.Namespaces[i].Value[j].Key + "][" + lr.Namespaces[i].Value[j].Hash + "]");
                                sw.WriteLine(lr.Namespaces[i].Value[j].String.Replace("\n", "{[br]}").Replace("\r", "{[cr]}"));
                                sw.WriteLine();
                            }
                        }
                        sw.Close();
                    }
                    break;
                // pack "C:\Program Files (x86)\Steam\SteamApps\common\The Park\Engine\Content\Localization\Engine\en\Engine_result.locres" "C:\Program Files (x86)\Steam\SteamApps\common\The Park\Engine\Content\Localization\Engine\en\Engine.locres.txt"
                // pack "C:\Program Files (x86)\Steam\SteamApps\common\The Park\AtlanticIslandPark\Content\Localization\Game\en\Game.locres" "C:\Program Files (x86)\Steam\SteamApps\common\The Park\AtlanticIslandPark\Content\Localization\Game\en\Game.locres.txt"
                case "pack":
                    {
                        LocRes lr = new LocRes();
                        StreamReader sr = new StreamReader(args[2]);
                        int nCurrentNamespace = 0;
                        while (!sr.EndOfStream)
                        {
                            string line = sr.ReadLine();
                            if (line == "")
                                continue;
                            if (line[0] == '{')
                            {
                                string Namespace = line.Substring(1, line.IndexOf('}') - 1);
                                nCurrentNamespace = lr.Namespaces.Count;
                                lr.Namespaces.Add(new KeyValuePair<string, List<StringInfo>>(Namespace, new List<StringInfo>()));
                                continue;
                            }
                            if (line[0] == '[')
                            {
                                line = line.Substring(line.IndexOf('[') + 1);
                                string Key = line.Substring(0, line.IndexOf(']'));
                                line = line.Substring(line.IndexOf('[') + 1);
                                UInt32 Hash = UInt32.Parse(line.Substring(0, line.IndexOf(']')));
                                string String = sr.ReadLine();
                                lr.Namespaces[nCurrentNamespace].Value.Add(new StringInfo(Key, Hash, String.Replace("{[br]}", "\n").Replace("{[cr]}", "\r")));
                                continue;
                            }
                            throw new Exception("Something wrong!");
                        }
                        sr.Close();
                        FileStream fs = new FileStream(args[1], FileMode.Create);
                        lr.Serialize(fs);
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
