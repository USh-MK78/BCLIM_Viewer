using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BCLIMLibrary
{
    /// <summary>
    /// IMAG (imag)
    /// </summary>
    public class IMAG
    {
        public char[] IMAG_Header { get; set; }
        public int HeaderSize { get; set; }
        public short ImageWidth { get; set; }
        public short ImageHeight { get; set; }
        public int ImageFormat { get; set; }

        public Component.Enum.CTRFormat CTRFormat
        {
            get
            {
                return (Component.Enum.CTRFormat)Enum.ToObject(typeof(Component.Enum.CTRFormat), ImageFormat);
            }
            set
            {
                ImageFormat = (int)value;
            }
        }

        public void ReadIMAG(BinaryReader br, byte[] BOM)
        {
            IMAG_Header = br.ReadChars(4);
            if (new string(IMAG_Header) != "imag") throw new Exception("imag : Error");

            EndianConvert endianConvert = new EndianConvert(BOM);
            HeaderSize = BitConverter.ToInt32(endianConvert.Convert(br.ReadBytes(4)), 0);
            ImageWidth = BitConverter.ToInt16(endianConvert.Convert(br.ReadBytes(2)), 0);
            ImageHeight = BitConverter.ToInt16(endianConvert.Convert(br.ReadBytes(2)), 0);
            ImageFormat = BitConverter.ToInt32(endianConvert.Convert(br.ReadBytes(4)), 0);
        }

        public void WriteIMAG(BinaryWriter bw)
        {
            bw.Write(IMAG_Header);
            bw.Write(HeaderSize);
            bw.Write(ImageWidth);
            bw.Write(ImageHeight);
            bw.Write(ImageFormat);
        }

        public int GetSize()
        {
            int size = IMAG_Header.Length + sizeof(int) + sizeof(short) + sizeof(short) + sizeof(int);
            return size;
        }

        /// <summary>
        /// Initialize IMAG
        /// </summary>
        /// <param name="ImageWidth">Width</param>
        /// <param name="ImageHeight">Height</param>
        /// <param name="Format">CTR Format</param>
        public IMAG(short ImageWidth, short ImageHeight, Component.Enum.CTRFormat Format)
        {
            IMAG_Header = "imag".ToCharArray();
            HeaderSize = 16;
            this.ImageWidth = ImageWidth;
            this.ImageHeight = ImageHeight;
            CTRFormat = Format;
        }

        /// <summary>
        /// Initialize IMAG
        /// </summary>
        public IMAG()
        {
            IMAG_Header = "imag".ToCharArray();
            HeaderSize = 16;
            ImageWidth = 0;
            ImageHeight = 0;
            ImageFormat = 0;
        }
    }
}
