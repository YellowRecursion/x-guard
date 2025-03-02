@echo off
setlocal enabledelayedexpansion

rem Configuration (можно менять между Release/Debug)
set CONFIG=Release

rem Пути
set ROOT_DIR=%~dp0
set BUILD_DIR=%ROOT_DIR%Build\

rem Очистка и создание Build директории
if exist "%BUILD_DIR%" (
    rmdir /s /q "%BUILD_DIR%"
)
mkdir "%BUILD_DIR%"

rem Список проектов для копирования (относительные пути)
set PROJECTS[0]=XGuardKeepAlive\bin\%CONFIG%\net8.0
set PROJECTS[1]=XGuardLauncher\bin\%CONFIG%\net8.0
set PROJECTS[2]=XGuardUser\bin\%CONFIG%\net8.0-windows
set PROJECTS[3]=XGuardMain\bin\%CONFIG%\net8.0

rem Копирование файлов из каждого проекта
for /L %%i in (0,1,4) do (
    set SRC_DIR=%ROOT_DIR%!PROJECTS[%%i]!
    if exist "!SRC_DIR!\" (
        xcopy "!SRC_DIR!\*" "%BUILD_DIR%" /E /I /Y /Q
    ) else (
        echo Directory not found: !SRC_DIR!
    )
)

echo Все файлы скопированы в: %BUILD_DIR%
endlocal