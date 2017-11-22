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
using ImageManager;

namespace SpreadShirt
{
    public partial class Form1 : Form, IMainFormDelegate
    {
        FrmProgress frmProgress;
        static string Title = "", SubTitle = "", Tag = "", Headline = "", RedBubleTag = "";
        static string NextPageURL = "";
        static public List<string> listRedbubleTag = new List<string>();
        static string baseURL = "https://www.spreadshirt.com";
        DataTable dtShirts = new DataTable();
        List<string> listShirts = new List<string>();
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

            checkSpyBrand_CheckedChanged(null, null);
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
            if(((AngleSharpParser)parsers[0]).CurrentParser.Title != "")
                Title = ((AngleSharpParser)parsers[0]).CurrentParser.Title;
            if (((AngleSharpParser)parsers[0]).CurrentParser.SubTitle != "")
                SubTitle = ((AngleSharpParser)parsers[0]).CurrentParser.SubTitle;
            if (((AngleSharpParser)parsers[0]).CurrentParser.Tag != "")
                Tag = ((AngleSharpParser)parsers[0]).CurrentParser.Tag;
            if (((AngleSharpParser)parsers[0]).CurrentParser.Headline != "")
                Headline = ((AngleSharpParser)parsers[0]).CurrentParser.Headline;
            if (type == RequestType.HTML)
            {
                listRedbubleTag = ((AngleSharpParser)parsers[0]).CurrentParser.listRedbubleTag;
                NextPageURL = ((AngleSharpParser)parsers[0]).CurrentParser.NextPageURL;
            }
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
            if(checkSpyBrand.Checked && txtBrandUrl.Text.TrimEnd() == "")
            {
                MessageBox.Show("Enter the content to query", "Notice", MessageBoxButtons.OK);
                return;
            }
            else if (!checkSpyBrand.Checked && txtQuery.Text.TrimEnd() == "")
            {
                MessageBox.Show("Enter the content to query", "Notice", MessageBoxButtons.OK);
                return;
            }

            doCancel = false;
            listResult.DataSource = null;
            txtLog.Text = "";
            txtLog.AppendText(String.Format("+++ Do search : {0} +++\r\n", txtQuery.Text));
            listShirts = new List<string>();
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
            frmProgress.SetOwnerDelegate(this);
            frmProgress.ShowDialog();
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

        public static bool CheckURLValid(string url)
        {
            Uri uriResult;
            return Uri.TryCreate(url, UriKind.Absolute, out uriResult) && uriResult.Scheme == Uri.UriSchemeHttp;
        }
        private void AddShirt(string filename)
        {
            for(int i = 0; i < listRedbubleTag.Count; i ++)
            {
                if (i == 0) Tag += ", ";
                Tag += listRedbubleTag[i];
                if (i < listRedbubleTag.Count - 1)
                    Tag += ", ";
            }
            listShirts.Add(filename);
            string desc = SubTitle + "\r\n LIMITED EDITION ! Ending soon !\r\n\r\n100 % Printed in the U.S.A - Ship Worldwide \r\n\r\n* HOW TO ORDER?\r\n1.Select style and color\r\n2.Click Buy it Now\r\n3.Select size and quantity\r\n4.Enter shipping and billing information\r\n5.Done!Simple as that!\r\n\r\nTIP: SHARE it with your friends, order together and save on shipping.\r\n" + Tag ;
            //file name depend on platform, default with 600x600
            if(checkAllPlatform.Checked)
            {
                string folder = Path.GetDirectoryName(filename);
                folder += @"\sunFrog";
                filename = folder + @"\" + Path.GetFileName(filename);
            }
            dtShirts.Rows.Add(filename, "", "Center", "Middle", "Center", "Top", Title, desc, "Funny", Tag, 3, Path.GetFileNameWithoutExtension(filename), "FALSE", "TRUE", 1);
        }

        private void ExportToExcel(string filePath)
        {
            ExcelUtlity excelUtil = new ExcelUtlity();
            bool ret = excelUtil.WriteDataTableToExcel(dtShirts, "Sheet1", filePath);

            if(checkAllPlatform.Checked)
            {
                //scale image 
                for (int i = 0; i < listShirts.Count; i++)
                {
                    string path = listShirts[i];
                    string newPath = "";
                    #region Merch dimension WxH = 4500 x 5400
                    IImageInfo imgBaseMerch = WebManager.GetImageInfo(File.ReadAllBytes(path));
                    imgBaseMerch.FileName = Path.GetFileName(path);
                    imgBaseMerch.ContentType = Path.GetExtension(path);

                    imgBaseMerch.Path = "merch";
                    IImageInfo imgMerch = imgBaseMerch.ResizeMe(5400, 4500);
                    newPath = Path.GetDirectoryName(path) + @"\" + imgBaseMerch.Path;
                    if (!Directory.Exists(newPath))
                        Directory.CreateDirectory(newPath);
                    imgMerch.Save(newPath);
                    #endregion

                    #region SunFrog dimension WxH = 2400 x 3200
                    IImageInfo imgBaseSunFrog = WebManager.GetImageInfo(File.ReadAllBytes(path));
                    imgBaseSunFrog.FileName = Path.GetFileName(path);
                    imgBaseSunFrog.ContentType = Path.GetExtension(path);

                    imgBaseSunFrog.Path = "sunFrog";
                    IImageInfo sunFrogImage = imgBaseSunFrog.ResizeMe(3200, 2400);
                    newPath = Path.GetDirectoryName(path) + @"\" + imgBaseSunFrog.Path;
                    if (!Directory.Exists(newPath))
                        Directory.CreateDirectory(newPath);
                    sunFrogImage.Save(newPath);

                    //File.Copy(newPath + @"\" + imgBaseSunFrog.FileName, path, true);
                    #endregion
                }
            }
            //update UI
            this.BeginInvoke((Action)delegate ()
            {
                //code to update UI
                txtLog.AppendText(String.Format(" **** Finish export to excel, path : {0}  ****\r\n", filePath));
            });
        }

        private void checkSpyBrand_CheckedChanged(object sender, EventArgs e)
        {
            bool isSpyBrand = checkSpyBrand.Checked;
            txtQuery.Enabled = !isSpyBrand;
            txtRedBubbleURL.Enabled = !isSpyBrand;

            txtBrandUrl.Enabled = isSpyBrand;
            maxPage.Value = 1;

            if (isSpyBrand)
                txtBrandUrl.Focus();
            else
                txtQuery.Focus();
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
                string queryURL = "";
                List<string> listHref = new List<string>();
                if (!checkSpyBrand.Checked)
                {
                    string strQuery = @"/" + txtQuery.Text.TrimEnd().Replace(" ", "+") + "+gifts";
                    string realheadline = txtQuery.Text.TrimEnd();
                    realheadline = Regex.Replace(realheadline.TrimEnd(), @"(\s+|@|&|'|\(|\)|<|>|#)", "");
                    queryURL = baseURL + strQuery;
                    listHref = Request(queryURL, RequestType.HTML);//request page 1 first
                    Headline = Regex.Replace(Headline.TrimEnd(), @"(\s+|@|&|'|\(|\)|<|>|#)", "");
                    if (!Headline.ToLower().Substring(0, 5).Contains(realheadline.ToLower().Substring(0, 5)))//-> different headline -> not found any items
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
                }
                else
                {
                    queryURL = txtBrandUrl.Text;
                    listHref = Request(queryURL, RequestType.HTML);//request page 1 first
                    parentFolderName = Path.GetFileNameWithoutExtension(txtBrandUrl.Text);
                }

                parentFolderName = Regex.Replace(parentFolderName.TrimEnd(), @"(\s+|@|&|'|\(|\)|<|>|#)", "");

                //update UI
                this.BeginInvoke((Action)delegate ()
                {
                    //code to update UI
                    frmProgress.UpdateProgressPercent(20);
                    txtLog.AppendText(String.Format(" **** Finish search, there are : {0} results ****\r\n", listHref.Count));
                    listResult.DataSource = listHref;
                });
                #endregion
                if(txtRedBubbleURL.Text != "" && txtRedBubbleURL.Text.Contains("https://www.redbubble.com"))
                {
                    //for getting redbuble tag
                    Request(txtRedBubbleURL.Text, RequestType.HTML);
                }
                GetImageList(listHref, parentFolderName);
                //end of request page 1
                int defaultRequestPageNum = (int)maxPage.Value;
                int currentPage = 1;
                while(currentPage < defaultRequestPageNum)
                {
                    if (NextPageURL == "") break;
                    currentPage++;
                    SearchNextPage(currentPage);
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
                    frmProgress.Close();
                    txtQuery.Clear();
                });
            }
            catch (Exception ex)
            {
                this.BeginInvoke((Action)delegate ()
                {
                    txtLog.AppendText(ex.ToString() + "\r\n");
                });
            }
        } 
        
        private void GetImageList(List<string> listHref, string parentFolderName)
        {
            #region Request each shirt in page
            for (int i = 0; i < listHref.Count; i++)
            {
                if (doCancel) break;
                string url = baseURL + listHref[i];
                if (!url.Contains("shirt"))
                    continue;
                List<string> listImageURL = Request(url, RequestType.PNG);
                //Title, Subtitle, Tag already updated

                if (listImageURL.Count > 0)
                {
                    int progressStep = 80 / listHref.Count;
                    string downloadURL = baseURL + listImageURL[0];
                    string fileName = Path.GetFileName(downloadURL);
                    int count = 1;
                    while (CheckExistFileName(fileName))
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
                    if (indexCut > -1)
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
                        if (!Directory.Exists(folderDir))
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

        }
        private void SearchNextPage(int page)
        {
            if(NextPageURL != "")
            {
                this.BeginInvoke((Action)delegate ()
                {
                    txtLog.AppendText("Searching page : " + page);
                    frmProgress.UpdatePage(page);
                });
                string parentFolderName = Regex.Replace(txtQuery.Text.TrimEnd(), @"(\s+|@|&|'|\(|\)|<|>|#)", "");
                List<string> listHref = Request(NextPageURL, RequestType.HTML);//request next page
                                                                               //update UI
                this.BeginInvoke((Action)delegate ()
                {
                    //code to update UI
                    frmProgress.UpdateProgressPercent(20);
                    txtLog.AppendText(String.Format(" **** Finish search, there are : {0} results ****\r\n", listHref.Count));
                    listResult.DataSource = listHref;
                });

                GetImageList(listHref, parentFolderName);
            }
            else
            {
                this.BeginInvoke((Action)delegate ()
                {
                    txtLog.AppendText("Last page !!!" );
                });
            }
        }

        public void DoCancel()
        {
            doCancel = true;
        }
    }
}
