package com.example.moviespot.presentation.screens.seats

import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.setValue
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.example.moviespot.data.dto.AvailableSeatDto
import com.example.moviespot.data.dto.SeatResponseDto
import com.example.moviespot.data.dto.SessionResponseDto
import com.example.moviespot.data.repository.SeatRepository
import com.example.moviespot.data.repository.SessionRepository
import kotlinx.coroutines.async
import kotlinx.coroutines.awaitAll
import kotlinx.coroutines.coroutineScope
import kotlinx.coroutines.launch
import retrofit2.HttpException

data class SeatUiModel(
    val id: Int,
    val seatNumber: String,
    val row: Char,
    val column: Int,
    val type: String,
    val isAvailable: Boolean,
    val isReserved: Boolean,
    val price: Double
)

data class SeatSelectionUiState(
    val isLoading: Boolean = false,
    val error: String? = null,
    val session: SessionResponseDto? = null,
    val seats: List<SeatUiModel> = emptyList(),
    val selectedSeatIds: Set<Int> = emptySet(),
    val lastClickedSeatId: Int? = null,
    val maxSelectedMessage: String? = null
) {
    val totalPrice: Double
        get() = seats.filter { selectedSeatIds.contains(it.id) }.sumOf { it.price }

    val canConfirm: Boolean
        get() = selectedSeatIds.isNotEmpty()
}

class SeatSelectionViewModel(
    private val sessionRepository: SessionRepository,
    private val seatRepository: SeatRepository
) : ViewModel() {

    var uiState by mutableStateOf(SeatSelectionUiState())

    fun load(sessionId: Int) {

        uiState = uiState.copy(isLoading = true, error = null)

        viewModelScope.launch {

            try {

                val session = sessionRepository.getSessionById(sessionId)
                    ?: throw IllegalStateException("Sessão não encontrada.")

                val hallSeats = seatRepository.getSeatsByCinemaHall(session.cinemaHallId)
                    ?: throw IllegalStateException("Erro ao carregar os lugares da sala.")

                val availableSeats = seatRepository.getAvailableSeats(sessionId)

                val seatsUi = buildSeatUiModelsBackendPrice(
                    hallSeats = hallSeats,
                    availableSeats = availableSeats,
                    sessionId = sessionId
                )

                uiState = uiState.copy(
                    isLoading = false,
                    session = session,
                    seats = seatsUi,
                    selectedSeatIds = emptySet(),
                    lastClickedSeatId = null,
                    maxSelectedMessage = null,
                    error = null
                )

            } catch (e: HttpException) {

                uiState = uiState.copy(
                    isLoading = false,
                    error = parseBackendError(e)
                )

            } catch (e: Exception) {

                uiState = uiState.copy(
                    isLoading = false,
                    error = e.message ?: "Erro inesperado ao carregar os lugares."
                )
            }
        }
    }

    private suspend fun buildSeatUiModelsBackendPrice(
        hallSeats: List<SeatResponseDto>,
        availableSeats: List<AvailableSeatDto>,
        sessionId: Int
    ): List<SeatUiModel> {

        val availableIds = availableSeats.mapTo(mutableSetOf()) { it.id }

        val seats = coroutineScope {
            hallSeats.map { seat ->
                async {
                    val price = seatRepository
                        .getSeatPrice(seat.id, sessionId)
                        .price

                    SeatUiModel(
                        id = seat.id,
                        seatNumber = seat.seatNumber,
                        row = seat.seatNumber.firstOrNull() ?: ' ',
                        column = seat.seatNumber.drop(1).toIntOrNull() ?: 0,
                        type = seat.seatType,
                        isAvailable = availableIds.contains(seat.id),
                        isReserved = !availableIds.contains(seat.id),
                        price = price
                    )
                }
            }.awaitAll()
        }

        return seats.sortedWith(compareBy<SeatUiModel> { it.row }.thenBy { it.column })
    }

    fun onSeatClicked(seatId: Int) {

        val seat = uiState.seats.firstOrNull { it.id == seatId } ?: return

        if (!seat.isAvailable || seat.isReserved) return

        val selected = uiState.selectedSeatIds
        val newSet: Set<Int>
        val msg: String?

        when {
            seatId in selected -> {
                newSet = selected - seatId
                msg = null
            }

            selected.size >= 5 -> {
                newSet = selected
                msg = "Só podes selecionar até 5 lugares."
            }

            else -> {
                newSet = selected + seatId
                msg = null
            }
        }

        uiState = uiState.copy(
            selectedSeatIds = newSet,
            lastClickedSeatId = seatId,
            maxSelectedMessage = msg
        )
    }

    private fun parseBackendError(e: HttpException): String {

        return when (e.code()) {

            400 -> "Pedido inválido."
            401 -> "Sessão expirada."
            403 -> "Sem permissões."
            404 -> "Sessão não encontrada ou sem lugares disponíveis."
            500 -> "Erro interno do servidor."

            else -> "Erro ao carregar lugares (${e.code()})."
        }
    }
}
