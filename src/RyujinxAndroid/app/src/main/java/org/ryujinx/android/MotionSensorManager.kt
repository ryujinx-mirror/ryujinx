package org.ryujinx.android

import android.app.Activity
import android.hardware.Sensor
import android.hardware.SensorEvent
import android.hardware.SensorEventListener2
import android.hardware.SensorManager
import android.view.OrientationEventListener

class MotionSensorManager(val activity: MainActivity) : SensorEventListener2 {
    private var isRegistered: Boolean = false
    private var gyro: Sensor?
    private var accelerometer: Sensor?
    private var sensorManager: SensorManager =
        activity.getSystemService(Activity.SENSOR_SERVICE) as SensorManager
    private var controllerId: Int = -1

    private val motionGyroOrientation: FloatArray = FloatArray(3)
    private val motionAcelOrientation: FloatArray = FloatArray(3)

    init {
        accelerometer = sensorManager.getDefaultSensor(Sensor.TYPE_ACCELEROMETER)
        gyro = sensorManager.getDefaultSensor(Sensor.TYPE_GYROSCOPE)
        setOrientation90()
        var orientationListener = object : OrientationEventListener(activity) {
            override fun onOrientationChanged(orientation: Int) {
                when {
                    isWithinOrientationRange(orientation, 270) -> {
                        setOrientation270()
                    }

                    isWithinOrientationRange(orientation, 90) -> {
                        setOrientation90()
                    }
                }
            }

            private fun isWithinOrientationRange(
                currentOrientation: Int, targetOrientation: Int, epsilon: Int = 90
            ): Boolean {
                return currentOrientation > targetOrientation - epsilon
                        && currentOrientation < targetOrientation + epsilon
            }
        }
    }

    fun setOrientation270() {
        motionGyroOrientation[0] = -1.0f
        motionGyroOrientation[1] = 1.0f
        motionGyroOrientation[2] = 1.0f
        motionAcelOrientation[0] = 1.0f
        motionAcelOrientation[1] = -1.0f
        motionAcelOrientation[2] = -1.0f
    }

    fun setOrientation90() {
        motionGyroOrientation[0] = 1.0f
        motionGyroOrientation[1] = -1.0f
        motionGyroOrientation[2] = 1.0f
        motionAcelOrientation[0] = -1.0f
        motionAcelOrientation[1] = 1.0f
        motionAcelOrientation[2] = -1.0f
    }

    fun setControllerId(id: Int) {
        controllerId = id
    }

    fun register() {
        if (isRegistered)
            return
        gyro?.apply {
            sensorManager.registerListener(
                this@MotionSensorManager,
                gyro,
                SensorManager.SENSOR_DELAY_GAME
            )
        }
        accelerometer?.apply {
            sensorManager.registerListener(
                this@MotionSensorManager,
                accelerometer,
                SensorManager.SENSOR_DELAY_GAME
            )
        }

        isRegistered = true
    }

    fun unregister() {
        sensorManager.unregisterListener(this)
        isRegistered = false

        if (controllerId != -1) {
            RyujinxNative.jnaInstance.inputSetAccelerometerData(0.0F, 0.0F, 0.0F, controllerId)
            RyujinxNative.jnaInstance.inputSetGyroData(0.0F, 0.0F, 0.0F, controllerId)
        }
    }

    override fun onSensorChanged(event: SensorEvent?) {
        if (controllerId != -1)
            if (isRegistered)
                event?.apply {
                    when (sensor.type) {
                        Sensor.TYPE_ACCELEROMETER -> {
                            val x = motionAcelOrientation[0] * event.values[1]
                            val y = motionAcelOrientation[1] * event.values[0]
                            val z = motionAcelOrientation[2] * event.values[2]

                            RyujinxNative.jnaInstance.inputSetAccelerometerData(
                                x,
                                y,
                                z,
                                controllerId
                            )
                        }

                        Sensor.TYPE_GYROSCOPE -> {
                            val x = motionGyroOrientation[0] * event.values[1]
                            val y = motionGyroOrientation[1] * event.values[0]
                            val z = motionGyroOrientation[2] * event.values[2]
                            RyujinxNative.jnaInstance.inputSetGyroData(x, y, z, controllerId)
                        }
                    }
                }
            else {
                RyujinxNative.jnaInstance.inputSetAccelerometerData(0.0F, 0.0F, 0.0F, controllerId)
                RyujinxNative.jnaInstance.inputSetGyroData(0.0F, 0.0F, 0.0F, controllerId)
            }
    }

    override fun onAccuracyChanged(sensor: Sensor?, accuracy: Int) {
    }

    override fun onFlushCompleted(sensor: Sensor?) {
    }
}
