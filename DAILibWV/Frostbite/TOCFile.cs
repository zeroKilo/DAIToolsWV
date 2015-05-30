using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAILibWV.Frostbite
{
    public class TOCFile
    {
        public struct TOCBundleInfoStruct
        {
            public string id; //path
            public int offset;
            public int size;
            public bool isdelta;
            public bool isbase;
        }
        public struct TOCChunkInfoStruct
        {
            public byte[] id;
            public byte[] sha1;
            public int offset;
            public int size;
        }
        public string MyPath;
        public List<BJSON.Entry> lines;
        public List<TOCBundleInfoStruct> bundles;
        public List<TOCChunkInfoStruct> chunks;
        public bool iscas;
        private int magic;
        private byte[] serial;
        private byte[] xorKey;
        private MemoryStream unxoredStream;

        public TOCFile(string path)
        {
            Load(path);
        }

        public void Load(string path)
        {
            MyPath = path;
            xorKey = new byte[257];
            ReadFile();
            ProcessFile();
        }

        public void Save()
        {
            Save(MyPath);
        }

        public void Save(string path)
        {
            MyPath = path;
            WriteFile();
        }

        public byte[] ExportBinaryBundle(TOCBundleInfoStruct info)
        {
            if (info.isbase)
                return new byte[0];
            string sbpath = Helpers.GetFileNameWithOutExtension(MyPath) + ".sb";
            FileStream fs = new FileStream(sbpath, FileMode.Open, FileAccess.Read);
            fs.Seek(0, SeekOrigin.End);
            if(info.offset+info.size > fs.Position)
                return new byte[0];
            fs.Seek(info.offset, 0);
            byte[] buff = new byte[info.size];
            fs.Read(buff, 0, info.size);
            fs.Close();
            uint magic = BitConverter.ToUInt32(buff, 4);
            if (magic != 0xd58e799d)
                return new byte[0];
            return buff;
        }

        public byte[] ExportBundleDataByPath(string path)
        {
            TOCBundleInfoStruct info = new TOCBundleInfoStruct();
            info.isbase = true;
            foreach (TOCBundleInfoStruct inf in bundles)
                if (inf.id.ToLower() == path.ToLower())
                {
                    info = inf;
                    break;
                }
            if (info.isbase)
                return new byte[0];
            string sbpath = Helpers.GetFileNameWithOutExtension(MyPath) + ".sb";
            FileStream fs = new FileStream(sbpath, FileMode.Open, FileAccess.Read);
            fs.Seek(0, SeekOrigin.End);
            if (info.offset + info.size > fs.Position)
                return new byte[0];
            fs.Seek(info.offset, 0);
            byte[] buff = new byte[info.size];
            fs.Read(buff, 0, info.size);
            fs.Close();
            return buff;
        }


        private void ReadFile()
        {
            using (FileStream fs = new FileStream(MyPath, FileMode.Open, FileAccess.Read))
            {
                magic = Helpers.ReadInt(fs);
                if (magic != 0x03CED100 && magic != 0x01CED100)
                    return;
                byte b = (byte)fs.ReadByte();
                while (b == 0)
                    b = (byte)fs.ReadByte();
                MemoryStream m = new MemoryStream();
                m.WriteByte(b);
                for (int i = 0; i < 0x101; i++)
                    m.WriteByte((byte)fs.ReadByte());
                serial = m.ToArray();

                if (magic == 0x03CED100)
                {
                    fs.Seek(0x22C, 0);
                    lines = new List<BJSON.Entry>();
                    BJSON.ReadEntries(fs, lines);
                }
                else if (magic == 0x01CED100)
                {
                    fs.Seek(0x128, SeekOrigin.Begin);
                    fs.Read(xorKey, 0, 257);
                    fs.Seek(3, SeekOrigin.Current);
                    unxoredStream = new MemoryStream();
                    BuildUnxoredStream(fs, unxoredStream);
                    unxoredStream.Seek(0, SeekOrigin.Begin);
                    Helpers.ReadUInt(unxoredStream);
                    lines = new List<BJSON.Entry>();
                    BJSON.ReadEntries(unxoredStream, lines);
                }
            }
        }

        private void ProcessFile()
        {

            BJSON.Entry root = lines[0];
            bundles = new List<TOCBundleInfoStruct>();
            chunks = new List<TOCChunkInfoStruct>();
            foreach (BJSON.Field f in root.fields)
                switch (f.fieldname)
                {
                    case "bundles":
                        ProcessBundles(f);
                        break;
                    case "chunks":
                        ProcessChunks(f);
                        break;
                    case "cas":
                        iscas = (bool)f.data;
                        break;
                }
        }

        private void ProcessBundles(BJSON.Field f)
        {
            List<BJSON.Entry> list = (List<BJSON.Entry>)f.data;
            foreach (BJSON.Entry e in list)
            {
                TOCBundleInfoStruct info = new TOCBundleInfoStruct();
                info.isdelta = false;
                foreach(BJSON.Field f2 in e.fields)
                    switch (f2.fieldname)
                    {
                        case "id":
                            info.id = (string)f2.data;
                            break;
                        case "offset":
                            info.offset = Helpers.ReadInt(new MemoryStream((byte[])f2.data));
                            break;
                        case "size":
                            info.size = Helpers.ReadInt(new MemoryStream((byte[])f2.data));
                            break;
                        case "delta":
                            info.isdelta = (bool)f2.data;
                            break;
                        case "base":
                            info.isbase = (bool)f2.data;
                            break;
                    }
                bundles.Add(info);
            }
        }

        private void ProcessChunks(BJSON.Field f)
        {
            List<BJSON.Entry> list = (List<BJSON.Entry>)f.data;
            foreach (BJSON.Entry e in list)
            {
                TOCChunkInfoStruct info = new TOCChunkInfoStruct();
                foreach(BJSON.Field f2 in e.fields)
                    switch (f2.fieldname)
                    {
                        case "id":
                            info.id = (byte[])f2.data;
                            break;
                        case "sha1":
                            info.sha1 = (byte[])f2.data;
                            break;
                        case "offset":
                            info.offset = Helpers.ReadInt(new MemoryStream((byte[])f2.data));
                            break;
                        case "size":
                            info.size = Helpers.ReadInt(new MemoryStream((byte[])f2.data));
                            break;
                    }
                chunks.Add(info);
            }
        }
    
        private void WriteFile()
        {
            using (FileStream fs = new FileStream(MyPath, FileMode.Create, FileAccess.Write))
            {
                Helpers.WriteInt(fs, magic);
                Helpers.WriteInt(fs, 0);
                fs.Write(serial, 0, serial.Length);
                if (magic == 0x03CED100)
                {
                    byte[] buff = new byte[0x122];
                    fs.Write(buff, 0, 0x122);
                    foreach (BJSON.Entry e in lines)
                        BJSON.WriteEntry(fs, e);
                }
                else if (magic == 0x01CED100)
                {
                    byte[] buff = new byte[0x1E];
                    fs.Write(buff, 0, 0x1E);
                    fs.Write(xorKey, 0, 0x101);
                    fs.WriteByte(0);
                    fs.WriteByte(0);
                    fs.WriteByte(0);
                    MemoryStream m = new MemoryStream();
                    MemoryStream m2 = new MemoryStream();
                    foreach (BJSON.Entry e in lines)
                        BJSON.WriteEntry(m, e);
                    m.WriteByte(0);
                    m2.WriteByte(0x81);
                    Helpers.WriteLEB128(m2, (int)m.Length);
                    m2.Write(m.ToArray(), 0, (int)m.Length);
                    m2.Seek(0, 0);
                    for (int i = 0; i < m2.Length; i++)
                    {
                        byte k = xorKey[i % 0x101];
                        byte b = (byte)m2.ReadByte();
                        fs.WriteByte((byte)(0x7B ^ k ^ b));
                    }
                }
            }
        }

        private void BuildUnxoredStream(FileStream fs, MemoryStream unxoredStream)
        {
            byte[] nextBytes = new byte[257];
            int lengthRead = 0;
            while ((lengthRead = fs.Read(nextBytes, 0, 257)) > 0)
                for (int byteCount = 0; byteCount < lengthRead; byteCount++)
                {
                    byte unxorByte = (byte)(nextBytes[byteCount] ^ xorKey[byteCount] ^ 0x7b);
                    unxoredStream.WriteByte(unxorByte);
                }
        }
    }
}
