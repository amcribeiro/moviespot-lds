package com.example.moviespot.data.remote.api

import com.example.moviespot.data.dto.CinemaResponseDto
import retrofit2.Response
import retrofit2.http.GET
import retrofit2.http.Path

interface CinemaApiService {

    @GET("Cinemas")
    suspend fun getCinemas(): Response<List<CinemaResponseDto>>

    @GET("Cinemas/{id}")
    suspend fun getCinemaById(@Path("id") id: Int): Response<CinemaResponseDto>
}
