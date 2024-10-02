package org.ryujinx.android.viewmodels

import android.annotation.SuppressLint
import androidx.compose.runtime.MutableState
import androidx.navigation.NavHostController
import com.anggrayudi.storage.extension.launchOnUiThread
import kotlinx.coroutines.runBlocking
import kotlinx.coroutines.sync.Semaphore
import org.ryujinx.android.GameController
import org.ryujinx.android.GameHost
import org.ryujinx.android.Logging
import org.ryujinx.android.MainActivity
import org.ryujinx.android.MotionSensorManager
import org.ryujinx.android.NativeGraphicsInterop
import org.ryujinx.android.NativeHelpers
import org.ryujinx.android.PerformanceManager
import org.ryujinx.android.PhysicalControllerManager
import org.ryujinx.android.RegionCode
import org.ryujinx.android.RyujinxNative
import org.ryujinx.android.SystemLanguage
import java.io.File

@SuppressLint("WrongConstant")
class MainViewModel(val activity: MainActivity) {
    var physicalControllerManager: PhysicalControllerManager? = null
    var motionSensorManager: MotionSensorManager? = null
    var gameModel: GameModel? = null
    var controller: GameController? = null
    var performanceManager: PerformanceManager? = null
    var selected: GameModel? = null
    var isMiiEditorLaunched = false
    val userViewModel = UserViewModel()
    val logging = Logging(this)
    var firmwareVersion = ""
    private var gameTimeState: MutableState<Double>? = null
    private var gameFpsState: MutableState<Double>? = null
    private var fifoState: MutableState<Double>? = null
    private var usedMemState: MutableState<Int>? = null
    private var totalMemState: MutableState<Int>? = null
    private var frequenciesState: MutableList<Double>? = null
    private var progress: MutableState<String>? = null
    private var progressValue: MutableState<Float>? = null
    private var showLoading: MutableState<Boolean>? = null
    private var refreshUser: MutableState<Boolean>? = null

    var gameHost: GameHost? = null
        set(value) {
            field = value
            field?.setProgressStates(showLoading, progressValue, progress)
        }
    var navController: NavHostController? = null

    var homeViewModel: HomeViewModel = HomeViewModel(activity, this)

    init {
        performanceManager = PerformanceManager(activity)
    }

    fun closeGame() {
        RyujinxNative.jnaInstance.deviceSignalEmulationClose()
        gameHost?.close()
        RyujinxNative.jnaInstance.deviceCloseEmulation()
        motionSensorManager?.unregister()
        physicalControllerManager?.disconnect()
        motionSensorManager?.setControllerId(-1)
    }

    fun refreshFirmwareVersion() {
        firmwareVersion = RyujinxNative.jnaInstance.deviceGetInstalledFirmwareVersion()
    }

    fun loadGame(game: GameModel): Int {
        val descriptor = game.open()

        if (descriptor == 0)
            return 0

        val update = game.openUpdate()

        if(update == -2)
        {
            return -2
        }

        gameModel = game
        isMiiEditorLaunched = false

        val settings = QuickSettings(activity)

        var success = RyujinxNative.jnaInstance.graphicsInitialize(
            enableShaderCache = settings.enableShaderCache,
            enableTextureRecompression = settings.enableTextureRecompression,
            rescale = settings.resScale,
            backendThreading = org.ryujinx.android.BackendThreading.Auto.ordinal
        )

        if (!success)
            return 0

        val nativeHelpers = NativeHelpers.instance
        val nativeInterop = NativeGraphicsInterop()
        nativeInterop.VkRequiredExtensions = arrayOf(
            "VK_KHR_surface", "VK_KHR_android_surface"
        )
        nativeInterop.VkCreateSurface = nativeHelpers.getCreateSurfacePtr()
        nativeInterop.SurfaceHandle = 0

        val driverViewModel = VulkanDriverViewModel(activity)
        val drivers = driverViewModel.getAvailableDrivers()

        var driverHandle = 0L

        if (driverViewModel.selected.isNotEmpty()) {
            val metaData = drivers.find { it.driverPath == driverViewModel.selected }

            metaData?.apply {
                val privatePath = activity.filesDir
                val privateDriverPath = privatePath.canonicalPath + "/driver/"
                val pD = File(privateDriverPath)
                if (pD.exists())
                    pD.deleteRecursively()

                pD.mkdirs()

                val driver = File(driverViewModel.selected)
                val parent = driver.parentFile
                if (parent != null) {
                    for (file in parent.walkTopDown()) {
                        if (file.absolutePath == parent.absolutePath)
                            continue
                        file.copyTo(File(privateDriverPath + file.name), true)
                    }
                }

                driverHandle = NativeHelpers.instance.loadDriver(
                    activity.applicationInfo.nativeLibraryDir!! + "/",
                    privateDriverPath,
                    this.libraryName
                )
            }

        }

        val extensions = nativeInterop.VkRequiredExtensions

        success = RyujinxNative.jnaInstance.graphicsInitializeRenderer(
            extensions!!,
            extensions.size,
            driverHandle
        )
        if (!success)
            return 0

        val semaphore = Semaphore(1, 0)
        runBlocking {
            semaphore.acquire()
            launchOnUiThread {
                // We are only able to initialize the emulation context on the main thread
                success = RyujinxNative.jnaInstance.deviceInitialize(
                    settings.isHostMapped,
                    settings.useNce,
                    SystemLanguage.AmericanEnglish.ordinal,
                    RegionCode.USA.ordinal,
                    settings.enableVsync,
                    settings.enableDocked,
                    settings.enablePtc,
                    false,
                    "UTC",
                    settings.ignoreMissingServices
                )

                semaphore.release()
            }
            semaphore.acquire()
            semaphore.release()
        }

        if (!success)
            return 0

        success =
            RyujinxNative.jnaInstance.deviceLoadDescriptor(descriptor, game.type.ordinal, update)

        return if (success) 1 else 0
    }

    fun loadMiiEditor(): Boolean {
        gameModel = null
        isMiiEditorLaunched = true

        val settings = QuickSettings(activity)

        var success = RyujinxNative.jnaInstance.graphicsInitialize(
            enableShaderCache = settings.enableShaderCache,
            enableTextureRecompression = settings.enableTextureRecompression,
            rescale = settings.resScale,
            backendThreading = org.ryujinx.android.BackendThreading.Auto.ordinal
        )

        if (!success)
            return false

        val nativeHelpers = NativeHelpers.instance
        val nativeInterop = NativeGraphicsInterop()
        nativeInterop.VkRequiredExtensions = arrayOf(
            "VK_KHR_surface", "VK_KHR_android_surface"
        )
        nativeInterop.VkCreateSurface = nativeHelpers.getCreateSurfacePtr()
        nativeInterop.SurfaceHandle = 0

        val driverViewModel = VulkanDriverViewModel(activity)
        val drivers = driverViewModel.getAvailableDrivers()

        var driverHandle = 0L

        if (driverViewModel.selected.isNotEmpty()) {
            val metaData = drivers.find { it.driverPath == driverViewModel.selected }

            metaData?.apply {
                val privatePath = activity.filesDir
                val privateDriverPath = privatePath.canonicalPath + "/driver/"
                val pD = File(privateDriverPath)
                if (pD.exists())
                    pD.deleteRecursively()

                pD.mkdirs()

                val driver = File(driverViewModel.selected)
                val parent = driver.parentFile
                if (parent != null) {
                    for (file in parent.walkTopDown()) {
                        if (file.absolutePath == parent.absolutePath)
                            continue
                        file.copyTo(File(privateDriverPath + file.name), true)
                    }
                }

                driverHandle = NativeHelpers.instance.loadDriver(
                    activity.applicationInfo.nativeLibraryDir!! + "/",
                    privateDriverPath,
                    this.libraryName
                )
            }

        }

        val extensions = nativeInterop.VkRequiredExtensions

        success = RyujinxNative.jnaInstance.graphicsInitializeRenderer(
            extensions!!,
            extensions.size,
            driverHandle
        )
        if (!success)
            return false

        val semaphore = Semaphore(1, 0)
        runBlocking {
            semaphore.acquire()
            launchOnUiThread {
                // We are only able to initialize the emulation context on the main thread
                success = RyujinxNative.jnaInstance.deviceInitialize(
                    settings.isHostMapped,
                    settings.useNce,
                    SystemLanguage.AmericanEnglish.ordinal,
                    RegionCode.USA.ordinal,
                    settings.enableVsync,
                    settings.enableDocked,
                    settings.enablePtc,
                    false,
                    "UTC",
                    settings.ignoreMissingServices
                )

                semaphore.release()
            }
            semaphore.acquire()
            semaphore.release()
        }

        if (!success)
            return false

        success = RyujinxNative.jnaInstance.deviceLaunchMiiEditor()

        return success
    }

    fun clearPptcCache(titleId: String) {
        if (titleId.isNotEmpty()) {
            val basePath = MainActivity.AppPath + "/games/$titleId/cache/cpu"
            if (File(basePath).exists()) {
                var caches = mutableListOf<String>()

                val mainCache = basePath + "${File.separator}0"
                File(mainCache).listFiles()?.forEach {
                    if (it.isFile && it.name.endsWith(".cache"))
                        caches.add(it.absolutePath)
                }
                val backupCache = basePath + "${File.separator}1"
                File(backupCache).listFiles()?.forEach {
                    if (it.isFile && it.name.endsWith(".cache"))
                        caches.add(it.absolutePath)
                }
                for (path in caches)
                    File(path).delete()
            }
        }
    }

    fun purgeShaderCache(titleId: String) {
        if (titleId.isNotEmpty()) {
            val basePath = MainActivity.AppPath + "/games/$titleId/cache/shader"
            if (File(basePath).exists()) {
                var caches = mutableListOf<String>()
                File(basePath).listFiles()?.forEach {
                    if (!it.isFile)
                        it.delete()
                    else {
                        if (it.name.endsWith(".toc") || it.name.endsWith(".data"))
                            caches.add(it.absolutePath)
                    }
                }
                for (path in caches)
                    File(path).delete()
            }
        }
    }

    fun deleteCache(titleId: String) {
        fun deleteDirectory(directory: File) {
            if (directory.exists() && directory.isDirectory) {
                directory.listFiles()?.forEach { file ->
                    if (file.isDirectory) {
                        deleteDirectory(file)
                    } else {
                        file.delete()
                    }
                }
                directory.delete()
            }
        }
        if (titleId.isNotEmpty()) {
            val basePath = MainActivity.AppPath + "/games/$titleId/cache"
            if (File(basePath).exists()) {
                deleteDirectory(File(basePath))
            }
        }
    }

    fun setStatStates(
        fifo: MutableState<Double>,
        gameFps: MutableState<Double>,
        gameTime: MutableState<Double>,
        usedMem: MutableState<Int>,
        totalMem: MutableState<Int>,
        frequencies: MutableList<Double>
    ) {
        fifoState = fifo
        gameFpsState = gameFps
        gameTimeState = gameTime
        usedMemState = usedMem
        totalMemState = totalMem
        frequenciesState = frequencies
    }

    fun updateStats(
        fifo: Double,
        gameFps: Double,
        gameTime: Double
    ) {
        fifoState?.apply {
            this.value = fifo
        }
        gameFpsState?.apply {
            this.value = gameFps
        }
        gameTimeState?.apply {
            this.value = gameTime
        }
        usedMemState?.let { usedMem ->
            totalMemState?.let { totalMem ->
                MainActivity.performanceMonitor.getMemoryUsage(
                    usedMem,
                    totalMem
                )
            }
        }
        frequenciesState?.let { MainActivity.performanceMonitor.getFrequencies(it) }
    }

    fun setGameController(controller: GameController) {
        this.controller = controller
    }

    fun navigateToGame() {
        activity.setFullScreen(true)
        navController?.navigate("game")
        activity.isGameRunning = true
        if (QuickSettings(activity).enableMotion)
            motionSensorManager?.register()
    }

    fun setProgressStates(
        showLoading: MutableState<Boolean>,
        progressValue: MutableState<Float>,
        progress: MutableState<String>
    ) {
        this.showLoading = showLoading
        this.progressValue = progressValue
        this.progress = progress
        gameHost?.setProgressStates(showLoading, progressValue, progress)
    }
}
