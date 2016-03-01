using System;
using System.IO;

namespace Helpers
{
    public class BinaryHelper
    {
        public Int32 ReadInt32(FileStream fs)
        {
            byte[] buf = new byte[4];
            fs.Read(buf, 0, 4);
            return BitConverter.ToInt32(buf, 0);
        }

        public string ReadString(FileStream fs)
        {
            int nLength = ReadInt32(fs);
            if (nLength > 0)
            {
                byte[] buf = new byte[nLength];
                fs.Read(buf, 0, nLength);
                return System.Text.Encoding.ASCII.GetString(buf, 0, nLength - 1);
            }
            else if (nLength < 0)
            {
                nLength = -nLength * 2;
                byte[] buf = new byte[nLength];
                fs.Read(buf, 0, nLength);
                return System.Text.Encoding.Unicode.GetString(buf, 0, nLength - 2);
            }
            return "";
        }

        public UInt32 ReadUInt32(FileStream fs)
        {
            byte[] buf = new byte[4];
            fs.Read(buf, 0, 4);
            return BitConverter.ToUInt32(buf, 0);
        }

        public Guid ReadGuid(FileStream fs)
        {
            byte[] buf = new byte[16];
            fs.Read(buf, 0, 16);
            return new Guid(buf);
        }

        public Int16 ReadInt16(FileStream fs)
        {
            byte[] buf = new byte[2];
            fs.Read(buf, 0, 2);
            return BitConverter.ToInt16(buf, 0);
        }

        public Int64 ReadInt64(FileStream fs)
        {
            byte[] buf = new byte[8];
            fs.Read(buf, 0, 8);
            return BitConverter.ToInt64(buf, 0);
        }

        public byte ReadByte(FileStream fs)
        {
            int value = fs.ReadByte();
            return (byte)value;
        }

        public bool ReadBool(FileStream fs)
        {
            UInt32 value = ReadUInt32(fs);
            if (value == 0)
                return false;
            return true;
        }

        public UInt64 ReadUInt64(FileStream fs)
        {
            byte[] buf = new byte[8];
            fs.Read(buf, 0, 8);
            return BitConverter.ToUInt64(buf, 0);
        }

        public float ReadFloat(FileStream fs)
        {
            byte[] buf = new byte[4];
            fs.Read(buf, 0, 4);
            return BitConverter.ToSingle(buf, 0); ;
        }

        public void WriteInt32(FileStream fs, Int32 Value)
        {
            byte[] buf = BitConverter.GetBytes(Value);
            fs.Write(buf, 0, buf.Length);
        }

        public void WriteString(FileStream fs, string Value)
        {
            if (Value.Length == 0)
            {
                WriteInt32(fs, 0);
                return;
            }
            bool bNeedUnicode = false;
            for (int i = 0; i < Value.Length; i++)
            {
                if (!('\u0000' <= Value[i] && Value[i] <= '\u00FF'))
                {
                    bNeedUnicode = true;
                    break;
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

        public void WriteInt16(FileStream fs, Int16 Value)
        {
            byte[] buf = BitConverter.GetBytes(Value);
            fs.Write(buf, 0, buf.Length);
        }

        public void WriteByte(FileStream fs, byte Value)
        {
            fs.WriteByte(Value);
        }

        public void WriteUInt32(FileStream fs, UInt32 Value)
        {
            byte[] buf = BitConverter.GetBytes(Value);
            fs.Write(buf, 0, buf.Length);
        }

        public void WriteGuid(FileStream fs, Guid Value)
        {
            byte[] buf = Value.ToByteArray();
            fs.Write(buf, 0, buf.Length);
        }

        public void WriteInt64(FileStream fs, Int64 Value)
        {
            byte[] buf = BitConverter.GetBytes(Value);
            fs.Write(buf, 0, buf.Length);
        }

        public void WriteBool(FileStream fs, bool Value)
        {
            if (Value)
                WriteUInt32(fs, 1);
            else
                WriteUInt32(fs, 0);
        }

        public void WriteFloat(FileStream fs, float Value)
        {
            byte[] buf = BitConverter.GetBytes(Value);
            fs.Write(buf, 0, 4);
        }
    }
}
