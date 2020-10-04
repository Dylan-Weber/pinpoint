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
using Pinpoint.Core.Snippets;
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
        private readonly QueryEngine _queryEngine;

        public MainWindow()
        {
            InitializeComponent();
            Model = new MainWindowModel();

            // Load old settings
            AppSettings.Load();

            // Load existing snippet sources
            _queryEngine = new QueryEngine();
            _settingsWindow = new SettingsWindow(this, _queryEngine);

            _queryEngine.Initialize();

            HotkeyManager.Current.AddOrReplace(AppConstants.HotkeyIdentifier, Key.Space, ModifierKeys.Alt, OnToggleVisibility);
        }

        internal MainWindowModel Model
        {
            get => (MainWindowModel)DataContext;
            set => DataContext = value;
        }

        public void OnToggleVisibility(object? sender, HotkeyEventArgs e)
        {
            if (_settingsWindow.Visibility == Visibility.Visible)
            {
                return;
            }

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

        private async void TxtQuery_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    if (LstResults.SelectedIndex >= 0)
                    {
                        OpenSelectedResult();
                        TxtQuery.Clear();
                    }
                    break;

                case Key.Down:
                    LstResults.Focus();
                    break;

                case Key.Up:
                    break;

                default:
                    await UpdateResults();
                    break;
            }
        }

        private void TxtQuery_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtQuery.Text))
            {
                Model.Results.Clear();
            }
        }

        private async Task UpdateResults()
        {
            Model.Results.Clear();

            var query = new Query(TxtQuery.Text.Trim());

            if (query.IsEmpty)
            {
                return;
            }

            await foreach(var result in _queryEngine.Process(query))
            {
                Model.Results.Add(result);
            }

            if (Model.Results.Count > 0 && LstResults.SelectedIndex == -1)
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
            switch (LstResults.SelectedItems[0])
            {
                case OcrTextSnippet s:
                    var ocrSnippetWindow = new OcrSnippetWindow(_queryEngine, s);
                    ocrSnippetWindow.Show();
                    break;

                case TextSnippet s:
                    var textSnippetWindow = new TextSnippetWindow(_queryEngine, s);
                    textSnippetWindow.Show();
                    break;

                case FileSnippet s:
                    Process.Start(s.FilePath);
                    break;
            }

            Hide();
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
            var newSimpleSnippetWindow = new TextSnippetWindow(_queryEngine);
            newSimpleSnippetWindow.Show();
            Hide();
        }

        private void ItmNewCustomSnippet_OnClick(object sender, RoutedEventArgs e)
        {
            var screenCaptureOverlay = new ScreenCaptureOverlayWindow(_queryEngine);
            screenCaptureOverlay.Show();
            Hide();
        }
    }
}