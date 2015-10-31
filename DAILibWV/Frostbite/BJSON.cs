using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DAILibWV.Frostbite
{
    public static class BJSON
    {
        public class Entry
        {
            public int type;
            public long offset;
            public List<Field> fields;
            public string type87name;
            public Field FindField(string name)
            {
                foreach (Field f in fields)
                    if (f.fieldname == name)
                        return f;
                    else if (f.type == 1)
                        foreach (Entry e in (List<Entry>)f.data)
                        {
                            Field result = e.FindField(name);
                            if (result != null)
                                return result;
                        }
                return null;
            }
            public int FindFieldIndex(string name)
            {
                for (int i = 0; i < fields.Count; i++)
                    if (fields[i].fieldname == name)
                        return i;
                return -1;
            }
            public bool RemoveField(string name)
            {
                for (int i = 0; i < fields.Count; i++)
                    if (fields[i].fieldname == name)
                    {
                        fields.RemoveAt(i);
                        return true;
                    }
                return false;
            }
        }

        public class Field
        {
            public byte type;
            public string fieldname;
            public List<Field> fields;
            public object data;
            public Field()
            {
            }
            public Field(byte t, string n, object d)
            {
                type = t;
                fieldname = n;
                data = d;
            }
        }    

        public static void ReadEntries(Stream s, List<Entry> list)
        {
            long len = s.Length;
            while (s.Position < len)
            {
                Entry e = new Entry();
                e.offset = s.Position;
                byte type1 = (byte)s.ReadByte();
                ulong size;
                e.type = type1;
                switch (type1)
                {
                    case 0x82:
                        size = Helpers.ReadLEB128(s);
                        long pos = s.Position;
                        e.fields = new List<Field>();
                        long lastpos = pos - 1;
                        while (s.Position - pos < (long)size && lastpos != s.Position)
                        {
                            lastpos = s.Position;
                            Field currentField = ReadField(s);
                            if (currentField.type != 0)
                                e.fields.Add(currentField);
                        }
                        list.Add(e);
                        break;
                    case 0x87:
                        size = Helpers.ReadLEB128(s);
                        string res = "";
                        for (int i = 0; i < (int)size; i++)
                        {
                            byte b = (byte)s.ReadByte();
                            if (b != 0)
                                res += (char)b;
                        }
                        e.type87name = res;
                        list.Add(e);
                        break;
                    default: return;
                }
            }
        }

        public static Field ReadField(Stream s)
        {
            Field result = new Field();
            result.fields = null;
            result.type = 0;
            byte type = (byte)s.ReadByte();
            if (type == 0)
                return result;
            string fieldname = Helpers.ReadNullString(s);
            result.type = type;
            result.fieldname = fieldname;
            ParseBinaryJSONEntry(s, result);
            return result;
        }

        private static void ParseBinaryJSONEntry(Stream s, Field result)
        {
            ulong size;
            ulong count;
            byte[] buff;
            switch (result.type)
            {
                case 0x01:
                    size = Helpers.ReadLEB128(s);
                    result.data = new List<Entry>();
                    ReadEntries(s, (List<Entry>)result.data);
                    break;
                case 0x07:
                    count = Helpers.ReadLEB128(s);
                    string res = "";
                    for (int i = 0; i < (int)count; i++)
                    {
                        byte b = (byte)s.ReadByte();
                        if (b != 0)
                            res += (char)b;
                    }
                    result.data = res;
                    break;
                case 0x06:
                    result.data = (s.ReadByte() == 1);
                    break;
                case 0x08:
                    buff = new byte[4];
                    s.Read(buff, 0, 4);
                    result.data = buff;
                    break;
                case 0x09:
                    buff = new byte[8];
                    s.Read(buff, 0, 8);
                    result.data = buff;
                    break;
                case 0xf:
                    buff = new byte[0x10];
                    s.Read(buff, 0, 0x10);
                    result.data = buff;
                    break;
                case 0x10:
                    buff = new byte[0x14];
                    s.Read(buff, 0, 0x14);
                    result.data = buff;
                    break;
                case 0x02:
                    size = Helpers.ReadLEB128(s);
                    long tpos = s.Position;
                    result.data = new List<Field>();
                    while (s.Position - tpos < (long)size)
                    {
                        Field f = ReadField(s);
                        if (f.type != 0)
                            ((List<Field>)result.data).Add(f);
                    }
                    s.ReadByte();
                    break;
                case 0x13:
                    size = Helpers.ReadLEB128(s);
                    buff = new byte[size];
                    s.Read(buff, 0, (int)size);
                    result.data = buff;
                    break;
            }
        }

        public static void WriteEntry(Stream s, Entry e)
        {
            Helpers.BinaryWriter7Bit w = new Helpers.BinaryWriter7Bit(s);
            switch (e.type)
            {
                case 0x82:
                    s.WriteByte(0x82);
                    MemoryStream m = new MemoryStream();
                    foreach (Field f in e.fields)
                        WriteField(m, f);
                    m.WriteByte(0);
                    w.Write7BitEncodedInt((int)m.Length);
                    s.Write(m.ToArray(), 0, (int)m.Length);
                    break;
                case 0x87:
                    s.WriteByte(0x87);
                    w.Write7BitEncodedInt(e.type87name.Length + 1);
                    Helpers.WriteNullString(s, e.type87name);
                    break;
            }
        }

        public static void WriteField(Stream s, Field f)
        {
            if (f.type == 0)
            {
                s.WriteByte(0);
                return;
            }
            s.WriteByte(f.type);
            Helpers.WriteNullString(s, f.fieldname);
            Helpers.BinaryWriter7Bit w = new Helpers.BinaryWriter7Bit(s);
            MemoryStream m;
            switch (f.type)
            {
                case 0x01:
                    List<Entry> listb = (List<Entry>)f.data;
                    m = new MemoryStream();
                    foreach (Entry e in listb)
                        WriteEntry(m, e);
                    m.WriteByte(0);
                    w.Write7BitEncodedInt((int)m.Length);
                    s.Write(m.ToArray(), 0, (int)m.Length);
                    break;
                case 0x07:
                    w.Write7BitEncodedInt((int)((string)f.data).Length + 1);
                    Helpers.WriteNullString(s, (string)f.data);
                    break;
                case 0x06:
                    s.WriteByte(((bool)f.data) ? (byte)1 : (byte)0);
                    break;
                case 0x08:
                    s.Write((byte[])f.data, 0, 4);
                    break;
                case 0x09:
                    s.Write((byte[])f.data, 0, 8);
                    break;
                case 0xf:
                case 0x10:
                    s.Write((byte[])f.data, 0, (int)((byte[])f.data).Length);
                    break;
                case 0x02:
                    List<Field> listf = (List<Field>)f.data;
                    m = new MemoryStream();
                    foreach (Field e in listf)
                        WriteField(m, e);
                    m.WriteByte(0);
                    w.Write7BitEncodedInt((int)m.Length);
                    s.Write(m.ToArray(), 0, (int)m.Length);
                    break;
                case 0x13:
                    w.Write7BitEncodedInt((int)((byte[])f.data).Length);
                    s.Write((byte[])f.data, 0, (int)((byte[])f.data).Length);
                    break;
            }
            if (f.fields != null)
                foreach (Field subfield in f.fields)
                    WriteField(s, subfield);
        }

        public static TreeNode MakeEntry(TreeNode t, Entry e)
        {
            if (e.type == 0x87)
            {
                t.Text = "[87] (" + e.type87name + ")";
                return t;
            }
            if (e.type == 0x82)
            {
                int count = 0;
                foreach (Field f in e.fields)
                    if (count != -1 && f.type != 0)
                        t.Nodes.Add(MakeField(new TreeNode((count++) + " : " +  f.fieldname), f));
            }
            return t;
        }

        public static TreeNode MakeField(TreeNode t, Field f)
        {
            byte[] buff;
            if (f.fields != null)
                foreach (Field subfield in f.fields)
                    t.Nodes.Add(MakeField(new TreeNode(subfield.fieldname), subfield));
            else
                switch (f.type)
                {
                    case 1:
                        List<Entry> listb = (List<Entry>)f.data;
                        int count = 0;
                        foreach (Entry e in listb)
                            t.Nodes.Add(MakeEntry(new TreeNode(count++ + " : [" + e.type.ToString("X") + "][@0x" + e.offset.ToString("X") + "]"), e));
                        break;
                    case 2:
                        List<Field> listf = (List<Field>)f.data;
                        foreach (Field e in listf)
                            t.Nodes.Add(MakeField(new TreeNode(e.fieldname), e));
                        break;
                    case 6:
                        bool b = (bool)f.data;
                        t.Nodes.Add(new TreeNode(b.ToString()) { Tag = f });
                        break;
                    case 7:
                        string s = (string)f.data;
                        t.Nodes.Add(new TreeNode(s) { Tag = f });
                        break;
                    case 8:
                        buff = (byte[])f.data;
                        buff.Reverse();
                        if (t.Text == "resType")
                        {
                            string nodeTitle = string.Format("0x{0} {1}", BitConverter.ToUInt32(buff, 0).ToString("X"), Helpers.GetResType(BitConverter.ToUInt32(buff, 0)));
                            t.Nodes.Add(new TreeNode(nodeTitle) { Tag = f });
                        }
                        else
                            t.Nodes.Add(new TreeNode("0x" + BitConverter.ToUInt32(buff, 0).ToString("X")) { Tag = f });
                        break;
                    case 9:
                        buff = (byte[])f.data;
                        buff.Reverse();
                        t.Nodes.Add(new TreeNode("0x" + BitConverter.ToUInt64(buff, 0).ToString("X")) { Tag = f });
                        break;
                    case 0xf:
                        buff = (byte[])f.data;
                        buff.Reverse();
                        t.Nodes.Add(new TreeNode("0x" + BitConverter.ToUInt64(buff, 8).ToString("X")) { Tag = f });
                        t.Nodes.Add(new TreeNode("0x" + BitConverter.ToUInt64(buff, 0).ToString("X")) { Tag = f });
                        break;
                    case 0x10:
                    case 0x13:
                        buff = (byte[])f.data;
                        if (f.fieldname != "payload")
                        {
                            StringBuilder res = new StringBuilder();
                            foreach (byte bb in buff)
                                res.Append(bb.ToString("X2"));
                            t.Nodes.Add(new TreeNode(res.ToString()) { Tag = f });
                        }
                        else
                        {
                            t.Nodes.Add(Encoding.Default.GetString(buff));
                        }
                        break;
                    default:
                        break;
                }
            return t;
        }
    }
}
