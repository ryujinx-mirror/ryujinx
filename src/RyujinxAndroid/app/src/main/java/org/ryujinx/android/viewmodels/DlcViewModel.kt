package org.ryujinx.android.viewmodels

import androidx.compose.runtime.Composable
import androidx.compose.runtime.MutableState
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.ui.text.intl.Locale
import androidx.compose.ui.text.toLowerCase
import com.anggrayudi.storage.SimpleStorageHelper
import com.anggrayudi.storage.file.getAbsolutePath
import com.google.gson.Gson
import com.google.gson.reflect.TypeToken
import org.ryujinx.android.MainActivity
import org.ryujinx.android.RyujinxNative
import java.io.File

class DlcViewModel(val titleId: String) {
    private var storageHelper: SimpleStorageHelper

    companion object {
        const val UpdateRequestCode = 1002
    }

    fun remove(item: DlcItem) {
        data?.apply {
            this.removeAll { it.path == item.containerPath }
        }
    }

    fun add(refresh: MutableState<Boolean>) {
        val callBack = storageHelper.onFileSelected

        storageHelper.onFileSelected = { requestCode, files ->
            run {
                storageHelper.onFileSelected = callBack
                if (requestCode == UpdateRequestCode) {
                    val file = files.firstOrNull()
                    file?.apply {
                        val path = file.getAbsolutePath(storageHelper.storage.context)
                        if (path.isNotEmpty()) {
                            data?.apply {
                                val contents = RyujinxNative.jnaInstance.deviceGetDlcContentList(
                                    path,
                                    titleId.toLong(16)
                                )

                                if (contents.isNotEmpty()) {
                                    val contentPath = path
                                    val container = DlcContainerList(contentPath)

                                    for (content in contents)
                                        container.dlc_nca_list.add(
                                            DlcContainer(
                                                true,
                                                titleId,
                                                content
                                            )
                                        )

                                    this.add(container)
                                }
                            }
                        }
                    }
                    refresh.value = true
                }
            }
        }
        storageHelper.openFilePicker(UpdateRequestCode)
    }

    fun save(items: List<DlcItem>) {
        data?.apply {

            val gson = Gson()
            val json = gson.toJson(this)
            jsonPath = MainActivity.AppPath + "/games/" + titleId.toLowerCase(Locale.current)
            File(jsonPath).mkdirs()
            File("$jsonPath/dlc.json").writeText(json)
        }
    }

    @Composable
    fun getDlc(): List<DlcItem> {
        var items = mutableListOf<DlcItem>()

        data?.apply {
            for (container in this) {
                val containerPath = container.path

                if (!File(containerPath).exists())
                    continue

                for (dlc in container.dlc_nca_list) {
                    val enabled = remember {
                        mutableStateOf(dlc.enabled)
                    }
                    items.add(
                        DlcItem(
                            File(containerPath).name,
                            enabled,
                            containerPath,
                            dlc.fullPath,
                            RyujinxNative.jnaInstance.deviceGetDlcTitleId(
                                containerPath,
                                dlc.fullPath
                            )
                        )
                    )
                }
            }
        }

        return items.toList()
    }

    var data: MutableList<DlcContainerList>? = null
    private var jsonPath: String

    init {
        jsonPath =
            MainActivity.AppPath + "/games/" + titleId.toLowerCase(Locale.current) + "/dlc.json"
        storageHelper = MainActivity.StorageHelper!!

        reloadFromDisk()
    }

    private fun reloadFromDisk() {
        data = mutableListOf()
        if (File(jsonPath).exists()) {
            val gson = Gson()
            val typeToken = object : TypeToken<MutableList<DlcContainerList>>() {}.type
            data =
                gson.fromJson<MutableList<DlcContainerList>>(File(jsonPath).readText(), typeToken)
        }

    }
}

data class DlcContainerList(
    var path: String = "",
    var dlc_nca_list: MutableList<DlcContainer> = mutableListOf()
)

data class DlcContainer(
    var enabled: Boolean = false,
    var titleId: String = "",
    var fullPath: String = ""
)

data class DlcItem(
    var name: String = "",
    var isEnabled: MutableState<Boolean> = mutableStateOf(false),
    var containerPath: String = "",
    var fullPath: String = "",
    var titleId: String = ""
)
