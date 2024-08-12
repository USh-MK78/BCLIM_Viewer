using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CTRImageViewControlLibrary
{
    public partial class CTRImageView: UserControl
    {
        public CTRImageView()
        {
            InitializeComponent();
        }

        public int Bitmap_SizeX { get; set; }
        public int Bitmap_SizeY { get; set; }

        public Bitmap Image { get; set; }

        private void CTRImageView_Load(object sender, EventArgs e)
        {
            PictureBoxCTR.Image = Image;
        }
    }
}
