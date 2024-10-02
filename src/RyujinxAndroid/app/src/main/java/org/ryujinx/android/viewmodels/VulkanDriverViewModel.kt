package org.ryujinx.android.viewmodels

import androidx.compose.runtime.MutableState
import com.anggrayudi.storage.file.extension
import com.anggrayudi.storage.file.openInputStream
import com.google.gson.Gson
import org.ryujinx.android.MainActivity
import java.io.BufferedOutputStream
import java.io.File
import java.io.FileOutputStream
import java.util.zip.ZipInputStream

class VulkanDriverViewModel(val activity: MainActivity) {
    var selected: String = ""

    companion object {
        const val DriverRequestCode: Int = 1003
        const val DriverFolder: String = "drivers"
    }

    private fun getAppPath(): String {
        var appPath =
            MainActivity.AppPath
        appPath += "/"

        return appPath
    }

    fun ensureDriverPath(): File {
        val driverPath = getAppPath() + DriverFolder

        val driverFolder = File(driverPath)

        if (!driverFolder.exists())
            driverFolder.mkdirs()

        return driverFolder
    }

    fun getAvailableDrivers(): MutableList<DriverMetadata> {
        val driverFolder = ensureDriverPath()

        val folders = driverFolder.walkTopDown()

        val drivers = mutableListOf<DriverMetadata>()

        val selectedDriverFile = File(driverFolder.absolutePath + "/selected")
        if (selectedDriverFile.exists()) {
            selected = selectedDriverFile.readText()

            if (!File(selected).exists()) {
                selected = ""
                saveSelected()
            }
        }

        val gson = Gson()

        for (folder in folders) {
            if (folder.isDirectory && folder.parent == driverFolder.absolutePath) {
                val meta = File(folder.absolutePath + "/meta.json")

                if (meta.exists()) {
                    val metadata = gson.fromJson(meta.readText(), DriverMetadata::class.java)
                    if (metadata.name.isNotEmpty()) {
                        val driver = folder.absolutePath + "/${metadata.libraryName}"
                        metadata.driverPath = driver
                        if (File(driver).exists())
                            drivers.add(metadata)
                    }
                }
            }
        }

        return drivers
    }

    fun saveSelected() {
        val driverFolder = ensureDriverPath()

        val selectedDriverFile = File(driverFolder.absolutePath + "/selected")
        selectedDriverFile.writeText(selected)
    }

    fun removeSelected() {
        if (selected.isNotEmpty()) {
            val sel = File(selected)
            if (sel.exists()) {
                sel.parentFile?.deleteRecursively()
            }
            selected = ""

            saveSelected()
        }
    }

    fun add(refresh: MutableState<Boolean>) {
        activity.storageHelper?.apply {

            val callBack = this.onFileSelected

            onFileSelected = { requestCode, files ->
                run {
                    onFileSelected = callBack
                    if (requestCode == DriverRequestCode) {
                        val file = files.firstOrNull()
                        file?.apply {
                            val stream = file.openInputStream(storage.context)
                            stream?.apply {
                                val name = file.name?.removeSuffix("." + file.extension) ?: ""
                                val driverFolder = ensureDriverPath()
                                val extractionFolder = File(driverFolder.absolutePath + "/${name}")
                                extractionFolder.deleteRecursively()
                                extractionFolder.mkdirs()
                                ZipInputStream(stream).use { zip ->
                                    var entry = zip.nextEntry
                                    while (entry != null) {
                                        val filePath =
                                            extractionFolder.absolutePath + File.separator + entry.name

                                        if (!entry.isDirectory) {
                                            File(filePath).delete()
                                            val bos =
                                                BufferedOutputStream(FileOutputStream(filePath))
                                            val bytesIn = ByteArray(4096)
                                            var read: Int
                                            while (zip.read(bytesIn)
                                                    .also { read = it } != -1
                                            ) {
                                                bos.write(bytesIn, 0, read)
                                            }
                                            bos.close()
                                        } else {
                                            val dir = File(filePath)
                                            dir.mkdir()
                                        }

                                        entry = zip.nextEntry
                                    }
                                }
                            }
                        }

                        refresh.value = true
                    }
                }
            }
            openFilePicker(
                DriverRequestCode,
                filterMimeTypes = arrayOf("application/zip")
            )
        }
    }
}

data class DriverMetadata(
    var schemaVersion: Int = 0,
    var name: String = "",
    var description: String = "",
    var author: String = "",
    var packageVersion: String = "",
    var vendor: String = "",
    var driverVersion: String = "",
    var minApi: Int = 0,
    var libraryName: String = "",
    var driverPath: String = ""
)