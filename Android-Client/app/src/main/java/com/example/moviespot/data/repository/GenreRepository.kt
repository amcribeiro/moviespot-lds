package com.example.moviespot.data.repository

import android.util.Log
import com.example.moviespot.data.dto.GenreResponseDto
import com.example.moviespot.data.remote.api.GenreApiService

class GenreRepository(
    private val api: GenreApiService
) {

    suspend fun getGenres(): List<GenreResponseDto>? {
        return try {
            val result = api.getGenres()

            Log.d("Genres", "Received ${result.size} genres")
            result
        } catch (e: Exception) {
            Log.e("Genres", "Error fetching genres: ${e.message}")
            null
        }
    }
}