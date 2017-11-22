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
    public interface IMainFormDelegate
    {
        void DoCancel();
    }
    public partial class FrmProgress : Form
    {
        public bool isCancel = false;
        private IMainFormDelegate ownerDelegate = null;
        public FrmProgress()
        {
            isCancel = false;
            InitializeComponent();
            btnCancel.Enabled = true;
        }

        public void SetOwnerDelegate(IMainFormDelegate delegator)
        {
            ownerDelegate = delegator;
        }
        public void UpdateProgressDesc(string desc)
        {
            if (isCancel) return;
            lbDesc.Text = desc;
        }

        public void UpdateProgressPercent(int percent)
        {
            if (isCancel) return;
            progressHTTP.Value = percent;
            lbPercent.Text = percent.ToString() + "%";
        }

        public int GetCurrentProgress()
        {
            return progressHTTP.Value;
        }

        public void UpdatePage(int page)
        {
            if (isCancel) return;
            lbPage.Text = "Querying page " + page.ToString();
        }
        private void btnCancel_Click(object sender, EventArgs e)
        {
            UpdateProgressPercent(100);
            UpdateProgressDesc("Exporting excel file");
            isCancel = true;
            btnCancel.Enabled = false;
            //Close();
            if (ownerDelegate != null)
                ownerDelegate.DoCancel();
        }
    }
}
