package com.example.moviespot.presentation.screens.cinemas

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.example.moviespot.data.dto.CinemaResponseDto
import com.example.moviespot.data.repository.CinemaRepository
import com.example.moviespot.presentation.utils.haversine
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.launch

class CinemaViewModel(
    private val cinemaRepo: CinemaRepository
) : ViewModel() {

    val cinemas = MutableStateFlow<List<CinemaResponseDto>>(emptyList())

    val loading = MutableStateFlow(false)
    val error = MutableStateFlow<String?>(null)

    fun load() {
        viewModelScope.launch {
            try {
                loading.value = true
                error.value = null

                val result =
                    cinemaRepo.getAllCinemas()
                        ?: throw IllegalStateException("Erro ao carregar cinemas.")

                cinemas.value = result

            } catch (e: Throwable) {
                error.value = parseBackendError(e)
                cinemas.value = emptyList()
            } finally {
                loading.value = false
            }
        }
    }

    fun sortByDistance(lat: Double, lon: Double) {
        cinemas.value = cinemas.value.sortedBy {
            haversine(lat, lon, it.latitude, it.longitude)
        }
    }

    private fun parseBackendError(e: Throwable): String {

        val msg = e.message ?: ""

        return when {

            msg.contains("Unable to resolve host", true) ->
                "Sem ligação ao servidor."

            msg.contains("timeout", true) ->
                "O servidor demorou demasiado a responder."

            msg.contains("There are no cinemas", true) ->
                "Não existem cinemas registados."

            msg.contains("Cinema with ID", true) ->
                "Cinema não encontrado."

            msg.contains("401", true) ||
                    msg.contains("unauthorized", true) ->
                "Sessão expirada. Faça login novamente."

            msg.contains("403", true) ||
                    msg.contains("forbidden", true) ->
                "Não tem permissões para aceder aos cinemas."

            msg.isNotBlank() ->
                msg

            else ->
                "Erro inesperado ao carregar cinemas."
        }
    }
}