package org.ryujinx.android

import android.annotation.SuppressLint
import android.content.Context
import android.view.SurfaceHolder
import android.view.SurfaceView
import androidx.compose.runtime.MutableState
import org.ryujinx.android.viewmodels.GameModel
import org.ryujinx.android.viewmodels.MainViewModel
import kotlin.concurrent.thread

@SuppressLint("ViewConstructor")
class GameHost(context: Context?, private val mainViewModel: MainViewModel) : SurfaceView(context),
    SurfaceHolder.Callback {
    private var _currentWindow: Long = -1
    private var isProgressHidden: Boolean = false
    private var progress: MutableState<String>? = null
    private var progressValue: MutableState<Float>? = null
    private var showLoading: MutableState<Boolean>? = null
    private var game: GameModel? = null
    private var _isClosed: Boolean = false
    private var _renderingThreadWatcher: Thread? = null
    private var _height: Int = 0
    private var _width: Int = 0
    private var _updateThread: Thread? = null
    private var _guestThread: Thread? = null
    private var _isInit: Boolean = false
    private var _isStarted: Boolean = false
    private val _nativeWindow: NativeWindow

    val currentSurface:Long
        get() {
            return _currentWindow
        }

    val currentWindowhandle: Long
        get() {
            return _nativeWindow.nativePointer
        }

    init {
        holder.addCallback(this)

        _nativeWindow = NativeWindow(this)

        mainViewModel.gameHost = this
    }

    override fun surfaceCreated(holder: SurfaceHolder) {
    }

    fun setProgress(info : String, progressVal: Float) {
        showLoading?.apply {
            progressValue?.apply {
                this.value = progressVal
            }

            progress?.apply {
                this.value = info
            }
        }
    }

    override fun surfaceChanged(holder: SurfaceHolder, format: Int, width: Int, height: Int) {
        if (_isClosed)
            return

        if (_width != width || _height != height) {
            _currentWindow = _nativeWindow.requeryWindowHandle()

            _nativeWindow.swapInterval = 0
        }

        _width = width
        _height = height

        start(holder)

        RyujinxNative.jnaInstance.graphicsRendererSetSize(
            width,
            height
        )

        if (_isStarted) {
            RyujinxNative.jnaInstance.inputSetClientSize(width, height)
        }
    }

    override fun surfaceDestroyed(holder: SurfaceHolder) {

    }

    fun close() {
        _isClosed = true
        _isInit = false
        _isStarted = false

        RyujinxNative.jnaInstance.uiHandlerSetResponse(false, "")

        _updateThread?.join()
        _renderingThreadWatcher?.join()
    }

    private fun start(surfaceHolder: SurfaceHolder) {
        if (_isStarted)
            return

        _isStarted = true

        game = if (mainViewModel.isMiiEditorLaunched) null else mainViewModel.gameModel

        RyujinxNative.jnaInstance.inputInitialize(width, height)

        val id = mainViewModel.physicalControllerManager?.connect()
        mainViewModel.motionSensorManager?.setControllerId(id ?: -1)

        RyujinxNative.jnaInstance.graphicsRendererSetSize(
            surfaceHolder.surfaceFrame.width(),
            surfaceHolder.surfaceFrame.height()
        )

        NativeHelpers.instance.setIsInitialOrientationFlipped(mainViewModel.activity.display?.rotation == 3)

        _guestThread = thread(start = true) {
            runGame()
        }

        _updateThread = thread(start = true) {
            var c = 0
            val helper = NativeHelpers.instance
            while (_isStarted) {
                RyujinxNative.jnaInstance.inputUpdate()
                Thread.sleep(1)
                c++
                if (c >= 1000) {
                    if (progressValue?.value == -1f)
                        progress?.apply {
                            this.value =
                                "Loading ${if (mainViewModel.isMiiEditorLaunched) "Mii Editor" else game!!.titleName}"
                        }
                    c = 0
                    mainViewModel.updateStats(
                        RyujinxNative.jnaInstance.deviceGetGameFifo(),
                        RyujinxNative.jnaInstance.deviceGetGameFrameRate(),
                        RyujinxNative.jnaInstance.deviceGetGameFrameTime()
                    )
                }
            }
        }
    }

    private fun runGame() {
        RyujinxNative.jnaInstance.graphicsRendererRunLoop()

        game?.close()
    }

    fun setProgressStates(
        showLoading: MutableState<Boolean>?,
        progressValue: MutableState<Float>?,
        progress: MutableState<String>?
    ) {
        this.showLoading = showLoading
        this.progressValue = progressValue
        this.progress = progress

        showLoading?.apply {
            showLoading.value = !isProgressHidden
        }
    }

    fun hideProgressIndicator() {
        isProgressHidden = true
        showLoading?.apply {
            if (value == isProgressHidden)
                value = !isProgressHidden
        }
    }
}
