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
        static public float speedMove = 0.5f;
        static public float speedZoom = 0.05f;
        static public float speedRotate = MathHelper.DegreesToRadians(10f); // В радианах
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

        static public List<Color4> areaColors;

        static public float indent = 0.2f;

        // Указано направление обхода по узлам
        static public Dictionary<Quadrant, Direction[]> Directions = new Dictionary<Quadrant, Direction[]>()
        {
            { Quadrant.RightTop, new Direction[]{Direction.Right, Direction.Top} },
            { Quadrant.LeftTop, new Direction[]{Direction.Left, Direction.Top} },
            { Quadrant.LeftBottom, new Direction[]{Direction.Left, Direction.Bottom} },
            { Quadrant.RightBottom, new Direction[]{Direction.Right, Direction.Bottom} }
        };

        static public bool smartMerge = false;
        static public bool showCurrentUnstructedNode = false;
        static public Color4 currentUnstructedNodeColor = new Color4(1f, 0, 0, 1f);

        static public Plane plane = Plane.XY;

        static public Color4 MinColor = Color4.Red;
        static public Color4 MaxColor = Color4.Blue;
    }
}
