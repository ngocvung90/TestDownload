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
using System.Runtime.InteropServices;
using Excel = Microsoft.Office.Interop.Excel;
using ExcelUtil;

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

        string folderpath = "";
        private void DoExport()
        {
            string sunfrogPath = "sunfrog";
            string[] listFolder = Directory.GetFiles(folderpath, "*.png", SearchOption.AllDirectories);
            //2. Update to data table to export new Excel
            DataTable dtShirtsTS = new DataTable();
            DataTable dtShirtsSunfrog = new DataTable();

            dtShirtsTS.Columns.Add("Front", typeof(string));
            dtShirtsTS.Columns.Add("Back", typeof(string));
            dtShirtsTS.Columns.Add("Front Horizontal Align", typeof(string));
            dtShirtsTS.Columns.Add("Front Vertical Align", typeof(string));
            dtShirtsTS.Columns.Add("Back Horizontal Align", typeof(string));
            dtShirtsTS.Columns.Add("Back Vertical Align", typeof(string));
            dtShirtsTS.Columns.Add("Title", typeof(string));
            dtShirtsTS.Columns.Add("Description", typeof(string));
            dtShirtsTS.Columns.Add("Sunfrog Category", typeof(string));
            dtShirtsTS.Columns.Add("Keywords", typeof(string));
            dtShirtsTS.Columns.Add("Duration", typeof(int));
            dtShirtsTS.Columns.Add("Url", typeof(string));
            dtShirtsTS.Columns.Add("Private", typeof(string));
            dtShirtsTS.Columns.Add("Auto Restart", typeof(string));
            dtShirtsTS.Columns.Add("Goal", typeof(int));

            dtShirtsSunfrog.Columns.Add("Front", typeof(string));
            dtShirtsSunfrog.Columns.Add("Back", typeof(string));
            dtShirtsSunfrog.Columns.Add("Front Horizontal Align", typeof(string));
            dtShirtsSunfrog.Columns.Add("Front Vertical Align", typeof(string));
            dtShirtsSunfrog.Columns.Add("Back Horizontal Align", typeof(string));
            dtShirtsSunfrog.Columns.Add("Back Vertical Align", typeof(string));
            dtShirtsSunfrog.Columns.Add("Title", typeof(string));
            dtShirtsSunfrog.Columns.Add("Description", typeof(string));
            dtShirtsSunfrog.Columns.Add("Sunfrog Category", typeof(string));
            dtShirtsSunfrog.Columns.Add("Keywords", typeof(string));
            dtShirtsSunfrog.Columns.Add("Duration", typeof(int));
            dtShirtsSunfrog.Columns.Add("Url", typeof(string));
            dtShirtsSunfrog.Columns.Add("Private", typeof(string));
            dtShirtsSunfrog.Columns.Add("Auto Restart", typeof(string));
            dtShirtsSunfrog.Columns.Add("Goal", typeof(int));

            Excel.Application xlApp;
            Excel.Workbook xlWorkBook;
            Excel.Worksheet xlWorkSheet;
            Excel.Range range;

            string str, strSunfrog;
            int rCnt;
            int cCnt;
            int rw = 0;
            int cl = 0;

            xlApp = new Excel.Application();
            xlWorkBook = xlApp.Workbooks.Open(folderpath + @"\Import.xlsx", 0, true, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);
            xlWorkSheet = (Excel.Worksheet)xlWorkBook.Worksheets.get_Item(1);

            range = xlWorkSheet.UsedRange;
            rw = range.Rows.Count;
            cl = range.Columns.Count;

            double step = (double)(100.0f / rw);
            
            for (rCnt = 2; rCnt <= rw; rCnt++)
            {
                List<string> arrValues = new List<string>();
                DataRow dataRow = dtShirtsTS.NewRow();
                DataRow dataRowSunFrog = dtShirtsSunfrog.NewRow();
                int col = 0;
                for (cCnt = 1; cCnt <= cl; cCnt++)
                {
                    if (range.Cells[rCnt, cCnt] != null && range.Cells[rCnt, cCnt].Value != null)
                    {
                        str = (range.Cells[rCnt, cCnt] as Excel.Range).Value2.ToString();
                        strSunfrog = str;
                        if (cCnt == 1)//path
                        {
                            if (!File.Exists(Path.GetDirectoryName(str) + @"\" + sunfrogPath + @"\" + Path.GetFileName(str)))
                            {
                                #region SunFrog dimension WxH = 2400 x 3200
                                IImageInfo imgBaseSunFrog = WebManager.GetImageInfo(File.ReadAllBytes(str));
                                imgBaseSunFrog.FileName = Path.GetFileName(str);
                                imgBaseSunFrog.ContentType = Path.GetExtension(str);

                                imgBaseSunFrog.Path = sunfrogPath;
                                IImageInfo sunFrogImage = imgBaseSunFrog.ResizeMe(3200, 2400);
                                string newPath = Path.GetDirectoryName(str) + @"\" + imgBaseSunFrog.Path;
                                if (!Directory.Exists(newPath))
                                    Directory.CreateDirectory(newPath);
                                sunFrogImage.Save(newPath);
                                #endregion
                            }

                            str = Path.GetDirectoryName(str) + @"\" + sunfrogPath + @"\" + Path.GetFileName(str);
                            strSunfrog = str;
                        }
                        else if (cCnt == 8)//desc culumn
                            strSunfrog = dataRowSunFrog[6].ToString();
                        else if(cCnt == 10)//col keyword, sunfrog only allow 3 keyword
                        {
                            string[] arrKeywords = str.Split(',');
                            if(arrKeywords.Length > 3)
                            {
                                string newKey = "";
                                for (int i = 0; i < 3; i++)
                                {
                                    newKey += arrKeywords[i];
                                    if (i != 2) newKey += ",";
                                }
                                strSunfrog = newKey;
                            }

                        }
                        arrValues.Add(str);
                        dataRow[col] = str;
                        dataRowSunFrog[col] = strSunfrog;
                        col++;
                    }
                    else
                    {
                        dataRow[col] = "";
                        dataRowSunFrog[col] = "";
                        col++;
                        arrValues.Add("");
                    }
                }
                dtShirtsTS.Rows.Add(dataRow);
                dtShirtsSunfrog.Rows.Add(dataRowSunFrog);

                //update UI
                this.BeginInvoke((Action)delegate ()
                {
                    double newStep = rCnt * step;
                    progressConvert.Value = (int)newStep;
                });
            }

            ExcelUtlity excelUtil = new ExcelUtlity();
            excelUtil.WriteDataTableToExcel(dtShirtsSunfrog, "Sheet1", folderpath + @"\ImportSunfrog.xlsx");
            excelUtil.WriteDataTableToExcel(dtShirtsTS, "Sheet1", folderpath + @"\ImportOther.xlsx");
            xlWorkBook.Close(true, null, null);
            xlApp.Quit();

            Marshal.ReleaseComObject(xlWorkSheet);
            Marshal.ReleaseComObject(xlWorkBook);
            Marshal.ReleaseComObject(xlApp);


            //update UI
            this.BeginInvoke((Action)delegate ()
            {
                MessageBox.Show("Finished ! Check new excel path : " + folderpath + @"\ImportSunfrog.xlsx", "Notice", MessageBoxButtons.OK);
            });
        }
        private void button2_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1 = new FolderBrowserDialog();
            folderBrowserDialog1.ShowDialog();
            folderpath = folderBrowserDialog1.SelectedPath;
            progressConvert.Value = 0;

            Thread t = new Thread(new ThreadStart(DoExport));
            t.Start();
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