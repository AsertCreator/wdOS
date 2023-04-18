/*
* wdOS SDK v1 - alloc.c file
* Not part of wdOS's implementation of libc
* Contains implemention of certain elements of wdOS API
*/
#include "../include/wdos.h"

// Allocates memory from RAM. Raises MSG_MEMORY_ALLOC message
void* wd_user_mem_alloc(size_t size) {
	return (void*)wd_user_msg_send(MSG_MEMORY_ALLOC, size, 0, 0);
}
// Frees allocated memory from RAM. Raises MSG_MEMORY_FREE message
void wd_user_mem_free(void* memory) {
	wd_user_msg_send(MSG_MEMORY_FREE, (uint32_t)memory, 0, 0);
}