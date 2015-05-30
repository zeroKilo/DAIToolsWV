using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAILibWV.Frostbite
{
    public class CATFile
    {
        public string MyPath;
        public int Level;
        public uint div;
        public List<uint[]> lines;
        public List<List<uint[]>> fastlookup;

        public CATFile(string path, int level = 256) //use only 2^x as level, like 2,4,8,16,32
        {
            MyPath = path;
            Level = level;
            div = (uint)(0x100000000 / Level);
            ReadFile();
        }

        private void ReadFile()
        {
            FileStream fs = new FileStream(MyPath, FileMode.Open, FileAccess.Read);
            for (int i = 0; i < 4; i++)
                if (Helpers.ReadUInt(fs) != 0x6E61794E)
                    return;
            lines = new List<uint[]>();
            fastlookup = new List<List<uint[]>>();
            for (int i = 0; i < Level; i++)
                fastlookup.Add(new List<uint[]>());
            while (fs.Position < fs.Length)
            {
                uint[] line = new uint[9];
                line[8] = (uint)fs.Position;
                for (int i = 0; i < 8; i++)
                    if (i < 5)
                        line[i] = Helpers.ReadLEUInt(fs);
                    else
                        line[i] = Helpers.ReadUInt(fs);

                int dic = (int)(line[0] / div);
                List<uint[]> t = fastlookup[dic];
                t.Add(line);
                fastlookup[dic] = t;
                lines.Add(line);
            }
            fs.Close();
        }

        public List<uint> FindBySHA1(byte[] sha1)
        {
            MemoryStream m = new MemoryStream(sha1);
            List<uint> res = new List<uint>();
            uint[] sha1ints = new uint[5];
            for (int i = 0; i < 5; i++)
                sha1ints[i] = Helpers.ReadLEUInt(m);
            int dic = (int)(sha1ints[0] / div);
            for (int i = 0; i < fastlookup[dic].Count; i++)
                if (fastlookup[dic][i][0] == sha1ints[0] &&
                   fastlookup[dic][i][1] == sha1ints[1] &&
                   fastlookup[dic][i][2] == sha1ints[2] &&
                   fastlookup[dic][i][3] == sha1ints[3] &&
                   fastlookup[dic][i][4] == sha1ints[4])
                {
                    res.AddRange(fastlookup[dic][i]);
                    break;
                }
            return res;
        }
    }
}
