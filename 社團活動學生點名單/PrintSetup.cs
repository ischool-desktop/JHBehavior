using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevComponents.DotNetBar;
using System.Xml;
using JHSchool;
using System.IO;
using FISCA.Presentation.Controls;
using K12.Data.Configuration;
using FISCA.DSAUtil;

namespace Behavior.ClubActivitiesPointList
{
    public partial class PrintSetup : BaseForm
    {        
        private GetConfigData _Getcd;
        private XmlElement print;
        private string BufferString = null;
        private byte[] _buffer;
        private string base64;

        public PrintSetup(GetConfigData cd)
        {
            InitializeComponent();
            _Getcd = cd;

        }

        private void AttendanceSetup_Load(object sender, EventArgs e)
        {
            if (_Getcd._PrintMode == "true")
            {
                radioButton1.Checked = true;
            }
            else
            {
                radioButton2.Checked = true;
            }

            cbWeekDay.SelectedIndex = _Getcd._Week;

            _buffer = _Getcd._buffer;
            if (_buffer != null)
            {
                base64 = Convert.ToBase64String(_buffer);
            }
            else
            {
                base64 = "";
            }
        }


        private void btnSave_Click(object sender, EventArgs e)
        {
            #region �x�s
            string RedioCheck = "true";
            if (radioButton1.Checked) //true�N�O�w�]�d��
            {
                RedioCheck = "true";
            }
            else
            {
                RedioCheck = "false";
            }

            DSXmlHelper dsx = new DSXmlHelper("PrintSetup");
            dsx.AddElement("PrintMode");
            dsx.SetAttribute("PrintMode", "bool", RedioCheck);
            dsx.AddElement("PrintTemp");
            dsx.SetAttribute("PrintTemp", "Temp", base64);
            dsx.AddElement("Week");
            dsx.SetAttribute("Week", "day", cbWeekDay.SelectedIndex.ToString()); //�w�]��P���@

            try
            {
                _Getcd.SavePrint(dsx.BaseElement);
            }
            catch(Exception ex)
            {
                MsgBox.Show("�x�s����!" + ex.Message);
                return;
            }
            MsgBox.Show("�x�s�]�w���\!");
            this.Close();

            #endregion
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            #region �d�ݽd��
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "�t�s�s��";
            sfd.FileName = "(�d��)���ά��ʽd��.doc";
            sfd.Filter = "Word�ɮ� (*.doc)|*.doc|�Ҧ��ɮ� (*.*)|*.*";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    FileStream fs = new FileStream(sfd.FileName, FileMode.Create);
                    fs.Write(Properties.Resources.���ά��ʽd��, 0, Properties.Resources.���ά��ʽd��.Length);
                    fs.Close();
                    System.Diagnostics.Process.Start(sfd.FileName);
                }
                catch
                {
                    MsgBox.Show("���w���|�L�k�s���C", "�t�s�ɮץ���", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            } 
            #endregion
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            #region �d�ݦ۩w�d��
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "�t�s�s��";
            sfd.FileName = "(�ۭq�d��)���ά��ʽd��.doc";
            sfd.Filter = "Word�ɮ� (*.doc)|*.doc|�Ҧ��ɮ� (*.*)|*.*";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    FileStream fs = new FileStream(sfd.FileName, FileMode.Create);
                    if (_buffer == null)
                    {
                        MsgBox.Show("�|�L�۩w�d��,�ФW�ǽd��!");
                        return;
                    }
                    if (Aspose.Words.Document.DetectFileFormat(new MemoryStream(_buffer)) == Aspose.Words.LoadFormat.Doc)
                        fs.Write(_buffer, 0, _buffer.Length);
                    else
                        fs.Write(Properties.Resources.���ά��ʽd��, 0, Properties.Resources.���ά��ʽd��.Length);
                    fs.Close();
                    System.Diagnostics.Process.Start(sfd.FileName);
                }
                catch
                {
                    MsgBox.Show("���w���|�L�k�s���C", "�t�s�ɮץ���", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            } 
            #endregion
        }

        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            #region �W�Ǧ۩w�d��
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "�п�ܦۭq�d��";
            ofd.Filter = "Word�ɮ� (*.doc)|*.doc";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if (Aspose.Words.Document.DetectFileFormat(ofd.FileName) == Aspose.Words.LoadFormat.Doc)
                    {
                        FileStream fs = new FileStream(ofd.FileName, FileMode.Open);
                        byte[] tempBuffer = new byte[fs.Length];
                        _buffer = tempBuffer;
                        fs.Read(tempBuffer, 0, tempBuffer.Length);
                        base64 = Convert.ToBase64String(tempBuffer);
                        //_isUpload = true;
                        fs.Close();
                        MsgBox.Show("�W�Ǧ��\�C");
                        radioButton2.Checked = true;
                    }
                    else
                        MsgBox.Show("�W���ɮ׮榡����");
                }
                catch
                {
                    MsgBox.Show("���w���|�L�k�s���C", "�}���ɮץ���", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            } 
            #endregion
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}