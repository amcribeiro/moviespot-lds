package com.example.moviespot.presentation.screens.booking

import android.os.Build
import android.util.Log
import androidx.annotation.RequiresApi
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.setValue
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.example.moviespot.data.dto.BookingCreateDto
import com.example.moviespot.data.dto.BookingResponseDto
import com.example.moviespot.data.remote.auth.TokenProvider
import com.example.moviespot.data.repository.BookingRepository
import com.example.moviespot.data.repository.ReviewRepository
import com.example.moviespot.data.repository.SeatRepository
import com.example.moviespot.data.repository.SessionRepository
import com.example.moviespot.utils.TopicManager
import kotlinx.coroutines.async
import kotlinx.coroutines.awaitAll
import kotlinx.coroutines.launch
import retrofit2.HttpException
import java.io.IOException
import java.time.OffsetDateTime

class BookingViewModel(
    private val bookingRepo: BookingRepository,
    private val sessionRepo: SessionRepository,
    private val seatRepo: SeatRepository,
    private val reviewRepo: ReviewRepository,
    // NOVO: Injetar TokenProvider aqui
    private val tokenProvider: TokenProvider
) : ViewModel() {

    data class BookingSummaryState(
        val movieTitle: String = "",
        val cinemaName: String = "",
        val sessionDate: String = "",
        val sessionTime: String = "",
        val selectedSeats: List<String> = emptyList(),
        val seatsWithPrice: List<Pair<String, Double>> = emptyList(),
        val totalPrice: Double = 0.0,
        val error: String? = null,
        val isLoading: Boolean = false
    )

    var summaryState by mutableStateOf(BookingSummaryState())
        private set

    fun loadSessionForSummary(sessionId: Int) {
        viewModelScope.launch {
            try {
                summaryState = summaryState.copy(isLoading = true, error = null)

                val session = sessionRepo.getSessionById(sessionId)
                    ?: throw IllegalStateException("Sessão não encontrada.")

                summaryState = summaryState.copy(
                    isLoading = false,
                    movieTitle = session.movieTitle ?: "",
                    cinemaName = session.cinemaHallName ?: "",
                    sessionDate = session.startDate.substringBefore("T"),
                    sessionTime = session.startDate.substringAfter("T").substring(0, 5)
                )

            } catch (e: HttpException) {
                summaryState = summaryState.copy(
                    isLoading = false,
                    error = "Erro a carregar sessão (${e.code()})."
                )
            } catch (e: Exception) {
                summaryState = summaryState.copy(
                    isLoading = false,
                    error = e.message ?: "Erro inesperado ao carregar sessão."
                )
            }
        }
    }

    fun setSeatsForSummary(seatIds: List<Int>, sessionId: Int) {
        viewModelScope.launch {
            try {
                summaryState = summaryState.copy(isLoading = true, error = null)

                val seatPrices = seatIds.map { seatId ->
                    async {
                        val dto = seatRepo.getSeatPrice(seatId, sessionId)
                        dto.seatNumber to dto.price
                    }
                }.awaitAll()

                summaryState = summaryState.copy(
                    isLoading = false,
                    selectedSeats = seatPrices.map { it.first },
                    seatsWithPrice = seatPrices,
                    totalPrice = seatPrices.sumOf { it.second }
                )

            } catch (e: HttpException) {
                summaryState = summaryState.copy(
                    isLoading = false,
                    error = "Erro ao obter preços (${e.code()})."
                )
            } catch (e: Exception) {
                summaryState = summaryState.copy(
                    isLoading = false,
                    error = e.message ?: "Erro inesperado ao carregar preços."
                )
            }
        }
    }

    var selectedBooking by mutableStateOf<BookingResponseDto?>(null)
        private set

    var createSuccess by mutableStateOf(false)
        private set

    var isCreating by mutableStateOf(false)
    var createError by mutableStateOf<String?>(null)

    // ALTERADO: Removemos o parâmetro tokenProvider daqui
    fun createBooking(
        sessionId: Int,
        seatIds: List<Int>,
        voucherId: Int?,
        onProceedPayment: (bookingId: Int, voucherId: Int?, userId: Int) -> Unit
    ) {
        // Usamos o tokenProvider injetado no construtor
        val userId = tokenProvider.getUserId() ?: run {
            createError = "Sessão expirada. Faz login novamente."
            return
        }

        viewModelScope.launch {
            isCreating = true
            createError = null
            createSuccess = false

            try {
                val booking = bookingRepo.createBooking(
                    BookingCreateDto(
                        userId = userId,
                        sessionId = sessionId,
                        seatIds = seatIds
                    )
                ) ?: throw IllegalStateException("Erro ao criar reserva.")

                selectedBooking = booking
                createSuccess = true

                TopicManager.subscribeToSession(sessionId)

                onProceedPayment(
                    booking.id,
                    voucherId,
                    userId
                )

            } catch (e: HttpException) {
                createError = when (e.code()) {
                    400 -> "Lugares inválidos ou já ocupados."
                    404 -> "Sessão não encontrada."
                    401 -> "Sessão expirada. Faz login novamente."
                    else -> "Erro no servidor (${e.code()})."
                }
            } catch (_: IOException) {
                createError = "Sem ligação à internet."
            } catch (e: Exception) {
                createError = e.message ?: "Erro inesperado ao criar reserva."
            } finally {
                isCreating = false
            }
        }
    }

    /* ============================= FCM ============================= */

    // ALTERADO: Removemos o parâmetro tokenProvider daqui também
    fun subscribeToUserSessionTopics() {
        viewModelScope.launch {
            try {
                val userId = tokenProvider.getUserId() ?: return@launch

                val userBookings = bookingRepo.getBookingsByUser(userId) ?: emptyList()

                userBookings
                    .map { it.sessionId }
                    .distinct()
                    .forEach {
                        TopicManager.subscribeToSession(it)
                    }

            } catch (e: Exception) {
                e.printStackTrace()
            }
        }
    }

    /* ============================================================
                          AUTO REVIEW CHECK
   ============================================================ */

    val pendingReviewBookingId = mutableStateOf<Int?>(null)

    @RequiresApi(Build.VERSION_CODES.O)
    // ALTERADO: Removemos o parâmetro tokenProvider daqui também
    fun loadBookingsForReviewCheck() {

        val userId = tokenProvider.getUserId()

        Log.d("REVIEW_CHECK", "➡ UserId = $userId")

        if (userId == null) return

        viewModelScope.launch {
            try {
                val bookings = bookingRepo.getBookingsByUser(userId) ?: emptyList()
                Log.d("REVIEW_CHECK", "➡ Bookings encontradas = ${bookings.size}")

                if (bookings.isEmpty()) return@launch

                val userReviews = try {
                    reviewRepo.getReviewsByUser(userId)
                } catch (e: Exception) {
                    Log.d("REVIEW_CHECK", "⚠ Erro ao obter reviews: ${e.message}")
                    emptyList()
                }

                Log.d("REVIEW_CHECK", "➡ Reviews do utilizador = ${userReviews.size}")

                bookings.forEach { booking ->
                    val session = sessionRepo.getSessionById(booking.sessionId)
                    if (session == null) return@forEach

                    val sessionEnd = OffsetDateTime.parse(session.endDate)
                    val now = OffsetDateTime.now()

                    if (!sessionEnd.isBefore(now)) return@forEach

                    val alreadyReviewed = userReviews.any { review ->
                        review.bookingId == booking.id
                    }

                    if (!alreadyReviewed) {
                        pendingReviewBookingId.value = booking.id
                        return@launch
                    }
                }
            } catch (e: Exception) {
                Log.e("REVIEW_CHECK", "❌ Erro geral:", e)
            }
        }
    }

    fun clearPendingReview() {
        pendingReviewBookingId.value = null
    }
}