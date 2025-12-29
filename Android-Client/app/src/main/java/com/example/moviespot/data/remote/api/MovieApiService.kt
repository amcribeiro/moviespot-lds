package com.example.moviespot.data.remote.api

import com.example.moviespot.data.dto.MovieDto
import retrofit2.Response
import retrofit2.http.GET
import retrofit2.http.Path

interface MovieApiService {

    @GET("Movie")
    suspend fun getAllMovies(): Response<List<MovieDto>>

    @GET("Movie/{id}")
    suspend fun getMovieById(@Path("id") id: Int): Response<MovieDto>
}
