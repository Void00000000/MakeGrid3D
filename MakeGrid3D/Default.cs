using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MakeGrid3D
{
    static class Default
    {
        static public float speedTranslate = 1;
        static public float speedZoom = 1.01f;
        static public float linesSize = 3;
        static public float pointsSize = 10;
        static public Color4 linesColor = Color4.White;
        static public Color4 pointsColor = Color4.White;
        static public Color4 bgColor = new Color4(118/255f, 113/255f, 132/255f, 1);
        static public bool wireframeMode = false;
        static public bool unstructedGridMode = false;
        static public bool drawRemovedLinesMode = false;
        static public int maxAR_width = 20;
        static public int maxAR_height = 9;

        static public Color4 area1Color = new Color4(252/255f, 78/255f, 3/255f, 1);
        static public Color4 area2Color = new Color4(78/255f, 252/255f, 3/255f, 1);
        static public Color4 area3Color = new Color4(78/255f, 78/255f, 252/255f, 1);
        static public Color4[] areaColors = {area1Color, area2Color, area3Color};
    }
}
