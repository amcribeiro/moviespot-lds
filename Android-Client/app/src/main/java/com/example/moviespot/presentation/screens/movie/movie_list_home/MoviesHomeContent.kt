package com.example.moviespot.presentation.screens.movie.movie_list_home

import androidx.compose.foundation.background
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.verticalScroll
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.unit.dp
import com.example.moviespot.data.dto.MovieDto
import com.example.moviespot.presentation.screens.movie.movie_list_home.components.CoverFlowSection
import com.example.moviespot.presentation.screens.movie.movie_list_home.components.TrendingList

@Composable
fun MoviesHomeContent(
    modifier: Modifier = Modifier,
    movies: List<MovieDto>,
    onMovieClick: (Int) -> Unit,
    onNavigateToFullList: () -> Unit
) {
    Column(
        modifier = modifier
            .fillMaxSize()
            .verticalScroll(rememberScrollState())
            .background(Color(0xFF121212))
    ) {
        CoverFlowSection(
            movies = movies,
            onMovieClick = onMovieClick
        )

        Spacer(Modifier.height(24.dp))

        TrendingList(
            movies = movies.take(10),
            onMovieClick = onMovieClick,
            onViewAllClick = { onNavigateToFullList() }
        )

        Spacer(Modifier.height(40.dp))
    }
}
