package com.example.moviespot.data.repository

import com.example.moviespot.data.dto.ForgotPasswordRequestDto
import com.example.moviespot.data.dto.ResetPasswordRequestDto
import com.example.moviespot.data.remote.api.UserApiService
import retrofit2.HttpException

class UserRepository(
    private val api: UserApiService
) {

    suspend fun forgotPassword(email: String): Boolean {
        val request = ForgotPasswordRequestDto(email)
        val response = api.forgotPassword(request)

        // CORREÇÃO: Lançar exceção se a resposta não for sucesso (ex: 404)
        if (!response.isSuccessful) {
            throw HttpException(response)
        }

        return true
    }

    suspend fun resetPassword(token: String, newPassword: String): Boolean {
        val request = ResetPasswordRequestDto(token, newPassword)
        val response = api.resetPassword(request)

        // CORREÇÃO: Lançar exceção se a resposta não for sucesso
        if (!response.isSuccessful) {
            throw HttpException(response)
        }

        return true
    }
}
