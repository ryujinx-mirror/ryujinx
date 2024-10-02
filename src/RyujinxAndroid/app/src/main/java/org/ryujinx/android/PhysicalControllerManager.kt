package org.ryujinx.android

import android.view.InputDevice
import android.view.KeyEvent
import android.view.MotionEvent
import org.ryujinx.android.viewmodels.QuickSettings

class PhysicalControllerManager(val activity: MainActivity) {
    private var controllerId: Int = -1

    fun onKeyEvent(event: KeyEvent): Boolean {
        val id = getGamePadButtonInputId(event.keyCode)
        if (id != GamePadButtonInputId.None) {
            val isNotFallback = (event.flags and KeyEvent.FLAG_FALLBACK) == 0
            if (/*controllerId != -1 &&*/ isNotFallback) {
                when (event.action) {
                    KeyEvent.ACTION_UP -> {
                        RyujinxNative.jnaInstance.inputSetButtonReleased(id.ordinal, controllerId)
                    }

                    KeyEvent.ACTION_DOWN -> {
                        RyujinxNative.jnaInstance.inputSetButtonPressed(id.ordinal, controllerId)
                    }
                }
                return true
            } else if (!isNotFallback) {
                return true
            }
        }

        return false
    }

    fun onMotionEvent(ev: MotionEvent) {
        if (true) {
            if (ev.action == MotionEvent.ACTION_MOVE) {
                val leftStickX = ev.getAxisValue(MotionEvent.AXIS_X)
                val leftStickY = ev.getAxisValue(MotionEvent.AXIS_Y)
                val rightStickX = ev.getAxisValue(MotionEvent.AXIS_Z)
                val rightStickY = ev.getAxisValue(MotionEvent.AXIS_RZ)
                RyujinxNative.jnaInstance.inputSetStickAxis(
                    1,
                    leftStickX,
                    -leftStickY,
                    controllerId
                )
                RyujinxNative.jnaInstance.inputSetStickAxis(
                    2,
                    rightStickX,
                    -rightStickY,
                    controllerId
                )

                ev.device?.apply {
                    if (sources and InputDevice.SOURCE_DPAD != InputDevice.SOURCE_DPAD) {
                        // Controller uses HAT
                        val dPadHor = ev.getAxisValue(MotionEvent.AXIS_HAT_X)
                        val dPadVert = ev.getAxisValue(MotionEvent.AXIS_HAT_Y)
                        if (dPadVert == 0.0f) {
                            RyujinxNative.jnaInstance.inputSetButtonReleased(
                                GamePadButtonInputId.DpadUp.ordinal,
                                controllerId
                            )
                            RyujinxNative.jnaInstance.inputSetButtonReleased(
                                GamePadButtonInputId.DpadDown.ordinal,
                                controllerId
                            )
                        }
                        if (dPadHor == 0.0f) {
                            RyujinxNative.jnaInstance.inputSetButtonReleased(
                                GamePadButtonInputId.DpadLeft.ordinal,
                                controllerId
                            )
                            RyujinxNative.jnaInstance.inputSetButtonReleased(
                                GamePadButtonInputId.DpadRight.ordinal,
                                controllerId
                            )
                        }

                        if (dPadVert < 0.0f) {
                            RyujinxNative.jnaInstance.inputSetButtonPressed(
                                GamePadButtonInputId.DpadUp.ordinal,
                                controllerId
                            )
                            RyujinxNative.jnaInstance.inputSetButtonReleased(
                                GamePadButtonInputId.DpadDown.ordinal,
                                controllerId
                            )
                        }
                        if (dPadHor < 0.0f) {
                            RyujinxNative.jnaInstance.inputSetButtonPressed(
                                GamePadButtonInputId.DpadLeft.ordinal,
                                controllerId
                            )
                            RyujinxNative.jnaInstance.inputSetButtonReleased(
                                GamePadButtonInputId.DpadRight.ordinal,
                                controllerId
                            )
                        }

                        if (dPadVert > 0.0f) {
                            RyujinxNative.jnaInstance.inputSetButtonReleased(
                                GamePadButtonInputId.DpadUp.ordinal,
                                controllerId
                            )
                            RyujinxNative.jnaInstance.inputSetButtonPressed(
                                GamePadButtonInputId.DpadDown.ordinal,
                                controllerId
                            )
                        }
                        if (dPadHor > 0.0f) {
                            RyujinxNative.jnaInstance.inputSetButtonReleased(
                                GamePadButtonInputId.DpadLeft.ordinal,
                                controllerId
                            )
                            RyujinxNative.jnaInstance.inputSetButtonPressed(
                                GamePadButtonInputId.DpadRight.ordinal,
                                controllerId
                            )
                        }
                    }
                }
            }
        }
    }

    fun connect(): Int {
        controllerId = RyujinxNative.jnaInstance.inputConnectGamepad(0)
        return controllerId
    }

    fun disconnect() {
        controllerId = -1
    }

    private fun getGamePadButtonInputId(keycode: Int): GamePadButtonInputId {
        val quickSettings = QuickSettings(activity)
        return when (keycode) {
            KeyEvent.KEYCODE_BUTTON_A -> if (!quickSettings.useSwitchLayout) GamePadButtonInputId.A else GamePadButtonInputId.B
            KeyEvent.KEYCODE_BUTTON_B -> if (!quickSettings.useSwitchLayout) GamePadButtonInputId.B else GamePadButtonInputId.A
            KeyEvent.KEYCODE_BUTTON_X -> if (!quickSettings.useSwitchLayout) GamePadButtonInputId.X else GamePadButtonInputId.Y
            KeyEvent.KEYCODE_BUTTON_Y -> if (!quickSettings.useSwitchLayout) GamePadButtonInputId.Y else GamePadButtonInputId.X
            KeyEvent.KEYCODE_BUTTON_L1 -> GamePadButtonInputId.LeftShoulder
            KeyEvent.KEYCODE_BUTTON_L2 -> GamePadButtonInputId.LeftTrigger
            KeyEvent.KEYCODE_BUTTON_R1 -> GamePadButtonInputId.RightShoulder
            KeyEvent.KEYCODE_BUTTON_R2 -> GamePadButtonInputId.RightTrigger
            KeyEvent.KEYCODE_BUTTON_THUMBL -> GamePadButtonInputId.LeftStick
            KeyEvent.KEYCODE_BUTTON_THUMBR -> GamePadButtonInputId.RightStick
            KeyEvent.KEYCODE_DPAD_UP -> GamePadButtonInputId.DpadUp
            KeyEvent.KEYCODE_DPAD_DOWN -> GamePadButtonInputId.DpadDown
            KeyEvent.KEYCODE_DPAD_LEFT -> GamePadButtonInputId.DpadLeft
            KeyEvent.KEYCODE_DPAD_RIGHT -> GamePadButtonInputId.DpadRight
            KeyEvent.KEYCODE_BUTTON_START -> GamePadButtonInputId.Plus
            KeyEvent.KEYCODE_BUTTON_SELECT -> GamePadButtonInputId.Minus
            else -> GamePadButtonInputId.None
        }
    }
}
