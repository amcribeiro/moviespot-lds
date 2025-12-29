package com.example.moviespot.data.remote.api

import com.example.moviespot.data.dto.ForgotPasswordRequestDto
import com.example.moviespot.data.dto.ResetPasswordRequestDto
import retrofit2.Response
import retrofit2.http.Body
import retrofit2.http.POST

interface UserApiService {

    @POST("User/forgot-password")
    suspend fun forgotPassword(
        @Body request: ForgotPasswordRequestDto
    ): Response<String>

    @POST("User/reset-password")
    suspend fun resetPassword(
        @Body request: ResetPasswordRequestDto
    ): Response<String>
}
