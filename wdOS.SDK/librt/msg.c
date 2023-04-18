/*
* wdOS SDK v1 - msg.c file
* Not part of wdOS's implementation of libc
* Contains implemention of certain elements of wdOS API
*/
#include "../include/wdos.h"

// Sends a message to operating system, to initate some event
uint32_t wd_user_msg_send(uint16_t msg, uint32_t arg0) {
	int result = 0;
	register int eax asm("eax");
	register int ecx asm("ecx");
	eax = msg;
	ecx = arg0;
	asm volatile ("int $0xF0");
	result = eax;
	return result;
}