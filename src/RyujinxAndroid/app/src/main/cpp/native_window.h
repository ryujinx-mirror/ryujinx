// SPDX-License-Identifier: MPL-2.0
// Copyright © 2021 Skyline Team and Contributors (https://github.com/skyline-emu/)
// Copyright © 2021 The Android Open Source Project

#pragma once

/* A collection of various types from AOSP that allow us to access private APIs for Native Window which we utilize for emulating the guest SF more accurately */

/**
 * @url https://cs.android.com/android/platform/superproject/+/android11-release:frameworks/native/libs/nativebase/include/nativebase/nativebase.h;l=29;drc=cb496acbe593326e8d5d563847067d02b2df40ec
 */
#define ANDROID_NATIVE_UNSIGNED_CAST(x) static_cast<unsigned int>(x)

/**
 * @url https://cs.android.com/android/platform/superproject/+/android11-release:frameworks/native/libs/nativebase/include/nativebase/nativebase.h;l=34-38;drc=cb496acbe593326e8d5d563847067d02b2df40ec
 */
#define ANDROID_NATIVE_MAKE_CONSTANT(a, b, c, d)  \
    ((ANDROID_NATIVE_UNSIGNED_CAST(a) << 24) | \
     (ANDROID_NATIVE_UNSIGNED_CAST(b) << 16) | \
     (ANDROID_NATIVE_UNSIGNED_CAST(c) <<  8) | \
     (ANDROID_NATIVE_UNSIGNED_CAST(d)))

/**
 * @url https://cs.android.com/android/platform/superproject/+/android11-release:frameworks/native/libs/nativewindow/include/system/window.h;l=60;drc=401cda638e7d17f6697b5a65c9a5ad79d056202d
 */
#define ANDROID_NATIVE_WINDOW_MAGIC     ANDROID_NATIVE_MAKE_CONSTANT('_','w','n','d')

constexpr int AndroidNativeWindowMagic{ANDROID_NATIVE_WINDOW_MAGIC};

#undef ANDROID_NATIVE_WINDOW_MAGIC
#undef ANDROID_NATIVE_MAKE_CONSTANT
#undef ANDROID_NATIVE_UNSIGNED_CAST

/**
 * @url https://cs.android.com/android/platform/superproject/+/android11-release:frameworks/native/libs/nativewindow/include/system/window.h;l=325-331;drc=401cda638e7d17f6697b5a65c9a5ad79d056202d
 */
constexpr int64_t NativeWindowTimestampAuto{-9223372036854775807LL - 1};

/**
 * @url https://cs.android.com/android/platform/superproject/+/android11-release:frameworks/native/libs/nativewindow/include/system/window.h;l=198-259;drc=401cda638e7d17f6697b5a65c9a5ad79d056202d
 */
enum {
    NATIVE_WINDOW_CONNECT = 1,   /* deprecated */
    NATIVE_WINDOW_DISCONNECT = 2,   /* deprecated */
    NATIVE_WINDOW_SET_CROP = 3,   /* private */
    NATIVE_WINDOW_SET_BUFFER_COUNT = 4,
    NATIVE_WINDOW_SET_BUFFERS_TRANSFORM = 6,
    NATIVE_WINDOW_SET_BUFFERS_TIMESTAMP = 7,
    NATIVE_WINDOW_SET_BUFFERS_DIMENSIONS = 8,
    NATIVE_WINDOW_SET_SCALING_MODE = 10,   /* private */
    NATIVE_WINDOW_LOCK = 11,   /* private */
    NATIVE_WINDOW_UNLOCK_AND_POST = 12,   /* private */
    NATIVE_WINDOW_API_CONNECT = 13,   /* private */
    NATIVE_WINDOW_API_DISCONNECT = 14,   /* private */
    NATIVE_WINDOW_SET_BUFFERS_USER_DIMENSIONS = 15,   /* private */
    NATIVE_WINDOW_SET_POST_TRANSFORM_CROP = 16,   /* deprecated, unimplemented */
    NATIVE_WINDOW_SET_BUFFERS_STICKY_TRANSFORM = 17,   /* private */
    NATIVE_WINDOW_SET_SIDEBAND_STREAM = 18,
    NATIVE_WINDOW_SET_BUFFERS_DATASPACE = 19,
    NATIVE_WINDOW_SET_SURFACE_DAMAGE = 20,   /* private */
    NATIVE_WINDOW_SET_SHARED_BUFFER_MODE = 21,
    NATIVE_WINDOW_SET_AUTO_REFRESH = 22,
    NATIVE_WINDOW_GET_REFRESH_CYCLE_DURATION = 23,
    NATIVE_WINDOW_GET_NEXT_FRAME_ID = 24,
    NATIVE_WINDOW_ENABLE_FRAME_TIMESTAMPS = 25,
    NATIVE_WINDOW_GET_COMPOSITOR_TIMING = 26,
    NATIVE_WINDOW_GET_FRAME_TIMESTAMPS = 27,
    NATIVE_WINDOW_GET_WIDE_COLOR_SUPPORT = 28,
    NATIVE_WINDOW_GET_HDR_SUPPORT = 29,
    NATIVE_WINDOW_GET_CONSUMER_USAGE64 = 31,
    NATIVE_WINDOW_SET_BUFFERS_SMPTE2086_METADATA = 32,
    NATIVE_WINDOW_SET_BUFFERS_CTA861_3_METADATA = 33,
    NATIVE_WINDOW_SET_BUFFERS_HDR10_PLUS_METADATA = 34,
    NATIVE_WINDOW_SET_AUTO_PREROTATION = 35,
    NATIVE_WINDOW_GET_LAST_DEQUEUE_START = 36,    /* private */
    NATIVE_WINDOW_SET_DEQUEUE_TIMEOUT = 37,    /* private */
    NATIVE_WINDOW_GET_LAST_DEQUEUE_DURATION = 38,    /* private */
    NATIVE_WINDOW_GET_LAST_QUEUE_DURATION = 39,    /* private */
    NATIVE_WINDOW_SET_FRAME_RATE = 40,
    NATIVE_WINDOW_SET_CANCEL_INTERCEPTOR = 41,    /* private */
    NATIVE_WINDOW_SET_DEQUEUE_INTERCEPTOR = 42,    /* private */
    NATIVE_WINDOW_SET_PERFORM_INTERCEPTOR = 43,    /* private */
    NATIVE_WINDOW_SET_QUEUE_INTERCEPTOR = 44,    /* private */
    NATIVE_WINDOW_ALLOCATE_BUFFERS = 45,    /* private */
    NATIVE_WINDOW_GET_LAST_QUEUED_BUFFER = 46,    /* private */
    NATIVE_WINDOW_SET_QUERY_INTERCEPTOR = 47,    /* private */
    NATIVE_WINDOW_GET_LAST_QUEUED_BUFFER2 = 50,    /* private */
};

/**
 * @url https://cs.android.com/android/platform/superproject/+/android11-release:frameworks/native/libs/nativebase/include/nativebase/nativebase.h;l=43-56;drc=cb496acbe593326e8d5d563847067d02b2df40ec
 */
struct android_native_base_t {
    int magic;
    int version;
    void *reserved[4];

    void (*incRef)(android_native_base_t *);

    void (*decRef)(android_native_base_t *);
};

/**
 * @url https://cs.android.com/android/platform/superproject/+/android11-release:frameworks/native/libs/nativewindow/include/system/window.h;l=341-560;drc=401cda638e7d17f6697b5a65c9a5ad79d056202d
 */
struct ANativeWindow {
    struct android_native_base_t common;

    /* flags describing some attributes of this surface or its updater */
    const uint32_t flags;

    /* min swap interval supported by this updated */
    const int minSwapInterval;

    /* max swap interval supported by this updated */
    const int maxSwapInterval;

    /* horizontal and vertical resolution in DPI */
    const float xdpi;
    const float ydpi;

    /* Some storage reserved for the OEM's driver. */
    intptr_t oem[4];

    /*
     * Set the swap interval for this surface.
     *
     * Returns 0 on success or -errno on error.
     */
    int (*setSwapInterval)(struct ANativeWindow *window,
                           int interval);

    /*
     * Hook called by EGL to acquire a buffer. After this call, the buffer
     * is not locked, so its content cannot be modified. This call may block if
     * no buffers are available.
     *
     * The window holds a reference to the buffer between dequeueBuffer and
     * either queueBuffer or cancelBuffer, so clients only need their own
     * reference if they might use the buffer after queueing or canceling it.
     * Holding a reference to a buffer after queueing or canceling it is only
     * allowed if a specific buffer count has been set.
     *
     * Returns 0 on success or -errno on error.
     *
     * XXX: This function is deprecated.  It will continue to work for some
     * time for binary compatibility, but the new dequeueBuffer function that
     * outputs a fence file descriptor should be used in its place.
     */
    int (*dequeueBuffer_DEPRECATED)(struct ANativeWindow *window,
                                    struct ANativeWindowBuffer **buffer);

    /*
     * hook called by EGL to lock a buffer. This MUST be called before modifying
     * the content of a buffer. The buffer must have been acquired with
     * dequeueBuffer first.
     *
     * Returns 0 on success or -errno on error.
     *
     * XXX: This function is deprecated.  It will continue to work for some
     * time for binary compatibility, but it is essentially a no-op, and calls
     * to it should be removed.
     */
    int (*lockBuffer_DEPRECATED)(struct ANativeWindow *window,
                                 struct ANativeWindowBuffer *buffer);

    /*
     * Hook called by EGL when modifications to the render buffer are done.
     * This unlocks and post the buffer.
     *
     * The window holds a reference to the buffer between dequeueBuffer and
     * either queueBuffer or cancelBuffer, so clients only need their own
     * reference if they might use the buffer after queueing or canceling it.
     * Holding a reference to a buffer after queueing or canceling it is only
     * allowed if a specific buffer count has been set.
     *
     * Buffers MUST be queued in the same order than they were dequeued.
     *
     * Returns 0 on success or -errno on error.
     *
     * XXX: This function is deprecated.  It will continue to work for some
     * time for binary compatibility, but the new queueBuffer function that
     * takes a fence file descriptor should be used in its place (pass a value
     * of -1 for the fence file descriptor if there is no valid one to pass).
     */
    int (*queueBuffer_DEPRECATED)(struct ANativeWindow *window,
                                  struct ANativeWindowBuffer *buffer);

    /*
     * hook used to retrieve information about the native window.
     *
     * Returns 0 on success or -errno on error.
     */
    int (*query)(const struct ANativeWindow *window,
                 int what, int *value);

    /*
     * hook used to perform various operations on the surface.
     * (*perform)() is a generic mechanism to add functionality to
     * ANativeWindow while keeping backward binary compatibility.
     *
     * DO NOT CALL THIS HOOK DIRECTLY.  Instead, use the helper functions
     * defined below.
     *
     * (*perform)() returns -ENOENT if the 'what' parameter is not supported
     * by the surface's implementation.
     *
     * See above for a list of valid operations, such as
     * NATIVE_WINDOW_SET_USAGE or NATIVE_WINDOW_CONNECT
     */
    int (*perform)(struct ANativeWindow *window,
                   int operation, ...);

    /*
     * Hook used to cancel a buffer that has been dequeued.
     * No synchronization is performed between dequeue() and cancel(), so
     * either external synchronization is needed, or these functions must be
     * called from the same thread.
     *
     * The window holds a reference to the buffer between dequeueBuffer and
     * either queueBuffer or cancelBuffer, so clients only need their own
     * reference if they might use the buffer after queueing or canceling it.
     * Holding a reference to a buffer after queueing or canceling it is only
     * allowed if a specific buffer count has been set.
     *
     * XXX: This function is deprecated.  It will continue to work for some
     * time for binary compatibility, but the new cancelBuffer function that
     * takes a fence file descriptor should be used in its place (pass a value
     * of -1 for the fence file descriptor if there is no valid one to pass).
     */
    int (*cancelBuffer_DEPRECATED)(struct ANativeWindow *window,
                                   struct ANativeWindowBuffer *buffer);

    /*
     * Hook called by EGL to acquire a buffer. This call may block if no
     * buffers are available.
     *
     * The window holds a reference to the buffer between dequeueBuffer and
     * either queueBuffer or cancelBuffer, so clients only need their own
     * reference if they might use the buffer after queueing or canceling it.
     * Holding a reference to a buffer after queueing or canceling it is only
     * allowed if a specific buffer count has been set.
     *
     * The libsync fence file descriptor returned in the int pointed to by the
     * fenceFd argument will refer to the fence that must signal before the
     * dequeued buffer may be written to.  A value of -1 indicates that the
     * caller may access the buffer immediately without waiting on a fence.  If
     * a valid file descriptor is returned (i.e. any value except -1) then the
     * caller is responsible for closing the file descriptor.
     *
     * Returns 0 on success or -errno on error.
     */
    int (*dequeueBuffer)(struct ANativeWindow *window,
                         struct ANativeWindowBuffer **buffer, int *fenceFd);

    /*
     * Hook called by EGL when modifications to the render buffer are done.
     * This unlocks and post the buffer.
     *
     * The window holds a reference to the buffer between dequeueBuffer and
     * either queueBuffer or cancelBuffer, so clients only need their own
     * reference if they might use the buffer after queueing or canceling it.
     * Holding a reference to a buffer after queueing or canceling it is only
     * allowed if a specific buffer count has been set.
     *
     * The fenceFd argument specifies a libsync fence file descriptor for a
     * fence that must signal before the buffer can be accessed.  If the buffer
     * can be accessed immediately then a value of -1 should be used.  The
     * caller must not use the file descriptor after it is passed to
     * queueBuffer, and the ANativeWindow implementation is responsible for
     * closing it.
     *
     * Returns 0 on success or -errno on error.
     */
    int (*queueBuffer)(struct ANativeWindow *window,
                       struct ANativeWindowBuffer *buffer, int fenceFd);

    /*
     * Hook used to cancel a buffer that has been dequeued.
     * No synchronization is performed between dequeue() and cancel(), so
     * either external synchronization is needed, or these functions must be
     * called from the same thread.
     *
     * The window holds a reference to the buffer between dequeueBuffer and
     * either queueBuffer or cancelBuffer, so clients only need their own
     * reference if they might use the buffer after queueing or canceling it.
     * Holding a reference to a buffer after queueing or canceling it is only
     * allowed if a specific buffer count has been set.
     *
     * The fenceFd argument specifies a libsync fence file decsriptor for a
     * fence that must signal before the buffer can be accessed.  If the buffer
     * can be accessed immediately then a value of -1 should be used.
     *
     * Note that if the client has not waited on the fence that was returned
     * from dequeueBuffer, that same fence should be passed to cancelBuffer to
     * ensure that future uses of the buffer are preceded by a wait on that
     * fence.  The caller must not use the file descriptor after it is passed
     * to cancelBuffer, and the ANativeWindow implementation is responsible for
     * closing it.
     *
     * Returns 0 on success or -errno on error.
     */
    int (*cancelBuffer)(struct ANativeWindow *window,
                        struct ANativeWindowBuffer *buffer, int fenceFd);
};
