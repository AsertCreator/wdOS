/*
* wdOS SDK v1 - companion.c file
* Not part of wdOS's implementation of libc
* Contains implemention of certain elements of wdOS API
*/
#include "wdos.h"

// Sends a message to operating system, to initate some event
static uint32_t SendMessage(uint16_t msg, uint32_t arg0, uint32_t arg1, uint32_t arg2) {
#ifdef __GNUC__
	int result = 0;
	register int eax asm("eax");
	register int ebx asm("ebx");
	register int ecx asm("ecx");
	register int edx asm("edx");
	eax = msg;
	ebx = arg0;
	ecx = arg1;
	edx = arg2;
	// interrupt
	result = eax;
	return result;
#else
	return INT_MIN; //You must use GCC to compile
#endif
}

// Waits until mutex is unlocked, if its not. Then it locks a mutex
static void LockMutex(PMUTEX mutex) {
	while (mutex->lock);
	mutex->lock = true;
}
// Unlocks a mutex, to allow another programs/threads to use object
static void UnlockMutex(PMUTEX mutex) {
	mutex->lock = false;
}

// Allocates memory from RAM. Raises MSG_MEMORY_ALLOC message
static HANDLE AllocMemory(size_t size) {
	return (HANDLE)SendMessage(MSG_MEMORY_ALLOC, size , 0, 0);
}
// Frees allocated memory from RAM. Raises MSG_MEMORY_FREE message
static void FreeMemory(HANDLE memory) {
	SendMessage(MSG_MEMORY_FREE, (uint32_t)memory, 0, 0);
}

// Sets PIT chip frequency on meotherboard that raises IRQ 0. Raises MSG_CONTROL_PIT message
static void SetPITFrequency(int freq) {
	SendMessage(MSG_CONTROL_PIT, (uint32_t)freq, 0, 0);
}
// Gets current UNIX Timestamp. Raises MSG_CONTROL_GETTIME message
static TIME GetUNIXTime() {
	return SendMessage(MSG_CONTROL_GETTIME, 0, 0, 0);
}
// Sets current UNIX Timestamp. Raises MSG_CONTROL_SETTIME message
static void SetUNIXTime(TIME t) {
	SendMessage(MSG_CONTROL_SETTIME, t, 0, 0);
}

// Sets interrupt handler to IDT. Raises MSG_CONTROL_SETINT message
static void SetInterruptHandler(uint8_t interrupt, HANDLE handler) {
	SendMessage(MSG_CONTROL_SETINT, interrupt, handler, 0);
}
// Gets interrupt handler from IDT. Raises MSG_CONTROL_GETINT message
static HANDLE GetInterruptHandler(uint8_t interrupt) {
	return SendMessage(MSG_CONTROL_GETINT, interrupt, 0, 0);
}