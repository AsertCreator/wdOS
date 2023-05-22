# wdOS Platform API
This document explains how wdOS Platform API works and what functions
it does expose. This document is not appliable to any version of wdOS
Core OS.

## wdOS Platform Syscall Interface
wdOS Platform syscalls are based on interrupt 0xF0. Applications can
write certain information to certain registers and Platform will
act accordingly. These 'certain registers' are EAX and ECX. EAX will
contain kernel function identifier and ECX will contain arguments.
Note, that wdOS Platform excepts all arguemnts be in ECX, but some
functions require multiple arguments. For this to work, librt packs
multiple function arguments into one structure in memory and then
passes its address to ECX. Then, Platform unpacks it and passes control
to kernel function.

## Kernel Function Identitifer
Kernel Function Identitifer is stored in EAX register as said above.
It consists of two parts or two words: kernel function table identifier
and in-table function identifier.

## Kernel Function Table
Kernel Function Table contains kernel function wrappers that allows
native applications to run on it