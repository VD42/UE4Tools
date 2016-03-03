using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Helpers;
using System.IO;

namespace LocResTools
{
    public class StringInfo : BinaryHelper
    {
        public string Key;
        public UInt32 Hash;
        public string String;

        public StringInfo()
        {

        }

        public StringInfo(string key, UInt32 hash, string text)
        {
            Key = key;
            Hash = hash;
            String = text;
        }

        public void DeSerialize(FileStream fs)
        {
            Key = ReadString(fs);
            Hash = ReadUInt32(fs);
            String = ReadString(fs);
        }

        public void Serialize(FileStream fs)
        {
            WriteString(fs, Key);
            WriteUInt32(fs, Hash);
            WriteString(fs, String);
        }
    }

    public class LocRes : BinaryHelper
    {
        public List<KeyValuePair<string, List<StringInfo>>> Namespaces;

        public LocRes()
        {
            Namespaces = new List<KeyValuePair<string, List<StringInfo>>>();
        }

        public void DeSerialize(FileStream fs)
        {
            BinaryHelper.bUseUnicode = true;
            UInt32 Count = ReadUInt32(fs);
            for (int i = 0; i < (int)Count; i++)
            {
                string Namespace = ReadString(fs);
                Namespaces.Add(new KeyValuePair<string, List<StringInfo>>(Namespace, new List<StringInfo>()));
                UInt32 StringsCount = ReadUInt32(fs);
                for (int j = 0; j < (int)StringsCount; j++)
                {
                    Namespaces[i].Value.Add(new StringInfo());
                    Namespaces[i].Value[j].DeSerialize(fs);
                }
            }
            BinaryHelper.bUseUnicode = false;
        }

        public void Serialize(FileStream fs)
        {
            BinaryHelper.bUseUnicode = true;
            WriteUInt32(fs, (UInt32)Namespaces.Count);
            for (int i = 0; i < Namespaces.Count; i++)
            {
                WriteString(fs, Namespaces[i].Key);
                WriteUInt32(fs, (UInt32)Namespaces[i].Value.Count);
                for (int j = 0; j < Namespaces[i].Value.Count; j++)
                    Namespaces[i].Value[j].Serialize(fs);
            }
            BinaryHelper.bUseUnicode = false;
        }
    }
}
