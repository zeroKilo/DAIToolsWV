using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAILibWV.Frostbite
{
    public class Mesh
    {
        public class Vector3
        {
            public float x, y, z;
            public Vector3(float _x, float _y, float _z)
            {
                x = _x;
                y = _y;
                z = _y;
            }
            public float Length()
            {
                return (float)Math.Sqrt(x * x + y * y + z * z);
            }

            public override string ToString()
            {
                return "[" + x + " ; " + y + " ; " + z + "]";
            }

        }

        public class Vertex
        {
            public int VertexType;
            public int Offset;
            public int Unknown01;
        }

        public class MeshSection
        {
            public int Unknown01;
            public int Unknown02;
            public String SectionName;
            public int Unknown03;
            public int TriangleCount;
            public int StartIndex;
            public int VertexBufferOffset;
            public int VertexCount;
            public int VertexStride;
            public int Unknown04;
            public int Unknown05;
            public int Unknown06;
            public List<Vertex> VertexEntries;
            public int[] Unknowns2;
            public ushort[] SubBoneList;
        }

        public class MeshLOD
        {
            public int BoneDataCount;
            public int Unknown02;
            public int NumSubObjects;
            public long SectionsOffset;
            public List<MeshSection> Sections;
            public int Unknown03;
            public long[] DataOffsets;
            public int[] DataValues;
            public int Unknown04;
            public int IndexBufferSize;
            public int VertexBufferSize;
            public int Unknown05;
            public byte[] ChunkID;
            public int InlineDataOffset;
            public int Unknown07;
            public int Unknown08;
            public string String01;
            public string String02;
            public string String03;
            public int Unknown09;
            public int Unknown10;
            public int Unknown11;
            public int BoneCount;
            public List<long> BoneData;
            public int Unknown13;
        }

        public class MeshHeader
        {
            public Vector3 MinPosition;
            public float Unknown01;
            public Vector3 MaxPosition;
            public float Unknown02;
            public long[] LODoffsets;
            public List<MeshLOD> LODs;
            public string MeshPath;
            public string MeshName;
            public int Unknown03;
            public int Unknown04;
            public int Unknown05;
            public int TotalLODCount;
            public int TotalSubObjectCount;
        }

        public MeshHeader header;

        public Mesh(Stream s)
        {
            ReadHeader(s);
        }

        public static string SerializeString(Stream s)
        {
            long offset = Helpers.ReadLong(s);
            long pos = s.Position;
            s.Seek(offset, 0);
            string result = Helpers.ReadNullString(s);
            s.Seek(pos, 0);
            return result;
        }

        private void ReadHeader(Stream s)
        {
            MeshHeader h = new MeshHeader();
            h.MinPosition = new Vector3(Helpers.ReadFloat(s), Helpers.ReadFloat(s), Helpers.ReadFloat(s));
            h.Unknown01 = Helpers.ReadFloat(s);
            h.MaxPosition = new Vector3(Helpers.ReadFloat(s), Helpers.ReadFloat(s), Helpers.ReadFloat(s));
            h.Unknown02 = Helpers.ReadFloat(s);
            h.LODoffsets = new long[6];
            for (int i = 0; i < 6; i++)
                h.LODoffsets[i] = Helpers.ReadLong(s);
            h.MeshPath = SerializeString(s);
            h.MeshName = SerializeString(s);
            h.Unknown03 = Helpers.ReadInt(s);
            h.Unknown04 = Helpers.ReadInt(s);
            h.Unknown05 = Helpers.ReadInt(s);
            h.TotalLODCount = Helpers.ReadShort(s);
            h.TotalSubObjectCount = Helpers.ReadShort(s);
            h.LODs = new List<MeshLOD>();
            for (int i = 0; i < 6; i++)
                if (h.LODoffsets[i] != 0)
                {
                    s.Seek(h.LODoffsets[i], 0);
                    h.LODs.Add(ReadMeshLOD(s));
                }
            header = h;
        }

        private MeshLOD ReadMeshLOD(Stream s)
        {
            MeshLOD r = new MeshLOD();
            r.BoneDataCount = Helpers.ReadInt(s);
            r.Unknown02 = Helpers.ReadInt(s);
            r.NumSubObjects = Helpers.ReadInt(s);
            r.SectionsOffset = Helpers.ReadLong(s);
            r.Unknown03 = Helpers.ReadInt(s);
            r.DataOffsets = new long[4];
            r.DataValues = new int[4];
            for (int i = 0; i < 4; i++)
            {
                r.DataOffsets[i] = Helpers.ReadLong(s);
                r.DataValues[i] = Helpers.ReadInt(s);
            }
            r.Unknown04 = Helpers.ReadInt(s);
            r.IndexBufferSize = Helpers.ReadInt(s);
            r.VertexBufferSize = Helpers.ReadInt(s);
            r.Unknown05 = Helpers.ReadInt(s);
            r.ChunkID = new byte[16];
            for (int i = 0; i < 16; i++)
                r.ChunkID[i] = (byte)s.ReadByte();
            r.InlineDataOffset = Helpers.ReadInt(s);
            r.Unknown07 = Helpers.ReadInt(s);
            r.Unknown08 = Helpers.ReadInt(s);
            r.String01 = SerializeString(s);
            r.String02 = SerializeString(s);
            r.String03 = SerializeString(s);
            r.Unknown09 = Helpers.ReadInt(s);
            r.Unknown10 = Helpers.ReadInt(s);
            r.Unknown11 = Helpers.ReadInt(s);
            r.BoneCount = Helpers.ReadInt(s);
            r.BoneData = new List<long>();
            for (int i = 0; i < r.BoneDataCount; i++)
            {
                r.BoneData.Add(Helpers.ReadLong(s));
                r.BoneData.Add(Helpers.ReadLong(s));
            }
            r.Unknown13 = Helpers.ReadInt(s);
            r.Sections = new List<MeshSection>();
            if (r.NumSubObjects > 0)
            {
                s.Seek(r.SectionsOffset, SeekOrigin.Begin);
                for (int i = 0; i < r.NumSubObjects; i++)
                    r.Sections.Add(ReadMeshSection(s));
            }
            for (int i = 0; i < r.NumSubObjects; i++)
                if (r.Sections[i].Unknown05 > 0)
                {
                    int NumSubObjectBones = (r.Sections[i].Unknown04 >> 24);
                    r.Sections[i].SubBoneList = new ushort[NumSubObjectBones];
                    s.Seek(r.Sections[i].Unknown05, 0);
                    for (int j = 0; j < NumSubObjectBones; j++)
                        r.Sections[i].SubBoneList[j] = Helpers.ReadUShort(s);
                }
            return r;
        }

        private MeshSection ReadMeshSection(Stream s)
        {
            MeshSection r = new MeshSection();
            r.Unknown01 = Helpers.ReadInt(s);
            r.Unknown02 = Helpers.ReadInt(s);
            r.SectionName = SerializeString(s);
            r.Unknown03 = Helpers.ReadInt(s);
            r.TriangleCount = Helpers.ReadInt(s);
            r.StartIndex = Helpers.ReadInt(s);
            r.VertexBufferOffset = Helpers.ReadInt(s);
            r.VertexCount = Helpers.ReadInt(s);
            r.Unknown04 = Helpers.ReadInt(s);
            r.Unknown05 = Helpers.ReadInt(s);
            r.Unknown06 = Helpers.ReadInt(s);
            r.VertexEntries = new List<Vertex>();
            for (int i = 0; i < 16; i++)
            {
                Vertex VertexEntry = new Vertex();
                VertexEntry.VertexType = Helpers.ReadShort(s);
                VertexEntry.Offset = s.ReadByte();
                VertexEntry.Unknown01 = s.ReadByte();
                if (VertexEntry.Offset != 0xFF)
                    r.VertexEntries.Add(VertexEntry);
            }
            r.VertexStride = Helpers.ReadInt(s);
            r.Unknowns2 = new int[19];
            for (int i = 0; i < 19; i++)
                r.Unknowns2[i] = Helpers.ReadInt(s);
            r.SubBoneList = new ushort[0];
            return r;
        }

        public string HeaderToStr()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("MinPosition            : {0}\n", header.MinPosition);
            sb.AppendFormat("Unknown1               : {0}\n", header.Unknown01);
            sb.AppendFormat("MixPosition            : {0}\n", header.MaxPosition);
            sb.AppendFormat("Unknown2               : {0}\n", header.Unknown02);
            int count = 0;
            foreach (long l in header.LODoffsets)
                sb.AppendFormat("LODOffset {0}            : 0x{1}\n", count++, l.ToString("X16"));
            sb.AppendFormat("MeshPath               : {0}\n", header.MeshPath);
            sb.AppendFormat("MeshName               : {0}\n", header.MeshName);
            sb.AppendFormat("Unknown3               : 0x{0}\n", header.Unknown03.ToString("X8"));
            sb.AppendFormat("Unknown4               : 0x{0}\n", header.Unknown04.ToString("X8"));
            sb.AppendFormat("Unknown5               : 0x{0}\n", header.Unknown05.ToString("X8"));
            sb.AppendFormat("Total LOD Count        : {0}\n", header.TotalLODCount);
            sb.AppendFormat("Total Section Count    : {0}\n", header.TotalSubObjectCount);
            return sb.ToString();
        }

        public string LODsToString()
        {
            StringBuilder sb = new StringBuilder();
            int count = 0;
            foreach (MeshLOD lod in header.LODs)
                sb.AppendFormat("[LOD Level {0}]:\n{1}", count++, LODToString(lod));
            return sb.ToString();
        }

        public string LODToString(MeshLOD lod)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(".BoneDataCount        : {0}\n", lod.BoneDataCount);
            sb.AppendFormat(".Unknown02            : 0x{0}\n", lod.Unknown02.ToString("X8"));
            sb.AppendFormat(".NumSections          : {0}\n", lod.NumSubObjects);
            sb.AppendFormat(".SectionsOffset       : 0x{0}\n", lod.SectionsOffset.ToString("X16"));
            sb.AppendFormat(".Unknown03            : 0x{0}\n", lod.Unknown03.ToString("X8"));
            for (int i = 0; i < 4; i++)
                sb.AppendFormat(".Data Entry[{0}]        : 0x{1} - 0x{2}\n", i, lod.DataOffsets[i].ToString("X16"), lod.DataValues[i].ToString("X8"));
            sb.AppendFormat(".Unknown04            : 0x{0}\n", lod.Unknown04.ToString("X8"));
            sb.AppendFormat(".IndexBufferSize      : 0x{0}\n", lod.IndexBufferSize.ToString("X8"));
            sb.AppendFormat(".VertexBufferSize     : 0x{0}\n", lod.VertexBufferSize.ToString("X8"));
            sb.AppendFormat(".Unknown05            : 0x{0}\n", lod.Unknown05.ToString("X8"));
            sb.AppendFormat(".ChunkID              : 0x{0}\n", Helpers.ByteArrayToHexString(lod.ChunkID));
            sb.AppendFormat(".InlineDataOffset     : 0x{0}\n", lod.InlineDataOffset.ToString("X8"));
            sb.AppendFormat(".Unknown07            : 0x{0}\n", lod.Unknown07.ToString("X8"));
            sb.AppendFormat(".Unknown08            : 0x{0}\n", lod.Unknown08.ToString("X8"));
            sb.AppendFormat(".String01             : {0}\n", lod.String01);
            sb.AppendFormat(".String02             : {0}\n", lod.String02);
            sb.AppendFormat(".String03             : {0}\n", lod.String03);
            sb.AppendFormat(".Unknown09            : 0x{0}\n", lod.Unknown09.ToString("X8"));
            sb.AppendFormat(".Unknown10            : 0x{0}\n", lod.Unknown10.ToString("X8"));
            sb.AppendFormat(".Unknown11            : 0x{0}\n", lod.Unknown11.ToString("X8"));
            sb.AppendFormat(".BoneCount            : {0}\n", lod.BoneCount);
            sb.AppendFormat(".BoneData             : ");
            for (int i = 0; i < lod.BoneData.Count / 2; i++)
                sb.AppendFormat("[0x" + lod.BoneData[i * 2].ToString("X") + " - 0x" + lod.BoneData[i * 2 + 1].ToString("X") + "] ");
            sb.AppendFormat("\n.Unknown03            : 0x{0}\n", lod.Unknown13.ToString("X8"));
            int count = 0;
            foreach (MeshSection sec in lod.Sections)
                sb.AppendFormat(".[Section {0}]:\n{1}", count++, SectionToString(sec));
            return sb.ToString();
        }

        public string SectionToString(MeshSection sec)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("..Unknown01          : 0x{0}\n", sec.Unknown01.ToString("X8"));
            sb.AppendFormat("..Unknown02          : 0x{0}\n", sec.Unknown02.ToString("X8"));
            sb.AppendFormat("..SectionName        : {0}\n", sec.SectionName);
            sb.AppendFormat("..Unknown03          : 0x{0}\n", sec.Unknown03.ToString("X8"));
            sb.AppendFormat("..TriangleCount      : {0}\n", sec.TriangleCount);
            sb.AppendFormat("..StartIndex         : {0}\n", sec.StartIndex);
            sb.AppendFormat("..VertexBufferOffset : 0x{0}\n", sec.VertexBufferOffset.ToString("X8"));
            sb.AppendFormat("..VertexCount        : {0}\n", sec.VertexCount);
            sb.AppendFormat("..VertexStride       : 0x{0}\n", sec.VertexStride.ToString("X8"));
            sb.AppendFormat("..Unknown04          : 0x{0}\n", sec.Unknown04.ToString("X8"));
            sb.AppendFormat("..Unknown05          : 0x{0}\n", sec.Unknown05.ToString("X8"));
            sb.AppendFormat("..Unknown06          : 0x{0}\n", sec.Unknown06.ToString("X8"));
            sb.Append("..VertexEntries      : ");
            if (sec.VertexEntries != null) 
            foreach (Vertex v in sec.VertexEntries)
                sb.AppendFormat("[Type 0x{0} ; Offset 0x{1} ; Unknown 0x{2}]", v.VertexType.ToString("X"), v.Offset.ToString("X"), v.Unknown01.ToString("X"));
            sb.Append("\n..Unknown List       : ");
            foreach (int u in sec.Unknowns2)
                sb.AppendFormat("0x{0}, ", u.ToString("X8"));
            sb.Append("\n..Section Bone List  : ");
            foreach (int u in sec.SubBoneList)
                sb.AppendFormat("{0}, ", u);
            sb.Append("\n");
            return sb.ToString();
        }
    }
}
