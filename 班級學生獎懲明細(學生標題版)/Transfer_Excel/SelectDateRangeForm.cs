﻿using FISCA.Presentation.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Transfer_Excel
{
    public partial class SelectDateRangeForm : BaseForm
    {
        protected DateTime _startDate;
        protected DateTime _endDate;
        protected bool _startTextBoxOK = false;
        protected bool _endTextBoxOK = false;
        protected bool _printable = false;

        public DateTime StartDate
        {
            get { return _startDate; }
        }

        public DateTime EndDate
        {
            get { return _endDate; }
        }

        public SelectDateRangeForm(string title)
            : this()
        {
            Text = title;

        }

        public SelectDateRangeForm()
        {
            InitializeComponent();

            _startDate = DateTime.Today;
            _endDate = DateTime.Today;
            dateTimeInput1.Value = _startDate;
            dateTimeInput2.Value = _endDate;
        }

        private bool ValidateRange(string startDate, string endDate)
        {
            DateTime a, b;
            a = DateTime.Parse(startDate);
            b = DateTime.Parse(endDate);

            if (DateTime.Compare(b, a) < 0)
            {
                _printable = false;
                return false;
            }
            else
            {
                _printable = true;
                _startDate = a;
                _endDate = b;
                return true;
            }
        }


        

        private void buttonX1_Click(object sender, EventArgs e)
        {
            if (_printable == true)
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void dateTimeInput2_TextChanged(object sender, EventArgs e)
        {
            errorProvider2.Clear();
            _endTextBoxOK = true;
            if (_startTextBoxOK)
            {
                if (!ValidateRange(dateTimeInput1.Text, dateTimeInput2.Text))
                    errorProvider2.SetError(dateTimeInput2, "日期區間錯誤");
                else
                {
                    errorProvider1.Clear();
                    errorProvider2.Clear();
                }
            }

        }

        private void SelectDateRangeForm_Load(object sender, EventArgs e)
        {

        }

        private void dateTimeInput1_TextChanged_1(object sender, EventArgs e)
        {
            errorProvider1.Clear();
            _startTextBoxOK = true;
            if (_endTextBoxOK)
            {
                if (!ValidateRange(dateTimeInput1.Text, dateTimeInput2.Text))
                    errorProvider1.SetError(dateTimeInput1, "日期區間錯誤");
                else
                {
                    errorProvider1.Clear();
                    errorProvider2.Clear();
                }
            }
        }
        
    }
}
