# API Reference - Roblox Macro

## Класс: InputSimulator

Статический класс для симуляции пользовательского ввода через Windows API.

### Методы управления курсором мыши

#### GetCursorPosition(out int x, out int y)
Получает текущие координаты курсора мыши.

```csharp
InputSimulator.GetCursorPosition(out int x, out int y);
Console.WriteLine($"Позиция мыши: ({x}, {y})");
```

#### SetCursorPosition(int x, int y)
Устанавливает позицию курсора мыши.

```csharp
InputSimulator.SetCursorPosition(640, 480);
```

#### MoveMouse(int x, int y)
Плавно перемещает курсор на указанные координаты.

```csharp
InputSimulator.MoveMouse(100, 100);
```

### Методы кликов мыши

#### LeftClick()
Имитирует клик левой кнопкой мыши в текущей позиции.

```csharp
InputSimulator.LeftClick();
```

#### LeftClickAt(int x, int y)
Имитирует клик левой кнопкой мыши в указанной позиции.

```csharp
InputSimulator.LeftClickAt(500, 300);
```

#### RightClick()
Имитирует клик правой кнопкой мыши.

```csharp
InputSimulator.RightClick();
```

#### LeftMouseDown() / LeftMouseUp()
Нажимает и отпускает левую кнопку мыши отдельно.

```csharp
InputSimulator.LeftMouseDown();
Thread.Sleep(100);
InputSimulator.LeftMouseUp();
```

### Методы работы с клавиатурой

#### KeyClick(int keyCode)
Полный цикл нажатия и отпускания клавиши.

```csharp
InputSimulator.KeyClick(InputSimulator.VirtualKeys.VK_A);
```

#### KeyDown(int keyCode) / KeyUp(int keyCode)
Нажимает и отпускает клавишу отдельно.

```csharp
InputSimulator.KeyDown(InputSimulator.VirtualKeys.VK_CONTROL);
InputSimulator.KeyClick(InputSimulator.VirtualKeys.VK_A);
InputSimulator.KeyUp(InputSimulator.VirtualKeys.VK_CONTROL);
```

#### KeyCombination(params int[] keyCodes)
Нажимает комбинацию клавиш.

```csharp
// Ctrl+A
InputSimulator.KeyCombination(
    InputSimulator.VirtualKeys.VK_CONTROL,
    InputSimulator.VirtualKeys.VK_A
);

// Ctrl+Shift+S
InputSimulator.KeyCombination(
    InputSimulator.VirtualKeys.VK_CONTROL,
    InputSimulator.VirtualKeys.VK_SHIFT,
    InputSimulator.VirtualKeys.VK_S
);
```

#### TypeText(string text)
Вводит текст через клавиатуру.

```csharp
InputSimulator.TypeText("Hello World!");
```

---

## Класс: MacroRecorder

Класс для записи действий макроса.

### Свойства

| Свойство | Тип | Описание |
|----------|-----|---------|
| RecordedActions | IReadOnlyList<MacroAction> | Список записанных действий |
| IsRecording | bool | Проверяет, идет ли запись |

### События

#### ActionRecorded
Событие, вызываемое при записи нового действия.

```csharp
recorder.ActionRecorded += (action) => 
    Console.WriteLine($"Записано: {action}");
```

### Методы

#### StartRecording()
Начать запись макроса.

```csharp
var recorder = new MacroRecorder();
recorder.StartRecording();
```

#### StopRecording()
Остановить запись макроса.

```csharp
recorder.StopRecording();
```

#### Clear()
Очистить все записанные действия.

```csharp
recorder.Clear();
```

#### GetRecordingDuration()
Получить длительность записи.

```csharp
TimeSpan duration = recorder.GetRecordingDuration();
Console.WriteLine($"Записано за {duration.TotalSeconds} секунд");
```

#### SaveToFile(string filePath)
Сохранить макрос в JSON файл.

```csharp
recorder.SaveToFile("my_macro.json");
```

#### LoadFromFile(string filePath)
Загрузить макрос из JSON файла.

```csharp
bool success = recorder.LoadFromFile("my_macro.json");
```

---

## Класс: MacroPlayer

Класс для воспроизведения записанных макросов.

### Свойства

| Свойство | Тип | Описание |
|----------|-----|---------|
| IsPlaying | bool | Проверяет, идет ли воспроизведение |

### События

#### StatusChanged
Событие при изменении статуса воспроизведения.

```csharp
player.StatusChanged += (status) => 
    Console.WriteLine($"Статус: {status}");
```

#### PlaybackCompleted
Событие при завершении воспроизведения.

```csharp
player.PlaybackCompleted += () => 
    Console.WriteLine("Макрос завершен!");
```

#### PlaybackError
Событие при ошибке воспроизведения.

```csharp
player.PlaybackError += (error) => 
    Console.WriteLine($"Ошибка: {error}");
```

### Методы

#### SetMacro(IReadOnlyList<MacroAction> actions)
Установить макрос для воспроизведения.

```csharp
player.SetMacro(recorder.RecordedActions);
```

#### StartPlayback(int repetitions = 1)
Запустить воспроизведение макроса.

```csharp
// Воспроизвести 5 раз
player.StartPlayback(5);

// Бесконечное воспроизведение (0 = бесконечно)
player.StartPlayback(0);
```

#### StopPlayback()
Остановить воспроизведение макроса.

```csharp
player.StopPlayback();
```

#### LoadFromFile(string filePath)
Загрузить и установить макрос из файла.

```csharp
player.LoadFromFile("my_macro.json");
```

#### GetMacroInfo()
Получить информацию о загруженном макросе.

```csharp
string info = player.GetMacroInfo();
Console.WriteLine(info);
```

---

## Класс: HotkeyManager

Класс для управления глобальными горячими клавишами.

### Методы

#### RegisterHotkey(uint keyCode, Action action, HotkeyModifier modifiers = HotkeyModifier.None)
Зарегистрировать горячую клавишу.

```csharp
var hotkeyManager = new HotkeyManager(windowHandle);

// F7
int hotkeyId1 = hotkeyManager.RegisterHotkey(
    (uint)InputSimulator.VirtualKeys.VK_F7,
    () => Console.WriteLine("F7 нажат!"),
    HotkeyModifier.None
);

// Ctrl+Alt+M
int hotkeyId2 = hotkeyManager.RegisterHotkey(
    (uint)InputSimulator.VirtualKeys.VK_M,
    () => Console.WriteLine("Ctrl+Alt+M нажат!"),
    HotkeyModifier.Ctrl | HotkeyModifier.Alt
);
```

#### UnregisterHotkey(int hotkeyId)
Отменить регистрацию горячей клавиши.

```csharp
hotkeyManager.UnregisterHotkey(hotkeyId1);
```

#### ProcessHotkey(int hotkeyId)
Обработать нажатие горячей клавиши (вызывается автоматически).

```csharp
hotkeyManager.ProcessHotkey(hotkeyId);
```

#### Dispose()
Освободить все зарегистрированные горячие клавиши.

```csharp
hotkeyManager.Dispose();
```

---

## Класс: MacroProfileManager

Класс для управления профилями макросов.

### Методы

#### SaveProfile(...)
Сохранить макрос как профиль.

```csharp
var manager = new MacroProfileManager();
manager.SaveProfile(
    profileName: "Raid Combo",
    gameMode: GameMode.Raid,
    actions: recordedActions,
    description: "Оптимизированная комбо для Raid режима",
    repetitions: 10,
    infiniteRepeat: false
);
```

#### LoadProfile(string profileName)
Загрузить профиль макроса.

```csharp
var profile = manager.LoadProfile("Raid Combo");
if (profile != null)
{
    player.SetMacro(profile.Actions);
}
```

#### GetAvailableProfiles()
Получить список всех сохраненных профилей.

```csharp
List<string> profiles = manager.GetAvailableProfiles();
foreach (var profile in profiles)
{
    Console.WriteLine(profile);
}
```

#### DeleteProfile(string profileName)
Удалить профиль.

```csharp
manager.DeleteProfile("Old Macro");
```

#### GetProfileInfo(string profileName)
Получить информацию о профиле.

```csharp
string info = manager.GetProfileInfo("Raid Combo");
Console.WriteLine(info);
```

---

## Перечисление: ActionType

Типы действий в макросе.

```csharp
public enum ActionType
{
    MouseMove,      // Движение мыши
    MouseClick,     // Клик мыши
    MouseDown,      // Нажатие кнопки мыши
    MouseUp,        // Отпускание кнопки мыши
    KeyDown,        // Нажатие клавиши
    KeyUp,          // Отпускание клавиши
    Wait            // Пауза/ожидание
}
```

---

## Перечисление: GameMode

Доступные игровые режимы.

```csharp
public enum GameMode
{
    StoryMode,      // 📖 Story Mode
    LegendStage,    // 👑 Legends Stage
    InfinityMode,   // ♾️ Infinity Mode
    Raid,           // ⚔️ Raid
    Challenge       // ⭐ Challenge
}
```

---

## Перечисление: HotkeyModifier

Модификаторы для горячих клавиш.

```csharp
[Flags]
public enum HotkeyModifier
{
    None = 0,
    Alt = 1,
    Ctrl = 2,
    Shift = 4,
    Win = 8
}
```

---

## Примеры комплексного использования

### Полный цикл запись-воспроизведение

```csharp
using RobloxMacro.Services;
using RobloxMacro.Models;

// Инициализировать рекордер
var recorder = new MacroRecorder();

// Начать запись
recorder.StartRecording();
recorder.ActionRecorded += (action) => 
    Console.WriteLine($"Записано: {action}");

// ... ждем пока пользователь выполнит действия ...
Thread.Sleep(5000);

// Остановить запись
recorder.StopRecording();
Console.WriteLine($"Записано {recorder.RecordedActions.Count} действий");

// Инициализировать плеер
var player = new MacroPlayer();

// Подписаться на события
player.StatusChanged += (status) => Console.WriteLine(status);
player.PlaybackCompleted += () => Console.WriteLine("✓ Завершено");
player.PlaybackError += (error) => Console.WriteLine($"✗ {error}");

// Установить макрос
player.SetMacro(recorder.RecordedActions);

// Запустить воспроизведение 3 раза
player.StartPlayback(3);

// Ждать завершения
while (player.IsPlaying)
{
    Thread.Sleep(100);
}

// Сохранить для последующего использования
recorder.SaveToFile("my_macro.json");
```

---

**Документация API v1.0**  
Последнее обновление: 2026  
© Roblox Macro Suite
