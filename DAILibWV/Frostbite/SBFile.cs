using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAILibWV.Frostbite
{
    public class SBFile
    {
        public string MyPath;
        public List<BJSON.Entry> lines;
        public List<Bundle> bundles;

        public SBFile(string path)
        {
            MyPath = path;
            ReadFile();
            ProcessFile();
        }

        public void Save()
        {
            Save(MyPath);
        }

        public void Save(string path)
        {
            FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write);
            foreach (BJSON.Entry e in lines)
                BJSON.WriteEntry(fs, e);
            fs.Close();
        }



        private void ReadFile()
        {
            FileStream fs = new FileStream(MyPath, FileMode.Open, FileAccess.Read);
            lines = new List<BJSON.Entry>();
            BJSON.ReadEntries(fs, lines);
            fs.Close();
        }

        public void ProcessFile()
        {
            bundles = new List<Bundle>();
            foreach (BJSON.Entry e in lines)
                if (e.type == 0x82)
                    foreach (BJSON.Field f in e.fields)
                        if (f.fieldname == "bundles")
                            foreach (BJSON.Entry b in (List<BJSON.Entry>)f.data)
                                bundles.Add(Bundle.Create(b));
        }
    }
}
