#include "../include/wdos.hpp"
using namespace wdOS;

ushort ProcessRuntime::_tableid = 0xFF03;

int ProcessRuntime::Execute(char* path, char* cmd) {
	ExecuteInfo info = { };
	info.ExecutablePath = path;
	info.ConsoleArguments = cmd;
	return PlatformSyscall(_tableid, 0, (uint)&info);
}
void* ProcessRuntime::Alloc(uint sz) {
	return (void*)PlatformSyscall(_tableid, 1, sz);
}
void ProcessRuntime::Free(void* vd) {
	PlatformSyscall(_tableid, 2, (uint)vd);
}
char* ProcessRuntime::GetConsoleArguments() {
	return (char*)PlatformSyscall(_tableid, 3, 0);
}