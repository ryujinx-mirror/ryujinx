package org.ryujinx.android.viewmodels

import android.content.Intent
import android.net.Uri
import androidx.compose.runtime.MutableState
import androidx.compose.runtime.snapshots.SnapshotStateList
import androidx.compose.ui.text.intl.Locale
import androidx.compose.ui.text.toLowerCase
import androidx.documentfile.provider.DocumentFile
import com.anggrayudi.storage.SimpleStorageHelper
import com.anggrayudi.storage.file.extension
import com.google.gson.Gson
import org.ryujinx.android.MainActivity
import java.io.File
import kotlin.math.max

class TitleUpdateViewModel(val titleId: String) {
    private var canClose: MutableState<Boolean>? = null
    private var basePath: String
    private var updateJsonName = "updates.json"
    private var storageHelper: SimpleStorageHelper
    private var currentPaths: MutableList<String> = mutableListOf()
    private var pathsState: SnapshotStateList<String>? = null

    companion object {
        const val UpdateRequestCode = 1002
    }

    fun remove(index: Int) {
        if (index <= 0)
            return

        data?.paths?.apply {
            val str = removeAt(index - 1)
            Uri.parse(str)?.apply {
                storageHelper.storage.context.contentResolver.releasePersistableUriPermission(
                    this,
                    Intent.FLAG_GRANT_READ_URI_PERMISSION
                )
            }
            pathsState?.clear()
            pathsState?.addAll(this)
            currentPaths = this
        }
    }

    fun add() {
        val callBack = storageHelper.onFileSelected

        storageHelper.onFileSelected = { requestCode, files ->
            run {
                storageHelper.onFileSelected = callBack
                if (requestCode == UpdateRequestCode) {
                    val file = files.firstOrNull()
                    file?.apply {
                        if (file.extension == "nsp") {
                            storageHelper.storage.context.contentResolver.takePersistableUriPermission(
                                file.uri,
                                Intent.FLAG_GRANT_READ_URI_PERMISSION
                            )
                            currentPaths.add(file.uri.toString())
                        }
                    }

                    refreshPaths()
                }
            }
        }
        storageHelper.openFilePicker(UpdateRequestCode)
    }

    private fun refreshPaths() {
        data?.apply {
            val existingPaths = mutableListOf<String>()
            currentPaths.forEach {
                val uri = Uri.parse(it)
                val file = DocumentFile.fromSingleUri(storageHelper.storage.context, uri)
                if (file?.exists() == true) {
                    existingPaths.add(it)
                }
            }

            if (!existingPaths.contains(selected)) {
                selected = ""
            }
            pathsState?.clear()
            pathsState?.addAll(existingPaths)
            paths = existingPaths
            canClose?.apply {
                value = true
            }
        }
    }

    fun save(
        index: Int,
        openDialog: MutableState<Boolean>
    ) {
        data?.apply {
            this.selected = ""
            if (paths.isNotEmpty() && index > 0) {
                val ind = max(index - 1, paths.count() - 1)
                this.selected = paths[ind]
            }
            val gson = Gson()
            File(basePath).mkdirs()


            val metadata = TitleUpdateMetadata()
            val savedUpdates = mutableListOf<String>()
            currentPaths.forEach {
                val uri = Uri.parse(it)
                val file = DocumentFile.fromSingleUri(storageHelper.storage.context, uri)
                if (file?.exists() == true) {
                    savedUpdates.add(it)
                }
            }
            metadata.paths = savedUpdates

            if (selected.isNotEmpty()) {
                val uri = Uri.parse(selected)
                val file = DocumentFile.fromSingleUri(storageHelper.storage.context, uri)
                if (file?.exists() == true) {
                    metadata.selected = selected
                }
            } else {
                metadata.selected = selected
            }

            val json = gson.toJson(metadata)
            File("$basePath/$updateJsonName").writeText(json)

            openDialog.value = false
        }
    }

    fun setPaths(paths: SnapshotStateList<String>, canClose: MutableState<Boolean>) {
        pathsState = paths
        this.canClose = canClose
        data?.apply {
            pathsState?.clear()
            pathsState?.addAll(this.paths)
        }
    }

    var data: TitleUpdateMetadata? = null
    private var jsonPath: String

    init {
        basePath = MainActivity.AppPath + "/games/" + titleId.toLowerCase(Locale.current)
        jsonPath = "${basePath}/${updateJsonName}"

        data = TitleUpdateMetadata()
        if (File(jsonPath).exists()) {
            val gson = Gson()
            data = gson.fromJson(File(jsonPath).readText(), TitleUpdateMetadata::class.java)

        }
        currentPaths = data?.paths ?: mutableListOf()
        storageHelper = MainActivity.StorageHelper!!
        refreshPaths()

        File("$basePath/update").deleteRecursively()

    }
}

data class TitleUpdateMetadata(
    var selected: String = "",
    var paths: MutableList<String> = mutableListOf()
)
