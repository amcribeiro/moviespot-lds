package com.example.moviespot.data.remote.api

import com.example.moviespot.data.dto.CinemaHallReadDto
import com.example.moviespot.data.dto.CinemaHallDetailsDto
import retrofit2.Response
import retrofit2.http.GET
import retrofit2.http.Path

interface CinemaHallApiService {

    @GET("CinemaHall")
    suspend fun getAllCinemaHalls(): Response<List<CinemaHallReadDto>>

    @GET("CinemaHall/{id}")
    suspend fun getCinemaHallById(@Path("id") id: Int): Response<CinemaHallDetailsDto>

    @GET("CinemaHall/cinema/{cinemaId}")
    suspend fun getCinemaHallsByCinema(@Path("cinemaId") cinemaId: Int): Response<List<CinemaHallReadDto>>
}
