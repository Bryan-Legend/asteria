"C:\Program Files (x86)\Microsoft Visual Studio 10.0\Common7\IDE\devenv" ../HD.sln /rebuild Release
pause

rmdir /s /q build
mkdir build
cd build
copy ..\..\HD\HD\bin\x86\Release\*.exe
copy ..\..\HD\HD\bin\x86\Release\*.dll
copy ..\..\HD\HD\bin\x86\Release\*.exe.config
copy ..\..\Server\bin\Release\Server.exe
copy ..\..\Server\bin\Release\*.dll
mkdir Content
xcopy /E /Y ..\..\Content Content
del /s /q /f /a:h Thumbs.db
cd ..
"C:\Program Files (x86)\Caphyon\Advanced Installer 10.5.2\bin\x86\AdvancedInstaller.com" /build asteria.aip
rem "C:\Program Files (x86)\Caphyon\Advanced Installer 9.9\bin\x86\AdvancedInstaller.com" Updates.aip
pause

rem Upload to store.playasteria.com
rem "C:\Program Files (x86)\CoreFTP\coreftp" -OG -site hera -u C:\Users\Bryan\Documents\HD2\Website\PrecompiledWeb\Download\*.* -p /HTTP/wwwroot/playasteria.com/Download
