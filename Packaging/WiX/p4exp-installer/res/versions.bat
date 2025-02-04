rem Create versions.txt file with list of file descriptions and versions.
rem Example line: Plug-in for Windows Explorer (P4EXP) (x64)(x86): 2017.1/1592449
set verfile=versions.txt
set tmpfile=tempver.txt
set showver=showver.exe

del "%RootRelease%\packaging\wix\p4exp-installer\res\%verfile%"> nul 2>&1
del %tmpfile%> nul 2>&1

if not exist "%RootRelease%\packaging\wix\p4exp-installer\res\%showver%" goto errshowver

if not exist "%Bin64%\p4exp.dll" goto noexpx64
"%RootRelease%\packaging\wix\p4exp-installer\res\%showver%" "%Bin64%\p4exp.dll">%tmpfile%
if not exist %tmpfile% goto errtemp
for /f "tokens=1,2" %%y in (%tmpfile%) do if %%y==FileVersion: for /f "tokens=1,2,3,4 delims=." %%a in ("%%z") do (set version=%%a.%%b/) && (set change1=%%c) && (set change2=%%d) 
del %tmpfile%
if not (%change1%) == (0) (set version=%version%%change1%)
if %change2% lss 10 (set version=%version%000%change2%) && goto verok
if %change2% lss 100 (set version=%version%00%change2%) && goto verok
if %change2% lss 1000 (set version=%version%0%change2%) && goto verok
set version=%version%%change2%
:verok
echo Plug-in for Windows Explorer (P4EXP) (x64): %version%>>"%RootRelease%\packaging\wix\p4exp-installer\res\%verfile%"
:noexpx64

goto end

:errtemp
echo.
echo Temp version file was not created
echo There was a problem creating the product configuration executable versions file.
exit /b 1

:errshowver
echo.
echo Could not find showver.exe file
echo There was a problem creating the product configuration executable versions file.
exit /b 1

:end