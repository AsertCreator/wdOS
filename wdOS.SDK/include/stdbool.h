/*
* wdOS SDK v1 - stdbool.h file
* Part of wdOS's implementation of libc
* Contains bool type definitions
*/
#ifndef STDBOOL_H
#define STDBOOL_H
#include "stddef.h"

#define bool uint8_t
#define true (uint8_t)1
#define false (uint8_t)0
#define __bool_true_false_are_defined 1

#endif