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

typedef uint8_t byte;
typedef uint16_t ushort;
typedef uint32_t uint;
typedef uint64_t ulong;
typedef int8_t sbyte;

namespace wdOS {
	enum ConsoleColor {
		Black = 0, Blue = 1, Green = 2, Cyan = 3,
		Red = 4, Magenta = 5, Brown = 6, LightGrey = 7,
		DarkGrey = 8, LightBlue = 9, LightGreen = 10,
		LightCyan = 11, LightRed = 12, LightMagenta = 13,
		LightBrown = 14, White = 15,
	};
	enum LogLevel {
		Info, Warning, Error, Fatal
	};

	typedef struct {
		char* ExecutablePath;
		char* ConsoleArguments;
	} ExecuteInfo;

	typedef struct {
		char* String;
		char* Component;
		byte LogLevel;
	} LogInfo;

	uint PlatformSyscall(ushort com, ushort func, uint arg);

	class UserConsole {
	public:
		static ushort _tableid;

		static uint GetCursorX();
		static uint GetCursorY();
		static uint GetConsoleForeground();
		static uint GetConsoleBackground();
		static void SetConsoleForeground(uint val);
		static void SetConsoleBackground(uint val);
		static void SetCursorX(uint val);
		static void SetCursorY(uint val);
		static void Write(const char* buffer);
		static void Write(char* buffer);
		static uint Read(char* buffer);
	};
	class ProcessRuntime {
	public:
		static ushort _tableid;

		static int Execute(char* path, char* cmd);
		static void* Alloc(uint sz);
		static void Free(void* vd);
		static char* GetConsoleArguments();
	};
}

#endif