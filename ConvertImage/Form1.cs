using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ImageManager;
using System.IO;
using System.Threading;

namespace ConvertImage
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        int w, h;
        string path = "";
        private void button1_Click(object sender, EventArgs e)
        {
            w = -1; h = -1;
            path = "";
            bool retW = Int32.TryParse(txtWidth.Text, out w);
            bool retH = Int32.TryParse(txtHeight.Text, out h);
            if (retW && retH)
            {
                openFileDialog1 = new OpenFileDialog();
                openFileDialog1.InitialDirectory = @"C:\";
                openFileDialog1.Title = "Browse png Files";
                openFileDialog1.Filter = "png files (*.png)|*.png|All files (*.*)|*.*";
                openFileDialog1.CheckFileExists = true;
                openFileDialog1.CheckPathExists = true;
                openFileDialog1.RestoreDirectory = true;
                this.openFileDialog1.Multiselect = false;

                openFileDialog1.ShowDialog();

                path = openFileDialog1.FileName;
                Thread t = new Thread(new ThreadStart(DoConvert));
                progressConvert.Value = 20;
                t.Start();
            }
            else
            {
                MessageBox.Show("Invalid Width or Height, check again.");
            }
        }
        private void DoConvert() 
        {
            string newPath = "";
            IImageInfo imgBaseMerch = WebManager.GetImageInfo(File.ReadAllBytes(path));
            imgBaseMerch.FileName = Path.GetFileName(path);
            imgBaseMerch.ContentType = Path.GetExtension(path);
            //update UI
            this.BeginInvoke((Action)delegate ()
            {
                progressConvert.Value = 40;
            });

            imgBaseMerch.Path = "merch";
            IImageInfo imgMerch = imgBaseMerch.ResizeMe(h, w);
            //update UI
            this.BeginInvoke((Action)delegate ()
            {
                progressConvert.Value = 80;
            });
            newPath = Path.GetDirectoryName(path) + @"\" + imgBaseMerch.Path;
            if (!Directory.Exists(newPath))
                Directory.CreateDirectory(newPath);
            imgMerch.Save(newPath);
            //update UI
            this.BeginInvoke((Action)delegate ()
            {
                progressConvert.Value = 100;
                MessageBox.Show("Finished !");
                progressConvert.Value = 0;
            });
        }
    }
}