package com.example.moviespot.data.repository

import android.util.Log
import com.example.moviespot.data.dto.CinemaResponseDto
import com.example.moviespot.data.remote.api.CinemaApiService

class CinemaRepository(
    private val api: CinemaApiService
) {

    suspend fun getAllCinemas(): List<CinemaResponseDto>? {
        return try {
            val response = api.getCinemas()
            if (response.isSuccessful) response.body() else null
        } catch (e: Exception) {
            Log.e("CinemaRepo", "Error fetching cinemas: ${e.message}")
            null
        }
    }

    suspend fun getCinemaById(id: Int): CinemaResponseDto? {
        return try {
            val response = api.getCinemaById(id)
            if (response.isSuccessful) response.body() else null
        } catch (e: Exception) {
            Log.e("CinemaRepo", "Error fetching cinema $id: ${e.message}")
            null
        }
    }
}