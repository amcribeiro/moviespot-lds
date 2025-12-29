package com.example.moviespot.presentation.screens.session

import android.os.Build
import androidx.annotation.RequiresApi
import androidx.compose.runtime.*
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.example.moviespot.data.dto.CinemaHallReadDto
import com.example.moviespot.data.dto.CinemaResponseDto
import com.example.moviespot.data.dto.SessionResponseDto
import com.example.moviespot.data.repository.CinemaHallRepository
import com.example.moviespot.data.repository.CinemaRepository
import com.example.moviespot.data.repository.SessionRepository
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.launch
import java.text.SimpleDateFormat
import java.util.Locale
import kotlin.math.*

data class AccordionCinemaData(
    val cinema: CinemaResponseDto,
    val halls: List<CinemaHallReadDto>,
    val sessionsByHall: Map<Int, List<SessionResponseDto>>
)

class SessionViewModel(
    private val cinemaRepo: CinemaRepository,
    private val hallRepo: CinemaHallRepository,
    private val sessionRepo: SessionRepository
) : ViewModel() {

    private val _accordions = MutableStateFlow<List<AccordionCinemaData>>(emptyList())
    val accordions: StateFlow<List<AccordionCinemaData>> = _accordions

    private var lastSortedList: List<AccordionCinemaData> = emptyList()

    val loading = MutableStateFlow(false)
    val error = MutableStateFlow<String?>(null)

    var selectedDate: String? by mutableStateOf(null)
    var showDatePicker by mutableStateOf(false)
    var isFiltered by mutableStateOf(false)

    @RequiresApi(Build.VERSION_CODES.O)
    fun load(movieId: Int) {
        viewModelScope.launch {
            try {
                loading.value = true
                error.value = null

                val cinemas = cinemaRepo.getAllCinemas()
                    ?: throw IllegalStateException("Erro ao carregar cinemas.")

                val halls = hallRepo.getAllHalls()
                    ?: throw IllegalStateException("Erro ao carregar salas.")

                val sessions =
                    sessionRepo.getAllSessions()
                        ?: throw IllegalStateException("Erro ao carregar sessões.")

                val validSessions =
                    sessions.filter { isSessionAtLeastOneHourAway(it.startDate) }

                val grouped = cinemas.mapNotNull { cinema ->

                    val hs = halls.filter { it.cinemaId == cinema.id }
                    if (hs.isEmpty()) return@mapNotNull null

                    val map = hs.associate { hall ->
                        hall.id to validSessions.filter {
                            it.movieId == movieId &&
                                    it.cinemaHallId == hall.id
                        }
                    }.filterValues { it.isNotEmpty() }

                    if (map.isEmpty()) return@mapNotNull null

                    AccordionCinemaData(cinema, hs, map)
                }

                if (grouped.isEmpty()) {
                    error.value = "Não existem sessões disponíveis para este filme."
                    _accordions.value = emptyList()
                    return@launch
                }

                _accordions.value = grouped
                lastSortedList = grouped

            } catch (e: Throwable) {
                error.value = parseBackendError(e)
                _accordions.value = emptyList()
            } finally {
                loading.value = false
            }
        }
    }

    fun filterByDay(date: String) {
        selectedDate = date
        isFiltered = true
        _accordions.value = applyFilterTo(lastSortedList)
    }

    private fun applyFilterTo(list: List<AccordionCinemaData>): List<AccordionCinemaData> {
        if (selectedDate == null) return list

        return list.map { c ->
            val map = c.sessionsByHall.mapValues { (_, sessions) ->
                sessions.filter { it.startDate.startsWith(selectedDate!!) }
            }.filterValues { it.isNotEmpty() }

            c.copy(sessionsByHall = map)

        }.filter { it.sessionsByHall.isNotEmpty() }
    }

    fun clearFilter() {
        selectedDate = null
        isFiltered = false
        _accordions.value = lastSortedList
    }

    fun sortByDistance(lat: Double, lon: Double) {
        lastSortedList =
            lastSortedList.sortedBy {
                haversine(lat, lon, it.cinema.latitude, it.cinema.longitude)
            }

        _accordions.value =
            if (isFiltered) applyFilterTo(lastSortedList)
            else lastSortedList
    }
    @RequiresApi(Build.VERSION_CODES.O)
    private fun isSessionAtLeastOneHourAway(startDate: String): Boolean {
        return try {

            val sessionTimeMillis = try {
                java.time.Instant.parse(startDate).toEpochMilli()
            } catch (_: Exception) {
                val fallback = startDate.substring(0, 19)
                val sdf = SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss", Locale.US)
                sdf.parse(fallback)?.time ?: return false
            }

            val cutoff = System.currentTimeMillis() + 60 * 60 * 1000
            sessionTimeMillis >= cutoff

        } catch (_: Exception) {
            false
        }
    }

    private fun haversine(lat1: Double, lon1: Double, lat2: Double, lon2: Double): Double {
        val R = 6371.0
        val dLat = Math.toRadians(lat2 - lat1)
        val dLon = Math.toRadians(lon2 - lon1)

        val a = sin(dLat / 2).pow(2) +
                cos(Math.toRadians(lat1)) *
                cos(Math.toRadians(lat2)) *
                sin(dLon / 2).pow(2)

        val c = 2 * atan2(sqrt(a), sqrt(1 - a))
        return R * c
    }

    private fun parseBackendError(e: Throwable): String {

        val msg = e.message ?: ""

        return when {

            msg.contains("Unable to resolve host", true) ->
                "Sem ligação ao servidor."

            msg.contains("timeout", true) ->
                "O servidor demorou demasiado a responder."

            msg.contains("No sessions", true) ->
                "Não existem sessões registadas."

            msg.contains("Session with ID", true) ->
                "Sessão não encontrada."

            msg.contains("time range", true) ->
                "Já existe uma sessão nesse horário."

            msg.contains("no available seats", true) ->
                "Não existem lugares disponíveis."

            msg.contains("There are no cinemas", true) ->
                "Não existem cinemas registados."

            msg.contains("Cinema with ID", true) ->
                "Cinema não encontrado."

            msg.contains("cinema halls", true) ->
                "Não existem salas neste cinema."

            msg.contains("Cinema hall with ID", true) ->
                "Sala não encontrada."

            msg.contains("date", true) ->
                "Data inválida."

            msg.isNotBlank() ->
                msg

            else ->
                "Erro inesperado ao carregar sessões."
        }
    }
}
