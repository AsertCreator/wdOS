/*
* wdOS SDK v1 - msg.c file
* Not part of wdOS's implementation of libc
* Contains implemention of certain elements of wdOS API
*/
#include "../include/wdos.h"

// Writes string to a certain file. Raises MSG_IO_WRITE message
int wd_user_io_write(int fd, char* ch, int ix, int sz) {
	io_info info;
	info.buffer = ch;
	info.fd = fd;
	info.ix = ix;
	info.sz = sz;
	return wd_user_msg_send(MSG_IO_WRITE, &info);
}
// Reads string from a certain file. Raises MSG_IO_READ message
void wd_user_io_read(int fd, char* bf, int ix, int sz) {
	io_info info;
	info.buffer = ch;
	info.fd = fd;
	info.ix = ix;
	info.sz = sz;
	wd_user_msg_send(MSG_IO_READ, &info);
}