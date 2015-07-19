using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace DAILibWV
{
    public class Mod
    {
        public struct ModJob
        {
            public int type;//0 = texture replacement
            public string respath;
            public List<string> paths;
            public byte[] data;
        }

        public string headerXML;
        public List<ModJob> jobs;
        public static readonly int currVersion = 1;

        public string GetTypeName(int type)
        {
            switch (type)
            {
                case 0:
                    return "Texture Mod";
                default:
                    return "Unknown Modtype";
            }
        }

        public void Load(string path)
        {
            Load(File.ReadAllBytes(path));
        }

        public void Load(byte[] data)
        {
            MemoryStream m = new MemoryStream(data);
            m.Seek(0, 0);
            ReadHeader(m);
            ParseHeader(m);
        }

        private void ReadHeader(Stream s)
        {
            int len = Helpers.ReadInt(s);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < len; i++)
                sb.Append((char)s.ReadByte());
            headerXML = sb.ToString();
        }

        private void ParseHeader(Stream s)
        {
            jobs = new List<ModJob>();
            StringReader sr = new StringReader(headerXML);
            XmlTextReader r = new XmlTextReader(sr);
            r.ReadToFollowing("version");
            int version = Convert.ToInt32(r.ReadElementContentAsString());
            if (version > currVersion)
                return;
            r.ReadToFollowing("game");
            if (r.ReadElementContentAsString() != "DAI")
                return;
            r.ReadToFollowing("jobcount");
            int count = Convert.ToInt32(r.ReadElementContentAsString());
            r.ReadToFollowing("jobs");
            List<byte[]> dataList = new List<byte[]>();
            while (s.Position < s.Length)
            {
                int size = Helpers.ReadInt(s);
                if (size == 0)
                    return;
                byte[] data = new byte[size];
                s.Read(data, 0, size);
                dataList.Add(data);
            }
            for (int i = 0; i < count; i++)
                ParseJob(r, dataList);
        }

        private void ParseJob(XmlTextReader r, List<byte[]> dataList)
        {
            ModJob mj = new ModJob();
            r.ReadToFollowing("job");
            r.ReadToFollowing("type");
            mj.type = Convert.ToInt32(r.ReadElementContentAsString());
            switch (mj.type)
            {
                case 0:
                    r.ReadToFollowing("dataindex");
                    int index = Convert.ToInt32(r.ReadElementContentAsString());
                    mj.data = dataList[index];
                    r.ReadToFollowing("respath");
                    mj.respath = r.ReadElementContentAsString();
                    r.ReadToFollowing("bundlecount");
                    int count = Convert.ToInt32(r.ReadElementContentAsString());
                    mj.paths = new List<string>();
                    for (int i = 0; i < count; i++)
                    {
                        r.ReadToFollowing("path");
                        mj.paths.Add(r.ReadElementContentAsString());
                    }
                    break;
            }
            jobs.Add(mj);
        }

        public void Save(string path)
        {
            File.WriteAllBytes(path, Save());
        }

        public byte[] Save()
        {
            CreateHeader();
            MemoryStream m = new MemoryStream();
            Helpers.WriteInt(m, headerXML.Length);
            foreach (char c in headerXML)
                m.WriteByte((byte)c);
            foreach(ModJob mj in jobs)
                switch (mj.type)
                {
                    case 0:
                        Helpers.WriteInt(m, mj.data.Length);
                        m.Write(mj.data, 0, mj.data.Length);
                        break;
                }
            return m.ToArray();
        }

        public void CreateHeader()
        {
            StringWriter sw = new StringWriter();
            XmlTextWriter w = new XmlTextWriter(sw);
            w.Formatting = Formatting.Indented;
            w.Indentation = 4;
            w.WriteStartDocument();
            w.WriteStartElement("mod");
            w.WriteStartElement("details");
            w.WriteElementString("version", "1");
            w.WriteElementString("game", "DAI");
            w.WriteElementString("author", "unknown");
            w.WriteElementString("creationdate", DateTime.Now.ToLongDateString());
            w.WriteElementString("jobcount", jobs.Count.ToString());
            w.WriteFullEndElement();
            w.WriteStartElement("jobs");
            int count = 0;
            int datacount = 0;
            foreach (ModJob mj in jobs)
            {
                w.WriteStartElement("job");
                w.WriteElementString("index", count.ToString());     
                w.WriteElementString("type", mj.type.ToString());                
                switch (mj.type)
                {
                    case 0:
                        w.WriteElementString("dataindex", (datacount++).ToString());
                        w.WriteElementString("respath", mj.respath);
                        w.WriteElementString("bundlecount", mj.paths.Count.ToString());
                        w.WriteStartElement("bundles");
                        foreach (string p in mj.paths)
                            w.WriteElementString("path", p);
                        w.WriteFullEndElement();
                        break;
                }
                w.WriteFullEndElement();
                count++;
            }
            w.WriteFullEndElement();
            w.WriteFullEndElement();
            w.Flush();
            headerXML = sw.ToString();
        }
    }
}
