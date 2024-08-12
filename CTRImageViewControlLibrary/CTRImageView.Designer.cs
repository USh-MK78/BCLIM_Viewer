namespace CTRImageViewControlLibrary
{
    partial class CTRImageView
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージド リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region コンポーネント デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を 
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.PictureBoxCTR = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.PictureBoxCTR)).BeginInit();
            this.SuspendLayout();
            // 
            // PictureBoxCTR
            // 
            this.PictureBoxCTR.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PictureBoxCTR.Location = new System.Drawing.Point(0, 0);
            this.PictureBoxCTR.Name = "PictureBoxCTR";
            this.PictureBoxCTR.Size = new System.Drawing.Size(251, 248);
            this.PictureBoxCTR.TabIndex = 0;
            this.PictureBoxCTR.TabStop = false;
            // 
            // CTRImageView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.PictureBoxCTR);
            this.Name = "CTRImageView";
            this.Size = new System.Drawing.Size(251, 248);
            this.Load += new System.EventHandler(this.CTRImageView_Load);
            ((System.ComponentModel.ISupportInitialize)(this.PictureBoxCTR)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox PictureBoxCTR;
    }
}
