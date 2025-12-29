package com.example.moviespot.data.remote.api

import com.example.moviespot.data.dto.BookingCreateDto
import com.example.moviespot.data.dto.BookingResponseDto
import retrofit2.Response
import retrofit2.http.*

interface BookingApiService {

    @GET("Booking/user/{userId}")
    suspend fun getBookingsByUser(@Path("userId") userId: Int): Response<List<BookingResponseDto>>

    @GET("Booking/{id}")
    suspend fun getBookingById(@Path("id") bookingId: Int): Response<BookingResponseDto>

    @POST("Booking")
    suspend fun createBooking(@Body booking: BookingCreateDto): Response<BookingResponseDto>
}
