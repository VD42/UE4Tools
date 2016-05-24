using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace UAssetTools
{
    public static class Extensions
    {
        public static void Serialize(this Int32 val, FArchive ar)
        {
            if (ar.IsReading())
                val = BitConverter.ToInt32(ar.Read(4), 0);
            else if (ar.IsWriting())
                ar.Write(BitConverter.GetBytes(val));
        }

        public static void Serialize(this UInt32 val, FArchive ar)
        {
            if (ar.IsReading())
                val = BitConverter.ToUInt32(ar.Read(4), 0);
            else if (ar.IsWriting())
                ar.Write(BitConverter.GetBytes(val));
        }

        public static void Serialize(this Guid val, FArchive ar)
        {
            if (ar.IsReading())
                val = new Guid(ar.Read(16));
            else if (ar.IsWriting())
                ar.Write(val.ToByteArray());
        }

        public static void Serialize(this Int16 val, FArchive ar)
        {
            if (ar.IsReading())
                val = BitConverter.ToInt16(ar.Read(2), 0);
            else if (ar.IsWriting())
                ar.Write(BitConverter.GetBytes(val));
        }

        public static void Serialize(this Byte val, FArchive ar)
        {
            if (ar.IsReading())
                val = ar.Read(1)[0];
            else if (ar.IsWriting())
                ar.Write(new byte[1] { val });
        }

        public static void Serialize(this Boolean val, FArchive ar)
        {
            if (ar.IsReading())
            {
                UInt32 tmp = 0;
                tmp.Serialize(ar);
                if (tmp == 0)
                    val = false;
                else
                    val = true;
            }
            else if (ar.IsWriting())
            {
                UInt32 tmp = 0;
                if (val)
                    tmp = 1;
                tmp.Serialize(ar);
            }
        }

        public static void Serialize(this UInt64 val, FArchive ar)
        {
            if (ar.IsReading())
                val = BitConverter.ToUInt64(ar.Read(8), 0);
            else if (ar.IsWriting())
                ar.Write(BitConverter.GetBytes(val));
        }

        public static void Serialize(this Single val, FArchive ar)
        {
            if (ar.IsReading())
                val = BitConverter.ToSingle(ar.Read(4), 0);
            else if (ar.IsWriting())
                ar.Write(BitConverter.GetBytes(val));
        }

        public static void Serialize(this UInt16 val, FArchive ar)
        {
            if (ar.IsReading())
                val = BitConverter.ToUInt16(ar.Read(2), 0);
            else if (ar.IsWriting())
                ar.Write(BitConverter.GetBytes(val));
        }

        public static void Serialize(this String val, FArchive ar)
        {
            if (ar.IsReading())
            {
                Int32 nLength = 0;
                nLength.Serialize(ar);
                if (nLength > 0 && !ar.ForceUnicode())
                {
                    if (nLength > 10000)
                        throw new Exception("Probably bad read!");
                    val = Encoding.ASCII.GetString(ar.Read(nLength), 0, nLength - 1);
                }
                else if (nLength < 0)
                {
                    nLength = -nLength * 2;
                    if (nLength > 20000)
                        throw new Exception("Probably bad read!");
                    val = Encoding.Unicode.GetString(ar.Read(nLength), 0, nLength - 2);
                }
            }
            else if (ar.IsWriting())
            {
                if (val.Length == 0)
                {
                    Int32 nLength = 0;
                    nLength.Serialize(ar);
                }
                Boolean bNeedUnicode = false;
                if (ar.ForceUnicode())
                {
                    bNeedUnicode = true;
                }
                else
                {
                    for (int i = 0; i < val.Length; i++)
                    {
                        if (!('\u0000' <= val[i] && val[i] <= '\u00FF'))
                        {
                            bNeedUnicode = true;
                            break;
                        }
                    }
                }
                if (bNeedUnicode)
                {
                    Int32 nLength = -(val.Length + 1);
                    nLength.Serialize(ar);
                    ar.Write(Encoding.Unicode.GetBytes(val));
                }
                else
                {
                    Int32 nLength = val.Length + 1;
                    nLength.Serialize(ar);
                    ar.Write(Encoding.ASCII.GetBytes(val));
                }
            }
        }

        public static void Serialize<T>(this List<T> val, FArchive ar)
        {
            MethodInfo info = typeof(T).Module.GetMethod("Serialize");
            if (info == null)
                throw new Exception("Serialize method not found!");
            Int32 nCount = val.Count;
            nCount.Serialize(ar);
            if (ar.IsReading())
                val.Capacity = nCount;
            for (int i = 0; i < nCount; i++)
                info.Invoke(val[i], new object[1] { ar });
        }

        public static void Serialize(this Int64 val, FArchive ar)
        {
            if (ar.IsReading())
                val = BitConverter.ToInt64(ar.Read(8), 0);
            else if (ar.IsWriting())
                ar.Write(BitConverter.GetBytes(val));
        }
    }

    public class FArchive
    {
        public enum Type
        {
            Read,
            Write
        }

        private Type m_type;
        private Stream m_stream;
        private Boolean m_bForceUnicode;
        private Dictionary<String, Int64> m_mapPositions;

        public FArchive(Stream stream, Type type)
        {
            m_type = type;
            m_stream = stream;
            m_bForceUnicode = false;
            m_mapPositions = new Dictionary<String, Int64>();
        }

        public bool IsReading()
        {
            return m_type == Type.Read;
        }

        public bool IsWriting()
        {
            return m_type == Type.Write;
        }

        public byte[] Read(int nCount)
        {
            byte[] buf = new byte[nCount];
            m_stream.Read(buf, 0, nCount);
            return buf;
        }

        public void Write(byte[] buf)
        {
            m_stream.Write(buf, 0, buf.Length);
        }

        public Boolean ForceUnicode()
        {
            return m_bForceUnicode;
        }

        public void SetForceUnicode()
        {
            m_bForceUnicode = true;
        }

        public void DropForceUnicode()
        {
            m_bForceUnicode = false;
        }

        public void SavePosition(String name)
        {
            m_mapPositions[name] = m_stream.Position;
        }

        public void WriteToPosition<T>(String name, T val)
        {
            if (!m_mapPositions.ContainsKey(name))
                throw new Exception("Position not found!");
            MethodInfo info = typeof(T).Module.GetMethod("Serialize");
            if (info == null)
                throw new Exception("Serialize method not found!");
            Int64 nCurrentPosition = m_stream.Position;
            m_stream.Position = m_mapPositions[name];
            info.Invoke(val, new object[1] { this });
            m_stream.Position = nCurrentPosition;
        }

        public void Seek(Int64 nPosition)
        {
            m_stream.Position = nPosition;
        }

        public Int64 Position()
        {
            return m_stream.Position;
        }
    }
}
