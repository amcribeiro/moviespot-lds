package com.example.moviespot.presentation.screens.movie.movie_list_home.components

import android.annotation.SuppressLint
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyRow
import androidx.compose.foundation.lazy.itemsIndexed
import androidx.compose.foundation.lazy.rememberLazyListState
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.graphics.graphicsLayer
import androidx.compose.ui.layout.ContentScale
import androidx.compose.ui.platform.LocalDensity
import androidx.compose.ui.unit.dp
import coil.compose.AsyncImage
import kotlinx.coroutines.delay
import kotlinx.coroutines.launch
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.ui.platform.LocalConfiguration
import com.example.moviespot.data.dto.MovieDto
import kotlin.math.abs

@SuppressLint("ConfigurationScreenWidthHeight", "FrequentlyChangingValue",
    "UnrememberedMutableState"
)
@Composable
fun CoverFlowCarousel(
    movies: List<MovieDto>,
    onMovieSelected: (Int) -> Unit,
    onMovieClick: (Int) -> Unit,
    autoScrollMs: Long = 3000
) {
    if (movies.isEmpty()) return

    val SIZE = movies.size
    val EXTENDED_LIST = remember { movies + movies + movies }

    val listState = rememberLazyListState(initialFirstVisibleItemIndex = SIZE)
    val coroutine = rememberCoroutineScope()
    val density = LocalDensity.current
    val itemWidth = 168.dp
    val itemHeight = 240.dp
    val cornerRadius = RoundedCornerShape(16.dp)
    val itemSpacing = 10.dp

    val screenWidth = LocalConfiguration.current.screenWidthDp.dp
    val sidePadding = (screenWidth - itemWidth) / 2

    LaunchedEffect(Unit) {
        while (true) {
            delay(autoScrollMs)
            coroutine.launch {
                listState.animateScrollToItem(listState.firstVisibleItemIndex + 1)
            }
        }
    }

    LaunchedEffect(listState.firstVisibleItemIndex) {
        val index = listState.firstVisibleItemIndex
        if (index >= SIZE * 2) {
            coroutine.launch { listState.scrollToItem(index - SIZE) }
        } else if (index < SIZE) {
            coroutine.launch { listState.scrollToItem(index + SIZE) }
        }
    }

    val centeredIndex by derivedStateOf {
        val layout = listState.layoutInfo
        val viewportCenter = (layout.viewportStartOffset + layout.viewportEndOffset) / 2f

        layout.visibleItemsInfo.minByOrNull { item ->
            val itemCenter = item.offset + item.size / 2f
            abs(itemCenter - viewportCenter)
        }?.index ?: SIZE
    }

    LaunchedEffect(centeredIndex) {
        onMovieSelected(centeredIndex % SIZE)
    }

    LazyRow(
        state = listState,
        modifier = Modifier
            .fillMaxWidth()
            .height(itemHeight + 20.dp),
        horizontalArrangement = Arrangement.spacedBy(itemSpacing),
        contentPadding = PaddingValues(horizontal = sidePadding)
    ) {
        itemsIndexed(EXTENDED_LIST) { index, movie ->

            val layout = listState.layoutInfo
            val visible = layout.visibleItemsInfo
            val viewportCenter =
                (layout.viewportStartOffset + layout.viewportEndOffset) / 2f

            val itemInfo = visible.find { it.index == index }

            val normDist = if (itemInfo != null) {
                val center = itemInfo.offset + itemInfo.size / 2f
                ((center - viewportCenter) / with(density) { itemWidth.toPx() })
                    .coerceIn(-1f, 1f)
            } else 0f

            val scale = 1f - 0.35f * abs(normDist)
            val alpha = 1f - 0.50f * abs(normDist)

            Box(
                modifier = Modifier
                    .width(itemWidth)
                    .height(itemHeight)
                    .graphicsLayer {
                        scaleX = scale
                        scaleY = scale
                        this.alpha = alpha
                    }
                    .clickable { onMovieClick(movie.id) },
                contentAlignment = Alignment.Center
            ) {
                AsyncImage(
                    model = movie.posterPath,
                    contentDescription = movie.title,
                    contentScale = ContentScale.Crop,
                    modifier = Modifier
                        .fillMaxSize()
                        .clip(cornerRadius)
                )
            }
        }
    }
}
