using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace RobloxMacro.Services
{
    /// <summary>
    /// Менеджер глобальных горячих клавиш (работает даже когда приложение не в фокусе)
    /// </summary>
    public class HotkeyManager : IDisposable
    {
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const uint MOD_NONE = 0x0000;
        private const uint MOD_ALT = 0x0001;
        private const uint MOD_CONTROL = 0x0002;
        private const uint MOD_SHIFT = 0x0004;
        private const uint MOD_WIN = 0x0008;

        private Dictionary<int, Action> _hotkeyActions = new();
        // Fallback mapping from virtual-key to action when RegisterHotKey fails
        private Dictionary<uint, Action> _vkActions = new();
        private IntPtr _windowHandle;
        private int _hotkeyIdCounter = 0;

        // Low-level keyboard hook for fallback
        private IntPtr _hookHandle = IntPtr.Zero;
        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        private LowLevelKeyboardProc _procDelegate;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;

        /// <summary>
        /// Инициализировать менеджер горячих клавиш с указанным окном
        /// </summary>
        public HotkeyManager(IntPtr windowHandle)
        {
            _windowHandle = windowHandle;
        }

        /// <summary>
        /// Регистрировать горячую клавишу
        /// </summary>
        /// <param name="keyCode">Виртуальный код клавиши (VK_*)</param>
        /// <param name="action">Действие, которое выполнить при нажатии</param>
        /// <param name="modifiers">Модификаторы (Alt, Ctrl, Shift, Win)</param>
        /// <returns>ID зарегистрированной горячей клавиши</returns>
        public int RegisterHotkey(uint keyCode, Action action, HotkeyModifier modifiers = HotkeyModifier.None)
        {
            int hotkeyId = ++_hotkeyIdCounter;
            uint modifierFlags = GetModifierFlags(modifiers);

            if (RegisterHotKey(_windowHandle, hotkeyId, modifierFlags, keyCode))
            {
                _hotkeyActions[hotkeyId] = action;
                return hotkeyId;
            }

            // Попытка fallback: добавить в vk->action и установить хук клавиатуры
            _vkActions[keyCode] = action;
            EnsureKeyboardHook();
            return hotkeyId; // возвращаем id, хотя системная регистрация не выполнена
        }

        /// <summary>
        /// Отменить регистрацию горячей клавиши
        /// </summary>
        public bool UnregisterHotkey(int hotkeyId)
        {
            if (_hotkeyActions.ContainsKey(hotkeyId))
            {
                UnregisterHotKey(_windowHandle, hotkeyId);
                _hotkeyActions.Remove(hotkeyId);
                return true;
            }

            // Также удалить из fallback-словаря, если там есть
            // Ищем соответствующую пару по значению действия
            foreach (var kv in new Dictionary<uint, Action>(_vkActions))
            {
                // Не гарантируем уникальность, но удалим первое совпадение
                if (kv.Value == _hotkeyActions.GetValueOrDefault(hotkeyId))
                {
                    _vkActions.Remove(kv.Key);
                    break;
                }
            }

            return false;
        }

        /// <summary>
        /// Обработать сообщение WM_HOTKEY от окна
        /// </summary>
        public void ProcessHotkey(int hotkeyId)
        {
            if (_hotkeyActions.TryGetValue(hotkeyId, out var action))
            {
                try
                {
                    action?.Invoke();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error executing hotkey action: {ex.Message}");
                }
            }
        }

        private void EnsureKeyboardHook()
        {
            if (_hookHandle != IntPtr.Zero)
                return;

            _procDelegate = HookCallback;
            IntPtr hMod = GetModuleHandle(System.Diagnostics.Process.GetCurrentProcess().MainModule.ModuleName);
            _hookHandle = SetWindowsHookEx(WH_KEYBOARD_LL, _procDelegate, hMod, 0);
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam.ToInt32() == WM_KEYDOWN)
            {
                try
                {
                    uint vk = (uint)Marshal.ReadInt32(lParam);
                    if (_vkActions.TryGetValue(vk, out var act))
                    {
                        act?.Invoke();
                    }
                }
                catch { }
            }

            return CallNextHookEx(_hookHandle, nCode, wParam, lParam);
        }

        /// <summary>
        /// Конвертировать флаги модификаторов
        /// </summary>
        private uint GetModifierFlags(HotkeyModifier modifiers)
        {
            uint flags = MOD_NONE;

            if ((modifiers & HotkeyModifier.Alt) != 0)
                flags |= MOD_ALT;
            if ((modifiers & HotkeyModifier.Ctrl) != 0)
                flags |= MOD_CONTROL;
            if ((modifiers & HotkeyModifier.Shift) != 0)
                flags |= MOD_SHIFT;
            if ((modifiers & HotkeyModifier.Win) != 0)
                flags |= MOD_WIN;

            return flags;
        }

        /// <summary>
        /// Освободить все зарегистрированные горячие клавиши
        /// </summary>
        public void Dispose()
        {
            foreach (var hotkeyId in new List<int>(_hotkeyActions.Keys))
            {
                UnregisterHotkey(hotkeyId);
            }
            _hotkeyActions.Clear();
            _vkActions.Clear();

            if (_hookHandle != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_hookHandle);
                _hookHandle = IntPtr.Zero;
            }
        }

        /// <summary>
        /// Отменить регистрацию всех горячих клавиш
        /// </summary>
        public void UnregisterAll()
        {
            foreach (var hotkeyId in new List<int>(_hotkeyActions.Keys))
            {
                UnregisterHotkey(hotkeyId);
            }
        }
    }

    /// <summary>
    /// Модификаторы для горячих клавиш
    /// </summary>
    [Flags]
    public enum HotkeyModifier
    {
        None = 0,
        Alt = 1,
        Ctrl = 2,
        Shift = 4,
        Win = 8
    }
}
