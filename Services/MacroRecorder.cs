using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using RobloxMacro.Models;

namespace RobloxMacro.Services
{
    /// <summary>
    /// Сервис для записи движений мыши и нажатий клавиш в макрос
    /// </summary>
    public class MacroRecorder
    {
        private List<MacroAction> _recordedActions = new();
        private bool _isRecording = false;
        private Stopwatch _recordingTimer;
        private Thread _recordingThread;
        private int _lastMouseX = 0;
        private int _lastMouseY = 0;

        /// <summary>
        /// Событие для уведомления о записи нового действия
        /// </summary>
        public event Action<MacroAction> ActionRecorded;

        /// <summary>
        /// Получить список всех записанных действий
        /// </summary>
        public IReadOnlyList<MacroAction> RecordedActions => _recordedActions.AsReadOnly();

        /// <summary>
        /// Проверить, идет ли запись
        /// </summary>
        public bool IsRecording => _isRecording;

        /// <summary>
        /// Начать запись макроса
        /// </summary>
        public void StartRecording()
        {
            if (_isRecording)
                return;

            _recordedActions.Clear();
            _isRecording = true;
            _recordingTimer = Stopwatch.StartNew();

            // Получить начальную позицию мыши
            InputSimulator.GetCursorPosition(out _lastMouseX, out _lastMouseY);

            // Запустить фоновый поток для мониторинга ввода
            _recordingThread = new Thread(MonitorInputLoop)
            {
                IsBackground = true
            };
            _recordingThread.Start();
        }

        /// <summary>
        /// Остановить запись макроса
        /// </summary>
        public void StopRecording()
        {
            _isRecording = false;
            _recordingTimer?.Stop();

            // Дождемся завершения потока мониторинга
            if (_recordingThread != null && _recordingThread.IsAlive)
            {
                _recordingThread.Join(1000);
            }
        }

        /// <summary>
        /// Очистить записанные действия
        /// </summary>
        public void Clear()
        {
            StopRecording();
            _recordedActions.Clear();
        }

        /// <summary>
        /// Получить временную длительность записи
        /// </summary>
        public TimeSpan GetRecordingDuration()
        {
            if (_recordingTimer == null)
                return TimeSpan.Zero;

            return _recordingTimer.Elapsed;
        }

        /// <summary>
        /// Мониторить ввод пользователя и записывать действия
        /// </summary>
        private void MonitorInputLoop()
        {
            try
            {
                while (_isRecording)
                {
                    // Получить текущую позицию мыши
                    InputSimulator.GetCursorPosition(out int currentX, out int currentY);

                    // Если мышь переместилась, записать движение
                    if (currentX != _lastMouseX || currentY != _lastMouseY)
                    {
                        var action = new MacroAction(ActionType.MouseMove, currentX, currentY);
                        action.Delay = (int)_recordingTimer.ElapsedMilliseconds;
                        _recordedActions.Add(action);
                        ActionRecorded?.Invoke(action);

                        _lastMouseX = currentX;
                        _lastMouseY = currentY;
                    }

                    Thread.Sleep(20); // Проверяем каждые 20ms
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in MonitorInputLoop: {ex.Message}");
            }
        }

        /// <summary>
        /// Записать действие мыши (клик)
        /// </summary>
        public void RecordMouseClick(int x, int y, int mouseButton = 1)
        {
            if (!_isRecording)
                return;

            var action = new MacroAction(ActionType.MouseClick, x, y)
            {
                MouseButton = mouseButton,
                Delay = (int)_recordingTimer.ElapsedMilliseconds
            };
            _recordedActions.Add(action);
            ActionRecorded?.Invoke(action);
        }

        /// <summary>
        /// Записать нажатие клавиши
        /// </summary>
        public void RecordKeyDown(int keyCode)
        {
            if (!_isRecording)
                return;

            var action = new MacroAction(ActionType.KeyDown, keyCode, true)
            {
                Delay = (int)_recordingTimer.ElapsedMilliseconds
            };
            _recordedActions.Add(action);
            ActionRecorded?.Invoke(action);
        }

        /// <summary>
        /// Записать отпускание клавиши
        /// </summary>
        public void RecordKeyUp(int keyCode)
        {
            if (!_isRecording)
                return;

            var action = new MacroAction(ActionType.KeyUp, keyCode, true)
            {
                Delay = (int)_recordingTimer.ElapsedMilliseconds
            };
            _recordedActions.Add(action);
            ActionRecorded?.Invoke(action);
        }

        /// <summary>
        /// Записать паузу/задержку
        /// </summary>
        public void RecordWait(int delayMs)
        {
            if (!_isRecording)
                return;

            var action = new MacroAction(ActionType.Wait, delayMs)
            {
                Delay = (int)_recordingTimer.ElapsedMilliseconds
            };
            _recordedActions.Add(action);
            ActionRecorded?.Invoke(action);
        }

        /// <summary>
        /// Сохранить записанный макрос в файл JSON
        /// </summary>
        public void SaveToFile(string filePath)
        {
            try
            {
                var json = System.Text.Json.JsonSerializer.Serialize(_recordedActions, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                System.IO.File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving macro: {ex.Message}");
            }
        }

        /// <summary>
        /// Загрузить макрос из файла JSON
        /// </summary>
        public bool LoadFromFile(string filePath)
        {
            try
            {
                if (!System.IO.File.Exists(filePath))
                    return false;

                var json = System.IO.File.ReadAllText(filePath);
                var actions = System.Text.Json.JsonSerializer.Deserialize<List<MacroAction>>(json);

                if (actions != null)
                {
                    _recordedActions = actions;
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading macro: {ex.Message}");
            }

            return false;
        }
    }
}
