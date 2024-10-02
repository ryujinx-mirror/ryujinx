package org.ryujinx.android.views

import android.annotation.SuppressLint
import android.content.ActivityNotFoundException
import android.content.Intent
import android.provider.DocumentsContract
import androidx.activity.compose.BackHandler
import androidx.compose.animation.AnimatedVisibility
import androidx.compose.animation.core.MutableTransitionState
import androidx.compose.animation.core.animateFloat
import androidx.compose.animation.core.tween
import androidx.compose.animation.core.updateTransition
import androidx.compose.animation.expandVertically
import androidx.compose.animation.fadeIn
import androidx.compose.animation.fadeOut
import androidx.compose.animation.shrinkVertically
import androidx.compose.foundation.clickable
import androidx.compose.foundation.interaction.MutableInteractionSource
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.ExperimentalLayoutApi
import androidx.compose.foundation.layout.FlowRow
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.layout.sizeIn
import androidx.compose.foundation.layout.width
import androidx.compose.foundation.layout.wrapContentHeight
import androidx.compose.foundation.layout.wrapContentWidth
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.verticalScroll
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.ArrowBack
import androidx.compose.material.icons.filled.KeyboardArrowUp
import androidx.compose.material3.AlertDialogDefaults
import androidx.compose.material3.BasicAlertDialog
import androidx.compose.material3.Button
import androidx.compose.material3.ButtonDefaults
import androidx.compose.material3.Card
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.Label
import androidx.compose.material3.LinearProgressIndicator
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.PlainTooltip
import androidx.compose.material3.RadioButton
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Slider
import androidx.compose.material3.Surface
import androidx.compose.material3.Switch
import androidx.compose.material3.Text
import androidx.compose.material3.TextButton
import androidx.compose.material3.TopAppBar
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.rotate
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.documentfile.provider.DocumentFile
import com.anggrayudi.storage.file.extension
import org.ryujinx.android.Helpers
import org.ryujinx.android.MainActivity
import org.ryujinx.android.providers.DocumentProvider
import org.ryujinx.android.viewmodels.FirmwareInstallState
import org.ryujinx.android.viewmodels.MainViewModel
import org.ryujinx.android.viewmodels.SettingsViewModel
import org.ryujinx.android.viewmodels.VulkanDriverViewModel
import kotlin.concurrent.thread

class SettingViews {
    companion object {
        const val EXPANSTION_TRANSITION_DURATION = 450
        const val IMPORT_CODE = 12341

        @OptIn(ExperimentalMaterial3Api::class, ExperimentalLayoutApi::class)
        @Composable
        fun Main(settingsViewModel: SettingsViewModel, mainViewModel: MainViewModel) {
            val loaded = remember {
                mutableStateOf(false)
            }

            val isHostMapped = remember {
                mutableStateOf(false)
            }
            val useNce = remember {
                mutableStateOf(false)
            }
            val enableVsync = remember {
                mutableStateOf(false)
            }
            val enableDocked = remember {
                mutableStateOf(false)
            }
            val enablePtc = remember {
                mutableStateOf(false)
            }
            val ignoreMissingServices = remember {
                mutableStateOf(false)
            }
            val enableShaderCache = remember {
                mutableStateOf(false)
            }
            val enableTextureRecompression = remember {
                mutableStateOf(false)
            }
            val resScale = remember {
                mutableStateOf(1f)
            }
            val useVirtualController = remember {
                mutableStateOf(true)
            }
            val showFirwmareDialog = remember {
                mutableStateOf(false)
            }
            val firmwareInstallState = remember {
                mutableStateOf(FirmwareInstallState.None)
            }
            val firmwareVersion = remember {
                mutableStateOf(mainViewModel.firmwareVersion)
            }
            val isGrid = remember { mutableStateOf(true) }
            val useSwitchLayout = remember { mutableStateOf(true) }
            val enableMotion = remember { mutableStateOf(true) }
            val enablePerformanceMode = remember { mutableStateOf(true) }
            val controllerStickSensitivity = remember { mutableStateOf(1.0f) }

            val enableDebugLogs = remember { mutableStateOf(true) }
            val enableStubLogs = remember { mutableStateOf(true) }
            val enableInfoLogs = remember { mutableStateOf(true) }
            val enableWarningLogs = remember { mutableStateOf(true) }
            val enableErrorLogs = remember { mutableStateOf(true) }
            val enableGuestLogs = remember { mutableStateOf(true) }
            val enableAccessLogs = remember { mutableStateOf(true) }
            val enableTraceLogs = remember { mutableStateOf(true) }
            val enableGraphicsLogs = remember { mutableStateOf(true) }

            if (!loaded.value) {
                settingsViewModel.initializeState(
                    isHostMapped,
                    useNce,
                    enableVsync, enableDocked, enablePtc, ignoreMissingServices,
                    enableShaderCache,
                    enableTextureRecompression,
                    resScale,
                    useVirtualController,
                    isGrid,
                    useSwitchLayout,
                    enableMotion,
                    enablePerformanceMode,
                    controllerStickSensitivity,
                    enableDebugLogs,
                    enableStubLogs,
                    enableInfoLogs,
                    enableWarningLogs,
                    enableErrorLogs,
                    enableGuestLogs,
                    enableAccessLogs,
                    enableTraceLogs,
                    enableGraphicsLogs
                )
                loaded.value = true
            }
            Scaffold(modifier = Modifier.fillMaxSize(),
                topBar = {
                    TopAppBar(title = {
                        Text(text = "Settings")
                    },
                        modifier = Modifier.padding(top = 16.dp),
                        navigationIcon = {
                            IconButton(onClick = {
                                settingsViewModel.save(
                                    isHostMapped,
                                    useNce,
                                    enableVsync,
                                    enableDocked,
                                    enablePtc,
                                    ignoreMissingServices,
                                    enableShaderCache,
                                    enableTextureRecompression,
                                    resScale,
                                    useVirtualController,
                                    isGrid,
                                    useSwitchLayout,
                                    enableMotion,
                                    enablePerformanceMode,
                                    controllerStickSensitivity,
                                    enableDebugLogs,
                                    enableStubLogs,
                                    enableInfoLogs,
                                    enableWarningLogs,
                                    enableErrorLogs,
                                    enableGuestLogs,
                                    enableAccessLogs,
                                    enableTraceLogs,
                                    enableGraphicsLogs
                                )
                                settingsViewModel.navController.popBackStack()
                            }) {
                                Icon(Icons.AutoMirrored.Filled.ArrowBack, contentDescription = "Back")
                            }
                        })
                }) { contentPadding ->
                Column(
                    modifier = Modifier
                        .padding(contentPadding)
                        .verticalScroll(rememberScrollState())
                ) {
                    ExpandableView(onCardArrowClick = { }, title = "App") {
                        Column(modifier = Modifier.fillMaxWidth()) {
                            Row(
                                modifier = Modifier
                                    .fillMaxWidth()
                                    .padding(8.dp),
                                horizontalArrangement = Arrangement.SpaceBetween,
                                verticalAlignment = Alignment.CenterVertically
                            ) {
                                Text(
                                    text = "Use Grid",
                                    modifier = Modifier.align(Alignment.CenterVertically)
                                )
                                Switch(checked = isGrid.value, onCheckedChange = {
                                    isGrid.value = !isGrid.value
                                })
                            }
                            Row(
                                modifier = Modifier
                                    .fillMaxWidth()
                                    .padding(8.dp),
                                horizontalArrangement = Arrangement.SpaceBetween,
                                verticalAlignment = Alignment.CenterVertically
                            ) {
                                Text(
                                    text = "Game Folder",
                                    modifier = Modifier.align(Alignment.CenterVertically)
                                )
                                Button(onClick = {
                                    settingsViewModel.openGameFolder()
                                }) {
                                    Text(text = "Choose Folder")
                                }
                            }

                            Row(
                                modifier = Modifier
                                    .fillMaxWidth()
                                    .padding(8.dp),
                                horizontalArrangement = Arrangement.SpaceBetween,
                                verticalAlignment = Alignment.CenterVertically
                            ) {
                                Text(
                                    text = "System Firmware",
                                    modifier = Modifier.align(Alignment.CenterVertically)
                                )
                                Text(
                                    text = firmwareVersion.value,
                                    modifier = Modifier.align(Alignment.CenterVertically)
                                )
                            }

                            FlowRow(
                                modifier = Modifier
                                    .fillMaxWidth()
                                    .padding(8.dp),
                                horizontalArrangement = Arrangement.SpaceBetween,
                            ) {
                                Button(onClick = {
                                    fun createIntent(action: String): Intent {
                                        val intent = Intent(action)
                                        intent.addCategory(Intent.CATEGORY_DEFAULT)
                                        intent.data = DocumentsContract.buildRootUri(
                                            DocumentProvider.AUTHORITY,
                                            DocumentProvider.ROOT_ID
                                        )
                                        intent.addFlags(Intent.FLAG_GRANT_PERSISTABLE_URI_PERMISSION or Intent.FLAG_GRANT_PREFIX_URI_PERMISSION or Intent.FLAG_GRANT_WRITE_URI_PERMISSION)
                                        return intent
                                    }
                                    try {
                                        mainViewModel.activity.startActivity(createIntent(Intent.ACTION_VIEW))
                                        return@Button
                                    } catch (_: ActivityNotFoundException) {
                                    }
                                    try {
                                        mainViewModel.activity.startActivity(createIntent("android.provider.action.BROWSE"))
                                        return@Button
                                    } catch (_: ActivityNotFoundException) {
                                    }
                                    try {
                                        mainViewModel.activity.startActivity(createIntent("com.google.android.documentsui"))
                                        return@Button
                                    } catch (_: ActivityNotFoundException) {
                                    }
                                    try {
                                        mainViewModel.activity.startActivity(createIntent("com.android.documentsui"))
                                        return@Button
                                    } catch (_: ActivityNotFoundException) {
                                    }
                                }) {
                                    Text(text = "Open App Folder")
                                }

                                Button(onClick = {
                                    settingsViewModel.importProdKeys()
                                }) {
                                    Text(text = "Import prod Keys")
                                }

                                Button(onClick = {
                                    showFirwmareDialog.value = true
                                }) {
                                    Text(text = "Install Firmware")
                                }
                            }
                        }
                    }

                    if (showFirwmareDialog.value) {
                        BasicAlertDialog(onDismissRequest = {
                            if (firmwareInstallState.value != FirmwareInstallState.Install) {
                                showFirwmareDialog.value = false
                                settingsViewModel.clearFirmwareSelection(firmwareInstallState)
                            }
                        }) {
                            Card(
                                modifier = Modifier
                                    .padding(16.dp)
                                    .fillMaxWidth(),
                                shape = MaterialTheme.shapes.medium
                            ) {
                                Column(
                                    modifier = Modifier
                                        .padding(16.dp)
                                        .fillMaxWidth()
                                        .align(Alignment.CenterHorizontally),
                                    verticalArrangement = Arrangement.SpaceBetween
                                ) {
                                    if (firmwareInstallState.value == FirmwareInstallState.None) {
                                        Text(text = "Select a zip or XCI file to install from.")
                                        Row(
                                            horizontalArrangement = Arrangement.End,
                                            modifier = Modifier
                                                .fillMaxWidth()
                                                .padding(top = 4.dp)
                                        ) {
                                            Button(onClick = {
                                                settingsViewModel.selectFirmware(
                                                    firmwareInstallState
                                                )
                                            }, modifier = Modifier.padding(horizontal = 8.dp)) {
                                                Text(text = "Select File")
                                            }
                                            Button(onClick = {
                                                showFirwmareDialog.value = false
                                                settingsViewModel.clearFirmwareSelection(
                                                    firmwareInstallState
                                                )
                                            }, modifier = Modifier.padding(horizontal = 8.dp)) {
                                                Text(text = "Cancel")
                                            }
                                        }
                                    } else if (firmwareInstallState.value == FirmwareInstallState.Query) {
                                        Text(text = "Firmware ${settingsViewModel.selectedFirmwareVersion} will be installed. Do you want to continue?")
                                        Row(
                                            horizontalArrangement = Arrangement.End,
                                            modifier = Modifier
                                                .fillMaxWidth()
                                                .padding(top = 4.dp)
                                        ) {
                                            Button(onClick = {
                                                settingsViewModel.installFirmware(
                                                    firmwareInstallState
                                                )

                                                if (firmwareInstallState.value == FirmwareInstallState.None) {
                                                    showFirwmareDialog.value = false
                                                    settingsViewModel.clearFirmwareSelection(
                                                        firmwareInstallState
                                                    )
                                                }
                                            }, modifier = Modifier.padding(horizontal = 8.dp)) {
                                                Text(text = "Yes")
                                            }
                                            Button(onClick = {
                                                showFirwmareDialog.value = false
                                                settingsViewModel.clearFirmwareSelection(
                                                    firmwareInstallState
                                                )
                                            }, modifier = Modifier.padding(horizontal = 8.dp)) {
                                                Text(text = "No")
                                            }
                                        }
                                    } else if (firmwareInstallState.value == FirmwareInstallState.Install) {
                                        Text(text = "Installing Firmware ${settingsViewModel.selectedFirmwareVersion}...")
                                        LinearProgressIndicator(
                                            modifier = Modifier
                                                .padding(top = 4.dp)
                                        )
                                    } else if (firmwareInstallState.value == FirmwareInstallState.Verifying) {
                                        Text(text = "Verifying selected file...")
                                        LinearProgressIndicator(
                                            modifier = Modifier
                                                .fillMaxWidth()
                                        )
                                    } else if (firmwareInstallState.value == FirmwareInstallState.Done) {
                                        Text(text = "Installed Firmware ${settingsViewModel.selectedFirmwareVersion}")
                                        firmwareVersion.value = mainViewModel.firmwareVersion
                                    } else if (firmwareInstallState.value == FirmwareInstallState.Cancelled) {
                                        val file = settingsViewModel.selectedFirmwareFile
                                        if (file != null) {
                                            if (file.extension == "xci" || file.extension == "zip") {
                                                if (settingsViewModel.selectedFirmwareVersion.isEmpty()) {
                                                    Text(text = "Unable to find version in selected file")
                                                } else {
                                                    Text(text = "Unknown Error has occurred. Please check logs")
                                                }
                                            } else {
                                                Text(text = "File type is not supported")
                                            }
                                        } else {
                                            Text(text = "File type is not supported")
                                        }
                                    }
                                }
                            }
                        }
                    }
                    ExpandableView(onCardArrowClick = { }, title = "System") {
                        Column(modifier = Modifier.fillMaxWidth()) {
                            Row(
                                modifier = Modifier
                                    .fillMaxWidth()
                                    .padding(8.dp),
                                horizontalArrangement = Arrangement.SpaceBetween,
                                verticalAlignment = Alignment.CenterVertically
                            ) {
                                Text(
                                    text = "Use NCE",
                                    modifier = Modifier.align(Alignment.CenterVertically)
                                )
                                Switch(checked = useNce.value, onCheckedChange = {
                                    useNce.value = !useNce.value
                                })
                            }
                            Row(
                                modifier = Modifier
                                    .fillMaxWidth()
                                    .padding(8.dp),
                                horizontalArrangement = Arrangement.SpaceBetween,
                                verticalAlignment = Alignment.CenterVertically
                            ) {
                                Text(
                                    text = "Is Host Mapped",
                                    modifier = Modifier.align(Alignment.CenterVertically)
                                )
                                Switch(checked = isHostMapped.value, onCheckedChange = {
                                    isHostMapped.value = !isHostMapped.value
                                })
                            }
                            Row(
                                modifier = Modifier
                                    .fillMaxWidth()
                                    .padding(8.dp),
                                horizontalArrangement = Arrangement.SpaceBetween,
                                verticalAlignment = Alignment.CenterVertically
                            ) {
                                Text(
                                    text = "Enable VSync",
                                    modifier = Modifier.align(Alignment.CenterVertically)
                                )
                                Switch(checked = enableVsync.value, onCheckedChange = {
                                    enableVsync.value = !enableVsync.value
                                })
                            }
                            Row(
                                modifier = Modifier
                                    .fillMaxWidth()
                                    .padding(8.dp),
                                horizontalArrangement = Arrangement.SpaceBetween,
                                verticalAlignment = Alignment.CenterVertically
                            ) {
                                Text(
                                    text = "Enable PTC",
                                    modifier = Modifier.align(Alignment.CenterVertically)
                                )
                                Switch(checked = enablePtc.value, onCheckedChange = {
                                    enablePtc.value = !enablePtc.value
                                })
                            }
                            Row(
                                modifier = Modifier
                                    .fillMaxWidth()
                                    .padding(8.dp),
                                horizontalArrangement = Arrangement.SpaceBetween,
                                verticalAlignment = Alignment.CenterVertically
                            ) {
                                Text(
                                    text = "Enable Docked Mode",
                                    modifier = Modifier.align(Alignment.CenterVertically)
                                )
                                Switch(checked = enableDocked.value, onCheckedChange = {
                                    enableDocked.value = !enableDocked.value
                                })
                            }
                            Row(
                                modifier = Modifier
                                    .fillMaxWidth()
                                    .padding(8.dp),
                                horizontalArrangement = Arrangement.SpaceBetween,
                                verticalAlignment = Alignment.CenterVertically
                            ) {
                                Text(
                                    text = "Ignore Missing Services",
                                    modifier = Modifier.align(Alignment.CenterVertically)
                                )
                                Switch(checked = ignoreMissingServices.value, onCheckedChange = {
                                    ignoreMissingServices.value = !ignoreMissingServices.value
                                })
                            }
                            Row(
                                modifier = Modifier
                                    .fillMaxWidth()
                                    .padding(8.dp),
                                horizontalArrangement = Arrangement.SpaceBetween,
                                verticalAlignment = Alignment.CenterVertically
                            ) {
                                Column(
                                    modifier = Modifier.align(Alignment.CenterVertically)
                                ) {
                                    Text(
                                        text = "Enable Performance Mode",
                                    )
                                    Text(
                                        text = "Forces CPU and GPU to run at max clocks if available.",
                                        fontSize = 12.sp
                                    )
                                    Text(
                                        text = "OS power settings may override this.",
                                        fontSize = 12.sp
                                    )
                                }
                                Switch(checked = enablePerformanceMode.value, onCheckedChange = {
                                    enablePerformanceMode.value = !enablePerformanceMode.value
                                })
                            }
                            val isImporting = remember {
                                mutableStateOf(false)
                            }
                            val showImportWarning = remember {
                                mutableStateOf(false)
                            }
                            val showImportCompletion = remember {
                                mutableStateOf(false)
                            }
                            var importFile = remember {
                                mutableStateOf<DocumentFile?>(null)
                            }
                            Button(onClick = {
                                val storage = MainActivity.StorageHelper
                                storage?.apply {
                                    val callBack = this.onFileSelected
                                    onFileSelected = { requestCode, files ->
                                        run {
                                            onFileSelected = callBack
                                            if (requestCode == IMPORT_CODE) {
                                                val file = files.firstOrNull()
                                                file?.apply {
                                                    if (this.extension == "zip") {
                                                        importFile.value = this
                                                        showImportWarning.value = true
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    openFilePicker(
                                        IMPORT_CODE,
                                        filterMimeTypes = arrayOf("application/zip")
                                    )
                                }
                            }) {
                                Text(text = "Import App Data")
                            }

                            if (showImportWarning.value) {
                                BasicAlertDialog(onDismissRequest = {
                                    showImportWarning.value = false
                                    importFile.value = null
                                }) {
                                    Card(
                                        modifier = Modifier
                                            .padding(16.dp)
                                            .fillMaxWidth(),
                                        shape = MaterialTheme.shapes.medium
                                    ) {
                                        Column(
                                            modifier = Modifier
                                                .padding(16.dp)
                                                .fillMaxWidth()
                                        ) {
                                            Text(text = "Importing app data will delete your current profile. Do you still want to continue?")
                                            Row(
                                                horizontalArrangement = Arrangement.End,
                                                modifier = Modifier.fillMaxWidth()
                                            ) {
                                                Button(onClick = {
                                                    val file = importFile.value
                                                    showImportWarning.value = false
                                                    importFile.value = null
                                                    file?.apply {
                                                        thread {
                                                            Helpers.importAppData(this, isImporting)
                                                            showImportCompletion.value = true
                                                            mainViewModel.userViewModel.refreshUsers()
                                                        }
                                                    }
                                                }, modifier = Modifier.padding(horizontal = 8.dp)) {
                                                    Text(text = "Yes")
                                                }
                                                Button(onClick = {
                                                    showImportWarning.value = false
                                                    importFile.value = null
                                                }, modifier = Modifier.padding(horizontal = 8.dp)) {
                                                    Text(text = "No")
                                                }
                                            }
                                        }

                                    }
                                }
                            }

                            if (showImportCompletion.value) {
                                BasicAlertDialog(onDismissRequest = {
                                    showImportCompletion.value = false
                                    importFile.value = null
                                    mainViewModel.userViewModel.refreshUsers()
                                    mainViewModel.homeViewModel.requestReload()
                                }) {
                                    Card(
                                        modifier = Modifier,
                                        shape = MaterialTheme.shapes.medium
                                    ) {
                                        Text(
                                            modifier = Modifier
                                                .padding(24.dp),
                                            text = "App Data import completed."
                                        )
                                    }
                                }
                            }

                            if (isImporting.value) {
                                Text(text = "Importing Files")

                                LinearProgressIndicator(
                                    modifier = Modifier
                                        .fillMaxWidth()
                                        .padding(8.dp)
                                )
                            }
                        }
                    }
                    ExpandableView(onCardArrowClick = { }, title = "Graphics") {
                        Column(modifier = Modifier.fillMaxWidth()) {
                            Row(
                                modifier = Modifier
                                    .fillMaxWidth()
                                    .padding(8.dp),
                                horizontalArrangement = Arrangement.SpaceBetween,
                                verticalAlignment = Alignment.CenterVertically
                            ) {
                                Text(
                                    text = "Enable Shader Cache",
                                    modifier = Modifier.align(Alignment.CenterVertically)
                                )
                                Switch(checked = enableShaderCache.value, onCheckedChange = {
                                    enableShaderCache.value = !enableShaderCache.value
                                })
                            }
                            Row(
                                modifier = Modifier
                                    .fillMaxWidth()
                                    .padding(8.dp),
                                horizontalArrangement = Arrangement.SpaceBetween,
                                verticalAlignment = Alignment.CenterVertically
                            ) {
                                Text(
                                    text = "Resolution Scale",
                                    modifier = Modifier.align(Alignment.CenterVertically)
                                )
                                Text(text = resScale.value.toString() + "x")
                            }
                            Slider(value = resScale.value,
                                valueRange = 0.5f..4f,
                                steps = 6,
                                onValueChange = { it ->
                                    resScale.value = it
                                })
                            Row(
                                modifier = Modifier
                                    .fillMaxWidth()
                                    .padding(8.dp),
                                horizontalArrangement = Arrangement.SpaceBetween,
                                verticalAlignment = Alignment.CenterVertically
                            ) {
                                Text(
                                    text = "Enable Texture Recompression",
                                    modifier = Modifier.align(Alignment.CenterVertically)
                                )
                                Switch(
                                    checked = enableTextureRecompression.value,
                                    onCheckedChange = {
                                        enableTextureRecompression.value =
                                            !enableTextureRecompression.value
                                    })
                            }
                            Row(
                                modifier = Modifier
                                    .fillMaxWidth()
                                    .padding(8.dp),
                                horizontalArrangement = Arrangement.Start,
                                verticalAlignment = Alignment.CenterVertically
                            ) {
                                var isDriverSelectorOpen = remember {
                                    mutableStateOf(false)
                                }
                                var driverViewModel =
                                    VulkanDriverViewModel(settingsViewModel.activity)
                                var isChanged = remember {
                                    mutableStateOf(false)
                                }
                                var refresh = remember {
                                    mutableStateOf(false)
                                }
                                var drivers = driverViewModel.getAvailableDrivers()
                                var selectedDriver = remember {
                                    mutableStateOf(0)
                                }

                                if (refresh.value) {
                                    isChanged.value = true
                                    refresh.value = false
                                }

                                if (isDriverSelectorOpen.value) {
                                    BasicAlertDialog(onDismissRequest = {
                                        isDriverSelectorOpen.value = false

                                        if (isChanged.value) {
                                            driverViewModel.saveSelected()
                                        }
                                    }) {
                                        Column {
                                            Surface(
                                                modifier = Modifier
                                                    .wrapContentWidth()
                                                    .wrapContentHeight(),
                                                shape = MaterialTheme.shapes.large,
                                                tonalElevation = AlertDialogDefaults.TonalElevation
                                            ) {
                                                if (!isChanged.value) {
                                                    selectedDriver.value =
                                                        drivers.indexOfFirst { it.driverPath == driverViewModel.selected } + 1
                                                    isChanged.value = true
                                                }
                                                Column {
                                                    Column(
                                                        modifier = Modifier
                                                            .fillMaxWidth()
                                                            .height(350.dp)
                                                            .verticalScroll(rememberScrollState())
                                                    ) {
                                                        Row(
                                                            modifier = Modifier
                                                                .fillMaxWidth()
                                                                .padding(8.dp),
                                                            verticalAlignment = Alignment.CenterVertically
                                                        ) {
                                                            RadioButton(
                                                                selected = selectedDriver.value == 0 || driverViewModel.selected.isEmpty(),
                                                                onClick = {
                                                                    selectedDriver.value = 0
                                                                    isChanged.value = true
                                                                    driverViewModel.selected = ""
                                                                })
                                                            Column {
                                                                Text(text = "Default",
                                                                    modifier = Modifier
                                                                        .fillMaxWidth()
                                                                        .clickable {
                                                                            selectedDriver.value = 0
                                                                            isChanged.value = true
                                                                            driverViewModel.selected =
                                                                                ""
                                                                        })
                                                            }
                                                        }
                                                        var driverIndex = 1
                                                        for (driver in drivers) {
                                                            var ind = driverIndex
                                                            Row(
                                                                modifier = Modifier
                                                                    .fillMaxWidth()
                                                                    .padding(4.dp),
                                                                verticalAlignment = Alignment.CenterVertically
                                                            ) {
                                                                RadioButton(
                                                                    selected = selectedDriver.value == ind,
                                                                    onClick = {
                                                                        selectedDriver.value = ind
                                                                        isChanged.value = true
                                                                        driverViewModel.selected =
                                                                            driver.driverPath
                                                                    })
                                                                Column(modifier = Modifier.clickable {
                                                                    selectedDriver.value =
                                                                        ind
                                                                    isChanged.value =
                                                                        true
                                                                    driverViewModel.selected =
                                                                        driver.driverPath
                                                                }) {
                                                                    Text(
                                                                        text = driver.libraryName,
                                                                        modifier = Modifier
                                                                            .fillMaxWidth()
                                                                    )
                                                                    Text(
                                                                        text = driver.driverVersion,
                                                                        modifier = Modifier
                                                                            .fillMaxWidth()
                                                                    )
                                                                    Text(
                                                                        text = driver.description,
                                                                        modifier = Modifier
                                                                            .fillMaxWidth()
                                                                    )
                                                                }
                                                            }

                                                            driverIndex++
                                                        }
                                                    }
                                                    Row(
                                                        horizontalArrangement = Arrangement.End,
                                                        modifier = Modifier
                                                            .fillMaxWidth()
                                                            .padding(16.dp)
                                                    ) {
                                                        Button(onClick = {
                                                            driverViewModel.removeSelected()
                                                            refresh.value = true
                                                        }, modifier = Modifier.padding(8.dp)) {
                                                            Text(text = "Remove")
                                                        }

                                                        Button(onClick = {
                                                            driverViewModel.add(refresh)
                                                            refresh.value = true
                                                        }, modifier = Modifier.padding(8.dp)) {
                                                            Text(text = "Add")
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }

                                TextButton(
                                    {
                                        isChanged.value = false
                                        isDriverSelectorOpen.value = !isDriverSelectorOpen.value
                                    },
                                    modifier = Modifier.align(Alignment.CenterVertically)
                                ) {
                                    Text(text = "Drivers")
                                }
                            }

                        }
                    }
                    ExpandableView(onCardArrowClick = { }, title = "Input") {
                        Column(modifier = Modifier.fillMaxWidth()) {
                            Row(
                                modifier = Modifier
                                    .fillMaxWidth()
                                    .padding(8.dp),
                                horizontalArrangement = Arrangement.SpaceBetween,
                                verticalAlignment = Alignment.CenterVertically
                            ) {
                                Text(
                                    text = "Show virtual controller",
                                    modifier = Modifier.align(Alignment.CenterVertically)
                                )
                                Switch(checked = useVirtualController.value, onCheckedChange = {
                                    useVirtualController.value = !useVirtualController.value
                                })
                            }
                            Row(
                                modifier = Modifier
                                    .fillMaxWidth()
                                    .padding(8.dp),
                                horizontalArrangement = Arrangement.SpaceBetween,
                                verticalAlignment = Alignment.CenterVertically
                            ) {
                                Text(
                                    text = "Enable Motion",
                                    modifier = Modifier.align(Alignment.CenterVertically)
                                )
                                Switch(checked = enableMotion.value, onCheckedChange = {
                                    enableMotion.value = !enableMotion.value
                                })
                            }
                            Row(
                                modifier = Modifier
                                    .fillMaxWidth()
                                    .padding(8.dp),
                                horizontalArrangement = Arrangement.SpaceBetween,
                                verticalAlignment = Alignment.CenterVertically
                            ) {
                                Text(
                                    text = "Use Switch Controller Layout",
                                    modifier = Modifier.align(Alignment.CenterVertically)
                                )
                                Switch(checked = useSwitchLayout.value, onCheckedChange = {
                                    useSwitchLayout.value = !useSwitchLayout.value
                                })
                            }

                            val interactionSource: MutableInteractionSource = remember { MutableInteractionSource() }

                            Row(
                                modifier = Modifier
                                    .fillMaxWidth()
                                    .padding(8.dp),
                                horizontalArrangement = Arrangement.SpaceBetween,
                                verticalAlignment = Alignment.CenterVertically
                            ) {
                                Text(
                                    text = "Controller Stick Sensitivity",
                                    modifier = Modifier.align(Alignment.CenterVertically)
                                )
                                Slider(modifier = Modifier.width(250.dp), value = controllerStickSensitivity.value, onValueChange = {
                                    controllerStickSensitivity.value = it
                                }, valueRange = 0.1f..2f,
                                    steps = 20,
                                    interactionSource = interactionSource,
                                    thumb = {
                                        Label(
                                            label = {
                                                PlainTooltip(modifier = Modifier
                                                    .sizeIn(45.dp, 25.dp)
                                                    .wrapContentWidth()) {
                                                    Text("%.2f".format(controllerStickSensitivity.value))
                                                }
                                            },
                                            interactionSource = interactionSource
                                        ) {
                                            Icon(
                                                imageVector = org.ryujinx.android.Icons.circle(
                                                    color = MaterialTheme.colorScheme.primary
                                                ),
                                                contentDescription = null,
                                                modifier = Modifier.size(ButtonDefaults.IconSize),
                                                tint = MaterialTheme.colorScheme.primary
                                            )
                                        }
                                    }
                                )
                            }

                        }
                    }
                    ExpandableView(onCardArrowClick = { }, title = "Log") {
                        Column(modifier = Modifier.fillMaxWidth()) {
                            Row(
                                modifier = Modifier
                                    .fillMaxWidth()
                                    .padding(8.dp),
                                horizontalArrangement = Arrangement.SpaceBetween,
                                verticalAlignment = Alignment.CenterVertically
                            ) {
                                Text(
                                    text = "Enable Debug Logs",
                                    modifier = Modifier.align(Alignment.CenterVertically)
                                )
                                Switch(checked = enableDebugLogs.value, onCheckedChange = {
                                    enableDebugLogs.value = !enableDebugLogs.value
                                })
                            }
                            Row(
                                modifier = Modifier
                                    .fillMaxWidth()
                                    .padding(8.dp),
                                horizontalArrangement = Arrangement.SpaceBetween,
                                verticalAlignment = Alignment.CenterVertically
                            ) {
                                Text(
                                    text = "Enable Stub Logs",
                                    modifier = Modifier.align(Alignment.CenterVertically)
                                )
                                Switch(checked = enableStubLogs.value, onCheckedChange = {
                                    enableStubLogs.value = !enableStubLogs.value
                                })
                            }
                            Row(
                                modifier = Modifier
                                    .fillMaxWidth()
                                    .padding(8.dp),
                                horizontalArrangement = Arrangement.SpaceBetween,
                                verticalAlignment = Alignment.CenterVertically
                            ) {
                                Text(
                                    text = "Enable Info Logs",
                                    modifier = Modifier.align(Alignment.CenterVertically)
                                )
                                Switch(checked = enableInfoLogs.value, onCheckedChange = {
                                    enableInfoLogs.value = !enableInfoLogs.value
                                })
                            }
                            Row(
                                modifier = Modifier
                                    .fillMaxWidth()
                                    .padding(8.dp),
                                horizontalArrangement = Arrangement.SpaceBetween,
                                verticalAlignment = Alignment.CenterVertically
                            ) {
                                Text(
                                    text = "Enable Warning Logs",
                                    modifier = Modifier.align(Alignment.CenterVertically)
                                )
                                Switch(checked = enableWarningLogs.value, onCheckedChange = {
                                    enableWarningLogs.value = !enableWarningLogs.value
                                })
                            }
                            Row(
                                modifier = Modifier
                                    .fillMaxWidth()
                                    .padding(8.dp),
                                horizontalArrangement = Arrangement.SpaceBetween,
                                verticalAlignment = Alignment.CenterVertically
                            ) {
                                Text(
                                    text = "Enable Error Logs",
                                    modifier = Modifier.align(Alignment.CenterVertically)
                                )
                                Switch(checked = enableErrorLogs.value, onCheckedChange = {
                                    enableErrorLogs.value = !enableErrorLogs.value
                                })
                            }
                            Row(
                                modifier = Modifier
                                    .fillMaxWidth()
                                    .padding(8.dp),
                                horizontalArrangement = Arrangement.SpaceBetween,
                                verticalAlignment = Alignment.CenterVertically
                            ) {
                                Text(
                                    text = "Enable Guest Logs",
                                    modifier = Modifier.align(Alignment.CenterVertically)
                                )
                                Switch(checked = enableGuestLogs.value, onCheckedChange = {
                                    enableGuestLogs.value = !enableGuestLogs.value
                                })
                            }
                            Row(
                                modifier = Modifier
                                    .fillMaxWidth()
                                    .padding(8.dp),
                                horizontalArrangement = Arrangement.SpaceBetween,
                                verticalAlignment = Alignment.CenterVertically
                            ) {
                                Text(
                                    text = "Enable Access Logs",
                                    modifier = Modifier.align(Alignment.CenterVertically)
                                )
                                Switch(checked = enableAccessLogs.value, onCheckedChange = {
                                    enableAccessLogs.value = !enableAccessLogs.value
                                })
                            }
                            Row(
                                modifier = Modifier
                                    .fillMaxWidth()
                                    .padding(8.dp),
                                horizontalArrangement = Arrangement.SpaceBetween,
                                verticalAlignment = Alignment.CenterVertically
                            ) {
                                Text(
                                    text = "Enable Trace Logs",
                                    modifier = Modifier.align(Alignment.CenterVertically)
                                )
                                Switch(checked = enableTraceLogs.value, onCheckedChange = {
                                    enableTraceLogs.value = !enableTraceLogs.value
                                })
                            }
                            Row(
                                modifier = Modifier
                                    .fillMaxWidth()
                                    .padding(8.dp),
                                horizontalArrangement = Arrangement.SpaceBetween,
                                verticalAlignment = Alignment.CenterVertically
                            ) {
                                Text(
                                    text = "Enable Graphics Debug Logs",
                                    modifier = Modifier.align(Alignment.CenterVertically)
                                )
                                Switch(checked = enableGraphicsLogs.value, onCheckedChange = {
                                    enableGraphicsLogs.value = !enableGraphicsLogs.value
                                })
                            }
                            Row(
                                modifier = Modifier
                                    .fillMaxWidth()
                                    .padding(8.dp),
                                horizontalArrangement = Arrangement.SpaceBetween,
                                verticalAlignment = Alignment.CenterVertically
                            ) {
                                Button(onClick = {
                                    mainViewModel.logging.requestExport()
                                }) {
                                    Text(text = "Send Logs")
                                }
                            }
                        }
                    }
                }

                BackHandler {
                    settingsViewModel.save(
                        isHostMapped,
                        useNce, enableVsync, enableDocked, enablePtc, ignoreMissingServices,
                        enableShaderCache,
                        enableTextureRecompression,
                        resScale,
                        useVirtualController,
                        isGrid,
                        useSwitchLayout,
                        enableMotion,
                        enablePerformanceMode,
                        controllerStickSensitivity,
                        enableDebugLogs,
                        enableStubLogs,
                        enableInfoLogs,
                        enableWarningLogs,
                        enableErrorLogs,
                        enableGuestLogs,
                        enableAccessLogs,
                        enableTraceLogs,
                        enableGraphicsLogs
                    )
                    settingsViewModel.navController.popBackStack()
                }
            }
        }

        @OptIn(ExperimentalMaterial3Api::class)
        @Composable
        @SuppressLint("UnusedTransitionTargetStateParameter")
        fun ExpandableView(
            onCardArrowClick: () -> Unit,
            title: String,
            content: @Composable () -> Unit
        ) {
            val expanded = false
            val mutableExpanded = remember {
                mutableStateOf(expanded)
            }
            val transitionState = remember {
                MutableTransitionState(expanded).apply {
                    targetState = !mutableExpanded.value
                }
            }
            val transition = updateTransition(transitionState, label = "transition")
            val arrowRotationDegree by transition.animateFloat({
                tween(durationMillis = EXPANSTION_TRANSITION_DURATION)
            }, label = "rotationDegreeTransition") {
                if (mutableExpanded.value) 0f else 180f
            }

            Card(
                shape = MaterialTheme.shapes.medium,
                modifier = Modifier
                    .fillMaxWidth()
                    .padding(
                        horizontal = 24.dp,
                        vertical = 8.dp
                    )
            ) {
                Column {
                    Card(
                        onClick = {
                            mutableExpanded.value = !mutableExpanded.value
                            onCardArrowClick()
                        }) {
                        Row(
                            modifier = Modifier.fillMaxWidth(),
                            horizontalArrangement = Arrangement.SpaceBetween,
                            verticalAlignment = Alignment.CenterVertically
                        ) {
                            CardTitle(title = title)
                            CardArrow(
                                degrees = arrowRotationDegree,
                            )

                        }
                    }
                    ExpandableContent(visible = mutableExpanded.value, content = content)
                }
            }
        }

        @Composable
        fun CardArrow(
            degrees: Float,
        ) {
            Icon(
                Icons.Filled.KeyboardArrowUp,
                contentDescription = "Expandable Arrow",
                modifier = Modifier
                    .padding(8.dp)
                    .rotate(degrees),
            )
        }

        @Composable
        fun CardTitle(title: String) {
            Text(
                text = title,
                modifier = Modifier
                    .padding(16.dp),
                textAlign = TextAlign.Center,
            )
        }

        @Composable
        fun ExpandableContent(
            visible: Boolean = true,
            content: @Composable () -> Unit
        ) {
            val enterTransition = remember {
                expandVertically(
                    expandFrom = Alignment.Top,
                    animationSpec = tween(EXPANSTION_TRANSITION_DURATION)
                ) + fadeIn(
                    initialAlpha = 0.3f,
                    animationSpec = tween(EXPANSTION_TRANSITION_DURATION)
                )
            }
            val exitTransition = remember {
                shrinkVertically(
                    // Expand from the top.
                    shrinkTowards = Alignment.Top,
                    animationSpec = tween(EXPANSTION_TRANSITION_DURATION)
                ) + fadeOut(
                    // Fade in with the initial alpha of 0.3f.
                    animationSpec = tween(EXPANSTION_TRANSITION_DURATION)
                )
            }

            AnimatedVisibility(
                visible = visible,
                enter = enterTransition,
                exit = exitTransition
            ) {
                Column(modifier = Modifier.padding(8.dp)) {
                    content()
                }
            }
        }
    }
}
