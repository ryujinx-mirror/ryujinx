package org.ryujinx.android

import android.content.ContentUris
import android.content.Context
import android.database.Cursor
import android.net.Uri
import android.os.Environment
import android.provider.DocumentsContract
import android.provider.MediaStore
import androidx.compose.runtime.MutableState
import androidx.documentfile.provider.DocumentFile
import com.anggrayudi.storage.SimpleStorageHelper
import com.anggrayudi.storage.callback.FileCallback
import com.anggrayudi.storage.file.copyFileTo
import com.anggrayudi.storage.file.openInputStream
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.launch
import net.lingala.zip4j.io.inputstream.ZipInputStream
import java.io.BufferedOutputStream
import java.io.File
import java.io.FileOutputStream

class Helpers {
    companion object {
        fun getPath(context: Context, uri: Uri): String? {

            // DocumentProvider
            if (DocumentsContract.isDocumentUri(context, uri)) {
                // ExternalStorageProvider
                if (isExternalStorageDocument(uri)) {
                    val docId = DocumentsContract.getDocumentId(uri)
                    val split = docId.split(":".toRegex()).toTypedArray()
                    val type = split[0]
                    if ("primary".equals(type, ignoreCase = true)) {
                        return Environment.getExternalStorageDirectory().toString() + "/" + split[1]
                    }

                } else if (isDownloadsDocument(uri)) {
                    val id = DocumentsContract.getDocumentId(uri)
                    val contentUri = ContentUris.withAppendedId(
                        Uri.parse("content://downloads/public_downloads"),
                        java.lang.Long.valueOf(id)
                    )
                    return getDataColumn(context, contentUri, null, null)
                } else if (isMediaDocument(uri)) {
                    val docId = DocumentsContract.getDocumentId(uri)
                    val split = docId.split(":".toRegex()).toTypedArray()
                    val type = split[0]
                    var contentUri: Uri? = null
                    when (type) {
                        "image" -> {
                            contentUri = MediaStore.Images.Media.EXTERNAL_CONTENT_URI
                        }

                        "video" -> {
                            contentUri = MediaStore.Video.Media.EXTERNAL_CONTENT_URI
                        }

                        "audio" -> {
                            contentUri = MediaStore.Audio.Media.EXTERNAL_CONTENT_URI
                        }
                    }
                    val selection = "_id=?"
                    val selectionArgs = arrayOf(split[1])
                    return getDataColumn(context, contentUri, selection, selectionArgs)
                }
            } else if ("content".equals(uri.scheme, ignoreCase = true)) {
                return getDataColumn(context, uri, null, null)
            } else if ("file".equals(uri.scheme, ignoreCase = true)) {
                return uri.path
            }
            return null
        }

        fun copyToData(
            file: DocumentFile, path: String, storageHelper: SimpleStorageHelper,
            isCopying: MutableState<Boolean>,
            copyProgress: MutableState<Float>,
            currentProgressName: MutableState<String>,
            finish: () -> Unit
        ) {
            var fPath = path + "/${file.name}"
            var callback: FileCallback? = object : FileCallback() {
                override fun onFailed(errorCode: ErrorCode) {
                    super.onFailed(errorCode)
                    File(fPath).delete()
                    finish()
                }

                override fun onStart(file: Any, workerThread: Thread): Long {
                    copyProgress.value = 0f

                    (file as DocumentFile).apply {
                        currentProgressName.value = "Copying ${file.name}"
                    }
                    return super.onStart(file, workerThread)
                }

                override fun onReport(report: Report) {
                    super.onReport(report)

                    if (!isCopying.value) {
                        Thread.currentThread().interrupt()
                    }

                    copyProgress.value = report.progress / 100f
                }

                override fun onCompleted(result: Any) {
                    super.onCompleted(result)
                    isCopying.value = false
                    finish()
                }
            }
            val ioScope = CoroutineScope(Dispatchers.IO)
            isCopying.value = true
            File(fPath).delete()
            file.apply {
                val f = this
                ioScope.launch {
                    f.copyFileTo(
                        storageHelper.storage.context,
                        File(path),
                        callback = callback!!
                    )

                }
            }
        }

        private fun getDataColumn(
            context: Context,
            uri: Uri?,
            selection: String?,
            selectionArgs: Array<String>?
        ): String? {
            var cursor: Cursor? = null
            val column = "_data"
            val projection = arrayOf(column)
            try {
                cursor = uri?.let {
                    context.contentResolver.query(
                        it,
                        projection,
                        selection,
                        selectionArgs,
                        null
                    )
                }
                if (cursor != null && cursor.moveToFirst()) {
                    val column_index: Int = cursor.getColumnIndexOrThrow(column)
                    return cursor.getString(column_index)
                }
            } finally {
                cursor?.close()
            }
            return null
        }

        private fun isExternalStorageDocument(uri: Uri): Boolean {
            return "com.android.externalstorage.documents" == uri.authority
        }

        private fun isDownloadsDocument(uri: Uri): Boolean {
            return "com.android.providers.downloads.documents" == uri.authority
        }

        private fun isMediaDocument(uri: Uri): Boolean {
            return "com.android.providers.media.documents" == uri.authority
        }

        fun importAppData(
            file: DocumentFile,
            isImporting: MutableState<Boolean>
        ) {
            isImporting.value = true
            try {
                MainActivity.StorageHelper?.apply {
                    val stream = file.openInputStream(storage.context)
                    stream?.apply {
                        val folders = listOf("bis", "games", "profiles", "system")
                        for (f in folders) {
                            val dir = File(MainActivity.AppPath + "${File.separator}${f}")
                            if (dir.exists()) {
                                dir.deleteRecursively()
                            }

                            dir.mkdirs()
                        }
                        ZipInputStream(stream).use { zip ->
                            while (true) {
                                val header = zip.nextEntry ?: break
                                if (!folders.any { header.fileName.startsWith(it) }) {
                                    continue
                                }
                                val filePath =
                                    MainActivity.AppPath + File.separator + header.fileName

                                if (!header.isDirectory) {
                                    val bos = BufferedOutputStream(FileOutputStream(filePath))
                                    val bytesIn = ByteArray(4096)
                                    var read: Int = 0
                                    while (zip.read(bytesIn).also { read = it } > 0) {
                                        bos.write(bytesIn, 0, read)
                                    }
                                    bos.close()
                                } else {
                                    val dir = File(filePath)
                                    dir.mkdir()
                                }
                            }
                        }
                        stream.close()
                    }
                }
            } finally {
                isImporting.value = false
                RyujinxNative.jnaInstance.deviceReloadFilesystem()
            }
        }
    }
}
