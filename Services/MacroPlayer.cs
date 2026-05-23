using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using RobloxMacro.Models;

namespace RobloxMacro.Services
{
    /// <summary>
    /// Сервис для воспроизведения записанных макросов
    /// </summary>
    public class MacroPlayer
    {
        private List<MacroAction> _macroActions = new();
        private bool _isPlaying = false;
        private bool _shouldStop = false;
        private CancellationTokenSource _cancellationTokenSource;
        private Task _playbackTask;

        /// <summary>
        /// Событие для уведомления о статусе воспроизведения
        /// </summary>
        public event Action<string> StatusChanged;

        /// <summary>
        /// Событие для уведомления о завершении воспроизведения
        /// </summary>
        public event Action PlaybackCompleted;

        /// <summary>
        /// Событие при ошибке во время воспроизведения
        /// </summary>
        public event Action<string> PlaybackError;

        /// <summary>
        /// Проверить, идет ли воспроизведение
        /// </summary>
        public bool IsPlaying => _isPlaying;

        /// <summary>
        /// Установить макрос для воспроизведения
        /// </summary>
        public void SetMacro(IReadOnlyList<MacroAction> actions)
        {
            if (_isPlaying)
                return;

            _macroActions = new List<MacroAction>(actions);
            StatusChanged?.Invoke($"Макрос загружен: {_macroActions.Count} действий");
        }

        /// <summary>
        /// Запустить воспроизведение макроса
        /// </summary>
        /// <param name="repetitions">Количество повторов (0 = бесконечно)</param>
        public void StartPlayback(int repetitions = 1)
        {
            if (_isPlaying || _macroActions.Count == 0)
                return;

            _isPlaying = true;
            _shouldStop = false;
            _cancellationTokenSource = new CancellationTokenSource();

            _playbackTask = Task.Run(() => PlaybackLoop(repetitions, _cancellationTokenSource.Token));
        }

        /// <summary>
        /// Остановить воспроизведение макроса
        /// </summary>
        public void StopPlayback()
        {
            _shouldStop = true;
            _cancellationTokenSource?.Cancel();

            if (_playbackTask != null)
            {
                try
                {
                    _playbackTask.Wait(2000); // Ждем максимум 2 секунды
                }
                catch { }
            }

            _isPlaying = false;
            StatusChanged?.Invoke("Воспроизведение остановлено");
        }

        /// <summary>
        /// Основной цикл воспроизведения
        /// </summary>
        private void PlaybackLoop(int repetitions, CancellationToken cancellationToken)
        {
            try
            {
                int currentRepetition = 0;
                bool infiniteLoop = repetitions == 0;

                while ((infiniteLoop || currentRepetition < repetitions) && !_shouldStop)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    currentRepetition++;
                    StatusChanged?.Invoke($"Повтор {currentRepetition}/{(infiniteLoop ? "∞" : repetitions.ToString())}");

                    // Выполнить все действия макроса
                    ExecuteMacroActions(cancellationToken);

                    if (cancellationToken.IsCancellationRequested)
                        break;

                    // Небольшая пауза между повторами
                    Thread.Sleep(200);
                }

                if (!cancellationToken.IsCancellationRequested)
                {
                    StatusChanged?.Invoke("Воспроизведение завершено");
                    PlaybackCompleted?.Invoke();
                }
            }
            catch (OperationCanceledException)
            {
                // Нормальное завершение по отмене
            }
            catch (Exception ex)
            {
                PlaybackError?.Invoke($"Ошибка во время воспроизведения: {ex.Message}");
            }
            finally
            {
                _isPlaying = false;
            }
        }

        /// <summary>
        /// Выполнить все действия макроса
        /// </summary>
        private void ExecuteMacroActions(CancellationToken cancellationToken)
        {
            long lastTimestamp = _macroActions[0].Timestamp;

            foreach (var action in _macroActions)
            {
                if (cancellationToken.IsCancellationRequested || _shouldStop)
                    break;

                // Вычислить интервал между действиями
                long timeDifference = action.Timestamp - lastTimestamp;
                if (timeDifference > 0 && timeDifference < 5000) // Максимум 5 секунд задержки
                {
                    Thread.Sleep((int)timeDifference);
                }
                else if (action.Delay > 0)
                {
                    Thread.Sleep(action.Delay);
                }

                // Выполнить действие
                try
                {
                    ExecuteAction(action);
                }
                catch (Exception ex)
                {
                    PlaybackError?.Invoke($"Ошибка при выполнении действия: {ex.Message}");
                }

                lastTimestamp = action.Timestamp;
            }
        }

        /// <summary>
        /// Выполнить одно действие макроса
        /// </summary>
        private void ExecuteAction(MacroAction action)
        {
            switch (action.ActionType)
            {
                case ActionType.MouseMove:
                    InputSimulator.MoveMouse(action.X, action.Y);
                    break;

                case ActionType.MouseClick:
                    if (action.MouseButton == 1)
                        InputSimulator.LeftClickAt(action.X, action.Y);
                    else if (action.MouseButton == 2)
                    {
                        InputSimulator.SetCursorPosition(action.X, action.Y);
                        Thread.Sleep(10);
                        InputSimulator.RightClick();
                    }
                    break;

                case ActionType.MouseDown:
                    if (action.MouseButton == 1)
                        InputSimulator.LeftMouseDown();
                    else if (action.MouseButton == 2)
                        InputSimulator.RightMouseDown();
                    break;

                case ActionType.MouseUp:
                    if (action.MouseButton == 1)
                        InputSimulator.LeftMouseUp();
                    else if (action.MouseButton == 2)
                        InputSimulator.RightMouseUp();
                    break;

                case ActionType.KeyDown:
                    InputSimulator.KeyDown(action.KeyCode);
                    break;

                case ActionType.KeyUp:
                    InputSimulator.KeyUp(action.KeyCode);
                    break;

                case ActionType.Wait:
                    Thread.Sleep(action.Delay);
                    break;
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
                    SetMacro(actions);
                    return true;
                }
            }
            catch (Exception ex)
            {
                PlaybackError?.Invoke($"Ошибка загрузки макроса: {ex.Message}");
            }

            return false;
        }

        /// <summary>
        /// Сохранить текущий макрос в файл (JSON)
        /// </summary>
        public bool SaveToFile(string filePath)
        {
            try
            {
                var json = System.Text.Json.JsonSerializer.Serialize(_macroActions);
                System.IO.File.WriteAllText(filePath, json);
                StatusChanged?.Invoke($"Макрос сохранен: {_macroActions.Count} действий");
                return true;
            }
            catch (Exception ex)
            {
                PlaybackError?.Invoke($"Ошибка сохранения макроса: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Получить информацию о загруженном макросе
        /// </summary>
        public string GetMacroInfo()
        {
            if (_macroActions.Count == 0)
                return "Макрос не загружен";

            return $"Действий: {_macroActions.Count}";
        }
    }
}
