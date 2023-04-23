# Q3C273 - The offensive backdoor / RAT forked from [Quasar](https://github.com/quasar/Quasar)

Its name **Q3C273** inspired from the first *Quasar* ever to be identified, [3C 273](https://en.wikipedia.org/wiki/3C_273).

## TODO

* [x] Replace direct sensitive method calls (such as SetWindowsHookEx) with SilentProcAddress
* [ ] Make program location more customizable (Currently, no `\` character supported, and doesn't support many paths)
* [x] Make Reflective-DLL-injectable (see [SharpNeedle](https://github.com/ChadSki/SharpNeedle), [SimpleSyringe](https://github.com/hsheric0210/simplesyringe)), and [Powersploit's](https://github.com/PowerShellMafia/PowerSploit/blob/master/CodeExecution/Invoke-ReflectivePEInjection.ps1)
* [x] Remove any signs of name `Quasar` from the builded executable.
* [ ] Thread context hijacking injection (https://github.com/luciopaiva/easyhook/blob/master/EasyHookDll/RemoteHook/stealth.c)

* [ ] More persistence options. Such as [WMI event filter injection by Powersploit](https://github.com/PowerShellMafia/PowerSploit/blob/master/Persistence/Persistence.psm1), Winlogon shell injection, Time provider injection, etc. More available on [here](https://attack.mitre.org/tactics/TA0003/).
* [ ] [AV bypass](https://github.com/PowerShellMafia/PowerSploit/blob/master/AntivirusBypass/Find-AVSignature.ps1)?
* [ ] [APC injection](https://github.com/3gstudent/Inject-dll-by-APC)
* [ ] Ransomeware. (File encryption)
* [ ] Integrate [UACMe](https://github.com/hfiref0x/UACME) to bypass UAC's and replace Client Privilege Elevation with it.

* [ ] Check some famous and useful post-exploitation frameworks
  * [PoshC2](https://github.com/nettitude/PoshC2)
  * [SILENTTRINITY](https://github.com/byt3bl33d3r/SILENTTRINITY)
  * [pupy](https://github.com/n1nj4sec/pupy)
  * [Merlin](https://github.com/Ne0nd0g/merlin)
  * [Ghost](https://github.com/EntySec/Ghost)
  * [VENOM](https://github.com/r00t-3xp10it/venom)
  * [Khepri](https://github.com/geemion/Khepri)
  * [Kubesploit](https://github.com/cyberark/kubesploit)
  * [emp3r0r](https://github.com/jm33-m0/emp3r0r)
  * [BlackMamba](https://github.com/loseys/BlackMamba)
  * [Potato](https://github.com/foxglovesec/Potato)
  * [BeRoot](https://github.com/AlessandroZ/BeRoot)
  * [PowerHub](https://github.com/AdrianVollmer/PowerHub)
  * [covertutils](https://github.com/operatorequals/covertutils)
  * [OffensiveAutoIt](https://github.com/V1V1/OffensiveAutoIt)
  * [Evasor](https://github.com/cyberark/Evasor)
  * [Covermyass](https://github.com/sundowndev/covermyass)
  * [RSPET](https://github.com/panagiks/RSPET)
  * [FudgeC2](https://github.com/Ziconius/FudgeC2)
  * [HatSploit](https://github.com/EntySec/HatSploit)
  * [redpill](https://github.com/r00t-3xp10it/redpill)
  * [XENA](https://github.com/zarkones/XENA)
  * [TREVORspray](https://github.com/blacklanternsecurity/TREVORspray)
  * [CredMaster](https://github.com/knavesec/CredMaster)
  * [OffensiveDLR](https://github.com/byt3bl33d3r/OffensiveDLR)
  * [Empire](https://github.com/EmpireProject/Empire)
  * [Nishang](https://github.com/samratashok/nishang)
  * [Responder](https://github.com/lgandx/Responder)
  * [RTA](https://github.com/endgameinc/RTA)
  * [CALDERA](https://github.com/mitre/caldera)
  * [Atomic Red Team](https://github.com/redcanaryco/atomic-red-team)
  * [Metta](https://github.com/uber-common/metta)

* [ ] Also check these lists: [Red Teaming Toolkit](https://github.com/infosecn1nja/Red-Teaming-Toolkit)

[![Build status](https://ci.appveyor.com/api/projects/status/5857hfy6r1ltb5f2?svg=true)](https://ci.appveyor.com/project/MaxXor/quasar)
[![Downloads](https://img.shields.io/github/downloads/quasar/Quasar/total.svg)](https://github.com/quasar/Quasar/releases)
[![License](https://img.shields.io/github/license/quasar/Quasar.svg)](LICENSE)

**Free, Open-Source Remote Administration Tool for Windows**

Quasar is a fast and light-weight remote administration tool coded in C#. The usage ranges from user support through day-to-day administrative work to employee monitoring. Providing high stability and an easy-to-use user interface, Quasar is the perfect remote administration solution for you.

## Screenshots

![remote-shell](Images/remote-shell.png)

![remote-desktop](Images/remote-desktop.png)

![remote-files](Images/remote-files.png)

## Features
* TCP network stream (IPv4 & IPv6 support)
* Fast network serialization (Protocol Buffers)
* Encrypted communication (TLS)
* UPnP Support (automatic port forwarding)
* Task Manager
* File Manager
* Startup Manager
* Remote Desktop
* Remote Shell
* Remote Execution
* System Information
* Registry Editor
* System Power Commands (Restart, Shutdown, Standby)
* Keylogger (Unicode Support)
* Reverse Proxy (SOCKS5)
* Password Recovery (Common Browsers and FTP Clients)
* ... and many more!

## Download
* [Latest stable release](https://github.com/quasar/Quasar/releases) (recommended)
* [Latest development snapshot](https://ci.appveyor.com/project/MaxXor/quasar)

## Supported runtimes and operating systems
* .NET Framework 4.5.2 or higher
* Supported operating systems (32- and 64-bit)
  * Windows 11
  * Windows Server 2022
  * Windows 10
  * Windows Server 2019
  * Windows Server 2016
  * Windows 8/8.1
  * Windows Server 2012
  * Windows 7
  * Windows Server 2008 R2
* For older systems please use [Quasar version 1.3.0](https://github.com/quasar/Quasar/releases/tag/v1.3.0.0)

## Compiling
Open the project `Quasar.sln` in Visual Studio 2019+ with installed .NET desktop development features and [restore the NuGET packages](https://docs.microsoft.com/en-us/nuget/consume-packages/package-restore). Once all packages are installed the project can be compiled as usual by clicking `Build` at the top or by pressing `F6`. The resulting executables can be found in the `Bin` directory. See below which build configuration to choose from.

## Building a client
| Build configuration         | Usage scenario | Description
| ----------------------------|----------------|--------------
| Debug configuration         | Testing        | The pre-defined [Settings.cs](/Quasar.Client/Config/Settings.cs) will be used, so edit this file before compiling the client. You can execute the client directly with the specified settings.
| Release configuration       | Production     | Start `Quasar.exe` and use the client builder.

## Contributing
See [CONTRIBUTING.md](CONTRIBUTING.md)

## Roadmap
See [ROADMAP.md](ROADMAP.md)

## Documentation
See the [wiki](https://github.com/quasar/Quasar/wiki) for usage instructions and other documentation.

## License
Quasar is distributed under the [MIT License](LICENSE).  
Third-party licenses are located [here](Licenses).

## Thank you!
I really appreciate all kinds of feedback and contributions. Thanks for using and supporting Quasar!
