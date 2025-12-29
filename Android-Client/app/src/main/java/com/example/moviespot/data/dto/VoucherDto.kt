package com.example.moviespot.data.dto

import kotlinx.serialization.Serializable

@Serializable
data class VoucherResponseDto(
    val id: Int,
    val code: String,
    val value: Double,
    val validUntil: String,
    val maxUsages: Int,
    val usages: Int,
    val createdAt: String,
    val updatedAt: String
)

@Serializable
data class VoucherUpdateDto(
    val code: String,
    val value: Double,
    val validUntil: String,
    val maxUsages: Int,
    val usages: Int
)
