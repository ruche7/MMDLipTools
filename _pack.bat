@setlocal
@echo off

set SRC_EXE_DIR=%~dp0\MikuMikuLipMaker\bin\Release
set SRC_EXE_FILE=%SRC_EXE_DIR%\MikuMikuLipMaker.exe
set SRC_DOC_FILE=%~dp0\data\MikuMikuLipMaker\readme.txt
set SRC_PLUGIN_FILE=%~dp0\mmm\MikuMikuLipClient\bin\Release\ruche.mmm.MikuMikuLipClient.dll
set DEST_BASE_DIR=%~dp0\__release\MikuMikuLipMaker
set DEST_SYSTEM_DIR=%DEST_BASE_DIR%\system
set DEST_PLUGIN_DIR=%DEST_BASE_DIR%\mmm_plugin

REM ---- check source

if not exist "%SRC_EXE_FILE%" (
  echo "%SRC_EXE_FILE%" is not found.
  goto ON_ERROR
)
if not exist "%SRC_DOC_FILE%" (
  echo "%SRC_DOC_FILE%" is not found.
  goto ON_ERROR
)
if not exist "%SRC_PLUGIN_FILE%" (
  echo "%SRC_PLUGIN_FILE%" is not found.
  goto ON_ERROR
)

REM ---- remake destination

if exist "%DEST_BASE_DIR%" rmdir /S /Q "%DEST_BASE_DIR%"
mkdir "%DEST_BASE_DIR%"
if errorlevel 1 goto ON_ERROR

if exist "%DEST_SYSTEM_DIR%" rmdir /S /Q "%DEST_SYSTEM_DIR%"
mkdir "%DEST_SYSTEM_DIR%"
if errorlevel 1 goto ON_ERROR

if exist "%DEST_PLUGIN_DIR%" rmdir /S /Q "%DEST_PLUGIN_DIR%"
mkdir "%DEST_PLUGIN_DIR%"
if errorlevel 1 goto ON_ERROR

REM ---- copy files

xcopy /Y "%SRC_EXE_DIR%"\*.exe "%DEST_BASE_DIR%"
if errorlevel 1 goto ON_ERROR
xcopy /Y "%SRC_EXE_DIR%"\*.exe.config "%DEST_BASE_DIR%"
if errorlevel 1 goto ON_ERROR
xcopy /Y "%SRC_EXE_DIR%"\*.dll "%DEST_SYSTEM_DIR%"
if errorlevel 1 goto ON_ERROR
xcopy /Y /I "%SRC_EXE_DIR%\ja" "%DEST_SYSTEM_DIR%\ja"
if errorlevel 1 goto ON_ERROR
xcopy /Y "%SRC_DOC_FILE%" "%DEST_BASE_DIR%"
if errorlevel 1 goto ON_ERROR
xcopy /Y "%SRC_PLUGIN_FILE%" "%DEST_PLUGIN_DIR%"
if errorlevel 1 goto ON_ERROR

exit /b 0

:ON_ERROR
pause
exit /b 1
