using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpreadShirt
{
    public partial class FrmProgress : Form
    {
        public bool isCancel = false;
        public FrmProgress()
        {
            isCancel = false;
            InitializeComponent();
        }

        public void UpdateProgressDesc(string desc)
        {
            lbDesc.Text = desc;
        }

        public void UpdateProgressPercent(int percent)
        {
            progressHTTP.Value = percent;
            lbPercent.Text = percent.ToString() + "%";
        }

        public int GetCurrentProgress()
        {
            return progressHTTP.Value;
        }

        public void UpdatePage(int page)
        {
            lbPage.Text = "Querying page " + page.ToString();
        }
        private void btnCancel_Click(object sender, EventArgs e)
        {
            isCancel = true;
            Close();
        }
    }
}
