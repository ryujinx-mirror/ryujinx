package org.ryujinx.android

import com.sun.jna.JNIEnv
import com.sun.jna.Library
import com.sun.jna.Native
import org.ryujinx.android.viewmodels.GameInfo
import java.util.Collections

interface RyujinxNativeJna : Library {
    fun deviceInitialize(
        isHostMapped: Boolean, useNce: Boolean,
        systemLanguage: Int,
        regionCode: Int,
        enableVsync: Boolean,
        enableDockedMode: Boolean,
        enablePtc: Boolean,
        enableInternetAccess: Boolean,
        timeZone: String,
        ignoreMissingServices: Boolean
    ): Boolean

    fun graphicsInitialize(
        rescale: Float = 1f,
        maxAnisotropy: Float = 1f,
        fastGpuTime: Boolean = true,
        fast2DCopy: Boolean = true,
        enableMacroJit: Boolean = false,
        enableMacroHLE: Boolean = true,
        enableShaderCache: Boolean = true,
        enableTextureRecompression: Boolean = false,
        backendThreading: Int = BackendThreading.Auto.ordinal
    ): Boolean

    fun graphicsInitializeRenderer(
        extensions: Array<String>,
        extensionsLength: Int,
        driver: Long
    ): Boolean

    fun javaInitialize(appPath: String, env: JNIEnv): Boolean
    fun deviceLaunchMiiEditor(): Boolean
    fun deviceGetGameFrameRate(): Double
    fun deviceGetGameFrameTime(): Double
    fun deviceGetGameFifo(): Double
    fun deviceLoadDescriptor(fileDescriptor: Int, gameType: Int, updateDescriptor: Int): Boolean
    fun graphicsRendererSetSize(width: Int, height: Int)
    fun graphicsRendererSetVsync(enabled: Boolean)
    fun graphicsRendererRunLoop()
    fun deviceReloadFilesystem()
    fun inputInitialize(width: Int, height: Int)
    fun inputSetClientSize(width: Int, height: Int)
    fun inputSetTouchPoint(x: Int, y: Int)
    fun inputReleaseTouchPoint()
    fun inputUpdate()
    fun inputSetButtonPressed(button: Int, id: Int)
    fun inputSetButtonReleased(button: Int, id: Int)
    fun inputConnectGamepad(index: Int): Int
    fun inputSetStickAxis(stick: Int, x: Float, y: Float, id: Int)
    fun inputSetAccelerometerData(x: Float, y: Float, z: Float, id: Int)
    fun inputSetGyroData(x: Float, y: Float, z: Float, id: Int)
    fun deviceCloseEmulation()
    fun deviceSignalEmulationClose()
    fun userGetOpenedUser(): String
    fun userGetUserPicture(userId: String): String
    fun userSetUserPicture(userId: String, picture: String)
    fun userGetUserName(userId: String): String
    fun userSetUserName(userId: String, userName: String)
    fun userAddUser(username: String, picture: String)
    fun userDeleteUser(userId: String)
    fun userOpenUser(userId: String)
    fun userCloseUser(userId: String)
    fun loggingSetEnabled(logLevel: Int, enabled: Boolean)
    fun deviceVerifyFirmware(fileDescriptor: Int, isXci: Boolean): String
    fun deviceInstallFirmware(fileDescriptor: Int, isXci: Boolean)
    fun deviceGetInstalledFirmwareVersion(): String
    fun uiHandlerSetup()
    fun uiHandlerSetResponse(isOkPressed: Boolean, input: String)
    fun deviceGetDlcTitleId(path: String, ncaPath: String): String
    fun deviceGetGameInfo(fileDescriptor: Int, extension: String, info: GameInfo)
    fun userGetAllUsers(): Array<String>
    fun deviceGetDlcContentList(path: String, titleId: Long): Array<String>
    fun loggingEnabledGraphicsLog(enabled: Boolean)
}

class RyujinxNative {

    companion object {
        val jnaInstance: RyujinxNativeJna = Native.load(
            "ryujinx",
            RyujinxNativeJna::class.java,
            Collections.singletonMap(Library.OPTION_ALLOW_OBJECTS, true)
        )

        @JvmStatic
        fun test()
        {
            val i = 0
        }

        @JvmStatic
        fun frameEnded()
        {
            MainActivity.frameEnded()
        }

        @JvmStatic
        fun getSurfacePtr() : Long
        {
            return MainActivity.mainViewModel?.gameHost?.currentSurface ?: -1
        }

        @JvmStatic
        fun getWindowHandle() : Long
        {
            return MainActivity.mainViewModel?.gameHost?.currentWindowhandle ?: -1
        }

        @JvmStatic
        fun updateProgress(infoPtr : Long, progress: Float)
        {
            val info = NativeHelpers.instance.getStringJava(infoPtr);
            MainActivity.mainViewModel?.gameHost?.setProgress(info, progress)
        }

        @JvmStatic
        fun updateUiHandler(
            newTitlePointer: Long,
            newMessagePointer: Long,
            newWatermarkPointer: Long,
            newType: Int,
            min: Int,
            max: Int,
            nMode: Int,
            newSubtitlePointer: Long,
            newInitialTextPointer: Long
        )
        {
            var uiHandler = MainActivity.mainViewModel?.activity?.uiHandler
            uiHandler?.apply {
                val newTitle = NativeHelpers.instance.getStringJava(newTitlePointer)
                val newMessage = NativeHelpers.instance.getStringJava(newMessagePointer)
                val newWatermark = NativeHelpers.instance.getStringJava(newWatermarkPointer)
                val newSubtitle = NativeHelpers.instance.getStringJava(newSubtitlePointer)
                val newInitialText = NativeHelpers.instance.getStringJava(newInitialTextPointer)
                val newMode = KeyboardMode.entries[nMode]
                update(newTitle,
                    newMessage,
                    newWatermark,
                    newType,
                    min,
                    max,
                    newMode,
                    newSubtitle,
                    newInitialText);
            }
        }
    }
}
