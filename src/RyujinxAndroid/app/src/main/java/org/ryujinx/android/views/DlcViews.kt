package org.ryujinx.android.views

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.wrapContentWidth
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Delete
import androidx.compose.material3.Checkbox
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Surface
import androidx.compose.material3.Text
import androidx.compose.material3.TextButton
import androidx.compose.runtime.Composable
import androidx.compose.runtime.MutableState
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.dp
import org.ryujinx.android.viewmodels.DlcItem
import org.ryujinx.android.viewmodels.DlcViewModel

class DlcViews {
    companion object {
        @Composable
        fun Main(titleId: String, name: String, openDialog: MutableState<Boolean>) {
            val viewModel = DlcViewModel(titleId)

            var dlcList = remember {
                mutableListOf<DlcItem>()
            }

            viewModel.data?.apply {
                dlcList.clear()
            }

            var refresh = remember {
                mutableStateOf(true)
            }

            Column(modifier = Modifier.padding(16.dp)) {
                Column {
                    Row(
                        modifier = Modifier
                            .padding(8.dp)
                            .fillMaxWidth(),
                        horizontalArrangement = Arrangement.SpaceBetween
                    ) {
                        Text(
                            text = "DLC for ${name}",
                            textAlign = TextAlign.Center,
                            modifier = Modifier.align(
                                Alignment.CenterVertically
                            )
                        )
                    }
                    Surface(
                        modifier = Modifier
                            .padding(8.dp),
                        color = MaterialTheme.colorScheme.surfaceVariant,
                        shape = MaterialTheme.shapes.medium
                    ) {

                        if (refresh.value) {
                            dlcList.clear()
                            dlcList.addAll(viewModel.getDlc())
                            refresh.value = false
                        }
                        LazyColumn(
                            modifier = Modifier
                                .fillMaxWidth()
                                .height(400.dp)
                        ) {
                            items(dlcList) { dlcItem ->
                                dlcItem.apply {
                                    Row(
                                        modifier = Modifier
                                            .padding(8.dp)
                                            .fillMaxWidth()
                                    ) {
                                        Checkbox(
                                            checked = (dlcItem.isEnabled.value),
                                            onCheckedChange = { dlcItem.isEnabled.value = it })
                                        Text(
                                            text = dlcItem.name,
                                            modifier = Modifier
                                                .align(Alignment.CenterVertically)
                                                .wrapContentWidth(Alignment.Start)
                                                .fillMaxWidth(0.9f)
                                        )
                                        IconButton(
                                            onClick = {
                                                viewModel.remove(dlcItem)
                                                refresh.value = true
                                            }) {
                                            Icon(
                                                Icons.Filled.Delete,
                                                contentDescription = "remove"
                                            )
                                        }
                                    }
                                }
                            }
                        }
                    }

                }
                Spacer(modifier = Modifier.height(8.dp))
                Row(modifier = Modifier.align(Alignment.End)) {
                    TextButton(
                        modifier = Modifier.padding(4.dp),
                        onClick = {
                            viewModel.add(refresh)
                        }
                    ) {

                        Text("Add")
                    }
                    TextButton(
                        modifier = Modifier.padding(4.dp),
                        onClick = {
                            openDialog.value = false
                            viewModel.save(dlcList)
                        },
                    ) {
                        Text("Save")
                    }
                }
            }
        }
    }
}
