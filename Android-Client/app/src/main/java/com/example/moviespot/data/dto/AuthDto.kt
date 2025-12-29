package com.example.moviespot.data.dto

import kotlinx.serialization.Serializable

@Serializable
data class ForgotPasswordRequestDto(
    val email: String
)

@Serializable
data class LoginRequestDto(
    val email: String,
    val password: String
)

@Serializable
data class LoginResponseDto(
    val email: String,
    val accessToken: String,
    val expiresIn: Int,
    val refreshToken: String
)

@Serializable
data class RefreshRequestDto(val refreshToken: String)

@Serializable
data class RefreshResponseDto(
    val accessToken: String,
    val refreshToken: String
)

@Serializable
data class ResetPasswordRequestDto(
    val token: String,
    val newPassword: String
)

@Serializable
data class UserCreateDto(
    val name: String,
    val email: String,
    val password: String,
    val phone: String,
    val role: String = "User"
)