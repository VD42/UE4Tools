﻿using System;
using System.Collections.Generic;
using Helpers;
using System.IO;
using System.Security.Cryptography;

namespace PakTools
{
    public class PakInfo : BinaryHelper
    {
        public UInt32 Magic;
        public Int32 Version;
        public Int64 IndexOffset;
        public Int64 IndexSize;
        public byte[] IndexHash;

        public PakInfo()
        {
            IndexHash = new byte[20];
        }

        public void DeSerialize(Stream fs)
        {
            Magic = ReadUInt32(fs);
            if (Magic != 0x5A6F12E1)
                throw new Exception("Bad signature!");
            Version = ReadInt32(fs);
            IndexOffset = ReadInt64(fs);
            IndexSize = ReadInt64(fs);
            fs.Read(IndexHash, 0, 20);
        }

        public void Serialize(Stream fs)
        {
            WriteUInt32(fs, Magic);
            WriteInt32(fs, Version);
            WriteInt64(fs, IndexOffset);
            WriteInt64(fs, IndexSize);
            fs.Write(IndexHash, 0, 20);
        }

        public Int64 GetSerializedSize()
        {
            return 4 + 4 + 8 + 8 + 20;
        }
}

    public class PakCompressedBlock : BinaryHelper
    {
        public Int64 CompressedStart;
        public Int64 CompressedEnd;

        public void DeSerialize(Stream fs)
        {
            CompressedStart = ReadInt64(fs);
            CompressedEnd = ReadInt64(fs);
        }

        public void Serialize(Stream fs)
        {
            WriteInt64(fs, CompressedStart);
            WriteInt64(fs, CompressedEnd);
        }
    }

    public class PakEntry : BinaryHelper
    {
        public Int64 Offset;
        public Int64 Size;
        public Int64 UncompressedSize;
        public Int32 CompressionMethod;
        public byte[] Hash;
        public List<PakCompressedBlock> CompressionBlocks;
        public byte bEncrypted;
        public Int32 CompressionBlockSize;

        public PakEntry()
        {
            Hash = new byte[20];
            CompressionBlocks = new List<PakCompressedBlock>();
        }

        public void DeSerialize(Stream fs)
        {
            Offset = ReadInt64(fs);
            Size = ReadInt64(fs);
            UncompressedSize = ReadInt64(fs);
            CompressionMethod = ReadInt32(fs);
            fs.Read(Hash, 0, 20);
            if (CompressionMethod > 0)
            {
                int BlockCount = ReadInt32(fs);
                for (int i = 0; i < BlockCount; i++)
                {
                    CompressionBlocks.Add(new PakCompressedBlock());
                    CompressionBlocks[i].DeSerialize(fs);
                }
            }
            bEncrypted = ReadByte(fs);
            if (bEncrypted != 0)
                throw new Exception("Encription not supported!");
            CompressionBlockSize = ReadInt32(fs);
        }

        public void Serialize(Stream fs)
        {
            WriteInt64(fs, Offset);
            WriteInt64(fs, Size);
            WriteInt64(fs, UncompressedSize);
            WriteInt32(fs, CompressionMethod);
            fs.Write(Hash, 0, 20);
            if (CompressionMethod > 0)
                throw new Exception("Compression not supported!");
            WriteByte(fs, bEncrypted);
            if (bEncrypted != 0)
                throw new Exception("Encription not supported!");
            WriteInt32(fs, CompressionBlockSize);
        }
    }

    public class PakFile : BinaryHelper
    {
        public PakInfo Info;
        public string MountPoint;
        public Int32 NumEntries;

        public Int64 IndexOffsetValue;

        public PakFile()
        {
            Info = new PakInfo();
        }

        public void DeSerialize(Stream fs)
        {
            fs.Seek(-Info.GetSerializedSize(), SeekOrigin.End);
            Info.DeSerialize(fs);
            if (Info.Version != 3)
                throw new Exception("Other versions not supported!");
            LoadIndex(fs);
        }

        public void Serialize(Stream fs)
        {
            MountPoint = Program.OutPrefix;
            SaveIndex(fs);
            Info.IndexOffset = IndexOffsetValue;
            Info.IndexSize = fs.Position - IndexOffsetValue;
            Info.Version = 3;
            Info.Magic = 0x5A6F12E1;
            byte[] Data = new byte[Info.IndexSize];
            fs.Seek(Info.IndexOffset, SeekOrigin.Begin);
            fs.Read(Data, 0, (int)Info.IndexSize);
            SHA1 sha = new SHA1CryptoServiceProvider();
            Info.IndexHash = sha.ComputeHash(Data);
            Info.Serialize(fs);
        }

        public void CreatePath(string FilePath)
        {
            string DirPath = Path.GetDirectoryName(FilePath);
            if (!Directory.Exists(DirPath))
            {
                CreatePath(DirPath);
                Directory.CreateDirectory(DirPath);
            }
        }

        public void LoadIndex(Stream fs)
        {
            Int64 nIndexPosition = Info.IndexOffset;
            fs.Seek(nIndexPosition, SeekOrigin.Begin);
            MountPoint = ReadString(fs);
            NumEntries = ReadInt32(fs);
            for (int i = 0; i < NumEntries; i++)
            {
                string Filename = ReadString(fs);

                Console.WriteLine("[" + (i + 1) + " of " + NumEntries + "] " + Filename);

                PakEntry Entry = new PakEntry();
                Entry.DeSerialize(fs);
                nIndexPosition = fs.Position;

                if (Entry.bEncrypted != 0)
                    throw new Exception("Encription not supported!");

                fs.Seek(Entry.Offset, SeekOrigin.Begin);
                PakEntry FileHeader = new PakEntry();
                FileHeader.DeSerialize(fs);
                
                if (!System.Collections.StructuralComparisons.StructuralEqualityComparer.Equals(Entry.Hash, FileHeader.Hash))
                    throw new Exception("Wrong hash!");

                string FilePath = Path.Combine(Program.OutPath, Filename.Replace('/', '\\'));
                CreatePath(FilePath);
                Stream fs_out = new FileStream(FilePath, FileMode.Create);

                if (Entry.CompressionMethod == 0)
                {
                    byte[] Data = new byte[Entry.Size];
                    fs.Read(Data, 0, Data.Length);
                    fs_out.Write(Data, 0, Data.Length);
                }
                else if (Entry.CompressionMethod == 1)
                {
                    for (int j = 0; j < Entry.CompressionBlocks.Count; j++)
                    {
                        fs.Seek(Entry.CompressionBlocks[j].CompressedStart + 2, SeekOrigin.Begin);
                        System.IO.Compression.DeflateStream fs_decompressed = new System.IO.Compression.DeflateStream(fs, System.IO.Compression.CompressionMode.Decompress, true);
                        fs_decompressed.CopyTo(fs_out);
                        fs_decompressed.Close();
                    }
                }
                else
                {
                    throw new Exception("This compression method not supported!");
                }

                fs_out.Close();
                fs.Seek(nIndexPosition, SeekOrigin.Begin);
            }
        }

        public void SaveIndex(Stream fs)
        {
            List<string> files = new List<string>();
            Program.GetFiles(Program.OutPath, ref files);
            List<PakEntry> info = new List<PakEntry>();
            for (int i = 0; i < files.Count; i++)
            {
                Stream fs_in = new FileStream(files[i], FileMode.Open);
                byte[] Data = new byte[fs_in.Length];
                fs_in.Read(Data, 0, Data.Length);
                fs_in.Close();

                Int64 Offset = fs.Position;
                PakEntry FileHeader = new PakEntry();
                SHA1 sha = new SHA1CryptoServiceProvider();
                FileHeader.Hash = sha.ComputeHash(Data);
                FileHeader.Size = Data.Length;
                FileHeader.UncompressedSize = Data.Length;
                FileHeader.Serialize(fs);

                fs.Write(Data, 0, Data.Length);

                FileHeader.Offset = Offset;
                info.Add(FileHeader);
            }

            IndexOffsetValue = fs.Position;

            WriteString(fs, MountPoint);
            WriteInt32(fs, files.Count);

            for (int i = 0; i < files.Count; i++)
            {
                WriteString(fs, files[i].Replace(Program.OutPath + "\\", "").Replace('\\', '/'));
                info[i].Serialize(fs);
            }
        }
    }
}
