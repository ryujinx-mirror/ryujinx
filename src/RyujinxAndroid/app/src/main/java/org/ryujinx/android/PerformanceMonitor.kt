package org.ryujinx.android

import android.app.ActivityManager
import android.content.Context.ACTIVITY_SERVICE
import androidx.compose.runtime.MutableState
import java.io.RandomAccessFile

class PerformanceMonitor {
    val numberOfCores = Runtime.getRuntime().availableProcessors()

    fun getFrequencies(frequencies: MutableList<Double>){
        frequencies.clear()
        for (i in 0..<numberOfCores) {
            var freq = 0.0
            try {
                val reader = RandomAccessFile(
                    "/sys/devices/system/cpu/cpu${i}/cpufreq/scaling_cur_freq",
                    "r"
                )
                val f = reader.readLine()
                reader.close()
                freq = f.toDouble() / 1000.0
            } catch (e: Exception) {

            }

            frequencies.add(freq)
        }
    }

    fun getMemoryUsage(
        usedMem: MutableState<Int>,
        totalMem: MutableState<Int>) {
        MainActivity.mainViewModel?.activity?.apply {
            val actManager = getSystemService(ACTIVITY_SERVICE) as ActivityManager
            val memInfo = ActivityManager.MemoryInfo()
            actManager.getMemoryInfo(memInfo)
            val availMemory = memInfo.availMem.toDouble() / (1024 * 1024)
            val totalMemory = memInfo.totalMem.toDouble() / (1024 * 1024)

            usedMem.value = (totalMemory - availMemory).toInt()
            totalMem.value = totalMemory.toInt()
        }
    }
}