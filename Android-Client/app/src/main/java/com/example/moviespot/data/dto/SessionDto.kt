package com.example.moviespot.data.dto

import kotlinx.serialization.Serializable

@Serializable
data class SessionResponseDto(
    val id: Int,
    val movieId: Int,
    val movieTitle: String?,
    val cinemaHallId: Int,
    val cinemaHallName: String?,
    val createdBy: Int,
    val createdByName: String?,
    val startDate: String,
    val endDate: String,
    val price: Double,
    val createdAt: String,
    val updatedAt: String
)


