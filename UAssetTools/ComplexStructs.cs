using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UAssetTools
{
    public class CompositeFont : StructProperty
    {
        public void DeSerialize(Stream fs)
        {
            base.DeSerialize(fs);
        }
    }

    public class Typeface : StructProperty
    {
        public void DeSerialize(Stream fs)
        {
            base.DeSerialize(fs);
        }
    }

    public class FontData : StructProperty
    {
        public void DeSerialize(Stream fs)
        {
            base.DeSerialize(fs);
        }
    }

    public class FontImportOptionsData : StructProperty
    {
        public void DeSerialize(Stream fs)
        {
            base.DeSerialize(fs);
        }
    }

    public class FontCharacter : StructProperty
    {
        public Int32 StartU;
        public Int32 StartV;
        public Int32 USize;
        public Int32 VSize;
        public byte TextureIndex;
        public Int32 VerticalOffset;

        public void DeSerialize(Stream fs)
        {
            StartU = ReadInt32(fs);
            StartV = ReadInt32(fs);
            USize = ReadInt32(fs);
            VSize = ReadInt32(fs);
            TextureIndex = ReadByte(fs);
            VerticalOffset = ReadInt32(fs);
        }

        public void Serialize(Stream fs)
        {
            WriteInt32(fs, StartU);
            WriteInt32(fs, StartV);
            WriteInt32(fs, USize);
            WriteInt32(fs, VSize);
            WriteByte(fs, TextureIndex);
            WriteInt32(fs, VerticalOffset);
        }
    }
}
