using AngleSharp.Performance;
using AngleSharp.Performance.Html;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections;
using Excel;
using System.Text.RegularExpressions;

namespace SpreadShirt
{
    public partial class Form1 : Form
    {
        FrmProgress frmProgress;
        static string Title = "", SubTitle = "", Tag = "", Headline = "";
        static List<string> listPage = new List<string>();
        static string baseURL = "https://www.spreadshirt.com";
        DataTable dtShirts = new DataTable();
        List<string> listFileName = new List<string>();
        string saveFileLocationName = "savedLocation.txt";
        bool doCancel = false;
        public Form1()
        {
            InitializeComponent();
            txtLog.SelectionStart = txtLog.TextLength;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if(File.Exists(saveFileLocationName))
            {
                string savedLocation = File.ReadAllText(saveFileLocationName);
                txtSaveLocation.Text = savedLocation;
            }
        }

        enum RequestType
        {
            HTML = 0,
            PNG
        }
        static List<string> Request(string url, RequestType type)
        {
            var websites = new UrlTests(
                extension: ".html",
                withBuffer: true);

            websites.Include(
                url).Wait();

            var parsers = new List<ITestee>
            {
                new AngleSharpParser()
            };

            var testsuite = new TestSuite(parsers, websites.Tests, new Output(), new Warmup())
            {
                NumberOfRepeats = 1,
                NumberOfReRuns = 1
            };

            testsuite.Run(type == RequestType.HTML);
            Title = ((AngleSharpParser)parsers[0]).CurrentParser.Title;
            SubTitle = ((AngleSharpParser)parsers[0]).CurrentParser.SubTitle;
            Tag = ((AngleSharpParser)parsers[0]).CurrentParser.Tag;
            Headline = ((AngleSharpParser)parsers[0]).CurrentParser.Headline;
            if (type == RequestType.HTML)
                listPage = ((AngleSharpParser)parsers[0]).CurrentParser.listPage;
            return ((AngleSharpParser)parsers[0]).CurrentParser.listHref;
        }


        //async method to download the file
        static async void getImage(string url, string path)
        {
            try
            {
                //instance of HTTPClient
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.UserAgent.Clear();
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 6.3; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/39.0.2171.27 Safari/537.36 OPR/26.0.1656.8 (Edition beta)");
                //send  request asynchronously
                HttpResponseMessage response = await client.GetAsync(url);

                // Check that response was successful or throw exception
                response.EnsureSuccessStatusCode();

                // Read response asynchronously and save asynchronously to file
                using (FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    //copy the content from response to filestream
                    await response.Content.CopyToAsync(fileStream);
                }
            }

            catch (HttpRequestException rex)
            {
                Console.WriteLine(rex.ToString());
            }
            catch (Exception ex)
            {
                // For debugging
                Console.WriteLine(ex.ToString());
            }
        }

        private void btnQuery_Click(object sender, EventArgs e)
        {
            if (txtQuery.Text.TrimEnd() == "")
            {
                MessageBox.Show("Enter the content to query", "Notice", MessageBoxButtons.OK);
                return;
            }

            doCancel = false;
            listResult.DataSource = null;
            txtLog.Text = "";
            txtLog.AppendText(String.Format("+++ Do search : {0} +++\r\n", txtQuery.Text));
            listFileName.Clear();
            dtShirts = new DataTable();
            dtShirts.Columns.Add("Front", typeof(string));
            dtShirts.Columns.Add("Back", typeof(string));
            dtShirts.Columns.Add("Front Horizontal Align", typeof(string));
            dtShirts.Columns.Add("Front Vertical Align", typeof(string));
            dtShirts.Columns.Add("Back Horizontal Align", typeof(string));
            dtShirts.Columns.Add("Back Vertical Align", typeof(string));
            dtShirts.Columns.Add("Title", typeof(string));
            dtShirts.Columns.Add("Description", typeof(string));
            dtShirts.Columns.Add("Sunfrog Category", typeof(string));
            dtShirts.Columns.Add("Keywords", typeof(string));
            dtShirts.Columns.Add("Duration", typeof(int));
            dtShirts.Columns.Add("Url", typeof(string));
            dtShirts.Columns.Add("Private", typeof(string));
            dtShirts.Columns.Add("Auto Restart", typeof(string));
            dtShirts.Columns.Add("Goal", typeof(int));

            Thread t = new Thread(new ThreadStart(DoSearch));
            t.Start();

            frmProgress = new FrmProgress();
            frmProgress.UpdateProgressDesc(String.Format("Searching {0} ...", txtQuery.Text));
            frmProgress.UpdateProgressPercent(0);
            frmProgress.StartPosition = FormStartPosition.CenterParent;
            frmProgress.ShowDialog();

            doCancel = frmProgress.isCancel;
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            // This line calls the folder diag
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            // This is what will execute if the user selects a folder and hits OK (File if you change to FileBrowserDialog)
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string folder = dlg.SelectedPath;
                txtSaveLocation.Text = folder;
                File.WriteAllText(saveFileLocationName, folder);
            }
        }

        private void AddShirt(string filename)
        {
            string desc = "LIMITED EDITION ! Ending soon !\r\n\r\n100 % Printed in the U.S.A - Ship Worldwide \r\n\r\n* HOW TO ORDER?\r\n1.Select style and color\r\n2.Click Buy it Now\r\n3.Select size and quantity\r\n4.Enter shipping and billing information\r\n5.Done!Simple as that!\r\n\r\nTIP: SHARE it with your friends, order together and save on shipping.";
            dtShirts.Rows.Add(filename, "", "Center", "Middle", "Center", "Top", Title, desc, "Funny", Tag, 3, Path.GetFileNameWithoutExtension(filename), "FALSE", "TRUE", 1);
        }

        private void ExportToExcel(string filePath)
        {
            ExcelUtlity excelUtil = new ExcelUtlity();
            bool ret = excelUtil.WriteDataTableToExcel(dtShirts, "Sheet1", filePath);
            //update UI
            this.BeginInvoke((Action)delegate ()
            {
                //code to update UI
                txtLog.AppendText(String.Format(" **** Finish export to excel, path : {0}  ****\r\n", filePath));
            });
        }

        bool CheckExistFileName(string fileName)
        {
            int index = listFileName.FindIndex(s => s == fileName);
            return index != -1;
        }
        private void DoSearch()
        {
            try
            {
                #region First time request
                string parentFolderName = Regex.Replace(txtQuery.Text.TrimEnd(), @"(\s+|@|&|'|\(|\)|<|>|#)", "");
                string strQuery = @"/" + txtQuery.Text.TrimEnd().Replace(" ", "+") + "+gifts";
                string realheadline = txtQuery.Text.TrimEnd() + " gifts";
                realheadline = Regex.Replace(realheadline.TrimEnd(), @"(\s+|@|&|'|\(|\)|<|>|#)", "");
                List <string> listHref = Request(baseURL + strQuery, RequestType.HTML);//request page 1 first
                Headline = Regex.Replace(Headline.TrimEnd(), @"(\s+|@|&|'|\(|\)|<|>|#)", "");
                if (!realheadline.ToLower().Contains(Headline.ToLower()))//-> different headline -> not found any items
                {
                    this.BeginInvoke((Action)delegate ()
                    {
                        //code to update UI
                        txtLog.AppendText(String.Format("Not found any items, headline : {0}", Headline));
                        frmProgress.Close();
                        txtQuery.Clear();
                    });
                    return;
                }
                //update UI
                this.BeginInvoke((Action)delegate ()
                {
                    //code to update UI
                    frmProgress.UpdateProgressPercent(20);
                    txtLog.AppendText(String.Format(" **** Finish search, there are : {0} results ****\r\n", listHref.Count));
                    listResult.DataSource = listHref;
                });
                #endregion
                #region Request each shirt in page
                for (int i = 0; i < listHref.Count; i++)
                {
                    if (doCancel) break;
                    string url = baseURL + listHref[i];
                    List<string> listImageURL = Request(url, RequestType.PNG);
                    //Title, Subtitle, Tag already updated

                    if (listImageURL.Count > 0)
                    {
                        int progressStep = 80 / listHref.Count;
                        string downloadURL = baseURL + listImageURL[0];
                        string fileName = Path.GetFileName(downloadURL);
                        int count = 1;
                        while(CheckExistFileName(fileName))
                        {
                            string extension = Path.GetExtension(fileName);
                            fileName = Path.GetFileNameWithoutExtension(fileName);
                            fileName += "_" + count.ToString() + extension;
                            count++;
                        }
                        listFileName.Add(fileName);

                        string folderName = Path.GetFileNameWithoutExtension(fileName);
                        downloadURL = downloadURL.Replace("/mp", "");
                        int indexCut = downloadURL.IndexOf(",width");
                        if(indexCut > -1)
                        {
                            downloadURL = downloadURL.Substring(0, indexCut);
                            downloadURL += "?height=600&mediatype=png";
                        }
                        else
                        {
                            this.BeginInvoke((Action)delegate ()
                            {
                                txtLog.AppendText(String.Format("Error when working with url {0} ... \r\n", downloadURL));
                            });
                        }
                        //update UI
                        this.BeginInvoke((Action)delegate ()
                        {
                            //code to update UI
                            frmProgress.UpdateProgressPercent(frmProgress.GetCurrentProgress() + progressStep);
                            frmProgress.UpdateProgressDesc(String.Format("Downloading {0} ...", fileName));
                            txtLog.AppendText(String.Format("Downloading {0} ... \r\n", fileName));
                        });

                        string folderDir = "";
                        if (txtSaveLocation.Text != "")
                        {
                            folderDir = txtSaveLocation.Text + @"\" + parentFolderName + @"\" + folderName;
                            if(!Directory.Exists(folderDir))
                                Directory.CreateDirectory(folderDir);
                            fileName = folderDir + @"\" + fileName;
                        }
                        else
                        {
                            folderDir = Application.StartupPath + @"\" + parentFolderName + @"\" + folderName;
                            if (!Directory.Exists(folderDir))
                                Directory.CreateDirectory(folderDir);
                            fileName = folderDir + @"\" + fileName;
                        }
                        AddShirt(fileName);
                        string description = Title + "\r\n\r\n" + SubTitle + "\r\n\r\n" + Tag;
                        File.WriteAllText(folderDir + @"\Description.txt", description);
                        getImage(downloadURL, fileName);
                    }
                }
                #endregion
                //end of request page 1
                int defaultRequestPageNum = 1;
                int currentPage = 1;
                while(currentPage < defaultRequestPageNum)
                {
                    if (listPage.Count > currentPage)
                    {
                        string nextHref = listPage[currentPage];
                    }
                    else break;
                }
                #region Export all data to excel
                string excelFolderPath = "";
                if (txtSaveLocation.Text != "")
                    excelFolderPath = txtSaveLocation.Text + @"\" + parentFolderName ;
                else
                    excelFolderPath = Application.StartupPath + @"\" + parentFolderName;
                if (!Directory.Exists(excelFolderPath))
                    Directory.CreateDirectory(excelFolderPath);
                ExportToExcel(excelFolderPath + @"\Import.xlsx");
                #endregion
                //update UI
                this.BeginInvoke((Action)delegate ()
                {
                    //code to update UI
                    if(!doCancel)
                        frmProgress.Close();
                    txtQuery.Clear();
                });
            }
            catch (Exception ex)
            {
                txtLog.AppendText(ex.ToString() + "\r\n");
            }
        }      
    }
}
