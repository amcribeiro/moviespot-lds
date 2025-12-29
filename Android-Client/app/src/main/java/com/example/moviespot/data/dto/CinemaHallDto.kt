package com.example.moviespot.data.dto

import kotlinx.serialization.Serializable

@Serializable
data class CinemaHallReadDto(
    val id: Int,
    val name: String,
    val cinemaId: Int
)

@Serializable
data class CinemaHallDetailsDto(
    val id: Int,
    val name: String,
    val cinemaId: Int,
    val cinemaName: String?,
    val createdAt: String,
    val updatedAt: String
)