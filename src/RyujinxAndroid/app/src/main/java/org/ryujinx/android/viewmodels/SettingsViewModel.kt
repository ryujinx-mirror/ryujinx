package org.ryujinx.android.viewmodels

import android.content.SharedPreferences
import androidx.compose.runtime.MutableState
import androidx.documentfile.provider.DocumentFile
import androidx.navigation.NavHostController
import androidx.preference.PreferenceManager
import com.anggrayudi.storage.callback.FileCallback
import com.anggrayudi.storage.file.FileFullPath
import com.anggrayudi.storage.file.copyFileTo
import com.anggrayudi.storage.file.extension
import com.anggrayudi.storage.file.getAbsolutePath
import org.ryujinx.android.LogLevel
import org.ryujinx.android.MainActivity
import org.ryujinx.android.RyujinxNative
import java.io.File
import kotlin.concurrent.thread

class SettingsViewModel(var navController: NavHostController, val activity: MainActivity) {
    var selectedFirmwareVersion: String = ""
    private var previousFileCallback: ((requestCode: Int, files: List<DocumentFile>) -> Unit)?
    private var previousFolderCallback: ((requestCode: Int, folder: DocumentFile) -> Unit)?
    private var sharedPref: SharedPreferences
    var selectedFirmwareFile: DocumentFile? = null

    init {
        sharedPref = getPreferences()
        previousFolderCallback = activity.storageHelper!!.onFolderSelected
        previousFileCallback = activity.storageHelper!!.onFileSelected
        activity.storageHelper!!.onFolderSelected = { _, folder ->
            run {
                val p = folder.getAbsolutePath(activity)
                val editor = sharedPref.edit()
                editor?.putString("gameFolder", p)
                editor?.apply()
            }
        }
    }

    private fun getPreferences(): SharedPreferences {
        return PreferenceManager.getDefaultSharedPreferences(activity)
    }

    fun initializeState(
        isHostMapped: MutableState<Boolean>,
        useNce: MutableState<Boolean>,
        enableVsync: MutableState<Boolean>,
        enableDocked: MutableState<Boolean>,
        enablePtc: MutableState<Boolean>,
        ignoreMissingServices: MutableState<Boolean>,
        enableShaderCache: MutableState<Boolean>,
        enableTextureRecompression: MutableState<Boolean>,
        resScale: MutableState<Float>,
        useVirtualController: MutableState<Boolean>,
        isGrid: MutableState<Boolean>,
        useSwitchLayout: MutableState<Boolean>,
        enableMotion: MutableState<Boolean>,
        enablePerformanceMode: MutableState<Boolean>,
        controllerStickSensitivity: MutableState<Float>,
        enableDebugLogs: MutableState<Boolean>,
        enableStubLogs: MutableState<Boolean>,
        enableInfoLogs: MutableState<Boolean>,
        enableWarningLogs: MutableState<Boolean>,
        enableErrorLogs: MutableState<Boolean>,
        enableGuestLogs: MutableState<Boolean>,
        enableAccessLogs: MutableState<Boolean>,
        enableTraceLogs: MutableState<Boolean>,
        enableGraphicsLogs: MutableState<Boolean>
    ) {

        isHostMapped.value = sharedPref.getBoolean("isHostMapped", true)
        useNce.value = sharedPref.getBoolean("useNce", true)
        enableVsync.value = sharedPref.getBoolean("enableVsync", true)
        enableDocked.value = sharedPref.getBoolean("enableDocked", true)
        enablePtc.value = sharedPref.getBoolean("enablePtc", true)
        ignoreMissingServices.value = sharedPref.getBoolean("ignoreMissingServices", false)
        enableShaderCache.value = sharedPref.getBoolean("enableShaderCache", true)
        enableTextureRecompression.value =
            sharedPref.getBoolean("enableTextureRecompression", false)
        resScale.value = sharedPref.getFloat("resScale", 1f)
        useVirtualController.value = sharedPref.getBoolean("useVirtualController", true)
        isGrid.value = sharedPref.getBoolean("isGrid", true)
        useSwitchLayout.value = sharedPref.getBoolean("useSwitchLayout", true)
        enableMotion.value = sharedPref.getBoolean("enableMotion", true)
        enablePerformanceMode.value = sharedPref.getBoolean("enablePerformanceMode", false)
        controllerStickSensitivity.value = sharedPref.getFloat("controllerStickSensitivity", 1.0f)

        enableDebugLogs.value = sharedPref.getBoolean("enableDebugLogs", false)
        enableStubLogs.value = sharedPref.getBoolean("enableStubLogs", false)
        enableInfoLogs.value = sharedPref.getBoolean("enableInfoLogs", true)
        enableWarningLogs.value = sharedPref.getBoolean("enableWarningLogs", true)
        enableErrorLogs.value = sharedPref.getBoolean("enableErrorLogs", true)
        enableGuestLogs.value = sharedPref.getBoolean("enableGuestLogs", true)
        enableAccessLogs.value = sharedPref.getBoolean("enableAccessLogs", false)
        enableTraceLogs.value = sharedPref.getBoolean("enableStubLogs", false)
        enableGraphicsLogs.value = sharedPref.getBoolean("enableGraphicsLogs", false)
    }

    fun save(
        isHostMapped: MutableState<Boolean>,
        useNce: MutableState<Boolean>,
        enableVsync: MutableState<Boolean>,
        enableDocked: MutableState<Boolean>,
        enablePtc: MutableState<Boolean>,
        ignoreMissingServices: MutableState<Boolean>,
        enableShaderCache: MutableState<Boolean>,
        enableTextureRecompression: MutableState<Boolean>,
        resScale: MutableState<Float>,
        useVirtualController: MutableState<Boolean>,
        isGrid: MutableState<Boolean>,
        useSwitchLayout: MutableState<Boolean>,
        enableMotion: MutableState<Boolean>,
        enablePerformanceMode: MutableState<Boolean>,
        controllerStickSensitivity: MutableState<Float>,
        enableDebugLogs: MutableState<Boolean>,
        enableStubLogs: MutableState<Boolean>,
        enableInfoLogs: MutableState<Boolean>,
        enableWarningLogs: MutableState<Boolean>,
        enableErrorLogs: MutableState<Boolean>,
        enableGuestLogs: MutableState<Boolean>,
        enableAccessLogs: MutableState<Boolean>,
        enableTraceLogs: MutableState<Boolean>,
        enableGraphicsLogs: MutableState<Boolean>
    ) {
        val editor = sharedPref.edit()

        editor.putBoolean("isHostMapped", isHostMapped.value)
        editor.putBoolean("useNce", useNce.value)
        editor.putBoolean("enableVsync", enableVsync.value)
        editor.putBoolean("enableDocked", enableDocked.value)
        editor.putBoolean("enablePtc", enablePtc.value)
        editor.putBoolean("ignoreMissingServices", ignoreMissingServices.value)
        editor.putBoolean("enableShaderCache", enableShaderCache.value)
        editor.putBoolean("enableTextureRecompression", enableTextureRecompression.value)
        editor.putFloat("resScale", resScale.value)
        editor.putBoolean("useVirtualController", useVirtualController.value)
        editor.putBoolean("isGrid", isGrid.value)
        editor.putBoolean("useSwitchLayout", useSwitchLayout.value)
        editor.putBoolean("enableMotion", enableMotion.value)
        editor.putBoolean("enablePerformanceMode", enablePerformanceMode.value)
        editor.putFloat("controllerStickSensitivity", controllerStickSensitivity.value)

        editor.putBoolean("enableDebugLogs", enableDebugLogs.value)
        editor.putBoolean("enableStubLogs", enableStubLogs.value)
        editor.putBoolean("enableInfoLogs", enableInfoLogs.value)
        editor.putBoolean("enableWarningLogs", enableWarningLogs.value)
        editor.putBoolean("enableErrorLogs", enableErrorLogs.value)
        editor.putBoolean("enableGuestLogs", enableGuestLogs.value)
        editor.putBoolean("enableAccessLogs", enableAccessLogs.value)
        editor.putBoolean("enableTraceLogs", enableTraceLogs.value)
        editor.putBoolean("enableGraphicsLogs", enableGraphicsLogs.value)

        editor.apply()
        activity.storageHelper!!.onFolderSelected = previousFolderCallback

        RyujinxNative.jnaInstance.loggingSetEnabled(LogLevel.Debug.ordinal, enableDebugLogs.value)
        RyujinxNative.jnaInstance.loggingSetEnabled(LogLevel.Info.ordinal, enableInfoLogs.value)
        RyujinxNative.jnaInstance.loggingSetEnabled(LogLevel.Stub.ordinal, enableStubLogs.value)
        RyujinxNative.jnaInstance.loggingSetEnabled(
            LogLevel.Warning.ordinal,
            enableWarningLogs.value
        )
        RyujinxNative.jnaInstance.loggingSetEnabled(LogLevel.Error.ordinal, enableErrorLogs.value)
        RyujinxNative.jnaInstance.loggingSetEnabled(
            LogLevel.AccessLog.ordinal,
            enableAccessLogs.value
        )
        RyujinxNative.jnaInstance.loggingSetEnabled(LogLevel.Guest.ordinal, enableGuestLogs.value)
        RyujinxNative.jnaInstance.loggingSetEnabled(LogLevel.Trace.ordinal, enableTraceLogs.value)
        RyujinxNative.jnaInstance.loggingEnabledGraphicsLog(enableGraphicsLogs.value)
    }

    fun openGameFolder() {
        val path = sharedPref.getString("gameFolder", "") ?: ""

        if (path.isEmpty())
            activity.storageHelper?.storage?.openFolderPicker()
        else
            activity.storageHelper?.storage?.openFolderPicker(
                activity.storageHelper!!.storage.requestCodeFolderPicker,
                FileFullPath(activity, path)
            )
    }

    fun importProdKeys() {
        activity.storageHelper!!.onFileSelected = { _, files ->
            run {
                activity.storageHelper!!.onFileSelected = previousFileCallback
                val file = files.firstOrNull()
                file?.apply {
                    if (name == "prod.keys") {
                        val outputFile = File(MainActivity.AppPath + "/system")
                        outputFile.delete()

                        thread {
                            file.copyFileTo(
                                activity,
                                outputFile,
                                callback = object : FileCallback() {
                                })
                        }
                    }
                }
            }
        }
        activity.storageHelper?.storage?.openFilePicker()
    }

    fun selectFirmware(installState: MutableState<FirmwareInstallState>) {
        if (installState.value != FirmwareInstallState.None)
            return
        activity.storageHelper!!.onFileSelected = { _, files ->
            run {
                activity.storageHelper!!.onFileSelected = previousFileCallback
                val file = files.firstOrNull()
                file?.apply {
                    if (extension == "xci" || extension == "zip") {
                        installState.value = FirmwareInstallState.Verifying
                        thread {
                            val descriptor =
                                activity.contentResolver.openFileDescriptor(file.uri, "rw")
                            descriptor?.use { d ->
                                selectedFirmwareVersion =
                                    RyujinxNative.jnaInstance.deviceVerifyFirmware(
                                        d.fd,
                                        extension == "xci"
                                    )
                                selectedFirmwareFile = file
                                if (selectedFirmwareVersion.isEmpty()) {
                                    installState.value = FirmwareInstallState.Query
                                } else {
                                    installState.value = FirmwareInstallState.Cancelled
                                }
                            }
                        }
                    } else {
                        installState.value = FirmwareInstallState.Cancelled
                    }
                }
            }
        }
        activity.storageHelper?.storage?.openFilePicker()
    }

    fun installFirmware(installState: MutableState<FirmwareInstallState>) {
        if (installState.value != FirmwareInstallState.Query)
            return
        if (selectedFirmwareFile == null) {
            installState.value = FirmwareInstallState.None
            return
        }
        selectedFirmwareFile?.apply {
            val descriptor =
                activity.contentResolver.openFileDescriptor(uri, "rw")
            descriptor?.use { d ->
                installState.value = FirmwareInstallState.Install
                thread {
                    try {
                        RyujinxNative.jnaInstance.deviceInstallFirmware(
                            d.fd,
                            extension == "xci"
                        )
                    } finally {
                        MainActivity.mainViewModel?.refreshFirmwareVersion()
                        installState.value = FirmwareInstallState.Done
                    }
                }
            }
        }
    }

    fun clearFirmwareSelection(installState: MutableState<FirmwareInstallState>) {
        selectedFirmwareFile = null
        selectedFirmwareVersion = ""
        installState.value = FirmwareInstallState.None
    }
}


enum class FirmwareInstallState {
    None,
    Cancelled,
    Verifying,
    Query,
    Install,
    Done
}
