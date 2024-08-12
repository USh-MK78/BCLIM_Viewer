using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BCLIMLibrary.CTRImage
{
    public class ColorConverter
    {
        public class Decode
        {
            public static Color L8(byte Data)
            {
                return Color.FromArgb(0xFF, Data, Data, Data);
            }

            public static Color A8(byte Data)
            {
                return Color.FromArgb(Data, 0xFF, 0xFF, 0xFF);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="Data"></param>
            /// <returns></returns>
            public static Color LA4(byte Data)
            {
                //ETC1Color eTC1Color = ETC1Color.Default();
                byte R = (byte)(Data >> 4);
                byte G = (byte)(Data >> 4);
                byte B = (byte)(Data >> 4);
                byte A = (byte)(Data & 0x0F);
                return Color.FromArgb(A, R, G, B);
            }

            public static Color L4(byte Data)
            {
                byte value = (byte)(Data * 0x11);
                return Color.FromArgb(0xFF, value, value, value);
            }

            public static Color A4(byte Data)
            {
                byte value = (byte)(Data * 0x11);
                return Color.FromArgb(value, 0xFF, 0xFF, 0xFF);
            }

            public static Color LA8(ushort Data)
            {
                ushort value = (byte)((Data >> 8 & 0xFF));
                ushort A = (byte)(Data & 0xFF);

                return Color.FromArgb(A, value, value, value);
            }

            public static Color HILO8(ushort Data)
            {
                ushort R = (byte)(Data >> 8);
                ushort G = (byte)(Data & 0xFF);

                return Color.FromArgb(0xFF, R, G, 0xFF);
            }

            public static Color RGB565(ushort Data)
            {
                byte R = Converter.Convert5To8((byte)((Data >> 11) & 0x1F));
                byte G = (byte)(((Data >> 5) & 0x3F) * 4);
                byte B = Converter.Convert5To8((byte)(Data & 0x1F));
                return Color.FromArgb(0xFF, R, G, B);
            }
        }
    }
}
