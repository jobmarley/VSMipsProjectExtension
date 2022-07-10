# VSMipsProjectExtension

This is a project + debugger extension for visual studio to be used with [MIPS_core](https://github.com/jobmarley/MIPS_core).

It only support very basic functionalities.
To use it MipsRemoteDebugger must be running on the target machine, and the project must be properly configured as explained [here](https://github.com/jobmarley/mips_project_test).

## Supported features
- launch
- pause/resume
- dwarf debug info in elf
- callstack
- hardcoded breakpoints (mips instruction "sdbbp", breakpoint window and adding breakpoint on the fly not supported)
- local variable for simple types

![image](https://user-images.githubusercontent.com/99695100/178160470-03acb55f-7645-4ae0-8374-3537b45f9cbd.png)
