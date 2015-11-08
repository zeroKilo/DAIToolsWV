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
    public abstract class RenderObject
    {
        public abstract void Render(Device device);
    }
}
