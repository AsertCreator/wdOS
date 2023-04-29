#include "../include/wdos.hpp"
using namespace wdOS;

uint wdOS::PlatformSyscall(ushort com, ushort func, uint arg) {
	uint id = (uint)com | ((uint)func >> 16);
	uint res = 0;
	register uint* eax asm("eax");
	register uint* ecx asm("ecx");
	*eax = id;
	*ecx = arg;
	asm volatile ("int $0xF0");
	res = *eax;
	return res;
}