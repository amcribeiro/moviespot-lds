package com.example.moviespot.data.remote.api

import com.example.moviespot.data.dto.*
import retrofit2.http.Body
import retrofit2.http.POST

interface AuthApiService {
    @POST("User/login")
    suspend fun login(@Body request: LoginRequestDto): LoginResponseDto

    @POST("User/refresh")
    suspend fun refresh(@Body request: RefreshRequestDto): RefreshResponseDto

    @POST("User/register")
    suspend fun register(@Body request: UserCreateDto): LoginResponseDto

}
