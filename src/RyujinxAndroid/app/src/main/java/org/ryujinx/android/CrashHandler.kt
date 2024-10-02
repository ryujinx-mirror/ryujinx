package org.ryujinx.android

import java.io.File
import java.lang.Thread.UncaughtExceptionHandler

class CrashHandler : UncaughtExceptionHandler {
    var crashLog: String = ""
    override fun uncaughtException(t: Thread, e: Throwable) {
        crashLog += e.toString() + "\n"

        File(MainActivity.AppPath + "${File.separator}Logs${File.separator}crash.log").writeText(
            crashLog
        )
    }
}
