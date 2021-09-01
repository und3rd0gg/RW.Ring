﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Notung.Logging;

namespace LogAnalyzer
{
  public class TablePresenter : INotifyPropertyChanged
  {
    private string m_file_name = string.Empty;
    private string m_separator = "===============================================";
    private string m_template = "[{Date}] [{Level}] [{Process}] [{Source}]\r\n{Message}";
    private int m_current_file = -1;
    private ObservableCollection<FileEntry> m_file_entries = new ObservableCollection<FileEntry>();
    private readonly Dictionary<string, string> m_filters = new Dictionary<string, string>();

    public event PropertyChangedEventHandler PropertyChanged;
    public event EventHandler<ExceptionEventArgs> ExceptionOccured;

    public ObservableCollection<FileEntry> OpenedFiles
    {
      get { return m_file_entries; }
    }

    public FileEntry CurrentFile
    {
      get
      {
        if (m_current_file >= 0 && m_current_file < m_file_entries.Count)
          return m_file_entries[m_current_file];
        else
          return null;
      }
      set
      {
        if (value != null)
          ChangeCurrentFile(m_file_entries.IndexOf(value));
        else
          ChangeCurrentFile(-1);
      }
    }

    public string Separator
    {
      get { return m_separator; }
      set
      {
        if (object.Equals(m_separator, value))
          return;

        m_separator = value;
        this.OnPropertyChanged("Separator");
      }
    }

    public string MessageTemplate
    {
      get { return m_template; }
      set
      {
        if (object.Equals(m_template, value))
          return;

        m_template = value;
        this.OnPropertyChanged("MessageTemplate");
      }
    }

    public override string ToString()
    {
      return m_file_name ?? string.Empty;
    }

    public void OpenConfig(string fileName)
    {
      ConfigXmlDocument doc = new ConfigXmlDocument();
      doc.Load(fileName);

      var nodeList = doc.SelectNodes("/configuration/applicationSettings/Notung.Logging.LogSettings/setting");

      foreach (var element in nodeList.OfType<XmlElement>())
      {
        if (element.GetAttribute("name") == "Separator")
          this.Separator = element.SelectSingleNode("value").InnerText;

        if (element.GetAttribute("name") == "MessageTemplate")
          this.MessageTemplate = element.SelectSingleNode("value").InnerText;
      }
    }

    public void OpenLog(string fileName)
    {
      for (int i = 0; i < m_file_entries.Count; i++)
      {
        if (m_file_entries[i].FileName.Equals(fileName))
        {
          ChangeCurrentFile(i);
          return;
        }
      }

      try
      {
        m_file_entries.Add(new FileEntry
        {
          FileName = fileName,
          Table = this.LoadLogTable(fileName)
        });

        ChangeCurrentFile(m_file_entries.Count - 1);
      }
      catch (Exception ex)
      {
        if (this.ExceptionOccured != null)
          this.ExceptionOccured(this, new ExceptionEventArgs(ex));
      }
    }

    public void OpenDirectory(string selectedPath)
    {
      if (Directory.GetFiles(selectedPath, "*.log").Length == 0 &&
        Directory.Exists(Path.Combine(selectedPath, "Logs")))
        selectedPath = Path.Combine(selectedPath, "Logs");

      for (int i = 0; i < m_file_entries.Count; i++)
      {
        if (m_file_entries[i].FileName.Equals(selectedPath))
        {
          ChangeCurrentFile(i);
          return;
        }
      }

      try
      {
        m_file_entries.Add(new FileEntry
        {
          FileName = selectedPath,
          Table = this.LoadLogDirectory(selectedPath)
        });

        ChangeCurrentFile(m_file_entries.Count - 1);
      }
      catch (Exception ex)
      {
        if (this.ExceptionOccured != null)
          this.ExceptionOccured(this, new ExceptionEventArgs(ex));
      }
    }

    public string GetDirectoryPath()
    {
      var path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

      if (Directory.Exists(Path.Combine(path, "ARI")))
        return Path.Combine(path, "ARI");

      var dirs = Directory.GetDirectories(path);

      if (dirs.Length > 0)
        return dirs[0];
      else
        return path;
    }

    public void ClosePage(FileEntry page)
    {
      if (page == null)
        return;

      var index = m_file_entries.IndexOf(page);

      if (index < 0)
        return;

      var old_value = m_current_file;

      if (index <= old_value)
      {
        m_file_entries.RemoveAt(index);

        if (m_current_file >= 0)
          this.ChangeCurrentFile(old_value - 1);
        else if (m_file_entries.Count > 0)
          this.ChangeCurrentFile(0);
      }
      else
        m_file_entries.RemoveAt(index);
    }

    public void SetFilter(string column, string value)
    {
      if (string.IsNullOrEmpty(column))
        return;

      if (string.IsNullOrEmpty(value))
        m_filters.Remove(column);
      else
        m_filters[column] = value;

      if (this.CurrentFile != null)
      {
        StringBuilder sb = new StringBuilder();
        bool first = true;

        foreach (var kv in m_filters)
        {
          if (!this.CurrentFile.Table.Columns.Contains(kv.Key))
            continue;

          if (first)
            first = false;
          else
            sb.Append(" AND ");

          sb.AppendFormat("Convert({0}, System.String) LIKE '{1}%'", kv.Key, kv.Value.Replace("'", "''"));
        }

        CurrentFile.Table.DefaultView.RowFilter = sb.ToString();
      }
    }

    private void OnPropertyChanged(string property)
    {
      if (this.PropertyChanged != null)
        this.PropertyChanged(this, new PropertyChangedEventArgs(property));
    }

    private DataTable LoadLogTable(string fileName)
    {
      var table = new DataTable();
      table.BeginLoadData();

      this.FillTable(fileName, table);

      table.EndLoadData();
      return table;
    }

    private DataTable LoadLogDirectory(string path)
    {
      var table = new DataTable();
      table.BeginLoadData();

      foreach (var fileName in Directory.GetFiles(path, "*.log"))
      {
        this.FillTable(fileName, table);
      }

      table.EndLoadData();
      return table;
    }

    private void FillTable(string fileName, DataTable table)
    {
      List<string> lines = new List<string>();
      var builder = new LogStringBuilder(this.MessageTemplate);

      using (var reader = new StreamReader(fileName))
      {
        string line = null;

        while ((line = reader.ReadLine()) != null)
        {
          if (line.StartsWith("==============================="))
          {
            builder.FillRow(string.Join(Environment.NewLine, lines), table, true);
            lines.Clear();

            if (table.Columns.Contains("Message"))
            {
              var message = table.Rows[table.Rows.Count - 1]["Message"].ToString();
              var message_lines = message.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

              table.Rows[table.Rows.Count - 1]["Message"] = message_lines[0];

              if (message_lines.Length > 1)
                table.Rows[table.Rows.Count - 1]["Details"] = string.Join(Environment.NewLine, message_lines.Skip(1));
            }
          }
          else
            lines.Add(line);
        }
      }
    }

    private void ChangeCurrentFile(int index)
    {
      if (m_current_file == index)
        return;
      
      m_current_file = index;

      m_filters.Clear();

      this.OnPropertyChanged("CurrentFile");
    }
  }

  public class FileEntry
  {
    private static readonly string _user_directory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    
    public string FileName { get; set; }

    public DataTable Table { get; set; }

    public override string ToString()
    {
      string file = this.FileName;

      if (file == null)
        return "log.log";

      if (file.StartsWith(_user_directory))
        file = file.Substring(_user_directory[_user_directory.Length - 1] == '\\' ?
          _user_directory.Length : _user_directory.Length + 1);

      if (Path.GetFileName(Path.GetDirectoryName(file)).Equals("Logs", StringComparison.OrdinalIgnoreCase))
        file = Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(file)), Path.GetFileName(file));

      return file;
    }
  }

  public class ExceptionEventArgs : EventArgs
  {
    public ExceptionEventArgs(Exception error)
    {
      this.Error = error;
    }

    public Exception Error { get; private set; }
  }
}