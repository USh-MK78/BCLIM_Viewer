using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BCLIMLibrary.CTRImage
{
    public class ETC1ColorFormat
    {
        public readonly ETC1BitShiftComponent ETC1BitShiftComponent;
        public readonly ETC1BitSizeComponent ETC1BitSizeComponent;

        public ETC1ColorFormat(ETC1BitShiftComponent ETC1BitShiftComponent, ETC1BitSizeComponent ETC1BitSizeComponent)
        {
            this.ETC1BitShiftComponent = ETC1BitShiftComponent;
            this.ETC1BitSizeComponent = ETC1BitSizeComponent;
        }

        //The naming is based on the bit order when read out in the correct endianness
        public static readonly ETC1ColorFormat ARGB8888 = new ETC1ColorFormat(new ETC1BitShiftComponent(24, 16, 8, 0), new ETC1BitSizeComponent(8, 8, 8, 8));
        public static readonly ETC1ColorFormat ARGB3444 = new ETC1ColorFormat(new ETC1BitShiftComponent(12, 8, 4, 0), new ETC1BitSizeComponent(3, 4, 4, 4));
        public static readonly ETC1ColorFormat RGBA8888 = new ETC1ColorFormat(new ETC1BitShiftComponent(null, 24, 15, 8), new ETC1BitSizeComponent(8, 8, 8, 8));
        public static readonly ETC1ColorFormat RGBA4444 = new ETC1ColorFormat(new ETC1BitShiftComponent(null, 12, 8, 4), new ETC1BitSizeComponent(3, 4, 4, 4));
        public static readonly ETC1ColorFormat RGB888 = new ETC1ColorFormat(new ETC1BitShiftComponent(null, 16, 8, 0), new ETC1BitSizeComponent(null, 8, 8, 8));
        public static readonly ETC1ColorFormat RGB565 = new ETC1ColorFormat(new ETC1BitShiftComponent(null, 11, 5, 0), new ETC1BitSizeComponent(null, 5, 6, 5));
        public static readonly ETC1ColorFormat ARGB1555 = new ETC1ColorFormat(new ETC1BitShiftComponent(15, 10, 5, 0), new ETC1BitSizeComponent(1, 5, 5, 5));
        public static readonly ETC1ColorFormat XRGB1555 = new ETC1ColorFormat(new ETC1BitShiftComponent(null, 10, 5, 0), new ETC1BitSizeComponent(null, 5, 5, 5));
        public static readonly ETC1ColorFormat ABGR1555 = new ETC1ColorFormat(new ETC1BitShiftComponent(15, null, 5, 10), new ETC1BitSizeComponent(1, 5, 5, 5));
        public static readonly ETC1ColorFormat XBGR1555 = new ETC1ColorFormat(new ETC1BitShiftComponent(null, null, 5, 10), new ETC1BitSizeComponent(null, 5, 5, 5));
        public static readonly ETC1ColorFormat RGBA5551 = new ETC1ColorFormat(new ETC1BitShiftComponent(null, 11, 6, 1), new ETC1BitSizeComponent(1, 5, 5, 5));
    }

    /// <summary>
    /// ビットサイズを表す
    /// </summary>
    public struct ETC1BitSizeComponent
    {
        /// <summary>
        /// 
        /// </summary>
        public readonly int? ASize;
        public readonly int? RSize;
        public readonly int? GSize;
        public readonly int? BSize;

        /// <summary>
        /// Initialize ETC1BitSizeComponent
        /// </summary>
        /// <param name="ASize">Color A Size, Nullable</param>
        /// <param name="RSize">Color R Size, Nullable</param>
        /// <param name="GSize">Color G Size, Nullable</param>
        /// <param name="BSize">Color B Size, Nullable</param>
        public ETC1BitSizeComponent(int? ASize, int? RSize, int? GSize, int? BSize)
        {
            this.ASize = ASize;
            this.RSize = RSize;
            this.GSize = GSize;
            this.BSize = BSize;
        }
    }

    /// <summary>
    /// ビットシフトを表す
    /// </summary>
    public struct ETC1BitShiftComponent
    {
        public readonly int? AShift;
        public readonly int? RShift;
        public readonly int? GShift;
        public readonly int? BShift;

        /// <summary>
        /// Initialize ETC1ShiftFormat
        /// </summary>
        /// <param name="AShift">Color A Shift, Nullable</param>
        /// <param name="RShift">Color R Shift, Nullable</param>
        /// <param name="GShift">Color G Shift, Nullable</param>
        /// <param name="BShift">Color B Shift, Nullable</param>
        public ETC1BitShiftComponent(int? AShift, int? RShift, int? GShift, int? BShift)
        {
            this.AShift = AShift;
            this.RShift = RShift;
            this.GShift = GShift;
            this.BShift = BShift;
        }
    }

    public class ETC1ColorFormatConvert
    {
        //public static Bitmap Resize(Bitmap Original, int Width, int Height)
        //{
        //    if (Original.Width == Width && Original.Height == Height) return Original;
        //    Bitmap res = new Bitmap(Width, Height);
        //    using (Graphics g = Graphics.FromImage(res))
        //    {
        //        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
        //        g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
        //        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
        //        g.DrawImage(Original, 0, 0, Width, Height);
        //        g.Flush();
        //    }
        //    return res;
        //}

        public static uint ConvertColorFormat(uint InColor, ETC1ColorFormat InputFormat, ETC1ColorFormat OutputFormat)
        {
            if (InputFormat == OutputFormat) return InColor;
            //From color format to components:
            uint? A, R, G, B;
            uint? mask;
            if (InputFormat.ETC1BitSizeComponent.ASize == null) A = 255;
            else
            {
                mask = ~(0xFFFFFFFFu << InputFormat.ETC1BitSizeComponent.ASize);
                A = (((((InColor >> InputFormat.ETC1BitShiftComponent.AShift) & mask) * 255u) + mask / 2) / mask);
            }
            mask = ~(0xFFFFFFFFu << InputFormat.ETC1BitSizeComponent.RSize);
            R = ((((InColor >> InputFormat.ETC1BitShiftComponent.RShift) & mask) * 255u) + mask / 2) / mask;
            mask = ~(0xFFFFFFFFu << InputFormat.ETC1BitSizeComponent.GSize);
            G = ((((InColor >> InputFormat.ETC1BitShiftComponent.GShift) & mask) * 255u) + mask / 2) / mask;
            mask = ~(0xFFFFFFFFu << InputFormat.ETC1BitSizeComponent.BSize);
            B = ((((InColor >> InputFormat.ETC1BitShiftComponent.BShift) & mask) * 255u) + mask / 2) / mask;
            return ToColorFormat((uint)A, (uint)R, (uint)G, (uint)B, OutputFormat);
        }

        public static uint ToColorFormat(int R, int G, int B, ETC1ColorFormat OutputFormat)
        {
            return ToColorFormat(255u, (uint)R, (uint)G, (uint)B, OutputFormat);
        }

        public static uint ToColorFormat(int A, int R, int G, int B, ETC1ColorFormat OutputFormat)
        {
            return ToColorFormat((uint)A, (uint)R, (uint)G, (uint)B, OutputFormat);
        }

        public static uint ToColorFormat(uint R, uint G, uint B, ETC1ColorFormat OutputFormat)
        {
            return ToColorFormat(255u, R, G, B, OutputFormat);
        }

        public static uint ToColorFormat(uint A, uint R, uint G, uint B, ETC1ColorFormat OutputFormat)
        {
            uint? result = 0;
            uint? mask;
            if (OutputFormat.ETC1BitSizeComponent.ASize != null)
            {
                mask = ~(0xFFFFFFFFu << OutputFormat.ETC1BitSizeComponent.ASize);
                result |= ((A * mask + 127u) / 255u) << OutputFormat.ETC1BitShiftComponent.AShift;
            }
            mask = ~(0xFFFFFFFFu << OutputFormat.ETC1BitSizeComponent.RSize);
            result |= ((R * mask + 127u) / 255u) << OutputFormat.ETC1BitShiftComponent.RShift;
            mask = ~(0xFFFFFFFFu << OutputFormat.ETC1BitSizeComponent.GSize);
            result |= ((G * mask + 127u) / 255u) << OutputFormat.ETC1BitShiftComponent.GShift;
            mask = ~(0xFFFFFFFFu << OutputFormat.ETC1BitSizeComponent.BSize);
            result |= ((B * mask + 127u) / 255u) << OutputFormat.ETC1BitShiftComponent.BShift;
            return (uint)result;
        }
    }
}
