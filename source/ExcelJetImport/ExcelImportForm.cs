﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

namespace JetExcelOleDbImport
{
  internal partial class ExcelImportForm : Form
  {
    public ExcelImportForm()
    {
      InitializeComponent();
    }

    public string ExcelFileName
    {
      get { return m_path_edit.Text; }
    }

    public string TableName
    {
      get { return m_table_edit.SelectedItem as string; }
    }

    private void m_button_open_excel_Click(object sender, EventArgs e)
    {
      using (var dlg = new OpenFileDialog())
      {
        dlg.Filter = "Excel files|*.xls";

        if (dlg.ShowDialog(this) == DialogResult.OK)
        {
          m_path_edit.Text = dlg.FileName;
          Utils.FillTableList(m_path_edit.Text, m_table_edit.Items);
        }
      }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
      base.OnFormClosing(e);

      if (this.DialogResult != DialogResult.OK)
        return;

      List<string> messages = new List<string>();

      if (string.IsNullOrEmpty(m_path_edit.Text))
        messages.Add("Не выбран файл Excel");
      if (string.IsNullOrEmpty(this.TableName))
        messages.Add("Не выбрана таблица");

      Utils.CheckFileIsOpen(m_path_edit.Text, messages);

      if (messages.Count > 0)
      {
        MessageBox.Show(string.Join(Environment.NewLine, messages), this.Text);
        e.Cancel = true;
      }
    }
  }
}