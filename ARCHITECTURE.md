# 🏗️ Архитектура приложения Roblox Macro

## Обзор системы

```
┌─────────────────────────────────────────────────────────────┐
│                    WPF UI (XAML)                            │
│              MainWindow.xaml + Code-Behind                 │
│  ┌──────────────────────────────────────────────────────┐  │
│  │ Status Bar | Game Mode Selector | Control Buttons   │  │
│  └──────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
                           ↓
┌─────────────────────────────────────────────────────────────┐
│                  ViewModel (MVVM)                           │
│                MainViewModel.cs                             │
│  • Управление состоянием приложения                         │
│  • Привязка данных к UI                                    │
│  • Обработка команд пользователя                            │
└─────────────────────────────────────────────────────────────┘
                           ↓
┌─────────────────────────────────────────────────────────────┐
│                Services Layer                               │
│  ┌─────────────────────────────────────────────────────┐   │
│  │ MacroRecorder    │ MacroPlayer    │ HotkeyManager  │   │
│  │ (Запись)         │ (Воспроизведение)  │ (F7/F8)    │   │
│  └─────────────────────────────────────────────────────┘   │
│  ┌─────────────────────────────────────────────────────┐   │
│  │ InputSimulator              │ MacroProfileManager    │   │
│  │ (Windows API, SendInput)    │ (Профили макросов)    │   │
│  └─────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
                           ↓
┌─────────────────────────────────────────────────────────────┐
│                  Models/Data                                │
│  • GameMode enum (StoryMode, LegendStage, ...)             │
│  • MacroAction (действие в макросе)                         │
│  • MacroProfile (профиль с мета-данными)                   │
└─────────────────────────────────────────────────────────────┘
                           ↓
┌─────────────────────────────────────────────────────────────┐
│           Windows API / System Level                        │
│  • SendInput (симуляция ввода)                              │
│  • RegisterHotKey (глобальные горячие клавиши)              │
│  • GetCursorPos (позиция мыши)                              │
└─────────────────────────────────────────────────────────────┘
```

## 📁 Структура файлов

```
Macros/
│
├── Models/
│   ├── GameMode.cs                    # Перечисление режимов
│   └── MacroAction.cs                 # Модель действия
│
├── Services/
│   ├── InputSimulator.cs               # Windows API обертка
│   ├── MacroRecorder.cs                # Запись макросов
│   ├── MacroPlayer.cs                  # Воспроизведение
│   ├── HotkeyManager.cs                # Горячие клавиши
│   └── MacroProfileManager.cs          # Управление профилями
│
├── ViewModels/
│   └── MainViewModel.cs                # MVVM ViewModel
│
├── MainWindow.xaml                      # UI интерфейс
├── MainWindow.xaml.cs                   # Code-Behind
├── App.xaml                             # Конфигурация приложения
├── App.xaml.cs                          # Логика приложения
│
├── RobloxMacro.csproj                   # Файл проекта
├── AssemblyInfo.cs                      # Метаданные сборки
│
├── README.md                            # Основная документация
├── QUICKSTART.md                        # Быстрый старт
├── API_REFERENCE.md                     # Справочник API
├── BUILD_GUIDE.md                       # Инструкции по сборке
└── .gitignore                           # Игнорирование файлов
```

## 🔄 Поток выполнения

### Запуск приложения

```
1. App.xaml.cs: OnStartup()
   ↓
2. MainWindow_Loaded()
   ├── Инициализировать ViewModel
   ├── Установить привязки данных (DataBinding)
   ├── Создать HwndSource для обработки горячих клавиш
   ├── Инициализировать HotkeyManager
   └── Зарегистрировать F7 и F8
   ↓
3. HwndHook() активируется при нажатии F7/F8
```

### Запись макроса

```
1. Пользователь нажимает "Начать запись"
   ↓
2. MainViewModel.StartRecording()
   ├── recorder.StartRecording()
   └── Запустить фоновый поток мониторинга
   ↓
3. MonitorInputLoop() в фоновом потоке
   ├── Каждые 20ms проверить позицию мыши
   ├── Фиксировать движения и клики
   └── Добавлять в _recordedActions
   ↓
4. Пользователь нажимает "Стоп запись"
   ├── StopRecording()
   ├── Остановить фоновый поток
   ├── Загрузить макрос в плеер
   └── Обновить UI (счетчик действий)
   ↓
5. Макрос готов к воспроизведению
```

### Воспроизведение макроса

```
1. Пользователь нажимает "Запуск повтора"
   ↓
2. MainViewModel.StartPlayback()
   ├── player.StartPlayback(repetitions)
   ├── Создать CancellationToken
   └── Запустить Task для PlaybackLoop()
   ↓
3. PlaybackLoop() в отдельном потоке
   ├── Для каждого повтора:
   │   ├── Вызвать ExecuteMacroActions()
   │   │   ├── Для каждого действия:
   │   │   │   ├── Вычислить задержку
   │   │   │   ├── Вызвать ExecuteAction()
   │   │   │   └── Выполнить (SendInput, KeyDown и т.д.)
   │   │   └── Пауза между действиями (Thread.Sleep)
   │   └── Обновить StatusMessage (повтор X/Y)
   ↓
4. Воспроизведение завершено
   ├── IsPlaying = false
   ├── Вызвать PlaybackCompleted()
   └── Обновить UI
```

### Горячие клавиши (F7, F8)

```
1. Пользователь нажимает F7 или F8
   ↓
2. Windows отправляет WM_HOTKEY сообщение
   ↓
3. HwndHook() перехватывает сообщение
   ├── Проверить hotkeyId
   ├── Вызвать ProcessHotkey(hotkeyId)
   └── Вызвать соответствующий обработчик
   ↓
4. TogglePlayback() (F7)
   └── Если IsPlaying → StopPlayback()
       Иначе → StartPlayback()
   ↓
   ИЛИ
   ↓
5. PanicStop() (F8)
   ├── StopRecording() (если идет запись)
   └── StopPlayback() (если идет воспроизведение)
```

## 🔐 Безопасность и потокобезопасность

### Использование многопоточности

```csharp
// MacroRecorder
private Thread _recordingThread;  // Фоновый мониторинг ввода

// MacroPlayer
private Task _playbackTask;       // Асинхронное воспроизведение
private CancellationTokenSource _cancellationTokenSource;  // Отмена
```

### Синхронизация между UI потоком и рабочими потоками

```csharp
// UI → Worker потокBin
_isRecording = true;              // Установить флаг из UI

// Worker поток → UI
Dispatcher.Invoke(() => {
    StatusMessage = "...";         // Обновить UI из рабочего потока
});

// Или через события
ActionRecorded?.Invoke(action);   // Событие в UI потоке
```

## 📊 Типы данных и структуры

### MacroAction

```csharp
public class MacroAction
{
    public ActionType ActionType { get; set; }  // MouseMove, KeyDown и т.д.
    public int X, Y { get; set; }               // Координаты
    public int KeyCode { get; set; }            // Код клавиши
    public int MouseButton { get; set; }        // 1=левая, 2=правая
    public int Delay { get; set; }              // Задержка (мс)
    public long Timestamp { get; set; }         // Временная метка
}
```

### Сохранение в JSON

```json
[
  {
    "actionType": "MouseMove",
    "x": 500,
    "y": 300,
    "keyCode": 0,
    "mouseButton": 0,
    "delay": 120,
    "timestamp": 12345678
  },
  {
    "actionType": "MouseClick",
    "x": 500,
    "y": 300,
    "keyCode": 0,
    "mouseButton": 1,
    "delay": 250,
    "timestamp": 12345778
  }
]
```

## 🎨 UI Компоненты

### Главное окно

```
┌─────────────────────────────────────────────────────────┐
│ Row 0: Header (Заголовок)                               │
├─────────────────────────────────────────────────────────┤
│ Row 1: Content (Основной контент)                       │
│ • Game Mode Selection (Выбор режима)                    │
│ • Recording/Playback Controls (Управление)              │
│ • Playback Settings (Параметры)                         │
│ • Additional Functions (Доп. функции)                   │
│ • Hotkey Info (Информация о горячих клавишах)          │
├─────────────────────────────────────────────────────────┤
│ Row 2: Status Bar (Информационная панель)               │
│ [Status Message] [Recording●] [Playing●]                │
├─────────────────────────────────────────────────────────┤
│ Row 3: Footer (Футер)                                   │
│ © 2026 Roblox Macro Suite | v1.0 | Made with ❤️       │
└─────────────────────────────────────────────────────────┘
```

### Стилизация

```xaml
<!-- Темная тема -->
Background="#1E1E1E"          <!-- Основной фон -->
CardBackground="#2D2D2D"      <!-- Фон карточек -->

<!-- Акцентные цвета -->
AccentBlue="#0078D4"          <!-- Основной акцент -->
AccentGreen="#27AE60"         <!-- Успех/Запись -->
AccentRed="#E74C3C"           <!-- Опасность/Стоп -->
AccentOrange="#E67E22"        <!-- Внимание -->

<!-- Эффекты -->
CornerRadius="8-12"           <!-- Скругленные углы -->
DropShadow BlurRadius="6-8"   <!-- Тень -->
Opacity="0.3-0.4"             <!-- Полупрозрачность -->
```

## 🔧 Использование Windows API

### SendInput для симуляции ввода

```csharp
[DllImport("user32.dll")]
private static extern bool SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

// Использование
INPUT input = new INPUT();
input.type = InputType.MOUSE;
input.U.mi.dwFlags = MOUSEEVENTF.LEFTDOWN;
SendInput(1, new INPUT[] { input }, Marshal.SizeOf<INPUT>());
```

### RegisterHotKey для глобальных горячих клавиш

```csharp
[DllImport("user32.dll")]
private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

// Использование
RegisterHotKey(windowHandle, 1, MOD_NONE, VK_F7);
```

## 📈 Масштабируемость

### Расширение функций

1. **Добавить новый тип действия**
   - Добавить в `ActionType enum`
   - Реализовать в `ExecuteAction()`
   - Добавить запись в `MonitorInputLoop()`

2. **Добавить новый игровой режим**
   - Добавить в `GameMode enum`
   - Добавить в `GameModes collection` (ViewModel)

3. **Добавить новый файл для сохранения**
   - Создать `IMacroFormatter` интерфейс
   - Реализовать JSON, XML, CSV форматы
   - Обновить `SaveToFile()`, `LoadFromFile()`

## 🚀 Оптимизация производительности

### Фоновый мониторинг ввода

```csharp
// Оптимизировано
Thread.Sleep(20);  // Проверяем каждые 20ms (50 раз в секунду)

// Не оптимально
Thread.Sleep(1);   // Использует много CPU
Thread.Sleep(100); // Пропускает быстрые действия
```

### Кэширование координат

```csharp
private int _lastMouseX = 0;
private int _lastMouseY = 0;

// Только добавляем, если координаты изменились
if (currentX != _lastMouseX || currentY != _lastMouseY)
{
    // Добавить действие
}
```

## 📚 Справочные материалы

### Windows API Virtual Keys
- https://learn.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes

### SendInput Function
- https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-sendinput

### RegisterHotKey Function
- https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-registerhotkey

### WPF Data Binding
- https://learn.microsoft.com/en-us/dotnet/desktop/wpf/data/data-binding-overview

---

**Architecture Document v1.0**  
Последнее обновление: 2026  
© Roblox Macro Suite
