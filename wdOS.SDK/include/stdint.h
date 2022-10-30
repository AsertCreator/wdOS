/*
* wdOS SDK v1 - stdint.h file
* Part of wdOS's implementation of libc
* Contains integer types
*/
#ifndef STDINT_H
#define STDINT_H
#include "limits.h"

typedef signed long long int64_t;
typedef signed int int32_t;
typedef signed short int16_t;
typedef signed char int8_t;

typedef unsigned long long uint64_t;
typedef unsigned int uint32_t;
typedef unsigned short uint16_t;
typedef unsigned char uint8_t;

typedef int64_t* intptr_t;
typedef int64_t intmax_t;
typedef int8_t int_least8_t;
typedef int8_t int_fast8_t;

typedef uint64_t* uintptr_t;
typedef uint64_t uintmax_t;
typedef uint8_t uint_least8_t;
typedef uint8_t uint_fast8_t;

#endif