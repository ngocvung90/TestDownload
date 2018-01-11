using AngleSharp.Performance;
using AngleSharp.Performance.Html;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Windows.Forms;
using Excel;
using System.Text.RegularExpressions;
using ImageManager;

namespace SpreadShirtMutiKey
{
    public partial class Form1 : Form, IMainFormDelegate
    {
        FrmProgress frmProgress;
        static string Title = "", SubTitle = "", Tag = "", Headline = "", RedBubleTag = "";
        static string NextPageURL = "";
        static public List<string> listRedbubleTag = new List<string>();
        static string baseURL = "https://www.spreadshirt.com";
        DataTable dtShirtsTS = new DataTable();
        DataTable dtShirtsViral = new DataTable();
        DataTable dtShirtsSunfrog = new DataTable();
        DataTable dtShirtsTeezily = new DataTable();

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
            if (txtQuery.Text.TrimEnd() == "")
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
            #region Add column
            dtShirtsTS = new DataTable();
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

            dtShirtsViral = new DataTable();
            dtShirtsViral.Columns.Add("Front", typeof(string));
            dtShirtsViral.Columns.Add("Back", typeof(string));
            dtShirtsViral.Columns.Add("Front Horizontal Align", typeof(string));
            dtShirtsViral.Columns.Add("Front Vertical Align", typeof(string));
            dtShirtsViral.Columns.Add("Back Horizontal Align", typeof(string));
            dtShirtsViral.Columns.Add("Back Vertical Align", typeof(string));
            dtShirtsViral.Columns.Add("Title", typeof(string));
            dtShirtsViral.Columns.Add("Description", typeof(string));
            dtShirtsViral.Columns.Add("Sunfrog Category", typeof(string));
            dtShirtsViral.Columns.Add("Keywords", typeof(string));
            dtShirtsViral.Columns.Add("Duration", typeof(int));
            dtShirtsViral.Columns.Add("Url", typeof(string));
            dtShirtsViral.Columns.Add("Private", typeof(string));
            dtShirtsViral.Columns.Add("Auto Restart", typeof(string));
            dtShirtsViral.Columns.Add("Goal", typeof(int));

            dtShirtsSunfrog = new DataTable();
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

            dtShirtsTeezily = new DataTable();
            dtShirtsTeezily.Columns.Add("Front", typeof(string));
            dtShirtsTeezily.Columns.Add("Back", typeof(string));
            dtShirtsTeezily.Columns.Add("Front Horizontal Align", typeof(string));
            dtShirtsTeezily.Columns.Add("Front Vertical Align", typeof(string));
            dtShirtsTeezily.Columns.Add("Back Horizontal Align", typeof(string));
            dtShirtsTeezily.Columns.Add("Back Vertical Align", typeof(string));
            dtShirtsTeezily.Columns.Add("Title", typeof(string));
            dtShirtsTeezily.Columns.Add("Description", typeof(string));
            dtShirtsTeezily.Columns.Add("Sunfrog Category", typeof(string));
            dtShirtsTeezily.Columns.Add("Keywords", typeof(string));
            dtShirtsTeezily.Columns.Add("Duration", typeof(int));
            dtShirtsTeezily.Columns.Add("Url", typeof(string));
            dtShirtsTeezily.Columns.Add("Private", typeof(string));
            dtShirtsTeezily.Columns.Add("Auto Restart", typeof(string));
            dtShirtsTeezily.Columns.Add("Goal", typeof(int));

            #endregion

            Thread t = new Thread(new ThreadStart(DoSearch));
            t.Start();

            frmProgress = new FrmProgress();
            frmProgress.UpdateProgressDesc(String.Format("Searching ..."));
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
                if (i == 0) Tag += ",";
                Tag += listRedbubleTag[i];
                if (i < listRedbubleTag.Count - 1)
                    Tag += ",";
            }
            Tag = Tag.Replace("Tags:", "");
            Tag = Tag.Replace(" ", "");
            listShirts.Add(filename);
            string desc = /*SubTitle + */"LIMITED EDITION ! Ending soon !\r\n\r\n100 % Printed in the U.S.A - Ship Worldwide \r\n\r\n* HOW TO ORDER?\r\n1.Select style and color\r\n2.Click Buy it Now\r\n3.Select size and quantity\r\n4.Enter shipping and billing information\r\n5.Done!Simple as that!\r\n\r\nTIP: SHARE it with your friends, order together and save on shipping.\r\n" /*+ Tag */;
            //file name depend on platform, default with 600x600
            string folder = Path.GetDirectoryName(filename);
            folder += @"\sunFrog";
            string sun_viralFilename = folder + @"\" + Path.GetFileName(filename);

            //build sunfrog tag
            string[] arrKeywords = Tag.Split(',');
            string strSunfrogTag = "";
            if (arrKeywords.Length > 3)
            {
                string newKey = "";
                for (int i = 0; i < 3; i++)
                {
                    newKey += arrKeywords[i];
                    if (i != 2) newKey += ",";
                }
                strSunfrogTag = newKey;
            }

            dtShirtsSunfrog.Rows.Add(sun_viralFilename, "", "Center", "Middle", "Center", "Top", Title, Title, "Funny", strSunfrogTag, 3, Path.GetFileNameWithoutExtension(filename), "FALSE", "TRUE", 1);
            dtShirtsViral.Rows.Add(sun_viralFilename, "", "Center", "Middle", "Center", "Top", Title, desc, "Funny", Tag, 4, Path.GetFileNameWithoutExtension(filename), "FALSE", "TRUE", 10);
            dtShirtsTS.Rows.Add(sun_viralFilename, "", "Center", "Middle", "Center", "Top", Title, desc, "Funny", Tag, 3, Path.GetFileNameWithoutExtension(filename), "FALSE", "TRUE", 1);
            dtShirtsTeezily.Rows.Add(filename, "", "Center", "Middle", "Center", "Top", Title, desc, "Funny", Tag, 3, Path.GetFileNameWithoutExtension(filename), "FALSE", "TRUE", 1);
        }

        private void ExportToExcel(string filePath, string excelName)
        {
            ExcelUtlity excelUtil = new ExcelUtlity();
            //scale image 
            for (int i = 0; i < listShirts.Count; i++)
            {
                string path = listShirts[i];

                if (!File.Exists(path)) continue;
                string newPath = "";
                #region Merch dimension WxH = 4500 x 5400
                //IImageInfo imgBaseMerch = WebManager.GetImageInfo(File.ReadAllBytes(path));
                //imgBaseMerch.FileName = Path.GetFileName(path);
                //imgBaseMerch.ContentType = Path.GetExtension(path);

                //imgBaseMerch.Path = "merch";
                //IImageInfo imgMerch = imgBaseMerch.ResizeMe(5400, 4500);
                //newPath = Path.GetDirectoryName(path) + @"\" + imgBaseMerch.Path;
                //if (!Directory.Exists(newPath))
                //    Directory.CreateDirectory(newPath);
                //imgMerch.Save(newPath);
                #endregion

                #region SunFrog dimension WxH = 2400 x 3200
                IImageInfo imgBaseSunFrog = WebManager.GetImageInfo(File.ReadAllBytes(path));
                imgBaseSunFrog.FileName = Path.GetFileName(path);
                imgBaseSunFrog.ContentType = Path.GetExtension(path);

                imgBaseSunFrog.Path = Path.GetDirectoryName(path);
                IImageInfo sunFrogImage = imgBaseSunFrog.ResizeMe(3200, 2400);
                newPath = Path.GetDirectoryName(path) + @"\" + "sunFrog";
                if (!Directory.Exists(newPath))
                    Directory.CreateDirectory(newPath);
                sunFrogImage.Save(newPath);
                #endregion
            }

            string folder = Path.GetDirectoryName(filePath);

            bool ret = excelUtil.WriteDataTableToExcel(dtShirtsTS, "Sheet1", filePath);
            excelUtil.WriteDataTableToExcel(dtShirtsSunfrog, "Sheet1", folder + @"\" + String.Format("Import_Sunfrog_{0}.xlsx", excelName));// Import_SunFrog.xlsx
            excelUtil.WriteDataTableToExcel(dtShirtsViral, "Sheet1", folder + @"\" + String.Format("Import_Viral_{0}.xlsx", excelName));
            excelUtil.WriteDataTableToExcel(dtShirtsTeezily, "Sheet1", folder + @"\" + String.Format("Import_Teezily_{0}.xlsx", excelName));
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
                string[] arrQuery = Regex.Split(txtQuery.Text, "\r\n");
                string strExcelName = "";
                for(int i = 0; i < arrQuery.Length; i ++)
                {
                    #region First time request
                    string parentFolderName = Regex.Replace(arrQuery[i].TrimEnd(), @"(\s+|@|&|'|\(|\)|<|>|#)", "");
                    string queryURL = "";
                    List<string> listHref = new List<string>();
                    string strQuery = @"/" + arrQuery[i].TrimEnd().Replace(" ", "+") + "+gifts";
                    string realheadline = arrQuery[i].TrimEnd();
                    realheadline = Regex.Replace(realheadline.TrimEnd(), @"(\s+|@|&|'|\(|\)|<|>|#)", "");
                    queryURL = baseURL + strQuery;
                    listHref = Request(queryURL, RequestType.HTML);//request page 1 first
                    Headline = Regex.Replace(Headline.TrimEnd(), @"(\s+|@|&|'|\(|\)|<|>|#)", "");
                    if (!Headline.ToLower().Substring(0, 5).Contains(realheadline.ToLower().Substring(0, 5)))//-> different headline -> not found any items
                    {
                        this.BeginInvoke((Action)delegate ()
                        {
                            //code to update UI
                            txtLog.AppendText(String.Format("Not found any items for keyword :{0}, headline : {1}", arrQuery[i], Headline));
                        });
                        continue;
                    }

                    if(strExcelName.Length + arrQuery[i].Length < 200)//max filename
                    {
                        strExcelName += arrQuery[i];
                        if (i < arrQuery.Length - 1)
                            strExcelName += "_";
                    }

                    this.BeginInvoke((Action)delegate ()
                    {
                        //code to update UI
                        frmProgress.UpdateProgressPercent(20);
                        txtLog.AppendText(String.Format(" **** Finish search {0}, there are : {1} results ****\r\n", arrQuery[i], listHref.Count));
                        listResult.DataSource = listHref;
                    });
                    #endregion
                    GetImageList(listHref, parentFolderName);
                }
                    
                #region Export all data to excel
                string excelFolderPath = "";
                if (txtSaveLocation.Text != "")
                    excelFolderPath = txtSaveLocation.Text + @"\" + "1.ExcelOutput" + @"\" + strExcelName ;
                else
                    excelFolderPath = Application.StartupPath + @"\" + "1.ExcelOutput" + @"\" + strExcelName;
                if (!Directory.Exists(excelFolderPath))
                    Directory.CreateDirectory(excelFolderPath);

                string tsExcelName = String.Format("Import_{0}.xlsx", strExcelName);
                ExportToExcel(excelFolderPath + @"\" + tsExcelName, strExcelName);
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
        public void DoCancel()
        {
            doCancel = true;
        }
    }
}
