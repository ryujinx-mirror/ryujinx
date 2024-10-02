package org.ryujinx.android

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.wrapContentHeight
import androidx.compose.foundation.layout.wrapContentWidth
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.text.KeyboardActions
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.foundation.verticalScroll
import androidx.compose.material3.AlertDialogDefaults
import androidx.compose.material3.BasicAlertDialog
import androidx.compose.material3.Button
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Surface
import androidx.compose.material3.Text
import androidx.compose.material3.TextField
import androidx.compose.runtime.Composable
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.input.ImeAction
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.unit.dp
import androidx.compose.ui.window.DialogProperties
import com.halilibo.richtext.markdown.Markdown
import com.halilibo.richtext.ui.material3.RichText

enum class KeyboardMode {
    Default, Numeric, ASCII, FullLatin, Alphabet, SimplifiedChinese, TraditionalChinese, Korean, LanguageSet2, LanguageSet2Latin
}

class UiHandler {
    private var initialText: String = ""
    private var subtitle: String = ""
    private var maxLength: Int = 0
    private var minLength: Int = 0
    private var watermark: String = ""
    private var type: Int = -1
    private var mode: KeyboardMode = KeyboardMode.Default
    val showMessage = mutableStateOf(false)
    val inputText = mutableStateOf("")
    var title: String = ""
    var message: String = ""

    init {
        RyujinxNative.jnaInstance.uiHandlerSetup()
    }

    fun update(
        newTitle: String,
        newMessage: String,
        newWatermark: String,
        newType: Int,
        min: Int,
        max: Int,
        newMode: KeyboardMode,
        newSubtitle: String,
        newInitialText: String
    )
    {
        title = newTitle
        message = newMessage
        watermark = newWatermark
        type = newType
        minLength = min
        maxLength = max
        mode = newMode
        subtitle = newSubtitle
        initialText = newInitialText
        inputText.value = initialText
        showMessage.value = type > 0
    }

    @OptIn(ExperimentalMaterial3Api::class)
    @Composable
    fun Compose() {
        val showMessageListener = remember {
            showMessage
        }

        val inputListener = remember {
            inputText
        }
        val validation = remember {
            mutableStateOf("")
        }

        fun validate(): Boolean {
            if (inputText.value.isEmpty()) {
                validation.value = "Must be between ${minLength} and ${maxLength} characters"
            } else {
                return inputText.value.length < minLength || inputText.value.length > maxLength
            }

            return false
        }

        fun getInputType(): KeyboardType {
            return when (mode) {
                KeyboardMode.Default -> KeyboardType.Text
                KeyboardMode.Numeric -> KeyboardType.Decimal
                KeyboardMode.ASCII -> KeyboardType.Ascii
                else -> {
                    KeyboardType.Text
                }
            }
        }

        fun submit() {
            if (type == 2) {
                if (inputListener.value.length < minLength || inputListener.value.length > maxLength)
                    return
            }
            RyujinxNative.jnaInstance.uiHandlerSetResponse(
                true,
                if (type == 2) inputListener.value else ""
            )
            showMessageListener.value = false
        }

        if (showMessageListener.value) {
            BasicAlertDialog(
                onDismissRequest = { },
                modifier = Modifier
                    .wrapContentWidth()
                    .wrapContentHeight(),
                properties = DialogProperties(dismissOnBackPress = false, false)
            ) {
                Column {
                    Surface(
                        modifier = Modifier
                            .wrapContentWidth()
                            .wrapContentHeight(),
                        shape = MaterialTheme.shapes.large,
                        tonalElevation = AlertDialogDefaults.TonalElevation
                    ) {
                        Column(
                            modifier = Modifier
                                .padding(16.dp)
                        ) {
                            Text(text = title)
                            Column(
                                modifier = Modifier
                                    .height(128.dp)
                                    .verticalScroll(rememberScrollState())
                                    .padding(8.dp),
                                verticalArrangement = Arrangement.Center
                            ) {
                                RichText {
                                    Markdown(content = message)
                                }
                                if (type == 2) {
                                    validate()
                                    if (watermark.isNotEmpty())
                                        TextField(
                                            value = inputListener.value,
                                            onValueChange = { inputListener.value = it },
                                            modifier = Modifier
                                                .fillMaxWidth()
                                                .padding(4.dp),
                                            label = {
                                                Text(text = watermark)
                                            },
                                            keyboardOptions = KeyboardOptions(keyboardType = getInputType()),
                                            isError = validate()
                                        )
                                    else
                                        TextField(
                                            value = inputListener.value,
                                            onValueChange = { inputListener.value = it },
                                            modifier = Modifier
                                                .fillMaxWidth()
                                                .padding(4.dp),
                                            keyboardOptions = KeyboardOptions(
                                                keyboardType = getInputType(),
                                                imeAction = ImeAction.Done
                                            ),
                                            isError = validate(),
                                            singleLine = true,
                                            keyboardActions = KeyboardActions(onDone = { submit() })
                                        )
                                    if (subtitle.isNotEmpty())
                                        Text(text = subtitle)
                                    Text(text = validation.value)
                                }
                            }
                            Row(
                                horizontalArrangement = Arrangement.End,
                                modifier = Modifier
                                    .fillMaxWidth()
                            ) {
                                Button(onClick = {
                                    submit()
                                }) {
                                    Text(text = "OK")
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}

