# Winamp Server
Winamp server is a .NET console application that allows to play YouTube (using IE) or radio (using VLC) by controlling with web application using OWIN and SignalR.

On a server side, there is .NET Framework 4.6 console application, that uses:
* JSON configuration files
* VLC wrapper for playing radio streams
* background IE wrapper to run YouTube clips
* AudioSwitcher for adjusting volume
* SignalR for communication to clients
* Owin to run web server for clients

On a client side, there is simple HTML page with JavaScript, that uses:
* jQuery for selecting HTML elements and binding actions
* SignalR JS client for communication with server

## Winamp naming and styles
I do not own Winamp name and it is used only to honour classic Winamp application that was used for years and died because of online music.
I do not own any of the styling images that are copies of original Winamp application.

If any of image or naming is not appropriate or restricted by any of real authors, please contact me and I will remove those as soon as possible.

## How to use

- Download built version of application from Releases folder
- Make sure you have Internet explorer 11 installed
- I recomment installing any of the Ad blocker, so that YouTube won't show Ads and so destroy inner timer.
- If needed set correct bindings in WinampServer.exe.config file, as default runs on 81 port.
- Make sure this port is opened on machine you are running.
- Run WinampServer.exe
- Enjoy :)

## Commands

There are commands list, that can be runned using:
- Command line on server
- PowerShell like command line on client
- UI actions on client

### Available commands
Command | Description | UI | UI Shell | Server Shell
--- | --- | --- | --- | ---
m:y | Switches to YouTube mode | X | X | X
m:r | Switches to radio mode | X | X | X
v:{number} | Change volume to {number} percent | | X | X
w:{ID} | Add new YouTube video with {ID} (see YouTube URL param) to queue list | X | X | X
r:{radio_key} | Changes to radio from Radios.json settings | X | X | X
play | Starts player | X | X | X
start | Starts player | X | X | X
stop | Stops player | X | X | X
pause | Pause player | X | X | X
next | Stop current video and takes next from Queue | X | X | X
show ie | Reveals IE instance from background on server | | X | X
hide ie | Hides IE instance to background on server | X | X | X
register user:{name} | Assigs {name} to IP on server and saves to UsersConfig.json | | X | X
yer:{radio_key} | Sets to start specific radio after last item from queue finishes | X | X | X
exit | Stop and safely exit player by storing memory to PlayerStatus.json | | | x