using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAILibWV.OldSupport
{
    public class DAIMODFile
    {
        public string MyPath;
        public string XML;
        public string Script;
        public List<byte[]> Data;

        public DAIMODFile(string path)
        {
            MyPath = path;
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            fs.Seek(0xC, 0);
            string name = Helpers.ReadNullString(fs);
            XML = Helpers.ReadNullString(fs);
            Script = Helpers.ReadNullString(fs);
            Data = new List<byte[]>();
            int count = Helpers.ReadInt(fs);
            for (int i = 0; i < count; i++)
            {
                byte[] data = new byte[Helpers.ReadInt(fs)];                
                fs.Read(data, 0, data.Length);
                Data.Add(data);
            }
            fs.Close();
        }
    }
}
