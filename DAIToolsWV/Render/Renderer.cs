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


namespace DAIToolsWV.Render
{
    public class Renderer
    {
        public Device device = null;
        public static PresentParameters presentParams = new PresentParameters();
        public static Material Mat;
        public static Texture DefaultTex;
        public List<RenderObject> list = new List<RenderObject>();
        public float CamDistance = 10f;
        public Vector3 worldoffset = new Vector3(0, 0, 0);

        public void Init(IntPtr handle, int w, int h)
        {
            try
            {
                presentParams.Windowed = true;
                presentParams.SwapEffect = SwapEffect.Discard;
                presentParams.EnableAutoDepthStencil = true;
                presentParams.AutoDepthStencilFormat = DepthFormat.D16;
                presentParams.BackBufferWidth = w;
                presentParams.BackBufferHeight = h;
                device = new Device(0, DeviceType.Hardware, handle, CreateFlags.SoftwareVertexProcessing, presentParams);
                Mat = new Material();
                Mat.Diffuse = Color.White;
                Mat.Specular = Color.LightGray;
                Mat.SpecularSharpness = 15.0F;
                device.Material = Mat;
                string loc = Path.GetDirectoryName(Application.ExecutablePath);
                DefaultTex = TextureLoader.FromFile(device, loc + "\\ext\\img\\Default.bmp");
            }
            catch (DirectXException)
            {
                device = null;
            }
        }

        public void Render()
        {
            if (device == null)
                return;
            try
            {
                device.SetRenderState(RenderStates.ShadeMode, 1);
                device.SetRenderState(RenderStates.ZEnable, true);
                device.Clear(ClearFlags.Target, System.Drawing.Color.LightBlue, 1.0f, 0);
                device.Clear(ClearFlags.ZBuffer, System.Drawing.Color.Black, 1.0f, 0);
                device.BeginScene();
                device.SetTexture(0, DefaultTex);
                int iTime = Environment.TickCount;
                float fAngle = iTime * (2.0f * (float)Math.PI) / 10000.0f;
                device.Transform.World = Matrix.Translation(worldoffset) * Matrix.RotationZ(fAngle);
                device.Transform.View = Matrix.LookAtLH(new Vector3(0.0f, CamDistance, CamDistance/ 2), new Vector3(0, 0, 0), new Vector3(0.0f, 0.0f, 1.0f));
                device.Transform.Projection = Matrix.PerspectiveFovLH((float)Math.PI / 4, 1.0f, 0.01f, 100000.0f);
                foreach (RenderObject obj in list)
                    obj.Render(device);
                device.EndScene();
                device.Present();
            }
            catch (DirectXException)
            {
                return;
            }
        }

    }
}
