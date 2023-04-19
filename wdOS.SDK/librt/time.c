/*
* wdOS SDK v1 - time.c file
* Not part of wdOS's implementation of libc
* Contains implemention of certain elements of wdOS API
*/
#include "../include/wdos.h"

// Gets current UNIX Timestamp. Raises MSG_CONTROL_GETTIME message
time_t wd_user_time_get() {
	return wd_user_msg_send(MSG_CONTROL_GETTIME, 0);
}
// Sets current UNIX Timestamp. Raises MSG_CONTROL_SETTIME message
void wd_user_time_set(time_t t) {
	wd_user_msg_send(MSG_CONTROL_SETTIME, t);
}