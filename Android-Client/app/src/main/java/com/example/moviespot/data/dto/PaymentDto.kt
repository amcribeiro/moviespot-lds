package com.example.moviespot.data.dto

import kotlinx.serialization.Serializable

@Serializable
data class CreatePaymentRequestDto(
    val bookingId: Int,
    val voucherId: Int? = null
)

@Serializable
data class StripeIntentResponseDto(
    val clientSecret: String
)

@Serializable
data class CheckPaymentStatusResponseDto(
    val status: String
)


