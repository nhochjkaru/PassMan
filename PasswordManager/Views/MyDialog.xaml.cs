﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PasswordManager.Views
{
    /// <summary>
    /// Interaction logic for MyDialog.xaml
    /// </summary>
    partial class MyDialog : Window
    {

        public MyDialog()
        {
            InitializeComponent();
            ResponseTextBox.Focus();
        }

        public PasswordBox ResponseText
        {
            get { return ResponseTextBox; }
            set { ResponseTextBox = value; }
        }

        private void OKButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
