package com.example.moviespot.data.remote.api

import com.example.moviespot.data.dto.SessionResponseDto
import retrofit2.Response
import retrofit2.http.GET
import retrofit2.http.Path

interface SessionApiService {

    @GET("Session")
    suspend fun getAllSessions(): Response<List<SessionResponseDto>>

    @GET("Session/{id}")
    suspend fun getSessionById(@Path("id") id: Int): Response<SessionResponseDto>

}


