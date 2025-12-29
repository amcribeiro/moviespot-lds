package com.example.moviespot.data.repository

import android.util.Log
import com.example.moviespot.data.dto.BookingCreateDto
import com.example.moviespot.data.dto.BookingResponseDto
import com.example.moviespot.data.remote.api.BookingApiService

class BookingRepository(
    private val api: BookingApiService
) {

    suspend fun getBookingsByUser(userId: Int): List<BookingResponseDto>? {
        val response = api.getBookingsByUser(userId)

        Log.d("Bookings", "➡ Status: ${response.code()}")
        Log.d("Bookings", "➡ Successful: ${response.isSuccessful}")
        Log.d("Bookings", "➡ Body: ${response.body()}")

        return if (response.isSuccessful) response.body() else null
    }

    suspend fun getBookingById(id: Int): BookingResponseDto? {
        val response = api.getBookingById(id)

        Log.d("BookingDetail", "➡ Status: ${response.code()}")
        Log.d("BookingDetail", "➡ Successful: ${response.isSuccessful}")
        Log.d("BookingDetail", "➡ Body: ${response.body()}")

        return if (response.isSuccessful) response.body() else null
    }

    suspend fun createBooking(dto: BookingCreateDto): BookingResponseDto {
        val response = api.createBooking(dto)

        if (!response.isSuccessful) {
            throw retrofit2.HttpException(response)
        }

        return response.body()!!
    }
}
