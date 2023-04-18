/*
* wdOS SDK v1 - wdos.h file
* Not part of wdOS's implementation of libc
* Contains crucial elements of wdOS API
*/
#include "../include/wdos.h"

// Executes certain program. Raises MSG_CONTROL_EXEC message
int wd_user_exec(const char* path, const char* args, bool sibling) {
	exec_info info;
	info.args = args;
	info.path = path;
	info.sibling = sibling;
	return wd_user_msg_send(MSG_CONTROL_EXEC, &info);
}