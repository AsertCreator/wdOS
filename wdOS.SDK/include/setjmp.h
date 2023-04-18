// NOTE to copy-pasters: semantics differ slightly from standard C
typedef void* jmp_buf[10];
#define setjmp __builtin_setjmp
#define longjmp __builtin_longjmp