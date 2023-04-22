# Q3C273.Utilities

Some sources related to Windows PE structure are copied from [stjeong/DotNetSamples](https://github.com/stjeong/DotNetSamples/)

* [WindowsPE](https://github.com/stjeong/DotNetSamples/tree/master/WinConsole/PEFormat/WindowsPE)
* [KernelStructOffset](https://github.com/stjeong/DotNetSamples/tree/master/WinConsole/Debugger/KernelStructOffset)

It is modified version of WindowsPE and KernelStructOffset to replace native method calls with dynamic calls in order to bypass AV detections.

Also, some of its enums and structs unrelated to Portable Executable format are merged with `ClientNatives.Structs`.