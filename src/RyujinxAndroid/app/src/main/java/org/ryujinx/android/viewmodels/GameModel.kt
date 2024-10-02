package org.ryujinx.android.viewmodels

import android.content.Context
import android.net.Uri
import android.os.ParcelFileDescriptor
import androidx.documentfile.provider.DocumentFile
import com.anggrayudi.storage.file.extension
import org.ryujinx.android.RyujinxNative


class GameModel(var file: DocumentFile, val context: Context) {
    private var updateDescriptor: ParcelFileDescriptor? = null
    var type: FileType
    var descriptor: ParcelFileDescriptor? = null
    var fileName: String?
    var fileSize = 0.0
    var titleName: String? = null
    var titleId: String? = null
    var developer: String? = null
    var version: String? = null
    var icon: String? = null

    init {
        fileName = file.name
        val pid = open()
        val gameInfo = GameInfo()
        RyujinxNative.jnaInstance.deviceGetGameInfo(pid, file.extension, gameInfo)
        close()

        fileSize = gameInfo.FileSize
        titleId = gameInfo.TitleId
        titleName = gameInfo.TitleName
        developer = gameInfo.Developer
        version = gameInfo.Version
        icon = gameInfo.Icon
        type = when {
            (file.extension == "xci") -> FileType.Xci
            (file.extension == "nsp") -> FileType.Nsp
            (file.extension == "nro") -> FileType.Nro
            else -> FileType.None
        }

        if (type == FileType.Nro && (titleName.isNullOrEmpty() || titleName == "Unknown")) {
            titleName = file.name
        }
    }

    fun open(): Int {
        descriptor = context.contentResolver.openFileDescriptor(file.uri, "rw")

        return descriptor?.fd ?: 0
    }

    fun openUpdate(): Int {
        if (titleId?.isNotEmpty() == true) {
            val vm = TitleUpdateViewModel(titleId ?: "")

            if (vm.data?.selected?.isNotEmpty() == true) {
                val uri = Uri.parse(vm.data?.selected)
                val file = DocumentFile.fromSingleUri(context, uri)
                if (file?.exists() == true) {
                    try {
                        updateDescriptor =
                            context.contentResolver.openFileDescriptor(file.uri, "rw")

                        return updateDescriptor?.fd ?: -1
                    } catch (e: Exception) {
                        return -2
                    }
                }
            }
        }

        return -1
    }

    fun close() {
        descriptor?.close()
        descriptor = null
        updateDescriptor?.close()
        updateDescriptor = null
    }
}

enum class FileType {
    None,
    Nsp,
    Xci,
    Nro
}
