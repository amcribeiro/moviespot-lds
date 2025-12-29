package com.example.moviespot.data.remote.api

import com.example.moviespot.data.dto.CheckPaymentStatusResponseDto
import com.example.moviespot.data.dto.CreatePaymentRequestDto
import com.example.moviespot.data.dto.StripeIntentResponseDto
import retrofit2.http.Body
import retrofit2.http.GET
import retrofit2.http.POST
import retrofit2.http.Query

interface PaymentApiService {
    @POST("Payment/checkout")
    suspend fun checkout(@Body request: CreatePaymentRequestDto): StripeIntentResponseDto

    @GET("Payment/check-payment-status")
    suspend fun checkPaymentStatus(@Query("paymentIntentId") paymentIntentId: String): CheckPaymentStatusResponseDto
}
