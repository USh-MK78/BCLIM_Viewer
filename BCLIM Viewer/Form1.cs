using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BCLIMLibrary;

namespace BCLIM_Viewer
{
    public partial class Form1 : Form
    {
        public BCLIM BCLIMData { get; set; }

        public Form1()
        {
            InitializeComponent();

            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
        }

        private void openBCLIMToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog Open_BCLIM = new OpenFileDialog()
            {
                Title = "BCLIMを開く",
                InitialDirectory = Environment.CurrentDirectory,
                Filter = "bclim file|*.bclim"
            };

            if (Open_BCLIM.ShowDialog() == DialogResult.OK)
            {
                FileStream fs = new FileStream(Open_BCLIM.FileName, FileMode.Open, FileAccess.Read);
                BinaryReader br = new BinaryReader(fs);

                BCLIMData = new BCLIM();
                BCLIMData.ReadCLIM(br);

                br.Close();
                fs.Close();

                Bitmap b = BCLIMLibrary.ImageConverter.ToBitmap(BCLIMData.BCLIMImageData, BCLIMData.IMAGData.ImageWidth, BCLIMData.IMAGData.ImageHeight, BCLIMData.IMAGData.CTRFormat);

                pictureBox1.Image = b;
                //ctrImageView1.Image = b;

                BCLIM_PropertyGrid.SelectedObject = new BCLIM_PG(BCLIMData);
            }
            else return;
        }

        private void saveBCLIMToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog Save_BCLIM = new SaveFileDialog()
            {
                Title = "Save to BCLIM",
                InitialDirectory = Environment.CurrentDirectory,
                Filter = "bclim file|*.bclim"
            };
            if (Save_BCLIM.ShowDialog() == DialogResult.OK)
            {
                FileStream fs = new FileStream(Save_BCLIM.FileName, FileMode.Create, FileAccess.Write);
                BinaryWriter bw = new BinaryWriter(fs);


                BCLIMData.WriteCLIM(bw, BCLIMData.IMAGData.CTRFormat, new Bitmap(pictureBox1.Image));

                bw.Close();
                fs.Close();
            }
            else return;
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BCLIMData = null;
            pictureBox1.Image = null;
            BCLIM_PropertyGrid.SelectedObject = null;
        }

        private void pngToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog Save_PNG = new SaveFileDialog()
            {
                Title = "Save to PNG",
                InitialDirectory = Environment.CurrentDirectory,
                Filter = "png file|*.png"
            };
            if (Save_PNG.ShowDialog() == DialogResult.OK)
            {
                Bitmap bitmap = new Bitmap(pictureBox1.Image);
                bitmap.Save(Save_PNG.FileName, System.Drawing.Imaging.ImageFormat.Png);
                bitmap.Dispose();
            }
            else return;

            //var t = BCLIMLibrary.ImageConverter.FromBitmap(new Bitmap(pictureBox1.Image), BCLIMData.IMAGData.CTRFormat);
        }
    }
}
