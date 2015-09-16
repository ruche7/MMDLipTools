﻿using System;
using System.Windows;
using System.Windows.Input;

namespace LipVmdMaker
{
    /// <summary>
    /// MainWindow の View クラス。
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// コンストラクタ。
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 閉じるボタンの押下時に呼び出される。
        /// </summary>
        private void OnCloseCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }
    }
}
