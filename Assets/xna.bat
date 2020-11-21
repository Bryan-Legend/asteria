@echo off
@setlocal enableextensions
@cd /d "%~dp0"
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