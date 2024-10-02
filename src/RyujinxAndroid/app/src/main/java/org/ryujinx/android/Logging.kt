package org.ryujinx.android

import android.content.Intent
import androidx.core.content.FileProvider
import net.lingala.zip4j.ZipFile
import org.ryujinx.android.viewmodels.MainViewModel
import java.io.File
import java.net.URLConnection

class Logging(private var viewModel: MainViewModel) {
    val logPath = MainActivity.AppPath + "/Logs"

    init {
        File(logPath).mkdirs()
    }

    fun requestExport() {
        val files = File(logPath).listFiles()
        files?.apply {
            val zipExportPath = MainActivity.AppPath + "/log.zip"
            File(zipExportPath).delete()
            var count = 0
            if (files.isNotEmpty()) {
                val zipFile = ZipFile(zipExportPath)
                for (file in files) {
                    if (file.isFile) {
                        zipFile.addFile(file)
                        count++
                    }
                }
                zipFile.close()
            }
            if (count > 0) {
                val zip = File(zipExportPath)
                val uri = FileProvider.getUriForFile(
                    viewModel.activity,
                    viewModel.activity.packageName + ".fileprovider",
                    zip
                )
                val intent = Intent(Intent.ACTION_SEND)
                intent.putExtra(Intent.EXTRA_STREAM, uri)
                intent.setDataAndType(uri, URLConnection.guessContentTypeFromName(zip.name))
                intent.addFlags(Intent.FLAG_GRANT_READ_URI_PERMISSION)
                val chooser = Intent.createChooser(intent, "Share logs")
                viewModel.activity.startActivity(chooser)
            } else {
                File(zipExportPath).delete()
            }
        }
    }

    fun clearLogs() {
        if (File(logPath).exists()) {
            File(logPath).deleteRecursively()
        }

        File(logPath).mkdirs()
    }
}

internal enum class LogLevel {
    Debug, Stub, Info, Warning, Error, Guest, AccessLog, Notice, Trace
}
