package org.ryujinx.android.views

import android.content.res.Resources
import android.graphics.BitmapFactory
import androidx.compose.animation.AnimatedVisibility
import androidx.compose.animation.slideInVertically
import androidx.compose.animation.slideOutVertically
import androidx.compose.foundation.ExperimentalFoundationApi
import androidx.compose.foundation.Image
import androidx.compose.foundation.basicMarquee
import androidx.compose.foundation.border
import androidx.compose.foundation.combinedClickable
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.layout.width
import androidx.compose.foundation.layout.wrapContentHeight
import androidx.compose.foundation.layout.wrapContentWidth
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.LazyRow
import androidx.compose.foundation.lazy.grid.GridCells
import androidx.compose.foundation.lazy.grid.LazyVerticalGrid
import androidx.compose.foundation.lazy.grid.items
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Add
import androidx.compose.material.icons.filled.Menu
import androidx.compose.material.icons.filled.Person
import androidx.compose.material.icons.filled.Refresh
import androidx.compose.material.icons.filled.Search
import androidx.compose.material.icons.filled.Settings
import androidx.compose.material3.AlertDialogDefaults
import androidx.compose.material3.BasicAlertDialog
import androidx.compose.material3.Card
import androidx.compose.material3.CircularProgressIndicator
import androidx.compose.material3.DropdownMenu
import androidx.compose.material3.DropdownMenuItem
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.FloatingActionButton
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.LinearProgressIndicator
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.ModalBottomSheet
import androidx.compose.material3.Scaffold
import androidx.compose.material3.SearchBar
import androidx.compose.material3.SearchBarDefaults
import androidx.compose.material3.Surface
import androidx.compose.material3.Text
import androidx.compose.material3.TextButton
import androidx.compose.runtime.Composable
import androidx.compose.runtime.MutableState
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.geometry.Offset
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.graphics.asImageBitmap
import androidx.compose.ui.input.nestedscroll.NestedScrollConnection
import androidx.compose.ui.input.nestedscroll.NestedScrollSource
import androidx.compose.ui.input.nestedscroll.nestedScroll
import androidx.compose.ui.layout.ContentScale
import androidx.compose.ui.res.painterResource
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import androidx.navigation.NavHostController
import com.anggrayudi.storage.extension.launchOnUiThread
import org.ryujinx.android.R
import org.ryujinx.android.viewmodels.FileType
import org.ryujinx.android.viewmodels.GameModel
import org.ryujinx.android.viewmodels.HomeViewModel
import org.ryujinx.android.viewmodels.QuickSettings
import java.util.Base64
import java.util.Locale
import kotlin.concurrent.thread
import kotlin.math.roundToInt

class HomeViews {
    companion object {
        const val ListImageSize = 150
        const val GridImageSize = 300

        @OptIn(ExperimentalMaterial3Api::class, ExperimentalFoundationApi::class)
        @Composable
        fun Home(
            viewModel: HomeViewModel = HomeViewModel(),
            navController: NavHostController? = null,
            isPreview: Boolean = false
        ) {
            viewModel.ensureReloadIfNecessary()
            val showAppActions = remember { mutableStateOf(false) }
            val showLoading = remember { mutableStateOf(false) }
            val openTitleUpdateDialog = remember { mutableStateOf(false) }
            val canClose = remember { mutableStateOf(true) }
            val openDlcDialog = remember { mutableStateOf(false) }
            var openAppBarExtra by remember { mutableStateOf(false) }
            val showError = remember {
                mutableStateOf("")
            }

            val selectedModel = remember {
                mutableStateOf(viewModel.mainViewModel?.selected)
            }
            val query = remember {
                mutableStateOf("")
            }
            var refreshUser by remember {
                mutableStateOf(true)
            }

            var isFabVisible by remember {
                mutableStateOf(true)
            }

            val nestedScrollConnection = remember {
                object : NestedScrollConnection {
                    override fun onPreScroll(available: Offset, source: NestedScrollSource): Offset {
                        if (available.y < -1) {
                            isFabVisible = false
                        }
                        if (available.y > 1) {
                            isFabVisible = true
                        }
                        return Offset.Zero
                    }
                }
            }

            Scaffold(
                modifier = Modifier.fillMaxSize(),
                topBar = {
                    SearchBar(
                        modifier = Modifier
                            .fillMaxWidth()
                            .padding(8.dp),
                        shape = SearchBarDefaults.inputFieldShape,
                        query = query.value,
                        onQueryChange = {
                            query.value = it
                        },
                        onSearch = {},
                        active = false,
                        onActiveChange = {},
                        leadingIcon = {
                            Icon(
                                Icons.Filled.Search,
                                contentDescription = "Search Games"
                            )
                        },
                        placeholder = {
                            Text(text = "Ryujinx")
                        },
                        trailingIcon = {
                            IconButton(onClick = {
                                openAppBarExtra = !openAppBarExtra
                            }) {
                                if (!refreshUser) {
                                    refreshUser = true
                                }
                                if (refreshUser)
                                    if (viewModel.mainViewModel?.userViewModel?.openedUser?.userPicture?.isNotEmpty() == true) {
                                        val pic =
                                            viewModel.mainViewModel.userViewModel.openedUser.userPicture
                                        Image(
                                            bitmap = BitmapFactory.decodeByteArray(
                                                pic,
                                                0,
                                                pic?.size ?: 0
                                            )
                                                .asImageBitmap(),
                                            contentDescription = "user image",
                                            contentScale = ContentScale.Crop,
                                            modifier = Modifier
                                                .padding(4.dp)
                                                .size(52.dp)
                                                .clip(CircleShape)
                                        )
                                    } else {
                                        Icon(
                                            Icons.Filled.Person,
                                            contentDescription = "user"
                                        )
                                    }
                            }
                        }
                    ) {

                    }
                },
                floatingActionButton = {
                    AnimatedVisibility(visible = isFabVisible,
                        enter = slideInVertically(initialOffsetY = { it * 2 }),
                        exit = slideOutVertically(targetOffsetY = { it * 2 })) {
                        FloatingActionButton(
                            onClick = {
                                viewModel.requestReload()
                                viewModel.ensureReloadIfNecessary()
                            },
                            shape = MaterialTheme.shapes.small
                        ) {
                            Icon(Icons.Default.Refresh, contentDescription = "refresh")
                        }
                    }
                }

            ) { contentPadding ->
                Column(modifier = Modifier.padding(contentPadding)) {
                    val iconSize = 52.dp
                    AnimatedVisibility(
                        visible = openAppBarExtra,
                    )
                    {
                        Card(
                            modifier = Modifier
                                .padding(vertical = 8.dp, horizontal = 16.dp)
                                .fillMaxWidth(),
                            shape = MaterialTheme.shapes.medium
                        ) {
                            Column(modifier = Modifier.padding(8.dp)) {
                                Row(
                                    modifier = Modifier
                                        .fillMaxWidth()
                                        .padding(12.dp),
                                    verticalAlignment = Alignment.CenterVertically,
                                    horizontalArrangement = Arrangement.SpaceBetween
                                ) {
                                    if (refreshUser) {
                                        Box(
                                            modifier = Modifier
                                                .border(
                                                    width = 2.dp,
                                                    color = Color(0xFF14bf00),
                                                    shape = CircleShape
                                                )
                                                .size(iconSize)
                                                .padding(2.dp),
                                            contentAlignment = Alignment.Center
                                        ) {
                                            if (viewModel.mainViewModel?.userViewModel?.openedUser?.userPicture?.isNotEmpty() == true) {
                                                val pic =
                                                    viewModel.mainViewModel.userViewModel.openedUser.userPicture
                                                Image(
                                                    bitmap = BitmapFactory.decodeByteArray(
                                                        pic,
                                                        0,
                                                        pic?.size ?: 0
                                                    )
                                                        .asImageBitmap(),
                                                    contentDescription = "user image",
                                                    contentScale = ContentScale.Crop,
                                                    modifier = Modifier
                                                        .padding(4.dp)
                                                        .size(iconSize)
                                                        .clip(CircleShape)
                                                )
                                            } else {
                                                Icon(
                                                    Icons.Filled.Person,
                                                    contentDescription = "user"
                                                )
                                            }
                                        }
                                        Card(
                                            modifier = Modifier
                                                .padding(horizontal = 4.dp)
                                                .fillMaxWidth(0.7f),
                                            shape = MaterialTheme.shapes.small,
                                        ) {
                                            LazyRow {
                                                if (viewModel.mainViewModel?.userViewModel?.userList?.isNotEmpty() == true) {
                                                    items(viewModel.mainViewModel.userViewModel.userList) { user ->
                                                        if (user.id != viewModel.mainViewModel.userViewModel.openedUser.id) {
                                                            Image(
                                                                bitmap = BitmapFactory.decodeByteArray(
                                                                    user.userPicture,
                                                                    0,
                                                                    user.userPicture?.size ?: 0
                                                                )
                                                                    .asImageBitmap(),
                                                                contentDescription = "selected image",
                                                                contentScale = ContentScale.Crop,
                                                                modifier = Modifier
                                                                    .padding(4.dp)
                                                                    .size(iconSize)
                                                                    .clip(CircleShape)
                                                                    .combinedClickable(
                                                                        onClick = {
                                                                            viewModel.mainViewModel.userViewModel.openUser(
                                                                                user
                                                                            )
                                                                            refreshUser =
                                                                                false
                                                                        })
                                                            )
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        Box(
                                            modifier = Modifier
                                                .size(iconSize)
                                        ) {
                                            IconButton(
                                                modifier = Modifier.fillMaxSize(),
                                                onClick = {
                                                    openAppBarExtra = false
                                                    navController?.navigate("user")
                                                }) {
                                                Icon(
                                                    Icons.Filled.Add,
                                                    contentDescription = "N/A"
                                                )
                                            }
                                        }
                                    }
                                }
                                TextButton(modifier = Modifier.fillMaxWidth(),
                                    onClick = {
                                        navController?.navigate("settings")
                                    }
                                ) {
                                    Row(modifier = Modifier.fillMaxWidth()) {
                                        Icon(
                                            Icons.Filled.Settings,
                                            contentDescription = "Settings"
                                        )
                                        Text(
                                            text = "Settings",
                                            modifier = Modifier
                                                .align(Alignment.CenterVertically)
                                                .padding(start = 8.dp)
                                        )
                                    }
                                }
                            }
                        }
                    }
                    Box {
                        val list = remember {
                            viewModel.gameList
                        }
                        val isLoading = remember {
                            viewModel.isLoading
                        }
                        viewModel.filter(query.value)

                        if (!isPreview) {
                            var settings = QuickSettings(viewModel.activity!!)

                            if (isLoading.value) {
                                Box(modifier = Modifier.fillMaxSize())
                                {
                                    CircularProgressIndicator(
                                        modifier = Modifier
                                            .width(64.dp)
                                            .align(Alignment.Center),
                                        color = MaterialTheme.colorScheme.secondary,
                                        trackColor = MaterialTheme.colorScheme.surfaceVariant
                                    )
                                }
                            } else {
                                if (settings.isGrid) {
                                    val size =
                                        GridImageSize / Resources.getSystem().displayMetrics.density
                                    LazyVerticalGrid(
                                        columns = GridCells.Adaptive(minSize = (size + 4).dp),
                                        modifier = Modifier
                                            .fillMaxSize()
                                            .padding(4.dp)
                                            .nestedScroll(nestedScrollConnection),
                                        horizontalArrangement = Arrangement.SpaceEvenly
                                    ) {
                                        items(list) {
                                            it.titleName?.apply {
                                                if (this.isNotEmpty() && (query.value.trim()
                                                        .isEmpty() || this.lowercase(Locale.getDefault())
                                                        .contains(query.value))
                                                )
                                                    GridGameItem(
                                                        it,
                                                        viewModel,
                                                        showAppActions,
                                                        showLoading,
                                                        selectedModel,
                                                        showError
                                                    )
                                            }
                                        }
                                    }
                                } else {
                                    LazyColumn(Modifier.fillMaxSize()) {
                                        items(list) {
                                            it.titleName?.apply {
                                                if (this.isNotEmpty() && (query.value.trim()
                                                        .isEmpty() || this.lowercase(
                                                        Locale.getDefault()
                                                    )
                                                        .contains(query.value))
                                                )
                                                    Box(modifier = Modifier.animateItemPlacement()) {
                                                        ListGameItem(
                                                            it,
                                                            viewModel,
                                                            showAppActions,
                                                            showLoading,
                                                            selectedModel,
                                                            showError
                                                        )
                                                    }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (showLoading.value) {
                    BasicAlertDialog(onDismissRequest = { }) {
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
                                Text(text = "Loading")
                                LinearProgressIndicator(
                                    modifier = Modifier
                                        .fillMaxWidth()
                                        .padding(top = 16.dp)
                                )
                            }

                        }
                    }
                }
                if (openTitleUpdateDialog.value) {
                    BasicAlertDialog(onDismissRequest = {
                        openTitleUpdateDialog.value = false
                    }) {
                        Surface(
                            modifier = Modifier
                                .wrapContentWidth()
                                .wrapContentHeight(),
                            shape = MaterialTheme.shapes.large,
                            tonalElevation = AlertDialogDefaults.TonalElevation
                        ) {
                            val titleId = viewModel.mainViewModel?.selected?.titleId ?: ""
                            val name = viewModel.mainViewModel?.selected?.titleName ?: ""
                            TitleUpdateViews.Main(titleId, name, openTitleUpdateDialog, canClose)
                        }

                    }
                }
                if (openDlcDialog.value) {
                    BasicAlertDialog(onDismissRequest = {
                        openDlcDialog.value = false
                    }) {
                        Surface(
                            modifier = Modifier
                                .wrapContentWidth()
                                .wrapContentHeight(),
                            shape = MaterialTheme.shapes.large,
                            tonalElevation = AlertDialogDefaults.TonalElevation
                        ) {
                            val titleId = viewModel.mainViewModel?.selected?.titleId ?: ""
                            val name = viewModel.mainViewModel?.selected?.titleName ?: ""
                            DlcViews.Main(titleId, name, openDlcDialog)
                        }

                    }
                }
            }

            if (showAppActions.value)
                ModalBottomSheet(
                    content = {
                        Row(
                            modifier = Modifier.padding(8.dp),
                            horizontalArrangement = Arrangement.SpaceEvenly
                        ) {
                            if (showAppActions.value) {
                                IconButton(onClick = {
                                    if (viewModel.mainViewModel?.selected != null) {
                                        thread {
                                            showLoading.value = true
                                            val success =
                                                viewModel.mainViewModel.loadGame(viewModel.mainViewModel.selected!!)
                                            if (success == 1) {
                                                launchOnUiThread {
                                                    viewModel.mainViewModel.navigateToGame()
                                                }
                                            } else {
                                                if (success == -2)
                                                    showError.value =
                                                        "Error loading update. Please re-add update file"
                                                viewModel.mainViewModel.selected!!.close()
                                            }
                                            showLoading.value = false
                                        }
                                    }
                                }) {
                                    Icon(
                                        org.ryujinx.android.Icons.playArrow(MaterialTheme.colorScheme.onSurface),
                                        contentDescription = "Run"
                                    )
                                }
                                val showAppMenu = remember { mutableStateOf(false) }
                                Box {
                                    IconButton(onClick = {
                                        showAppMenu.value = true
                                    }) {
                                        Icon(
                                            Icons.Filled.Menu,
                                            contentDescription = "Menu"
                                        )
                                    }
                                    DropdownMenu(
                                        expanded = showAppMenu.value,
                                        onDismissRequest = { showAppMenu.value = false }) {
                                        DropdownMenuItem(text = {
                                            Text(text = "Clear PPTC Cache")
                                        }, onClick = {
                                            showAppMenu.value = false
                                            viewModel.mainViewModel?.clearPptcCache(
                                                viewModel.mainViewModel.selected?.titleId ?: ""
                                            )
                                        })
                                        DropdownMenuItem(text = {
                                            Text(text = "Purge Shader Cache")
                                        }, onClick = {
                                            showAppMenu.value = false
                                            viewModel.mainViewModel?.purgeShaderCache(
                                                viewModel.mainViewModel.selected?.titleId ?: ""
                                            )
                                        })
                                        DropdownMenuItem(text = {
                                            Text(text = "Delete All Cache")
                                        }, onClick = {
                                            showAppMenu.value = false
                                            viewModel.mainViewModel?.deleteCache(
                                                viewModel.mainViewModel.selected?.titleId ?: ""
                                            )
                                        })
                                        DropdownMenuItem(text = {
                                            Text(text = "Manage Updates")
                                        }, onClick = {
                                            showAppMenu.value = false
                                            openTitleUpdateDialog.value = true
                                        })
                                        DropdownMenuItem(text = {
                                            Text(text = "Manage DLC")
                                        }, onClick = {
                                            showAppMenu.value = false
                                            openDlcDialog.value = true
                                        })
                                    }
                                }
                            }
                        }
                    },
                    onDismissRequest = {
                        showAppActions.value = false
                        selectedModel.value = null
                    }
                )
        }

        @OptIn(ExperimentalFoundationApi::class)
        @Composable
        fun ListGameItem(
            gameModel: GameModel,
            viewModel: HomeViewModel,
            showAppActions: MutableState<Boolean>,
            showLoading: MutableState<Boolean>,
            selectedModel: MutableState<GameModel?>,
            showError: MutableState<String>
        ) {
            remember {
                selectedModel
            }
            val color =
                if (selectedModel.value == gameModel) MaterialTheme.colorScheme.primary else MaterialTheme.colorScheme.surface

            val decoder = Base64.getDecoder()
            Surface(
                shape = MaterialTheme.shapes.medium,
                color = color,
                modifier = Modifier
                    .fillMaxWidth()
                    .padding(8.dp)
                    .combinedClickable(
                        onClick = {
                            if (viewModel.mainViewModel?.selected != null) {
                                showAppActions.value = false
                                viewModel.mainViewModel.apply {
                                    selected = null
                                }
                                selectedModel.value = null
                            } else if (gameModel.titleId.isNullOrEmpty() || gameModel.titleId != "0000000000000000" || gameModel.type == FileType.Nro) {
                                thread {
                                    showLoading.value = true
                                    val success =
                                        viewModel.mainViewModel?.loadGame(gameModel) ?: false
                                    if (success == 1) {
                                        launchOnUiThread {
                                            viewModel.mainViewModel?.navigateToGame()
                                        }
                                    } else {
                                        if (success == -2)
                                            showError.value =
                                                "Error loading update. Please re-add update file"
                                        gameModel.close()
                                    }
                                    showLoading.value = false
                                }
                            }
                        },
                        onLongClick = {
                            viewModel.mainViewModel?.selected = gameModel
                            showAppActions.value = true
                            selectedModel.value = gameModel
                        })
            ) {
                Row(
                    modifier = Modifier
                        .fillMaxWidth()
                        .padding(8.dp),
                    horizontalArrangement = Arrangement.SpaceBetween
                ) {
                    Row {
                        if (!gameModel.titleId.isNullOrEmpty() && (gameModel.titleId != "0000000000000000" || gameModel.type == FileType.Nro)) {
                            if (gameModel.icon?.isNotEmpty() == true) {
                                val pic = decoder.decode(gameModel.icon)
                                val size =
                                    ListImageSize / Resources.getSystem().displayMetrics.density
                                Image(
                                    bitmap = BitmapFactory.decodeByteArray(pic, 0, pic.size)
                                        .asImageBitmap(),
                                    contentDescription = gameModel.titleName + " icon",
                                    modifier = Modifier
                                        .padding(end = 8.dp)
                                        .width(size.roundToInt().dp)
                                        .height(size.roundToInt().dp)
                                )
                            } else if (gameModel.type == FileType.Nro)
                                NROIcon()
                            else NotAvailableIcon()
                        } else NotAvailableIcon()
                        Column {
                            Text(text = gameModel.titleName ?: "")
                            Text(text = gameModel.developer ?: "")
                            Text(text = gameModel.titleId ?: "")
                        }
                    }
                    Column {
                        Text(text = gameModel.version ?: "")
                        Text(text = String.format("%.3f", gameModel.fileSize))
                    }
                }
            }
        }

        @OptIn(ExperimentalFoundationApi::class)
        @Composable
        fun GridGameItem(
            gameModel: GameModel,
            viewModel: HomeViewModel,
            showAppActions: MutableState<Boolean>,
            showLoading: MutableState<Boolean>,
            selectedModel: MutableState<GameModel?>,
            showError: MutableState<String>
        ) {
            remember {
                selectedModel
            }
            val color =
                if (selectedModel.value == gameModel) MaterialTheme.colorScheme.primary else MaterialTheme.colorScheme.surface

            val decoder = Base64.getDecoder()
            Surface(
                shape = MaterialTheme.shapes.medium,
                color = color,
                modifier = Modifier
                    .fillMaxWidth()
                    .padding(8.dp)
                    .combinedClickable(
                        onClick = {
                            if (viewModel.mainViewModel?.selected != null) {
                                showAppActions.value = false
                                viewModel.mainViewModel.apply {
                                    selected = null
                                }
                                selectedModel.value = null
                            } else if (gameModel.titleId.isNullOrEmpty() || gameModel.titleId != "0000000000000000" || gameModel.type == FileType.Nro) {
                                thread {
                                    showLoading.value = true
                                    val success =
                                        viewModel.mainViewModel?.loadGame(gameModel) ?: false
                                    if (success == 1) {
                                        launchOnUiThread {
                                            viewModel.mainViewModel?.navigateToGame()
                                        }
                                    } else {
                                        if (success == -2)
                                            showError.value =
                                                "Error loading update. Please re-add update file"
                                        gameModel.close()
                                    }
                                    showLoading.value = false
                                }
                            }
                        },
                        onLongClick = {
                            viewModel.mainViewModel?.selected = gameModel
                            showAppActions.value = true
                            selectedModel.value = gameModel
                        })
            ) {
                Column(modifier = Modifier.padding(4.dp)) {
                    if (!gameModel.titleId.isNullOrEmpty() && (gameModel.titleId != "0000000000000000" || gameModel.type == FileType.Nro)) {
                        if (gameModel.icon?.isNotEmpty() == true) {
                            val pic = decoder.decode(gameModel.icon)
                            val size = GridImageSize / Resources.getSystem().displayMetrics.density
                            Image(
                                bitmap = BitmapFactory.decodeByteArray(pic, 0, pic.size)
                                    .asImageBitmap(),
                                contentDescription = gameModel.titleName + " icon",
                                modifier = Modifier
                                    .padding(0.dp)
                                    .clip(RoundedCornerShape(16.dp))
                                    .align(Alignment.CenterHorizontally)
                            )
                        } else if (gameModel.type == FileType.Nro)
                            NROIcon()
                        else NotAvailableIcon()
                    } else NotAvailableIcon()
                    Text(
                        text = gameModel.titleName ?: "N/A",
                        maxLines = 1,
                        overflow = TextOverflow.Ellipsis,
                        modifier = Modifier
                            .padding(vertical = 4.dp)
                            .basicMarquee()
                    )
                }
            }
        }

        @Composable
        fun NotAvailableIcon() {
            val size = ListImageSize / Resources.getSystem().displayMetrics.density
            Icon(
                Icons.Filled.Add,
                contentDescription = "N/A",
                modifier = Modifier
                    .padding(end = 8.dp)
                    .width(size.roundToInt().dp)
                    .height(size.roundToInt().dp)
            )
        }

        @Composable
        fun NROIcon() {
            val size = ListImageSize / Resources.getSystem().displayMetrics.density
            Image(
                painter = painterResource(id = R.drawable.icon_nro),
                contentDescription = "NRO",
                modifier = Modifier
                    .padding(end = 8.dp)
                    .width(size.roundToInt().dp)
                    .height(size.roundToInt().dp)
            )
        }

    }

    @Preview
    @Composable
    fun HomePreview() {
        Home(isPreview = true)
    }
}
