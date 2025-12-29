package com.example.moviespot.presentation.screens.cinemas

import android.annotation.SuppressLint
import android.content.Context
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.material3.CircularProgressIndicator
import androidx.compose.material3.Text
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.unit.dp
import com.example.moviespot.presentation.utils.*
import com.example.moviespot.presentation.utils.LocationViewModel

@SuppressLint("MissingPermission")
@Composable
fun CinemaListScreen(
    context: Context,
    viewModel: CinemaViewModel,
    locationVM: LocationViewModel,
    onCinemaClick: (id: Int) -> Unit
) {

    val cinemas by viewModel.cinemas.collectAsState()
    val loading by viewModel.loading.collectAsState()
    val error by viewModel.error.collectAsState()

    val userLat by locationVM.userLat.collectAsState()
    val userLon by locationVM.userLon.collectAsState()

    LaunchedEffect(Unit) {
        viewModel.load()

        locationVM.fetchUserLocation(context) {
            if (userLat != null && userLon != null) {
                viewModel.sortByDistance(userLat!!, userLon!!)
            }
        }
    }

    Box(
        modifier = Modifier.fillMaxSize()
    ) {

        if (loading) {
            CircularProgressIndicator(
                modifier = Modifier.align(Alignment.Center)
            )
            return@Box
        }

        if (error != null) {
            Text(
                text = error!!,
                color = Color.Red,
                modifier = Modifier.align(Alignment.Center)
            )
            return@Box
        }

        if (cinemas.isEmpty()) {
            Text(
                text = "Não existem cinemas para mostrar.",
                modifier = Modifier.align(Alignment.Center),
                color = Color.LightGray
            )
            return@Box
        }

        LazyColumn(
            modifier = Modifier.padding(top = 10.dp, bottom = 14.dp)
        ) {

            items(cinemas.size) { i ->
                val c = cinemas[i]

                val distance = if (userLat != null && userLon != null)
                    readableDistance(
                        haversine(
                            userLat!!,
                            userLon!!,
                            c.latitude,
                            c.longitude
                        )
                    )
                else null

                CinemaListCard(
                    cinema = c,
                    distance = distance,
                    onOpenMaps = {
                        context.openInMaps(
                            c.latitude,
                            c.longitude,
                            c.name
                        )
                    },
                    onClick = {
                        onCinemaClick(c.id)
                    }
                )
            }
        }
    }
}
