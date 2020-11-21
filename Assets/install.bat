@echo off
@setlocal enableextensions
@cd /d "%~dp0"
xcopy %0\..\* "C:\Program Files (x86)\Asteria\" /E /Y

echo Set oWS = WScript.CreateObject("WScript.Shell") > CreateShortcut.vbs
echo sLinkFile = "%HOMEDRIVE%%HOMEPATH%\Desktop\Asteria.lnk" >> CreateShortcut.vbs
echo Set oLink = oWS.CreateShortcut(sLinkFile) >> CreateShortcut.vbs
echo oLink.TargetPath = "C:\Program Files (x86)\Asteria\Asteria.exe" >> CreateShortcut.vbs
echo oLink.WorkingDirectory = "C:\Program Files (x86)\Asteria\" >> CreateShortcut.vbs
echo oLink.Save >> CreateShortcut.vbs
cscript CreateShortcut.vbs
del CreateShortcut.vbs

set /p name= Do you want to install XNA? 
IF %name% == Y GOTO install
IF %name% == y GOTO install
GOTO end 
:install 
REM wget http://download.microsoft.com/download/A/C/2/AC2C903B-E6E8-42C2-9FD7-BEBAC362A930/xnafx40_redist.msi
bitsadmin /transfer DownloadingXNA /download /priority normal http://download.microsoft.com/download/A/C/2/AC2C903B-E6E8-42C2-9FD7-BEBAC362A930/xnafx40_redist.msi %HOMEDRIVE%%HOMEPATH%\Downloads\xnafx40_redist.msi

%HOMEDRIVE%%HOMEPATH%\Downloads\xnafx40_redist.msi /qn
:end
PAUSE