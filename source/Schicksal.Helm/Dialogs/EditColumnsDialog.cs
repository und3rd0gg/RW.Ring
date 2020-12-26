﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections.ObjectModel;
using Notung;

namespace Schicksal.Helm.Dialogs
{
  public partial class EditColumnsDialog : Form
  {
    public EditColumnsDialog()
    {
      InitializeComponent();

      m_binding_source.DataSource = new BindingList<TableColumnInfo>();
      columnTypeDataGridViewTextBoxColumn.DataSource = Enum.GetValues(typeof(TypeCode))
        .Cast<TypeCode>().Where(c => c > TypeCode.DBNull ).ToArray();
    }

    public BindingList<TableColumnInfo> Columns
    {
      get { return m_binding_source.DataSource as BindingList<TableColumnInfo>; }
    }

    private class TableColumnInfoList : KeyedCollection<string, TableColumnInfo>
    {
      protected override string GetKeyForItem(TableColumnInfo item)
      {
        return item.ColumnName;
      }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
      base.OnFormClosing(e);

      if (this.DialogResult != System.Windows.Forms.DialogResult.OK)
        return;

      InfoBuffer buffer = new InfoBuffer();
      HashSet<string> unique = new HashSet<string>();

      if (this.Columns.Count == 0)
        buffer.Add("No columns specified", InfoLevel.Warning);

      foreach (var col in this.Columns)
      {
        if (string.IsNullOrEmpty(col.ColumnName))
          buffer.Add("Empty column name", InfoLevel.Warning);
        else if (!unique.Add(col.ColumnName))
          buffer.Add(string.Format("Column name {0} is not unique", col.ColumnName), InfoLevel.Warning);
      }

      if (buffer.Count > 0)
      {
        AppManager.Notificator.Show(buffer);
        e.Cancel = true;
      }
    }
  }

  public class TableColumnInfo
  {
    public TableColumnInfo()
    {
      this.ColumnType = TypeCode.Double;
    }

    public string ColumnName { get; set; }

    public TypeCode ColumnType { get; set; }

    public static DataTable CreateTable(IList<TableColumnInfo> columns)
    {
      if (columns == null)
        throw new ArgumentNullException("columns");

      DataTable table = new DataTable();

      foreach (var col in columns)
        table.Columns.Add(col.ColumnName, Type.GetType("System." + col.ColumnType));

      return table;
    }
  }
}