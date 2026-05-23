using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows.Input;
using RobloxMacro.Models;
using RobloxMacro.Services;

namespace RobloxMacro.ViewModels
{
    /// <summary>
    /// ViewModel для главного окна приложения
    /// Реализует INotifyPropertyChanged для привязки данных к UI
    /// </summary>
    public class MainViewModel : INotifyPropertyChanged
    {
        private MacroRecorder _recorder = new();
        private MacroPlayer _player = new();
        private GameMode _selectedGameMode = GameMode.StoryMode;
        private string _statusMessage = "Готово";
        private bool _isRecording = false;
        private bool _isPlaying = false;
        private int _recordedActionsCount = 0;
        private int _macroRepetitions = 1;
        private bool _infiniteRepeat = false;
        private bool _isAlwaysOnTop = false;
        private TimeSpan _recordingDuration = TimeSpan.Zero;
        private string _recordingTimeDisplay = "00:00:00";
        private DateTime? _recordingStartTime;
        private readonly DispatcherTimer _recordingTimer;

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Выбранный игровой режим
        /// </summary>
        public GameMode SelectedGameMode
        {
            get => _selectedGameMode;
            set
            {
                if (_selectedGameMode != value)
                {
                    _selectedGameMode = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(SelectedGameModeName));
                    StatusMessage = $"Режим изменен на: {GetGameModeName(value)}";
                }
            }
        }

        /// <summary>
        /// Сообщение статуса
        /// </summary>
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                if (_statusMessage != value)
                {
                    _statusMessage = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Идет ли запись макроса
        /// </summary>
        public bool IsRecording
        {
            get => _isRecording;
            set
            {
                if (_isRecording != value)
                {
                    _isRecording = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Идет ли воспроизведение макроса
        /// </summary>
        public bool IsPlaying
        {
            get => _isPlaying;
            set
            {
                if (_isPlaying != value)
                {
                    _isPlaying = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Количество записанных действий
        /// </summary>
        public int RecordedActionsCount
        {
            get => _recordedActionsCount;
            set
            {
                if (_recordedActionsCount != value)
                {
                    _recordedActionsCount = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Количество повторений макроса
        /// </summary>
        public int MacroRepetitions
        {
            get => _macroRepetitions;
            set
            {
                if (_macroRepetitions != value)
                {
                    _macroRepetitions = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Бесконечное повторение макроса
        /// </summary>
        public bool InfiniteRepeat
        {
            get => _infiniteRepeat;
            set
            {
                if (_infiniteRepeat != value)
                {
                    _infiniteRepeat = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _hotkeyStartStopKey = 0x76; // VK_F7 by default
        private int _hotkeyRecordKey = 0x78; // VK_F9 by default

        /// <summary>
        /// Горячая клавиша для старта/остановки (VK code)
        /// </summary>
        public int HotkeyStartStopKey
        {
            get => _hotkeyStartStopKey;
            set
            {
                if (_hotkeyStartStopKey != value)
                {
                    _hotkeyStartStopKey = value;
                    HotkeyStartStopDisplay = KeyInterop.KeyFromVirtualKey(value).ToString();
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Горячая клавиша для записи (VK code)
        /// </summary>
        public int HotkeyRecordKey
        {
            get => _hotkeyRecordKey;
            set
            {
                if (_hotkeyRecordKey != value)
                {
                    _hotkeyRecordKey = value;
                    HotkeyRecordDisplay = KeyInterop.KeyFromVirtualKey(value).ToString();
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Отображаемые подписи для полей захвата горячих клавиш
        /// </summary>
        public string HotkeyStartStopDisplay { get; set; } = KeyInterop.KeyFromVirtualKey(0x76).ToString();
        public string HotkeyRecordDisplay { get; set; } = KeyInterop.KeyFromVirtualKey(0x78).ToString();

        /// <summary>
        /// Путь к папке для сохранения макросов
        /// </summary>
        public string SaveFolderPath { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        /// <summary>
        /// Параметры положения камеры (ширина/высота/дальность)
        /// </summary>
        public int CameraWidth { get; set; } = 1920;
        public int CameraHeight { get; set; } = 1080;
        public int CameraDepth { get; set; } = 0;

        /// <summary>
        /// Удерживать окно поверх других приложений
        /// </summary>
        public bool IsAlwaysOnTop
        {
            get => _isAlwaysOnTop;
            set
            {
                if (_isAlwaysOnTop != value)
                {
                    _isAlwaysOnTop = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Отображаемое название выбранного режима
        /// </summary>
        public string SelectedGameModeName => GetGameModeName(_selectedGameMode);

        /// <summary>
        /// Текущая продолжительность записи
        /// </summary>
        public string RecordingTimeDisplay
        {
            get => _recordingTimeDisplay;
            private set
            {
                if (_recordingTimeDisplay != value)
                {
                    _recordingTimeDisplay = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Список доступных игровых режимов
        /// </summary>
        public ObservableCollection<GameModeItem> GameModes { get; } = new()
        {
            new GameModeItem { Mode = GameMode.StoryMode, Name = "Story Mode", Icon = "📖" },
            new GameModeItem { Mode = GameMode.LegendStage, Name = "Legends Stage", Icon = "👑" },
            new GameModeItem { Mode = GameMode.InfinityMode, Name = "Infinity Mode", Icon = "♾️" },
            new GameModeItem { Mode = GameMode.Raid, Name = "Raid", Icon = "⚔️" },
            new GameModeItem { Mode = GameMode.Challenge, Name = "Challenge", Icon = "⭐" }
        };

        /// <summary>
        /// Получить рекордер
        /// </summary>
        public MacroRecorder Recorder => _recorder;

        /// <summary>
        /// Получить плеер
        /// </summary>
        public MacroPlayer Player => _player;

        /// <summary>
        /// Инициализировать ViewModel
        /// </summary>
        public MainViewModel()
        {
            // Подписаться на события рекордера
            _recorder.ActionRecorded += OnActionRecorded;

            // Подписаться на события плеера
            _player.StatusChanged += OnPlayerStatusChanged;
            _player.PlaybackCompleted += OnPlaybackCompleted;
            _player.PlaybackError += OnPlaybackError;

            _recordingTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            _recordingTimer.Tick += (sender, args) => UpdateRecordingDuration();
        }

        /// <summary>
        /// Начать запись макроса
        /// </summary>
        public void StartRecording()
        {
            if (_recorder.IsRecording || _isPlaying)
                return;

            _recorder.Clear();
            _recorder.StartRecording();
            _recordingStartTime = DateTime.UtcNow;
            RecordingTimeDisplay = "00:00:00";
            _recordingTimer.Start();
            IsRecording = true;
            StatusMessage = "🔴 Запись началась...";
        }

        /// <summary>
        /// Остановить запись макроса
        /// </summary>
        public void StopRecording()
        {
            if (!_recorder.IsRecording)
                return;

            _recorder.StopRecording();
            _recordingTimer.Stop();
            UpdateRecordingDuration();
            IsRecording = false;
            RecordedActionsCount = _recorder.RecordedActions.Count;
            StatusMessage = $"✓ Запись остановлена ({RecordedActionsCount} действий, {RecordingTimeDisplay})";

            // Загрузить записанный макрос в плеер
            _player.SetMacro(_recorder.RecordedActions);
        }

        /// <summary>
        /// Очистить записанные действия и сбросить время записи
        /// </summary>
        public void ClearRecordedActions()
        {
            if (_recorder.IsRecording)
                return;

            _recorder.Clear();
            _player.SetMacro(Array.Empty<MacroAction>());
            RecordedActionsCount = 0;
            RecordingTimeDisplay = "00:00:00";
            StatusMessage = "✓ Запись очищена";
        }

        /// <summary>
        /// Запустить воспроизведение макроса
        /// </summary>
        public void StartPlayback()
        {
            if (_isPlaying || _recorder.RecordedActions.Count == 0)
                return;

            int repetitions = _infiniteRepeat ? 0 : _macroRepetitions;
            _player.StartPlayback(repetitions);
            IsPlaying = true;
            StatusMessage = "▶ Воспроизведение началось...";
        }

        /// <summary>
        /// Остановить воспроизведение макроса
        /// </summary>
        public void StopPlayback()
        {
            if (!_isPlaying)
                return;

            _player.StopPlayback();
            IsPlaying = false;
            StatusMessage = "⏹ Воспроизведение остановлено";
        }

        /// <summary>
        /// Сохранить текущий макрос в файл (в папку SaveFolderPath)
        /// </summary>
        public bool SaveMacroToFile(string fileName)
        {
            try
            {
                var folder = string.IsNullOrWhiteSpace(SaveFolderPath) ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) : SaveFolderPath;
                var path = System.IO.Path.Combine(folder, fileName);
                return _player.SaveToFile(path);
            }
            catch (Exception ex)
            {
                StatusMessage = $"✗ Ошибка сохранения: {ex.Message}";
                return false;
            }
        }

        /// <summary>
        /// Загрузить макрос из файла
        /// </summary>
        public bool LoadMacroFromFile(string filePath)
        {
            try
            {
                var ok = _player.LoadFromFile(filePath);
                if (ok)
                {
                    StatusMessage = $"✓ Макрос загружен: {filePath}";
                    RecordedActionsCount = _recorder.RecordedActions.Count;
                }
                return ok;
            }
            catch (Exception ex)
            {
                StatusMessage = $"✗ Ошибка загрузки: {ex.Message}";
                return false;
            }
        }

        /// <summary>
        /// Установить камеру Roblox (жесткая последовательность для выравнивания)
        /// </summary>
        public void SetupRobloxCamera()
        {
            StatusMessage = "⚙️ Настройка камеры...";

            Task.Run(() =>
            {
                try
                {
                    // Устанавливаем курсор в центр экрана и сбрасываем вращение камеры
                    InputSimulator.SetCursorPosition(960, 540);
                    System.Threading.Thread.Sleep(150);

                    InputSimulator.RightMouseDown();
                    System.Threading.Thread.Sleep(300);
                    InputSimulator.SetCursorPosition(960, 540);
                    System.Threading.Thread.Sleep(300);
                    InputSimulator.RightMouseUp();
                    System.Threading.Thread.Sleep(200);

                    // Последовательность нажатий для приведения камеры в фиксированное положение
                    for (int i = 0; i < 3; i++)
                    {
                        InputSimulator.KeyClick(InputSimulator.VirtualKeys.VK_O);
                        System.Threading.Thread.Sleep(120);
                    }

                    for (int i = 0; i < 2; i++)
                    {
                        InputSimulator.KeyClick(InputSimulator.VirtualKeys.VK_I);
                        System.Threading.Thread.Sleep(120);
                    }

                    InputSimulator.SetCursorPosition(960, 540);
                    System.Threading.Thread.Sleep(150);

                    StatusMessage = "✓ Камера установлена в стандартное положение";
                }
                catch (Exception ex)
                {
                    StatusMessage = $"✗ Ошибка при настройке камеры: {ex.Message}";
                }
            });
        }

        /// <summary>
        /// Получить название игрового режима
        /// </summary>
        private string GetGameModeName(GameMode mode)
        {
            return mode switch
            {
                GameMode.StoryMode => "Story Mode",
                GameMode.LegendStage => "Legends Stage",
                GameMode.InfinityMode => "Infinity Mode",
                GameMode.Raid => "Raid",
                GameMode.Challenge => "Challenge",
                _ => "Unknown"
            };
        }

        /// <summary>
        /// Обработчик события записи нового действия
        /// </summary>
        private void OnActionRecorded(MacroAction action)
        {
            RecordedActionsCount = _recorder.RecordedActions.Count;
            UpdateRecordingDuration();
        }

        /// <summary>
        /// Обновить текущее время записи
        /// </summary>
        private void UpdateRecordingDuration()
        {
            if (_recordingStartTime.HasValue)
            {
                _recordingDuration = DateTime.UtcNow - _recordingStartTime.Value;
                RecordingTimeDisplay = _recordingDuration.ToString(@"hh\:mm\:ss");
            }
            else
            {
                RecordingTimeDisplay = "00:00:00";
            }
        }

        /// <summary>
        /// Обработчик изменения статуса воспроизведения
        /// </summary>
        private void OnPlayerStatusChanged(string status)
        {
            StatusMessage = status;
        }

        /// <summary>
        /// Обработчик завершения воспроизведения
        /// </summary>
        private void OnPlaybackCompleted()
        {
            IsPlaying = false;
            StatusMessage = "✓ Воспроизведение завершено";
        }

        /// <summary>
        /// Обработчик ошибки воспроизведения
        /// </summary>
        private void OnPlaybackError(string error)
        {
            IsPlaying = false;
            StatusMessage = $"✗ {error}";
        }

        /// <summary>
        /// Вызвать событие PropertyChanged
        /// </summary>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// Модель для отображения игрового режима в UI
    /// </summary>
    public class GameModeItem
    {
        public GameMode Mode { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
    }
}
