package com.example.moviespot.data.dto

import kotlinx.serialization.Serializable

@Serializable
data class GenreResponseDto(
    val id: Int,
    val name: String,
)