using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BCLIMLibrary.Component
{
    public class Enum
    {
        /// <summary>
        /// CTRFormat
        /// </summary>
        public enum CTRFormat
        {
            L8 = 0,
            A8 = 1,
            LA4 = 2,
            LA8 = 3,
            HILO8 = 4,
            RGB565 = 5,
            RGB8 = 6,
            RGBA5551 = 7,
            RGBA4 = 8, //RGBA4444
            RGBA8 = 9, //RGBA8888
            ETC1 = 10,
            ETC1A4 = 11,
            L4 = 12,
            A4 = 13,
        }

        public enum ImageFormatBpp : uint
        {
            RGBA8 = 32,
            RGB8 = 24,
            RGBA5551 = 16,
            RGB565 = 16,
            RGBA4 = 16,
            LA8 = 16,
            HILO8 = 16,
            L8 = 8,
            A8 = 8,
            LA4 = 8,
            L4 = 4,
            A4 = 4,
            ETC1 = 4,
            ETC1A4 = 8,
        }

        /// <summary>
        /// Get CTRFormat Bit Size
        /// </summary>
        /// <param name="Format"></param>
        /// <returns></returns>
        public static int GetImageFormatBpp(CTRFormat Format)
        {
            string f = Format.ToString();

            ImageFormatBpp imageFormatBpp = (ImageFormatBpp)System.Enum.Parse(typeof(ImageFormatBpp), f);
            return (int)imageFormatBpp;
        }

        //private static readonly int[] Bpp = { 32, 24, 16, 16, 16, 16, 16, 8, 8, 8, 4, 4, 4, 8 };

        /// <summary>
        /// Color Component size
        /// </summary>
        public enum ComponentSize
        {
            /// <summary>
            /// DefaultValue
            /// </summary>
            NOTSET = -1,

            /// <summary>
            /// 4bit (1 nibble), Ex. 0xFF => 0x1111 ==> 0x1100, 0x0011
            /// </summary>
            BIT_4 = 0,

            /// <summary>
            /// 8bit (1 byte)
            /// </summary>
            BIT_8 = 1,

            /// <summary>
            /// 16bit (2 byte)
            /// </summary>
            BIT_16 = 2,

            /// <summary>
            /// 24bit (3 byte)
            /// </summary>
            BIT_24 = 3,

            /// <summary>
            /// 32bit (4 byte)
            /// </summary>
            BIT_32 = 4,

            /// <summary>
            /// 64bit (8 byte)
            /// </summary>
            BIT_64 = 5,

            /// <summary>
            /// 128bit (16 byte)
            /// </summary>
            BIT_128 = 6,
        }

        /// <summary>
        /// Get ComponentSize
        /// </summary>
        /// <param name="Format">CTRFormat</param>
        /// <returns>ComponentSize</returns>
        public static ComponentSize GetComponentSize(CTRFormat Format)
        {
            ComponentSize componentSize = ComponentSize.NOTSET;
            if (Format == CTRFormat.L4 || Format == CTRFormat.A4)
            {
                componentSize = ComponentSize.BIT_4;
            }
            else if (Format == CTRFormat.L8 || Format == CTRFormat.A8 || Format == CTRFormat.LA4)
            {
                componentSize = ComponentSize.BIT_8;
            }
            //if (Format == CTRFormat.L4 || Format == CTRFormat.A4 | Format == CTRFormat.ETC1)
            //{
            //    componentSize = ComponentSize.BIT_4;
            //}
            //else if (Format == CTRFormat.L8 || Format == CTRFormat.A8 || Format == CTRFormat.LA4 | Format == CTRFormat.ETC1A4)
            //{
            //    componentSize = ComponentSize.BIT_8;
            //}
            else if (Format == CTRFormat.LA8 || Format == CTRFormat.HILO8 || Format == CTRFormat.RGB565 || Format == CTRFormat.RGBA4 || Format == CTRFormat.RGBA5551)
            {
                componentSize = ComponentSize.BIT_16;
            }
            else if (Format == CTRFormat.RGB8)
            {
                componentSize = ComponentSize.BIT_24;
            }
            else if (Format == CTRFormat.RGBA8)
            {
                componentSize = ComponentSize.BIT_32;
            }
            else if (Format == CTRFormat.ETC1)
            {
                componentSize = ComponentSize.BIT_64;
            }
            else if (Format == CTRFormat.ETC1A4)
            {
                componentSize = ComponentSize.BIT_128;
            }

            return componentSize;
        }

        /// <summary>
        /// Get Byte Size
        /// </summary>
        /// <param name="componentSize"></param>
        /// <returns></returns>
        public static int GetComponentSizeInt(ComponentSize componentSize)
        {
            int size = -1;
            if (componentSize == ComponentSize.BIT_4 || componentSize == ComponentSize.BIT_8)
            {
                size = 1;
            }
            else if (componentSize == ComponentSize.BIT_16)
            {
                size = 2;
            }
            else if (componentSize == ComponentSize.BIT_24)
            {
                size = 3;
            }
            else if (componentSize == ComponentSize.BIT_32)
            {
                size = 4;
            }
            else if (componentSize == ComponentSize.BIT_64)
            {
                size = 8;
            }
            else if (componentSize == ComponentSize.BIT_128)
            {
                size = 16;
            }
            else if (componentSize == ComponentSize.NOTSET)
            {
                size = -1;
            }

            return size;
        }
    }
}
