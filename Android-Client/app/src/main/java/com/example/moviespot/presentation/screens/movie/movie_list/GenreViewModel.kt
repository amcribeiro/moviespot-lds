package com.example.moviespot.presentation.screens.movie.movie_list

import androidx.compose.runtime.*
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.example.moviespot.data.dto.GenreResponseDto
import com.example.moviespot.data.repository.GenreRepository
import kotlinx.coroutines.launch

class GenreViewModel(
    private val repo: GenreRepository
) : ViewModel() {

    var genres by mutableStateOf<List<GenreResponseDto>?>(null)
    var isLoading by mutableStateOf(false)
    var error by mutableStateOf<String?>(null)

    fun loadGenres() {
        viewModelScope.launch {
            isLoading = true
            error = null

            val result = repo.getGenres()
            if (result != null) genres = result
            else error = "Erro ao carregar géneros"

            isLoading = false
        }
    }
}
