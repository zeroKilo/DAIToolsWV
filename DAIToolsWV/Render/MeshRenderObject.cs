using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using DAILibWV.Frostbite;

namespace DAIToolsWV.Render
{
    public class MeshRenderObject : RenderObject
    {
        public Vector3 min,max,center;
        public DAILibWV.Frostbite.Mesh mesh;
        public static List<CustomVertex.PositionTextured[]> RawTriangles;

        public override void Render(Device device)
        {
            device.VertexFormat = CustomVertex.PositionTextured.Format;
            device.RenderState.Lighting = false;
            device.RenderState.FillMode = FillMode.Solid;
            device.RenderState.CullMode = Cull.None;
            foreach(CustomVertex.PositionTextured[] list in RawTriangles)
                device.DrawUserPrimitives(PrimitiveType.TriangleList, list.Length / 3, list);
        }

        public MeshRenderObject(DAILibWV.Frostbite.Mesh m)
        {
            mesh = m;
            foreach (DAILibWV.Frostbite.Mesh.MeshLOD lod in mesh.header.LODs)
            {
                if (lod.Sections == null || lod.Sections.Count == 0 || lod.Sections[0].VertexBuffer == null)
                    continue;
                RawTriangles = new List<CustomVertex.PositionTextured[]>();
                foreach (DAILibWV.Frostbite.Mesh.MeshSection sec in lod.Sections)
                {
                    List<CustomVertex.PositionTextured> list = new List<CustomVertex.PositionTextured>();
                    for (int i = 0; i < sec.TriangleCount; i++)
                    {
                        list.Add(DAI2DX(sec.VertexBuffer[sec.IndexBuffer[i].i0]));
                        list.Add(DAI2DX(sec.VertexBuffer[sec.IndexBuffer[i].i1]));
                        list.Add(DAI2DX(sec.VertexBuffer[sec.IndexBuffer[i].i2]));
                    }
                    RawTriangles.Add(list.ToArray());
                }
                break;
            }
            float inf = 10000000000f;
            min = new Vector3(inf, inf, inf);
            max = new Vector3(-inf, -inf, -inf);
            foreach (CustomVertex.PositionTextured[] list in RawTriangles)
            {
                foreach (CustomVertex.PositionTextured v in list)
                {
                    if (v.X > max.X) max.X = v.X;
                    if (v.Y > max.Y) max.Y = v.Y;
                    if (v.Z > max.Z) max.Z = v.Z;
                    if (v.X < min.X) min.X = v.X;
                    if (v.Y < min.Y) min.Y = v.Y;
                    if (v.Z < min.Z) min.Z = v.Z;
                }
            }
            center = (max + min) * 0.5f;
        }

        public CustomVertex.PositionTextured DAI2DX(DAILibWV.Frostbite.Mesh.Vertex p)
        {
            return new CustomVertex.PositionTextured(new Vector3(p.Position.x, p.Position.z, p.Position.y), p.TexCoords.x, p.TexCoords.y);
        }
    }
}
