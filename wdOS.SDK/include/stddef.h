/*
* wdOS SDK v1 - stddef.h file
* Part of wdOS's implementation of libc
* Contains default type definitions
*/
#ifndef STDDEF_H
#define STDDEF_H
#include "stdint.h"
#include "stdbool.h"

typedef uint32_t size_t;
typedef uint32_t ptrdiff_t;

#define NULL 0
#define offsetof( st, m ) ( (size_t) (& ((st *)0)->m ) )

#endif