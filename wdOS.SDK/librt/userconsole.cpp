#include "../include/wdos.hpp"
using namespace wdOS;

ushort UserConsole::_tableid = 0xFF04;

uint UserConsole::GetCursorX() {
	return PlatformSyscall(_tableid, 0, 0);
}
uint UserConsole::GetCursorY() {
	return PlatformSyscall(_tableid, 1, 0);
}
uint UserConsole::GetConsoleForeground() {
	return PlatformSyscall(_tableid, 2, 0);
}
uint UserConsole::GetConsoleBackground() {
	return PlatformSyscall(_tableid, 3, 0);
}
void UserConsole::SetConsoleForeground(uint val) {
	PlatformSyscall(_tableid, 4, val);
}
void UserConsole::SetConsoleBackground(uint val) {
	PlatformSyscall(_tableid, 5, val);
}
void UserConsole::SetCursorX(uint val) {
	PlatformSyscall(_tableid, 6, val);
}
void UserConsole::SetCursorY(uint val) {
	PlatformSyscall(_tableid, 7, val);
}
void UserConsole::Write(char* buffer) {
	PlatformSyscall(_tableid, 8, (uint)buffer);
}
uint UserConsole::Read(char* buffer) {
	return PlatformSyscall(_tableid, 9, (uint)buffer);
}