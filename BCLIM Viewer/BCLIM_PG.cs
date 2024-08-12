using BCLIMLibrary;
using BCLIMComponent = BCLIMLibrary.Component;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BCLIM_Viewer
{
    [TypeConverter(typeof(BCLIM_CustomPropertyGridClass.CustomSortTypeConverter))]
    public class BCLIM_PG
    {
        [Browsable(false)]
        [ReadOnly(true)]
        public byte[] BCLIMImageData { get; set; }

        [ReadOnly(true)]
        public char[] CLIM_Header { get; set; }

        [ReadOnly(true)]
        public byte[] BOM { get; set; }

        [ReadOnly(true)]
        public short CLIM_HeaderSize { get; set; } //Default : 0x14 (20 byte)

        public byte[] Version { get; set; } //0x4

        [ReadOnly(true)]
        public int FileSize { get; set; }

        public int UnknownData1 { get; set; }

        [TypeConverter(typeof(BCLIM_CustomPropertyGridClass.CustomExpandableObjectSortTypeConverter))]
        public IMAG_INFO IMAGData { get; set; }
        public class IMAG_INFO
        {
            [ReadOnly(true)]
            public char[] IMAG_Header { get; set; }

            [ReadOnly(true)]
            public int HeaderSize { get; set; }

            public short ImageWidth { get; set; }
            public short ImageHeight { get; set; }
            public int ImageFormat { get; set; }

            public BCLIMComponent.Enum.CTRFormat CTRFormat
            {
                get
                {
                    return (BCLIMComponent.Enum.CTRFormat)Enum.ToObject(typeof(BCLIMComponent.Enum.CTRFormat), ImageFormat);
                }
                set
                {
                    ImageFormat = (int)value;
                }
            }

            //public IMAG ToIMAG()
            //{
            //    IMAG IMAGData = new IMAG();
            //    IMAGData.
            //}

            public IMAG_INFO(IMAG IMAG)
            {
                IMAG_Header = IMAG.IMAG_Header;
                HeaderSize = IMAG.HeaderSize;
                ImageWidth = IMAG.ImageWidth;
                ImageHeight = IMAG.ImageHeight;
                ImageFormat = IMAG.ImageFormat;
            }

            public override string ToString()
            {
                return "Image Info (IMAG)";
            }
        }

        public int ImageDataSize { get; set; }

        public BCLIM_PG(BCLIM BCLIMData)
        {
            BCLIMImageData = BCLIMData.BCLIMImageData;
            CLIM_Header = BCLIMData.CLIM_Header;
            BOM = BCLIMData.BOM;
            CLIM_HeaderSize = BCLIMData.CLIM_HeaderSize;
            Version = BCLIMData.Version;
            FileSize = BCLIMData.FileSize;
            UnknownData1 = BCLIMData.UnknownData1;
            IMAGData = new IMAG_INFO(BCLIMData.IMAGData);
            ImageDataSize = BCLIMData.ImageDataSize;
        }

        public override string ToString()
        {
            return "BCLIM Data";
        }
    }
}
