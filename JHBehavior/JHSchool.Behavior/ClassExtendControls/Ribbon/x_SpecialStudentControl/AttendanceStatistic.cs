using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using FISCA.DSAUtil;
using DevComponents.DotNetBar;
using System.Xml;
using Aspose.Cells;
using System.IO;
using System.Diagnostics;
using Framework;
using Framework.Feature;
using Framework.Legacy;
using JHSchool.Behavior.Feature;
using JHSchool.Behavior.StuAdminExtendControls;
//using SmartSchool.Common;

namespace JHSchool.Behavior.ClassExtendControls.Ribbon
{
    public partial class AttendanceStatistic : UserControl, IDeXingExport
    {
        private List<string> _classidList;
        public AttendanceStatistic(List<string> classidList)
        {
            InitializeComponent();
            _classidList = classidList;
        }

        #region IDeXingExport ����

        public void LoadData()
        {
            DSResponse dsrsp = Framework.Feature.Config.GetAbsenceList();   //���o���O��Ӫ�
            DSXmlHelper helper = dsrsp.GetContent();
            foreach (XmlElement e in helper.GetElements("Absence"))
            {
                string name = e.GetAttribute("Name");
                int rowIndex = dataGridView.Rows.Add();
                DataGridViewRow row = dataGridView.Rows[rowIndex];
                row.Cells[1].Value = name;
            }

            #region �Ǧ~�׾Ǵ�
            comboBoxEx1.Text = School.DefaultSchoolYear;
            comboBoxEx1.Items.Add(int.Parse(School.DefaultSchoolYear) - 2);
            comboBoxEx1.Items.Add(int.Parse(School.DefaultSchoolYear) - 1);
            comboBoxEx1.Items.Add(int.Parse(School.DefaultSchoolYear));
            comboBoxEx2.Text = School.DefaultSemester;
            comboBoxEx2.Items.Add(1);
            comboBoxEx2.Items.Add(2); 
            #endregion
        }


        //�H�U���e�S�m�L�O�o�˼g
        public void Export()
        {
            dataGridView.EndEdit();
            if (!IsValid())
            {
                FISCA.Presentation.Controls.MsgBox.Show("��J��Ʀ��~�A�Эץ���A�i��ץX�I", "���ҿ��~", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            DSXmlHelper helper = new DSXmlHelper("Request");
            helper.AddElement(".", "SchoolYear", comboBoxEx1.Text);
            helper.AddElement(".", "Semester", comboBoxEx2.Text);

            List<string> list = new List<string>();

            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                if (!IsCheckedRow(row)) continue;
                XmlElement e = helper.AddElement("Absence");
                e.SetAttribute("Name", row.Cells[1].Value.ToString());
                e.SetAttribute("PeriodCount", "0");

                list.Add(row.Cells[1].Value.ToString());
            }

            foreach (string id in _classidList)
            {
                helper.AddElement(".", "ClassID", id);
            }
            DSResponse dsrsp = QueryAttendance.GetAttendanceStatistic(new DSRequest(helper));
            if (!dsrsp.HasContent)
            {
                FISCA.Presentation.Controls.MsgBox.Show("���o�^�и�ƥ���:" + dsrsp.GetFault().Message, "���~", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            
            DSXmlHelper rsp = dsrsp.GetContent();

            Dictionary<string, Dictionary<string, double>> studentAttendance = new Dictionary<string, Dictionary<string, double>>();

            #region �إߥثe��Ʀr��
            foreach (XmlElement e in rsp.GetElements("Absence"))
            {
                foreach (XmlElement ee in e.SelectNodes("Student"))
                {
                    //�p�G�ǥͤ��s�b
                    if (!studentAttendance.ContainsKey(ee.GetAttribute("ID")))
                    {
                        //�s�W���ǥ�(�Ǹ�key/���Ovalue),
                        studentAttendance.Add(ee.GetAttribute("ID"), new Dictionary<string, double>());
                        //�s�W���O(���Okey/��value
                        studentAttendance[ee.GetAttribute("ID")].Add(e.GetAttribute("Type"), double.Parse(ee.GetAttribute("PeriodCount")));
                    }
                    else
                    {   //�p�G�s�b,�h�P�_���O
                        if (!studentAttendance[ee.GetAttribute("ID")].ContainsKey(e.GetAttribute("Type")))
                        {
                            //�p�G���s�b
                            studentAttendance[ee.GetAttribute("ID")].Add(e.GetAttribute("Type"), double.Parse(ee.GetAttribute("PeriodCount")));
                        }
                        else
                        {
                            //�p�G���O�s�b
                            studentAttendance[ee.GetAttribute("ID")][e.GetAttribute("Type")] += double.Parse(ee.GetAttribute("PeriodCount"));
                        }
                    }
                }

            } 
            #endregion
            
            Workbook book = new Workbook();
            book.Worksheets.Clear();

            int sheetIndex = book.Worksheets.Add();
            Worksheet sheet = book.Worksheets[sheetIndex];
            sheet.Name = "���m�֭p�W��";
            string schoolName = School.ChineseName;
            //�N��l�X��
            sheet.Cells.Merge(0, 0, 1, 5);
            sheet.Cells[0, 0].PutValue(schoolName);

            sheet.Cells[1, 0].PutValue("�Z��");
            sheet.Cells[1, 1].PutValue("�y��");
            sheet.Cells[1, 2].PutValue("�m�W");
            sheet.Cells[1, 3].PutValue("�Ǹ�");

            Dictionary<string, int> saveAttAddress1 = new Dictionary<string, int>();
            int countList = 4;
            foreach (string var in list)
            {
                saveAttAddress1.Add(var, countList);
                sheet.Cells[1, countList].PutValue(var);
                countList++;
            }
            sheet.Cells[1, countList].PutValue("�ֿn�`��");

            int cellcount = 2;
            int _MergeInt = 0;

            //���o�@�W�ǥͤ����
            foreach (string var in studentAttendance.Keys)
            {
                double xyz = 0;
                //�B�z���O�ۥ[
                foreach (string invar in studentAttendance[var].Keys)
                {
                    //���O�O�_�O�ϥΪ̩ҿ�
                    if(list.Contains(invar))
                    {
                        //�N��Ƭۥ[
                        xyz = xyz + studentAttendance[var][invar];
                    }
                }

                //�p�G�֭p�ƶq�j�󵥩�ϥΪ̩ҿ�J
                if (xyz >= float.Parse(txtPeriodCount.Text))
                {
                    if (Student.Instance.Items[var].Class != null)
                    {
                        sheet.Cells[cellcount, 0].PutValue(Student.Instance.Items[var].Class.Name);
                    }
                    sheet.Cells[cellcount, 1].PutValue(Student.Instance.Items[var].SeatNo);
                    sheet.Cells[cellcount, 2].PutValue(Student.Instance.Items[var].Name);
                    sheet.Cells[cellcount, 3].PutValue(Student.Instance.Items[var].StudentNumber);
                    sheet.Cells[cellcount, countList].PutValue(xyz);

                    foreach (string invar in studentAttendance[var].Keys)
                    {
                        sheet.Cells[cellcount, saveAttAddress1[invar]].PutValue(studentAttendance[var][invar]);
                    }

                    foreach (int injar in saveAttAddress1.Values)
                    {
                        if (sheet.Cells[cellcount, injar].StringValue == string.Empty)
                        {
                            sheet.Cells[cellcount, injar].PutValue(0);
                        }
                        _MergeInt = injar;
                    }


                    cellcount++;
                }
            }


            //foreach (XmlElement e in rsp.GetElements("Absence"))
            //{


            //    int sheetIndex = book.Worksheets.Add();
            //    Worksheet sheet = book.Worksheets[sheetIndex];
            //    sheet.Name = ConvertToValidName(e.GetAttribute("Type"));

            //    string schoolName = GlobalOld.SchoolInformation.ChineseName;
            //    Cell A1 = sheet.Cells["A1"];
            //    A1.Style.Borders.SetColor(Color.Black);
            //    A1Name = schoolName + "  " + sheet.Name + "�����֭p�ǥͲM��";
            //    A1.PutValue(A1Name);
            //    A1.Style.HorizontalAlignment = TextAlignmentType.Center;
            //    sheet.Cells.Merge(0, 0, 1, 5);

            //    FormatCell(sheet.Cells["A2"], "�Z��");
            //    FormatCell(sheet.Cells["B2"], "�y��");
            //    FormatCell(sheet.Cells["C2"], "�m�W");
            //    FormatCell(sheet.Cells["D2"], "�Ǹ�");
            //    FormatCell(sheet.Cells["E2"], "�ֿn�`��");
            //    //FormatCell(sheet.Cells["F2"], "�ֿn����");

            //    int index = 3;
            //    foreach (XmlElement s in e.SelectNodes("Student"))
            //    {
            //        FormatCell(sheet.Cells["A" + index], s.GetAttribute("ClassName"));
            //        FormatCell(sheet.Cells["B" + index], s.GetAttribute("SeatNo"));
            //        FormatCell(sheet.Cells["C" + index], s.GetAttribute("Name"));
            //        FormatCell(sheet.Cells["D" + index], s.GetAttribute("StudentNumber"));
            //        FormatCell(sheet.Cells["E" + index], s.GetAttribute("PeriodCount"));
            //        //FormatCell(sheet.Cells["F" + index], s.GetAttribute("Subtract"));
            //        index++;
            //    }
            //}

            string path = Path.Combine(Application.StartupPath, "Reports");

            //�p�G�ؿ����s�b�h�إߡC
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            path = Path.Combine(path, ConvertToValidName("���m�֭p�W��") + ".xls");
            try
            {
                book.Save(path);
            }
            catch (IOException)
            {
                try
                {
                    FileInfo file = new FileInfo(path);
                    string nameTempalte = file.FullName.Replace(file.Extension, "") + "{0}.xls";
                    int count = 1;
                    string fileName = string.Format(nameTempalte, count);
                    while (File.Exists(fileName))
                        fileName = string.Format(nameTempalte, count++);

                    book.Save(fileName);
                    path = fileName;
                }
                catch (Exception ex)
                {
                    FISCA.Presentation.Controls.MsgBox.Show("�ɮ��x�s����:" + ex.Message, "���~", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            catch (Exception ex)
            {
                FISCA.Presentation.Controls.MsgBox.Show("�ɮ��x�s����:" + ex.Message, "���~", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                Process.Start(path);
            }
            catch (Exception ex)
            {
                FISCA.Presentation.Controls.MsgBox.Show("�ɮ׶}�ҥ���:" + ex.Message, "���~", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        public Control MainControl
        {
            get { return this.groupPanel1; }
        }

        private string ConvertToValidName(string A1Name)
        {
            char[] invalids = Path.GetInvalidFileNameChars();

            string result = A1Name;
            foreach (char each in invalids)
                result = result.Replace(each, '_');

            return result;
        }

        private void FormatCell(Cell cell, string value)
        {
            cell.PutValue(value);
            cell.Style.Borders.SetStyle(CellBorderType.Hair);
            cell.Style.Borders.SetColor(Color.Black);
            cell.Style.Borders.DiagonalStyle = CellBorderType.None;
            cell.Style.HorizontalAlignment = TextAlignmentType.Center;
        }

        private bool IsCheckedRow(DataGridViewRow row)
        {
            if (row.Cells[0].Value == null)
                return false;
            string value = row.Cells[0].Value.ToString();
            bool check = false;
            if (!bool.TryParse(value, out check))
                return false;
            return check;
        }
        #endregion

        //private void dataGridView_RowValidated(object sender, DataGridViewCellEventArgs e)
        //{
            
        //}

        private bool IsValid()
        {
            errorProvider1.Clear();
            bool valid = true;
            int count = 0;

            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                if (IsCheckedRow(row))
                {
                    count++;
                }
            }

            if (count == 0)
            {
                errorProvider1.SetError(dataGridView, "�ܤ֥�����ܤ@�ӯ��m���O");                
                return false;
            }

            if (string.Empty == txtPeriodCount.Text)
            {
                errorProvider1.SetError(txtPeriodCount, "�п�J�֭p�`�����e(�Ʀr)");
                return false;
            }

            double x;
            if (!double.TryParse(txtPeriodCount.Text,out x))
            {
                //�K�ޱK�ޣ����s
                //errorProvider1.SetIconPadding(txtPeriodCount, -18);
                errorProvider1.SetError(txtPeriodCount, "�п�J���T���e(�Ʀr)");
                return false;
            }

            return valid;
        }

        //private void dataGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        //{
        //    DataGridViewRow row = dataGridView.Rows[e.RowIndex];
        //    //DataGridViewCheckBoxCell cell = row.Cells[0] as DataGridViewCheckBoxCell;
        //    //if (cell.Value == null)
        //    //    return;

        //    DataGridViewCell c = row.Cells[2];
        //    c.ErrorText = string.Empty;
        //    string value = c.Value == null ? "0" : c.Value.ToString();
        //    decimal subtract = 0;
        //    if (!decimal.TryParse(value, out subtract))
        //    {
        //        row.Cells[2].ErrorText = "�������Ʀr";
        //        return;
        //    }
        //    c.Value = subtract;
        //    row.Cells[2].ErrorText = string.Empty;
        //}

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            foreach (DataGridViewRow var in dataGridView.Rows)
            {
                var.Cells[0].Value = checkBox1.Checked;
            }
        }

        private void linkLabel1_LinkClicked_1(object sender, LinkLabelLinkClickedEventArgs e)
        {
            PeriodConfigForm config = new PeriodConfigForm();
            config.ShowDialog();
        }

    }
}
