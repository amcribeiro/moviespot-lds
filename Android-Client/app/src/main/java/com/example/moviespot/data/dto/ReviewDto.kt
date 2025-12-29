package com.example.moviespot.data.dto

import kotlinx.serialization.Serializable

@Serializable
data class ReviewCreateDto(
    val bookingId: Int,
    val rating: Int,
    val comment: String? = null,
    val reviewDate: String? = null // ISO-8601
)

@Serializable
data class ReviewUpdateDto(
    val id: Int,
    val rating: Int,
    val comment: String? = null,
    val reviewDate: String? = null
)

@Serializable
data class ReviewResponseDto(
    val id: Int,
    val bookingId: Int,
    val rating: Int,
    val comment: String? = null,
    val reviewDate: String,
    val createdAt: String,
    val updatedAt: String,
    val userId: Int? = null,
    val movieTitle: String? = null
)
