using System;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using RobloxMacro.ViewModels;
using RobloxMacro.Services;

namespace RobloxMacro
{
    public partial class SettingsWindow : Window
    {
        private readonly MainViewModel _vm;

        public SettingsWindow(MainViewModel vm)
        {
            InitializeComponent();
            _vm = vm;
            this.DataContext = _vm;
        }

        private void HotkeyBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            if (sender is System.Windows.Controls.TextBox tb && tb.Tag is string tag)
            {
                // Convert Key to VK code via KeyInterop
                var vk = System.Windows.Input.KeyInterop.VirtualKeyFromKey(e.Key);
                var keyName = e.Key.ToString();

                if (tag == "Start")
                {
                    _vm.HotkeyStartStopKey = vk;
                    _vm.HotkeyStartStopDisplay = keyName;
                }
                else if (tag == "Record")
                {
                    _vm.HotkeyRecordKey = vk;
                    _vm.HotkeyRecordDisplay = keyName;
                }

                tb.Text = keyName;
            }
        }

        private void BrowseFolder_Click(object sender, RoutedEventArgs e)
        {
            using var dlg = new System.Windows.Forms.FolderBrowserDialog();
            dlg.SelectedPath = _vm.SaveFolderPath;
            var res = dlg.ShowDialog();
            if (res == System.Windows.Forms.DialogResult.OK)
            {
                _vm.SaveFolderPath = dlg.SelectedPath;
            }
        }

        private void SavePathBox_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is System.Windows.Controls.TextBox tb)
            {
                // При клике раскрываем поле: включаем перенос строк и выбираем весь текст
                tb.TextWrapping = System.Windows.TextWrapping.Wrap;
                tb.AcceptsReturn = true;
                tb.SelectAll();
                e.Handled = false; // allow focus
            }
        }

        private void SavePathBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.TextBox tb)
            {
                // Вернуть в однострочный режим
                tb.TextWrapping = System.Windows.TextWrapping.NoWrap;
                tb.AcceptsReturn = false;
            }
        }

        private void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            var data = new SettingsData(_vm.HotkeyStartStopKey, _vm.HotkeyRecordKey, _vm.SaveFolderPath, _vm.CameraWidth, _vm.CameraHeight, _vm.CameraDepth, _vm.IsAlwaysOnTop);
            SettingsService.Save(data);
            MessageBox.Show("Настройки сохранены", "OK", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SaveMacro_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog()
            {
                DefaultExt = ".json",
                Filter = "JSON files (*.json)|*.json|All files|*.*",
                InitialDirectory = _vm.SaveFolderPath
            };
            if (dlg.ShowDialog() == true)
            {
                var filename = System.IO.Path.GetFileName(dlg.FileName);
                if (_vm.SaveMacroToFile(filename))
                    MessageBox.Show("Макрос сохранен", "OK", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void LoadMacro_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog()
            {
                DefaultExt = ".json",
                Filter = "JSON files (*.json)|*.json|All files|*.*",
                InitialDirectory = _vm.SaveFolderPath
            };
            if (dlg.ShowDialog() == true)
            {
                if (_vm.LoadMacroFromFile(dlg.FileName))
                    MessageBox.Show("Макрос загружен", "OK", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
