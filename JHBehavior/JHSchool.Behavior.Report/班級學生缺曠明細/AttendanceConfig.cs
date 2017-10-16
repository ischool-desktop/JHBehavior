using System;
using System.Windows.Forms;
using System.Xml;
using Framework;
using FISCA.Presentation.Controls;

namespace JHSchool.Behavior.Report
{
    public partial class AttendanceConfig : BaseForm
    {
        private string _reportName = "";
        public AttendanceConfig(string reportname, int sizeIndex,string reportstyle)
        {
            InitializeComponent();

            _reportName = reportname;
            comboBoxEx1.SelectedIndex = sizeIndex;

            // 2017/10/13 �����s�W�˪O��������A�]���ꤤ���y���n�D�^��˪O
            //---comboBoX ��l��---
            comboBoxEx2.Text = reportstyle;
            comboBoxEx2.Items.Add("���媩");
            comboBoxEx2.Items.Add("�^�媩");
            
            

        }

        private void buttonX1_Click(object sender, EventArgs e)
        {
            #region �x�s Preference

            //XmlElement config = CurrentUser.Instance.Preference[_reportName];
            ConfigData cd = User.Configuration[_reportName];
            XmlElement config = cd.GetXml("XmlData", null);

            if (config == null)
                config = new XmlDocument().CreateElement(_reportName);

            XmlElement print = config.OwnerDocument.CreateElement("Print");
            print.SetAttribute("PaperSize", comboBoxEx1.SelectedIndex.ToString());
            print.SetAttribute("ReportStyle",comboBoxEx2.Text);

            if (config.SelectSingleNode("Print") == null)
                config.AppendChild(print);
            else
                config.ReplaceChild(print, config.SelectSingleNode("Print"));

            //CurrentUser.Instance.Preference[_reportName] = config;

            cd.SetXml("XmlData", config);
            cd.Save();

            #endregion

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void buttonX2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}