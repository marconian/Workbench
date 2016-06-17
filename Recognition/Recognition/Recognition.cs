using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace Recognition
{
    class Vision
    {
        protected internal Pixel[,] _map;
        protected internal Color[,] _rawMap;
        public int width
        {
            get { return RawMap.GetLength(0); }
        }
        public int height
        {
            get { return RawMap.GetLength(1); }
        }
        public Pixel[,] Map
        {
            get { return _map; }
            set { _map = value; }
        }
        public Color[,] RawMap
        {
            get { return _rawMap; }
            set { _rawMap = value; }
        }

        public List<List<Pixel>> _groups = new List<List<Pixel>>();
        public List<Pixel> edgePixels = new List<Pixel>();

        private void loadMap ()
        {
            if (_rawMap != null)
            {
                Map = new Pixel[width, height];
                _groups.Add(new List<Pixel>());
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        try {
                            Pixel pixel = new Pixel(x, y, _rawMap);

                            Map[x, y] = pixel;

                            if (pixel.edge) {
                                edgePixels.Add(pixel);
                            }
                        }
                        catch (Exception exception)
                        {
                            throw exception;
                        }
                    }
                }
            }
        }
        public void OpenEyes(string Url)
        {
            try
            {
                using (Bitmap img = new Bitmap(Url))
                {
                    OpenEyes(img);
                }
            }
            catch (ArgumentException exception)
            {
                throw exception;
            }
        }
        public void OpenEyes(Bitmap Image)
        {
            try
            {
                using (Bitmap img = Image)
                using (MemoryStream ms = new MemoryStream())
                {
                    Color[,] map = new Color[img.Width, img.Height];
                    for (int x = 0; x < img.Width; x++)
                    {
                        for (int y = 0; y < img.Height; y++)
                        {
                            if (img.GetPixel(x, y) != null)
                            {
                                map[x, y] = img.GetPixel(x, y);
                            }
                            else
                            {
                                map[x, y] = new Color();
                            }
                        }
                    }

                    RawMap = map;
                    loadMap();
                    //analyzeTexture(10);
                    //LoadEdgeMap();
                }
            }
            catch (ArgumentException exception)
            {
                throw exception;
            }
        }
        public void groupEdge(Pixel pixel, int searchBuffer)
        {
            List<Pixel> list = new List<Pixel>();
            for (Spiral spiral = new Spiral(searchBuffer); spiral.i < spiral.area; spiral.step())
            {
                int newX = pixel.x + spiral.x;
                int newY = pixel.y + spiral.y;
                if (newX >= 0 && newX < width && newY >= 0 && newY < height)
                {
                    Pixel neighb = Map[newX, newY];
                    if (neighb.edge && Converter.RGBtoDelta(pixel.center, neighb.center) < 5d)
                    {
                        if (neighb.group != 0)
                        {
                            Map[pixel.x, pixel.y].group = neighb.group;
                            for (var i = 0; i < list.Count; i++)
                            {
                                Pixel item = list[i];
                                Map[item.x, item.y].group = neighb.group;
                            }
                            _groups[neighb.group].Add(pixel);
                            _groups[neighb.group].AddRange(list);
                            break;
                        }
                        else
                        {
                            list.Add(neighb);
                        }
                    }
                }
            }
            if (list.Count > 0)
            {
                List<Pixel> newGroup = new List<Pixel>();
                _groups.Add(newGroup);

                int index = _groups.IndexOf(newGroup);
                Map[pixel.x, pixel.y].group = index;
                for (var i = 0; i < list.Count; i++)
                {
                    Pixel item = list[i];
                    Map[item.x, item.y].group = index;
                }

                newGroup.Add(pixel);
                newGroup.AddRange(list);
            }

        }        
        public int findEdge(int x, int y)
        {
            int w = Map.GetLength(0);
            int h = Map.GetLength(1);
            int sx, sy, dx, dy;
            sx = sy = dx = 0;
            dy = -1;
            int t = Math.Max(w, h);
            int maxl = t * t;

            for (int i = 0; i <= maxl; i++)
            {
                if ((x + sx) >= 0 && (x + sx) < w && (y + sy) >= 0 && (y + sy) < h)
                {
                    int edge = Map[(x + sx), (y + sy)].group;
                    if (edge != 0) { return edge; }
                }

                // spiral
                if (sx == sy || ((sx < 0) && (sx == -sy)) || ((sx > 0) && (sx == 1 - sy)))
                {
                    t = dx;
                    dx = -dy;
                    dy = t;
                }
                sx += dx;
                sy += dy;
            }

            return 0;
        }
        public Bitmap ShowVision()
        {
            if (Map != null)
            {
                Bitmap bitmap = new Bitmap(width, height);
                //int group = _groups.IndexOf(_groups.Max());
                int r = 100;
                int g = 100;
                int b = 100;

                for (var i = 0; i < edgePixels.Count; i++)
                {
                    Pixel pixel = edgePixels[i];
                    try {
                        if (Map[pixel.x, pixel.y].group == 0)
                        {
                            groupEdge(pixel, 3);
                        }
                    }
                    catch (IndexOutOfRangeException exception) { break; }
                }
                for (var j = 0; j < _groups.Count; j++)
                {
                    List<Pixel> group = _groups[j];

                    if (r < 255) { r++; }
                    else if (g < 255) { g++; }
                    else if (b < 255) { b++; }

                    for (var i = 0; i < group.Count; i++)
                    {
                        Pixel pixel = group[i];
                        try
                        {

                            bitmap.SetPixel(pixel.x, pixel.y, Color.FromArgb(r, g, b)); 

                        }
                        catch (ArgumentOutOfRangeException exception)
                        {
                            throw exception;
                        }
                    }
                }

                return bitmap;
            }
            else
            {
                return null;
            }
        }

        public void analyzeTexture(int block)
        {
            int xGroup = 0, yGroup = 0;
            for (var x = 0; x <= block; x++)
            {
                for (var y = 0; y <= block; y++)
                {
                    if (x == xGroup && y == yGroup)
                    {
                        xGroup = xGroup + block;
                        yGroup = yGroup + block;

                        try
                        {
                            int w = block; int h = block;
                            double factor = 0d;
                            double pixelCount = 0d;
                            for (var i = 0; i <= w; i++)
                            {
                                for (var j = 0; j <= w; j++)
                                {
                                    int newX = x + i, newY = y + j;
                                    if (newX >= 0 && newX < Map.GetLength(0) && newY >= 0 && newY < Map.GetLength(1))
                                    {
                                        factor += Map[newX, newY].delta;
                                        pixelCount++;
                                    }
                                }
                            }

                            factor = factor / pixelCount;

                            for (var i = 0; i <= w; i++)
                            {
                                for (var j = 0; j <= w; j++)
                                {
                                    int newX = x + i, newY = y + j;
                                    if (newX >= 0 && newX < Map.GetLength(0) && newY >= 0 && newY < Map.GetLength(1))
                                    {
                                        Map[newX, newY].surrounding = factor;
                                    }
                                }
                            }
                        }
                        catch (IndexOutOfRangeException exception)
                        {
                            throw exception;
                        }
                    }
                }
            }
        }
    }

    class Spiral
    {        
        protected internal int dx = 0;
        protected internal int dy = -1;
        protected internal int t = 3;
        protected internal int _buffer = 3;
        public int i
        {
            get; set;
        }
        public int x
        {
            get; set;
        }
        public int y
        {
            get; set;
        }
        public int area
        {
            get { return _buffer * _buffer; }
        }

        public Spiral() { }
        public Spiral (int buffer)
        {
            _buffer = buffer;
            t = _buffer;
        }

        public void step ()
        {
            i++;
            // spiral
            if ((x == y || ((x < 0)) && ((x == -y)) || ((x > 0)) && (x == 1 - y)))
            {
                t = dx;
                dx = -dy;
                dy = t;
            }
            x += dx;
            y += dy;
        }

    }

    class Pixel
    {
        internal protected int _x;
        internal protected int _y;
        internal protected Color[,] _rawmap;

        public Pixel (int y, int x, Color [,] rawMap)
        {
            _x = x;
            _y = y;
            _rawmap = rawMap;
        }

        public Color center
        {
            get
            {
                if (_rawmap == null || _x < 0 || _x >= _rawmap.GetLength(0) || _y < 0 || _y >= _rawmap.GetLength(1))
                {
                    return Color.FromArgb(255, 255, 255);
                }
                else
                {
                    return _rawmap[x, y];
                }
            }
        }

        public Color top
        {
            get
            {
                if (_rawmap == null || _x < 0 || _x >= _rawmap.GetLength(0) || _y - 1 < 0 || _y - 1 >= _rawmap.GetLength(1))
                {
                    return Color.FromArgb(255, 255, 255, 255);
                }
                else
                {
                    return _rawmap[x, y - 1];
                }
            }
        }

        public Color left
        {
            get
            {
                if (_rawmap == null || _x - 1 < 0 || _x - 1 >= _rawmap.GetLength(0) || _y < 0 || _y >= _rawmap.GetLength(1))
                {
                    return Color.FromArgb(255, 255, 255, 255);
                }
                else
                {
                    return _rawmap[x - 1, y];
                }
            }
        }

        public Color bottom
        {
            get
            {
                if (_rawmap == null || _x < 0 || _x >= _rawmap.GetLength(0) || _y + 1 < 0 || _y + 1 >= _rawmap.GetLength(1))
                {
                    return Color.FromArgb(255, 255, 255, 255);
                }
                else
                {
                    return _rawmap[x, y + 1];
                }
            }
        }

        public Color right
        {
            get
            {
                if (_rawmap == null || _x + 1 < 0 || _x + 1 >= _rawmap.GetLength(0) || _y < 0 || _y >= _rawmap.GetLength(1))
                {
                    return Color.FromArgb(255, 255, 255, 255);
                }
                else
                {
                    return _rawmap[x + 1, y];
                }
            }
        }
        

        public double delta
        {
            get
            {
                //double dSum = 0;
                //int count = 0;
                //for (Spiral spiral = new Spiral(3); spiral.i < spiral.area; spiral.step())
                //{
                //    if ((x + spiral.x) >= 0 && (x + spiral.x) < _rawmap.GetLength(0) && (y + spiral.y) >= 0 && (y + y) < _rawmap.GetLength(1))
                //    {
                //        dSum += Converter.RGBtoDelta(center, _rawmap[(x + spiral.x), (y + spiral.y)]);
                //        count++;
                //    }
                //}

                //return dSum / Convert.ToDouble(count);

                double t = Converter.RGBtoDelta(center, top);
                double l = Converter.RGBtoDelta(center, left);
                double b = Converter.RGBtoDelta(center, bottom);
                double r = Converter.RGBtoDelta(center, right);
                return (t + l + b + r) / 4d;
            }
        }

        public bool edge
        {
            get { return delta > 5d + surrounding; }
        }
        public int group
        {
            get; set;
        }

        public double top_delta
        {
            get { return Converter.RGBtoDelta(center, top); }
        }
        public double left_delta
        {
            get { return Converter.RGBtoDelta(center, left); }
        }
        public double bottom_delta
        {
            get { return Converter.RGBtoDelta(center, bottom); }
        }
        public double right_delta
        {
            get { return Converter.RGBtoDelta(center, right); }
        }

        public int x
        {
            get { return _x; }
        }

        public int y
        {
            get { return _y; }
        }
        public double surrounding
        {
            get; set;
        }


    }

    class Element
    {
        protected internal bool[,] _element;
        public bool[,] PixelGroup
        {
            get { return _element; }
            set { _element = value; }
        }
    }

    class Converter
    {
        public static cXYZ RGBtoXYZ (int r, int g, int b)
        {
            double _R = r / 255d;
            double _G = g / 255d;
            double _B = b / 255d;

            _R = (_R > 0.04045d ? Math.Pow((_R + 0.055d) / 1.055d, 2.4d) : _R / 12.92d) * 100d;
            _G = (_G > 0.04045d ? Math.Pow((_G + 0.055d) / 1.055d, 2.4d) : _G / 12.92d) * 100d;
            _B = (_B > 0.04045d ? Math.Pow((_B + 0.055d) / 1.055d, 2.4d) : _B / 12.92d) * 100d;

            cXYZ xyz = new cXYZ();
            xyz.X = _R * 0.4124d + _G * 0.3576d + _B * 0.1805d;
            xyz.Y = _R * 0.2126d + _G * 0.7152d + _B * 0.0722d;
            xyz.Z = _R * 0.0193d + _G * 0.1192d + _B * 0.9505d;

            return xyz;

        }
        public static cXYZ RGBtoXYZ(Color rgb)
        {
            cXYZ xyz = RGBtoXYZ(rgb.R, rgb.G, rgb.B);
            return xyz;
        }
        public static cLAB RGBtoLAB(int r, int g, int b)
        {
            cXYZ xyz = RGBtoXYZ(r, g, b);
            cLAB lab = XYZtoLAB(xyz);
            return lab;
        }
        public static cLAB RGBtoLAB(Color rgb)
        {
            cXYZ xyz = RGBtoXYZ(rgb.R, rgb.G, rgb.B);
            cLAB lab = XYZtoLAB(xyz);
            return lab;
        }
        public static cLAB XYZtoLAB(double X, double Y, double Z)
        {
            double _X = X / 95.047d;
            double _Y = Y / 100d;
            double _Z = Z / 108.883d;

            _X = _X > 0.008856d ? Math.Pow(_X, (1d / 3d)) : (7.787d * _X) + (16d / 116d);
            _Y = _Y > 0.008856d ? Math.Pow(_Y, (1d / 3d)) : (7.787d * _Y) + (16d / 116d);
            _Z = _Z > 0.008856d ? Math.Pow(_Z, (1d / 3d)) : (7.787d * _Z) + (16d / 116d);

            cLAB lab = new cLAB();
            lab.L = (116d * _Y) - 16d; // helderheid 0-100
            lab.a = 500d * (_X - _Y); // groen-rood -127 - 127
            lab.b = 200d * (_Y - _Z); // blauw-geel -127 - 127

            return lab;

        }
        public static cLAB XYZtoLAB(cXYZ XYZ)
        {
            cLAB lab = XYZtoLAB(XYZ.X, XYZ.Y, XYZ.Z);
            return lab;
        }

        public static Color XYZtoRGB (double X, double Y, double Z)
        {
            double _X = X / 100d;
            double _Y = Y / 100d;
            double _Z = Z / 100d;

            double _R = _X * 3.2406d + _Y * -1.5372d + _Z * -0.4986d;
            double _G = _X * -0.9689d + _Y * 1.8758d + _Z * 0.0415d;
            double _B = _X * 0.0557d + _Y * -0.2040d + _Z * 1.0570d;

            _R = _R > 0.0031308d ? 1.055d * Math.Pow(_R, 1d / 2.4d) - 0.055d : _R = 12.92d * _R;
            _G = _G > 0.0031308d ? 1.055d * Math.Pow(_G, 1d / 2.4d) - 0.055d : _G = 12.92d * _G;
            _B = _B > 0.0031308d ? 1.055d * Math.Pow(_B, 1d / 2.4d) - 0.055d : _B = 12.92d * _B;

            int R = Convert.ToInt32(_R * 255);
            int G = Convert.ToInt32(_G * 255);
            int B = Convert.ToInt32(_B * 255);

            return Color.FromArgb(R, G, B);

        }
        public static Color XYZtoRGB(cXYZ XYZ)
        {
            Color rgb = XYZtoRGB(XYZ);
            return rgb;
        }
        public static cXYZ LABtoXYZ(double L, double a, double b)
        {
            double _Y = (L + 16d) / 116d;
            double _X = a / 500d + _Y;
            double _Z = _Y - b / 200d;

            _X = Math.Pow(_X, 3d) > 0.008856d ? Math.Pow(_X, 3d) : (_X - 16d / 116d) / 7.787d;
            _Y = Math.Pow(_Y, 3d) > 0.008856d ? Math.Pow(_Y, 3d) : (_Y - 16d / 116d) / 7.787d;
            _Z = Math.Pow(_Z, 3d) > 0.008856d ? Math.Pow(_Z, 3d) : (_Z - 16d / 116d) / 7.787d;

            cXYZ xyz = new cXYZ();
            xyz.X = 95.047d * _X;
            xyz.Y = 100d * _Y;
            xyz.Z = 108.883d * _Z;

            return xyz;
        }
        public static cXYZ LABtoXYZ(cLAB LAB)
        {
            cXYZ xyz = LABtoXYZ(LAB.L, LAB.a, LAB.b);
            return xyz;
        }
        public static Color LABtoRGB(double L, double a, double b)
        {
            cXYZ xyz = LABtoXYZ(L, a, b);
            Color rgb = XYZtoRGB(xyz);
            return rgb;
        }
        public static Color LABtoRGB(cLAB LAB)
        {
            cXYZ xyz = LABtoXYZ(LAB);
            Color rgb = XYZtoRGB(xyz);
            return rgb;
        }

        public static double LABtoDelta(cLAB c1, cLAB c2)
        {
            try
            {
                double _L = Math.Pow(c1.L - c2.L, 2);
                double _a = Math.Pow(c1.a - c2.a, 2);
                double _b = Math.Pow(c1.b - c2.b, 2);

                double Delta = Math.Sqrt(_L + _a + _b);

                return Delta;
            }
            catch
            {
                return 0;
            }
        }
        public static double XYZtoDelta(cXYZ c1, cXYZ c2)
        {
            cLAB lab1 = XYZtoLAB(c1);
            cLAB lab2 = XYZtoLAB(c2);

            return LABtoDelta(lab1, lab2);
        }
        public static double RGBtoDelta(Color c1, Color c2)
        {
            cLAB lab1 = RGBtoLAB(c1);
            cLAB lab2 = RGBtoLAB(c2);

            return LABtoDelta(lab1, lab2);
        }
    }
    
    class cXYZ
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
    }
    class cLAB
    {
        public double L { get; set; } // Helderheid: 0 - 100
        public double a { get; set; } // Groen-rood: -127 - 127
        public double b { get; set; } // Blauw-geel: -127 - 127
    }
}
