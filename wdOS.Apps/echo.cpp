#include "../wdOS.SDK/include/wdos.hpp"
using namespace wdOS;

int main() {
	UserConsole::Write(ProcessRuntime::GetConsoleArguments());
	return 0;
}

int _start() {
	return main();
}