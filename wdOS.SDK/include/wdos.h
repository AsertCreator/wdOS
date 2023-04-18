/*
* wdOS SDK v1 - wdos.h file
* Not part of wdOS's implementation of libc
* Contains crucial elements of wdOS API
*/
#ifndef WDOS_H
#define WDOS_H
#include "stddef.h"
#include "limits.h"
#include "errno.h" 

#define TERMINAL_COLOR_BLACK 0
#define TERMINAL_COLOR_BLUE 1
#define TERMINAL_COLOR_GREEN 2
#define TERMINAL_COLOR_CYAN 3
#define TERMINAL_COLOR_RED 4
#define TERMINAL_COLOR_MAGENTA 5
#define TERMINAL_COLOR_BROWN 6
#define TERMINAL_COLOR_LIGHT_GREY 7
#define TERMINAL_COLOR_DARK_GREY 8
#define TERMINAL_COLOR_LIGHT_BLUE 9
#define TERMINAL_COLOR_LIGHT_GREEN 10
#define TERMINAL_COLOR_LIGHT_CYAN 11
#define TERMINAL_COLOR_LIGHT_RED 12
#define TERMINAL_COLOR_LIGHT_MAGENTA 13
#define TERMINAL_COLOR_LIGHT_BROWN 14
#define TERMINAL_COLOR_WHITE 15

#define STDOUT -1
#define STDERR -2
#define STDIN -3

// Writes string buffer into file. This is system message, not function!
#define MSG_IO_WRITE 10 
// Reads string buffer from file. This is system message, not function!
#define MSG_IO_READ 11 
// Allocate area in RAM. This is system message, not function!
#define MSG_MEMORY_ALLOC 20 
// Free allocated area in RAM. This is system message, not function!
#define MSG_MEMORY_FREE 21 
// Terminate this program. This is system message, not function!
#define MSG_CONTROL_STOP 30
// Set UNIX timestamp. This is system message, not function!
#define MSG_CONTROL_SETTIME 31 
// Get UNIX timestamp. This is system message, not function!
#define MSG_CONTROL_GETTIME 32
// Executes certain program. This is system message, not function!
#define MSG_CONTROL_EXEC 33

typedef uint32_t time_t; // Represents a UNIX timestamp a.k.a count of seconds sinse 1 Jan. 1970 

typedef struct {
	char* buffer;
	int ix;
	int sz;
	int fd;
} io_info;

typedef struct {
	const char* path;
	const char* args;
	bool sibling;
} exec_info;

// Sends a message to operating system, to initate some event
uint32_t wd_user_msg_send(uint16_t msg, uint32_t arg0);

// Writes string to a certain file. Raises MSG_IO_WRITE message
int wd_user_io_write(int fd, char* ch, int ix, int sz);
// Reads string from a certain file. Raises MSG_IO_READ message
void wd_user_io_read(int fd, char* bf, int ix, int sz);

// Allocates memory from RAM. Raises MSG_MEMORY_ALLOC message
void* wd_user_mem_alloc(size_t size);
// Frees allocated memory from RAM. Raises MSG_MEMORY_FREE message
void wd_user_mem_free(void* memory);

// Gets current UNIX Timestamp. Raises MSG_CONTROL_GETTIME message
time_t wd_user_time_get();
// Sets current UNIX Timestamp. Raises MSG_CONTROL_SETTIME message
void wd_user_time_set(time_t t);

// Executes certain program. Raises MSG_CONTROL_EXEC message
int wd_user_exec(const char* path, const char* args, bool sibling);

#endif