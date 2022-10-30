# wdOS
[Cosmos](https://github.com/CosmosOS/Cosmos)-based operating system for x86 PCs. And its the SDK for it :P

### What and how?
wdOS API is very simple. Its based on interrupt F0h and Windows Message system bootleg! Simple as it can be!
There are not so many messages, here are all:
- MSG_CONSOLE_WRITECHAR  0
- MSG_CONSOLE_WRITELINE  1
- MSG_CONSOLE_PUTCHARAT  2
- MSG_CONSOLE_CLEARSCR   3
- MSG_CONSOLE_SETFORE    4
- MSG_CONSOLE_SETBACK    5
- MSG_CONSOLE_GETFORE    6
- MSG_CONSOLE_GETBACK    7
- MSG_MEMORY_ALLOC       8
- MSG_MEMORY_FREE        9
- MSG_CONTROL_STOP       10
- MSG_CONTROL_PIT        11
- MSG_CONTROL_SETINT     12
- MSG_CONTROL_GETINT     13
- MSG_CONTROL_SETTIME    14
- MSG_CONTROL_GETTIME    15
As now you can't use it. But there is guide how to build programs for future:

#### Building program (not now)
You use gcc to compile programs. C++ is not supported, only C is. You can use libc or wdOS API based on messages.
Linker MUST compile program to plain binary, because its the most lightweight way to store compiled code. Plus you
MUST compile & link additional code with your app. I call it 'companion code'. It stored in SDK root folder and 
called 'companion.c'. It implements message transmition and another things. When link entire program you can put it 
to disk and run it using 'run' command

#### What's inside?
libc and wdOS API. Well... part of libc. I will not implement things like multithreading or another float calculations

#### Couldn't you just use emulated language?
Why? It's soooooooo slow! Plus it's too complicated to make it myself