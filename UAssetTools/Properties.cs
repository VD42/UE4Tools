using System;
using System.Collections.Generic;
using Helpers;
using System.IO;

namespace UAssetTools
{
    public class PropertyTag : BinaryHelper
    {
        public Name Name;
        public Name Type;
        public Int32 Size;
        public Int32 ArrayIndex;
        public Name StructName;
        public Guid StructGuid;
        public byte BoolVal;
        public Name EnumName;
        public Name InnerType;

        public Int32 ClassIndex;

        public Int64 SizeOffset;

        public PropertyTag()
        {
            Name = new Name();
            Type = new Name();
            StructName = new Name();
            EnumName = new Name();
            InnerType = new Name();
        }

        public void DeSerialize(Stream fs)
        {
            Name.DeSerialize(fs);
            string sName = PackageReader.NameMap[Name.ComparisonIndex];
            if (sName == "None")
                return;
            Type.DeSerialize(fs);
            string sType = PackageReader.NameMap[Type.ComparisonIndex];
            Size = ReadInt32(fs);
            ArrayIndex = ReadInt32(fs);
            if (sType == "StructProperty")
            {
                StructName.DeSerialize(fs);
                if (FileSummary.FileVersionUE4 < 441)
                    throw new Exception("This version not supported!");
                StructGuid = ReadGuid(fs);
            }
            else if (sType == "BoolProperty")
            {
                BoolVal = ReadByte(fs);
            }
            else if (sType == "ByteProperty")
            {
                EnumName.DeSerialize(fs);
            }
            else if (sType == "ArrayProperty")
            {
                if (FileSummary.FileVersionUE4 < 282)
                    throw new Exception("This version not supported!");
                InnerType.DeSerialize(fs);
            }
        }

        public void Serialize(Stream fs)
        {
            Name.Serialize(fs);
            string sName = PackageReader.NameMap[Name.ComparisonIndex];
            if (sName == "None")
                return;
            Type.Serialize(fs);
            string sType = PackageReader.NameMap[Type.ComparisonIndex];
            SizeOffset = fs.Position;  WriteInt32(fs, Size); // POST: Size (maybe not change)
            WriteInt32(fs, ArrayIndex);
            if (sType == "StructProperty")
            {
                StructName.Serialize(fs);
                WriteGuid(fs, StructGuid);
            }
            else if (sType == "BoolProperty")
            {
                WriteByte(fs, BoolVal);
            }
            else if (sType == "ByteProperty")
            {
                EnumName.Serialize(fs);
            }
            else if (sType == "ArrayProperty")
            {
                InnerType.Serialize(fs);
            }
        }
    }

    public class StructProperty : BinaryHelper
    {
        public List<KeyValuePair<PropertyTag, object>> Properties;

        public StructProperty()
        {
            Properties = new List<KeyValuePair<PropertyTag, object>>();
        }

        public void DeSerialize(Stream fs)
        {
            while (true)
            {
                PropertyTag Tag = new PropertyTag();
                Tag.DeSerialize(fs);
                string sName = PackageReader.NameMap[Tag.Name.ComparisonIndex];
                if (sName == "None")
                {
                    Properties.Add(new KeyValuePair<PropertyTag, object>(Tag, null));
                    return;
                }
                string sType = PackageReader.NameMap[Tag.Type.ComparisonIndex];
                switch (sType)
                {
                    case "BoolProperty":
                        Properties.Add(new KeyValuePair<PropertyTag, object>(Tag, Tag.BoolVal));
                        break;
                    case "IntProperty":
                        Properties.Add(new KeyValuePair<PropertyTag, object>(Tag, ReadInt32(fs)));
                        break;
                    case "TextProperty":
                        Text t = new Text();
                        t.DeSerialize(fs);
                        Properties.Add(new KeyValuePair<PropertyTag, object>(Tag, t));
                        break;
                    case "FloatProperty":
                        Properties.Add(new KeyValuePair<PropertyTag, object>(Tag, ReadFloat(fs)));
                        break;
                    case "ArrayProperty":
                        int nElementCount = ReadInt32(fs);
                        string sInnerType = PackageReader.NameMap[Tag.InnerType.ComparisonIndex];
                        switch (sInnerType)
                        {
                            case "StructProperty":
                                List<StructProperty> sps = new List<StructProperty>();
                                for (int i = 0; i < nElementCount; i++)
                                {
                                    sps.Add(new StructProperty());
                                    sps[i].DeSerialize(fs);
                                }
                                Properties.Add(new KeyValuePair<PropertyTag, object>(Tag, sps));
                                break;
                            case "TextProperty":
                                List<Text> tps = new List<Text>();
                                for (int i = 0; i < nElementCount; i++)
                                {
                                    tps.Add(new Text());
                                    tps[i].DeSerialize(fs);
                                }
                                Properties.Add(new KeyValuePair<PropertyTag, object>(Tag, tps));
                                break;
                            default:
                                throw new Exception("Unknown type!");
                        }
                        break;
                    case "StructProperty":
                        string sStructName = PackageReader.NameMap[Tag.StructName.ComparisonIndex];
                        switch (sStructName)
                        {
                            case "IntPoint":
                                IntPoint ip = new IntPoint();
                                ip.DeSerialize(fs);
                                Properties.Add(new KeyValuePair<PropertyTag, object>(Tag, ip));
                                break;
                            case "CompositeFont":
                                CompositeFont cf = new CompositeFont();
                                cf.DeSerialize(fs);
                                Properties.Add(new KeyValuePair<PropertyTag, object>(Tag, cf));
                                break;
                            case "Typeface":
                                Typeface tf = new Typeface();
                                tf.DeSerialize(fs);
                                Properties.Add(new KeyValuePair<PropertyTag, object>(Tag, tf));
                                break;
                            case "FontData":
                                FontData fd = new FontData();
                                fd.DeSerialize(fs);
                                Properties.Add(new KeyValuePair<PropertyTag, object>(Tag, fd));
                                break;
                            default:
                                throw new Exception("Unknown type!");
                        }
                        break;
                    case "ObjectProperty":
                        Properties.Add(new KeyValuePair<PropertyTag, object>(Tag, ReadInt32(fs))); // ??? ClassName 
                        break;
                    case "ByteProperty":
                        string sEnumName = PackageReader.NameMap[Tag.EnumName.ComparisonIndex];
                        if (sEnumName != "None")
                        {
                            Name EnumNameValue = new Name();
                            EnumNameValue.DeSerialize(fs);
                            string sEnumNameValue = PackageReader.NameMap[EnumNameValue.ComparisonIndex];
                            Properties.Add(new KeyValuePair<PropertyTag, object>(Tag, EnumNameValue));
                        }
                        else
                        {
                            Properties.Add(new KeyValuePair<PropertyTag, object>(Tag, ReadByte(fs)));
                        }
                        break;
                    case "NameProperty":
                        Name n = new Name();
                        n.DeSerialize(fs);
                        Properties.Add(new KeyValuePair<PropertyTag, object>(Tag, n));
                        break;
                    case "StrProperty":
                        Properties.Add(new KeyValuePair<PropertyTag, object>(Tag, ReadString(fs)));
                        break;
                    default:
                        throw new Exception("Unknown type!");
                }
            }
        }
        public void Serialize(Stream fs)
        {
            Int64 nCurrentPosition;
            Int64 nStartPosition;
            for (int i = 0; i < Properties.Count; i++)
            {
                string sName = PackageReader.NameMap[Properties[i].Key.Name.ComparisonIndex];
                if (sName == "None")
                {
                    Properties[i].Key.Serialize(fs);
                    return;
                }
                string sType = PackageReader.NameMap[Properties[i].Key.Type.ComparisonIndex];
                switch (sType)
                {
                    case "BoolProperty":
                        Properties[i].Key.BoolVal = (byte)Properties[i].Value;
                        Properties[i].Key.Serialize(fs);
                        break;
                    case "IntProperty":
                        Properties[i].Key.Serialize(fs);
                        WriteInt32(fs, (Int32)Properties[i].Value);
                        break;
                    case "TextProperty":
                        Properties[i].Key.Serialize(fs);
                        nStartPosition = fs.Position;
                        ((Text)Properties[i].Value).Serialize(fs);
                        nCurrentPosition = fs.Position;
                        fs.Seek(Properties[i].Key.SizeOffset, SeekOrigin.Begin);
                        WriteInt32(fs, (Int32)(nCurrentPosition - nStartPosition));
                        fs.Seek(nCurrentPosition, SeekOrigin.Begin);
                        break;
                    case "FloatProperty":
                        Properties[i].Key.Serialize(fs);
                        WriteFloat(fs, (float)Properties[i].Value);
                        break;
                    case "ArrayProperty":
                        Properties[i].Key.Serialize(fs);
                        string sInnerType = PackageReader.NameMap[Properties[i].Key.InnerType.ComparisonIndex];
                        switch (sInnerType)
                        {
                            case "StructProperty":
                                nStartPosition = fs.Position;
                                WriteInt32(fs, ((List<StructProperty>)Properties[i].Value).Count);
                                for (int j = 0; j < ((List<StructProperty>)Properties[i].Value).Count; j++)
                                    ((List<StructProperty>)Properties[i].Value)[j].Serialize(fs);
                                nCurrentPosition = fs.Position;
                                fs.Seek(Properties[i].Key.SizeOffset, SeekOrigin.Begin);
                                WriteInt32(fs, (Int32)(nCurrentPosition - nStartPosition));
                                fs.Seek(nCurrentPosition, SeekOrigin.Begin);
                                break;
                            case "TextProperty":
                                nStartPosition = fs.Position;
                                WriteInt32(fs, ((List<Text>)Properties[i].Value).Count);
                                for (int j = 0; j < ((List<Text>)Properties[i].Value).Count; j++)
                                    ((List<Text>)Properties[i].Value)[j].Serialize(fs);
                                nCurrentPosition = fs.Position;
                                fs.Seek(Properties[i].Key.SizeOffset, SeekOrigin.Begin);
                                WriteInt32(fs, (Int32)(nCurrentPosition - nStartPosition));
                                fs.Seek(nCurrentPosition, SeekOrigin.Begin);
                                break;
                            default:
                                throw new Exception("Unknown type!");
                        }
                        break;
                    case "StructProperty":
                        Properties[i].Key.Serialize(fs);
                        string sStructName = PackageReader.NameMap[Properties[i].Key.StructName.ComparisonIndex];
                        switch (sStructName)
                        {
                            case "IntPoint":
                                nStartPosition = fs.Position;
                                ((IntPoint)Properties[i].Value).Serialize(fs);
                                nCurrentPosition = fs.Position;
                                fs.Seek(Properties[i].Key.SizeOffset, SeekOrigin.Begin);
                                WriteInt32(fs, (Int32)(nCurrentPosition - nStartPosition));
                                fs.Seek(nCurrentPosition, SeekOrigin.Begin);
                                break;
                            case "CompositeFont":
                                nStartPosition = fs.Position;
                                ((CompositeFont)Properties[i].Value).Serialize(fs);
                                nCurrentPosition = fs.Position;
                                fs.Seek(Properties[i].Key.SizeOffset, SeekOrigin.Begin);
                                WriteInt32(fs, (Int32)(nCurrentPosition - nStartPosition));
                                fs.Seek(nCurrentPosition, SeekOrigin.Begin);
                                break;
                            case "Typeface":
                                nStartPosition = fs.Position;
                                ((Typeface)Properties[i].Value).Serialize(fs);
                                nCurrentPosition = fs.Position;
                                fs.Seek(Properties[i].Key.SizeOffset, SeekOrigin.Begin);
                                WriteInt32(fs, (Int32)(nCurrentPosition - nStartPosition));
                                fs.Seek(nCurrentPosition, SeekOrigin.Begin);
                                break;
                            case "FontData":
                                nStartPosition = fs.Position;
                                ((FontData)Properties[i].Value).Serialize(fs);
                                nCurrentPosition = fs.Position;
                                fs.Seek(Properties[i].Key.SizeOffset, SeekOrigin.Begin);
                                WriteInt32(fs, (Int32)(nCurrentPosition - nStartPosition));
                                fs.Seek(nCurrentPosition, SeekOrigin.Begin);
                                break;
                            default:
                                throw new Exception("Unknown type!");
                        }
                        break;
                    case "ObjectProperty":
                        Properties[i].Key.Serialize(fs);
                        WriteInt32(fs, (Int32)Properties[i].Value); // ??? ClassName
                        break;
                    case "ByteProperty":
                        Properties[i].Key.Serialize(fs);
                        string sEnumName = PackageReader.NameMap[Properties[i].Key.EnumName.ComparisonIndex];
                        if (sEnumName != "None")
                        {
                            nStartPosition = fs.Position;
                            ((Name)Properties[i].Value).Serialize(fs);
                            nCurrentPosition = fs.Position;
                            fs.Seek(Properties[i].Key.SizeOffset, SeekOrigin.Begin);
                            WriteInt32(fs, (Int32)(nCurrentPosition - nStartPosition));
                            fs.Seek(nCurrentPosition, SeekOrigin.Begin);
                        }
                        else
                        {
                            nStartPosition = fs.Position;
                            WriteByte(fs, (byte)Properties[i].Value);
                            nCurrentPosition = fs.Position;
                            fs.Seek(Properties[i].Key.SizeOffset, SeekOrigin.Begin);
                            WriteInt32(fs, (Int32)(nCurrentPosition - nStartPosition));
                            fs.Seek(nCurrentPosition, SeekOrigin.Begin);
                        }
                        break;
                    case "NameProperty":
                        Properties[i].Key.Serialize(fs);
                        ((Name)Properties[i].Value).Serialize(fs);
                        break;
                    case "StrProperty":
                        Properties[i].Key.Serialize(fs);
                        nStartPosition = fs.Position;
                        WriteString(fs, (string)Properties[i].Value);
                        nCurrentPosition = fs.Position;
                        fs.Seek(Properties[i].Key.SizeOffset, SeekOrigin.Begin);
                        WriteInt32(fs, (Int32)(nCurrentPosition - nStartPosition));
                        fs.Seek(nCurrentPosition, SeekOrigin.Begin);
                        break;
                    default:
                        throw new Exception("Unknown type!");
                }
            }
        }
    }
}
