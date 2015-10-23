using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DAILibWV.Frostbite;

namespace DAILibWV.Frostbite
{
    public class Bundle
    {
        public string path;
        public string salt;
        public List<ebxtype> ebx;
        public List<dbxtype> dbx;
        public List<restype> res;
        public List<chunktype> chunk;
        public bool align;
        public bool ridsupport;
        public bool compressed;
        public ulong totalsize;
        public ulong dbxtotalsize;
        private bool iscas = true;

        public struct ebxtype
        {
            public string name;
            public byte[] Sha1;
            public byte[] size;
            public byte[] originalSize;
            public int casPatchType;
            public byte[] baseSha1;
            public byte[] deltaSha1;
            public BJSON.Entry link;
        }

        public struct dbxtype
        {
            public string name;
            public byte[] SHA1;
            public byte[] size;
            public byte[] osize;
            public BJSON.Entry link;
        }
        public struct restype
        {
            public string name;
            public byte[] SHA1;
            public byte[] size;
            public byte[] osize;
            public byte[] rtype;
            public int casPatchType;
            public byte[] baseSha1;
            public byte[] deltaSha1;
            public BJSON.Entry link;
        }
        public struct chunktype
        {
            public byte[] id;
            public byte[] SHA1;
            public byte[] size;
            public int casPatchType;
            public byte[] baseSha1;
            public byte[] deltaSha1;
            public BJSON.Entry link;
        }

        public static Bundle Create(byte[] binary, bool fast = false)
        {
            Bundle res = new Bundle();
            res.iscas = false;
            res.ebx = new List<ebxtype>();
            res.res = new List<restype>();
            res.chunk = new List<chunktype>();
            BinaryBundle bin = new BinaryBundle(new MemoryStream(binary), fast);
            if(bin.EbxList != null)
                foreach (BinaryBundle.EbxEntry ebx in bin.EbxList)
                {
                    ebxtype e = new ebxtype();
                    e.name = ebx._name;
                    e.originalSize = BitConverter.GetBytes((long)ebx.ucsize);
                    e.Sha1 = ebx._sha1;
                    res.ebx.Add(e);
                }
            if (bin.ResList != null)
                foreach (BinaryBundle.ResEntry r in bin.ResList)
                {
                    restype e = new restype();
                    e.name = r._name;
                    e.osize = BitConverter.GetBytes((long)r.ucsize);
                    e.SHA1 = r._sha1;
                    e.rtype = BitConverter.GetBytes(r.type);
                    res.res.Add(e);
                }
            if (bin.ChunkList!= null)
                foreach (BinaryBundle.ChunkEntry c in bin.ChunkList)
                {
                    chunktype e = new chunktype();
                    e.size = BitConverter.GetBytes((long)c._originalSize);
                    e.SHA1 = c._sha1;
                    e.id = c.id;                    
                    res.chunk.Add(e);
                }
            return res;
        }

        public static Bundle Create(BJSON.Entry e)
        {
            Bundle res = new Bundle();
            res.iscas = true;
            res.ebx = new List<ebxtype>();
            res.res = new List<restype>();
            res.chunk = new List<chunktype>();
            foreach (BJSON.Field f in e.fields)
                switch (f.fieldname)
                {
                    case "path":
                        res.path = (string)f.data;
                        break;
                    case "magicSalt":
                        res.salt = BitConverter.ToUInt32((byte[])f.data, 0).ToString("X4");
                        break;
                    case "alignMembers":
                        res.align = (bool)f.data;
                        break;
                    case "storeCompressedSizes":
                        res.compressed = (bool)f.data;
                        break;
                    case "totalSize":
                        res.totalsize = BitConverter.ToUInt64((byte[])f.data, 0);
                        break;
                    case "dbxtotalSize":
                        res.dbxtotalsize = BitConverter.ToUInt64((byte[])f.data, 0);
                        break;
                    case "ebx":
                        res.ebx = ReadEbx(f);
                        break;
                    case "dbx":
                        res.dbx = ReadDbx(f);
                        break;
                    case "res":
                        res.res = ReadRes(f);
                        break;
                    case "chunks":
                    case "chunks0":
                        res.chunk.AddRange(ReadChunks(f));
                        break;
                }
            return res;
        }

        private static List<ebxtype> ReadEbx(BJSON.Field f)
        {
            List<ebxtype> res = new List<ebxtype>();
            List<BJSON.Entry> list = (List<BJSON.Entry>)f.data;
            foreach (BJSON.Entry e in list)
            {
                ebxtype ebx = new ebxtype();
                ebx.link = e;
                ebx.baseSha1 = ebx.deltaSha1 = ebx.Sha1 = new byte[0];
                foreach (BJSON.Field f2 in e.fields)
                    switch (f2.fieldname)
                    {
                        case "name":
                            ebx.name = (string)f2.data;
                            break;
                        case "sha1":
                            ebx.Sha1 = (byte[])f2.data;
                            break;
                        case "size":
                            ebx.size = (byte[])f2.data;
                            break;
                        case "originalSize":
                            ebx.originalSize = (byte[])f2.data;
                            break;
                        case "casPatchType":
                            ebx.casPatchType = Helpers.ReadInt(new MemoryStream((byte[])f2.data));
                            break;
                        case "baseSha1":
                            ebx.baseSha1 = (byte[])f2.data;
                            break;
                        case "deltaSha1":
                            ebx.deltaSha1 = (byte[])f2.data;
                            break;
                    }
                res.Add(ebx);
            }
            return res;
        }

        private static List<dbxtype> ReadDbx(BJSON.Field f)
        {
            List<dbxtype> res = new List<dbxtype>();
            List<BJSON.Entry> list = (List<BJSON.Entry>)f.data;
            foreach (BJSON.Entry e in list)
            {
                dbxtype dbx = new dbxtype();
                dbx.link = e;
                foreach (BJSON.Field f2 in e.fields)
                    switch (f2.fieldname)
                    {
                        case "name":
                            dbx.name = (string)f2.data;
                            break;
                        case "sha1":
                            dbx.SHA1 = (byte[])f2.data;
                            break;
                        case "size":
                            dbx.size = (byte[])f2.data;
                            break;
                        case "originalSize":
                            dbx.osize = (byte[])f2.data;
                            break;
                    }
                res.Add(dbx);
            }
            return res;
        }

        private static List<restype> ReadRes(BJSON.Field f)
        {
            List<restype> res = new List<restype>();
            List<BJSON.Entry> list = (List<BJSON.Entry>)f.data;
            foreach (BJSON.Entry e in list)
            {
                restype r = new restype();
                r.link = e;
                foreach (BJSON.Field f2 in e.fields)
                    switch (f2.fieldname)
                    {
                        case "name":
                            r.name = (string)f2.data;
                            break;
                        case "sha1":
                            r.SHA1 = (byte[])f2.data;
                            break;
                        case "size":
                            r.size = (byte[])f2.data;
                            break;
                        case "originalSize":
                            r.osize = (byte[])f2.data;
                            break;
                        case "resType":
                            r.rtype = (byte[])f2.data;
                            break;
                        case "casPatchType":
                            r.casPatchType = Helpers.ReadInt(new MemoryStream((byte[])f2.data));
                            break;
                        case "baseSha1":
                            r.baseSha1 = (byte[])f2.data;
                            break;
                        case "deltaSha1":
                            r.deltaSha1 = (byte[])f2.data;
                            break;
                    }
                res.Add(r);
            }
            return res;
        }

        private static List<chunktype> ReadChunks(BJSON.Field f)
        {
            List<chunktype> res = new List<chunktype>();
            List<BJSON.Entry> list = (List<BJSON.Entry>)f.data;
            foreach (BJSON.Entry e in list)
            {
                chunktype c = new chunktype();
                c.link = e;
                foreach (BJSON.Field f2 in e.fields)
                    switch (f2.fieldname)
                    {
                        case "id":
                            c.id = (byte[])f2.data;
                            break;
                        case "sha1":
                            c.SHA1 = (byte[])f2.data;
                            break;
                        case "size":
                            c.size = (byte[])f2.data;
                            break;
                        case "casPatchType":
                            c.casPatchType = Helpers.ReadInt(new MemoryStream((byte[])f2.data));
                            break;
                        case "baseSha1":
                            c.baseSha1 = (byte[])f2.data;
                            break;
                        case "deltaSha1":
                            c.deltaSha1 = (byte[])f2.data;
                            break;
                    }
                res.Add(c);
            }
            return res;
        }

    }
}
