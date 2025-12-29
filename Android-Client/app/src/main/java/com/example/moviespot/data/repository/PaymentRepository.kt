package com.example.moviespot.data.repository

import com.example.moviespot.data.dto.CreatePaymentRequestDto
import com.example.moviespot.data.remote.api.PaymentApiService

class PaymentRepository(private val api: PaymentApiService) {

    suspend fun createPaymentIntent(bookingId: Int, voucherId: Int?): String {
        val response = api.checkout(
            CreatePaymentRequestDto(
                bookingId = bookingId,
                voucherId = voucherId
            )
        )
        return response.clientSecret
    }
    suspend fun confirmPayment(paymentIntentId: String): String {
        val response = api.checkPaymentStatus(paymentIntentId)
        return response.status
    }
}
