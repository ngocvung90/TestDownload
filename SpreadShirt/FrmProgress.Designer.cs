namespace SpreadShirt
{
    partial class FrmProgress
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.progressHTTP = new System.Windows.Forms.ProgressBar();
            this.lbDesc = new System.Windows.Forms.Label();
            this.lbPercent = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // progressHTTP
            // 
            this.progressHTTP.Location = new System.Drawing.Point(17, 57);
            this.progressHTTP.Name = "progressHTTP";
            this.progressHTTP.Size = new System.Drawing.Size(468, 27);
            this.progressHTTP.TabIndex = 0;
            // 
            // lbDesc
            // 
            this.lbDesc.AutoSize = true;
            this.lbDesc.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbDesc.ForeColor = System.Drawing.Color.DarkRed;
            this.lbDesc.Location = new System.Drawing.Point(14, 29);
            this.lbDesc.Name = "lbDesc";
            this.lbDesc.Size = new System.Drawing.Size(41, 15);
            this.lbDesc.TabIndex = 1;
            this.lbDesc.Text = "label1";
            // 
            // lbPercent
            // 
            this.lbPercent.AutoSize = true;
            this.lbPercent.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbPercent.ForeColor = System.Drawing.Color.DarkRed;
            this.lbPercent.Location = new System.Drawing.Point(444, 96);
            this.lbPercent.Name = "lbPercent";
            this.lbPercent.Size = new System.Drawing.Size(41, 15);
            this.lbPercent.TabIndex = 2;
            this.lbPercent.Text = "label1";
            // 
            // FrmProgress
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.LightGoldenrodYellow;
            this.ClientSize = new System.Drawing.Size(497, 120);
            this.Controls.Add(this.lbPercent);
            this.Controls.Add(this.lbDesc);
            this.Controls.Add(this.progressHTTP);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "FrmProgress";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "FrmProgress";
            this.TopMost = true;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ProgressBar progressHTTP;
        private System.Windows.Forms.Label lbDesc;
        private System.Windows.Forms.Label lbPercent;
    }
}