package org.ryujinx.android.views

import android.graphics.BitmapFactory
import androidx.compose.foundation.ExperimentalFoundationApi
import androidx.compose.foundation.Image
import androidx.compose.foundation.combinedClickable
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.lazy.grid.GridCells
import androidx.compose.foundation.lazy.grid.LazyVerticalGrid
import androidx.compose.foundation.lazy.grid.items
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.ArrowBack
import androidx.compose.material.icons.filled.Refresh
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.material3.TopAppBar
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.graphics.asImageBitmap
import androidx.compose.ui.layout.ContentScale
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import androidx.navigation.NavHostController
import org.ryujinx.android.viewmodels.MainViewModel

class UserViews {
    companion object {
        @OptIn(ExperimentalMaterial3Api::class, ExperimentalFoundationApi::class)
        @Composable
        fun Main(viewModel: MainViewModel? = null) {
            val reload = remember {
                mutableStateOf(true)
            }

            fun refresh() {
                viewModel?.userViewModel?.refreshUsers()
                reload.value = true
            }
            LaunchedEffect(reload.value) {
                reload.value = false
            }

            Scaffold(modifier = Modifier.fillMaxSize(),
                topBar = {
                    TopAppBar(title = {
                        Text(text = "Users")
                    },
                        navigationIcon = {
                            IconButton(onClick = {
                                viewModel?.navController?.popBackStack()
                            }) {
                                Icon(Icons.AutoMirrored.Filled.ArrowBack, contentDescription = "Back")
                            }
                        })
                }) { contentPadding ->
                Box(
                    modifier = Modifier
                        .fillMaxSize()
                        .padding(contentPadding)
                ) {
                    Column(
                        modifier = Modifier
                            .fillMaxSize()
                            .padding(8.dp),
                        verticalArrangement = Arrangement.spacedBy(8.dp)
                    ) {
                        Text(text = "Selected user")
                        Row(
                            modifier = Modifier
                                .fillMaxWidth()
                                .padding(4.dp),
                            horizontalArrangement = Arrangement.spacedBy(8.dp)
                        ) {
                            if (viewModel?.userViewModel?.openedUser?.id?.isNotEmpty() == true) {
                                val openUser = viewModel.userViewModel.openedUser
                                Image(
                                    bitmap = BitmapFactory.decodeByteArray(
                                        openUser.userPicture,
                                        0,
                                        openUser.userPicture?.size ?: 0
                                    ).asImageBitmap(),
                                    contentDescription = "selected image",
                                    contentScale = ContentScale.Crop,
                                    modifier = Modifier
                                        .padding(4.dp)
                                        .size(96.dp)
                                        .clip(CircleShape)
                                )
                                Column(
                                    modifier = Modifier.fillMaxWidth(),
                                    verticalArrangement = Arrangement.spacedBy(4.dp)
                                ) {
                                    Text(text = openUser.username)
                                    Text(text = openUser.id)
                                }
                            }
                        }

                        Row(
                            modifier = Modifier.fillMaxWidth(),
                            verticalAlignment = Alignment.CenterVertically,
                            horizontalArrangement = Arrangement.SpaceBetween
                        ) {
                            Text(text = "Available Users")
                            IconButton(onClick = {
                                refresh()
                            }) {
                                Icon(
                                    imageVector = Icons.Filled.Refresh,
                                    contentDescription = "refresh users"
                                )
                            }
                        }

                        LazyVerticalGrid(
                            columns = GridCells.Adaptive(minSize = 96.dp),
                            modifier = Modifier
                                .fillMaxSize()
                                .padding(4.dp)
                        ) {
                            if (viewModel?.userViewModel?.userList?.isNotEmpty() == true) {
                                items(viewModel.userViewModel.userList) { user ->
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
                                            .fillMaxSize()
                                            .padding(4.dp)
                                            .clip(CircleShape)
                                            .align(Alignment.CenterHorizontally)
                                            .combinedClickable(
                                                onClick = {
                                                    viewModel.userViewModel.openUser(user)
                                                    reload.value = true
                                                })
                                    )
                                }
                            }
                        }
                    }

                }
            }
        }

    }

    @Preview
    @Composable
    fun Preview() {
        Main()
    }
}
