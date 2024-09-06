@echo off
set rt11exe=C:\bin\rt11\rt11.exe

rem Define ESCchar to use in ANSI escape sequences
rem https://stackoverflow.com/questions/2048509/how-to-echo-with-different-colors-in-the-windows-command-line
for /F "delims=#" %%E in ('"prompt #$E# & for %%E in (1) do rem"') do set "ESCchar=%%E"

for /f "tokens=2 delims==" %%a in ('wmic OS Get localdatetime /value') do set "dt=%%a"
set "YY=%dt:~2,2%" & set "YYYY=%dt:~0,4%" & set "MM=%dt:~4,2%" & set "DD=%dt:~6,2%"
set "DATESTAMP=%YYYY%-%MM%-%DD%"
for /f %%i in ('git rev-list HEAD --count') do (set REVISION=%%i)
echo REV.%REVISION% %DATESTAMP%

echo 	.ASCII /REV.%REVISION% %DATESTAMP%/ > VERSIO.MAC

@if exist TILES.OBJ del TILES.OBJ
@if exist S2CORE.LST del S2CORE.LST
@if exist S2CORE.OBJ del S2CORE.OBJ
@if exist S2CORE.MAP del S2CORE.MAP
@if exist S2CORE.SAV del S2CORE.SAV
@if exist S2CORE.COD del S2CORE.COD
@if exist S2CORE.LZS del S2CORE.LZS
@if exist S2BOOT.LST del S2BOOT.LST
@if exist S2BOOT.OBJ del S2BOOT.OBJ
@if exist S2BOOT.SAV del S2BOOT.SAV
@if exist SABOT2.BIN del SABOT2.BIN

%rt11exe% MACRO/LIST:DK: S2CORE.MAC

for /f "delims=" %%a in ('findstr /B "Errors detected" S2CORE.LST') do set "errdet=%%a"
if "%errdet%"=="Errors detected:  0" (
  echo S2CORE COMPILED SUCCESSFULLY
) ELSE (
  findstr /RC:"^[ABDEILMNOPQRTUZ] " S2CORE.LST
  echo ======= %errdet% =======
  goto :Failed
)

%rt11exe% LINK S2CORE /MAP:S2CORE.MAP

for /f "delims=" %%a in ('findstr /B "Undefined globals" S2CORE.MAP') do set "undefg=%%a"
if "%undefg%"=="" (
  type S2CORE.MAP
  echo.
  echo S2CORE LINKED SUCCESSFULLY
) ELSE (
  echo ======= LINK FAILED =======
  goto :Failed
)

rem Get S2CORE.SAV code size and cut off parts we don't need
for /f "delims=" %%a in ('findstr /RC:"High limit = " S2CORE.MAP') do set "codesize=%%a"
set "codesize=%codesize:~49,5%"
rem echo Code limit %codesize% words
set /a codesize="%codesize% * 2"
powershell gc S2CORE.SAV -Encoding byte -TotalCount %codesize% ^| sc S2CORE.CO0 -Encoding byte
set /a codesize="%codesize% - 712"
powershell gc S2CORE.CO0 -Encoding byte -Tail %codesize% ^| sc S2CORE.COD -Encoding byte
del S2CORE.CO0
rem echo Code size %codesize% bytes
dir /-c S2CORE.COD|findstr /R /C:"S2CORE.COD"

tools\lzsa3.exe S2CORE.COD S2CORE.LZS
dir /-c S2CORE.LZS|findstr /R /C:"S2CORE.LZS"
call :FileSize S2CORE.LZS
set "codelzsize=%fsize%"
rem echo Compressed size %codelzsize%

rem Reuse VERSIO.MAC to pass parameters into S2BOOT.MAC
echo S2LZSZ = %codelzsize%. >> VERSIO.MAC

%rt11exe% MACRO/LIST:DK: S2BOOT.MAC

for /f "delims=" %%a in ('findstr /B "Errors detected" S2BOOT.LST') do set "errdet=%%a"
if "%errdet%"=="Errors detected:  0" (
  echo S2BOOT COMPILED SUCCESSFULLY
) ELSE (
  findstr /RC:"^[ABDEILMNOPQRTUZ] " S2BOOT.LST
  echo ======= %errdet% =======
  goto :Failed
)

%rt11exe% LINK S2BOOT /MAP:S2BOOT.MAP

for /f "delims=" %%a in ('findstr /B "Undefined globals" S2BOOT.MAP') do set "undefg=%%a"
if "%undefg%"=="" (
  type S2BOOT.MAP
  echo.
  echo S2BOOT LINKED SUCCESSFULLY
) ELSE (
  echo ======= LINK FAILED =======
  goto :Failed
)

tools\Sav2BkBin.exe S2BOOT.SAV SABOT2.BIN
dir /-c SABOT2.BIN|findstr /R /C:"SABOT2.BIN"

echo %ESCchar%[92mSUCCESS%ESCchar%[0m
exit

:Failed
@echo off
echo %ESCchar%[91mFAILED%ESCchar%[0m
exit /b

:FileSize
set fsize=%~z1
exit /b 0
