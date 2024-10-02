// Write C++ code here.
//
// Do not forget to dynamically load the C++ library into your application.
//
// For instance,
//
// In MainActivity.java:
//    static {
//       System.loadLibrary("ryuijnx");
//    }
//
// Or, in MainActivity.kt:
//    companion object {
//      init {
//         System.loadLibrary("ryuijnx")
//      }
//    }

#include "ryuijnx.h"
#include "pthread.h"
#include <chrono>
#include <csignal>


std::chrono::time_point<std::chrono::steady_clock, std::chrono::nanoseconds> _currentTimePoint;

extern "C"
{
JNIEXPORT jlong JNICALL
Java_org_ryujinx_android_NativeHelpers_getNativeWindow(
        JNIEnv *env,
        jobject instance,
        jobject surface) {
    auto nativeWindow = ANativeWindow_fromSurface(env, surface);
    return nativeWindow == NULL ? -1 : (jlong) nativeWindow;
}

JNIEXPORT void JNICALL
Java_org_ryujinx_android_NativeHelpers_releaseNativeWindow(
        JNIEnv *env,
        jobject instance,
        jlong window) {
    auto nativeWindow = (ANativeWindow *) window;

    if (nativeWindow != NULL)
        ANativeWindow_release(nativeWindow);
}

long createSurface(long native_surface, long instance) {
    auto nativeWindow = (ANativeWindow *) native_surface;
    VkSurfaceKHR surface;
    auto vkInstance = (VkInstance) instance;
    auto fpCreateAndroidSurfaceKHR =
            reinterpret_cast<PFN_vkCreateAndroidSurfaceKHR>(vkGetInstanceProcAddr(vkInstance,
                                                                                  "vkCreateAndroidSurfaceKHR"));
    if (!fpCreateAndroidSurfaceKHR)
        return -1;
    VkAndroidSurfaceCreateInfoKHR info = {VK_STRUCTURE_TYPE_ANDROID_SURFACE_CREATE_INFO_KHR};
    info.window = nativeWindow;
    VK_CHECK(fpCreateAndroidSurfaceKHR(vkInstance, &info, nullptr, &surface));
    return (long) surface;
}

JNIEXPORT jlong JNICALL
Java_org_ryujinx_android_NativeHelpers_getCreateSurfacePtr(
        JNIEnv *env,
        jobject instance) {
    return (jlong) createSurface;
}

char *getStringPointer(
        JNIEnv *env,
        jstring jS) {
    const char *cparam = env->GetStringUTFChars(jS, 0);
    auto len = env->GetStringUTFLength(jS);
    char *s = new char[len];
    strcpy(s, cparam);
    env->ReleaseStringUTFChars(jS, cparam);

    return s;
}

jstring createString(
        JNIEnv *env,
        char *ch) {
    auto str = env->NewStringUTF(ch);

    return str;
}

jstring createStringFromStdString(
        JNIEnv *env,
        std::string s) {
    auto str = env->NewStringUTF(s.c_str());

    return str;
}


}
extern "C"
void setRenderingThread() {
    auto currentId = pthread_self();

    _renderingThreadId = currentId;

    _currentTimePoint = std::chrono::high_resolution_clock::now();
}
extern "C"
JNIEXPORT void JNICALL
Java_org_ryujinx_android_MainActivity_initVm(JNIEnv *env, jobject thiz) {
    JavaVM *vm = nullptr;
    auto success = env->GetJavaVM(&vm);
    _vm = vm;
    _mainActivity = thiz;
    _mainActivityClass = env->GetObjectClass(thiz);
}

bool isInitialOrientationFlipped = true;

extern "C"
void setCurrentTransform(long native_window, int transform) {
    if (native_window == 0 || native_window == -1)
        return;
    auto nativeWindow = (ANativeWindow *) native_window;

    auto nativeTransform = ANativeWindowTransform::ANATIVEWINDOW_TRANSFORM_IDENTITY;

    transform = transform >> 1;

    // transform is a valid VkSurfaceTransformFlagBitsKHR
    switch (transform) {
        case 0x1:
            nativeTransform = ANativeWindowTransform::ANATIVEWINDOW_TRANSFORM_IDENTITY;
            break;
        case 0x2:
            nativeTransform = ANativeWindowTransform::ANATIVEWINDOW_TRANSFORM_ROTATE_90;
            break;
        case 0x4:
            nativeTransform = isInitialOrientationFlipped
                              ? ANativeWindowTransform::ANATIVEWINDOW_TRANSFORM_IDENTITY
                              : ANativeWindowTransform::ANATIVEWINDOW_TRANSFORM_ROTATE_180;
            break;
        case 0x8:
            nativeTransform = ANativeWindowTransform::ANATIVEWINDOW_TRANSFORM_ROTATE_270;
            break;
        case 0x10:
            nativeTransform = ANativeWindowTransform::ANATIVEWINDOW_TRANSFORM_MIRROR_HORIZONTAL;
            break;
        case 0x20:
            nativeTransform = static_cast<ANativeWindowTransform>(
                    ANativeWindowTransform::ANATIVEWINDOW_TRANSFORM_MIRROR_HORIZONTAL |
                    ANATIVEWINDOW_TRANSFORM_ROTATE_90);
            break;
        case 0x40:
            nativeTransform = ANativeWindowTransform::ANATIVEWINDOW_TRANSFORM_MIRROR_VERTICAL;
            break;
        case 0x80:
            nativeTransform = static_cast<ANativeWindowTransform>(
                    ANativeWindowTransform::ANATIVEWINDOW_TRANSFORM_MIRROR_VERTICAL |
                    ANATIVEWINDOW_TRANSFORM_ROTATE_90);
            break;
        case 0x100:
            nativeTransform = ANativeWindowTransform::ANATIVEWINDOW_TRANSFORM_IDENTITY;
            break;
    }

    nativeWindow->perform(nativeWindow, NATIVE_WINDOW_SET_BUFFERS_TRANSFORM,
                          static_cast<int32_t>(nativeTransform));
}

extern "C"
JNIEXPORT jlong JNICALL
Java_org_ryujinx_android_NativeHelpers_loadDriver(JNIEnv *env, jobject thiz,
                                                  jstring native_lib_path,
                                                  jstring private_apps_path,
                                                  jstring driver_name) {
    auto libPath = getStringPointer(env, native_lib_path);
    auto privateAppsPath = getStringPointer(env, private_apps_path);
    auto driverName = getStringPointer(env, driver_name);

    auto handle = adrenotools_open_libvulkan(
            RTLD_NOW,
            ADRENOTOOLS_DRIVER_CUSTOM,
            nullptr,
            libPath,
            privateAppsPath,
            driverName,
            nullptr,
            nullptr
    );

    delete libPath;
    delete privateAppsPath;
    delete driverName;

    return (jlong) handle;
}

extern "C"
void debug_break(int code) {
    if (code >= 3)
        int r = 0;
}

extern "C"
JNIEXPORT void JNICALL
Java_org_ryujinx_android_NativeHelpers_setTurboMode(JNIEnv *env, jobject thiz, jboolean enable) {
    adrenotools_set_turbo(enable);
}

extern "C"
JNIEXPORT jint JNICALL
Java_org_ryujinx_android_NativeHelpers_getMaxSwapInterval(JNIEnv *env, jobject thiz,
                                                          jlong native_window) {
    auto nativeWindow = (ANativeWindow *) native_window;

    return nativeWindow->maxSwapInterval;
}

extern "C"
JNIEXPORT jint JNICALL
Java_org_ryujinx_android_NativeHelpers_getMinSwapInterval(JNIEnv *env, jobject thiz,
                                                          jlong native_window) {
    auto nativeWindow = (ANativeWindow *) native_window;

    return nativeWindow->minSwapInterval;
}

extern "C"
JNIEXPORT jint JNICALL
Java_org_ryujinx_android_NativeHelpers_setSwapInterval(JNIEnv *env, jobject thiz,
                                                       jlong native_window, jint swap_interval) {
    auto nativeWindow = (ANativeWindow *) native_window;

    return nativeWindow->setSwapInterval(nativeWindow, swap_interval);
}

extern "C"
JNIEXPORT jstring JNICALL
Java_org_ryujinx_android_NativeHelpers_getStringJava(JNIEnv *env, jobject thiz, jlong ptr) {
    return createString(env, (char*)ptr);
}

extern "C"
JNIEXPORT void JNICALL
Java_org_ryujinx_android_NativeHelpers_setIsInitialOrientationFlipped(JNIEnv *env, jobject thiz,
                                                                      jboolean is_flipped) {
    isInitialOrientationFlipped = is_flipped;
}
