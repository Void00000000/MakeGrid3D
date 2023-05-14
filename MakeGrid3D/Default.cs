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
        static public float speedZoom = 0.05f;
        static public float speedRotate = 0.1f;
        static public float cameraSpeed = 0.5f;
        static public float linesSize = 3;
        static public float pointsSize = 10;
        static public Color4 linesColor = Color4.White;
        static public Color4 pointsColor = Color4.White;
        static public Color4 bgColor = new Color4(118/255f, 113/255f, 132/255f, 1);
        static public bool wireframeMode = false;
        static public bool showGrid = true;
        static public bool unstructedGridMode = false;
        static public bool drawRemovedLinesMode = false;
        static public int maxAR_width = 20;
        static public int maxAR_height = 9;

        static public Color4 area1Color = new Color4(252/255f, 78/255f, 3/255f, 1);
        static public Color4 area2Color = new Color4(78/255f, 252/255f, 3/255f, 1);
        static public Color4 area3Color = new Color4(78/255f, 78/255f, 252/255f, 1);
        static public Color4[] areaColors = {area1Color, area2Color, area3Color};

        static public float indent = 0.2f;

        static public int I = 1;
        static public int J = 1;
        static public Direction dir1 = Direction.Left;
        static public Direction dir2 = Direction.Right;
        static public Direction dir3 = Direction.Bottom;
        static public Direction dir4 = Direction.Top;
        static public bool showCurrentUnstructedNode = false;
        static public Color4 currentUnstructedNodeColor = new Color4(1f, 0, 0, 1f);
    }
}
