package com.example.moviespot.presentation.screens.seats

import androidx.compose.foundation.background
import androidx.compose.foundation.border
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.ArrowBack
import androidx.compose.material3.Button
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Surface
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp

@Composable
fun SeatSelectionRoute(
    sessionId: Int,
    viewModel: SeatSelectionViewModel,
    onBack: () -> Unit,
    onConfirm: (selectedSeatIds: List<Int>) -> Unit
) {
    LaunchedEffect(sessionId) {
        viewModel.load(sessionId)
    }

    val state = viewModel.uiState

    SeatSelectionScreen(
        state = state,
        onBack = onBack,
        onSeatClick = { viewModel.onSeatClicked(it) },
        onConfirm = {
            onConfirm(
                state.selectedSeatIds.toList()
            )
        }
    )
}

@Composable
fun SeatSelectionScreen(
    state: SeatSelectionUiState,
    onBack: () -> Unit,
    onSeatClick: (seatId: Int) -> Unit,
    onConfirm: () -> Unit
) {
    Surface(
        modifier = Modifier.fillMaxSize(),
        color = MaterialTheme.colorScheme.background
    ) {
        Column(
            modifier = Modifier
                .fillMaxSize()
                .padding(16.dp)
        ) {
            Row(
                verticalAlignment = Alignment.CenterVertically
            ) {
                IconButton(onClick = onBack) {
                    Icon(
                        imageVector = Icons.AutoMirrored.Filled.ArrowBack,
                        contentDescription = "Voltar"
                    )
                }
                Text(
                    text = "Selecionar Lugares",
                    style = MaterialTheme.typography.titleLarge,
                    fontWeight = FontWeight.Bold
                )
            }

            Spacer(modifier = Modifier.height(8.dp))

            state.session?.let { session ->
                Text(
                    text = session.movieTitle ?: "Sessão #${session.id}",
                    style = MaterialTheme.typography.titleMedium,
                    fontWeight = FontWeight.SemiBold
                )
                Text(
                    text = "Sala: ${session.cinemaHallName ?: "#${session.cinemaHallId}"}",
                    style = MaterialTheme.typography.bodyMedium
                )
                Text(
                    text = "Preço base: %.2f €".format(session.price),
                    style = MaterialTheme.typography.bodyMedium
                )

                Spacer(modifier = Modifier.height(8.dp))
            }

            when {
                state.isLoading -> {
                    Box(
                        modifier = Modifier
                            .fillMaxSize()
                            .weight(1f),
                        contentAlignment = Alignment.Center
                    ) {
                        androidx.compose.material3.CircularProgressIndicator()
                    }
                }

                state.error != null -> {
                    Box(
                        modifier = Modifier
                            .fillMaxSize()
                            .weight(1f),
                        contentAlignment = Alignment.Center
                    ) {
                        Text(
                            text = state.error,
                            color = Color.Red,
                            fontWeight = FontWeight.SemiBold
                        )
                    }
                }

                else -> {
                    SeatsLegend()

                    Spacer(modifier = Modifier.height(8.dp))

                    SeatGrid(
                        seats = state.seats,
                        selectedSeatIds = state.selectedSeatIds,
                        onSeatClick = onSeatClick,
                        modifier = Modifier.weight(1f)
                    )

                    Spacer(modifier = Modifier.height(8.dp))

                    SeatDetailsPanel(state = state)

                    Spacer(modifier = Modifier.height(8.dp))

                    if (state.maxSelectedMessage != null) {
                        Text(
                            text = state.maxSelectedMessage,
                            color = Color.Red,
                            fontSize = 13.sp
                        )
                        Spacer(modifier = Modifier.height(4.dp))
                    }

                    SummaryBar(
                        totalPrice = state.totalPrice,
                        seatCount = state.selectedSeatIds.size,
                        canConfirm = state.canConfirm,
                        onConfirm = onConfirm
                    )
                }
            }
        }
    }
}

@Composable
private fun SeatsLegend() {
    Column(
        modifier = Modifier
            .fillMaxWidth()
            .padding(vertical = 6.dp),
        horizontalAlignment = Alignment.CenterHorizontally
    ) {
        Row(horizontalArrangement = Arrangement.spacedBy(12.dp)) {
            LegendItem("Normal", Color(0xFF1E88E5))
            LegendItem("VIP", Color(0xFFD4AF37))
            LegendItem("Reduced", Color(0xFF9C27B0))
        }
        Spacer(modifier = Modifier.height(4.dp))
        Row(horizontalArrangement = Arrangement.spacedBy(12.dp)) {
            LegendItem("Reservado", Color(0xFF424242))
            LegendItem("Selecionado", Color(0xFF50C878))
        }
    }
}

@Composable
private fun LegendItem(
    label: String,
    color: Color
) {
    Row(
        verticalAlignment = Alignment.CenterVertically
    ) {
        Box(
            modifier = Modifier
                .size(16.dp)
                .background(color, RoundedCornerShape(4.dp))
        )
        Spacer(modifier = Modifier.width(4.dp))
        Text(text = label, fontSize = 12.sp)
    }
}

@Composable
private fun SeatGrid(
    seats: List<SeatUiModel>,
    selectedSeatIds: Set<Int>,
    onSeatClick: (Int) -> Unit,
    modifier: Modifier = Modifier
) {
    val rows = seats.groupBy { it.row }.toSortedMap()

    LazyColumn(
        modifier = modifier.fillMaxWidth(),
        horizontalAlignment = Alignment.CenterHorizontally
    ) {
        items(rows.toList()) { (rowChar, rowSeats) ->
            Row(
                verticalAlignment = Alignment.CenterVertically,
                modifier = Modifier.padding(vertical = 4.dp)
            ) {
                Text(
                    text = rowChar.toString(),
                    modifier = Modifier.width(24.dp),
                    fontWeight = FontWeight.Bold
                )
                Spacer(modifier = Modifier.width(4.dp))

                Row {
                    rowSeats.sortedBy { it.column }.forEach { seat ->
                        SeatItem(
                            seat = seat,
                            isSelected = selectedSeatIds.contains(seat.id),
                            onClick = { onSeatClick(seat.id) }
                        )
                    }
                }
            }
        }

        item {
            Spacer(modifier = Modifier.height(18.dp))
            Box(modifier = Modifier.fillMaxWidth(), contentAlignment = Alignment.Center) {
                ScreenDecoration()
            }
        }
    }
}

@Composable
private fun SeatItem(
    seat: SeatUiModel,
    isSelected: Boolean,
    onClick: () -> Unit
) {
    val baseColor = when (seat.type.lowercase()) {
        "vip" -> Color(0xFFD4AF37)
        "reduced" -> Color(0xFF9C27B0)
        else -> Color(0xFF1E88E5)
    }

    val backgroundColor = when {
        !seat.isAvailable || seat.isReserved -> Color(0xFF424242)
        isSelected -> Color(0xFF50C878)
        else -> baseColor
    }

    val alpha = if (!seat.isAvailable || seat.isReserved) 0.4f else 1f

    Box(
        modifier = Modifier
            .padding(2.dp)
            .size(32.dp)
            .background(
                color = backgroundColor.copy(alpha = alpha),
                shape = RoundedCornerShape(6.dp)
            )
            .border(
                width = if (isSelected) 2.dp else 1.dp,
                color = if (isSelected) Color.Black else Color.DarkGray,
                shape = RoundedCornerShape(6.dp)
            )
            .clickable(
                enabled = seat.isAvailable && !seat.isReserved
            ) { onClick() },
        contentAlignment = Alignment.Center
    ) {
        Text(
            text = seat.column.toString(),
            fontSize = 11.sp,
            fontWeight = FontWeight.Bold,
            color = Color.White
        )
    }
}

@Composable
private fun SeatDetailsPanel(
    state: SeatSelectionUiState
) {
    val currentSeat = state.lastClickedSeatId?.let { id ->
        state.seats.firstOrNull { it.id == id }
    }

    if (currentSeat == null) {
        Text(
            text = "Toca num lugar para veres o preço individual.",
            style = MaterialTheme.typography.bodyMedium
        )
    } else {
        Column(
            modifier = Modifier
                .fillMaxWidth()
                .background(
                    color = MaterialTheme.colorScheme.surfaceVariant,
                    shape = RoundedCornerShape(8.dp)
                )
                .padding(8.dp)
        ) {
            Text(
                text = "Lugar ${currentSeat.seatNumber}",
                fontWeight = FontWeight.SemiBold
            )
            Text(
                text = "Tipo: ${currentSeat.type}",
                fontSize = 13.sp
            )
            Text(
                text = "Preço: %.2f €".format(currentSeat.price),
                fontSize = 13.sp,
                fontWeight = FontWeight.SemiBold
            )
        }
    }
}

@Composable
private fun SummaryBar(
    totalPrice: Double,
    seatCount: Int,
    canConfirm: Boolean,
    onConfirm: () -> Unit
) {
    Column(
        modifier = Modifier
            .fillMaxWidth()
            .background(
                color = MaterialTheme.colorScheme.surface,
                shape = RoundedCornerShape(12.dp)
            )
            .padding(12.dp)
    ) {
        Row(
            modifier = Modifier.fillMaxWidth(),
            horizontalArrangement = Arrangement.SpaceBetween,
            verticalAlignment = Alignment.CenterVertically
        ) {
            Column {
                Text(
                    text = "Lugares selecionados: $seatCount",
                    fontSize = 14.sp
                )
                Text(
                    text = "Total: %.2f €".format(totalPrice),
                    fontSize = 16.sp,
                    fontWeight = FontWeight.Bold
                )
            }

            Button(
                onClick = onConfirm,
                enabled = canConfirm
            ) {
                Text(text = "Continuar")
            }
        }
    }
}

@Composable
private fun ScreenDecoration() {
    Box(
        modifier = Modifier
            .width(190.dp)
            .height(26.dp)
            .background(Color.White, RoundedCornerShape(40))
            .padding(2.dp),
        contentAlignment = Alignment.Center
    ) {
        Text(
            text = "ECRÃ",
            fontSize = 11.sp,
            color = Color.Black,
            fontWeight = FontWeight.Bold
        )
    }
}
