using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAILibWV.Frostbite;

namespace DAILibWV
{
    public static class SHA1Access
    {
        public static CATFile cat_base = null;
        public static CATFile cat_patch = null;
        public static CASFile cas;

        public static byte[] GetDataBySha1(byte[] sha1)
        {
            CATFile cat = null;
            if (cat_base == null)
            {
                string path = GlobalStuff.FindSetting("gamepath") + "Data\\cas.cat";
                cat_base = new CATFile(path);
            }
            List<uint> casline = cat_base.FindBySHA1(sha1);
            if (casline.Count == 9)
                cat = cat_base;
            else
            {
                if (cat_patch == null)
                {
                    string path = GlobalStuff.FindSetting("gamepath") + "Update\\Patch\\Data\\cas.cat";
                    cat_patch = new CATFile(path);
                }
                cat = cat_patch;
                casline = cat_patch.FindBySHA1(sha1);
            }
            if (casline.Count == 9)
            {
                if (cas == null || cas.casnumber != casline[7])
                {
                    string[] files = Directory.GetFiles(Path.GetDirectoryName(cat.MyPath));
                    foreach (string file in files)
                        if (Path.GetFileName(file) == CASFile.GetCASFileName(casline[7]))
                        {
                            cas = new CASFile(file);
                            break;
                        }
                }
                if (cas != null && cas.casnumber == casline[7])
                {
                    CASFile.CASEntry ce = cas.ReadEntry(casline.ToArray());
                    return ce.data;
                }
            }
            return new byte[0];
        }
    }
}
