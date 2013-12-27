using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using FISCA.DSAUtil;
using DevComponents.DotNetBar;
using Aspose.Cells;
using System.Xml;
using System.IO;
using System.Diagnostics;
using Framework;
using Framework.Legacy;
using Framework.Feature;
using JHSchool.Behavior.Feature;
//using SmartSchool.Common;

namespace JHSchool.Behavior.ClassExtendControls.Ribbon
{
    public partial class DemeritStatistic : UserControl, IDeXingExport
    {
        private List<string> _classList;
        public DemeritStatistic(List<string> classList)
        {
            _classList = classList;
            InitializeComponent();
        }

        #region IDeXingExport ����
        public Control MainControl
        {
            get { return this.groupPanel1; }
        }

        //�w�]�e���˦�
        public void LoadData() //Ū���e��
        {
            cboSemester.Items.Add("1"); //�W�Ǵ�
            cboSemester.Items.Add("2"); //�U�Ǵ�
            cboSemester.SelectedIndex = 0; //�w�]����

            int schoolYear = int.Parse(School.DefaultSchoolYear); //�Ǧ~��
            for (int i = schoolYear; i > schoolYear - 4; i--) //��J�w�]�Ǧ~�� -3����
            {
                cboSchoolYear.Items.Add(i);
            }
            if (cboSchoolYear.Items.Count > 0) //��Ǧ~�פj��0,�h�w�]����
                cboSchoolYear.SelectedIndex = 0;

            rbType1.Checked = true; //??
        }


        public void Export() //�ץX���
        {
            if (!IsValid()) return; //���Ҹ��

            // ���o�����h
            DSResponse d = Framework.Feature.Config.GetMDReduce();
            /*<GetMDReduce>
            <Merit>
            <AB>3</AB> �p�\
            <BC>3</BC> �ż�
            </Merit>
            <Demerit>
            <AB>3</AB> �p�L
            <BC>3</BC> ĵ�i
            </Demerit>
            </GetMDReduce>*/

            //�p�G���o���~
            if (!d.HasContent)
            {
                FISCA.Presentation.Controls.MsgBox.Show("���o���g����W�h����:" + d.GetFault().Message, "���~", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DSXmlHelper h = d.GetContent();

            //�\Merit
            //�LDemerit
            int ab = int.Parse(h.GetText("Demerit/AB"));
            int bc = int.Parse(h.GetText("Demerit/BC"));

            //�T����쪺���e
            //�j�L
            int wa = int.Parse(txtA.Tag.ToString());
            //�p�L
            int wb = int.Parse(txtB.Tag.ToString());
            //ĵ�i
            int wc = int.Parse(txtC.Tag.ToString());

            //�p��X���
            int want = (wa * ab * bc) + (wb * bc) + wc;

            List<string> _studentIDList = new List<string>();
            Workbook book = new Workbook();
            Worksheet sheet = book.Worksheets[0];
            string schoolName = School.ChineseName;
            string A1Name = "";

            string wantString = wa + "�j�L " + wb + " �p�L " + wc + " ĵ�i";

            //�p�G�Ŀ�rbType1"��@�Ǵ��g�٪�{"
            //���rbType2�ڥ��S�Ψ�...
            if (rbType1.Checked)
            {
                #region ��{�S��ǥͲM��,Request
                DSXmlHelper helper = new DSXmlHelper("Request");
                helper.AddElement("Condition");

                if (_classList.Count == 0)
                {
                    FISCA.Presentation.Controls.MsgBox.Show("����ܯZ��");
                    return;
                }

                foreach (string classid in _classList)
                    helper.AddElement("Condition", "ClassID", classid);

                helper.AddElement("Condition", "SchoolYear", cboSchoolYear.SelectedItem.ToString());
                helper.AddElement("Condition", "Semester", cboSemester.SelectedItem.ToString());
                helper.AddElement("Order");
                helper.AddElement("Order", "GradeYear", "ASC");
                helper.AddElement("Order", "DisplayOrder", "ASC");
                helper.AddElement("Order", "ClassName", "ASC");
                helper.AddElement("Order", "SeatNo", "ASC");
                helper.AddElement("Order", "Name", "ASC"); 
                #endregion

                DSResponse dsrsp = QueryDemerit.GetDemeritStatistic(new DSRequest(helper));

                if (!dsrsp.HasContent)
                {
                    FISCA.Presentation.Controls.MsgBox.Show("�d�ߵ��G����:" + dsrsp.GetFault().Message, "���~", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                helper = dsrsp.GetContent();

                #region �]�w�Ӽ��D�a...1
                Cell A1 = sheet.Cells["A1"];
                A1.Style.Borders.SetColor(Color.Black);
                A1Name = schoolName + "  " + cboSchoolYear.SelectedItem.ToString() + "�Ǧ~�ײ�" + cboSemester.SelectedItem.ToString() + "�Ǵ� ��{�S��ǥͲM��";
                sheet.Name = A1Name;
                A1.PutValue(A1Name);
                A1.Style.HorizontalAlignment = TextAlignmentType.Center;
                sheet.Cells.Merge(0, 0, 1, 7);

                #endregion

                #region ���Title...1
                FormatCell(sheet.Cells["A2"], "�Z��");
                FormatCell(sheet.Cells["B2"], "�y��");
                FormatCell(sheet.Cells["C2"], "�m�W");
                FormatCell(sheet.Cells["D2"], "�Ǹ�");
                FormatCell(sheet.Cells["E2"], "�j�L");
                FormatCell(sheet.Cells["F2"], "�p�L");
                FormatCell(sheet.Cells["G2"], "ĵ�i"); 
                #endregion

                int index = 1;
                foreach (XmlElement e in helper.GetElements("Student"))
                {
                    string da = e.SelectSingleNode("DemeritA").InnerText;
                    string db = e.SelectSingleNode("DemeritB").InnerText;
                    string dc = e.SelectSingleNode("DemeritC").InnerText;

                    int a, b, c, total;
                    if (!int.TryParse(da, out a)) a = 0;
                    if (!int.TryParse(db, out b)) b = 0;
                    if (!int.TryParse(dc, out c)) c = 0;
                    total = (a * ab * bc) + (b * bc) + c;
                    if (total < want) continue;

                    _studentIDList.Add(e.GetAttribute("StudentID"));

                    int rowIndex = index + 2;
                    FormatCell(sheet.Cells["A" + rowIndex], e.SelectSingleNode("ClassName").InnerText);
                    FormatCell(sheet.Cells["B" + rowIndex], e.SelectSingleNode("SeatNo").InnerText);
                    FormatCell(sheet.Cells["C" + rowIndex], e.SelectSingleNode("Name").InnerText);
                    FormatCell(sheet.Cells["D" + rowIndex], e.SelectSingleNode("StudentNumber").InnerText);
                    FormatCell(sheet.Cells["E" + rowIndex], e.SelectSingleNode("DemeritA").InnerText);
                    FormatCell(sheet.Cells["F" + rowIndex], e.SelectSingleNode("DemeritB").InnerText);
                    FormatCell(sheet.Cells["G" + rowIndex], e.SelectSingleNode("DemeritC").InnerText);
                    index++;
                }

            }
            else // �Y�έp�֭p�ɪ��B�z
            {
                #region ��{�S��ǥͲM��,Request
                DSXmlHelper helper = new DSXmlHelper("Request");
                helper.AddElement("Condition");

                if (_classList.Count == 0)
                {
                    FISCA.Presentation.Controls.MsgBox.Show("����ܯZ��");
                    return;
                }

                foreach (string classid in _classList)
                    helper.AddElement("Condition", "ClassID", classid);

                helper.AddElement("Order");
                helper.AddElement("Order", "GradeYear", "ASC");
                helper.AddElement("Order", "DisplayOrder", "ASC");
                helper.AddElement("Order", "ClassName", "ASC");
                helper.AddElement("Order", "SeatNo", "ASC");
                helper.AddElement("Order", "Name", "ASC"); 
                #endregion

                DSResponse dsrsp = QueryDemerit.GetDemeritStatistic(new DSRequest(helper));

                if (!dsrsp.HasContent)
                {
                    FISCA.Presentation.Controls.MsgBox.Show("�d�ߵ��G����:" + dsrsp.GetFault().Message, "���~", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                helper = dsrsp.GetContent();

                #region �]�w�Ӽ��D�a...
                Cell A1 = sheet.Cells["A1"];
                A1.Style.Borders.SetColor(Color.Black);

                A1Name = schoolName + "  �g�ٲ֭p�M��";

                sheet.Name = A1Name;
                A1.PutValue(A1Name);
                A1.Style.HorizontalAlignment = TextAlignmentType.Center;
                sheet.Cells.Merge(0, 0, 1, 7); 
                #endregion

                #region ���Title
                FormatCell(sheet.Cells["A2"], "�Z��");
                FormatCell(sheet.Cells["B2"], "�y��");
                FormatCell(sheet.Cells["C2"], "�m�W");
                FormatCell(sheet.Cells["D2"], "�Ǹ�");
                FormatCell(sheet.Cells["E2"], "�j�L");
                FormatCell(sheet.Cells["F2"], "�p�L");
                FormatCell(sheet.Cells["G2"], "ĵ�i"); 
                #endregion

                int index = 3;
                foreach (XmlElement e in helper.GetElements("Student"))
                {
                    _studentIDList.Add(e.GetAttribute("StudentID"));
                    string da = e.SelectSingleNode("DemeritA").InnerText;
                    string db = e.SelectSingleNode("DemeritB").InnerText;
                    string dc = e.SelectSingleNode("DemeritC").InnerText;

                    int a, b, c, total;
                    if (!int.TryParse(da, out a)) a = 0;
                    if (!int.TryParse(db, out b)) b = 0;
                    if (!int.TryParse(dc, out c)) c = 0;
                    total = (a * ab * bc) + (b * bc) + c;
                    if (total < want) continue;

                    FormatCell(sheet.Cells["A" + index], e.SelectSingleNode("ClassName").InnerText);
                    FormatCell(sheet.Cells["B" + index], e.SelectSingleNode("SeatNo").InnerText);
                    FormatCell(sheet.Cells["C" + index], e.SelectSingleNode("Name").InnerText);
                    FormatCell(sheet.Cells["D" + index], e.SelectSingleNode("StudentNumber").InnerText);
                    FormatCell(sheet.Cells["E" + index], e.SelectSingleNode("DemeritA").InnerText);
                    FormatCell(sheet.Cells["F" + index], e.SelectSingleNode("DemeritB").InnerText);
                    FormatCell(sheet.Cells["G" + index], e.SelectSingleNode("DemeritC").InnerText);
                    index++;
                }
            }

            #region �g�ٲ֭p����,Request
            h = new DSXmlHelper("Request");
            h.AddElement("Field");
            h.AddElement("Field", "All");
            h.AddElement("Condition");
            h.AddElement("Condition", "Or");
            h.AddElement("Condition/Or", "MeritFlag", "0");
            h.AddElement("Condition/Or", "MeritFlag", "2");

            h.AddElement("Condition", "RefStudentID", "-1"); //�o�u�O����!!
            foreach (string sid in _studentIDList)
                h.AddElement("Condition", "RefStudentID", sid);

            if (rbType1.Checked)
            {
                h.AddElement("Condition", "SchoolYear", cboSchoolYear.Text);
                h.AddElement("Condition", "Semester", cboSemester.Text);
            }
            h.AddElement("Order");
            h.AddElement("Order", "GradeYear", "ASC");
            h.AddElement("Order", "ClassDisplayOrder", "ASC");
            h.AddElement("Order", "ClassName", "ASC");
            h.AddElement("Order", "SeatNo", "ASC");
            h.AddElement("Order", "RefStudentID", "ASC");
            h.AddElement("Order", "OccurDate", "ASC"); 
            #endregion

            d = QueryDiscipline.GetDiscipline(new DSRequest(h));

            if (!d.HasContent)
            {
                FISCA.Presentation.Controls.MsgBox.Show("���o���Ӹ�ƿ��~:" + d.GetFault().Message, "���~", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            h = d.GetContent();

            #region �]�w�Ӽ��D�a...
            book.Worksheets.Add();
            sheet = book.Worksheets[book.Worksheets.Count - 1];
            sheet.Name = schoolName + "�g�ٲ֭p����";
            Cell titleCell = sheet.Cells["A1"];
            titleCell.Style.Borders.SetColor(Color.Black);

            titleCell.PutValue(sheet.Name);
            titleCell.Style.HorizontalAlignment = TextAlignmentType.Center;
            sheet.Cells.Merge(0, 0, 1, 7); 
            #endregion

            #region ���Title
            FormatCell(sheet.Cells["A2"], "�Z��");
            FormatCell(sheet.Cells["B2"], "�y��");
            FormatCell(sheet.Cells["C2"], "�m�W");
            FormatCell(sheet.Cells["D2"], "�Ǹ�");
            FormatCell(sheet.Cells["E2"], "�Ǧ~��");
            FormatCell(sheet.Cells["F2"], "�Ǵ�");
            FormatCell(sheet.Cells["G2"], "���");
            FormatCell(sheet.Cells["H2"], "�j�L");
            FormatCell(sheet.Cells["I2"], "�p�L");
            FormatCell(sheet.Cells["J2"], "ĵ�i");
            //FormatCell(sheet.Cells["K2"], "�d��"); //�d��b�ꤤ�t���ݩ�S�O�g��...
            FormatCell(sheet.Cells["K2"], "�ƥ�"); 
            #endregion

            int ri = 3;
            foreach (XmlElement e in h.GetElements("Discipline"))
            {
                FormatCell(sheet.Cells["A" + ri], e.SelectSingleNode("ClassName").InnerText);
                FormatCell(sheet.Cells["B" + ri], e.SelectSingleNode("SeatNo").InnerText);
                FormatCell(sheet.Cells["C" + ri], e.SelectSingleNode("Name").InnerText);
                FormatCell(sheet.Cells["D" + ri], e.SelectSingleNode("StudentNumber").InnerText);
                FormatCell(sheet.Cells["E" + ri], e.SelectSingleNode("SchoolYear").InnerText);
                FormatCell(sheet.Cells["F" + ri], e.SelectSingleNode("Semester").InnerText);
                FormatCell(sheet.Cells["G" + ri], e.SelectSingleNode("OccurDate").InnerText);
                FormatCell(sheet.Cells["H" + ri], e.SelectSingleNode("Detail/Discipline/Demerit/@A").InnerText);
                FormatCell(sheet.Cells["I" + ri], e.SelectSingleNode("Detail/Discipline/Demerit/@B").InnerText);
                FormatCell(sheet.Cells["J" + ri], e.SelectSingleNode("Detail/Discipline/Demerit/@C").InnerText);
                //FormatCell(sheet.Cells["K" + ri], e.SelectSingleNode("MeritFlag").InnerText == "2" ? "�O" : "�_");
                FormatCell(sheet.Cells["K" + ri], e.SelectSingleNode("Reason").InnerText);
                ri++;
            }


            string path = Path.Combine(Application.StartupPath, "Reports");

            //�p�G�ؿ����s�b�h�إߡC
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            path = Path.Combine(path, ConvertToValidName(A1Name) + ".xls");
            try
            {
                book.Save(path);
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

        #endregion

        private bool IsValid()
        {
            error.Clear();
            error.Tag = true;
            ValidInt(txtA, txtA, "������J�Ʀr");
            ValidInt(txtB, txtB, "������J�Ʀr");
            ValidInt(txtC, txtC, "������J�Ʀr");

            return bool.Parse(error.Tag.ToString());
        }

        private void ValidInt(Control intControl, Control showErrorControl, string message)
        {
            int i = 0;
            intControl.Tag = "0";
            if (intControl.Text == string.Empty) return;
            if (!int.TryParse(intControl.Text, out i))
            {
                error.SetError(showErrorControl, message);
                error.Tag = false;
            }
            intControl.Tag = intControl.Text;
        }

        private void FormatCell(Cell cell, string value)
        {
            cell.PutValue(value); //���
            cell.Style.Borders.SetStyle(CellBorderType.Hair);
            cell.Style.Borders.SetColor(Color.Black); //�¦�
            cell.Style.Borders.DiagonalStyle = CellBorderType.None;
            cell.Style.HorizontalAlignment = TextAlignmentType.Center;
        }

        private void FormatCellWithStandard(Cell cell, string value, string standard)
        {
            int v = 0, s = 0;
            if (!int.TryParse(value, out v)) v = 0;
            if (!int.TryParse(standard, out s)) s = -1;
            if (v >= s && s != -1)
                cell.Style.Font.Color = Color.Red;
            cell.PutValue(value);
            cell.Style.Borders.SetStyle(CellBorderType.Hair);
            cell.Style.Borders.SetColor(Color.Black);
            cell.Style.Borders.DiagonalStyle = CellBorderType.None;
            cell.Style.HorizontalAlignment = TextAlignmentType.Center;
        }

        private string ConvertToValidName(string A1Name)
        {
            char[] invalids = Path.GetInvalidFileNameChars();

            string result = A1Name;
            foreach (char each in invalids)
                result = result.Replace(each, '_');

            return result;
        }
    }
}
