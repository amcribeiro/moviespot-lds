package com.example.moviespot.data.dto

import kotlinx.serialization.Serializable

@Serializable
data class SeatResponseDto(
    val id: Int,
    val cinemaHallId: Int,
    val seatNumber: String,
    val seatType: String,
    val createdAt: String,
    val updatedAt: String
)

@Serializable
data class SeatResponsePriceDto(
    val id: Int,
    val cinemaHallId: Int,
    val seatNumber: String,
    val seatType: String,
    val price: Double,
    val createdAt: String? = null,
    val updatedAt: String? = null
)
@Serializable
data class AvailableSeatDto(
    val id: Int,
    val seatNumber: String,
    val seatType: String
)