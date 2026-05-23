using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using RobloxMacro.ViewModels;
using RobloxMacro.Services;
using RobloxMacro.Models;

namespace RobloxMacro
{
    /// <summary>
    /// Главное окно приложения
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainViewModel _viewModel;
        private HotkeyManager _hotkeyManager;
        private HwndSource _hwndSource;

        // Windows API для перехвата горячих клавиш
        private const int WM_HOTKEY = 0x0312;

        public MainWindow()
        {
            // Установить конвертеры до загрузки XAML, чтобы StaticResource-ссылки работали корректно
            SetupConverters();

            InitializeComponent();

            // Инициализировать ViewModel
            _viewModel = new MainViewModel();
            this.DataContext = _viewModel;

            // Загрузить сохранённые настройки, если есть
            try
            {
                var settings = RobloxMacro.Services.SettingsService.Load();
                _viewModel.HotkeyStartStopKey = settings.HotkeyStartStopKey;
                _viewModel.HotkeyRecordKey = settings.HotkeyRecordKey;
                _viewModel.SaveFolderPath = settings.SaveFolderPath;
                _viewModel.CameraWidth = settings.CameraWidth;
                _viewModel.CameraHeight = settings.CameraHeight;
                _viewModel.CameraDepth = settings.CameraDepth;
                _viewModel.IsAlwaysOnTop = settings.IsAlwaysOnTop;
            }
            catch { }
            GameModeClickCommand = new RelayCommand(OnGameModeClick);
            this.SetBinding(TopmostProperty, new Binding(nameof(MainViewModel.IsAlwaysOnTop)) { Source = _viewModel, Mode = BindingMode.OneWay });

            // Подписаться на события окна
            this.Loaded += MainWindow_Loaded;
            this.Closed += MainWindow_Closed;
        }

        /// <summary>
        /// Обработчик загрузки окна
        /// </summary>
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Получить хендл окна для регистрации горячих клавиш
            IntPtr handle = new WindowInteropHelper(this).Handle;
            _hwndSource = HwndSource.FromHwnd(handle);

            if (_hwndSource != null)
            {
                _hwndSource.AddHook(HwndHook);
            }

            // Инициализировать менеджер горячих клавиш
            _hotkeyManager = new HotkeyManager(handle);

            // Зарегистрировать глобальные горячие клавиши
            RegisterHotkeys();

            // Установить фокус на окно
            this.Focus();
        }

        /// <summary>
        /// Обработчик закрытия окна
        /// </summary>
        private void MainWindow_Closed(object sender, EventArgs e)
        {
            // Очистить горячие клавиши
            _hotkeyManager?.Dispose();

            // Остановить все процессы
            if (_viewModel.IsRecording)
                _viewModel.StopRecording();

            if (_viewModel.IsPlaying)
                _viewModel.StopPlayback();

            // Удалить хук
            _hwndSource?.RemoveHook(HwndHook);
            _hwndSource?.Dispose();
        }

        /// <summary>
        /// Перехватчик сообщений окна (для обработки горячих клавиш)
        /// </summary>
        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY)
            {
                int hotkeyId = wParam.ToInt32();
                _hotkeyManager?.ProcessHotkey(hotkeyId);
                handled = true;
            }

            return IntPtr.Zero;
        }

        /// <summary>
        /// Зарегистрировать глобальные горячие клавиши
        /// </summary>
        private void RegisterHotkeys()
        {
            // Регистрируем горячие клавиши из настроек (если регистрация не удалась, сообщаем)
            _hotkeyManager.UnregisterAll();

            int startVk = _viewModel?.HotkeyStartStopKey ?? (int)InputSimulator.VirtualKeys.VK_F7;
            int recordVk = _viewModel?.HotkeyRecordKey ?? (int)InputSimulator.VirtualKeys.VK_F9;

            var id1 = _hotkeyManager.RegisterHotkey((uint)startVk, TogglePlayback, HotkeyModifier.None);
            if (id1 == -1) _viewModel.StatusMessage = "✗ Не удалось зарегистрировать горячую клавишу Start/Stop";

            var id2 = _hotkeyManager.RegisterHotkey((uint)recordVk, ToggleRecording, HotkeyModifier.None);
            if (id2 == -1) _viewModel.StatusMessage = "✗ Не удалось зарегистрировать горячую клавишу Record";
        }

        /// <summary>
        /// Команда переключения записи (F9)
        /// </summary>
        private void ToggleRecording()
        {
            if (_viewModel.IsRecording)
            {
                _viewModel.StopRecording();
            }
            else
            {
                _viewModel.StartRecording();
            }
        }

        /// <summary>
        /// Обновить регистрацию горячих клавиш (например, после изменения в настройках)
        /// </summary>
        private void UpdateRegisteredHotkeys()
        {
            if (_hotkeyManager == null || _viewModel == null)
                return;

            RegisterHotkeys();
        }

        private void OnOpenSettings(object sender, RoutedEventArgs e)
        {
            var settingsWin = new SettingsWindow(_viewModel) { Owner = this };
            settingsWin.ShowDialog();
            // После закрытия окна настроек перерегистрируем горячие клавиши
            UpdateRegisteredHotkeys();
        }

        /// <summary>
        /// Команда переключения воспроизведения (F7)
        /// </summary>
        private void TogglePlayback()
        {
            if (_viewModel.IsPlaying)
            {
                _viewModel.StopPlayback();
            }
            else if (_viewModel.RecordedActionsCount > 0)
            {
                _viewModel.StartPlayback();
            }
        }

        // Panic functionality removed

        /// <summary>
        /// Обработчик нажатия кнопки "Начать запись"
        /// </summary>
        private void OnStartRecording(object sender, RoutedEventArgs e)
        {
            _viewModel.StartRecording();
        }

        /// <summary>
        /// Обработчик нажатия кнопки "Остановить запись"
        /// </summary>
        private void OnStopRecording(object sender, RoutedEventArgs e)
        {
            _viewModel.StopRecording();
        }

        /// <summary>
        /// Обработчик нажатия кнопки "Запустить повтор"
        /// </summary>
        private void OnStartPlayback(object sender, RoutedEventArgs e)
        {
            _viewModel.StartPlayback();
        }

        /// <summary>
        /// Обработчик нажатия кнопки "Остановить повтор"
        /// </summary>
        private void OnStopPlayback(object sender, RoutedEventArgs e)
        {
            _viewModel.StopPlayback();
        }

        /// <summary>
        /// Обработчик нажатия кнопки "Настроить камеру"
        /// </summary>
        private void OnSetupCamera(object sender, RoutedEventArgs e)
        {
            _viewModel.SetupRobloxCamera();
        }

        /// <summary>
        /// Обработчик нажатия кнопки "Очистить запись"
        /// </summary>
        private void OnClearRecording(object sender, RoutedEventArgs e)
        {
            _viewModel.ClearRecordedActions();
        }

        /// <summary>
        /// Обработчик выбора режима игры
        /// </summary>
        private void OnGameModeClick(object parameter)
        {
            if (parameter is GameMode selectedMode)
            {
                _viewModel.SelectedGameMode = selectedMode;
                _viewModel.StatusMessage = $"Режим установлен: {selectedMode}";
            }
        }

        /// <summary>
        /// Команда для обработки выбора режима игры
        /// </summary>
        public RelayCommand GameModeClickCommand { get; set; }

        /// <summary>
        /// Установить конвертеры для привязки данных
        /// </summary>
        private void SetupConverters()
        {
            // Создать ресурсы для конвертеров
            this.Resources.Add("InvertBoolConverter", new InvertBooleanConverter());
            this.Resources.Add("RecordingIndicatorConverter", new RecordingIndicatorConverter());
            this.Resources.Add("PlayingIndicatorConverter", new PlayingIndicatorConverter());
        }
    }

    /// <summary>
    /// Релейная команда (простая реализация ICommand)
    /// </summary>
    public class RelayCommand : System.Windows.Input.ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        public event EventHandler CanExecuteChanged
        {
            add { System.Windows.Input.CommandManager.RequerySuggested += value; }
            remove { System.Windows.Input.CommandManager.RequerySuggested -= value; }
        }

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute ?? (_ => true);
        }

        public bool CanExecute(object parameter) => _canExecute(parameter);

        public void Execute(object parameter) => _execute(parameter);
    }

    /// <summary>
    /// Конвертер для инвертирования булева значения
    /// </summary>
    public class InvertBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool boolValue)
                return !boolValue;
            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool boolValue)
                return !boolValue;
            return false;
        }
    }

    /// <summary>
    /// Конвертер для отображения индикатора записи
    /// </summary>
    public class RecordingIndicatorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool isRecording && isRecording)
                return System.Windows.Media.Brushes.Red;
            return System.Windows.Media.Brushes.DarkGreen;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Конвертер для отображения индикатора воспроизведения
    /// </summary>
    public class PlayingIndicatorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool isPlaying && isPlaying)
                return System.Windows.Media.Brushes.LimeGreen;
            return System.Windows.Media.Brushes.DarkRed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
