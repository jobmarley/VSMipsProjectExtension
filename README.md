# VSMipsProjectExtension

This is a project + debugger extension to be used with [MIPS_core](https://github.com/jobmarley/MIPS_core).

It only support very basic functionalities.
To use it MipsRemoteDebugger must be running on the target machine, and the project must be properly configured as explained [here](https://github.com/jobmarley/mips_project_test).

## Supported features
- launch
- pause/resume
- dwarf debug info in elf
- callstack
- hardcoded breakpoints (mips instruction "sdbbp", breakpoint window and adding breakpoint on the fly not supported)
- local variable for simple types

![image](https://user-images.githubusercontent.com/99695100/178160112-6fc0c04e-99a5-4593-ad0f-1337fae8e340.png)
