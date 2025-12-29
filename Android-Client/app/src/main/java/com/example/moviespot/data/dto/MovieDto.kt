package com.example.moviespot.data.dto

import kotlinx.serialization.Serializable

@Serializable
data class MovieDto(
    val id: Int,
    val title: String,
    val description: String,
    val language: String,
    val releaseDate: String? = null,
    val country: String,
    val posterPath: String,
    val genres: List<String>
)