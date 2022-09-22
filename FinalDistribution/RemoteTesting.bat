@echo Launching remote testing session with 2 clients connected to a remote server.
@echo Please do not close this window until both clients have launched.
@echo off

@rem Shut down servers if any
tasklist /fi "ImageName eq hoodies_server.exe" /fo csv 2>NUL | find /I "hoodies_server.exe">NUL
if %ERRORLEVEL%==0 (
@echo Shutting down server.
@echo off
taskkill /im hoodies_server.exe

@rem Wait 10 seconds
ping 192.0.2.2 -n 1 -w 10000 > nul)

@rem Shut down clients if any
tasklist /fi "ImageName eq hoodies_client.exe" /fo csv 2>NUL | find /I "hoodies_client.exe">NUL
if %ERRORLEVEL%==0 (
@echo Shutting down client.
@echo off
taskkill /im hoodies_client.exe

@rem Wait 10 seconds
ping 192.0.2.2 -n 1 -w 10000 > nul)  

@rem Move to client folder
cd WindowsClient

@rem Start a client process
@echo Launching first client.
@echo off
start hoodies_client.exe

@rem Wait 2 seconds
ping 192.0.2.2 -n 1 -w 2000 > nul

@rem Start a client process
@echo Launching second client.
@echo off
start hoodies_client.exe

@echo You may close this window now.
@pause