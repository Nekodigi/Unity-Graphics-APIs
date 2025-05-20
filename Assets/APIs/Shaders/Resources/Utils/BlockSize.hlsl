#ifndef BLOCKSIZE_INCLUDED
#define BLOCKSIZE_INCLUDED

#if defined(BLOCK_1)
#define BLOCK_SIZE 1
#elif defined(BLOCK_2)
#define BLOCK_SIZE 4
#elif defined(BLOCK_16)
#define BLOCK_SIZE 16
#elif defined(BLOCK_64)
#define BLOCK_SIZE 64
#elif defined(BLOCK_256)
#define BLOCK_SIZE 256
#elif defined(BLOCK_1024)
#define BLOCK_SIZE 1024
#else
#define BLOCK_SIZE 64
#endif

#endif
