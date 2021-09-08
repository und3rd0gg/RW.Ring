﻿using System.Windows;
using System.Windows.Input;

namespace Notung.Feuerzauber.Dialogs
{
  /// <summary>
  /// Логика взаимодействия для AboutBox.xaml
  /// </summary>
  public partial class AboutBox : Window
  {
    public AboutBox()
    {
      InitializeComponent();

      this.DataContext = ApplicationInfo.Instance;
    }

    private void Header_MouseDown(object sender, MouseButtonEventArgs e)
    {
      this.DragMove();
    }
  }
}
