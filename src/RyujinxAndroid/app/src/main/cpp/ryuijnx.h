//
// Created by Emmanuel Hansen on 6/19/2023.
//

#ifndef RYUJINXNATIVE_RYUIJNX_H
#define RYUJINXNATIVE_RYUIJNX_H

#include <stdlib.h>
#include <dlfcn.h>
#include <string.h>
#include <string>
#include <jni.h>
#include <exception>
#include <android/log.h>
#include <android/native_window.h>
#include <android/native_window_jni.h>
#include "vulkan_wrapper.h"
#include <vulkan/vulkan_android.h>
#include <cassert>
#include <fcntl.h>
#include "adrenotools/driver.h"
#include "native_window.h"

// A macro to pass call to Vulkan and check for return value for success
#define CALL_VK(func)                                                 \
  if (VK_SUCCESS != (func)) {                                         \
    __android_log_print(ANDROID_LOG_ERROR, "Tutorial ",               \
                        "Vulkan error. File[%s], line[%d]", __FILE__, \
                        __LINE__);                                    \
    assert(false);                                                    \
  }

// A macro to check value is VK_SUCCESS
// Used also for non-vulkan functions but return VK_SUCCESS
#define VK_CHECK(x)  CALL_VK(x)

#define LoadLib(a) dlopen(a, RTLD_NOW)

void *_ryujinxNative = NULL;

// Ryujinx imported functions
bool (*initialize)(char *) = NULL;

long _renderingThreadId = 0;
JavaVM *_vm = nullptr;
jobject _mainActivity = nullptr;
jclass _mainActivityClass = nullptr;

#endif //RYUJINXNATIVE_RYUIJNX_H
