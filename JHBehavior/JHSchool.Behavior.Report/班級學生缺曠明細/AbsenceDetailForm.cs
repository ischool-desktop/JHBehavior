using System;
using System.Windows.Forms;
using System.Xml;
using Framework;

namespace JHSchool.Behavior.Report
{
    public partial class AbsenceDetailForm : SelectDateRangeForm
    {
        private int _sizeIndex = 0;
        private string _reportstyle;
        public int PaperSize
        {
            get { return _sizeIndex; }
        }
        
        public string ReportStyle
        {
            get { return _reportstyle; }
        }

        public AbsenceDetailForm()
        {
            InitializeComponent();
            LoadPreference();
        }

        private void LoadPreference()
        {
            #region Ū�� Preference

            //XmlElement config = CurrentUser.Instance.Preference["�Z�ů��m���Ӫ�"];
            ConfigData cd = User.Configuration["�Z�ů��m�O������"];
            XmlElement config = cd.GetXml("XmlData", null);

            if (config != null)
            {
                XmlElement print = (XmlElement)config.SelectSingleNode("Print");

                if (print != null)
                {
                    if (print.HasAttribute("PaperSize"))
                        _sizeIndex = int.Parse(print.GetAttribute("PaperSize"));
                    if (print.HasAttribute("ReportStyle"))
                        _reportstyle = print.GetAttribute("ReportStyle");
                }
                else
                {
                    XmlElement newPrint = config.OwnerDocument.CreateElement("Print");
                    newPrint.SetAttribute("PaperSize", "0");
                    config.AppendChild(newPrint);
                    //CurrentUser.Instance.Preference["�Z�ů��m�O������"] = config;
                    cd.SetXml("XmlData", config);
                }
            }
            else
            {
                #region ���ͪťճ]�w��
                config = new XmlDocument().CreateElement("�Z�ů��m�O������");
                XmlElement printSetup = config.OwnerDocument.CreateElement("Print");
                printSetup.SetAttribute("PaperSize", "0");
                config.AppendChild(printSetup);
                //CurrentUser.Instance.Preference["�Z�ů��m�O������"] = config;
                cd.SetXml("XmlData", config);
                #endregion
            }

            cd.Save();

            #endregion
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            //-----
            AttendanceConfig config = new AttendanceConfig("�Z�ů��m�O������", _sizeIndex, _reportstyle);
            if (config.ShowDialog() == DialogResult.OK)
            {
                LoadPreference();
            }
        }

        private void buttonX2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}