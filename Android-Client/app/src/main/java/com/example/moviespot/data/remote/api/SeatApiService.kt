package com.example.moviespot.data.remote.api
import com.example.moviespot.data.dto.AvailableSeatDto
import com.example.moviespot.data.dto.SeatResponseDto
import com.example.moviespot.data.dto.SeatResponsePriceDto
import retrofit2.Response
import retrofit2.http.GET
import retrofit2.http.Path

interface SeatApiService {
    @GET("seat/hall/{cinemaHallId}")
    suspend fun getSeatsByCinemaHall(@Path("cinemaHallId") hallId: Int): Response<List<SeatResponseDto>>

    @GET("seat/{id}")
    suspend fun getSeatById(@Path("id") id: Int): SeatResponseDto

    @GET("session/{sessionId}/available-seats")
    suspend fun getAvailableSeats(@Path("sessionId") sessionId: Int): List<AvailableSeatDto>

    @GET("seat/{seatId}/price/{sessionId}")
    suspend fun getSeatPrice(@Path("seatId") seatId: Int, @Path("sessionId") sessionId: Int): SeatResponsePriceDto
}