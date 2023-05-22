#include "../wdOS.SDK/include/wdos.hpp"
using namespace wdOS;

int main() {
	UserConsole::Write("hello world!");
	return 0;
}

int _start() {
	return main();
}