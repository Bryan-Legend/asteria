"C:\Program Files (x86)\Microsoft Visual Studio 10.0\Common7\IDE\devenv" ../HD.sln /clean Steam
"C:\Program Files (x86)\Microsoft Visual Studio 10.0\Common7\IDE\devenv" ../HD.sln /rebuild Steam
pause

rmdir /s /q build
mkdir build
cd build
copy ..\..\HD\HD\bin\x86\Steam\*.exe
copy ..\..\HD\HD\bin\x86\Steam\*.dll
copy ..\..\HD\HD\bin\x86\Steam\*.exe.config
copy ..\..\Server\bin\x86\Steam\Server.exe
copy ..\..\Server\bin\x86\Steam\*.dll
pause
mkdir Content
xcopy /E /Y ..\..\Content Content
del /s /q /f /a:h Thumbs.db
cd ..

xcopy /E /Y build ..\References\steamworks_sdk_129a\tools\ContentBuilder\content\

cd ..\References\steamworks_sdk_129a\tools\ContentBuilder
builder\steamcmd.exe +login lonecoder +run_app_build ..\scripts\app_build_307130.vdf
cd "..\..\..\..\Advanced Installer"
pause