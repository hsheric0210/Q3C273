# Q3C273.Injected - Payload executor DLL

This DLL is injected into some random process in the victim PC and act as payload 'executor' to bypass AV and such a thing.

It communicates with the main process using named pipes.

## TODO
* [ ] Silent TCP socket - Bypass from being detected by AV from suspicious network activity from client process. To do so, we should inject the DLL to browser processes.
* [ ] File read/write/delete function - Bypass AV from detecting some specific files such as browser cookie file, encrypted password files. To do so, we should inject the DLL to the related programs, such as browsers, some quasi-AVs such as nProtect, ASTx, Delfino.
* [ ] 