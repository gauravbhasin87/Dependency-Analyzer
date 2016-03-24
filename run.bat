:run.bat
:
:runs Analyzer code

START "CLIENT" /D"Client\bin\Debug\" "CommClient.exe"
START "SERVER1" /D"Server\bin\Debug\" "ServerHost.exe"
START "SERVER2" /D"Server2\bin\Debug\" "Server2.exe"