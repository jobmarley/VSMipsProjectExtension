# VSMipsProjectExtension

This is a project + debugger extension for visual studio to be used with [MIPS_core](https://github.com/jobmarley/MIPS_core).

It only support very basic functionalities.
To use it, MipsRemoteDebugger must be running on the target machine, and the project must be properly configured as explained [here](https://github.com/jobmarley/mips_project_test).

## Supported features
- launch
- pause/resume
- dwarf debug info in elf
- callstack
- hardcoded breakpoints (mips instruction "sdbbp")
- dynamic breakpoints
- local variable for simple types (char/char*, ints, float, pointers, classes/struct not supported for now)

![image](https://user-images.githubusercontent.com/99695100/178160470-03acb55f-7645-4ae0-8374-3537b45f9cbd.png)

## VSMipsProjectExtension
Contains the VSPackage of the extension, the IDebugLaunchProvider implementation used to launch the debugger

## MipsDebugEngine
Contains the debug engine. It communicates with MipsRemoteDebugger throught TCP to debug the processor.

## ElfSymbolProvider
This is a native side-by-side COM component. It contains everything related to debug symbols. I implemented it in native c++ because it uses libdwarf, and I didn't want to bother marshalling everything in c#.  
The libdwarf binaries were compiled by myself on mingw64.

## MipsRemoteDebugger
This is the server part of the debugger. It uses [mipsdebug](https://github.com/jobmarley/pcie_mips_driver) on the target machine to access the processor hardware.

## MipsRemoteTest
Same as the [test](https://github.com/jobmarley/pcie_mips_driver) application of the driver, but over TCP, in combination with MipsRemoteDebugger
