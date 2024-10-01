#include <stddef.h>
#include <string.h>
#include <pthread.h>

#include <libkern/OSCacheControl.h>

void armeilleure_jit_memcpy(void *dst, const void *src, size_t n) {
    pthread_jit_write_protect_np(0);
    memcpy(dst, src, n);
    pthread_jit_write_protect_np(1);

    // Ensure that the instruction cache for this range is invalidated.
    sys_icache_invalidate(dst, n);
}
