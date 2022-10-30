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

// Write char to next free position in terminal. This is system message, not function!
#define MSG_CONSOLE_WRITECHAR 0x00 
// Write line without line terminator to next free position in terminal. This is system message, not function!
#define MSG_CONSOLE_WRITELINE 0x01 
// Read char from terminal and returns it. This is system message, not function!
#define MSG_CONSOLE_READCHAR 0x02 
// Read line from terminal and returns it. This is system message, not function!
#define MSG_CONSOLE_READLINE 0x03 
// Put char at specified position in terminal. This is system message, not function!
#define MSG_CONSOLE_PUTCHARAT 0x04 
// Clear terminal a.k.a. puts space in every terminal char. This is system message, not function!
#define MSG_CONSOLE_CLEARSCR 0x05 
// Set foreground color in terminal. This is system message, not function!
#define MSG_CONSOLE_SETFORE 0x06 
// Set background color in terminal. This is system message, not function!
#define MSG_CONSOLE_SETBACK 0x07 
// Get foreground color in terminal. This is system message, not function!
#define MSG_CONSOLE_GETFORE 0x08 
// Get background color in terminal. This is system message, not function!
#define MSG_CONSOLE_GETBACK 0x09 
// Allocate area in RAM. This is system message, not function!
#define MSG_MEMORY_ALLOC 0x10 
// Free allocated area in RAM. This is system message, not function!
#define MSG_MEMORY_FREE 0x11 
// Terminate this program. This is system message, not function!
#define MSG_CONTROL_STOP 0x20 
// Control a PIT chip on motherboard. This is system message, not function!
#define MSG_CONTROL_PIT 0x21 
// Set interrupt in IDT. This is system message, not function!
#define MSG_CONTROL_SETINT 0x22 
// Get interrupt in IDT. This is system message, not function!
#define MSG_CONTROL_GETINT 0x23 
// Set UNIX timestamp. This is system message, not function!
#define MSG_CONTROL_SETTIME 0x24 
// Get UNIX timestamp. This is system message, not function!
#define MSG_CONTROL_GETTIME 0x25 

// Represents lockable boolean, that provides basic thread-safety
typedef struct {
	bool lock;	
} MUTEX, *PMUTEX;

typedef void* HANDLE; // Represents pointer to anything, whenever its a function or object
typedef uint32_t TIME; // Represents a UNIX timestamp a.k.a count of seconds sinse 1 Jan. 1970 

// Sends a message to operating system, to initate some event
static uint32_t SendMessage(uint16_t msg, uint32_t arg0, uint32_t arg1, uint32_t arg2);

// Waits until mutex is unlocked, if its not. Then it locks a mutex
static void LockMutex(PMUTEX mutex);
// Unlocks a mutex, to allow another programs/threads to use object
static void UnlockMutex(PMUTEX mutex);

// Allocates memory from RAM. Raises MSG_MEMORY_ALLOC message
static HANDLE AllocMemory(size_t size);
// Frees allocated memory from RAM. Raises MSG_MEMORY_FREE message
static void FreeMemory(HANDLE memory);

// Sets PIT chip frequency on meotherboard that raises IRQ 0. Raises MSG_CONTROL_PIT message
static void SetPITFrequency(int freq);
// Gets current UNIX Timestamp. Raises MSG_CONTROL_GETTIME message
static TIME GetUNIXTime();
// Sets current UNIX Timestamp. Raises MSG_CONTROL_SETTIME message
static void SetUNIXTime(TIME t);

// Sets interrupt handler to IDT. Raises MSG_CONTROL_SETINT message
static void SetInterruptHandler(uint8_t interrupt, HANDLE handler);
// Gets interrupt handler from IDT. Raises MSG_CONTROL_GETINT message
static HANDLE GetInterruptHandler(uint8_t interrupt);

#endif