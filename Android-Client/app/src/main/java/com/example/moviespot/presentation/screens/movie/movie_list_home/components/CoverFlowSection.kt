package com.example.moviespot.presentation.screens.movie.movie_list_home.components

import androidx.compose.foundation.layout.*
import androidx.compose.material3.Button
import androidx.compose.material3.ButtonDefaults
import androidx.compose.material3.Text
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import com.example.moviespot.data.dto.MovieDto

@Composable
fun CoverFlowSection(
    movies: List<MovieDto>,
    onMovieClick: (Int) -> Unit
) {
    if (movies.isEmpty()) {
        EmptyCoverFlowPlaceholder()
        return
    }

    var currentIndex by remember { mutableStateOf(0) }

    Column(
        modifier = Modifier
            .fillMaxWidth()
            .padding(top = 40.dp),
        horizontalAlignment = Alignment.CenterHorizontally
    ) {
        CoverFlowCarousel(
            movies = movies,
            onMovieSelected = { currentIndex = it },
            onMovieClick = onMovieClick
        )

        Spacer(Modifier.height(0.dp))

        val movie = movies.getOrNull(currentIndex) ?: return

        Text(
            movie.title,
            color = Color.White,
            fontSize = 22.sp,
            fontWeight = FontWeight.Bold,
            textAlign = TextAlign.Center,
            modifier = Modifier.fillMaxWidth()
        )

        Text(
            movie.genres.joinToString(),
            color = Color.LightGray,
            fontSize = 14.sp,
            textAlign = TextAlign.Center,
            modifier = Modifier.fillMaxWidth()
        )

        Spacer(Modifier.height(14.dp))

        Button(
            onClick = { onMovieClick(movie.id) },
            colors = ButtonDefaults.buttonColors(Color(0xFFFFEB3B)),
            modifier = Modifier
                .width(180.dp)
                .height(45.dp)
        ) {
            Text("WATCH NOW", color = Color.Black, fontWeight = FontWeight.Bold)
        }
    }
}

@Composable
private fun EmptyCoverFlowPlaceholder() {
    Column(
        modifier = Modifier
            .fillMaxWidth()
            .padding(top = 40.dp),
        horizontalAlignment = Alignment.CenterHorizontally
    ) {
        Text(
            "Sem filmes disponíveis",
            color = Color.White,
            fontSize = 20.sp,
            fontWeight = FontWeight.Bold
        )
        Text(
            "Tente ajustar os filtros",
            color = Color.LightGray,
            fontSize = 14.sp
        )
    }
}

