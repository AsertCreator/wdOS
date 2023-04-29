#include "../wdOS.SDK/include/wdos.hpp"
using namespace wdOS;

int main();

int _start() {
	return main();
}

int main() {
	UserConsole::Write(ProcessRuntime::GetConsoleArguments());
	return 0;
}