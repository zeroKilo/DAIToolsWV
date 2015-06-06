using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DAILibWV.Frostbite
{

    public class EBXFile
    {
        public struct HeaderStruct
        {
            public int magic;
            public int absStringOffset;
            public int lenStringToEOF;
            public int numGUID;
            public ushort numInstanceRepeater;
            public ushort numGUIDRepeater;
            public ushort unknown;
            public ushort numComplex;
            public ushort numField;
            public ushort lenName;
            public int lenString;
            public int numArrayRepeater;
            public int lenPayload;
            public int _arraySectionstart;
        }

        public struct ExternalGUIDStruct
        {
            public byte[] GUID1;
            public byte[] GUID2;
        }

        public struct KeyWordDicStruct
        {
            public string keyword;
            public int hash;
            public int offset;
        }

        public struct FieldDescriptor
        {
            public string _name;
            public int _index;
            public int hash;
            public ushort type;
            public ushort reference;
            public int offset;
            public int secondaryOffset;
        }

        public struct ComplexTypeDescriptor
        {
            public string _name;
            public int _index;
            public int hash;
            public int fieldStartIndex;
            public byte numField;
            public byte alignment;
            public ushort type;
            public ushort size;
            public ushort secondarySize;
        }

        public struct InstanceRepeater
        {
            public ushort complexIndex;
            public ushort repetitions;
        }

        public struct ArrayRepeater
        {
            public int offset;
            public int repetitions;
            public int complexIndex;
        }

        public struct Field
        {
            public int offset;
            public FieldDescriptor Descriptor;
            public object data;
        }

        public struct ComplexType
        {
            public List<byte[]> _wtfhash;
            public int offset;
            public ComplexTypeDescriptor Descriptor;
            public List<Field> Fields;
        }

        public struct InstanceStruct
        {
            public byte[] GUID;
            public string name;
            public ComplexType field;
        }


        public HeaderStruct Header;
        public byte[] GUID;
        public List<ExternalGUIDStruct> externalGUIDs;
        public List<KeyWordDicStruct> keyWordDic;
        public List<FieldDescriptor> fieldDescriptors;
        public List<ComplexTypeDescriptor> complexTypeDescriptors;
        public List<InstanceRepeater> instanceRepeaterList;
        public List<ArrayRepeater> arrayRepeaterList;
        public byte[] keywordarea;
        public List<string> typeNames;
        public List<InstanceStruct> instancesList;

        public EBXFile(Stream s)
        {
            try
            {
                ReadHeader(s);
                GUID = new byte[16];
                s.Read(GUID, 0, 16);
                ulong zero = Helpers.ReadULong(s);
                ReadExternalGUIDs(s);
                ReadKeyWords(s);
                ReadFieldDescriptors(s);
                ReadComplexTypeDescriptors(s);
                ReadInstanceRepeaters(s);
                ReadArrayRepeaterList(s);
                ReadInstanceNames(s);
                ReadInstances(s);
            }
            catch (Exception)
            {
            }
        }

        public void ReadHeader(Stream s)
        {
            Header = new HeaderStruct();
            Header.magic = Helpers.ReadInt(s);
            if (Header.magic != 0x0fb2d1ce)
                return;
            Header.absStringOffset = Helpers.ReadInt(s);
            Header.lenStringToEOF = Helpers.ReadInt(s);
            Header.numGUID = Helpers.ReadInt(s);
            Header.numInstanceRepeater = Helpers.ReadUShort(s);
            Header.numGUIDRepeater = Helpers.ReadUShort(s);
            Header.unknown = Helpers.ReadUShort(s);
            Header.numComplex = Helpers.ReadUShort(s);
            Header.numField = Helpers.ReadUShort(s);
            Header.lenName = Helpers.ReadUShort(s);
            Header.lenString = Helpers.ReadInt(s);
            Header.numArrayRepeater = Helpers.ReadInt(s);
            Header.lenPayload = Helpers.ReadInt(s);
            Header._arraySectionstart = Header.absStringOffset + Header.lenString + Header.lenPayload;
        }

        public void ReadExternalGUIDs(Stream s)
        {
            externalGUIDs = new List<ExternalGUIDStruct>();
            for (int i = 0; i < Header.numGUID; i++)
            {
                ExternalGUIDStruct ex = new ExternalGUIDStruct();
                ex.GUID1 = new byte[16];
                s.Read(ex.GUID1, 0, 16);
                ex.GUID2 = new byte[16];
                s.Read(ex.GUID2, 0, 16);
                externalGUIDs.Add(ex);
            }
        }

        public void ReadKeyWords(Stream s)
        {
            keywordarea = new byte[Header.lenName];
            s.Read(keywordarea, 0, Header.lenName);
            MemoryStream m = new MemoryStream(keywordarea);
            m.Seek(0, 0);
            keyWordDic = new List<KeyWordDicStruct>();
            long start = m.Position;
            while (m.Position < m.Length)
            {
                long pos = m.Position;
                string keyword = Helpers.ReadNullString(m);
                int hash = Helpers.HashFNV1(keyword);
                bool found = false;
                foreach (KeyWordDicStruct st in keyWordDic)
                    if (st.hash == hash)
                    {
                        found = true;
                        break;
                    }
                if (!found)
                {
                    KeyWordDicStruct st = new KeyWordDicStruct();
                    st.keyword = keyword;
                    st.hash = hash;
                    st.offset = (int)(pos - start);
                    keyWordDic.Add(st);
                }
            }

        }

        public void ReadFieldDescriptors(Stream s)
        {
            fieldDescriptors = new List<FieldDescriptor>();
            for (int i = 0; i < Header.numField; i++)
            {
                FieldDescriptor f = new FieldDescriptor();
                f.hash = Helpers.ReadInt(s);
                foreach (KeyWordDicStruct key in keyWordDic)
                    if (key.hash == f.hash)
                    {
                        f._name = key.keyword;
                        break;
                    }
                f.type = Helpers.ReadUShort(s);
                f.reference = Helpers.ReadUShort(s);
                f.offset = Helpers.ReadInt(s);
                f.secondaryOffset = Helpers.ReadInt(s);
                if (f._name == "$")
                    f.offset -= 8;
                f._index = i;
                fieldDescriptors.Add(f);
            }
        }

        public void ReadComplexTypeDescriptors(Stream s)
        {
            complexTypeDescriptors = new List<ComplexTypeDescriptor>();
            for (int i = 0; i < Header.numComplex; i++)
            {
                ComplexTypeDescriptor f = new ComplexTypeDescriptor();
                f.hash = Helpers.ReadInt(s);
                foreach (KeyWordDicStruct key in keyWordDic)
                    if (key.hash == f.hash)
                    {
                        f._name = key.keyword;
                        break;
                    }
                f.fieldStartIndex = Helpers.ReadInt(s);
                f.numField = (byte)s.ReadByte();
                f.alignment = (byte)s.ReadByte();
                f.type = Helpers.ReadUShort(s);
                f.size = Helpers.ReadUShort(s);
                f.secondarySize = Helpers.ReadUShort(s);
                f._index = i;
                complexTypeDescriptors.Add(f);
            }
        }

        public void ReadInstanceRepeaters(Stream s)
        {
            instanceRepeaterList = new List<InstanceRepeater>();
            for (int i = 0; i < Header.numInstanceRepeater; i++)
            {
                InstanceRepeater ir = new InstanceRepeater();
                ir.complexIndex = Helpers.ReadUShort(s);
                ir.repetitions = Helpers.ReadUShort(s);
                instanceRepeaterList.Add(ir);
            }
        }

        public void ReadArrayRepeaterList(Stream s)
        {
            arrayRepeaterList = new List<ArrayRepeater>();
            for (int i = 0; i < Header.numArrayRepeater + 1; i++)
            {
                ArrayRepeater ar = new ArrayRepeater();
                ar.offset = Helpers.ReadInt(s);
                ar.repetitions = Helpers.ReadInt(s);
                ar.complexIndex = Helpers.ReadInt(s);
                arrayRepeaterList.Add(ar);
            }
        }

        public void ReadInstanceNames(Stream s)
        {
            s.Seek(Header.absStringOffset, 0);
            byte[] buff = new byte[Header.lenString];
            s.Read(buff, 0, Header.lenString);
            string t = Helpers.ByteArrayAsString(buff);
            typeNames = new List<string>(t.Split(new char[] { '\0' }, StringSplitOptions.RemoveEmptyEntries));
        }

        public void ReadInstances(Stream s)
        {
            s.Seek(Header.absStringOffset + Header.lenString, 0);
            instancesList = new List<InstanceStruct>();
            int NonGuidIndex = 0;
            for (int i = 0; i < instanceRepeaterList.Count; i++)
            {
                InstanceRepeater curRep = instanceRepeaterList[i];
                for (int j = 0; j < curRep.repetitions; j++)
                {
                    int align = complexTypeDescriptors[curRep.complexIndex].alignment;
                    while (s.Position % align != 0)
                        s.ReadByte();
                    InstanceStruct instance = new InstanceStruct();
                    if (i < Header.numGUIDRepeater)
                    {
                        instance.GUID = new byte[16];
                        s.Read(instance.GUID, 0, 16);
                    }
                    else
                    {
                        instance.GUID = new byte[16];
                        MemoryStream m = new MemoryStream();
                        Helpers.WriteLEInt(m, NonGuidIndex);
                        byte[] b = m.ToArray();
                        for (int k = 0; k < 4; k++)
                            instance.GUID[12 + k] = b[k];
                        NonGuidIndex++;
                    }
                    instance.name = typeNames[i];
                    instance.field = ReadComplexType(s, curRep.complexIndex, new Field(), true);
                    instancesList.Add(instance);
                }
            }
        }

        public ComplexType ReadComplexType(Stream s, int ComplexIndex, Field parent, bool isInstance = false)
        {
            ComplexType result = new ComplexType();
            result.offset = (int)s.Position;
            ComplexTypeDescriptor desc = complexTypeDescriptors[ComplexIndex];
            result.Descriptor = desc;
            result.Fields = new List<Field>();
            //if (desc.type == 0x35)
            //{

            //}
            {
                ////int ObfuscationShift = (isInstance && desc.alignment == 4) ? 8 : 0;

                //if (desc.numField == 0)
                //{
                //    s.Seek(desc.size, SeekOrigin.Current);
                //    int index = Helpers.ReadInt(s);
                //    ArrayRepeater arr = arrayRepeaterList[index];
                //    s.Seek(Header._arraySectionstart + arr.offset, 0);
                //    result._wtfhash = new List<byte[]>();
                //    for (int i = 0; i < arr.repetitions; i++)
                //    {
                //        byte[] buff = new byte[0x14];
                //        s.Read(buff, 0, 0x14);
                //        result._wtfhash.Add(buff);
                //    }
                //}
                for (int i = desc.fieldStartIndex; i < desc.fieldStartIndex + desc.numField; i++)
                {
                    s.Seek(result.offset + fieldDescriptors[i].offset, 0);
                    result.Fields.Add(ReadField(s, i));
                }
                s.Seek(result.offset + desc.size, 0);
            }
            return result;
        }

        public Field ReadField(Stream s, int Index)
        {
            Field result = new Field();
            result.offset = (int)s.Position;
            FieldDescriptor desc = fieldDescriptors[Index];
            result.Descriptor = desc;
            int offset, index;
            ComplexTypeDescriptor cdesc;
            switch (desc.type)
            {
                case 0:
                case 0x41:
                case 0x29:
                case 0xd029:
                case 0x8029:
                    result.data = ReadComplexType(s, desc.reference, result);
                    break;
                case 0x407d:
                    index = Helpers.ReadInt(s);
                    result.data = keyWordDic.ElementAt(index).keyword;
                    break;
                case 0x409d:
                    offset = Helpers.ReadInt(s);
                    result.data = "";
                    if (offset != -1)
                        foreach (KeyWordDicStruct key in keyWordDic)
                            if (key.offset == offset)
                                result.data = key.keyword;
                    break;
                case 0xc0ad:
                case 0xc0bd:
                case 0xc0cd:
                    result.data = (byte)s.ReadByte();
                    break;
                case 0xcdd:
                case 0xced:
                    result.data = Helpers.ReadShort(s);
                    break;
                case 0x35:
                case 0xc0fd:
                    result.data = Helpers.ReadInt(s);
                    break;
                case 0xc15d:
                case 0x417d:
                case 0xc10d:
                    result.data = Helpers.ReadLong(s);
                    break;
                case 0xc13d:
                    result.data = Helpers.ReadFloat(s);
                    break;
                case 0x89:
                case 0xc089:
                    offset = Helpers.ReadInt(s);
                    cdesc = complexTypeDescriptors[desc.reference];
                    string value = "";
                    if(cdesc.numField != 0)
                        for(int i = cdesc.fieldStartIndex; i<cdesc.fieldStartIndex + cdesc.numField;i++)
                            if (fieldDescriptors[i].offset == offset)
                            {
                                value = fieldDescriptors[i]._name;
                                break;
                            }
                    result.data = value;
                    break;
                    //index = Helpers.ReadInt(s);
                    //cdesc = complexTypeDescriptors[desc.reference];
                    //ArrayRepeater arep = arrayRepeaterList[index];
                    //s.Seek(Header._arraySectionstart + arep.offset, 0);
                    //ComplexType acomp = new ComplexType();
                    //acomp.offset = (int)s.Position;
                    //acomp.Descriptor = cdesc;
                    //acomp.Fields = new List<Field>();
                    //for (int i = 0; i < arep.repetitions; i++)
                    //    acomp.Fields.Add(ReadField(s, cdesc.fieldStartIndex));
                    //result.data = acomp;
                    //break;
            }
            return result;
        }

        public string HeaderToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Header\n");
            sb.Append("Magic               : 0x" + Header.magic.ToString("X8") + "\n");
            sb.Append("AbsStringOffset     : 0x" + Header.absStringOffset.ToString("X8") + "\n");
            sb.Append("LenStringToEOF      : 0x" + Header.lenStringToEOF.ToString("X8") + "\n");
            sb.Append("NumGUID             : 0x" + Header.numGUID.ToString("X8") + "\n");
            sb.Append("NumInstanceRepeater : 0x" + Header.numInstanceRepeater.ToString("X4") + "\n");
            sb.Append("NumGUIDRepeater     : 0x" + Header.numGUIDRepeater.ToString("X4") + "\n");
            sb.Append("Unknown             : 0x" + Header.unknown.ToString("X4") + "\n");
            sb.Append("NumComplex          : 0x" + Header.numComplex.ToString("X4") + "\n");
            sb.Append("NumField            : 0x" + Header.numField.ToString("X4") + "\n");
            sb.Append("lenName             : 0x" + Header.lenName.ToString("X4") + "\n");
            sb.Append("LenString           : 0x" + Header.lenString.ToString("X8") + "\n");
            sb.Append("NumArrayRepeater    : 0x" + Header.numArrayRepeater.ToString("X8") + "\n");
            sb.Append("LenPayload          : 0x" + Header.lenPayload.ToString("X8") + "\n");
            sb.Append("ArraySectionstart   : 0x" + Header._arraySectionstart.ToString("X8") + "\n");
            sb.Append("\nGUID\n" + Helpers.ByteArrayToHexString(GUID));
            return sb.ToString();
        }

        public TreeNode InstanceTotree(TreeNode t, InstanceStruct ins, int index)
        {
            TreeNode result = new TreeNode(ins.name);
            TreeNode t2 = new TreeNode("GUID");
            t2.Nodes.Add(Helpers.ByteArrayToHexString(ins.GUID));
            result.Nodes.Add(t2);
            result.Nodes.Add(MakeComplexFieldNode(ins.field));
            t.Nodes.Add(result);
            return t;
        }

        public TreeNode MakeComplexFieldNode(ComplexType cfield)
        {
            TreeNode result = new TreeNode("CT[" + cfield.Descriptor._index.ToString("X") + "][" + cfield.Descriptor.type.ToString("X4") + "]: " + cfield.Descriptor._name + "(size 0x" + cfield.Descriptor.size.ToString("X4") + " numfields = " + cfield.Descriptor.numField + ")");
            TreeNode wtf = new TreeNode("WTFSHA1s");
            if (cfield._wtfhash != null)
                foreach (byte[] buff in cfield._wtfhash)
                    wtf.Nodes.Add(Helpers.ByteArrayToHexString(buff));
            if (wtf.Nodes.Count != 0)
                result.Nodes.Add(wtf);
            foreach (Field f in cfield.Fields)
                result.Nodes.Add(MakeFieldNode(f));
            return result;
        }

        public TreeNode MakeFieldNode(Field field)
        {
            TreeNode result = new TreeNode("F[" + field.Descriptor._index.ToString("X") + "][" + field.Descriptor.type.ToString("X4") + "]: " + field.Descriptor._name + " (offset 0x" + field.Descriptor.offset.ToString("X4") + " 2nd offset 0x" + field.Descriptor.secondaryOffset.ToString("X8") + ")");
            switch (field.Descriptor.type)
            {
                case 0x41:
                case 0x29:
                case 0xd029:
                case 0x00:
                case 0x8029:
                    result.Nodes.Add(MakeComplexFieldNode((ComplexType)field.data));
                    break;
                case 0x407d:
                case 0x409d:
                case 0x89:
                case 0xc089:
                    result.Nodes.Add((string)field.data);
                    break;
                case 0xc0ad:
                case 0xc0bd:
                case 0xc0cd:
                    result.Nodes.Add(((byte)field.data).ToString("X2"));
                    break;
                case 0xcdd:
                case 0xced:
                    result.Nodes.Add(((short)field.data).ToString("X4"));
                    break;
                case 0x35:
                case 0xc0fd:
                    result.Nodes.Add(((int)field.data).ToString("X8"));
                    break;
                case 0x417d:
                case 0xc10d:
                case 0xc15d:
                    result.Nodes.Add(((long)field.data).ToString("X16"));
                    break;
                case 0xc13d:
                    result.Nodes.Add(((float)field.data).ToString());
                    break;
            }
            return result;
        }

        public string toXML()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<EbxFile Guid=\"");
            sb.Append(Helpers.ByteArrayToHexString(GUID));
            sb.Append("\">\n");
            foreach (InstanceStruct ins in instancesList)
                sb.Append(InstanceToXML(ins));
            return sb.ToString();
        }

        public string InstanceToXML(InstanceStruct ins)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Helpers.MakeTabs(1) + "<" + ins.field.Descriptor._name + " Guid=\"");
            sb.Append(Helpers.ByteArrayToHexString(ins.GUID));
            sb.Append("\">\n");
            sb.Append(MakeComplexFieldXML(ins.field, 2));
            sb.Append(Helpers.MakeTabs(1) + "</" + ins.field.Descriptor._name + ">\n");
            return sb.ToString();
        }

        public string MakeComplexFieldXML(ComplexType cfield, int tab)
        {
            StringBuilder sb = new StringBuilder();
            string tabs = Helpers.MakeTabs(tab);
            if (cfield.Descriptor._name != "array")
                sb.AppendFormat(tabs + "<{0}>\n", cfield.Descriptor._name);
            foreach (Field f in cfield.Fields)
                sb.Append(MakeFieldXML(f, tab + 1));
            if (cfield.Descriptor._name != "array")
                sb.AppendFormat(tabs + "</{0}>\n", cfield.Descriptor._name);
            return sb.ToString();
        }

        public string MakeFieldXML(Field field, int tab)
        {
            StringBuilder sb = new StringBuilder();
            string tabs = Helpers.MakeTabs(tab);
            string tabs2 = Helpers.MakeTabs(tab + 1);
            FieldDescriptor desc = field.Descriptor;
            if (desc._name == "$")
                return MakeComplexFieldXML((ComplexType)field.data, tab);
            sb.AppendFormat(tabs + "<{0}>\n", desc._name);
            switch (desc.type)
            {
                case 0x41:
                case 0x29:
                case 0xd029:
                case 0x00:
                case 0x8029:
                    sb.Append(MakeComplexFieldXML((ComplexType)field.data, tab + 1));
                    break;
                case 0x407d:
                case 0x409d:
                case 0x89:
                case 0xc089:
                    sb.AppendFormat(tabs2 + "\"{0}\"\n", (string)field.data);
                    break;
                case 0xc0ad:
                case 0xc0bd:
                case 0xc0cd:
                    sb.AppendFormat(tabs2 + "0x{0}\n", ((byte)field.data).ToString("X2"));
                    break;
                case 0xcdd:
                case 0xced:
                    sb.AppendFormat(tabs2 + "0x{0}\n", ((short)field.data).ToString("X4"));
                    break;
                case 0x35:
                case 0xc0fd:
                    sb.AppendFormat(tabs2 + "0x{0}\n", ((int)field.data).ToString("X8"));
                    break;
                case 0x417d:
                case 0xc10d:
                case 0xc15d:
                    sb.AppendFormat(tabs2 + "0x{0}\n", ((long)field.data).ToString("X16"));
                    break;
                case 0xc13d:
                    sb.AppendFormat(tabs2 + "{0}f\n", ((float)field.data).ToString());
                    break;
            }
            sb.AppendFormat(tabs + "</{0}>\n", desc._name);
            return sb.ToString();
        }
        
    }
}
