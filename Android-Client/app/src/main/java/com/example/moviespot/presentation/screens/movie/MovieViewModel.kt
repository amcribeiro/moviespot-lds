package com.example.moviespot.presentation.screens.movie

import androidx.compose.runtime.*
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.example.moviespot.data.dto.MovieDto
import com.example.moviespot.data.repository.MovieRepository
import kotlinx.coroutines.launch
import kotlinx.serialization.json.Json
import retrofit2.HttpException
import java.net.ConnectException

class MovieViewModel(
    private val repo: MovieRepository
) : ViewModel() {


    var movies by mutableStateOf<List<MovieDto>>(emptyList())
        private set

    private var allMovies: List<MovieDto> = emptyList()

    val originalMovies: List<MovieDto>
        get() = allMovies

    var isLoadingMovies by mutableStateOf(false)
    var moviesError by mutableStateOf<String?>(null)

    var selectedMovie by mutableStateOf<MovieDto?>(null)

    var isLoadingDetails by mutableStateOf(false)
    var detailsError by mutableStateOf<String?>(null)

    var selectedGenres = mutableStateListOf<String>()
    var selectedCountries = mutableStateListOf<String>()
    var selectedYears = mutableStateListOf<String>()

    fun loadMovies() {
        viewModelScope.launch {
            isLoadingMovies = true
            moviesError = null

            try {
                val result = repo.getAllMovies() ?: emptyList()

                allMovies = result
                applyFilters()

            } catch (e: HttpException) {
                moviesError = parseBackendError(e)
            } catch (e: ConnectException) {
                moviesError = "Sem ligação ao servidor."
            } catch (e: Exception) {
                moviesError = "Erro inesperado ao carregar filmes."
            }

            isLoadingMovies = false
        }
    }

    fun applyFilters() {
        var result = allMovies

        if (selectedGenres.isNotEmpty())
            result = result.filter { it.genres.any(selectedGenres::contains) }

        if (selectedCountries.isNotEmpty())
            result = result.filter { it.country in selectedCountries }

        if (selectedYears.isNotEmpty())
            result = result.filter { it.releaseDate?.take(4) == selectedYears.first() }

        movies = result
    }

    fun loadMovieDetails(id: Int) {
        viewModelScope.launch {
            isLoadingDetails = true
            detailsError = null
            selectedMovie = null

            try {
                selectedMovie = repo.getMovieById(id)

            } catch (e: HttpException) {
                detailsError = parseBackendError(e)

            } catch (e: ConnectException) {
                detailsError = "Sem ligação ao servidor."

            } catch (e: Exception) {
                detailsError = "Erro inesperado ao carregar detalhes do filme."
            }

            isLoadingDetails = false
        }
    }

    private fun parseBackendError(e: HttpException): String {
        return try {
            val body = e.response()?.errorBody()?.string()?.trim()

            if (body.isNullOrBlank()) {
                return when (e.code()) {
                    400 -> "Pedido inválido."
                    401 -> "Não autorizado."
                    403 -> "Sem permissões."
                    404 -> "Recurso não encontrado."
                    else -> "Erro interno do servidor."
                }
            }

            val clean = body
                .removePrefix("\"")
                .removeSuffix("\"")
                .removePrefix("Error: ")
                .trim()

            if (clean.startsWith("[") && clean.endsWith("]")) {
                val arr = Json.decodeFromString<List<String>>(clean)
                return arr.joinToString("\n")
            }

            return clean

        } catch (_: Exception) {
            "Erro ao interpretar resposta do servidor."
        }
    }
}
