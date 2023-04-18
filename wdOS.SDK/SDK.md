# wdOS
[Cosmos](https://github.com/CosmosOS/Cosmos)-based operating system for x86 PCs. And 
its the SDK for it :P

### What and how?
wdOS API is very simple. Its based on interrupt F0h and Windows Message system bootleg! 
Simple as it can be! There are not so many messages, here are all:

- MSG_IO_WRITE        - 10
- MSG_IO_READ         - 11
- MSG_MEMORY_ALLOC    - 20
- MSG_MEMORY_FREE     - 21
- MSG_CONTROL_STOP    - 30
- MSG_CONTROL_SETTIME - 31
- MSG_CONTROL_GETTIME - 32
- MSG_CONTROL_EXEC    - 33

#### Building programs
You use GCC to compile programs. C++ is not supported, only C is. You can use libc 
or wdOS API based on messages. Linker MUST compile program to ELF. Plus you MUST 
compile & link additional code with your app. I call it 'librt'. It stored in SDK 
'librt' folder. It implements message transmition and most of libc. When you link 
entire program, you can put it to disk and run it using 'run' command.

#### What's inside?
libc and wdOS API. Well... part of libc. I will not implement multithreading, because
nor Cosmos, neither wdOS support multithreading. Actually, only one app can run at a
time. When app executes another app, executor gets suspended and resumes only after
executed one terminates and returns exit code.

#### Couldn't you just use emulated language?
Becuase creating JIT compiler is too complicated and creating a simple interpreter
will result in awful performance.