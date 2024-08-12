using BCLIMLibrary.CTRImage;
using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Math = System.Math;

namespace BCLIMLibrary
{
    public class BCLIM
    {
        public byte[] BCLIMImageData { get; set; }

        public char[] CLIM_Header { get; set; }
        public byte[] BOM { get; set; }
        public short CLIM_HeaderSize { get; set; } //Default : 0x14 (20 byte)
        public byte[] Version { get; set; } //0x4
        public int FileSize { get; set; }
        public int UnknownData1 { get; set; }
        public IMAG IMAGData { get; set; }
        public int ImageDataSize { get; set; }

        public void ReadCLIM(BinaryReader br)
        {
            //long CurrentPos = br.BaseStream.Position;

            #region Move CLIM Header
            //Get FileSize
            long FileSizeLength = br.BaseStream.Length;

            //Move End of File
            br.BaseStream.Position = FileSizeLength;

            //Move CLIM Header
            br.BaseStream.Seek(-40, SeekOrigin.Current);
            #endregion

            CLIM_Header = br.ReadChars(4);
            if (new string(CLIM_Header) != "CLIM") throw new Exception("BCLIM : Error");

            BOM = br.ReadBytes(2);

            EndianConvert endianConvert = new EndianConvert(BOM);

            CLIM_HeaderSize = BitConverter.ToInt16(endianConvert.Convert(br.ReadBytes(2)), 0);
            Version = endianConvert.Convert(br.ReadBytes(4));
            FileSize = BitConverter.ToInt32(endianConvert.Convert(br.ReadBytes(4)), 0);
            UnknownData1 = BitConverter.ToInt32(endianConvert.Convert(br.ReadBytes(4)), 0);

            IMAGData.ReadIMAG(br, BOM);

            ImageDataSize = BitConverter.ToInt32(endianConvert.Convert(br.ReadBytes(4)), 0);

            if (ImageDataSize != 0)
            {
                //Move Start of File
                br.BaseStream.Seek(0, SeekOrigin.Begin);

                Component.Enum.ComponentSize componentSize = Component.Enum.GetComponentSize(IMAGData.CTRFormat);

                BCLIMImageData = new byte[ImageDataSize];

                //Read CTRImage (byte[])
                BCLIMImageData = br.ReadBytes(ImageDataSize);

                char[] check = br.ReadChars(4);
                if (new string(check) != "CLIM") throw new Exception("Error : Failed to ReadImage");
            }
        }

        public void WriteCLIM(BinaryWriter bw, Component.Enum.CTRFormat Format, Bitmap bitmap)
        {
            CLIM_HeaderSize = GetHeaderSize();
            FileSize = GetSize();

            BCLIMImageData = ImageConverter.FromBitmap(bitmap, Format);
            bw.Write(BCLIMImageData);

            bw.Write(CLIM_Header);
            bw.Write(BOM);
            bw.Write(CLIM_HeaderSize);
            bw.Write(Version);
            bw.Write(FileSize);
            bw.Write(UnknownData1);

            IMAGData.WriteIMAG(bw);

            bw.Write(BCLIMImageData.Length);
        }

        public short GetHeaderSize()
        {
            int size = CLIM_Header.Length + BOM.Length + sizeof(short) + Version.Length + sizeof(int) + sizeof(int);
            return (short)size;
        }

        public int GetSize()
        {
            int size = 0;
            size += BCLIMImageData.Length;
            size += CLIM_Header.Length + BOM.Length + sizeof(short) + Version.Length + sizeof(int) + sizeof(int) + IMAGData.GetSize() + sizeof(int);
            return size;
        }

        public BCLIM()
        {
            BCLIMImageData = new List<byte>().ToArray();

            CLIM_Header = "CLIM".ToCharArray();
            BOM = new byte[2];
            CLIM_HeaderSize = 0; //Default : 0x14 (20 byte)
            Version = new byte[4];
            FileSize = 0;
            UnknownData1 = 0;
            IMAGData = new IMAG();
            ImageDataSize = 0;
        }
    }

    public class ImageConverter
    {
        private static readonly int[,] ETC1Modifiers =
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

        //private static readonly int[] Bpp = { 32, 24, 16, 16, 16, 16, 16, 8, 8, 8, 4, 4, 4, 8 };

        private static readonly int[] TileOrder =
        {
             0,  1,   4,  5,
             2,  3,   6,  7,

             8,  9,  12, 13,
            10, 11,  14, 15
        };

        //public static int GetBpp(Component.Enum.CTRFormat Format) { return Bpp[(uint)Format]; }

        public static Bitmap ToBitmap(byte[] Data, int Width, int Height, Component.Enum.CTRFormat Format, bool ExactSize = false)
        {
            return ToBitmap(Data, 0, Width, Height, Format, ExactSize);
        }

        public static unsafe Bitmap ToBitmap(byte[] Data, int Offset, int Width, int Height, Component.Enum.CTRFormat Format, bool ExactSize = false)
        {
            if (Data == null || Data.Length < 1 || Offset < 0 || Offset >= Data.Length || Width < 1 || Height < 1) return null;
            if (ExactSize && ((Width % 8) != 0 || (Height % 8) != 0)) return null;
            int physicalwidth = Width;
            int physicalheight = Height;
            if (!ExactSize)
            {
                Width = 1 << (int)Math.Ceiling(Math.Log(Width, 2));
                Height = 1 << (int)Math.Ceiling(Math.Log(Height, 2));
            }
            Bitmap bitm = new Bitmap(physicalwidth, physicalheight);
            BitmapData d = bitm.LockBits(new Rectangle(0, 0, bitm.Width, bitm.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            uint* res = (uint*)d.Scan0;
            int offs = Offset;//0;
            int stride = d.Stride / 4;

            if (Format == Component.Enum.CTRFormat.RGBA8)
            {
                for (int y = 0; y < Height; y += 8)
                {
                    for (int x = 0; x < Width; x += 8)
                    {
                        for (int i = 0; i < 64; i++)
                        {
                            int x2 = i % 8;
                            if (x + x2 >= physicalwidth) continue;
                            int y2 = i / 8;
                            if (y + y2 >= physicalheight) continue;
                            int pos = TileOrder[x2 % 4 + y2 % 4 * 4] + 16 * (x2 / 4) + 32 * (y2 / 4);
                            res[(y + y2) * stride + x + x2] =
                                CTRImage.ETC1ColorFormatConvert.ConvertColorFormat(
                                    BitConverter.ToUInt32(Data, offs + pos * 4),
                                    ETC1ColorFormat.RGBA8888,
                                    ETC1ColorFormat.ARGB8888);
                            /*GFXUtil.ToArgb(
                            Data[offs + pos * 4],
                            Data[offs + pos * 4 + 3],
                            Data[offs + pos * 4 + 2],
                            Data[offs + pos * 4 + 1]
                            );*/
                        }
                        offs += 64 * 4;
                    }
                }
            }
            //else if (Format == Component.Enum.CTRFormat.RGB8)
            //{
            //    for (int y = 0; y < Height; y += 8)
            //    {
            //        for (int x = 0; x < Width; x += 8)
            //        {
            //            for (int i = 0; i < 64; i++)
            //            {
            //                int x2 = i % 8;
            //                if (x + x2 >= physicalwidth) continue;
            //                int y2 = i / 8;
            //                if (y + y2 >= physicalheight) continue;
            //                int pos = TileOrder[x2 % 4 + y2 % 4 * 4] + 16 * (x2 / 4) + 32 * (y2 / 4);
            //                res[(y + y2) * stride + x + x2] =
            //                    CTRImage.ETC1ColorFormatConvert.ConvertColorFormat(
            //                        IOUtil.ReadU24LE(Data, offs + pos * 3),
            //                        ETC1ColorFormat.RGB888,
            //                        ETC1ColorFormat.ARGB8888);
            //                /*GFXUtil.ToArgb(
            //                Data[offs + pos * 3 + 2],
            //                Data[offs + pos * 3 + 1],
            //                Data[offs + pos * 3 + 0]
            //                );*/
            //            }
            //            offs += 64 * 3;
            //        }
            //    }
            //}
            else if (Format == Component.Enum.CTRFormat.RGBA5551)
            {
                for (int y = 0; y < Height; y += 8)
                {
                    for (int x = 0; x < Width; x += 8)
                    {
                        for (int i = 0; i < 64; i++)
                        {
                            int x2 = i % 8;
                            if (x + x2 >= physicalwidth) continue;
                            int y2 = i / 8;
                            if (y + y2 >= physicalheight) continue;
                            int pos = TileOrder[x2 % 4 + y2 % 4 * 4] + 16 * (x2 / 4) + 32 * (y2 / 4);
                            res[(y + y2) * stride + x + x2] =
                                CTRImage.ETC1ColorFormatConvert.ConvertColorFormat(
                                    BitConverter.ToUInt16(Data, offs + pos * 2),
                                    ETC1ColorFormat.RGBA5551,
                                    ETC1ColorFormat.ARGB8888);
                            //GFXUtil.RGBA5551ToArgb(IOUtil.ReadU16LE(Data, offs + pos * 2));
                        }
                        offs += 64 * 2;
                    }
                }
            }
            else if (Format == Component.Enum.CTRFormat.RGB565)
            {
                for (int y = 0; y < Height; y += 8)
                {
                    for (int x = 0; x < Width; x += 8)
                    {
                        for (int i = 0; i < 64; i++)
                        {
                            int x2 = i % 8;
                            if (x + x2 >= physicalwidth) continue;
                            int y2 = i / 8;
                            if (y + y2 >= physicalheight) continue;
                            int pos = TileOrder[x2 % 4 + y2 % 4 * 4] + 16 * (x2 / 4) + 32 * (y2 / 4);
                            res[(y + y2) * stride + x + x2] =
                                CTRImage.ETC1ColorFormatConvert.ConvertColorFormat(
                                    BitConverter.ToUInt16(Data, offs + pos * 2),
                                    ETC1ColorFormat.RGB565,
                                    ETC1ColorFormat.ARGB8888);
                            //GFXUtil.RGB565ToArgb(IOUtil.ReadU16LE(Data, offs + pos * 2));
                        }
                        offs += 64 * 2;
                    }
                }
            }
            else if (Format == Component.Enum.CTRFormat.RGBA4)
            {
                for (int y = 0; y < Height; y += 8)
                {
                    for (int x = 0; x < Width; x += 8)
                    {
                        for (int i = 0; i < 64; i++)
                        {
                            int x2 = i % 8;
                            if (x + x2 >= physicalwidth) continue;
                            int y2 = i / 8;
                            if (y + y2 >= physicalheight) continue;
                            int pos = TileOrder[x2 % 4 + y2 % 4 * 4] + 16 * (x2 / 4) + 32 * (y2 / 4);
                            res[(y + y2) * stride + x + x2] = CTRImage.ETC1ColorFormatConvert.ConvertColorFormat(BitConverter.ToUInt16(Data, offs + pos * 2), ETC1ColorFormat.RGBA4444, ETC1ColorFormat.ARGB8888);
                                //GFXUtil.ConvertColorFormat(
                                //    IOUtil.ReadU16LE(Data, offs + pos * 2),
                                //    ColorFormat.RGBA4444,
                                //    ColorFormat.ARGB8888);
                            /*GFXUtil.ToArgb(
                            (byte)((Data[offs + pos * 2] & 0xF) * 0x11),
                            (byte)((Data[offs + pos * 2 + 1] >> 4) * 0x11),
                            (byte)((Data[offs + pos * 2 + 1] & 0xF) * 0x11),
                            (byte)((Data[offs + pos * 2] >> 4) * 0x11)
                            );*/
                        }
                        offs += 64 * 2;
                    }
                }
            }
            else if (Format == Component.Enum.CTRFormat.LA8)
            {
                for (int y = 0; y < Height; y += 8)
                {
                    for (int x = 0; x < Width; x += 8)
                    {
                        for (int i = 0; i < 64; i++)
                        {
                            int x2 = i % 8;
                            if (x + x2 >= physicalwidth) continue;
                            int y2 = i / 8;
                            if (y + y2 >= physicalheight) continue;
                            int pos = TileOrder[x2 % 4 + y2 % 4 * 4] + 16 * (x2 / 4) + 32 * (y2 / 4);
                            res[(y + y2) * stride + x + x2] = CTRImage.ETC1ColorFormatConvert.ToColorFormat(Data[offs + pos * 2], Data[offs + pos * 2 + 1], Data[offs + pos * 2 + 1], Data[offs + pos * 2 + 1], ETC1ColorFormat.ARGB8888 );
                        }
                        offs += 64 * 2;
                    }
                }
            }
            else if (Format == Component.Enum.CTRFormat.HILO8)
            {
                for (int y = 0; y < Height; y += 8)
                {
                    for (int x = 0; x < Width; x += 8)
                    {
                        for (int i = 0; i < 64; i++)
                        {
                            int x2 = i % 8;
                            if (x + x2 >= physicalwidth) continue;
                            int y2 = i / 8;
                            if (y + y2 >= physicalheight) continue;
                            int pos = TileOrder[x2 % 4 + y2 % 4 * 4] + 16 * (x2 / 4) + 32 * (y2 / 4);
                            res[(y + y2) * stride + x + x2] = CTRImage.ETC1ColorFormatConvert.ToColorFormat(
                                Data[offs + pos * 2],
                                Data[offs + pos * 2 + 1],
                                Data[offs + pos * 2 + 1],
                                Data[offs + pos * 2 + 1],
                                ETC1ColorFormat.ARGB8888
                                );
                        }
                        offs += 64 * 2;
                    }
                }
            }
            else if (Format == Component.Enum.CTRFormat.L8)
            {
                for (int y = 0; y < Height; y += 8)
                {
                    for (int x = 0; x < Width; x += 8)
                    {
                        for (int i = 0; i < 64; i++)
                        {
                            int x2 = i % 8;
                            if (x + x2 >= physicalwidth) continue;
                            int y2 = i / 8;
                            if (y + y2 >= physicalheight) continue;
                            int pos = TileOrder[x2 % 4 + y2 % 4 * 4] + 16 * (x2 / 4) + 32 * (y2 / 4);
                            res[(y + y2) * stride + x + x2] = CTRImage.ETC1ColorFormatConvert.ToColorFormat(
                                Data[offs + pos],
                                Data[offs + pos],
                                Data[offs + pos],
                                ETC1ColorFormat.ARGB8888
                                );
                        }
                        offs += 64;
                    }
                }
            }
            else if (Format == Component.Enum.CTRFormat.A8)
            {
                for (int y = 0; y < Height; y += 8)
                {
                    for (int x = 0; x < Width; x += 8)
                    {
                        for (int i = 0; i < 64; i++)
                        {
                            int x2 = i % 8;
                            if (x + x2 >= physicalwidth) continue;
                            int y2 = i / 8;
                            if (y + y2 >= physicalheight) continue;
                            int pos = TileOrder[x2 % 4 + y2 % 4 * 4] + 16 * (x2 / 4) + 32 * (y2 / 4);
                            res[(y + y2) * stride + x + x2] = CTRImage.ETC1ColorFormatConvert.ToColorFormat(
                                Data[offs + pos],
                                255,
                                255,
                                255,
                                ETC1ColorFormat.ARGB8888
                                );
                        }
                        offs += 64;
                    }
                }
            }
            else if (Format == Component.Enum.CTRFormat.LA4)
            {
                for (int y = 0; y < Height; y += 8)
                {
                    for (int x = 0; x < Width; x += 8)
                    {
                        for (int i = 0; i < 64; i++)
                        {
                            int x2 = i % 8;
                            if (x + x2 >= physicalwidth) continue;
                            int y2 = i / 8;
                            if (y + y2 >= physicalheight) continue;
                            int pos = TileOrder[x2 % 4 + y2 % 4 * 4] + 16 * (x2 / 4) + 32 * (y2 / 4);
                            res[(y + y2) * stride + x + x2] = CTRImage.ETC1ColorFormatConvert.ToColorFormat(
                                (byte)((Data[offs + pos] & 0xF) * 0x11),
                                (byte)((Data[offs + pos] >> 4) * 0x11),
                                (byte)((Data[offs + pos] >> 4) * 0x11),
                                (byte)((Data[offs + pos] >> 4) * 0x11),
                                ETC1ColorFormat.ARGB8888
                                );
                        }
                        offs += 64;
                    }
                }
            }
            else if (Format == Component.Enum.CTRFormat.L4)
            {
                for (int y = 0; y < Height; y += 8)
                {
                    for (int x = 0; x < Width; x += 8)
                    {
                        for (int i = 0; i < 64; i++)
                        {
                            int x2 = i % 8;
                            if (x + x2 >= physicalwidth) continue;
                            int y2 = i / 8;
                            if (y + y2 >= physicalheight) continue;
                            int pos = TileOrder[x2 % 4 + y2 % 4 * 4] + 16 * (x2 / 4) + 32 * (y2 / 4);
                            int shift = (pos & 1) * 4;
                            res[(y + y2) * stride + x + x2] = CTRImage.ETC1ColorFormatConvert.ToColorFormat(
                                (byte)(((Data[offs + pos / 2] >> shift) & 0xF) * 0x11),
                                (byte)(((Data[offs + pos / 2] >> shift) & 0xF) * 0x11),
                                (byte)(((Data[offs + pos / 2] >> shift) & 0xF) * 0x11),
                                ETC1ColorFormat.ARGB8888
                                );
                        }
                        offs += 64 / 2;
                    }
                }
            }
            else if (Format == Component.Enum.CTRFormat.A4)
            {
                for (int y = 0; y < Height; y += 8)
                {
                    for (int x = 0; x < Width; x += 8)
                    {
                        for (int i = 0; i < 64; i++)
                        {
                            int x2 = i % 8;
                            if (x + x2 >= physicalwidth) continue;
                            int y2 = i / 8;
                            if (y + y2 >= physicalheight) continue;
                            int pos = TileOrder[x2 % 4 + y2 % 4 * 4] + 16 * (x2 / 4) + 32 * (y2 / 4);
                            int shift = (pos & 1) * 4;
                            res[(y + y2) * stride + x + x2] = CTRImage.ETC1ColorFormatConvert.ToColorFormat(
                                (byte)(((Data[offs + pos / 2] >> shift) & 0xF) * 0x11),
                                255,
                                255,
                                255,
                                ETC1ColorFormat.ARGB8888
                                );
                        }
                        offs += 64 / 2;
                    }
                }
            }
            else if (Format == Component.Enum.CTRFormat.ETC1)
            {
                for (int y = 0; y < Height; y += 8)
                {
                    for (int x = 0; x < Width; x += 8)
                    {
                        for (int i = 0; i < 8; i += 4)
                        {
                            for (int j = 0; j < 8; j += 4)
                            {
                                ulong alpha = 0xFFFFFFFFFFFFFFFF;
                                ulong data = BitConverter.ToUInt64(Data, offs);
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
                                for (int y3 = 0; y3 < 4; y3++)
                                {
                                    for (int x3 = 0; x3 < 4; x3++)
                                    {
                                        if (x + j + x3 >= physicalwidth) continue;
                                        if (y + i + y3 >= physicalheight) continue;

                                        int val = (int)((data >> (x3 * 4 + y3)) & 0x1);
                                        bool neg = ((data >> (x3 * 4 + y3 + 16)) & 0x1) == 1;
                                        uint c;
                                        if ((flipbit && y3 < 2) || (!flipbit && x3 < 2))
                                        {
                                            int add = ImageConverter.ETC1Modifiers[Table1, val] * (neg ? -1 : 1);
                                            c = CTRImage.ETC1ColorFormatConvert.ToColorFormat((byte)(((alpha >> ((x3 * 4 + y3) * 4)) & 0xF) * 0x11), (byte)ColorClamp(r1 + add), (byte)ColorClamp(g1 + add), (byte)ColorClamp(b1 + add), ETC1ColorFormat.ARGB8888);
                                        }
                                        else
                                        {
                                            int add = ImageConverter.ETC1Modifiers[Table2, val] * (neg ? -1 : 1);
                                            c = CTRImage.ETC1ColorFormatConvert.ToColorFormat((byte)(((alpha >> ((x3 * 4 + y3) * 4)) & 0xF) * 0x11), (byte)ColorClamp(r2 + add), (byte)ColorClamp(g2 + add), (byte)ColorClamp(b2 + add), ETC1ColorFormat.ARGB8888);
                                        }
                                        res[(i + y3) * stride + x + j + x3] = c;
                                    }
                                }
                                offs += 8;
                            }
                        }
                    }
                    res += stride * 8;
                }
            }
            else if (Format == Component.Enum.CTRFormat.ETC1A4)
            {
                for (int y = 0; y < Height; y += 8)
                {
                    for (int x = 0; x < Width; x += 8)
                    {
                        for (int i = 0; i < 8; i += 4)
                        {
                            for (int j = 0; j < 8; j += 4)
                            {
                                ulong alpha = 0xFFFFFFFFFFFFFFFF;
                                if (Format == Component.Enum.CTRFormat.ETC1A4)
                                {
                                    alpha = BitConverter.ToUInt64(Data, offs);
                                    offs += 8;
                                }
                                ulong data = BitConverter.ToUInt64(Data, offs);
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
                                for (int y3 = 0; y3 < 4; y3++)
                                {
                                    for (int x3 = 0; x3 < 4; x3++)
                                    {
                                        if (x + j + x3 >= physicalwidth) continue;
                                        if (y + i + y3 >= physicalheight) continue;

                                        int val = (int)((data >> (x3 * 4 + y3)) & 0x1);
                                        bool neg = ((data >> (x3 * 4 + y3 + 16)) & 0x1) == 1;
                                        uint c;
                                        if ((flipbit && y3 < 2) || (!flipbit && x3 < 2))
                                        {
                                            int add = ETC1Modifiers[Table1, val] * (neg ? -1 : 1);
                                            c = CTRImage.ETC1ColorFormatConvert.ToColorFormat((byte)(((alpha >> ((x3 * 4 + y3) * 4)) & 0xF) * 0x11), (byte)ColorClamp(r1 + add), (byte)ColorClamp(g1 + add), (byte)ColorClamp(b1 + add), ETC1ColorFormat.ARGB8888);
                                        }
                                        else
                                        {
                                            int add = ETC1Modifiers[Table2, val] * (neg ? -1 : 1);
                                            c = CTRImage.ETC1ColorFormatConvert.ToColorFormat((byte)(((alpha >> ((x3 * 4 + y3) * 4)) & 0xF) * 0x11), (byte)ColorClamp(r2 + add), (byte)ColorClamp(g2 + add), (byte)ColorClamp(b2 + add), ETC1ColorFormat.ARGB8888);
                                        }
                                        res[(i + y3) * stride + x + j + x3] = c;
                                    }
                                }
                                offs += 8;
                            }
                        }
                    }
                    res += stride * 8;
                }
            }
            bitm.UnlockBits(d);
            return bitm;
        }


        public static unsafe byte[] FromBitmap(Bitmap Picture, Component.Enum.CTRFormat Format, bool ExactSize = false)
        {
            if (ExactSize && ((Picture.Width % 8) != 0 || (Picture.Height % 8) != 0)) return null;
            int physicalwidth = Picture.Width;
            int physicalheight = Picture.Height;
            int ConvWidth = Picture.Width;
            int ConvHeight = Picture.Height;
            if (!ExactSize)
            {
                ConvWidth = 1 << (int)Math.Ceiling(Math.Log(Picture.Width, 2));
                ConvHeight = 1 << (int)Math.Ceiling(Math.Log(Picture.Height, 2));
            }
            BitmapData d = Picture.LockBits(new Rectangle(0, 0, Picture.Width, Picture.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            int BitSize = Component.Enum.GetImageFormatBpp(Format);

            uint* res = (uint*)d.Scan0;
            byte[] result = new byte[ConvWidth * ConvHeight * BitSize / 8]; //Width * Height * (BitLength / 8) //GetBpp(Format)
            int offs = 0;

            if (Format == Component.Enum.CTRFormat.RGBA8)
            {
                for (int y = 0; y < ConvHeight; y += 8)
                {
                    for (int x = 0; x < ConvWidth; x += 8)
                    {
                        for (int i = 0; i < 64; i++)
                        {
                            int x2 = i % 8;
                            if (x + x2 >= physicalwidth) continue;
                            int y2 = i / 8;
                            if (y + y2 >= physicalheight) continue;
                            int pos = TileOrder[x2 % 4 + y2 % 4 * 4] + 16 * (x2 / 4) + 32 * (y2 / 4);
                            Color c = Color.FromArgb((int)res[(y + y2) * d.Stride / 4 + x + x2]);
                            result[offs + pos * 4 + 0] = c.A;
                            result[offs + pos * 4 + 1] = c.B;
                            result[offs + pos * 4 + 2] = c.G;
                            result[offs + pos * 4 + 3] = c.R;
                        }
                        offs += 64 * 4;
                    }
                }
            }
            else if (Format == Component.Enum.CTRFormat.RGB8)
            {
                for (int y = 0; y < ConvHeight; y += 8)
                {
                    for (int x = 0; x < ConvWidth; x += 8)
                    {
                        for (int i = 0; i < 64; i++)
                        {
                            int x2 = i % 8;
                            if (x + x2 >= physicalwidth) continue;
                            int y2 = i / 8;
                            if (y + y2 >= physicalheight) continue;
                            int pos = TileOrder[x2 % 4 + y2 % 4 * 4] + 16 * (x2 / 4) + 32 * (y2 / 4);
                            Color c = Color.FromArgb((int)res[(y + y2) * d.Stride / 4 + x + x2]);
                            result[offs + pos * 3 + 0] = c.B;
                            result[offs + pos * 3 + 1] = c.G;
                            result[offs + pos * 3 + 2] = c.R;
                        }
                        offs += 64 * 3;
                    }
                }
            }
            else if (Format == Component.Enum.CTRFormat.RGBA5551)
            {
                for (int y = 0; y < ConvHeight; y += 8)
                {
                    for (int x = 0; x < ConvHeight; x += 8)
                    {
                        for (int i = 0; i < 64; i++)
                        {
                            int x2 = i % 8;
                            if (x + x2 >= physicalwidth) continue;
                            int y2 = i / 8;
                            if (y + y2 >= physicalheight) continue;
                            int pos = TileOrder[x2 % 4 + y2 % 4 * 4] + 16 * (x2 / 4) + 32 * (y2 / 4);

                            byte[] bytes = BitConverter.GetBytes((ushort)ETC1ColorFormatConvert.ConvertColorFormat(res[(y + y2) * d.Stride / 4 + x + x2], ETC1ColorFormat.ARGB8888, ETC1ColorFormat.RGBA5551));

                            for (int len = 0; len < bytes.Length; len++)
                            {
                                result[(offs + pos * 2) + len] = bytes[len];
                            }
                        }
                        offs += 64 * 2;
                    }
                }
            }
            else if (Format == Component.Enum.CTRFormat.RGB565)
            {
                for (int y = 0; y < ConvHeight; y += 8)
                {
                    for (int x = 0; x < ConvWidth; x += 8)
                    {
                        for (int i = 0; i < 64; i++)
                        {
                            int x2 = i % 8;
                            if (x + x2 >= physicalwidth) continue;
                            int y2 = i / 8;
                            if (y + y2 >= physicalheight) continue;
                            int pos = TileOrder[x2 % 4 + y2 % 4 * 4] + 16 * (x2 / 4) + 32 * (y2 / 4);

                            byte[] bytes = BitConverter.GetBytes((ushort)CTRImage.ETC1ColorFormatConvert.ConvertColorFormat(res[(y + y2) * d.Stride / 4 + x + x2], ETC1ColorFormat.ARGB8888, ETC1ColorFormat.RGB565)); //GFXUtil.ArgbToRGB565(res[(y + y2) * d.Stride / 4 + x + x2]));

                            for (int len = 0; len < bytes.Length; len ++)
                            {
                                result[(offs + pos * 2) + len] = bytes[len];
                            }
                        }
                        offs += 64 * 2;
                    }
                }
            }
            else if (Format == Component.Enum.CTRFormat.RGBA4)
            {
                for (int y = 0; y < ConvHeight; y += 8)
                {
                    for (int x = 0; x < ConvWidth; x += 8)
                    {
                        for (int i = 0; i < 64; i++)
                        {
                            int x2 = i % 8;
                            if (x + x2 >= physicalwidth) continue;
                            int y2 = i / 8;
                            if (y + y2 >= physicalheight) continue;
                            int pos = TileOrder[x2 % 4 + y2 % 4 * 4] + 16 * (x2 / 4) + 32 * (y2 / 4);

                            byte[] bytes = BitConverter.GetBytes((ushort)CTRImage.ETC1ColorFormatConvert.ConvertColorFormat(res[(y + y2) * d.Stride / 4 + x + x2], ETC1ColorFormat.ARGB8888, ETC1ColorFormat.RGBA4444));

                            for (int len = 0; len < bytes.Length; len++)
                            {
                                result[(offs + pos * 2) + len] = bytes[len];
                            }
                        }
                        offs += 64 * 2;
                    }
                }
            }
            else if (Format == Component.Enum.CTRFormat.LA8)
            {
                for (int y = 0; y < ConvHeight; y += 8)
                {
                    for (int x = 0; x < ConvWidth; x += 8)
                    {
                        for (int i = 0; i < 64; i++)
                        {
                            int x2 = i % 8;
                            if (x + x2 >= physicalwidth) continue;
                            int y2 = i / 8;
                            if (y + y2 >= physicalheight) continue;
                            int pos = TileOrder[x2 % 4 + y2 % 4 * 4] + 16 * (x2 / 4) + 32 * (y2 / 4);
                            Color c = Color.FromArgb((int)res[(y + y2) * d.Stride / 4 + x + x2]);
                            result[offs + pos * 2] = c.A;
                            result[offs + pos * 2 + 1] = c.B;
                            result[offs + pos * 2 + 1] = c.G;
                            result[offs + pos * 2 + 1] = c.R;
                        }
                        offs += 64 * 2;
                    }
                }
            }
            else if (Format == Component.Enum.CTRFormat.HILO8)
            {
                for (int y = 0; y < ConvHeight; y += 8)
                {
                    for (int x = 0; x < ConvWidth; x += 8)
                    {
                        for (int i = 0; i < 64; i++)
                        {
                            int x2 = i % 8;
                            if (x + x2 >= physicalwidth) continue;
                            int y2 = i / 8;
                            if (y + y2 >= physicalheight) continue;
                            int pos = TileOrder[x2 % 4 + y2 % 4 * 4] + 16 * (x2 / 4) + 32 * (y2 / 4);
                            Color c = Color.FromArgb((int)res[(y + y2) * d.Stride / 4 + x + x2]);
                            //result[offs + pos * 2] = c.A;
                            //result[offs + pos * 2 + 1] = c.B;
                            //result[offs + pos * 2 + 1] = c.G;
                            //result[offs + pos * 2 + 1] = c.R;

                            //From ; png2bclim
                            result[offs + pos * 2] = (byte)((c.A) << 8);
                            result[offs + pos * 2 + 1] = (byte)((c.G) + ((c.R) << 8));
                            result[offs + pos * 2 + 1] = (byte)((c.G) + ((c.R) << 8));
                            result[offs + pos * 2 + 1] = (byte)((c.G) + ((c.R) << 8));

                        }
                        offs += 64 * 2;
                    }
                }
            }
            else if (Format == Component.Enum.CTRFormat.L8)
            {
                for (int y = 0; y < ConvHeight; y += 8)
                {
                    for (int x = 0; x < ConvWidth; x += 8)
                    {
                        for (int i = 0; i < 64; i++)
                        {
                            int x2 = i % 8;
                            if (x + x2 >= physicalwidth) continue;
                            int y2 = i / 8;
                            if (y + y2 >= physicalheight) continue;
                            int pos = TileOrder[x2 % 4 + y2 % 4 * 4] + 16 * (x2 / 4) + 32 * (y2 / 4);
                            Color c = Color.FromArgb((int)res[(y + y2) * d.Stride / 4 + x + x2]);

                            //From ; png2bclim
                            result[offs + pos] = (byte)(((0x1D3E * c.B) >> 16) & 0xFF);
                            result[offs + pos] = (byte)(((0x9691 * c.G) >> 16) & 0xFF);
                            result[offs + pos] = (byte)(((0x4C82 * c.R) >> 16) & 0xFF);
                        }
                        offs += 64;
                    }
                }
            }
            else if (Format == Component.Enum.CTRFormat.A8)
            {
                for (int y = 0; y < ConvHeight; y += 8)
                {
                    for (int x = 0; x < ConvWidth; x += 8)
                    {
                        for (int i = 0; i < 64; i++)
                        {
                            int x2 = i % 8;
                            if (x + x2 >= physicalwidth) continue;
                            int y2 = i / 8;
                            if (y + y2 >= physicalheight) continue;
                            int pos = TileOrder[x2 % 4 + y2 % 4 * 4] + 16 * (x2 / 4) + 32 * (y2 / 4);
                            Color c = Color.FromArgb((int)res[(y + y2) * d.Stride / 4 + x + x2]);
                            result[offs + pos] = (byte)(~c.A);
                        }
                        offs += 64;
                    }
                }
            }
            else if (Format == Component.Enum.CTRFormat.LA4)
            {
                for (int y = 0; y < ConvHeight; y += 8)
                {
                    for (int x = 0; x < ConvWidth; x += 8)
                    {
                        for (int i = 0; i < 64; i++)
                        {
                            int x2 = i % 8;
                            if (x + x2 >= physicalwidth) continue;
                            int y2 = i / 8;
                            if (y + y2 >= physicalheight) continue;
                            int pos = TileOrder[x2 % 4 + y2 % 4 * 4] + 16 * (x2 / 4) + 32 * (y2 / 4);
                            Color c = Color.FromArgb((int)res[(y + y2) * d.Stride / 4 + x + x2]);
                            byte v = (byte)((c.A / 0x11) + (c.R / 0x11) << 4);

                            result[offs + pos] = v;

                        }
                        offs += 64;
                    }
                }
            }
            else if (Format == Component.Enum.CTRFormat.L4)
            {
                for (int y = 0; y < ConvHeight; y += 8)
                {
                    for (int x = 0; x < ConvWidth; x += 8)
                    {
                        for (int i = 0; i < 64; i++)
                        {
                            int x2 = i % 8;
                            if (x + x2 >= physicalwidth) continue;
                            int y2 = i / 8;
                            if (y + y2 >= physicalheight) continue;
                            int pos = TileOrder[x2 % 4 + y2 % 4 * 4] + 16 * (x2 / 4) + 32 * (y2 / 4);
                            Color c = Color.FromArgb((int)res[(y + y2) * d.Stride / 4 + x + x2]);

                            //From ; png2bclim
                            result[offs + pos * 2] = c.A;
                            result[offs + pos * 2 + 1] = (byte)(res[(y + y2) * d.Stride / 4 + x + x2] * 0x11);
                            result[offs + pos * 2 + 1] = (byte)(res[(y + y2) * d.Stride / 4 + x + x2] * 0x11);
                            result[offs + pos * 2 + 1] = (byte)(res[(y + y2) * d.Stride / 4 + x + x2] * 0x11);
                        }
                        offs += 64 / 2;
                    }
                }
            }
            else if (Format == Component.Enum.CTRFormat.A4)
            {
                for (int y = 0; y < ConvHeight; y += 8)
                {
                    for (int x = 0; x < ConvWidth; x += 8)
                    {
                        for (int i = 0; i < 64; i++)
                        {
                            int x2 = i % 8;
                            if (x + x2 >= physicalwidth) continue;
                            int y2 = i / 8;
                            if (y + y2 >= physicalheight) continue;
                            int pos = TileOrder[x2 % 4 + y2 % 4 * 4] + 16 * (x2 / 4) + 32 * (y2 / 4);
                            Color c = Color.FromArgb((int)res[(y + y2) * d.Stride / 4 + x + x2]);

                            //From ; png2bclim
                            result[offs + pos * 2] = (byte)(res[(y + y2) * d.Stride / 4 + x + x2] * 0x11);
                            result[offs + pos * 2 + 1] = c.A;
                            result[offs + pos * 2 + 1] = c.A;
                            result[offs + pos * 2 + 1] = c.A;
                        }
                        offs += 64 / 2;
                    }
                }
            }
            else if (Format == Component.Enum.CTRFormat.ETC1)
            {
                for (int y = 0; y < ConvHeight; y += 8)
                {
                    for (int x = 0; x < ConvWidth; x += 8)
                    {
                        for (int i = 0; i < 8; i += 4)
                        {
                            for (int j = 0; j < 8; j += 4)
                            {
                                Color[] pixels = new Color[4 * 4];
                                for (int yy = 0; yy < 4; yy++)
                                {
                                    for (int xx = 0; xx < 4; xx++)
                                    {
                                        if (x + j + xx >= physicalwidth) pixels[yy * 4 + xx] = Color.Transparent;
                                        else if (y + i + yy >= physicalheight) pixels[yy * 4 + xx] = Color.Transparent;
                                        else pixels[yy * 4 + xx] = Color.FromArgb((int)res[((y + i + yy) * (d.Stride / 4)) + x + j + xx]);
                                    }
                                }

                                byte[] bytes = BitConverter.GetBytes(ETC1.GenETC1(pixels));

                                for (int len = 0; len < bytes.Length; len++)
                                {
                                    result[offs + len] = bytes[len];
                                }

                                offs += 8;
                            }
                        }
                    }
                }
            }
            else if (Format == Component.Enum.CTRFormat.ETC1A4)
            {
                for (int y = 0; y < ConvHeight; y += 8)
                {
                    for (int x = 0; x < ConvWidth; x += 8)
                    {
                        for (int i = 0; i < 8; i += 4)
                        {
                            for (int j = 0; j < 8; j += 4)
                            {
                                if (Format == Component.Enum.CTRFormat.ETC1A4)
                                {
                                    ulong alpha = 0;
                                    int iiii = 0;
                                    for (int xx = 0; xx < 4; xx++)
                                    {
                                        for (int yy = 0; yy < 4; yy++)
                                        {
                                            uint color;
                                            if (x + j + xx >= physicalwidth) color = 0x00FFFFFF;
                                            else if (y + i + yy >= physicalheight) color = 0x00FFFFFF;
                                            else color = res[((y + i + yy) * (d.Stride / 4)) + x + j + xx];
                                            uint a = color >> 24;
                                            a >>= 4;
                                            alpha |= (ulong)a << (iiii * 4);
                                            iiii++;
                                        }
                                    }

                                    byte[] alphabytes = BitConverter.GetBytes(alpha);

                                    for (int len = 0; len < alphabytes.Length; len++)
                                    {
                                        result[offs + len] = alphabytes[len];
                                    }


                                    //IOUtil.WriteU64LE(result, offs, alpha);
                                    offs += 8;
                                }
                                Color[] pixels = new Color[4 * 4];
                                for (int yy = 0; yy < 4; yy++)
                                {
                                    for (int xx = 0; xx < 4; xx++)
                                    {
                                        if (x + j + xx >= physicalwidth) pixels[yy * 4 + xx] = Color.Transparent;
                                        else if (y + i + yy >= physicalheight) pixels[yy * 4 + xx] = Color.Transparent;
                                        else pixels[yy * 4 + xx] = Color.FromArgb((int)res[((y + i + yy) * (d.Stride / 4)) + x + j + xx]);
                                    }
                                }

                                byte[] bytes = BitConverter.GetBytes(ETC1.GenETC1(pixels));

                                for (int len = 0; len < bytes.Length; len++)
                                {
                                    result[offs + len] = bytes[len];
                                }

                                //IOUtil.WriteU64LE(result, offs, ETC1.GenETC1(pixels));
                                offs += 8;
                            }
                        }
                    }
                }
            }

            return result;
        }

        private static int ColorClamp(int Color)
        {
            if (Color > 255) Color = 255;
            if (Color < 0) Color = 0;
            return Color;
        }
    }
}
