using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Windows.Forms;
using DAILibWV.Frostbite;

namespace DAILibWV
{
    public static class DBAccess
    {
        public static string dbpath = Path.GetDirectoryName(Application.ExecutablePath) + "\\database.sqlite";
        public static readonly string TYPE_BASEGAME = "b";
        public static readonly string TYPE_UPDATE = "u";
        public static readonly string TYPE_PATCH = "p";

        public struct TOCInformation
        {
            public int index;
            public string path;
            public string md5;
            public bool incas;
            public string type;
        }

        public struct BundleInformation
        {
            public int index;
            public int tocIndex;
            public string bundlepath;
            public string filepath;
            public int offset;
            public int size;
            public bool isdelta;
            public bool isbase;
            public bool incas;
            public bool isbasegamefile;
            public bool isDLC;
            public bool isPatch;
        }

        public struct EBXInformation
        {
            public string ebxname;
            public string sha1;
            public string basesha1;
            public string deltasha1;
            public int casPatchType;
            public string guid;
            public string bundlepath;
            public int offset;
            public int size;
            public bool isbase;
            public bool isdelta;
            public string tocfilepath;
            public bool incas;
            public bool isbasegamefile;
            public bool isDLC;
            public bool isPatch;
        }

        public struct RESInformation
        {
            public string resname;
            public string sha1;
            public string rtype;
            public string bundlepath;
            public int offset;
            public int size;
            public bool isbase;
            public bool isdelta;
            public string tocfilepath;
            public bool incas;
            public bool isbasegamefile;
            public bool isDLC;
            public bool isPatch;
        }

        public struct TextureInformation
        {
            public string name;
            public byte[] sha1;
            public int bundleIndex;
        }

        public struct ChunkInformation
        {
            public byte[] id;
            public byte[] sha1;
            public int bundleIndex;
        }
        
        #region get SQL stuff

        public static void SQLCommand(string sql, SQLiteConnection con)
        {
            SQLiteCommand command = new SQLiteCommand(sql, con);
            command.ExecuteNonQuery();
        }

        public static SQLiteConnection GetConnection()
        {
            return new SQLiteConnection("Data Source=" + dbpath + ";Version=3;Compress=True;");
        }

        public static long GetLastRowId(SQLiteConnection con)
        {
            return con.LastInsertRowId;
        }

        public static long SQLGetRowCount(string table, SQLiteConnection con)
        {
            SQLiteCommand command = new SQLiteCommand("SELECT COUNT(*) FROM " + table, con);
            SQLiteDataReader reader = command.ExecuteReader();
            reader.Read();
            return (long)reader.GetValue(0);
        }

        public static SQLiteDataReader getReader(string sql, SQLiteConnection con)
        {
            SQLiteCommand command = new SQLiteCommand(sql, con);
            return command.ExecuteReader();
        }

        public static SQLiteDataReader getAll(string table, SQLiteConnection con)
        {
            SQLiteCommand command = new SQLiteCommand("SELECT * FROM " + table, con);
            return command.ExecuteReader();
        }

        public static SQLiteDataReader getAllJoined(string table1, string table2, string key1, string key2, SQLiteConnection con, string sort = null)
        {
            string sql = "SELECT * FROM " + table1 + " JOIN " + table2 + " ON (" + table1 + "." + key1 + " = " + table2 + "." + key2 + ")";
            if (sort != null)
                sql += " ORDER BY " + sort;
            SQLiteCommand command = new SQLiteCommand(sql, con);
            return command.ExecuteReader();
        }

        public static SQLiteDataReader getAllJoined3(string table1, string table2, string table3, string key12, string key21, string key23, string key32, SQLiteConnection con, string sort = null)
        {
            string sql = "SELECT * FROM " + table1 + " ";
            sql += "JOIN " + table2 + " ON (" + table1 + "." + key12 + " = " + table2 + "." + key21 + ") ";
            sql += "JOIN " + table3 + " ON (" + table2 + "." + key23 + " = " + table3 + "." + key32 + ") ";
            if (sort != null)
                sql += " ORDER BY " + sort;
            SQLiteCommand command = new SQLiteCommand(sql, con);
            return command.ExecuteReader();
        }

        public static SQLiteDataReader getAllSorted(string table, string order, SQLiteConnection con)
        {
            SQLiteCommand command = new SQLiteCommand("SELECT * FROM " + table + " ORDER BY " + order, con);
            return command.ExecuteReader();
        }

        public static SQLiteDataReader getAllWhere(string table, string where, SQLiteConnection con)
        {
            SQLiteCommand command = new SQLiteCommand("SELECT * FROM " + table + " WHERE " + where, con);
            return command.ExecuteReader();
        }

        public static SQLiteDataReader getAllJoinedWhere(string table1, string table2, string key1, string key2, string where, SQLiteConnection con, string sort = null)
        {
            string sql = "SELECT * FROM " + table1 + " JOIN " + table2 + " ON (" + table1 + "." + key1 + " = " + table2 + "." + key2 + ") WHERE " + where + " ";
            if (sort != null)
                sql += " ORDER BY " + sort;
            SQLiteCommand command = new SQLiteCommand(sql, con);
            return command.ExecuteReader();
        }

        public static SQLiteDataReader getAllJoined3Where(string table1, string table2, string table3, string key12, string key21, string key23, string key32, string where, SQLiteConnection con, string sort = null)
        {
            string sql = "SELECT * FROM " + table1 + " ";
            sql += "JOIN " + table2 + " ON (" + table1 + "." + key12 + " = " + table2 + "." + key21 + ") ";
            sql += "JOIN " + table3 + " ON (" + table2 + "." + key23 + " = " + table3 + "." + key32 + ") ";
            sql += "WHERE " + where;
            if (sort != null)
                sql += " ORDER BY " + sort;
            SQLiteCommand command = new SQLiteCommand(sql, con);
            return command.ExecuteReader();
        }

        #endregion

        #region database init and settings

        public static bool CheckIfScanIsNeeded()
        {
            return (GlobalStuff.FindSetting("isNew") == "1");
        }

        public static bool CheckIfDBExists()
        {
            return (File.Exists(dbpath));
        }

        public static void CreateDataBase()
        {
            if (!File.Exists(dbpath))
                File.Delete(dbpath);
            SQLiteConnection.CreateFile(dbpath);
            SQLiteConnection con = GetConnection();
            con.Open();
            SQLCommand("CREATE TABLE settings (key TEXT, value TEXT)", con);
            SQLCommand("INSERT INTO settings (key, value) values ('isNew', '1')", con);
            ClearGlobalChunkdb(con);
            ClearSBFilesdb(con);
            ClearTOCFilesdb(con);
            ClearBundlesdb(con);
            ClearEBXLookUpTabledb(con);
            con.Close();
        }

        public static void LoadSettings()
        {
            SQLiteConnection con = GetConnection();
            con.Open();
            SQLiteDataReader reader = getAll("settings", con);
            GlobalStuff.settings = new Dictionary<string, string>();
            while (reader.Read())
                GlobalStuff.settings.Add(reader.GetString(0), reader.GetString(1));
            con.Close();
        }

        public static void SaveSettings()
        {
            SQLiteConnection con = GetConnection();
            con.Open();
            SQLCommand("DROP TABLE settings", con);
            SQLCommand("CREATE TABLE settings (key TEXT, value TEXT)", con);
            foreach (KeyValuePair<string, string> setting in GlobalStuff.settings)
                SQLCommand("INSERT INTO settings (key, value) values ('" + setting.Key + "', '" + setting.Value + "')", con);
            con.Close();
            LoadSettings();
        }

        public static void ClearSBFilesdb(SQLiteConnection con)
        {
            SQLCommand("DROP TABLE IF EXISTS sbfiles", con);
            SQLCommand("CREATE TABLE sbfiles (id INTEGER PRIMARY KEY AUTOINCREMENT, path TEXT, type TEXT)", con);
        }

        public static void ClearTOCFilesdb(SQLiteConnection con)
        {
            SQLCommand("DROP TABLE IF EXISTS tocfiles", con);
            SQLCommand("CREATE TABLE tocfiles (id INTEGER PRIMARY KEY AUTOINCREMENT, path TEXT, md5 TEXT, incas TEXT, type TEXT)", con);
        }

        public static void ClearEBXLookUpTabledb(SQLiteConnection con)
        {
            SQLCommand("DROP TABLE IF EXISTS ebxlut", con);
            SQLCommand("CREATE TABLE ebxlut (id INTEGER PRIMARY KEY AUTOINCREMENT, "
            + "path TEXT, sha1 TEXT, basesha1 TEXT, deltasha1 TEXT, casptype INT, guid TEXT, "
            + "bundlepath TEXT, offset INT, size INT, isbase TEXT, isdelta TEXT, tocpath TEXT, incas TEXT, filetype TEXT)", con);
        }

        public static void ClearGlobalChunkdb(SQLiteConnection con)
        {
            SQLCommand("DROP TABLE IF EXISTS globalchunks", con);
            SQLCommand("CREATE TABLE globalchunks (idx INTEGER PRIMARY KEY AUTOINCREMENT, tocfile INTEGER, id TEXT, sha1 TEXT, offset INT, size INT)", con);
        }

        public static void ClearBundlesdb(SQLiteConnection con)
        {
            SQLCommand("DROP TABLE IF EXISTS bundles", con);
            SQLCommand("DROP TABLE IF EXISTS res", con);
            SQLCommand("DROP TABLE IF EXISTS chunks", con);
            SQLCommand("CREATE TABLE bundles (id INTEGER PRIMARY KEY AUTOINCREMENT, tocfile INTEGER, frostid TEXT, offset INT, size INT, base TEXT, delta TEXT, FOREIGN KEY (tocfile) REFERENCES tocfiles (id))", con);
            SQLCommand("CREATE TABLE res (name TEXT, sha1 TEXT, rtype TEXT, bundle INT, FOREIGN KEY (bundle) REFERENCES bundles (id))", con);
            SQLCommand("CREATE TABLE chunks (id TEXT, sha1 TEXT, bundle INT, FOREIGN KEY (bundle) REFERENCES bundles (id))", con);
        }

        #endregion

        #region add stuff

        public static void AddSHA1(uint[] entry, string type, SQLiteConnection con)
        {
            StringBuilder sb = new StringBuilder();
            foreach (uint u in entry)
                sb.Append(u.ToString("X8"));
            SQLCommand("INSERT INTO sha1db VALUES ('" + sb.ToString() + "', '" + type + "')", con);
        }

        public static void AddGlobalChunk(int tocid, byte[] id, byte[] sha1, int offset, int size, SQLiteConnection con)
        {
            StringBuilder sb = new StringBuilder();
            if (id != null)
                foreach (byte b in id)
                    sb.Append(b.ToString("X2"));
            StringBuilder sb2 = new StringBuilder();
            if (sha1 != null)
                foreach (byte b in sha1)
                    sb2.Append(b.ToString("X2"));
            SQLCommand("INSERT INTO globalchunks (tocfile, id, sha1, offset, size) VALUES (" + tocid + ",'" + sb.ToString() + "','" + sb2.ToString() + "', " + offset + "," + size + ")", con);
        }

        public static void AddSBFile(string path, string type, SQLiteConnection con)
        {
            SQLCommand("INSERT INTO sbfiles (path, type) VALUES ('" + path + "','" + type + "')", con);
        }

        public static void AddTOCFile(string path, string type, SQLiteConnection con)
        {
            string md5 = Helpers.ByteArrayToHexString(Helpers.ComputeHash(path));
            Debug.LogLn(" MD5: " + md5 + " Filename: " + Path.GetFileName(path));
            TOCFile toc = new TOCFile(path);
            bool incas = false;
            foreach (BJSON.Field f1 in toc.lines[0].fields)
                if (f1.fieldname == "cas")
                    incas = (bool)f1.data;
            SQLCommand("INSERT INTO tocfiles (path, md5, incas, type) VALUES ('" + path + "', '" + md5 + "','" + incas + "','" + type + "')", con);
        }

        public static void AddCASFile(string path, string type, SQLiteConnection con)
        {
            SQLCommand("INSERT INTO casfiles VALUES ('" + path + "','" + type + "')", con);
        }

        public static void AddRESFile(string name, byte[] sha1, byte[] rtype, int bundleid, SQLiteConnection con)
        {
            name = name.Replace("'", "");//lolfix
            SQLCommand("INSERT INTO res VALUES ('" + name + "','" + Helpers.ByteArrayToHexString(sha1) + "', '" + Helpers.ByteArrayToHexString(rtype) + "', " + bundleid + ")", con);
        }

        public static void AddChunk(byte[] id, byte[] sha1, int bundleid, SQLiteConnection con)
        {
            SQLCommand("INSERT INTO chunks VALUES ('" + Helpers.ByteArrayToHexString(id) + "','" + Helpers.ByteArrayToHexString(sha1) + "'," + bundleid + ")", con);
        }

        public static void AddBundle(int tocid, bool incas, Bundle b, TOCFile.TOCBundleInfoStruct info, SQLiteConnection con)
        {
            Debug.LogLn(" EBX:" + b.ebx.Count + " RES:" + b.res.Count + " CHUNK:" + b.chunk.Count, true);
            SQLCommand("INSERT INTO bundles (tocfile, frostid, offset, size, base, delta) VALUES (" + tocid + ",'" + info.id + "'," + info.offset + ", " + info.size + ", '" + info.isbase + "', '" + info.isdelta + "' )", con);
            int bundleid = (int)GetLastRowId(con);
            TOCInformation toci = GetTocInformationByIndex(tocid);
            var transaction = con.BeginTransaction();
            int counter = 0;
            if (b.ebx != null)
                foreach (Bundle.ebxtype ebx in b.ebx)
                    try
                    {
                        if (ebx.name != null && ebx.originalSize != null && ebx.size != null)
                        {
                            EBXInformation inf = new EBXInformation();
                            inf.basesha1 = Helpers.ByteArrayToHexString(ebx.baseSha1);
                            inf.bundlepath = b.path;
                            inf.casPatchType = ebx.casPatchType;
                            inf.deltasha1 = Helpers.ByteArrayToHexString(ebx.deltaSha1);
                            inf.ebxname = ebx.name;
                            inf.incas = incas;
                            inf.isbase = info.isbase;
                            inf.isdelta = info.isdelta;
                            if (toci.type == TYPE_BASEGAME)
                                inf.isbasegamefile = true;
                            if (toci.type == TYPE_UPDATE)
                                inf.isDLC = true;
                            if (toci.type == TYPE_PATCH)
                                inf.isPatch = true;
                            inf.offset = info.offset;
                            inf.sha1 = Helpers.ByteArrayToHexString(ebx.Sha1);
                            inf.size = info.size;
                            inf.tocfilepath = toci.path;
                            byte[] data = new byte[0];
                            if (inf.incas)
                                data = SHA1Access.GetDataBySha1(ebx.Sha1, 0x38);
                            else
                            {
                                BinaryBundle bb = null;
                                foreach (AddEBXHelpStruct h in aehelp)
                                    if (h.tocpath == inf.tocfilepath && h.bpath == inf.bundlepath)
                                    {
                                        bb = h.b;
                                        break;
                                    }
                                if (bb == null)
                                {
                                    TOCFile toc = new TOCFile(inf.tocfilepath);
                                    byte[] bundledata = toc.ExportBundleDataByPath(inf.bundlepath);
                                    bb = new BinaryBundle(new MemoryStream(bundledata));
                                    AddEBXHelpStruct h = new AddEBXHelpStruct();
                                    h.tocpath = inf.tocfilepath;
                                    h.bpath = inf.bundlepath;
                                    h.b = bb;
                                    if (aehelp.Count > 10)
                                        aehelp.RemoveAt(0);
                                }
                                foreach (BinaryBundle.EbxEntry ebx2 in bb.EbxList)
                                    if (inf.ebxname == ebx2._name)
                                        data = ebx2._data;
                            }
                            inf.guid = Helpers.ByteArrayToHexString(data, 0x28, 0x10);
                            AddEBXLUTFile(inf, con);
                            if ((counter++) % 100 == 0)
                            {
                                transaction.Commit();
                                transaction = con.BeginTransaction();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
            transaction.Commit();
            transaction = con.BeginTransaction();
            if (b.res != null)
                foreach (Bundle.restype res in b.res)
                    try
                    {
                        if (res.name != null)
                            AddRESFile(res.name, res.SHA1, res.rtype, bundleid, con);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
            transaction.Commit();
            transaction = con.BeginTransaction();
            if (b.chunk != null)
                foreach (Bundle.chunktype chunk in b.chunk)
                    try
                    {
                        AddChunk(chunk.id, chunk.SHA1, bundleid, con);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
            transaction.Commit();
        }

        private struct AddEBXHelpStruct
        {
            public string tocpath;
            public string bpath;
            public BinaryBundle b;
        }

        private static List<AddEBXHelpStruct> aehelp;

        public static void AddEBXLUTFile(EBXInformation ebx, SQLiteConnection con)
        {
            string ftype = "b";
            if (ebx.isDLC)
                ftype = "u";
            if (ebx.isPatch)
                ftype = "p";
            string guid = "";
            byte[] data = new byte[0];
            if (ebx.incas)
                data = SHA1Access.GetDataBySha1(Helpers.HexStringToByteArray(ebx.sha1));
            else
            {
                BinaryBundle b  = null;
                foreach(AddEBXHelpStruct h in aehelp)
                    if (h.tocpath == ebx.tocfilepath && h.bpath == ebx.bundlepath)
                    {
                        b = h.b;
                        break;
                    }
                if (b == null)
                {
                    TOCFile toc = new TOCFile(ebx.tocfilepath);
                    byte[] bundledata = toc.ExportBundleDataByPath(ebx.bundlepath);
                    b = new BinaryBundle(new MemoryStream(bundledata));
                    AddEBXHelpStruct h = new AddEBXHelpStruct();
                    h.tocpath = ebx.tocfilepath;
                    h.bpath = ebx.bundlepath;
                    h.b = b;
                    if (aehelp.Count > 10)
                        aehelp.RemoveAt(0);
                }
                foreach (BinaryBundle.EbxEntry ebx2 in b.EbxList)
                    if (ebx.ebxname == ebx2._name)
                        data = ebx2._data;
            }
            guid = Helpers.ByteArrayToHexString(data, 0x28, 0x10);
            SQLCommand("INSERT INTO ebxlut (path,sha1,basesha1,deltasha1,casptype,guid,bundlepath,offset,size,isbase,isdelta,tocpath,incas,filetype) VALUES ('"
                + ebx.ebxname.Replace("'","''") + "','"
                + ebx.sha1 + "','"
                + ebx.basesha1 + "','"
                + ebx.deltasha1 + "',"
                + ebx.casPatchType + ",'"
                + guid + "','"
                + ebx.bundlepath + "',"
                + ebx.offset + ","
                + ebx.size + ",'"
                + ebx.isbase + "','"
                + ebx.isdelta + "','"
                + ebx.tocfilepath + "','"
                + ebx.incas + "','"
                + ftype + "')", con);
        }

        #endregion

        #region get specific stuff

        public static string[] GetGameFiles(string table)
        {
            List<string> result = new List<string>();
            SQLiteConnection con = GetConnection();
            con.Open();
            SQLiteDataReader reader = getAll(table, con);
            while (reader.Read())
                result.Add(reader.GetString(1));
            con.Close();
            return result.ToArray();
        }

        public static string[] GetFiles(string type, bool withBase, bool withDLC, bool withPatch)
        {
            string[] list = DBAccess.GetGameFiles(type);
            string basepath = GlobalStuff.FindSetting("gamepath");
            List<string> tmp = new List<string>(list);
            for (int i = 0; i < tmp.Count; i++)
            {
                if (!withBase)
                    if (!tmp[i].Contains("Update"))
                    {
                        tmp.RemoveAt(i--);
                        continue;
                    }
                if (!withDLC)
                    if (tmp[i].Contains("Update") && !tmp[i].Contains("Patch"))
                    {
                        tmp.RemoveAt(i--);
                        continue;
                    }
                if (!withPatch)
                    if (tmp[i].Contains("Update") && tmp[i].Contains("Patch"))
                    {
                        tmp.RemoveAt(i--);
                        continue;
                    }

            }
            list = tmp.ToArray();
            for (int i = 0; i < list.Length; i++)
                list[i] = list[i].Substring(basepath.Length, list[i].Length - basepath.Length);
            return list;
        }

        public static BundleInformation[] GetBundleInformation()
        {
            List<BundleInformation> result = new List<BundleInformation>();
            SQLiteConnection con = GetConnection();
            con.Open();
            SQLiteDataReader reader = getAllJoined("bundles", "tocfiles", "tocfile", "id", con, "frostid");
            int count = 0;
            while (reader.Read())
            {
                if (count++ % 1000 == 0)
                    Application.DoEvents();
                BundleInformation bi = new BundleInformation();
                bi.index = reader.GetInt32(0);
                bi.tocIndex = reader.GetInt32(1);
                bi.bundlepath = reader.GetString(2).ToLower();
                bi.offset = reader.GetInt32(3);
                bi.size = reader.GetInt32(4);
                bi.isbase = reader.GetString(5) == "True";
                bi.isdelta = reader.GetString(6) == "True";
                bi.filepath = reader.GetString(8).ToLower();
                bi.incas = reader.GetString(10) == "True";
                switch (reader.GetString(11))
                {
                    case "b":
                        bi.isbasegamefile = true;
                        break;
                    case "u":
                        bi.isDLC= true;
                        break;
                    case "p":
                        bi.isPatch= true;
                        break;
                }
                result.Add(bi);
            }
            con.Close();
            return result.ToArray();
        }

        public static BundleInformation GetBundleInformationByIndex(int index)
        {
            BundleInformation result = new BundleInformation();
            result.tocIndex = -1;
            SQLiteConnection con = GetConnection();
            con.Open();
            SQLiteDataReader reader = getAllJoinedWhere("bundles", "tocfiles", "tocfile", "id", "bundles.id = " + index, con, "frostid");
            if (reader.Read())
            {
                BundleInformation bi = new BundleInformation();
                bi.index = reader.GetInt32(0);
                bi.tocIndex = reader.GetInt32(1);
                bi.bundlepath = reader.GetString(2).ToLower();
                bi.offset = reader.GetInt32(3);
                bi.size = reader.GetInt32(4);
                bi.isbase = reader.GetString(5) == "True";
                bi.isdelta = reader.GetString(6) == "True";
                bi.filepath = reader.GetString(8).ToLower();
                bi.incas = reader.GetString(10) == "True";
                switch (reader.GetString(11))
                {
                    case "b":
                        bi.isbasegamefile = true;
                        break;
                    case "u":
                        bi.isDLC = true;
                        break;
                    case "p":
                        bi.isPatch = true;
                        break;
                }
                result = bi;
            }
            con.Close();
            return result;
        }
        
        public static BundleInformation[] GetBundleInformationById(string path)
        {
            List<BundleInformation> result = new List<BundleInformation>();
            SQLiteConnection con = GetConnection();
            con.Open();
            SQLiteDataReader reader = getAllJoinedWhere("bundles", "tocfiles", "tocfile", "id", "lower(bundles.frostid) = '" + path.ToLower() + "'", con);
            int count = 0;
            while (reader.Read())
            {
                if (count++ % 1000 == 0)
                    Application.DoEvents();
                BundleInformation bi = new BundleInformation();
                bi.index = reader.GetInt32(0);
                bi.tocIndex = reader.GetInt32(1);
                bi.bundlepath = reader.GetString(2).ToLower();
                bi.offset = reader.GetInt32(3);
                bi.size = reader.GetInt32(4);
                bi.isbase = reader.GetString(5) == "True";
                bi.isdelta = reader.GetString(6) == "True";
                bi.filepath = reader.GetString(8).ToLower();
                bi.incas = reader.GetString(10) == "True";
                switch (reader.GetString(11))
                {
                    case "b":
                        bi.isbasegamefile = true;
                        break;
                    case "u":
                        bi.isDLC = true;
                        break;
                    case "p":
                        bi.isPatch = true;
                        break;
                }
                result.Add(bi);
            }
            con.Close();
            return result.ToArray();
        }

        public static EBXInformation[] GetEBXInformation(ToolStripStatusLabel label)
        {
            List<EBXInformation> result = new List<EBXInformation>();
            SQLiteConnection con = GetConnection();
            con.Open();
            SQLiteDataReader reader = getAll("ebxlut", con);
            int count = 0;
            int animcount = 0;
            while (reader.Read())
            {
                EBXInformation ebx = new EBXInformation();
                ebx.ebxname = reader.GetString(1);
                ebx.sha1 = reader.GetString(2);
                ebx.basesha1 = reader.GetString(3);
                ebx.deltasha1 = reader.GetString(4);
                ebx.casPatchType = reader.GetInt32(5);
                ebx.guid = reader.GetString(6);
                ebx.bundlepath = reader.GetString(7);
                ebx.offset = reader.GetInt32(8);
                ebx.size = reader.GetInt32(9);
                ebx.isbase = reader.GetString(10) == "True";
                ebx.isdelta = reader.GetString(11) == "True";
                ebx.tocfilepath = reader.GetString(12);
                ebx.incas = reader.GetString(13) == "True";
                switch (reader.GetString(14))
                {
                    default:
                        ebx.isbasegamefile = true;
                        break;
                    case "u":
                        ebx.isDLC = true;
                        break;
                    case "p":
                        ebx.isPatch = true;
                        break;
                }
                result.Add(ebx);
                if (count++ % 10000 == 0)
                {
                    Application.DoEvents();
                    label.Text = "Refreshing... " + Helpers.GetWaiter(animcount++);
                }
            }
            con.Close();
            GC.Collect();
            return result.ToArray();
        }

        public static EBXInformation[] GetEBXInformationBySHA1(string sha1)
        {
            List<EBXInformation> result = new List<EBXInformation>();
            sha1 = sha1.ToUpper();
            SQLiteConnection con = GetConnection();
            con.Open();
            SQLiteDataReader reader = getAllWhere("ebxlut", "sha1 = '" + sha1 + "' or basesha1 = '" + sha1 + "' or deltasha1 = '" + sha1 + "' ", con);
            int count = 0;
            while (reader.Read())
            {
                EBXInformation ebx = new EBXInformation();
                ebx.ebxname = reader.GetString(1);
                ebx.sha1 = reader.GetString(2);
                ebx.basesha1 = reader.GetString(3);
                ebx.deltasha1 = reader.GetString(4);
                ebx.casPatchType = reader.GetInt32(5);
                ebx.guid = reader.GetString(6);
                ebx.bundlepath = reader.GetString(7);
                ebx.offset = reader.GetInt32(8);
                ebx.size = reader.GetInt32(9);
                ebx.isbase = reader.GetString(10) == "True";
                ebx.isdelta = reader.GetString(11) == "True";
                ebx.tocfilepath = reader.GetString(12);
                ebx.incas = reader.GetString(13) == "True";
                switch (reader.GetString(14))
                {
                    default:
                        ebx.isbasegamefile = true;
                        break;
                    case "u":
                        ebx.isDLC = true;
                        break;
                    case "p":
                        ebx.isPatch = true;
                        break;
                }
                result.Add(ebx);
                if (count++ % 1000 == 0)
                    Application.DoEvents();
            }
            con.Close();
            return result.ToArray();
        }

        public static EBXInformation[] GetEBXInformationByPath(string path)
        {
            List<EBXInformation> result = new List<EBXInformation>();
            path = path.ToLower();
            SQLiteConnection con = GetConnection();
            con.Open();
            SQLiteDataReader reader = getAllWhere("ebxlut", "lower(path) = '" + path + "'", con);
            int count = 0;
            while (reader.Read())
            {
                EBXInformation ebx = new EBXInformation();
                ebx.ebxname = reader.GetString(1);
                ebx.sha1 = reader.GetString(2);
                ebx.basesha1 = reader.GetString(3);
                ebx.deltasha1 = reader.GetString(4);
                ebx.casPatchType = reader.GetInt32(5);
                ebx.guid = reader.GetString(6);
                ebx.bundlepath = reader.GetString(7);
                ebx.offset = reader.GetInt32(8);
                ebx.size = reader.GetInt32(9);
                ebx.isbase = reader.GetString(10) == "True";
                ebx.isdelta = reader.GetString(11) == "True";
                ebx.tocfilepath = reader.GetString(12);
                ebx.incas = reader.GetString(13) == "True";
                switch (reader.GetString(14))
                {
                    default:
                        ebx.isbasegamefile = true;
                        break;
                    case "u":
                        ebx.isDLC = true;
                        break;
                    case "p":
                        ebx.isPatch = true;
                        break;
                }
                result.Add(ebx);
                if (count++ % 1000 == 0)
                    Application.DoEvents();
            }
            con.Close();
            return result.ToArray();
        }

        public static RESInformation[] GetRESInformationBySHA1(string sha1)
        {
            List<RESInformation> result = new List<RESInformation>();
            sha1 = sha1.ToUpper();
            SQLiteConnection con = GetConnection();
            con.Open();
            SQLiteDataReader reader = getAllJoined3Where("res", "bundles", "tocfiles", "bundle", "id", "tocfile", "id", "res.sha1='" + sha1 + "'", con);
            int count = 0;
            while (reader.Read())
            {
                RESInformation res = new RESInformation();
                res.resname = reader.GetString(0);
                res.sha1 = reader.GetString(1);
                res.rtype = reader.GetString(2);
                res.bundlepath = reader.GetString(6);
                res.offset = reader.GetInt32(7);
                res.size = reader.GetInt32(8);
                res.isbase = reader.GetString(9) == "True";
                res.isdelta = reader.GetString(10) == "True";
                res.tocfilepath = reader.GetString(12);
                res.incas = reader.GetString(14) == "True";
                switch (reader.GetString(15))
                {
                    default:
                        res.isbasegamefile = true;
                        break;
                    case "u":
                        res.isDLC = true;
                        break;
                    case "p":
                        res.isPatch = true;
                        break;
                }
                result.Add(res);
                if (count++ % 1000 == 0)
                    Application.DoEvents();
            }
            con.Close();
            return result.ToArray();
        }

        public static string[] GetUsedRESTypes()
        {
            List<string> result = new List<string>();
            SQLiteConnection con = GetConnection();
            con.Open();
            SQLiteCommand command = new SQLiteCommand("SELECT DISTINCT rtype FROM res", con);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
                result.Add(reader.GetString(0));
            con.Close();
            return result.ToArray();
        }

        public static RESInformation[] GetRESInformationsByType(string type)
        {
            List<RESInformation> result = new List<RESInformation>();
            SQLiteConnection con = GetConnection();
            con.Open();
            SQLiteDataReader reader = getAllJoined3Where("res", "bundles", "tocfiles", "bundle", "id", "tocfile", "id", "res.rtype='" + type + "'", con);
            int count = 0;
            while (reader.Read())
            {
                RESInformation res = new RESInformation();
                res.resname = reader.GetString(0);
                res.sha1 = reader.GetString(1);
                res.rtype = reader.GetString(2);
                res.bundlepath = reader.GetString(6);
                res.offset = reader.GetInt32(7);
                res.size = reader.GetInt32(8);
                res.isbase = reader.GetString(9) == "True";
                res.isdelta = reader.GetString(10) == "True";
                res.tocfilepath = reader.GetString(12);
                res.incas = reader.GetString(14) == "True";
                   switch (reader.GetString(15))
                {
                    default:
                        res.isbasegamefile = true;
                        break;
                    case "u":
                        res.isDLC = true;
                        break;
                    case "p":
                        res.isPatch = true;
                        break;
                }
                result.Add(res);
                if (count++ % 1000 == 0)
                    Application.DoEvents();
            }
            con.Close();
            return result.ToArray();
        }

        public static TextureInformation[] GetTextureInformations()
        {
            List<TextureInformation> result = new List<TextureInformation>();
            SQLiteConnection con = GetConnection();
            con.Open();
            SQLiteDataReader reader = getAllWhere("res", "rtype = 'A654495C'", con);
            int count = 0;
            while (reader.Read())
            {
                if (count++ % 1000 == 0)
                    Application.DoEvents();
                TextureInformation ti = new TextureInformation();
                ti.name = reader.GetString(0);
                ti.sha1 = Helpers.HexStringToByteArray(reader.GetString(1));
                ti.bundleIndex = reader.GetInt32(3);
                result.Add(ti);
            }
            con.Close();
            return result.ToArray();
        }

        
        public static TextureInformation[] GetTextureInformationsById(string id)
        {
            List<TextureInformation> result = new List<TextureInformation>();
            SQLiteConnection con = GetConnection();
            con.Open();
            SQLiteDataReader reader = getAllWhere("res", "rtype = 'A654495C' and lower(name) = '" + id.ToLower() + "'", con);
            int count = 0;
            while (reader.Read())
            {
                if (count++ % 1000 == 0)
                    Application.DoEvents();
                TextureInformation ti = new TextureInformation();
                ti.name = reader.GetString(0);
                ti.sha1 = Helpers.HexStringToByteArray(reader.GetString(1));
                ti.bundleIndex = reader.GetInt32(3);
                result.Add(ti);
            }
            con.Close();
            return result.ToArray();
        }

        public static TOCInformation GetTocInformationByIndex(int index)
        {
            TOCInformation res = new TOCInformation();
            SQLiteConnection con = GetConnection();
            con.Open();
            SQLiteDataReader reader = getAllWhere("tocfiles", "id = " + index, con);
            if (reader.Read())
            {
                res.index = index;
                res.path = reader.GetString(1);
                res.md5 = reader.GetString(2);
                res.incas = reader.GetString(3) == "True";
                res.type = reader.GetString(4);
            }
            con.Close();
            return res;
        }

        public static ChunkInformation GetChunkInformationById(byte[] id)
        {
            ChunkInformation res = new ChunkInformation();
            res.id = id;
            res.bundleIndex = -1;
            SQLiteConnection con = GetConnection();
            con.Open();
            SQLiteDataReader reader = getAllWhere("chunks", "id = '" + Helpers.ByteArrayToHexString(id) + "'", con);
            if (reader.Read())
            {
                res.id = Helpers.HexStringToByteArray(reader.GetString(0));
                res.sha1 = Helpers.HexStringToByteArray(reader.GetString(1));
                res.bundleIndex = reader.GetInt32(2);
            }
            con.Close();
            return res;
        }

        public static ChunkInformation[] GetChunkInformationBySHA1(string sha1)
        {
            sha1 = sha1.ToUpper();
            List<ChunkInformation> res = new List<ChunkInformation>();
            SQLiteConnection con = GetConnection();
            con.Open();
            SQLiteDataReader reader = getAllWhere("chunks", "sha1 = '" + sha1 + "'", con);
            if (reader.Read())
            {
                ChunkInformation ci = new ChunkInformation();
                ci.id = Helpers.HexStringToByteArray(reader.GetString(0));
                ci.sha1 = Helpers.HexStringToByteArray(reader.GetString(1));
                ci.bundleIndex = reader.GetInt32(2);
                res.Add(ci);
            }
            con.Close();
            return res.ToArray();
        }

        public static ChunkInformation GetGlobalChunkInformationById(byte[] id)
        {
            ChunkInformation res = new ChunkInformation();
            res.id = id;
            res.bundleIndex = -1;
            SQLiteConnection con = GetConnection();
            con.Open();
            SQLiteDataReader reader = getAllWhere("globalchunks", "id = '" + Helpers.ByteArrayToHexString(id) + "'", con);
            if (reader.Read())
            {
                res.id = Helpers.HexStringToByteArray((string)reader["id"]);
                res.sha1 = Helpers.HexStringToByteArray((string)reader["sha1"]);
            }
            con.Close();
            return res;
        }

        #endregion

        #region initial scan stuff

        public static void StartScan(string path)
        {
            Debug.LogLn("Starting Scan...");
            Stopwatch sp = new Stopwatch();
            sp.Start();
            try
            {
                GlobalStuff.AssignSetting("isNew", "0");
                GlobalStuff.settings.Add("gamepath", path);
                SaveSettings();
                ScanFiles();
                ScanTOCsForBundles();
            }
            catch (Exception ex)
            {
                sp.Stop();
                Debug.LogLn("\n\n====ERROR======\nTime : " + sp.Elapsed.ToString() + "\n" + ex.Message);
            }
            sp.Stop();
            Debug.LogLn("\n\n===============\nTime : " + sp.Elapsed.ToString() + "\n");
        }
        
        private static void ScanFiles()
        {
            Debug.LogLn("Saving file paths into db...");
            SQLiteConnection con = GetConnection();
            con.Open();
            var transaction = con.BeginTransaction();
            string[] files = Directory.GetFiles(GlobalStuff.FindSetting("gamepath"), "*.sb", SearchOption.AllDirectories);
            Debug.LogLn("SB files...");
            foreach (string file in files)
                if (!file.ToLower().Contains("\\update\\"))
                    AddSBFile(file, TYPE_BASEGAME, con);
                else if (!file.ToLower().Contains("\\update\\patch\\"))
                    AddSBFile(file, TYPE_UPDATE, con);
                else
                    AddSBFile(file, TYPE_PATCH, con);
            transaction.Commit();
            transaction = con.BeginTransaction();
            Debug.LogLn("TOC files...");
            files = Directory.GetFiles(GlobalStuff.FindSetting("gamepath"), "*.toc", SearchOption.AllDirectories);
            foreach (string file in files)
                if (!file.ToLower().Contains("\\update\\"))
                    AddTOCFile(file, TYPE_BASEGAME, con);
                else if (!file.ToLower().Contains("\\update\\patch\\"))
                    AddTOCFile(file, TYPE_UPDATE, con);
                else
                    AddTOCFile(file, TYPE_PATCH, con);
            transaction.Commit();
            con.Close();
        }
        
        private static void ScanTOCsForBundles()
        {
            Debug.LogLn("Saving bundles into db...");
            SQLiteConnection con = GetConnection();
            con.Open();
            SQLiteDataReader reader = getAll("tocfiles", con);
            StringBuilder sb = new StringBuilder();
            List<string> files = new List<string>();
            List<int> fileids = new List<int>();
            while (reader.Read())
            {
                fileids.Add(reader.GetInt32(0));
                files.Add(reader.GetString(1));
            }
            int counter = 0;
            Stopwatch sp = new Stopwatch();
            sp.Start();
            foreach (string file in files)
            {
                counter++;
                Debug.LogLn("Opening " + file + " ...");
                TOCFile tocfile = new TOCFile(file);
                int counter2 = 0;
                
                foreach (TOCFile.TOCBundleInfoStruct info in tocfile.bundles)
                {
                    counter2++;
                    Bundle b;
                    string pathsb = Helpers.GetFileNameWithOutExtension(file) + ".sb";
                    FileStream fs = new FileStream(pathsb, FileMode.Open, FileAccess.Read);
                    fs.Seek(0, SeekOrigin.End);
                    long filesize = fs.Position;
                    if (info.offset > filesize)
                    {
                        fs.Close();
                        continue;
                    }
                    fs.Seek(info.offset, 0);
                    byte[] buff = new byte[info.size];
                    fs.Read(buff, 0, info.size);
                    if (tocfile.iscas)
                    {
                        if (buff[0] != 0x82)
                            continue;
                        List<BJSON.Entry> list = new List<BJSON.Entry>();
                        BJSON.ReadEntries(new MemoryStream(buff), list);
                        b = Bundle.Create(list[0]);
                    }
                    else
                    {
                        uint magic = BitConverter.ToUInt32(buff, 4);
                        if (magic != 0xd58e799d)
                            continue;
                        b = Bundle.Create(buff, true);
                    }
                    string log = " adding bundle: " + (counter2) + "/" + tocfile.bundles.Count + " ";
                    if (info.isbase) log += "ISBASEG ";
                    if (info.isdelta) log += "ISDELTA ";
                    log += "ID: " + info.id;
                    Debug.Log(log, true);
                    AddBundle(fileids[counter - 1], tocfile.iscas, b, info, con);
                }
                counter2 = 0;
                var transaction = con.BeginTransaction();
                foreach (TOCFile.TOCChunkInfoStruct info in tocfile.chunks)
                {
                    AddGlobalChunk(fileids[counter - 1], info.id, info.sha1, info.offset, info.size, con);
                    Debug.LogLn(" adding chunk: " + (counter2) + "/" + tocfile.chunks.Count + " " + Helpers.ByteArrayToHexString(info.id), counter2 % 1000 == 0);
                    if (counter2 % 1000 == 0)
                    {
                        transaction.Commit();
                        transaction = con.BeginTransaction();
                    }
                    counter2++;
                }
                transaction.Commit();
                long elapsed = sp.ElapsedMilliseconds;
                long ETA = ((elapsed / counter) * files.Count);
                TimeSpan ETAt = TimeSpan.FromMilliseconds(ETA);
                Debug.LogLn((counter) + "/" + files.Count + " files done." + " - Elapsed: " + sp.Elapsed.ToString() + " ETA: " + ETAt.ToString());
            }
            con.Close();
        }

        #endregion
    }
}
