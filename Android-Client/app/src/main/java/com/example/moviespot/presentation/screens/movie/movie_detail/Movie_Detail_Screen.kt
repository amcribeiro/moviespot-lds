package com.example.moviespot.presentation.screens.movie.movie_detail

import androidx.compose.foundation.*
import androidx.compose.foundation.layout.*
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.ArrowBack
import androidx.compose.material.icons.filled.*
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.graphics.vector.ImageVector
import androidx.compose.ui.layout.ContentScale
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import coil.compose.AsyncImage
import com.example.moviespot.data.dto.MovieDto
import com.example.moviespot.presentation.screens.movie.MovieViewModel

@Composable
fun MovieDetailsScreen(
    viewModel: MovieViewModel,
    onBack: () -> Unit,
    onWatchClick: () -> Unit
) {
    val movie = viewModel.selectedMovie
    val loading = viewModel.isLoadingDetails
    val error = viewModel.detailsError

    Box(
        modifier = Modifier
            .fillMaxSize()
            .background(Color(0xFF0F0F0F)),
        contentAlignment = Alignment.Center
    ) {
        when {

            loading -> CircularProgressIndicator(color = Color.Yellow)

            error != null -> Column(
                horizontalAlignment = Alignment.CenterHorizontally
            ) {

                Text(
                    error,
                    color = Color.Red,
                    fontSize = 16.sp,
                    modifier = Modifier.padding(16.dp)
                )

                Spacer(Modifier.height(16.dp))

                Button(onClick = onBack) {
                    Text("Voltar")
                }
            }

            movie != null -> MovieDetailsContent(
                movie = movie,
                onBack = onBack,
                onWatchClick = onWatchClick
            )
        }
    }
}


@Composable
fun MovieDetailsContent(
    movie: MovieDto,
    onBack: () -> Unit,
    onWatchClick: () -> Unit
) {
    val scroll = rememberScrollState()
    var selectedTab by remember { mutableIntStateOf(0) }

    Column(
        modifier = Modifier
            .fillMaxSize()
            .verticalScroll(scroll)
            .background(Color(0xFF0F0F0F))
    ) {

        Box(
            modifier = Modifier
                .fillMaxWidth()
                .height(320.dp)
        ) {
            AsyncImage(
                model = movie.posterPath,
                contentDescription = movie.title,
                modifier = Modifier.fillMaxSize(),
                contentScale = ContentScale.Crop
            )

            IconButton(
                onClick = onBack,
                modifier = Modifier
                    .padding(16.dp)
                    .align(Alignment.TopStart)
            ) {
                Icon(
                    Icons.AutoMirrored.Filled.ArrowBack,
                    contentDescription = null,
                    tint = Color.White
                )
            }
        }

        Spacer(Modifier.height(16.dp))

        val cleanDate = movie.releaseDate?.take(10) ?: "Unknown"

        Column(
            modifier = Modifier
                .fillMaxWidth()
                .padding(horizontal = 16.dp),
            horizontalAlignment = Alignment.CenterHorizontally
        ) {

            Text(
                movie.title,
                color = Color.White,
                fontWeight = FontWeight.Bold,
                fontSize = 26.sp
            )

            Spacer(Modifier.height(8.dp))

            Row(
                verticalAlignment = Alignment.CenterVertically,
                horizontalArrangement = Arrangement.Center,
                modifier = Modifier.fillMaxWidth()
            ) {

                Icon(
                    Icons.Default.Star,
                    contentDescription = null,
                    tint = Color(0xFFFFEB3B),
                    modifier = Modifier.size(18.dp)
                )

                Spacer(Modifier.width(6.dp))

                Text("No rating", color = Color.LightGray, fontSize = 14.sp)

                Spacer(Modifier.width(14.dp))

                Text(
                    "Data de lançamento: $cleanDate",
                    color = Color.LightGray,
                    fontSize = 14.sp
                )
            }
        }

        Spacer(Modifier.height(20.dp))

        Row(
            modifier = Modifier
                .fillMaxWidth()
                .padding(horizontal = 16.dp)
        ) {
            Button(
                onClick = onWatchClick,
                colors = ButtonDefaults.buttonColors(containerColor = Color(0xFFFFEB3B)),
                modifier = Modifier
                    .fillMaxWidth()
                    .height(48.dp)
            ) {
                Text("Watch ▶", color = Color.Black, fontWeight = FontWeight.Bold)
            }
        }

        Spacer(Modifier.height(26.dp))

        Row(
            modifier = Modifier
                .fillMaxWidth()
                .padding(vertical = 8.dp),
            horizontalArrangement = Arrangement.Center,
            verticalAlignment = Alignment.CenterVertically
        ) {
            ActionItemFixed(Icons.Default.BookmarkBorder, "Add List")

            Spacer(Modifier.width(32.dp))

            ActionItemFixed(Icons.Default.PlayCircle, "Trailer")

            Spacer(Modifier.width(32.dp))

            ActionItemFixed(Icons.Default.Share, "Share")
        }

        val tabs = listOf("Overview", "Casts", "Related")

        TabRow(
            selectedTabIndex = selectedTab,
            containerColor = Color(0xFF0F0F0F),
            contentColor = Color.White
        ) {
            tabs.forEachIndexed { i, text ->
                Tab(
                    selected = selectedTab == i,
                    onClick = { selectedTab = i },
                    text = {
                        Text(
                            text,
                            color = if (selectedTab == i) Color(0xFFFFEB3B) else Color.White
                        )
                    }
                )
            }
        }

        Spacer(Modifier.height(20.dp))

        when (selectedTab) {
            0 -> OverviewSection(movie)
            1 -> CastsSection()
            2 -> RelatedSection()
        }
    }
}


@Composable
fun OverviewSection(movie: MovieDto) {
    Column(modifier = Modifier.padding(horizontal = 16.dp)) {

        Text(
            movie.description,
            color = Color.LightGray,
            fontSize = 15.sp
        )

        Spacer(Modifier.height(20.dp))

        Text("Genre", color = Color.White, fontWeight = FontWeight.Bold, fontSize = 18.sp)

        Text(
            movie.genres.joinToString(),
            color = Color.LightGray,
            fontSize = 15.sp
        )
    }
}

@Composable fun CastsSection() {
    Column(modifier = Modifier.padding(16.dp)) {
        Text("No cast data available.", color = Color.LightGray)
    }
}

@Composable fun RelatedSection() {
    Column(modifier = Modifier.padding(16.dp)) {
        Text("No related movies available.", color = Color.LightGray)
    }
}

@Composable
fun ActionItemFixed(icon: ImageVector, label: String) {
    Column(
        modifier = Modifier.width(72.dp),
        horizontalAlignment = Alignment.CenterHorizontally
    ) {
        Icon(
            icon,
            contentDescription = null,
            tint = Color.White,
            modifier = Modifier.size(28.dp)
        )
        Text(
            label,
            color = Color.White,
            fontSize = 13.sp
        )
    }
}
