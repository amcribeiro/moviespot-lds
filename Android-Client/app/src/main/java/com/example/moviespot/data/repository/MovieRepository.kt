package com.example.moviespot.data.repository

import com.example.moviespot.data.dto.MovieDto
import com.example.moviespot.data.remote.api.MovieApiService
import retrofit2.HttpException

class MovieRepository(
    private val api: MovieApiService
) {

    suspend fun getAllMovies(): List<MovieDto>? {
        val response = api.getAllMovies()

        // CORREÇÃO: Lançar exceção se não for sucesso (ex: 500)
        if (!response.isSuccessful) {
            throw HttpException(response)
        }

        return response.body()
    }

    suspend fun getMovieById(id: Int): MovieDto? {
        val response = api.getMovieById(id)

        // CORREÇÃO: Lançar erro se não for sucesso (ex: 404, 500)
        if (!response.isSuccessful) {
            throw HttpException(response)
        }

        return response.body()
    }
}
