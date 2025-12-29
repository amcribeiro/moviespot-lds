package com.example.moviespot.presentation.screens.session

import android.Manifest
import android.content.pm.PackageManager
import android.os.Build
import androidx.annotation.RequiresApi
import androidx.activity.compose.rememberLauncherForActivityResult
import androidx.activity.result.contract.ActivityResultContracts
import androidx.compose.foundation.background
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.ArrowBack
import androidx.compose.material.icons.filled.DateRange
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.lifecycle.viewmodel.compose.viewModel
import com.example.moviespot.presentation.screens.sessions.components.CinemaAccordion
import com.example.moviespot.presentation.utils.LocationViewModel
import com.example.moviespot.presentation.utils.haversine
import com.example.moviespot.presentation.utils.readableDistance
import com.example.moviespot.presentation.utils.openInMaps
import java.time.Instant
import java.time.ZoneId

@RequiresApi(Build.VERSION_CODES.O)
@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun SessionAccordionScreen(
    movieId: Int,
    viewModel: SessionViewModel,
    onBack: () -> Unit,
    onSessionClick: (sessionId: Int) -> Unit,
    locationVM: LocationViewModel = viewModel()
) {
    val accordions by viewModel.accordions.collectAsState()
    val loading by viewModel.loading.collectAsState()
    val error by viewModel.error.collectAsState()

    val userLat by locationVM.userLat.collectAsState()
    val userLon by locationVM.userLon.collectAsState()

    val context = LocalContext.current

    val datePickerState = rememberDatePickerState()

    val permissionLauncher = rememberLauncherForActivityResult(
        ActivityResultContracts.RequestPermission()
    ) { granted ->
        if (granted) locationVM.fetchUserLocation(context)
    }

    LaunchedEffect(movieId) {
        viewModel.load(movieId)
        locationVM.fetchUserLocation(context)
    }

    Scaffold(
        topBar = {
            Row(
                Modifier
                    .fillMaxWidth()
                    .padding(12.dp),
                verticalAlignment = Alignment.CenterVertically
            ) {
                IconButton(onClick = onBack) {
                    Icon(Icons.AutoMirrored.Filled.ArrowBack, null, tint = Color.White)
                }

                Text(
                    text = if (viewModel.isFiltered)
                        "Sessões em ${viewModel.selectedDate}"
                    else "Onde assistir?",
                    color = Color.White,
                    fontSize = MaterialTheme.typography.titleLarge.fontSize,
                    fontWeight = FontWeight.Bold,
                    modifier = Modifier.weight(1f)
                )

                IconButton(onClick = { viewModel.showDatePicker = true }) {
                    Icon(Icons.Default.DateRange, null, tint = Color.White)
                }
            }
        },
        containerColor = Color(0xFF121212)
    ) { padding ->

        if (viewModel.showDatePicker) {
            DatePickerDialog(
                onDismissRequest = { viewModel.showDatePicker = false },
                confirmButton = {
                    TextButton(onClick = {
                        val newDate = datePickerState.selectedDateMillis?.let {
                            Instant.ofEpochMilli(it).atZone(ZoneId.systemDefault()).toLocalDate().toString()
                        }
                        if (newDate != null) viewModel.filterByDay(newDate)
                        viewModel.showDatePicker = false
                    }) { Text("OK") }
                },
                dismissButton = {
                    TextButton(onClick = { viewModel.showDatePicker = false }) { Text("Cancelar") }
                }
            ) {
                DatePicker(state = datePickerState)
            }
        }

        when {
            loading -> Box(Modifier.fillMaxSize().padding(padding), Alignment.Center) {
                CircularProgressIndicator(color = Color.Yellow)
            }

            error != null -> Box(Modifier.fillMaxSize(), Alignment.Center) {
                Text(error ?: "Erro", color = Color.Red)
            }

            else -> {
                if (accordions.isEmpty()) {

                    Box(
                        modifier = Modifier.fillMaxSize(),
                        contentAlignment = Alignment.Center
                    ) {
                        Text(
                            "Não existem sessões disponíveis para este filme.",
                            color = Color.LightGray,
                            fontSize = 16.sp
                        )
                    }

                } else {

                    Column(
                        Modifier
                            .padding(padding)
                            .fillMaxSize()
                            .background(Color(0xFF121212))
                    ) {

                        if (viewModel.isFiltered) {
                            TextButton(
                                onClick = { viewModel.clearFilter() },
                                modifier = Modifier.align(Alignment.End)
                            ) {
                                Text("Limpar filtro", color = Color(0xFFFFEB3B))
                            }
                        }

                        Button(
                            onClick = {

                                val hasPermission =
                                    context.checkSelfPermission(Manifest.permission.ACCESS_FINE_LOCATION) ==
                                            PackageManager.PERMISSION_GRANTED

                                if (!hasPermission) {
                                    permissionLauncher.launch(Manifest.permission.ACCESS_FINE_LOCATION)
                                    return@Button
                                }

                                locationVM.requestEnableGps(

                                    context = context,

                                    onEnabled = {
                                        locationVM.fetchUserLocation(
                                            context = context,

                                            onSuccess = {
                                                val lat = userLat
                                                val lon = userLon

                                                if (lat != null && lon != null) {
                                                    viewModel.sortByDistance(lat, lon)
                                                }
                                            },

                                            onFail = {
                                            }
                                        )
                                    },

                                    onCancelled = {
                                    }
                                )
                            },
                            modifier = Modifier
                                .fillMaxWidth()
                                .padding(16.dp),
                            colors = ButtonDefaults.buttonColors(
                                containerColor = Color(0xFFFFEB3B),
                                contentColor = Color.Black
                            )
                        ) {
                            Text("Ordenar por distância", fontWeight = FontWeight.Bold)
                        }

                        LazyColumn {
                            items(accordions.size) { i ->
                                val data = accordions[i]
                                val distTxt =
                                    if (userLat != null && userLon != null)
                                        readableDistance(
                                            haversine(
                                                userLat!!,
                                                userLon!!,
                                                data.cinema.latitude,
                                                data.cinema.longitude
                                            )
                                        )
                                    else null

                                CinemaAccordion(
                                    cinemaData = data,
                                    distanceText = distTxt,
                                    onOpenMaps = {
                                        context.openInMaps(
                                            data.cinema.latitude,
                                            data.cinema.longitude,
                                            data.cinema.name
                                        )
                                    },
                                    onSessionClick = onSessionClick
                                )
                            }
                        }
                    }
                }
            }
        }
    }
}
