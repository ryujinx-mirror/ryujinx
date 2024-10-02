package org.ryujinx.android

import androidx.compose.foundation.layout.padding
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.MaterialTheme
import androidx.compose.runtime.Composable
import androidx.compose.runtime.remember
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.graphics.PathFillType
import androidx.compose.ui.graphics.SolidColor
import androidx.compose.ui.graphics.StrokeCap
import androidx.compose.ui.graphics.StrokeJoin
import androidx.compose.ui.graphics.vector.ImageVector
import androidx.compose.ui.graphics.vector.path
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import compose.icons.CssGgIcons
import compose.icons.cssggicons.Games

class Icons {
    companion object {
        /// Icons exported from https://www.composables.com/icons
        @Composable
        fun circle(color: Color): ImageVector {
            return remember {
                ImageVector.Builder(
                    name = "circle",
                    defaultWidth = 40.0.dp,
                    defaultHeight = 40.0.dp,
                    viewportWidth = 40.0f,
                    viewportHeight = 40.0f
                ).apply {
                    path(
                        fill = SolidColor(color),
                        fillAlpha = 1f,
                        stroke = null,
                        strokeAlpha = 1f,
                        strokeLineWidth = 1.0f,
                        strokeLineCap = StrokeCap.Butt,
                        strokeLineJoin = StrokeJoin.Miter,
                        strokeLineMiter = 1f,
                        pathFillType = PathFillType.NonZero
                    )  {
                        moveTo(20f, 36.375f)
                        quadToRelative(-3.375f, 0f, -6.375f, -1.292f)
                        quadToRelative(-3f, -1.291f, -5.208f, -3.521f)
                        quadToRelative(-2.209f, -2.229f, -3.5f, -5.208f)
                        quadTo(3.625f, 23.375f, 3.625f, 20f)
                        quadToRelative(0f, -3.417f, 1.292f, -6.396f)
                        quadToRelative(1.291f, -2.979f, 3.521f, -5.208f)
                        quadToRelative(2.229f, -2.229f, 5.208f, -3.5f)
                        reflectiveQuadTo(20f, 3.625f)
                        quadToRelative(3.417f, 0f, 6.396f, 1.292f)
                        quadToRelative(2.979f, 1.291f, 5.208f, 3.5f)
                        quadToRelative(2.229f, 2.208f, 3.5f, 5.187f)
                        reflectiveQuadTo(36.375f, 20f)
                        quadToRelative(0f, 3.375f, -1.292f, 6.375f)
                        quadToRelative(-1.291f, 3f, -3.5f, 5.208f)
                        quadToRelative(-2.208f, 2.209f, -5.187f, 3.5f)
                        quadToRelative(-2.979f, 1.292f, -6.396f, 1.292f)
                        close()
                        moveToRelative(0f, -2.625f)
                        quadToRelative(5.75f, 0f, 9.75f, -4.021f)
                        reflectiveQuadToRelative(4f, -9.729f)
                        quadToRelative(0f, -5.75f, -4f, -9.75f)
                        reflectiveQuadToRelative(-9.75f, -4f)
                        quadToRelative(-5.708f, 0f, -9.729f, 4f)
                        quadToRelative(-4.021f, 4f, -4.021f, 9.75f)
                        quadToRelative(0f, 5.708f, 4.021f, 9.729f)
                        quadTo(14.292f, 33.75f, 20f, 33.75f)
                        close()
                        moveTo(20f, 20f)
                        close()
                    }
                }.build()
            }
        }
        @Composable
        fun listView(color: Color): ImageVector {
            return remember {
                ImageVector.Builder(
                    name = "list",
                    defaultWidth = 40.0.dp,
                    defaultHeight = 40.0.dp,
                    viewportWidth = 40.0f,
                    viewportHeight = 40.0f
                ).apply {
                    path(
                        fill = SolidColor(color),
                        fillAlpha = 1f,
                        stroke = null,
                        strokeAlpha = 1f,
                        strokeLineWidth = 1.0f,
                        strokeLineCap = StrokeCap.Butt,
                        strokeLineJoin = StrokeJoin.Miter,
                        strokeLineMiter = 1f,
                        pathFillType = PathFillType.NonZero
                    ) {
                        moveTo(13.375f, 14.458f)
                        quadToRelative(-0.583f, 0f, -0.958f, -0.395f)
                        quadToRelative(-0.375f, -0.396f, -0.375f, -0.938f)
                        quadToRelative(0f, -0.542f, 0.375f, -0.937f)
                        quadToRelative(0.375f, -0.396f, 0.958f, -0.396f)
                        horizontalLineToRelative(20.083f)
                        quadToRelative(0.584f, 0f, 0.959f, 0.396f)
                        quadToRelative(0.375f, 0.395f, 0.375f, 0.937f)
                        reflectiveQuadToRelative(-0.375f, 0.938f)
                        quadToRelative(-0.375f, 0.395f, -0.959f, 0.395f)
                        close()
                        moveToRelative(0f, 6.834f)
                        quadToRelative(-0.583f, 0f, -0.958f, -0.375f)
                        reflectiveQuadTo(12.042f, 20f)
                        quadToRelative(0f, -0.583f, 0.375f, -0.958f)
                        reflectiveQuadToRelative(0.958f, -0.375f)
                        horizontalLineToRelative(20.083f)
                        quadToRelative(0.584f, 0f, 0.959f, 0.395f)
                        quadToRelative(0.375f, 0.396f, 0.375f, 0.938f)
                        quadToRelative(0f, 0.542f, -0.375f, 0.917f)
                        reflectiveQuadToRelative(-0.959f, 0.375f)
                        close()
                        moveToRelative(0f, 6.916f)
                        quadToRelative(-0.583f, 0f, -0.958f, -0.396f)
                        quadToRelative(-0.375f, -0.395f, -0.375f, -0.937f)
                        reflectiveQuadToRelative(0.375f, -0.937f)
                        quadToRelative(0.375f, -0.396f, 0.958f, -0.396f)
                        horizontalLineToRelative(20.083f)
                        quadToRelative(0.584f, 0f, 0.959f, 0.396f)
                        quadToRelative(0.375f, 0.395f, 0.375f, 0.937f)
                        reflectiveQuadToRelative(-0.375f, 0.937f)
                        quadToRelative(-0.375f, 0.396f, -0.959f, 0.396f)
                        close()
                        moveToRelative(-6.833f, -13.75f)
                        quadToRelative(-0.584f, 0f, -0.959f, -0.395f)
                        quadToRelative(-0.375f, -0.396f, -0.375f, -0.938f)
                        quadToRelative(0f, -0.583f, 0.375f, -0.958f)
                        reflectiveQuadToRelative(0.959f, -0.375f)
                        quadToRelative(0.583f, 0f, 0.958f, 0.375f)
                        reflectiveQuadToRelative(0.375f, 0.958f)
                        quadToRelative(0f, 0.542f, -0.375f, 0.938f)
                        quadToRelative(-0.375f, 0.395f, -0.958f, 0.395f)
                        close()
                        moveToRelative(0f, 6.875f)
                        quadToRelative(-0.584f, 0f, -0.959f, -0.375f)
                        reflectiveQuadTo(5.208f, 20f)
                        quadToRelative(0f, -0.583f, 0.375f, -0.958f)
                        reflectiveQuadToRelative(0.959f, -0.375f)
                        quadToRelative(0.583f, 0f, 0.958f, 0.375f)
                        reflectiveQuadToRelative(0.375f, 0.958f)
                        quadToRelative(0f, 0.583f, -0.375f, 0.958f)
                        reflectiveQuadToRelative(-0.958f, 0.375f)
                        close()
                        moveToRelative(0f, 6.875f)
                        quadToRelative(-0.584f, 0f, -0.959f, -0.375f)
                        reflectiveQuadToRelative(-0.375f, -0.958f)
                        quadToRelative(0f, -0.542f, 0.375f, -0.937f)
                        quadToRelative(0.375f, -0.396f, 0.959f, -0.396f)
                        quadToRelative(0.583f, 0f, 0.958f, 0.396f)
                        quadToRelative(0.375f, 0.395f, 0.375f, 0.937f)
                        quadToRelative(0f, 0.583f, -0.375f, 0.958f)
                        reflectiveQuadToRelative(-0.958f, 0.375f)
                        close()
                    }
                }.build()
            }
        }

        @Composable
        fun gridView(color: Color): ImageVector {
            return remember {
                ImageVector.Builder(
                    name = "grid_view",
                    defaultWidth = 40.0.dp,
                    defaultHeight = 40.0.dp,
                    viewportWidth = 40.0f,
                    viewportHeight = 40.0f
                ).apply {
                    path(
                        fill = SolidColor(color),
                        fillAlpha = 1f,
                        stroke = null,
                        strokeAlpha = 1f,
                        strokeLineWidth = 1.0f,
                        strokeLineCap = StrokeCap.Butt,
                        strokeLineJoin = StrokeJoin.Miter,
                        strokeLineMiter = 1f,
                        pathFillType = PathFillType.NonZero
                    ) {
                        moveTo(7.875f, 18.667f)
                        quadToRelative(-1.083f, 0f, -1.854f, -0.771f)
                        quadToRelative(-0.771f, -0.771f, -0.771f, -1.854f)
                        verticalLineTo(7.875f)
                        quadToRelative(0f, -1.083f, 0.771f, -1.854f)
                        quadToRelative(0.771f, -0.771f, 1.854f, -0.771f)
                        horizontalLineToRelative(8.167f)
                        quadToRelative(1.083f, 0f, 1.875f, 0.771f)
                        quadToRelative(0.791f, 0.771f, 0.791f, 1.854f)
                        verticalLineToRelative(8.167f)
                        quadToRelative(0f, 1.083f, -0.791f, 1.854f)
                        quadToRelative(-0.792f, 0.771f, -1.875f, 0.771f)
                        close()
                        moveToRelative(0f, 16.083f)
                        quadToRelative(-1.083f, 0f, -1.854f, -0.771f)
                        quadToRelative(-0.771f, -0.771f, -0.771f, -1.854f)
                        verticalLineToRelative(-8.167f)
                        quadToRelative(0f, -1.083f, 0.771f, -1.875f)
                        quadToRelative(0.771f, -0.791f, 1.854f, -0.791f)
                        horizontalLineToRelative(8.167f)
                        quadToRelative(1.083f, 0f, 1.875f, 0.791f)
                        quadToRelative(0.791f, 0.792f, 0.791f, 1.875f)
                        verticalLineToRelative(8.167f)
                        quadToRelative(0f, 1.083f, -0.791f, 1.854f)
                        quadToRelative(-0.792f, 0.771f, -1.875f, 0.771f)
                        close()
                        moveToRelative(16.083f, -16.083f)
                        quadToRelative(-1.083f, 0f, -1.854f, -0.771f)
                        quadToRelative(-0.771f, -0.771f, -0.771f, -1.854f)
                        verticalLineTo(7.875f)
                        quadToRelative(0f, -1.083f, 0.771f, -1.854f)
                        quadToRelative(0.771f, -0.771f, 1.854f, -0.771f)
                        horizontalLineToRelative(8.167f)
                        quadToRelative(1.083f, 0f, 1.854f, 0.771f)
                        quadToRelative(0.771f, 0.771f, 0.771f, 1.854f)
                        verticalLineToRelative(8.167f)
                        quadToRelative(0f, 1.083f, -0.771f, 1.854f)
                        quadToRelative(-0.771f, 0.771f, -1.854f, 0.771f)
                        close()
                        moveToRelative(0f, 16.083f)
                        quadToRelative(-1.083f, 0f, -1.854f, -0.771f)
                        quadToRelative(-0.771f, -0.771f, -0.771f, -1.854f)
                        verticalLineToRelative(-8.167f)
                        quadToRelative(0f, -1.083f, 0.771f, -1.875f)
                        quadToRelative(0.771f, -0.791f, 1.854f, -0.791f)
                        horizontalLineToRelative(8.167f)
                        quadToRelative(1.083f, 0f, 1.854f, 0.791f)
                        quadToRelative(0.771f, 0.792f, 0.771f, 1.875f)
                        verticalLineToRelative(8.167f)
                        quadToRelative(0f, 1.083f, -0.771f, 1.854f)
                        quadToRelative(-0.771f, 0.771f, -1.854f, 0.771f)
                        close()
                        moveTo(7.875f, 16.042f)
                        horizontalLineToRelative(8.167f)
                        verticalLineTo(7.875f)
                        horizontalLineTo(7.875f)
                        close()
                        moveToRelative(16.083f, 0f)
                        horizontalLineToRelative(8.167f)
                        verticalLineTo(7.875f)
                        horizontalLineToRelative(-8.167f)
                        close()
                        moveToRelative(0f, 16.083f)
                        horizontalLineToRelative(8.167f)
                        verticalLineToRelative(-8.167f)
                        horizontalLineToRelative(-8.167f)
                        close()
                        moveToRelative(-16.083f, 0f)
                        horizontalLineToRelative(8.167f)
                        verticalLineToRelative(-8.167f)
                        horizontalLineTo(7.875f)
                        close()
                        moveToRelative(16.083f, -16.083f)
                        close()
                        moveToRelative(0f, 7.916f)
                        close()
                        moveToRelative(-7.916f, 0f)
                        close()
                        moveToRelative(0f, -7.916f)
                        close()
                    }
                }.build()
            }
        }

        @Composable
        fun applets(color: Color): ImageVector {
            return remember {
                ImageVector.Builder(
                    name = "apps",
                    defaultWidth = 40.0.dp,
                    defaultHeight = 40.0.dp,
                    viewportWidth = 40.0f,
                    viewportHeight = 40.0f
                ).apply {
                    path(
                        fill = SolidColor(color),
                        fillAlpha = 1f,
                        stroke = null,
                        strokeAlpha = 1f,
                        strokeLineWidth = 1.0f,
                        strokeLineCap = StrokeCap.Butt,
                        strokeLineJoin = StrokeJoin.Miter,
                        strokeLineMiter = 1f,
                        pathFillType = PathFillType.NonZero
                    ) {
                        moveTo(9.708f, 33.125f)
                        quadToRelative(-1.208f, 0f, -2.02f, -0.813f)
                        quadToRelative(-0.813f, -0.812f, -0.813f, -2.02f)
                        quadToRelative(0f, -1.167f, 0.813f, -2f)
                        quadToRelative(0.812f, -0.834f, 2.02f, -0.834f)
                        quadToRelative(1.167f, 0f, 2f, 0.813f)
                        quadToRelative(0.834f, 0.812f, 0.834f, 2.021f)
                        quadToRelative(0f, 1.208f, -0.813f, 2.02f)
                        quadToRelative(-0.812f, 0.813f, -2.021f, 0.813f)
                        close()
                        moveToRelative(10.292f, 0f)
                        quadToRelative(-1.167f, 0f, -1.979f, -0.813f)
                        quadToRelative(-0.813f, -0.812f, -0.813f, -2.02f)
                        quadToRelative(0f, -1.167f, 0.813f, -2f)
                        quadToRelative(0.812f, -0.834f, 1.979f, -0.834f)
                        reflectiveQuadToRelative(2f, 0.813f)
                        quadToRelative(0.833f, 0.812f, 0.833f, 2.021f)
                        quadToRelative(0f, 1.208f, -0.812f, 2.02f)
                        quadToRelative(-0.813f, 0.813f, -2.021f, 0.813f)
                        close()
                        moveToRelative(10.292f, 0f)
                        quadToRelative(-1.167f, 0f, -2f, -0.813f)
                        quadToRelative(-0.834f, -0.812f, -0.834f, -2.02f)
                        quadToRelative(0f, -1.167f, 0.813f, -2f)
                        quadToRelative(0.812f, -0.834f, 2.021f, -0.834f)
                        quadToRelative(1.208f, 0f, 2.02f, 0.813f)
                        quadToRelative(0.813f, 0.812f, 0.813f, 2.021f)
                        quadToRelative(0f, 1.208f, -0.813f, 2.02f)
                        quadToRelative(-0.812f, 0.813f, -2.02f, 0.813f)
                        close()
                        moveTo(9.708f, 22.792f)
                        quadToRelative(-1.208f, 0f, -2.02f, -0.813f)
                        quadToRelative(-0.813f, -0.812f, -0.813f, -1.979f)
                        reflectiveQuadToRelative(0.813f, -2f)
                        quadToRelative(0.812f, -0.833f, 2.02f, -0.833f)
                        quadToRelative(1.167f, 0f, 2f, 0.812f)
                        quadToRelative(0.834f, 0.813f, 0.834f, 2.021f)
                        quadToRelative(0f, 1.167f, -0.813f, 1.979f)
                        quadToRelative(-0.812f, 0.813f, -2.021f, 0.813f)
                        close()
                        moveToRelative(10.292f, 0f)
                        quadToRelative(-1.167f, 0f, -1.979f, -0.813f)
                        quadToRelative(-0.813f, -0.812f, -0.813f, -1.979f)
                        reflectiveQuadToRelative(0.813f, -2f)
                        quadToRelative(0.812f, -0.833f, 1.979f, -0.833f)
                        reflectiveQuadToRelative(2f, 0.812f)
                        quadToRelative(0.833f, 0.813f, 0.833f, 2.021f)
                        quadToRelative(0f, 1.167f, -0.812f, 1.979f)
                        quadToRelative(-0.813f, 0.813f, -2.021f, 0.813f)
                        close()
                        moveToRelative(10.292f, 0f)
                        quadToRelative(-1.167f, 0f, -2f, -0.813f)
                        quadToRelative(-0.834f, -0.812f, -0.834f, -1.979f)
                        reflectiveQuadToRelative(0.813f, -2f)
                        quadToRelative(0.812f, -0.833f, 2.021f, -0.833f)
                        quadToRelative(1.208f, 0f, 2.02f, 0.812f)
                        quadToRelative(0.813f, 0.813f, 0.813f, 2.021f)
                        quadToRelative(0f, 1.167f, -0.813f, 1.979f)
                        quadToRelative(-0.812f, 0.813f, -2.02f, 0.813f)
                        close()
                        moveTo(9.708f, 12.542f)
                        quadToRelative(-1.208f, 0f, -2.02f, -0.813f)
                        quadToRelative(-0.813f, -0.812f, -0.813f, -2.021f)
                        quadToRelative(0f, -1.208f, 0.813f, -2.02f)
                        quadToRelative(0.812f, -0.813f, 2.02f, -0.813f)
                        quadToRelative(1.167f, 0f, 2f, 0.813f)
                        quadToRelative(0.834f, 0.812f, 0.834f, 2.02f)
                        quadToRelative(0f, 1.167f, -0.813f, 2f)
                        quadToRelative(-0.812f, 0.834f, -2.021f, 0.834f)
                        close()
                        moveToRelative(10.292f, 0f)
                        quadToRelative(-1.167f, 0f, -1.979f, -0.813f)
                        quadToRelative(-0.813f, -0.812f, -0.813f, -2.021f)
                        quadToRelative(0f, -1.208f, 0.813f, -2.02f)
                        quadToRelative(0.812f, -0.813f, 1.979f, -0.813f)
                        reflectiveQuadToRelative(2f, 0.813f)
                        quadToRelative(0.833f, 0.812f, 0.833f, 2.02f)
                        quadToRelative(0f, 1.167f, -0.812f, 2f)
                        quadToRelative(-0.813f, 0.834f, -2.021f, 0.834f)
                        close()
                        moveToRelative(10.292f, 0f)
                        quadToRelative(-1.167f, 0f, -2f, -0.813f)
                        quadToRelative(-0.834f, -0.812f, -0.834f, -2.021f)
                        quadToRelative(0f, -1.208f, 0.813f, -2.02f)
                        quadToRelative(0.812f, -0.813f, 2.021f, -0.813f)
                        quadToRelative(1.208f, 0f, 2.02f, 0.813f)
                        quadToRelative(0.813f, 0.812f, 0.813f, 2.02f)
                        quadToRelative(0f, 1.167f, -0.813f, 2f)
                        quadToRelative(-0.812f, 0.834f, -2.02f, 0.834f)
                        close()
                    }
                }.build()
            }
        }

        @Composable
        fun playArrow(color: Color): ImageVector {
            return remember {
                ImageVector.Builder(
                    name = "play_arrow",
                    defaultWidth = 40.0.dp,
                    defaultHeight = 40.0.dp,
                    viewportWidth = 40.0f,
                    viewportHeight = 40.0f
                ).apply {
                    path(
                        fill = SolidColor(color),
                        fillAlpha = 1f,
                        stroke = null,
                        strokeAlpha = 1f,
                        strokeLineWidth = 1.0f,
                        strokeLineCap = StrokeCap.Butt,
                        strokeLineJoin = StrokeJoin.Miter,
                        strokeLineMiter = 1f,
                        pathFillType = PathFillType.NonZero
                    ) {
                        moveTo(15.542f, 30f)
                        quadToRelative(-0.667f, 0.458f, -1.334f, 0.062f)
                        quadToRelative(-0.666f, -0.395f, -0.666f, -1.187f)
                        verticalLineTo(10.917f)
                        quadToRelative(0f, -0.75f, 0.666f, -1.146f)
                        quadToRelative(0.667f, -0.396f, 1.334f, 0.062f)
                        lineToRelative(14.083f, 9f)
                        quadToRelative(0.583f, 0.375f, 0.583f, 1.084f)
                        quadToRelative(0f, 0.708f, -0.583f, 1.083f)
                        close()
                        moveToRelative(0.625f, -10.083f)
                        close()
                        moveToRelative(0f, 6.541f)
                        lineToRelative(10.291f, -6.541f)
                        lineToRelative(-10.291f, -6.542f)
                        close()
                    }
                }.build()
            }
        }

        @Composable
        fun folderOpen(color: Color): ImageVector {
            return remember {
                ImageVector.Builder(
                    name = "folder_open",
                    defaultWidth = 40.0.dp,
                    defaultHeight = 40.0.dp,
                    viewportWidth = 40.0f,
                    viewportHeight = 40.0f
                ).apply {
                    path(
                        fill = SolidColor(color),
                        fillAlpha = 1f,
                        stroke = null,
                        strokeAlpha = 1f,
                        strokeLineWidth = 1.0f,
                        strokeLineCap = StrokeCap.Butt,
                        strokeLineJoin = StrokeJoin.Miter,
                        strokeLineMiter = 1f,
                        pathFillType = PathFillType.NonZero
                    ) {
                        moveTo(6.25f, 33.125f)
                        quadToRelative(-1.083f, 0f, -1.854f, -0.792f)
                        quadToRelative(-0.771f, -0.791f, -0.771f, -1.875f)
                        verticalLineTo(9.667f)
                        quadToRelative(0f, -1.084f, 0.771f, -1.854f)
                        quadToRelative(0.771f, -0.771f, 1.854f, -0.771f)
                        horizontalLineToRelative(10.042f)
                        quadToRelative(0.541f, 0f, 1.041f, 0.208f)
                        quadToRelative(0.5f, 0.208f, 0.834f, 0.583f)
                        lineToRelative(1.875f, 1.834f)
                        horizontalLineTo(33.75f)
                        quadToRelative(1.083f, 0f, 1.854f, 0.791f)
                        quadToRelative(0.771f, 0.792f, 0.771f, 1.834f)
                        horizontalLineTo(18.917f)
                        lineTo(16.25f, 9.667f)
                        horizontalLineToRelative(-10f)
                        verticalLineTo(30.25f)
                        lineToRelative(3.542f, -13.375f)
                        quadToRelative(0.25f, -0.875f, 0.979f, -1.396f)
                        quadToRelative(0.729f, -0.521f, 1.604f, -0.521f)
                        horizontalLineToRelative(23.25f)
                        quadToRelative(1.292f, 0f, 2.104f, 1.021f)
                        quadToRelative(0.813f, 1.021f, 0.438f, 2.271f)
                        lineToRelative(-3.459f, 12.833f)
                        quadToRelative(-0.291f, 1f, -1f, 1.521f)
                        quadToRelative(-0.708f, 0.521f, -1.75f, 0.521f)
                        close()
                        moveToRelative(2.708f, -2.667f)
                        horizontalLineToRelative(23.167f)
                        lineToRelative(3.417f, -12.875f)
                        horizontalLineTo(12.333f)
                        close()
                        moveToRelative(0f, 0f)
                        lineToRelative(3.375f, -12.875f)
                        lineToRelative(-3.375f, 12.875f)
                        close()
                        moveToRelative(-2.708f, -15.5f)
                        verticalLineTo(9.667f)
                        verticalLineToRelative(5.291f)
                        close()
                    }
                }.build()
            }
        }

        @Composable
        fun gameUpdate(): ImageVector {
            val primaryColor = MaterialTheme.colorScheme.primary
            return remember {
                ImageVector.Builder(
                    name = "game_update_alt",
                    defaultWidth = 40.0.dp,
                    defaultHeight = 40.0.dp,
                    viewportWidth = 40.0f,
                    viewportHeight = 40.0f
                ).apply {
                    path(
                        fill = SolidColor(Color.Black.copy(alpha = 0.5f)),
                        stroke = SolidColor(primaryColor),
                        fillAlpha = 1f,
                        strokeAlpha = 1f,
                        strokeLineWidth = 1.0f,
                        strokeLineCap = StrokeCap.Butt,
                        strokeLineJoin = StrokeJoin.Miter,
                        strokeLineMiter = 1f,
                        pathFillType = PathFillType.NonZero
                    ) {
                        moveTo(6.25f, 33.083f)
                        quadToRelative(-1.083f, 0f, -1.854f, -0.791f)
                        quadToRelative(-0.771f, -0.792f, -0.771f, -1.834f)
                        verticalLineTo(9.542f)
                        quadToRelative(0f, -1.042f, 0.771f, -1.854f)
                        quadToRelative(0.771f, -0.813f, 1.854f, -0.813f)
                        horizontalLineToRelative(8.458f)
                        quadToRelative(0.584f, 0f, 0.959f, 0.396f)
                        reflectiveQuadToRelative(0.375f, 0.937f)
                        quadToRelative(0f, 0.584f, -0.375f, 0.959f)
                        reflectiveQuadToRelative(-0.959f, 0.375f)
                        horizontalLineTo(6.25f)
                        verticalLineToRelative(20.916f)
                        horizontalLineToRelative(27.542f)
                        verticalLineTo(9.542f)
                        horizontalLineToRelative(-8.5f)
                        quadToRelative(-0.584f, 0f, -0.959f, -0.375f)
                        reflectiveQuadToRelative(-0.375f, -0.959f)
                        quadToRelative(0f, -0.541f, 0.375f, -0.937f)
                        reflectiveQuadToRelative(0.959f, -0.396f)
                        horizontalLineToRelative(8.5f)
                        quadToRelative(1.041f, 0f, 1.833f, 0.813f)
                        quadToRelative(0.792f, 0.812f, 0.792f, 1.854f)
                        verticalLineToRelative(20.916f)
                        quadToRelative(0f, 1.042f, -0.792f, 1.834f)
                        quadToRelative(-0.792f, 0.791f, -1.833f, 0.791f)
                        close()
                        moveTo(20f, 25f)
                        quadToRelative(-0.25f, 0f, -0.479f, -0.083f)
                        quadToRelative(-0.229f, -0.084f, -0.396f, -0.292f)
                        lineTo(12.75f, 18.25f)
                        quadToRelative(-0.375f, -0.333f, -0.375f, -0.896f)
                        quadToRelative(0f, -0.562f, 0.417f, -0.979f)
                        quadToRelative(0.375f, -0.375f, 0.916f, -0.375f)
                        quadToRelative(0.542f, 0f, 0.959f, 0.375f)
                        lineToRelative(4.041f, 4.083f)
                        verticalLineTo(8.208f)
                        quadToRelative(0f, -0.541f, 0.375f, -0.937f)
                        reflectiveQuadTo(20f, 6.875f)
                        quadToRelative(0.542f, 0f, 0.938f, 0.396f)
                        quadToRelative(0.395f, 0.396f, 0.395f, 0.937f)
                        verticalLineToRelative(12.25f)
                        lineToRelative(4.084f, -4.083f)
                        quadToRelative(0.333f, -0.333f, 0.875f, -0.333f)
                        quadToRelative(0.541f, 0f, 0.916f, 0.375f)
                        quadToRelative(0.417f, 0.416f, 0.417f, 0.958f)
                        reflectiveQuadToRelative(-0.375f, 0.917f)
                        lineToRelative(-6.333f, 6.333f)
                        quadToRelative(-0.209f, 0.208f, -0.438f, 0.292f)
                        quadTo(20.25f, 25f, 20f, 25f)
                        close()
                    }
                }.build()
            }
        }

        @Composable
        fun download(): ImageVector {
            val primaryColor = MaterialTheme.colorScheme.primary
            return remember {
                ImageVector.Builder(
                    name = "download",
                    defaultWidth = 40.0.dp,
                    defaultHeight = 40.0.dp,
                    viewportWidth = 40.0f,
                    viewportHeight = 40.0f
                ).apply {
                    path(
                        fill = SolidColor(Color.Black.copy(alpha = 0.5f)),
                        stroke = SolidColor(primaryColor),
                        fillAlpha = 1f,
                        strokeAlpha = 1f,
                        strokeLineWidth = 1.0f,
                        strokeLineCap = StrokeCap.Butt,
                        strokeLineJoin = StrokeJoin.Miter,
                        strokeLineMiter = 1f,
                        pathFillType = PathFillType.NonZero
                    ) {
                        moveTo(20f, 26.25f)
                        quadToRelative(-0.25f, 0f, -0.479f, -0.083f)
                        quadToRelative(-0.229f, -0.084f, -0.438f, -0.292f)
                        lineToRelative(-6.041f, -6.083f)
                        quadToRelative(-0.417f, -0.375f, -0.396f, -0.917f)
                        quadToRelative(0.021f, -0.542f, 0.396f, -0.917f)
                        reflectiveQuadToRelative(0.916f, -0.396f)
                        quadToRelative(0.542f, -0.02f, 0.959f, 0.396f)
                        lineToRelative(3.791f, 3.792f)
                        verticalLineTo(8.292f)
                        quadToRelative(0f, -0.584f, 0.375f, -0.959f)
                        reflectiveQuadTo(20f, 6.958f)
                        quadToRelative(0.542f, 0f, 0.938f, 0.375f)
                        quadToRelative(0.395f, 0.375f, 0.395f, 0.959f)
                        verticalLineTo(21.75f)
                        lineToRelative(3.792f, -3.792f)
                        quadToRelative(0.375f, -0.416f, 0.917f, -0.396f)
                        quadToRelative(0.541f, 0.021f, 0.958f, 0.396f)
                        quadToRelative(0.375f, 0.375f, 0.375f, 0.917f)
                        reflectiveQuadToRelative(-0.375f, 0.958f)
                        lineToRelative(-6.083f, 6.042f)
                        quadToRelative(-0.209f, 0.208f, -0.438f, 0.292f)
                        quadToRelative(-0.229f, 0.083f, -0.479f, 0.083f)
                        close()
                        moveTo(9.542f, 32.958f)
                        quadToRelative(-1.042f, 0f, -1.834f, -0.791f)
                        quadToRelative(-0.791f, -0.792f, -0.791f, -1.834f)
                        verticalLineToRelative(-4.291f)
                        quadToRelative(0f, -0.542f, 0.395f, -0.938f)
                        quadToRelative(0.396f, -0.396f, 0.938f, -0.396f)
                        quadToRelative(0.542f, 0f, 0.917f, 0.396f)
                        reflectiveQuadToRelative(0.375f, 0.938f)
                        verticalLineToRelative(4.291f)
                        horizontalLineToRelative(20.916f)
                        verticalLineToRelative(-4.291f)
                        quadToRelative(0f, -0.542f, 0.375f, -0.938f)
                        quadToRelative(0.375f, -0.396f, 0.917f, -0.396f)
                        quadToRelative(0.583f, 0f, 0.958f, 0.396f)
                        reflectiveQuadToRelative(0.375f, 0.938f)
                        verticalLineToRelative(4.291f)
                        quadToRelative(0f, 1.042f, -0.791f, 1.834f)
                        quadToRelative(-0.792f, 0.791f, -1.834f, 0.791f)
                        close()
                    }
                }.build()
            }
        }

        @Composable
        fun vSync(): ImageVector {
            val primaryColor = MaterialTheme.colorScheme.primary
            return remember {
                ImageVector.Builder(
                    name = "60fps",
                    defaultWidth = 40.0.dp,
                    defaultHeight = 40.0.dp,
                    viewportWidth = 40.0f,
                    viewportHeight = 40.0f
                ).apply {
                    path(
                        fill = SolidColor(Color.Black.copy(alpha = 0.5f)),
                        stroke = SolidColor(primaryColor),
                        fillAlpha = 1f,
                        strokeAlpha = 1f,
                        strokeLineWidth = 1.0f,
                        strokeLineCap = StrokeCap.Butt,
                        strokeLineJoin = StrokeJoin.Miter,
                        strokeLineMiter = 1f,
                        pathFillType = PathFillType.NonZero
                    ) {
                        moveTo(7.292f, 31.458f)
                        quadToRelative(-1.542f, 0f, -2.625f, -1.041f)
                        quadToRelative(-1.084f, -1.042f, -1.084f, -2.625f)
                        verticalLineTo(12.208f)
                        quadToRelative(0f, -1.583f, 1.084f, -2.625f)
                        quadTo(5.75f, 8.542f, 7.292f, 8.542f)
                        horizontalLineTo(14f)
                        quadToRelative(0.75f, 0f, 1.292f, 0.541f)
                        quadToRelative(0.541f, 0.542f, 0.541f, 1.292f)
                        reflectiveQuadToRelative(-0.541f, 1.292f)
                        quadToRelative(-0.542f, 0.541f, -1.292f, 0.541f)
                        horizontalLineTo(7.208f)
                        verticalLineToRelative(5.084f)
                        horizontalLineToRelative(6.709f)
                        quadToRelative(1.541f, 0f, 2.583f, 1.041f)
                        quadToRelative(1.042f, 1.042f, 1.042f, 2.625f)
                        verticalLineToRelative(6.834f)
                        quadToRelative(0f, 1.583f, -1.042f, 2.625f)
                        quadToRelative(-1.042f, 1.041f, -2.583f, 1.041f)
                        close()
                        moveToRelative(-0.084f, -10.5f)
                        verticalLineToRelative(6.834f)
                        horizontalLineToRelative(6.709f)
                        verticalLineToRelative(-6.834f)
                        close()
                        moveToRelative(17.125f, 6.834f)
                        horizontalLineToRelative(8.459f)
                        verticalLineTo(12.208f)
                        horizontalLineToRelative(-8.459f)
                        verticalLineToRelative(15.584f)
                        close()
                        moveToRelative(0f, 3.666f)
                        quadToRelative(-1.541f, 0f, -2.583f, -1.041f)
                        quadToRelative(-1.042f, -1.042f, -1.042f, -2.625f)
                        verticalLineTo(12.208f)
                        quadToRelative(0f, -1.583f, 1.042f, -2.625f)
                        quadToRelative(1.042f, -1.041f, 2.583f, -1.041f)
                        horizontalLineToRelative(8.459f)
                        quadToRelative(1.541f, 0f, 2.583f, 1.041f)
                        quadToRelative(1.042f, 1.042f, 1.042f, 2.625f)
                        verticalLineToRelative(15.584f)
                        quadToRelative(0f, 1.583f, -1.042f, 2.625f)
                        quadToRelative(-1.042f, 1.041f, -2.583f, 1.041f)
                        close()
                    }
                }.build()
            }
        }

        @Composable
        fun videoGame(): ImageVector {
            val primaryColor = MaterialTheme.colorScheme.primary
            return remember {
                ImageVector.Builder(
                    name = "videogame_asset",
                    defaultWidth = 40.0.dp,
                    defaultHeight = 40.0.dp,
                    viewportWidth = 40.0f,
                    viewportHeight = 40.0f
                ).apply {
                    path(
                        fill = SolidColor(Color.Black.copy(alpha = 0.5f)),
                        stroke = SolidColor(primaryColor),
                        fillAlpha = 1f,
                        strokeAlpha = 1f,
                        strokeLineWidth = 1.0f,
                        strokeLineCap = StrokeCap.Butt,
                        strokeLineJoin = StrokeJoin.Miter,
                        strokeLineMiter = 1f,
                        pathFillType = PathFillType.NonZero
                    ) {
                        moveTo(6.25f, 29.792f)
                        quadToRelative(-1.083f, 0f, -1.854f, -0.792f)
                        quadToRelative(-0.771f, -0.792f, -0.771f, -1.833f)
                        verticalLineTo(12.833f)
                        quadToRelative(0f, -1.083f, 0.771f, -1.854f)
                        quadToRelative(0.771f, -0.771f, 1.854f, -0.771f)
                        horizontalLineToRelative(27.5f)
                        quadToRelative(1.083f, 0f, 1.854f, 0.771f)
                        quadToRelative(0.771f, 0.771f, 0.771f, 1.854f)
                        verticalLineToRelative(14.334f)
                        quadToRelative(0f, 1.041f, -0.771f, 1.833f)
                        reflectiveQuadToRelative(-1.854f, 0.792f)
                        close()
                        moveToRelative(0f, -2.625f)
                        horizontalLineToRelative(27.5f)
                        verticalLineTo(12.833f)
                        horizontalLineTo(6.25f)
                        verticalLineToRelative(14.334f)
                        close()
                        moveToRelative(7.167f, -1.792f)
                        quadToRelative(0.541f, 0f, 0.916f, -0.375f)
                        reflectiveQuadToRelative(0.375f, -0.917f)
                        verticalLineToRelative(-2.791f)
                        horizontalLineToRelative(2.75f)
                        quadToRelative(0.584f, 0f, 0.959f, -0.375f)
                        reflectiveQuadToRelative(0.375f, -0.917f)
                        quadToRelative(0f, -0.542f, -0.375f, -0.938f)
                        quadToRelative(-0.375f, -0.395f, -0.959f, -0.395f)
                        horizontalLineToRelative(-2.75f)
                        verticalLineToRelative(-2.75f)
                        quadToRelative(0f, -0.542f, -0.375f, -0.938f)
                        quadToRelative(-0.375f, -0.396f, -0.916f, -0.396f)
                        quadToRelative(-0.584f, 0f, -0.959f, 0.396f)
                        reflectiveQuadToRelative(-0.375f, 0.938f)
                        verticalLineToRelative(2.75f)
                        horizontalLineToRelative(-2.75f)
                        quadToRelative(-0.541f, 0f, -0.937f, 0.395f)
                        quadTo(8f, 19.458f, 8f, 20f)
                        quadToRelative(0f, 0.542f, 0.396f, 0.917f)
                        reflectiveQuadToRelative(0.937f, 0.375f)
                        horizontalLineToRelative(2.75f)
                        verticalLineToRelative(2.791f)
                        quadToRelative(0f, 0.542f, 0.396f, 0.917f)
                        reflectiveQuadToRelative(0.938f, 0.375f)
                        close()
                        moveToRelative(11.125f, -0.5f)
                        quadToRelative(0.791f, 0f, 1.396f, -0.583f)
                        quadToRelative(0.604f, -0.584f, 0.604f, -1.375f)
                        quadToRelative(0f, -0.834f, -0.604f, -1.417f)
                        quadToRelative(-0.605f, -0.583f, -1.396f, -0.583f)
                        quadToRelative(-0.834f, 0f, -1.417f, 0.583f)
                        quadToRelative(-0.583f, 0.583f, -0.583f, 1.375f)
                        quadToRelative(0f, 0.833f, 0.583f, 1.417f)
                        quadToRelative(0.583f, 0.583f, 1.417f, 0.583f)
                        close()
                        moveToRelative(3.916f, -5.833f)
                        quadToRelative(0.834f, 0f, 1.417f, -0.584f)
                        quadToRelative(0.583f, -0.583f, 0.583f, -1.416f)
                        quadToRelative(0f, -0.792f, -0.583f, -1.375f)
                        quadToRelative(-0.583f, -0.584f, -1.417f, -0.584f)
                        quadToRelative(-0.791f, 0f, -1.375f, 0.584f)
                        quadToRelative(-0.583f, 0.583f, -0.583f, 1.375f)
                        quadToRelative(0f, 0.833f, 0.583f, 1.416f)
                        quadToRelative(0.584f, 0.584f, 1.375f, 0.584f)
                        close()
                        moveTo(6.25f, 27.167f)
                        verticalLineTo(12.833f)
                        verticalLineToRelative(14.334f)
                        close()
                    }
                }.build()
            }
        }
    }
}

@Preview
@Composable
fun Preview() {
    IconButton(modifier = Modifier.padding(4.dp), onClick = {
    }) {
        Icon(
            imageVector = CssGgIcons.Games,
            contentDescription = "Open Panel"
        )
    }
}
