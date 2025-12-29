package com.example.moviespot.data.dto

import kotlinx.serialization.Serializable

@Serializable
data class CinemaResponseDto(
    val id: Int,
    val name: String,
    val street: String,
    val city: String,
    val state: String?,
    val zipCode: String?,
    val country: String,
    val latitude: Double,
    val longitude: Double,
    val createdAt: String,
    val updatedAt: String,
    val totalCinemaHalls: Int?
)