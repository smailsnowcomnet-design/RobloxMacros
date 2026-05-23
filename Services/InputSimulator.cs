using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace RobloxMacro.Services
{
    /// <summary>
    /// Сервис для симуляции пользовательского ввода (мышь, клавиатура)
    /// Использует Windows API для обхода стандартных ограничений
    /// </summary>
    public static class InputSimulator
    {
        // ========== Windows API Declarations ==========

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int x, int y);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct INPUT
        {
            public InputType type;
            public InputUnion U;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct InputUnion
        {
            [FieldOffset(0)]
            public MOUSEINPUT mi;

            [FieldOffset(0)]
            public KEYBDINPUT ki;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public MOUSEEVENTF dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public KEYEVENTF dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        private enum InputType : uint
        {
            MOUSE = 0,
            KEYBOARD = 1,
            HARDWARE = 2
        }

        [Flags]
        private enum MOUSEEVENTF : uint
        {
            MOVE = 0x0001,
            LEFTDOWN = 0x0002,
            LEFTUP = 0x0004,
            RIGHTDOWN = 0x0008,
            RIGHTUP = 0x0010,
            MIDDLEDOWN = 0x0020,
            MIDDLEUP = 0x0040,
            XDOWN = 0x0080,
            XUP = 0x0100,
            WHEEL = 0x0800,
            HWHEEL = 0x1000,
            MOVE_NOCOALESCE = 0x2000,
            VIRTUALDESK = 0x4000,
            ABSOLUTE = 0x8000
        }

        [Flags]
        private enum KEYEVENTF : uint
        {
            KEYDOWN = 0x0000,
            EXTENDEDKEY = 0x0001,
            KEYUP = 0x0002,
            UNICODE = 0x0004,
            SCANCODE = 0x0008
        }

        // ========== Public Methods ==========

        /// <summary>
        /// Получает текущие координаты курсора мыши
        /// </summary>
        public static void GetCursorPosition(out int x, out int y)
        {
            GetCursorPos(out POINT point);
            x = point.X;
            y = point.Y;
        }

        /// <summary>
        /// Устанавливает позицию курсора мыши
        /// </summary>
        public static void SetCursorPosition(int x, int y)
        {
            SetCursorPos(x, y);
        }

        /// <summary>
        /// Перемещает курсор на указанные координаты
        /// </summary>
        public static void MoveMouse(int x, int y)
        {
            SetCursorPos(x, y);
            Thread.Sleep(10); // Небольшая задержка для реалистичности
        }

        /// <summary>
        /// Имитирует клик левой кнопки мыши
        /// </summary>
        public static void LeftClick()
        {
            INPUT input1 = new INPUT();
            input1.type = InputType.MOUSE;
            input1.U.mi.dwFlags = MOUSEEVENTF.LEFTDOWN;

            INPUT input2 = new INPUT();
            input2.type = InputType.MOUSE;
            input2.U.mi.dwFlags = MOUSEEVENTF.LEFTUP;

            SendInput(1, new INPUT[] { input1 }, Marshal.SizeOf<INPUT>());
            Thread.Sleep(50);
            SendInput(1, new INPUT[] { input2 }, Marshal.SizeOf<INPUT>());
        }

        /// <summary>
        /// Имитирует клик левой кнопки мыши в указанной позиции
        /// </summary>
        public static void LeftClickAt(int x, int y)
        {
            SetCursorPos(x, y);
            Thread.Sleep(10);
            LeftClick();
        }

        /// <summary>
        /// Имитирует нажатие левой кнопки мыши (без отпускания)
        /// </summary>
        public static void LeftMouseDown()
        {
            INPUT input = new INPUT();
            input.type = InputType.MOUSE;
            input.U.mi.dwFlags = MOUSEEVENTF.LEFTDOWN;
            SendInput(1, new INPUT[] { input }, Marshal.SizeOf<INPUT>());
        }

        /// <summary>
        /// Имитирует отпускание левой кнопки мыши
        /// </summary>
        public static void LeftMouseUp()
        {
            INPUT input = new INPUT();
            input.type = InputType.MOUSE;
            input.U.mi.dwFlags = MOUSEEVENTF.LEFTUP;
            SendInput(1, new INPUT[] { input }, Marshal.SizeOf<INPUT>());
        }

        /// <summary>
        /// Имитирует клик правой кнопки мыши
        /// </summary>
        public static void RightClick()
        {
            INPUT input1 = new INPUT();
            input1.type = InputType.MOUSE;
            input1.U.mi.dwFlags = MOUSEEVENTF.RIGHTDOWN;

            INPUT input2 = new INPUT();
            input2.type = InputType.MOUSE;
            input2.U.mi.dwFlags = MOUSEEVENTF.RIGHTUP;

            SendInput(1, new INPUT[] { input1 }, Marshal.SizeOf<INPUT>());
            Thread.Sleep(50);
            SendInput(1, new INPUT[] { input2 }, Marshal.SizeOf<INPUT>());
        }

        /// <summary>
        /// Имитирует нажатие правой кнопки мыши (без отпускания)
        /// </summary>
        public static void RightMouseDown()
        {
            INPUT input = new INPUT();
            input.type = InputType.MOUSE;
            input.U.mi.dwFlags = MOUSEEVENTF.RIGHTDOWN;
            SendInput(1, new INPUT[] { input }, Marshal.SizeOf<INPUT>());
        }

        /// <summary>
        /// Имитирует отпускание правой кнопки мыши
        /// </summary>
        public static void RightMouseUp()
        {
            INPUT input = new INPUT();
            input.type = InputType.MOUSE;
            input.U.mi.dwFlags = MOUSEEVENTF.RIGHTUP;
            SendInput(1, new INPUT[] { input }, Marshal.SizeOf<INPUT>());
        }

        /// <summary>
        /// Имитирует нажатие клавиши клавиатуры
        /// </summary>
        /// <param name="keyCode">Виртуальный код клавиши (VK_*)</param>
        public static void KeyDown(int keyCode)
        {
            INPUT input = new INPUT();
            input.type = InputType.KEYBOARD;
            input.U.ki.wVk = (ushort)keyCode;
            input.U.ki.dwFlags = KEYEVENTF.KEYDOWN;
            SendInput(1, new INPUT[] { input }, Marshal.SizeOf<INPUT>());
        }

        /// <summary>
        /// Имитирует отпускание клавиши клавиатуры
        /// </summary>
        /// <param name="keyCode">Виртуальный код клавиши (VK_*)</param>
        public static void KeyUp(int keyCode)
        {
            INPUT input = new INPUT();
            input.type = InputType.KEYBOARD;
            input.U.ki.wVk = (ushort)keyCode;
            input.U.ki.dwFlags = KEYEVENTF.KEYUP;
            SendInput(1, new INPUT[] { input }, Marshal.SizeOf<INPUT>());
        }

        /// <summary>
        /// Имитирует нажатие и отпускание клавиши (полный клик)
        /// </summary>
        public static void KeyClick(int keyCode)
        {
            KeyDown(keyCode);
            Thread.Sleep(50);
            KeyUp(keyCode);
        }

        /// <summary>
        /// Имитирует ввод комбинации клавиш (например, Ctrl+A)
        /// </summary>
        public static void KeyCombination(params int[] keyCodes)
        {
            // Нажимаем все клавиши
            foreach (int code in keyCodes)
            {
                KeyDown(code);
            }

            Thread.Sleep(50);

            // Отпускаем все клавиши в обратном порядке
            for (int i = keyCodes.Length - 1; i >= 0; i--)
            {
                KeyUp(keyCodes[i]);
            }
        }

        /// <summary>
        /// Вводит текст через клавиатуру (посимвольно)
        /// </summary>
        public static void TypeText(string text)
        {
            foreach (char c in text)
            {
                INPUT input = new INPUT();
                input.type = InputType.KEYBOARD;
                input.U.ki.wVk = 0;
                input.U.ki.wScan = (ushort)c;
                input.U.ki.dwFlags = KEYEVENTF.UNICODE;
                SendInput(1, new INPUT[] { input }, Marshal.SizeOf<INPUT>());

                input.U.ki.dwFlags = KEYEVENTF.UNICODE | KEYEVENTF.KEYUP;
                SendInput(1, new INPUT[] { input }, Marshal.SizeOf<INPUT>());

                Thread.Sleep(50);
            }
        }

        /// <summary>
        /// Стандартные виртуальные коды клавиш
        /// </summary>
        public static class VirtualKeys
        {
            public const int VK_LBUTTON = 0x01;
            public const int VK_RBUTTON = 0x02;
            public const int VK_CANCEL = 0x03;
            public const int VK_MBUTTON = 0x04;
            public const int VK_BACK = 0x08;
            public const int VK_TAB = 0x09;
            public const int VK_CLEAR = 0x0C;
            public const int VK_RETURN = 0x0D;
            public const int VK_SHIFT = 0x10;
            public const int VK_CONTROL = 0x11;
            public const int VK_MENU = 0x12;
            public const int VK_PAUSE = 0x13;
            public const int VK_CAPITAL = 0x14;
            public const int VK_ESCAPE = 0x1B;
            public const int VK_SPACE = 0x20;
            public const int VK_PRIOR = 0x21;
            public const int VK_NEXT = 0x22;
            public const int VK_END = 0x23;
            public const int VK_HOME = 0x24;
            public const int VK_LEFT = 0x25;
            public const int VK_UP = 0x26;
            public const int VK_RIGHT = 0x27;
            public const int VK_DOWN = 0x28;
            public const int VK_SELECT = 0x29;
            public const int VK_PRINT = 0x2A;
            public const int VK_EXECUTE = 0x2B;
            public const int VK_SNAPSHOT = 0x2C;
            public const int VK_INSERT = 0x2D;
            public const int VK_DELETE = 0x2E;
            public const int VK_HELP = 0x2F;
            public const int VK_0 = 0x30;
            public const int VK_1 = 0x31;
            public const int VK_2 = 0x32;
            public const int VK_3 = 0x33;
            public const int VK_4 = 0x34;
            public const int VK_5 = 0x35;
            public const int VK_6 = 0x36;
            public const int VK_7 = 0x37;
            public const int VK_8 = 0x38;
            public const int VK_9 = 0x39;
            public const int VK_A = 0x41;
            public const int VK_B = 0x42;
            public const int VK_C = 0x43;
            public const int VK_D = 0x44;
            public const int VK_E = 0x45;
            public const int VK_F = 0x46;
            public const int VK_G = 0x47;
            public const int VK_H = 0x48;
            public const int VK_I = 0x49;
            public const int VK_J = 0x4A;
            public const int VK_K = 0x4B;
            public const int VK_L = 0x4C;
            public const int VK_M = 0x4D;
            public const int VK_N = 0x4E;
            public const int VK_O = 0x4F;
            public const int VK_P = 0x50;
            public const int VK_Q = 0x51;
            public const int VK_R = 0x52;
            public const int VK_S = 0x53;
            public const int VK_T = 0x54;
            public const int VK_U = 0x55;
            public const int VK_V = 0x56;
            public const int VK_W = 0x57;
            public const int VK_X = 0x58;
            public const int VK_Y = 0x59;
            public const int VK_Z = 0x5A;
            public const int VK_F1 = 0x70;
            public const int VK_F2 = 0x71;
            public const int VK_F3 = 0x72;
            public const int VK_F4 = 0x73;
            public const int VK_F5 = 0x74;
            public const int VK_F6 = 0x75;
            public const int VK_F7 = 0x76;
            public const int VK_F8 = 0x77;
            public const int VK_F9 = 0x78;
            public const int VK_F10 = 0x79;
            public const int VK_F11 = 0x7A;
            public const int VK_F12 = 0x7B;
            public const int VK_NUMLOCK = 0x90;
            public const int VK_SCROLL = 0x91;
            public const int VK_LSHIFT = 0xA0;
            public const int VK_RSHIFT = 0xA1;
            public const int VK_LCONTROL = 0xA2;
            public const int VK_RCONTROL = 0xA3;
            public const int VK_LMENU = 0xA4;
            public const int VK_RMENU = 0xA5;
        }
    }
}
