namespace RobloxMacro.Models
{
    /// <summary>
    /// Тип действия в макросе
    /// </summary>
    public enum ActionType
    {
        MouseMove,
        MouseClick,
        MouseDown,
        MouseUp,
        KeyDown,
        KeyUp,
        Wait
    }

    /// <summary>
    /// Представляет одно действие в записанном макросе
    /// </summary>
    public class MacroAction
    {
        /// <summary>
        /// Тип действия (движение мыши, клик, нажатие клавиши и т.д.)
        /// </summary>
        public ActionType ActionType { get; set; }

        /// <summary>
        /// X-координата мыши (для MouseMove и MouseClick)
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// Y-координата мыши (для MouseMove и MouseClick)
        /// </summary>
        public int Y { get; set; }

        /// <summary>
        /// Виртуальный код клавиши (для KeyDown и KeyUp)
        /// </summary>
        public int KeyCode { get; set; }

        /// <summary>
        /// Кнопка мыши: 1 = левая, 2 = правая, 3 = средняя
        /// </summary>
        public int MouseButton { get; set; } = 1;

        /// <summary>
        /// Время задержки перед выполнением действия (в миллисекундах)
        /// </summary>
        public int Delay { get; set; }

        /// <summary>
        /// Временная метка записи для вычисления интервалов между действиями
        /// </summary>
        public long Timestamp { get; set; }

        public MacroAction()
        {
        }

        public MacroAction(ActionType actionType, int delay = 0)
        {
            ActionType = actionType;
            Delay = delay;
            Timestamp = System.Diagnostics.Stopwatch.GetTimestamp();
        }

        public MacroAction(ActionType actionType, int x, int y, int delay = 0)
        {
            ActionType = actionType;
            X = x;
            Y = y;
            Delay = delay;
            Timestamp = System.Diagnostics.Stopwatch.GetTimestamp();
        }

        public MacroAction(ActionType actionType, int keyCode, bool isKey, int delay = 0)
        {
            ActionType = actionType;
            KeyCode = keyCode;
            Delay = delay;
            Timestamp = System.Diagnostics.Stopwatch.GetTimestamp();
        }

        public override string ToString()
        {
            return ActionType switch
            {
                ActionType.MouseMove => $"MouseMove({X}, {Y}) +{Delay}ms",
                ActionType.MouseClick => $"MouseClick({X}, {Y}) +{Delay}ms",
                ActionType.KeyDown => $"KeyDown({KeyCode}) +{Delay}ms",
                ActionType.KeyUp => $"KeyUp({KeyCode}) +{Delay}ms",
                ActionType.Wait => $"Wait({Delay}ms)",
                _ => $"{ActionType} +{Delay}ms"
            };
        }
    }
}
