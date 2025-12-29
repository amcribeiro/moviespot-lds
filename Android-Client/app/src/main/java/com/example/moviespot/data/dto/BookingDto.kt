package com.example.moviespot.data.dto

import kotlinx.serialization.Serializable

@Serializable
data class BookingCreateDto(
    val userId: Int,
    val sessionId: Int,
    val seatIds: List<Int>
)

@Serializable
data class BookingResponseDto(
    val id: Int,
    val userId: Int,
    val sessionId: Int,
    val bookingDate: String,
    val status: Boolean,
    val totalAmount: Double,
    val createdAt: String,
    val updatedAt: String
)

@Serializable
data class BookingUpdateDto(
    val id: Int,
    val userId: Int,
    val sessionId: Int,
    val status: Boolean,
    val totalAmount: Double
)
