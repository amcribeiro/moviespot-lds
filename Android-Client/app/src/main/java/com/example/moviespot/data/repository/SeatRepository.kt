package com.example.moviespot.data.repository

import com.example.moviespot.data.dto.AvailableSeatDto
import com.example.moviespot.data.dto.SeatResponseDto
import com.example.moviespot.data.dto.SeatResponsePriceDto
import com.example.moviespot.data.remote.api.SeatApiService

class SeatRepository(
    private val api: SeatApiService
) {

    suspend fun getSeatsByCinemaHall(hallId: Int): List<SeatResponseDto>? {
        val response = api.getSeatsByCinemaHall(hallId)
        return if (response.isSuccessful) response.body() else null
    }

    suspend fun getAvailableSeats(sessionId: Int): List<AvailableSeatDto> {
        return api.getAvailableSeats(sessionId)
    }

    suspend fun getSeatById(id: Int): SeatResponseDto {
        return api.getSeatById(id)
    }

    suspend fun getSeatPrice(seatId: Int, sessionId: Int): SeatResponsePriceDto {
        return api.getSeatPrice(seatId, sessionId)
    }
}
