package com.example.moviespot.presentation.screens.movie.movie_list_home

import androidx.compose.foundation.background
import androidx.compose.foundation.layout.*
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import com.example.moviespot.presentation.screens.movie.MovieViewModel

@Composable
fun Movie_List_Home_Screen(
    modifier: Modifier = Modifier,
    viewModel: MovieViewModel,
    onMovieClick: (Int) -> Unit,
    onNavigateToFullList: () -> Unit
) {
    LaunchedEffect(Unit) { viewModel.loadMovies() }

    val movies = viewModel.movies
    val loading = viewModel.isLoadingMovies
    val error = viewModel.moviesError

    Box(
        modifier = modifier
            .fillMaxSize()
            .background(Color(0xFF121212))
    ) {
        when {
            loading -> CircularProgressIndicator(Modifier.align(Alignment.Center))

            error != null -> Text(
                error,
                color = Color.Red,
                modifier = Modifier.align(Alignment.Center)
            )

            else -> MoviesHomeContent(
                modifier = Modifier.fillMaxSize(),
                movies,
                onMovieClick,
                onNavigateToFullList
            )
        }
    }
}
