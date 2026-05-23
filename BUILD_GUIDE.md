# Build Guide - Сборка приложения

## 🔨 Способ 1: Сборка через Visual Studio 2022

### Требования
- Visual Studio 2022 (Community Edition подходит)
- .NET 8.0 SDK
- Windows 10/11

### Шаги

1. **Открыть решение**
   ```
   Файл → Открыть → Папка
   Выбрать папку: Macros/
   ```

2. **Восстановить зависимости**
   ```
   Visual Studio автоматически восстановит NuGet пакеты
   Или вручную: Сборка → Восстановить решение
   ```

3. **Собрать проект**
   ```
   Сборка → Собрать решение
   Или Ctrl+Shift+B
   ```

4. **Запустить приложение**
   ```
   Отладка → Запустить без отладки (Ctrl+F5)
   ```

5. **Опубликовать**
   ```
   Сборка → Опубликовать
   Выбрать соответствующий профиль публикации
   ```

---

## 🔨 Способ 2: Сборка через .NET CLI

### Требования
- .NET 8.0 SDK (скачать с https://dotnet.microsoft.com/download)
- Любой текстовый редактор (VS Code, Notepad++ и т.д.)

### Шаги

#### Проверить версию .NET
```bash
dotnet --version
# Должно быть 8.0 или выше
```

#### Перейти в директорию проекта
```bash
cd "C:\Program Files (x86)\Steam\steamapps\common\P5X\client\pc\BepInEx\plugins\Macros"
```

#### Восстановить зависимости
```bash
dotnet restore
```

#### Собрать проект (Debug)
```bash
dotnet build -c Debug
```

#### Собрать проект (Release)
```bash
dotnet build -c Release
```

#### Запустить приложение
```bash
dotnet run
```

#### Опубликовать как исполняемый файл
```bash
# Опубликовать как отдельное приложение (автономное)
dotnet publish -c Release -r win-x64 --self-contained

# Выходной файл будет в:
# bin/Release/net8.0-windows/win-x64/publish/RobloxMacro.exe
```

#### Создать инсталлятор (опционально)
```bash
# Использовать WiX Toolset или NSIS для создания MSI
```

---

## 🐛 Отладка и диагностика

### Включить логирование
```bash
# Запустить с информативным выводом
dotnet run --verbosity diagnostic
```

### Проверить конфигурацию
```bash
# Проверить установленные SDK
dotnet --list-sdks

# Проверить установленные runtime
dotnet --list-runtimes
```

### Очистить кэш и пересобрать
```bash
# Удалить артефакты сборки
dotnet clean

# Восстановить и пересобрать
dotnet restore && dotnet build -c Release
```

---

## 📦 Структура выходных файлов

После сборки приложение будет находиться в:

```
bin/Release/net8.0-windows/
├── RobloxMacro.exe          # Основное приложение
├── RobloxMacro.dll          # Библиотека
├── RobloxMacro.runtimeconfig.json
├── System.*.dll             # Системные библиотеки
├── ...
└── [другие зависимости]
```

## 📋 Требования для разных типов сборки

### Debug сборка
- Быстрее компилируется
- Медленнее выполняется
- Включает символы отладки
- Размер: ~50MB

### Release сборка
- Оптимизирована
- Быстрее выполняется
- Меньше размер (~30MB)
- Рекомендуется для распространения

### Self-contained (автономная)
- Включает .NET runtime
- Работает без установленного .NET
- Размер: ~200MB
- Рекомендуется для пользователей

### Framework-dependent (зависит от фреймворка)
- Требует установленного .NET 8.0
- Размер: ~5MB
- Рекомендуется для разработчиков

---

## ✅ Проверка после сборки

### 1. Проверить, что приложение запускается
```bash
cd bin/Release/net8.0-windows
RobloxMacro.exe
```

### 2. Проверить функции в приложении
- [ ] UI загружается корректно
- [ ] Кнопки реагируют на клики
- [ ] Выбор режимов работает
- [ ] F7 и F8 работают (глобальные горячие клавиши)
- [ ] Status Bar обновляется

### 3. Проверить записи макроса
- [ ] Можно начать запись
- [ ] Фиксируются движения мыши
- [ ] Фиксируются нажатия клавиш
- [ ] Можно остановить запись

### 4. Проверить воспроизведение
- [ ] Макрос воспроизводится
- [ ] Правильно выполняются действия
- [ ] Можно остановить воспроизведение
- [ ] Повторения работают

---

## 🌍 Распространение приложения

### Вариант 1: Как ZIP архив
```bash
# Создать папку для распространения
mkdir RobloxMacro_v1.0

# Скопировать необходимые файлы
cp bin/Release/net8.0-windows/win-x64/publish/* RobloxMacro_v1.0/
cp README.md RobloxMacro_v1.0/
cp QUICKSTART.md RobloxMacro_v1.0/

# Создать архив
7z a RobloxMacro_v1.0.zip RobloxMacro_v1.0/
```

### Вариант 2: Как MSI инсталлятор
Требуется WiX Toolset:
```xml
<!-- Пример WiX файла (Product.wxs) -->
<?xml version="1.0" encoding="UTF-8"?>
<Wix xmlns="http://schemas.microsoft.com/wix/2006/wi">
  <Product Id="*" Name="Roblox Macro" Language="1033" Version="1.0.0.0">
    <!-- Конфигурация инсталлятора -->
  </Product>
</Wix>
```

### Вариант 3: Как портативное приложение
```bash
# Self-contained версия уже является портативной
# Просто скопировать содержимое publish папки
```

---

## 🔐 Подписание приложения (опционально)

Для подписания .exe файла (защита от предупреждений Windows):

```bash
# Требуется сертификат кода
signtool sign /f certificate.pfx /p password /t http://timestamp.server.com RobloxMacro.exe
```

---

## 🐳 Docker сборка (для CI/CD)

Если нужна автоматизированная сборка:

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 as builder

WORKDIR /src
COPY . .

RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:8.0-windowsservercore

WORKDIR /app
COPY --from=builder /app/publish .

ENTRYPOINT ["RobloxMacro.exe"]
```

---

## 📝 Troubleshooting

### Ошибка: "No matching .NET runtime found"
```
Решение: Установите .NET 8.0 runtime с https://dotnet.microsoft.com/download
```

### Ошибка: "Cannot find project file"
```
Решение: Убедитесь, что находитесь в правильной директории с RobloxMacro.csproj
```

### Ошибка: "The term 'dotnet' is not recognized"
```
Решение: Установите .NET SDK и перезагрузитесь в PowerShell/CMD
```

### Приложение не запускается после сборки
```
Решение:
1. Проверьте, что используется Release конфигурация
2. Убедитесь, что все зависимости установлены
3. Запустите диагностику: dotnet --info
```

---

## 🚀 Быстрая сборка и запуск

### Одна команда для всего
```bash
dotnet clean && dotnet build -c Release && dotnet run -c Release
```

### Сборка и публикация одновременно
```bash
dotnet publish -c Release -r win-x64 --self-contained -o ./publish
```

---

## 📊 Мониторинг производительности

Проверить производительность приложения:

```bash
# Получить информацию о процессе
tasklist | findstr RobloxMacro

# Более подробный анализ (если установлен)
dotnet trace collect --process-id <PID>
```

---

**Build Guide v1.0**  
Последнее обновление: 2026  
© Roblox Macro Suite
