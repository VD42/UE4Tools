using System;
using System.IO;

namespace Helpers
{
    public class BinaryHelper
    {
        public static bool bUseUnicode = false;

        public Int32 ReadInt32(Stream fs)
        {
            byte[] buf = new byte[4];
            fs.Read(buf, 0, 4);
            return BitConverter.ToInt32(buf, 0);
        }

        public string ReadString(Stream fs)
        {
            int nLength = ReadInt32(fs);
            if (nLength > 0 && !bUseUnicode)
            {
                if (nLength > 10000)
                    throw new Exception("Probably bad read!");
                byte[] buf = new byte[nLength];
                fs.Read(buf, 0, nLength);
                return System.Text.Encoding.ASCII.GetString(buf, 0, nLength - 1);
            }
            else if (nLength < 0)
            {
                nLength = -nLength * 2;
                if (nLength > 20000)
                    throw new Exception("Probably bad read!");
                byte[] buf = new byte[nLength];
                fs.Read(buf, 0, nLength);
                return System.Text.Encoding.Unicode.GetString(buf, 0, nLength - 2);
            }
            return "";
        }

        public UInt32 ReadUInt32(Stream fs)
        {
            byte[] buf = new byte[4];
            fs.Read(buf, 0, 4);
            return BitConverter.ToUInt32(buf, 0);
        }

        public Guid ReadGuid(Stream fs)
        {
            byte[] buf = new byte[16];
            fs.Read(buf, 0, 16);
            return new Guid(buf);
        }

        public Int16 ReadInt16(Stream fs)
        {
            byte[] buf = new byte[2];
            fs.Read(buf, 0, 2);
            return BitConverter.ToInt16(buf, 0);
        }

        public Int64 ReadInt64(Stream fs)
        {
            byte[] buf = new byte[8];
            fs.Read(buf, 0, 8);
            return BitConverter.ToInt64(buf, 0);
        }

        public byte ReadByte(Stream fs)
        {
            int value = fs.ReadByte();
            return (byte)value;
        }

        public bool ReadBool(Stream fs)
        {
            UInt32 value = ReadUInt32(fs);
            if (value == 0)
                return false;
            return true;
        }

        public UInt64 ReadUInt64(Stream fs)
        {
            byte[] buf = new byte[8];
            fs.Read(buf, 0, 8);
            return BitConverter.ToUInt64(buf, 0);
        }

        public float ReadFloat(Stream fs)
        {
            byte[] buf = new byte[4];
            fs.Read(buf, 0, 4);
            return BitConverter.ToSingle(buf, 0); ;
        }

        public UInt16 ReadUInt16(Stream fs)
        {
            byte[] buf = new byte[2];
            fs.Read(buf, 0, 2);
            return BitConverter.ToUInt16(buf, 0);
        }

        public void WriteInt32(Stream fs, Int32 Value)
        {
            byte[] buf = BitConverter.GetBytes(Value);
            fs.Write(buf, 0, buf.Length);
        }

        public void WriteString(Stream fs, string Value)
        {
            if (Value.Length == 0)
            {
                WriteInt32(fs, 0);
                return;
            }
            bool bNeedUnicode = false;
            if (bUseUnicode)
            {
                bNeedUnicode = true;
            }
            else
            {
                for (int i = 0; i < Value.Length; i++)
                {
                    if (!('\u0000' <= Value[i] && Value[i] <= '\u00FF'))
                    {
                        bNeedUnicode = true;
                        break;
                    }
                }
            }
            if (bNeedUnicode)
            {
                int nLength = -(Value.Length + 1);
                WriteInt32(fs, nLength);
                byte[] buf = System.Text.Encoding.Unicode.GetBytes(Value);
                fs.Write(buf, 0, buf.Length);
                WriteInt16(fs, 0);
            }
            else
            {
                int nLength = Value.Length + 1;
                WriteInt32(fs, nLength);
                byte[] buf = System.Text.Encoding.ASCII.GetBytes(Value);
                fs.Write(buf, 0, buf.Length);
                WriteByte(fs, 0);
            }
        }

        public void WriteInt16(Stream fs, Int16 Value)
        {
            byte[] buf = BitConverter.GetBytes(Value);
            fs.Write(buf, 0, buf.Length);
        }

        public void WriteByte(Stream fs, byte Value)
        {
            fs.WriteByte(Value);
        }

        public void WriteUInt32(Stream fs, UInt32 Value)
        {
            byte[] buf = BitConverter.GetBytes(Value);
            fs.Write(buf, 0, buf.Length);
        }

        public void WriteGuid(Stream fs, Guid Value)
        {
            byte[] buf = Value.ToByteArray();
            fs.Write(buf, 0, buf.Length);
        }

        public void WriteInt64(Stream fs, Int64 Value)
        {
            byte[] buf = BitConverter.GetBytes(Value);
            fs.Write(buf, 0, buf.Length);
        }

        public void WriteBool(Stream fs, bool Value)
        {
            if (Value)
                WriteUInt32(fs, 1);
            else
                WriteUInt32(fs, 0);
        }

        public void WriteFloat(Stream fs, float Value)
        {
            byte[] buf = BitConverter.GetBytes(Value);
            fs.Write(buf, 0, 4);
        }

        public void WriteUInt16(Stream fs, UInt16 Value)
        {
            byte[] buf = BitConverter.GetBytes(Value);
            fs.Write(buf, 0, buf.Length);
        }
    }
}
