package com.example.moviespot.data.remote.api

import com.example.moviespot.data.dto.GenreResponseDto
import retrofit2.http.GET

interface GenreApiService {
    @GET("Genre")
    suspend fun getGenres(): List<GenreResponseDto>
}
