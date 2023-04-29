# wdOS
[Cosmos](https://github.com/CosmosOS/Cosmos)-based operating system for x86 PCs. And 
its the SDK for it :P

### What and how?
wdOS API is based on Kernel Function Tables. These tables have wrappers on internal 
kernel functions. User programs can use interrupt 0xF0 to call kernel function.
That's easy, isn't it?

#### Building programs
You use GCC to compile programs. C++ is supported, but without exception support. 
Linker MUST compile program to BINARY. Plus you MUST compile & link additional code 
with your app. I call it 'librt'. It stored in SDK 'librt' folder. It implements 
some C/C++ wrappers and most of libc. When you link entire program, you can put it 
to wdOS system drive and run it using 'run' command.

#### What's inside?
libc and wdOS API. Well... part of libc. Multithreading is not currently supported
but Cosmos may get it's support soon. At now, only one app can run at a time. 
When app executes another app, executor gets suspended and resumes only after
executed one terminates and returns exit code.

#### Couldn't you just use bytecode language?
Becuase creating JIT compiler is too complicated and creating a simple interpreter
will result in awful performance.