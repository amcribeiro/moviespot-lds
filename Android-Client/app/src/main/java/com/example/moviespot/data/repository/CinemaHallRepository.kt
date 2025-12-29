package com.example.moviespot.data.repository

import android.util.Log
import com.example.moviespot.data.dto.CinemaHallDetailsDto
import com.example.moviespot.data.dto.CinemaHallReadDto
import com.example.moviespot.data.remote.api.CinemaHallApiService

class CinemaHallRepository(
    private val api: CinemaHallApiService
) {

    suspend fun getAllHalls(): List<CinemaHallReadDto>? {
        return try {
            val response = api.getAllCinemaHalls()
            if (response.isSuccessful) response.body() else null
        } catch (e: Exception) {
            Log.e("CinemaHallRepo", "Error fetching halls: ${e.message}")
            null
        }
    }

    suspend fun getHallById(id: Int): CinemaHallDetailsDto? {
        return try {
            val response = api.getCinemaHallById(id)
            if (response.isSuccessful) response.body() else null
        } catch (e: Exception) {
            Log.e("CinemaHallRepo", "Error fetching hall $id: ${e.message}")
            null
        }
    }

    suspend fun getHallsByCinema(cinemaId: Int): List<CinemaHallReadDto>? {
        return try {
            val response = api.getCinemaHallsByCinema(cinemaId)
            if (response.isSuccessful) response.body() else null
        } catch (e: Exception) {
            Log.e("CinemaHallRepo", "Error fetching halls for cinema $cinemaId: ${e.message}")
            null
        }
    }
}