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
        public struct StreamingPartitionHeader
        {
            public int magic;
            public int metaSize;
            public int payloadSize;
            public int importCount;
            public ushort typeCount;
            public ushort numGUIDRepeater;
            public ushort unknown;
            public ushort typeDescriptorCount;
            public ushort fieldDescriptorCount;
            public ushort typeStringTableSize;
            public int stringTableSize;
            public int arrayCount;
            public int arrayOffset;
            public int _arraySectionstart;
        }

        public struct Guid
        {
            public uint data1;
            public ushort data2;
            public ushort data3;
            public byte[] data4;
        }

        public struct StreamingPartitionImportEntry
        {
            public Guid partitionGuid;
            public Guid instanceGuid;
        }

        public struct KeyWordDicStruct
        {
            public string keyword;
            public int hash;
            public int offset;
        }

        public struct StreamingPartitionFieldDescriptor
        {
            public int fieldNameHash;
            public ushort flagBits;
            public ushort fieldTypeIndex;
            public int fieldOffset;
            public int secondaryOffset;
            public string _name;
            public int _index;
            public byte _type;
        }

        public struct StreamingPartitionTypeDescriptor
        {
            public int typeNameHash;
            public int layoutDescriptorIndex;
            public byte fieldCount;
            public byte alignment;
            public ushort typeFlags;
            public ushort instanceSize;
            public ushort secondaryInstanceSize;
            public string _name;
            public ushort _type;
            public int _index;
        }

        public struct StreamingPartitionInstanceEntry
        {
            public ushort typeDescriptorIndex;
            public ushort repetitions;
        }

        public struct StreamingPartitionArrayEntry
        {
            public int offset;
            public int elementCount;
            public int typeDescriptorIndex;
        }

        public struct Field
        {
            public int offset;
            public StreamingPartitionFieldDescriptor Descriptor;
            public object data;
            public List<byte[]> ArrayData;
        }

        public struct Type
        {
            public List<byte[]> _wtfhash;
            public int offset;
            public StreamingPartitionTypeDescriptor Descriptor;
            public List<Field> Fields;
            public List<byte[]> ArrayData;
        }

        public struct InstanceStruct
        {
            public byte[] GUID;
            public string name;
            public Type field;
        }


        public StreamingPartitionHeader Header;
        public byte[] GUID;
        public List<StreamingPartitionImportEntry> externalGUIDs;
        public List<KeyWordDicStruct> keyWordDic;
        public List<StreamingPartitionFieldDescriptor> fieldDescriptors;
        public List<StreamingPartitionTypeDescriptor> complexTypeDescriptors;
        public List<StreamingPartitionInstanceEntry> instanceRepeaterList;
        public List<StreamingPartitionArrayEntry> arrayRepeaterList;
        public byte[] keywordarea;
        public List<string> typeNames;
        public List<InstanceStruct> instancesList;

        private static byte[] mapTypeCodeToAlignment = { 0x00, 0x04, 0x00, 0x04, 
                                                         0x04, 0x00, 0x04, 0x04, 
                                                         0x04, 0x04, 0x01, 0x01, 
                                                         0x01, 0x02, 0x02, 0x04, 
                                                         0x04, 0x08, 0x08, 0x04, 
                                                         0x08, 0x04, 0x01 };

        private static byte[] mapTypeCodeToSize = { 0x00, 0x04, 0x00, 0x04, 
                                                    0x04, 0x00, 0x00, 0x04, 
                                                    0x04, 0x00, 0x01, 0x01, 
                                                    0x01, 0x02, 0x02, 0x04, 
                                                    0x04, 0x08, 0x08, 0x04, 
                                                    0x08, 0x10, 0x14 };

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
            catch (Exception ex)
            {
            }
        }

        public void ReadHeader(Stream s)
        {
            Header = new StreamingPartitionHeader();
            Header.magic = Helpers.ReadInt(s);
            if (Header.magic != 0x0fb2d1ce)
                return;
            Header.metaSize = Helpers.ReadInt(s);
            Header.payloadSize = Helpers.ReadInt(s);
            Header.importCount = Helpers.ReadInt(s);
            Header.typeCount = Helpers.ReadUShort(s);
            Header.numGUIDRepeater = Helpers.ReadUShort(s);
            Header.unknown = Helpers.ReadUShort(s);
            Header.typeDescriptorCount = Helpers.ReadUShort(s);
            Header.fieldDescriptorCount = Helpers.ReadUShort(s);
            Header.typeStringTableSize = Helpers.ReadUShort(s);
            Header.stringTableSize = Helpers.ReadInt(s);
            Header.arrayCount = Helpers.ReadInt(s);
            Header.arrayOffset = Helpers.ReadInt(s);
            Header._arraySectionstart = Header.metaSize + Header.stringTableSize + Header.arrayOffset;
        }

        public void ReadExternalGUIDs(Stream s)
        {
            externalGUIDs = new List<StreamingPartitionImportEntry>();
            for (int i = 0; i < Header.importCount; i++)
            {
                StreamingPartitionImportEntry ex = new StreamingPartitionImportEntry();
                ex.partitionGuid = new Guid();
                ex.partitionGuid.data1 = Helpers.ReadUInt(s);
                ex.partitionGuid.data2 = Helpers.ReadUShort(s);
                ex.partitionGuid.data3 = Helpers.ReadUShort(s);
                ex.partitionGuid.data4 = new byte[8];
                s.Read(ex.instanceGuid.data4, 0, 8);
                ex.instanceGuid = new Guid();
                ex.instanceGuid.data1 = Helpers.ReadUInt(s);
                ex.instanceGuid.data2 = Helpers.ReadUShort(s);
                ex.instanceGuid.data3 = Helpers.ReadUShort(s);
                ex.instanceGuid.data4 = new byte[8];
                s.Read(ex.instanceGuid.data4, 0, 8);
                externalGUIDs.Add(ex);
            }
        }

        public void ReadKeyWords(Stream s)
        {
            keywordarea = new byte[Header.typeStringTableSize];
            s.Read(keywordarea, 0, Header.typeStringTableSize);
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
            fieldDescriptors = new List<StreamingPartitionFieldDescriptor>();
            for (int i = 0; i < Header.fieldDescriptorCount; i++)
            {
                StreamingPartitionFieldDescriptor f = new StreamingPartitionFieldDescriptor();
                f.fieldNameHash = Helpers.ReadInt(s);
                foreach (KeyWordDicStruct key in keyWordDic)
                    if (key.hash == f.fieldNameHash)
                    {
                        f._name = key.keyword;
                        break;
                    }
                f.flagBits = Helpers.ReadUShort(s);
                f._type = (byte)((f.flagBits >> 4) & 0x1F);
                f.fieldTypeIndex = Helpers.ReadUShort(s);
                f.fieldOffset = Helpers.ReadInt(s);
                f.secondaryOffset = Helpers.ReadInt(s);
                if (f._name == "$")
                    f.fieldOffset -= 8;
                f._index = i;
                fieldDescriptors.Add(f);
            }
        }

        public void ReadComplexTypeDescriptors(Stream s)
        {
            complexTypeDescriptors = new List<StreamingPartitionTypeDescriptor>();
            for (int i = 0; i < Header.typeDescriptorCount; i++)
            {
                StreamingPartitionTypeDescriptor f = new StreamingPartitionTypeDescriptor();
                f.typeNameHash = Helpers.ReadInt(s);
                foreach (KeyWordDicStruct key in keyWordDic)
                    if (key.hash == f.typeNameHash)
                    {
                        f._name = key.keyword;
                        break;
                    }
                f.layoutDescriptorIndex = Helpers.ReadInt(s);
                f.fieldCount = (byte)s.ReadByte();
                f.alignment = (byte)s.ReadByte();
                f.typeFlags = Helpers.ReadUShort(s);
                f._type = (byte)((f.typeFlags >> 4) & 0x1F);
                f.instanceSize = Helpers.ReadUShort(s);
                f.secondaryInstanceSize = Helpers.ReadUShort(s);
                f._index = i;
                complexTypeDescriptors.Add(f);
            }
        }

        public void ReadInstanceRepeaters(Stream s)
        {
            instanceRepeaterList = new List<StreamingPartitionInstanceEntry>();
            for (int i = 0; i < Header.typeCount; i++)
            {
                StreamingPartitionInstanceEntry ir = new StreamingPartitionInstanceEntry();
                ir.typeDescriptorIndex = Helpers.ReadUShort(s);
                ir.repetitions = Helpers.ReadUShort(s);
                instanceRepeaterList.Add(ir);
            }
            while (s.Position % 0x10 != 0)
                s.Seek(1, SeekOrigin.Current);
        }

        public void ReadArrayRepeaterList(Stream s)
        {
            arrayRepeaterList = new List<StreamingPartitionArrayEntry>();
            for (int i = 0; i < Header.arrayCount; i++)
            {
                StreamingPartitionArrayEntry ar = new StreamingPartitionArrayEntry();
                ar.offset = Helpers.ReadInt(s);
                ar.elementCount = Helpers.ReadInt(s);
                ar.typeDescriptorIndex = Helpers.ReadInt(s);
                arrayRepeaterList.Add(ar);
            }
        }

        public void ReadInstanceNames(Stream s)
        {
            s.Seek(Header.metaSize, 0);
            byte[] buff = new byte[Header.stringTableSize];
            s.Read(buff, 0, Header.stringTableSize);
            string t = Helpers.ByteArrayAsString(buff);
            typeNames = new List<string>(t.Split(new char[] { '\0' }, StringSplitOptions.RemoveEmptyEntries));
        }

        public void ReadInstances(Stream s)
        {
            s.Seek(Header.metaSize + Header.stringTableSize, 0);
            instancesList = new List<InstanceStruct>();
            int NonGuidIndex = 0;
            ArrayPointer = 0;
            for (int i = 0; i < instanceRepeaterList.Count; i++)
            {
                StreamingPartitionInstanceEntry curRep = instanceRepeaterList[i];
                for (int j = 0; j < curRep.repetitions; j++)
                {
                    int align = complexTypeDescriptors[curRep.typeDescriptorIndex].alignment;
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
                    instance.name = "Instance" + i;
                    instance.field = ReadComplexType(s, curRep.typeDescriptorIndex, new Field(), true);
                    instancesList.Add(instance);
                }
            }
        }

        public Type ReadComplexType(Stream s, int ComplexIndex, Field parent, bool isInstance = false)
        {
            Type result = new Type();
            result.offset = (int)s.Position;
            StreamingPartitionTypeDescriptor desc = complexTypeDescriptors[ComplexIndex];
            result.Descriptor = desc;
            result.Fields = new List<Field>();
            byte realtype = (byte)((desc.typeFlags >> 4) & 0x1F);
            for (int i = desc.layoutDescriptorIndex; i < desc.layoutDescriptorIndex + desc.fieldCount; i++)
            {
                result.Fields.Add(ReadField(s, i));
            }
            return result;
        }

        private int ArrayPointer;

        public Field ReadField(Stream s, int Index)
        {
            Field result = new Field();
            result.offset = (int)s.Position;
            StreamingPartitionFieldDescriptor desc = fieldDescriptors[Index];
            result.Descriptor = desc;
            int offset, index;
            byte[] buff;
            StreamingPartitionTypeDescriptor cdesc;
            byte realtype = (byte)((desc.flagBits >> 4) & 0x1F);
            switch (realtype)
            {
                case 0x0:
                case 0x2:
                case 0x3:
                    result.data = ReadComplexType(s, desc.fieldTypeIndex, result);
                    break;
                case 0x13:
                    result.data = Helpers.ReadFloat(s);
                    break;
                case 0x4:
                    cdesc = complexTypeDescriptors[desc.fieldTypeIndex];
                    if (ArrayPointer < arrayRepeaterList.Count())
                    {
                        StreamingPartitionArrayEntry arep = arrayRepeaterList[ArrayPointer++];
                        s.Seek(Header._arraySectionstart + arep.offset, 0);
                        Type acomp = new Type();
                        acomp.offset = (int)s.Position;
                        acomp.Descriptor = cdesc;
                        acomp.Fields = new List<Field>();
                        for (int i = 0; i < arep.elementCount; i++)
                            for (int j = 0; j < cdesc.fieldCount; j++)
                                acomp.Fields.Add(ReadField(s, cdesc.layoutDescriptorIndex + j));
                        result.data = acomp;
                    }
                    break;
                case 0x5:
                    result.data = Helpers.ReadLong(s);
                    break;
                case 0x8:
                    offset = Helpers.ReadInt(s);
                    cdesc = complexTypeDescriptors[desc.fieldTypeIndex];
                    string value = "";
                    if (cdesc.fieldCount != 0)
                        for (int i = cdesc.layoutDescriptorIndex; i < cdesc.layoutDescriptorIndex + cdesc.fieldCount; i++)
                            if (fieldDescriptors[i].fieldOffset == offset)
                            {
                                value = fieldDescriptors[i]._name;
                                break;
                            }
                    result.data = value;
                    break;
                case 0x9:
                case 0x10:
                    offset = Helpers.ReadInt(s);
                    result.data = "";
                    if (offset != -1)
                        foreach (KeyWordDicStruct key in keyWordDic)
                            if (key.hash == offset)
                                result.data = key.keyword;
                    break;
                case 0xa:
                case 0xb:
                case 0xc:
                    result.data = (byte)s.ReadByte();
                    break;
                case 0xd:
                case 0xe:
                    result.data = Helpers.ReadShort(s);
                    break;
                case 0xf:
                    result.data = Helpers.ReadInt(s);
                    break;
                case 0x7:
                case 0x15:
                    buff = new byte[16];
                    s.Read(buff, 0, 16);
                    result.data = buff;
                    break;
            }
            return result;
        }

        public string HeaderToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Header\n");
            sb.Append("Magic               : 0x" + Header.magic.ToString("X8") + "\n");
            sb.Append("AbsStringOffset     : 0x" + Header.metaSize.ToString("X8") + "\n");
            sb.Append("LenStringToEOF      : 0x" + Header.payloadSize.ToString("X8") + "\n");
            sb.Append("NumGUID             : 0x" + Header.importCount.ToString("X8") + "\n");
            sb.Append("NumInstanceRepeater : 0x" + Header.typeCount.ToString("X4") + "\n");
            sb.Append("NumGUIDRepeater     : 0x" + Header.numGUIDRepeater.ToString("X4") + "\n");
            sb.Append("Unknown             : 0x" + Header.unknown.ToString("X4") + "\n");
            sb.Append("NumComplex          : 0x" + Header.typeDescriptorCount.ToString("X4") + "\n");
            sb.Append("NumField            : 0x" + Header.fieldDescriptorCount.ToString("X4") + "\n");
            sb.Append("lenName             : 0x" + Header.typeStringTableSize.ToString("X4") + "\n");
            sb.Append("LenString           : 0x" + Header.stringTableSize.ToString("X8") + "\n");
            sb.Append("NumArrayRepeater    : 0x" + Header.arrayCount.ToString("X8") + "\n");
            sb.Append("LenPayload          : 0x" + Header.arrayOffset.ToString("X8") + "\n");
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

        public TreeNode MakeComplexFieldNode(Type cfield)
        {
            TreeNode result = new TreeNode("CT[" + cfield.Descriptor._index.ToString("X") + "][" + cfield.Descriptor._type.ToString("X4") + "]: " + cfield.Descriptor._name + "(size 0x" + cfield.Descriptor.instanceSize.ToString("X4") + " numfields = " + cfield.Descriptor.fieldCount + ")");
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
            TreeNode result = new TreeNode("F[" + field.Descriptor._index.ToString("X") + "][" + field.Descriptor._type.ToString("X4") + "]: " + field.Descriptor._name + " (offset 0x" + field.Descriptor.fieldOffset.ToString("X4") + " 2nd offset 0x" + field.Descriptor.secondaryOffset.ToString("X8") + ")");
            byte realtype = (byte)((field.Descriptor.flagBits >> 4) & 0x1F);
            if (field.data == null)
                return result;
            switch (realtype)
            {
                case 0x0:
                case 0x2:
                case 0x3:
                    result.Nodes.Add(MakeComplexFieldNode((Type)field.data));
                    break;
                case 0x13:
                    result.Nodes.Add(((float)field.data).ToString());
                    break;
                case 0x4:
                    Type c = (Type)field.data;
                    foreach (Field f in c.Fields)
                        result.Nodes.Add(MakeFieldNode(f));
                    break;
                case 0x10:
                    result.Nodes.Add(field.data.ToString());
                    break;
                case 0x5:
                    result.Nodes.Add(((long)field.data).ToString("X16"));
                    break;
                case 0x9:
                case 0xa:
                case 0xb:
                case 0xc:
                    result.Nodes.Add(((byte)field.data).ToString("X2"));
                    break;
                case 0xd:
                case 0xe:
                    result.Nodes.Add(((short)field.data).ToString("X4"));
                    break;
                case 0x15:
                case 0x7:
                    result.Nodes.Add(Helpers.ByteArrayToHexString((byte[])field.data));
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

        public string MakeComplexFieldXML(Type cfield, int tab)
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
            StreamingPartitionFieldDescriptor desc = field.Descriptor;
            if (desc._name == "$")
                return MakeComplexFieldXML((Type)field.data, tab);
            sb.AppendFormat(tabs + "<{0}>\n", desc._name);
            byte realtype = (byte)((field.Descriptor.flagBits >> 4) & 0x1F);
            if (field.data == null)
                return "";
            switch (realtype)
            {
                case 0:
                case 2:
                case 3:
                    sb.Append(MakeComplexFieldXML((Type)field.data, tab + 1));
                    break;
                case 0x10:
                    sb.AppendFormat(tabs2 + "{0}\n", field.data.ToString());
                    break;
                case 0x5:
                    sb.AppendFormat(tabs2 + "0x{0}\n", ((long)field.data).ToString("X16"));
                    break;
                case 0x9:
                case 0xa:
                case 0xb:
                case 0xc:
                    sb.AppendFormat(tabs2 + "0x{0}\n", ((byte)field.data).ToString("X2"));
                    break;
                case 0xd:
                case 0xe:
                    sb.AppendFormat(tabs2 + "0x{0}\n", ((short)field.data).ToString("X4"));
                    break;
                case 0xf:
                    sb.AppendFormat(tabs2 + "0x{0}\n", ((int)field.data).ToString("X8"));
                    break;
                case 0x13:
                    sb.AppendFormat(tabs2 + "{0}f\n", ((float)field.data).ToString());
                    break;
            }
            sb.AppendFormat(tabs + "</{0}>\n", desc._name);
            return sb.ToString();
        }
        
    }
}
