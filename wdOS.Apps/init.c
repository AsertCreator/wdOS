#include <wdos.h>

int main() {
	wd_user_io_write(STDOUT, "hello world!", 0, 13);
	return 0;
}