
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Microsoft.SqlServer.Server;
using System.Drawing.Imaging;

namespace BCLIMLibrary.CTRImage
{
    public struct ETC1Color
    {
        public int A { get; set; }
        public int R { get; set; }
        public int G { get; set; }
        public int B { get; set; }

        public Color ToColor()
        {
            return Color.FromArgb(A, R, G, B);
        }

        public static Color L8(byte Data)
        {
            return new ETC1Color(0xFF, Data, Data, Data).ToColor();
        }

        public static Color A8(byte Data)
        {
            return new ETC1Color(Data, 0xFF, 0xFF, 0xFF).ToColor();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Data"></param>
        /// <returns></returns>
        public static Color LA4(byte Data)
        {
            ETC1Color eTC1Color = ETC1Color.Default();
            eTC1Color.R = (byte)(Data >> 4);
            eTC1Color.G = (byte)(Data >> 4);
            eTC1Color.B = (byte)(Data >> 4);
            eTC1Color.A = (byte)(Data & 0x0F);
            return eTC1Color.ToColor();
        }

        public static Color L4(byte Data)
        {
            byte value = (byte)(Data * 0x11);
            return new ETC1Color(0xFF, value, value, value).ToColor();
        }

        public static Color A4(byte Data)
        {
            byte value = (byte)(Data * 0x11);
            return new ETC1Color(value, 0xFF, 0xFF, 0xFF).ToColor();
        }

        public static Color LA8(ushort Data)
        {
            byte value = (byte)((Data >> 8 & 0xFF));
            byte A = (byte)(Data & 0xFF);

            return new ETC1Color(A, value, value, value).ToColor();
        }

        public static Color HILO8(ushort Data)
        {
            byte R = (byte)(Data >> 8);
            byte G = (byte)(Data & 0xFF);

            return new ETC1Color(0xFF, R, G, 0xFF).ToColor();
        }

        public static Color RGB565(ushort Data)
        {
            byte R = Converter.Convert5To8((byte)((Data >> 11) & 0x1F), 8);
            byte G = (byte)(((Data >> 5) & 0x3F) * 4);
            byte B = Converter.Convert5To8((byte)(Data & 0x1F), 8);
            return new ETC1Color(0xFF, R, G, B).ToColor();
            //return Color.FromArgb(0xFF, R, G, B);
        }



        //public static Color ETC1(ulong data)
        //{
        //    ulong alpha = 0xFFFFFFFFFFFFFFFF;
        //    bool diffbit = ((data >> 33) & 1) == 1;
        //    bool flipbit = ((data >> 32) & 1) == 1; //0: |||, 1: |-|
        //    int r1, r2, g1, g2, b1, b2;
        //    if (diffbit) //'differential' mode
        //    {
        //        int r = (int)((data >> 59) & 0x1F);
        //        int g = (int)((data >> 51) & 0x1F);
        //        int b = (int)((data >> 43) & 0x1F);
        //        r1 = (r << 3) | ((r & 0x1C) >> 2);
        //        g1 = (g << 3) | ((g & 0x1C) >> 2);
        //        b1 = (b << 3) | ((b & 0x1C) >> 2);
        //        r += (int)((data >> 56) & 0x7) << 29 >> 29;
        //        g += (int)((data >> 48) & 0x7) << 29 >> 29;
        //        b += (int)((data >> 40) & 0x7) << 29 >> 29;
        //        r2 = (r << 3) | ((r & 0x1C) >> 2);
        //        g2 = (g << 3) | ((g & 0x1C) >> 2);
        //        b2 = (b << 3) | ((b & 0x1C) >> 2);
        //    }
        //    else //'individual' mode
        //    {
        //        r1 = (int)((data >> 60) & 0xF) * 0x11;
        //        g1 = (int)((data >> 52) & 0xF) * 0x11;
        //        b1 = (int)((data >> 44) & 0xF) * 0x11;
        //        r2 = (int)((data >> 56) & 0xF) * 0x11;
        //        g2 = (int)((data >> 48) & 0xF) * 0x11;
        //        b2 = (int)((data >> 40) & 0xF) * 0x11;
        //    }
        //    int Table1 = (int)((data >> 37) & 0x7);
        //    int Table2 = (int)((data >> 34) & 0x7);
        //    for (int y3 = 0; y3 < 4; y3++)
        //    {
        //        for (int x3 = 0; x3 < 4; x3++)
        //        {
        //            if (x + j + x3 >= physicalwidth) continue;
        //            if (y + i + y3 >= physicalheight) continue;

        //            int val = (int)((data >> (x3 * 4 + y3)) & 0x1);
        //            bool neg = ((data >> (x3 * 4 + y3 + 16)) & 0x1) == 1;
        //            uint c;
        //            if ((flipbit && y3 < 2) || (!flipbit && x3 < 2))
        //            {
        //                int add = ETC1Modifiers[Table1, val] * (neg ? -1 : 1);
        //                c = GFXUtil.ToColorFormat((byte)(((alpha >> ((x3 * 4 + y3) * 4)) & 0xF) * 0x11), (byte)ColorClamp(r1 + add), (byte)ColorClamp(g1 + add), (byte)ColorClamp(b1 + add), ColorFormat.ARGB8888);
        //            }
        //            else
        //            {
        //                int add = ETC1Modifiers[Table2, val] * (neg ? -1 : 1);
        //                c = GFXUtil.ToColorFormat((byte)(((alpha >> ((x3 * 4 + y3) * 4)) & 0xF) * 0x11), (byte)ColorClamp(r2 + add), (byte)ColorClamp(g2 + add), (byte)ColorClamp(b2 + add), ColorFormat.ARGB8888);
        //            }
        //            res[(i + y3) * stride + x + j + x3] = c;
        //        }
        //    }
        //}

        public static Color[] ETC1A4(ulong alpha, ulong data)
        {
            //ulong alpha = 0xFFFFFFFFFFFFFFFF;
            //if (Format == ImageFormat.ETC1A4)
            //{
            //    this.alpha = alpha;
            //    offs += 8;
            //}
            //ulong data = IOUtil.ReadU64LE(Data, offs);
            bool diffbit = ((data >> 33) & 1) == 1;
            bool flipbit = ((data >> 32) & 1) == 1; //0: |||, 1: |-|
            int r1, r2, g1, g2, b1, b2;
            if (diffbit) //'differential' mode
            {
                int r = (int)((data >> 59) & 0x1F);
                int g = (int)((data >> 51) & 0x1F);
                int b = (int)((data >> 43) & 0x1F);
                r1 = (r << 3) | ((r & 0x1C) >> 2);
                g1 = (g << 3) | ((g & 0x1C) >> 2);
                b1 = (b << 3) | ((b & 0x1C) >> 2);
                r += (int)((data >> 56) & 0x7) << 29 >> 29;
                g += (int)((data >> 48) & 0x7) << 29 >> 29;
                b += (int)((data >> 40) & 0x7) << 29 >> 29;
                r2 = (r << 3) | ((r & 0x1C) >> 2);
                g2 = (g << 3) | ((g & 0x1C) >> 2);
                b2 = (b << 3) | ((b & 0x1C) >> 2);
            }
            else //'individual' mode
            {
                r1 = (int)((data >> 60) & 0xF) * 0x11;
                g1 = (int)((data >> 52) & 0xF) * 0x11;
                b1 = (int)((data >> 44) & 0xF) * 0x11;
                r2 = (int)((data >> 56) & 0xF) * 0x11;
                g2 = (int)((data >> 48) & 0xF) * 0x11;
                b2 = (int)((data >> 40) & 0xF) * 0x11;
            }
            int Table1 = (int)((data >> 37) & 0x7);
            int Table2 = (int)((data >> 34) & 0x7);

            Color[] colors = new Color[4];

            //Color : Ex. 0xFFFFFFFF00000000 => [0xFFFFFFFF] [0x00000000]
            for (int y3 = 0; y3 < 4; y3++)
            {
                //RGBA
                uint[] RGBA = new uint[4];
                for (int x3 = 0; x3 < 4; x3++)
                {
                    //if (x + j + x3 >= width) continue;
                    //if (y + i + y3 >= height) continue;

                    int val = (int)((data >> (x3 * 4 + y3)) & 0x1);
                    bool neg = ((data >> (x3 * 4 + y3 + 16)) & 0x1) == 1;
                    uint c;
                    if ((flipbit && y3 < 2) || (!flipbit && x3 < 2))
                    {
                        int add = Converter.ETC1Modifiers[Table1, val] * (neg ? -1 : 1);
                        c = ETC1ColorFormatConvert.ToColorFormat((byte)(((alpha >> ((x3 * 4 + y3) * 4)) & 0xF) * 0x11), (byte)Converter.ColorClamp(r1 + add), (byte)Converter.ColorClamp(g1 + add), (byte)Converter.ColorClamp(b1 + add), ETC1ColorFormat.ARGB8888);
                    }
                    else
                    {
                        int add = Converter.ETC1Modifiers[Table2, val] * (neg ? -1 : 1);
                        c = ETC1ColorFormatConvert.ToColorFormat((byte)(((alpha >> ((x3 * 4 + y3) * 4)) & 0xF) * 0x11), (byte)Converter.ColorClamp(r2 + add), (byte)Converter.ColorClamp(g2 + add), (byte)Converter.ColorClamp(b2 + add), ETC1ColorFormat.ARGB8888);
                    }
                    RGBA[x3] = c;
                }

                
                //foreach (var d in RGBA)
                //{
                //    Color color = Color.FromArgb((byte)d, (byte)d, (byte)d, (byte)d);
                //    colors[y3] = color;
                //}

                colors[y3] = Color.FromArgb((byte)RGBA[0], (byte)RGBA[1], (byte)RGBA[2], (byte)RGBA[3]);
            }

            return colors;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="InitializeBit"></param>
        /// <returns></returns>
        public static ETC1Color Default(byte InitializeBit = 0x00)
        {
            return new ETC1Color(InitializeBit, InitializeBit, InitializeBit, InitializeBit);
        }

        //public ETC1Color(byte A, byte R, byte G, byte B)
        //{
        //    this.A = A;
        //    this.R = R;
        //    this.G = G;
        //    this.B = B;
        //}

        public ETC1Color(int A, int R, int G, int B)
        {
            this.A = A;
            this.R = R;
            this.G = G;
            this.B = B;
        }
    }

    public class ColorFormat
    {
        //public static ETC1Color L8(byte Data)
        //{
        //    return new ETC1Color(0xFF, Data, Data, Data);
        //}

        //public static ETC1Color A8(byte Data)
        //{
        //    return new ETC1Color(Data, 0xFF, 0xFF, 0xFF);
        //}

        //public static ETC1Color LA4(byte Data)
        //{
        //    ETC1Color eTC1Color = new ETC1Color();
        //    eTC1Color.R = (byte)(Data >> 4);
        //    eTC1Color.A = (byte)(Data & 0xFF);
        //    return eTC1Color
        //}
    }

    public class Converter
    {

        public static readonly int[,] ETC1Modifiers =
        {
            { 2, 8 },
            { 5, 17 },
            { 9, 29 },
            { 13, 42 },
            { 18, 60 },
            { 24, 80 },
            { 33, 106 },
            { 47, 183 }
        };


        public static int ColorClamp(int Color)
        {
            if (Color > 255) Color = 255;
            if (Color < 0) Color = 0;
            return Color;
        }

        public static byte[] Generate5to8(int Inc = 8)
        {
            //byte[] ary = new byte[32];
            List<byte> ary = new List<byte>();
            for (int i = 0; i <= 0xFF; i = i + Inc)
            {
                ary.Add((byte)i);
            }

            return ary.ToArray();
        }

        public static byte Convert5To8(byte Data, int Inc = 8)
        {
            byte[] conv = Generate5to8(Inc);

            return conv[Data];
        }
    }
}
