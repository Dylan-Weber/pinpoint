﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using FontAwesome.WPF;
using NHotkey;
using NHotkey.Wpf;
using Pinpoint.Core;
using Pinpoint.Core.Sources;
using Pinpoint.Win.Models;
using Xceed.Wpf.Toolkit;
using Color = System.Windows.Media.Color;

namespace Pinpoint.Win.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly SettingsWindow _settingsWindow;
        private static readonly Stack<string> QueryHistory = new Stack<string>();

        public MainWindow()
        {
            InitializeComponent();
            Model = new MainWindowModel();

            // Load old settings
            AppSettings.Load();

            // Load existing snippet sources
            if (AppSettings.Contains("sources"))
            {
                var sources = AppSettings.GetListAs<FileSource>("sources");
                QueryEngine.AddSources(sources);
            }
            
            // Initialize after loading settings
            _settingsWindow = new SettingsWindow();
            HotkeyManager.Current.AddOrReplace("Show/Hide", Key.Space, ModifierKeys.Alt, OnToggleVisibility);
        }

        internal MainWindowModel Model
        {
            get => (MainWindowModel)DataContext;
            set => DataContext = value;
        }

        private void OnToggleVisibility(object? sender, HotkeyEventArgs e)
        {
            if (Visibility == Visibility.Visible)
            {
                Hide();
            }
            else
            {
                Show();
                TxtQuery.Focus();
            }

            e.Handled = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Auto-focus query field
            TxtQuery.Clear();
            TxtQuery.Focus();

            // Locate window horizontal center near top of screen
            Left = SystemParameters.PrimaryScreenWidth / 2 - Width / 2;
            Top = SystemParameters.PrimaryScreenHeight / 5;
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            NotifyIcon.Dispose();
            base.OnClosing(e);
        }

        private void NotifyIcon_TrayContextMenuOpen(object sender, RoutedEventArgs e)
        {
        }

        private void NotifyIcon_PreviewTrayContextMenuOpen(object sender, RoutedEventArgs e)
        {
        }

        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            _settingsWindow.Show();
            Hide();
        }

        private void BtnSettings_MouseEnter(object sender, MouseEventArgs e)
        {
            SetSettingsColor(sender, 216, 216, 216);
        }

        private void BtnSettings_MouseLeave(object sender, MouseEventArgs e)
        {
            SetSettingsColor(sender, 87, 87, 87);
        }

        private void SetSettingsColor(object sender, byte r, byte g, byte b)
        {
            var source = (IconButton)sender;
            source.Icon = new ImageAwesome
            {
                Icon = FontAwesomeIcon.Cogs,
                Height = source.Icon.Height,
                Foreground = new SolidColorBrush(Color.FromRgb(r, g, b)),
            };
        }

        private void TxtQuery_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    if (LstResults.SelectedIndex >= 0)
                    {
                        OpenSelectedResult();
                    }
                    break;

                case Key.Down:
                    LstResults.Focus();
                    break;

                case Key.Up:
                    break;
            }
        }

        private async void TxtQuery_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtQuery.Text))
            {
                Model.Results.Clear();
                return;
            }

            await UpdateResults();
        }

        private async Task UpdateResults()
        {
            Model.Results.Clear();

            var query = new Query(TxtQuery.Text.Trim());

            if (query.IsEmpty)
            {
                return;
            }

            await foreach(var result in QueryEngine.Process(query))
            {
                Model.Results.Add(result);
            }

            if (Model.Results.Count > 0)
            {
                LstResults.SelectedIndex = 0;
            }
        }

        private void LstResults_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    OpenSelectedResult();
                    break;

                case Key.Up:
                    // First item of list is already selected so focus query field
                    if (LstResults.SelectedIndex == 0)
                    {
                        TxtQuery.Focus();
                    }
                    break;

                case Key.Back:
                    TxtQuery.Text = TxtQuery.Text[..^1];
                    TxtQuery.CaretIndex = TxtQuery.Text.Length;
                    TxtQuery.Focus();
                    break;

                case Key.Left:
                    TxtQuery.CaretIndex = TxtQuery.Text.Length - 1;
                    TxtQuery.Focus();
                    break;
            }
        }

        private void OpenSelectedResult()
        {
            var item = LstResults.SelectedItems[0] as ISource;

            if (QueryHistory.Count == 3)
            {
                QueryHistory.Pop();
            }
            QueryHistory.Push(item.Identifier);

            Debug.WriteLine(item.Location);
        }

        private void LstResults_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (LstResults.SelectedIndex >= 0)
            {
                OpenSelectedResult();
            }
        }

        private void ItmSettings_Click(object sender, RoutedEventArgs e)
        {
            _settingsWindow.Show();
            Hide();
        }

        private void ItmExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void ItmNewSimpleSnippet_OnClick(object sender, RoutedEventArgs e)
        {
            var newSimpleSnippetWindow = new SimpleSnippetWindow();
            newSimpleSnippetWindow.Show();
            Hide();
        }

        private void ItmNewCustomSnippet_OnClick(object sender, RoutedEventArgs e)
        {
            var screenCaptureOverlay = new ScrenCaptureOverlayWindow();
            screenCaptureOverlay.Show();
            Hide();
        }
    }
}