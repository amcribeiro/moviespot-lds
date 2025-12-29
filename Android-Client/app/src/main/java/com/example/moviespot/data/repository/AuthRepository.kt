package com.example.moviespot.data.repository

import com.example.moviespot.data.dto.LoginRequestDto
import com.example.moviespot.data.dto.UserCreateDto
import com.example.moviespot.data.remote.api.AuthApiService
import com.example.moviespot.data.remote.auth.TokenProvider

class AuthRepository(
    private val api: AuthApiService,
    private val tokenProvider: TokenProvider
) {

    suspend fun login(email: String, password: String): Boolean {
        val result = api.login(LoginRequestDto(email, password))

        tokenProvider.saveTokens(
            access = result.accessToken,
            refresh = result.refreshToken
        )

        return true
    }
    suspend fun register(name: String, email: String, password: String, phone: String): Boolean {

        val result = api.register(
            UserCreateDto(
                name = name,
                email = email,
                password = password,
                phone = phone,
                role = "User"
            )
        )

        tokenProvider.saveTokens(
            access = result.accessToken,
            refresh = result.refreshToken
        )

        return true
    }
}
