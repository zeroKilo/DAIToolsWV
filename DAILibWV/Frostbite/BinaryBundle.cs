using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAILibWV.Frostbite
{
    public class BinaryBundle
    {
        public struct HeaderStruct
        {
            public uint magic;
            public uint totalCount;
            public uint ebxCount;
            public uint resCount;
            public uint chunkCount;
            public uint stringOffset;
            public uint chunkMetaOffset;
            public uint chunkMetaSize;
        }

        public struct EbxEntry
        {
            public int nameOffset;
            public int ucsize;
            public string _name;
            public byte[] _data;
            public byte[] _sha1;
        }

        public struct ResEntry
        {
            public int nameOffset;
            public int ucsize;
            public int type;
            public byte[] meta;
            public byte[] id;
            public string _name;
            public byte[] _data;
            public byte[] _sha1;
        }

        public struct ChunkEntry
        {
            public byte[] id;
            public ushort rangeStart;
            public ushort logicalSize;
            public int logicalOffset;
            public int _originalSize;
            public byte[] _data;
            public byte[] _sha1;
        }

        public HeaderStruct Header;
        public List<byte[]> Sha1List;
        public List<EbxEntry> EbxList;
        public List<ResEntry> ResList;
        public List<ChunkEntry> ChunkList;
        public BJSON.Field ChunkMeta;

        public BinaryBundle(string path, int offset, int size , bool fast = false)
        {
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            fs.Seek(offset, 0);
            Load(fs, fast);
            fs.Close();
        }

        public BinaryBundle(Stream data, bool fast = false)
        {
            Load(data, fast);
        }

        public BinaryBundle()
        {
            // TODO: Complete member initialization
        }

        public void Load(Stream data, bool fast = false)
        {
            uint headersize = Helpers.ReadLEUInt(data);
            long startoffset = data.Position;
            Header = new HeaderStruct();
            Header.magic = Helpers.ReadLEUInt(data);
            Header.totalCount = Helpers.ReadLEUInt(data);
            Header.ebxCount = Helpers.ReadLEUInt(data);
            Header.resCount = Helpers.ReadLEUInt(data);
            Header.chunkCount = Helpers.ReadLEUInt(data);
            Header.stringOffset = Helpers.ReadLEUInt(data);
            Header.chunkMetaOffset = Helpers.ReadLEUInt(data);
            Header.chunkMetaSize = Helpers.ReadLEUInt(data);
            ReadSha1List(data);
            ReadEbxList(data);
            ReadResList(data);
            ReadChunkList(data);
            if (Header.chunkCount != 0)
                ChunkMeta = BJSON.ReadField(data);
            else
                ChunkMeta = null;
            data.Seek(startoffset + Header.stringOffset, 0);
            ReadEbxListNames(data);
            data.Seek(startoffset + Header.stringOffset, 0);
            ReadResListNames(data);
            data.Seek(startoffset + headersize, 0);
            ReadEbxListData(data, fast);
            ReadResListData(data, fast);
            ReadChunkListData(data, fast);
            ApplySHA1s();
        }

        private void ReadSha1List(Stream data)
        {
            Sha1List = new List<byte[]>();
            for (int i = 0; i < Header.totalCount; i++)
            {
                byte[] buff = new byte[20];
                data.Read(buff, 0, 20);
                Sha1List.Add(buff);
            }
        }

        private void ReadEbxList(Stream data)
        {
            EbxList = new List<EbxEntry>();
            for (int i = 0; i < Header.ebxCount; i++)
            {
                EbxEntry b = new EbxEntry();
                b.nameOffset = Helpers.ReadLEInt(data);
                b.ucsize = Helpers.ReadLEInt(data);
                EbxList.Add(b);
            }
        }

        private void ReadResList(Stream data)
        {
            ResList = new List<ResEntry>();
            for (int i = 0; i < Header.resCount; i++)
            {
                ResEntry b = new ResEntry();
                b.nameOffset = Helpers.ReadLEInt(data);
                b.ucsize = Helpers.ReadLEInt(data);
                ResList.Add(b);
            }
            for (int i = 0; i < Header.resCount; i++)
            {
                ResEntry b = ResList[i];
                b.type = Helpers.ReadLEInt(data);
                ResList[i] = b;
            }
            for (int i = 0; i < Header.resCount; i++)
            {
                ResEntry b = ResList[i];
                b.meta = new byte[16];
                data.Read(b.meta, 0, 16);
                ResList[i] = b;
            }
            for (int i = 0; i < Header.resCount; i++)
            {
                ResEntry b = ResList[i];
                b.id = new byte[8];
                data.Read(b.id, 0, 8);
                ResList[i] = b;
            }
        }

        private void ReadChunkList(Stream data)
        {
            ChunkList = new List<ChunkEntry>();
            for (int i = 0; i < Header.chunkCount; i++)
            {
                ChunkEntry e = new ChunkEntry();
                e.id = new byte[16];
                data.Read(e.id, 0, 16);
                e.rangeStart = Helpers.ReadLEUShort(data);
                e.logicalSize = Helpers.ReadLEUShort(data);
                e.logicalOffset = Helpers.ReadLEInt(data);
                e._originalSize = e.logicalOffset + e.logicalSize;
                ChunkList.Add(e);
            }
        }

        private void ReadEbxListNames(Stream data)
        {
            long startOffset = data.Position;
            for (int i = 0; i < Header.ebxCount; i++)
            {
                EbxEntry e = EbxList[i];
                data.Seek(startOffset + e.nameOffset, 0);
                e._name = Helpers.ReadNullString(data);
                EbxList[i] = e;
            }
        }

        private void ReadResListNames(Stream data)
        {
            long startOffset = data.Position;
            for (int i = 0; i < Header.resCount; i++)
            {
                ResEntry e = ResList[i];
                data.Seek(startOffset + e.nameOffset, 0);
                e._name = Helpers.ReadNullString(data);
                ResList[i] = e;
            }
        }

        private byte[] ReadPayload(Stream data, int size, bool fast)
        {
            MemoryStream tmp = new MemoryStream();
            int tmpcounter = 0;
            while (tmp.Length <  size && tmpcounter < size)
            {
                int ucsize = Helpers.ReadLEInt(data);
                int magic = Helpers.ReadLEUShort(data);
                int csize = Helpers.ReadLEUShort(data);
                if (!fast)
                {
                    byte[] buff = new byte[0];
                    if (magic == 0x0270)
                    {
                        buff = new byte[csize];
                        data.Read(buff, 0, csize);
                        buff = Helpers.DecompressZlib(buff, ucsize);
                    }
                    if (magic == 0x0071 || magic == 0x0070)
                    {
                        buff = new byte[ucsize];
                        data.Read(buff, 0, ucsize);
                    }
                    tmp.Write(buff, 0, buff.Length);
                }
                else
                {
                    tmpcounter += ucsize;
                    if (magic == 0x0270)
                        data.Seek(csize, SeekOrigin.Current);
                    if (magic == 0x0071 || magic == 0x0070)
                        data.Seek(ucsize, SeekOrigin.Current);
                }
            }
            return tmp.ToArray();
        }

        private void ReadEbxListData(Stream data, bool fast = false)
        {
            for (int i = 0; i < Header.ebxCount; i++)
            {
                EbxEntry e = EbxList[i];
                e._data = ReadPayload(data, e.ucsize, fast);
                EbxList[i] = e;
            }
        }

        private void ReadResListData(Stream data, bool fast = false)
        {
            for (int i = 0; i < Header.resCount; i++)
            {
                ResEntry e = ResList[i];
                e._data = ReadPayload(data, e.ucsize, fast);
                ResList[i] = e;
            }
        }

        private void ReadChunkListData(Stream data, bool fast = false)
        {
            for (int i = 0; i < Header.chunkCount; i++)
            {
                ChunkEntry e = ChunkList[i];
                e._data = ReadPayload(data, e._originalSize, fast);
                ChunkList[i] = e;
            }
        }

        private void ApplySHA1s()
        {
            int count = 0;
            foreach (byte[] sha1 in Sha1List)
            {
                if (count < Header.ebxCount)
                {
                    EbxEntry e = EbxList[count];
                    e._sha1 = sha1;
                    EbxList[count] = e;
                }
                if (count >= Header.ebxCount && count < Header.ebxCount + Header.resCount)
                {
                    ResEntry e = ResList[count - (int)Header.ebxCount];
                    e._sha1 = sha1;
                    ResList[count - (int)Header.ebxCount] = e;
                }
                if (count >= Header.ebxCount + Header.resCount)
                {
                    ChunkEntry e = ChunkList[count - (int)(Header.ebxCount + Header.resCount)];
                    e._sha1 = sha1;
                    ChunkList[count - (int)(Header.ebxCount + Header.resCount)] = e;
                }
                count++;
            }
        }
        
        public void Save(string path)
        {
            FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write);
            Save(fs);
            fs.Close();
        }

        public void Save(Stream s)
        {
            Helpers.WriteLEUInt(s, 0);
            Helpers.WriteLEUInt(s, Header.magic);
            Helpers.WriteLEInt(s, Sha1List.Count);            
            Helpers.WriteLEInt(s, EbxList.Count);
            Helpers.WriteLEInt(s, ResList.Count);
            Helpers.WriteLEInt(s, ChunkList.Count);
            Helpers.WriteLEUInt(s, 0);
            Helpers.WriteLEUInt(s, 0);
            Helpers.WriteLEUInt(s, 0);
            WriteSha1List(s);
            long ebxpos = s.Position;
            WriteEbxList(s);
            WriteResList(s);
            WriteChunkList(s);
            long ch_start = s.Position - 4;
            if (Header.chunkCount != 0)
                BJSON.WriteField(s, ChunkMeta);
            long st_start = s.Position - 4;
            WriteStringTable(s);
            long tpos = s.Position;
            //Name Fix Up
            s.Seek(ebxpos, 0);
            WriteEbxList(s);
            //Header Size Fix Up
            s.Seek(0, 0);
            Helpers.WriteLEUInt(s, (uint)tpos - 4);
            s.Seek(0x18, 0);
            Helpers.WriteLEUInt(s, (uint)st_start);//String Table Offset
            Helpers.WriteLEUInt(s, (uint)ch_start);//chunkMeta Offset
            Helpers.WriteLEUInt(s, (uint)(st_start - ch_start));//chunkMeta Size
            s.Seek(tpos, 0);
            WriteEbxListData(s);
            WriteResListData(s);
            WriteChunkListData(s);
        }

        private void WriteSha1List(Stream s)
        {
            foreach (byte[] sha1 in Sha1List)
                s.Write(sha1, 0, 0x14);
        }

        private void WriteEbxList(Stream s)
        {
            foreach (EbxEntry b in EbxList)
            {
                Helpers.WriteLEInt(s, b.nameOffset);
                Helpers.WriteLEInt(s, b._data.Length);
            }
        }

        private void WriteResList(Stream s)
        {
            foreach(ResEntry b in ResList)
            {
                Helpers.WriteLEInt(s, b.nameOffset);
                Helpers.WriteLEInt(s, b._data.Length);
            }
            foreach(ResEntry b in ResList)
                Helpers.WriteLEInt(s, b.type);
            foreach(ResEntry b in ResList)
                s.Write(b.meta, 0, b.meta.Length);
            foreach(ResEntry b in ResList)
                s.Write(b.id, 0, b.id.Length);
        }

        private void WriteChunkList(Stream s)
        {
            foreach(ChunkEntry e in ChunkList)
            {
                s.Write(e.id, 0, 16);
                Helpers.WriteLEUShort(s, 0);
                Helpers.WriteLEUShort(s, 0);
                Helpers.WriteLEInt(s, e._data.Length);
            }
        }

        private void WriteStringTable(Stream s)
        {
            List<string> list = new List<string>();
            bool found;
            foreach (EbxEntry e in EbxList)
            {
                found = false;
                foreach(string str in list)
                    if (e._name == str)
                    {
                        found = true;
                        break;
                    }
                if (!found)
                    list.Add(e._name);
            }
            foreach (ResEntry e in ResList)
            {
                found = false;
                foreach (string str in list)
                    if (e._name == str)
                    {
                        found = true;
                        break;
                    }
                if (!found)
                    list.Add(e._name);
            }
            int pos = 0;
            foreach (string str in list)
            {
                Helpers.WriteNullString(s, str);
                for (int i = 0; i < EbxList.Count; i++)
                    if (EbxList[i]._name == str)
                    {
                        EbxEntry e = EbxList[i];
                        e.nameOffset = pos;
                        EbxList[i] = e;
                    }
                for (int i = 0; i < ResList.Count; i++)
                    if (ResList[i]._name == str)
                    {
                        ResEntry e = ResList[i];
                        e.nameOffset = pos;
                        ResList[i] = e;
                    }
                pos += 1 + str.Length;
            }
            while (s.Position % 0x10 != 0)
                s.WriteByte(0);
        }

        private void WriteEbxListData(Stream s)
        {
            foreach (EbxEntry e in EbxList)
                WritePayload(s, e._data);
        }

        private void WriteResListData(Stream s)
        {
            foreach (ResEntry e in ResList)
                WritePayload(s, e._data);
        }

        private void WriteChunkListData(Stream s)
        {
            foreach (ChunkEntry e in ChunkList)
                WritePayload(s, e._data);
        }

        private void WritePayload(Stream s, byte[] data)
        {
            int tmppos = 0;
            while (tmppos < data.Length)
            {
                int size = data.Length - tmppos;
                if (size > 0x10000)
                    size = 0x10000;
                Helpers.WriteLEInt(s, size);
                Helpers.WriteLEUShort(s, 0x0270);
                MemoryStream m = new MemoryStream();
                m.Write(data, tmppos, size);
                byte[] tmp = Helpers.CompressZlib(m.ToArray());
                Helpers.WriteLEUShort(s, (ushort)tmp.Length);
                s.Write(tmp, 0, tmp.Length);
                tmppos += size;
            }
        }
    }
}
