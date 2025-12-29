package com.example.moviespot.data.remote.network

import com.example.moviespot.data.dto.RefreshRequestDto
import com.example.moviespot.data.remote.api.AuthApiService
import com.example.moviespot.data.remote.auth.TokenProvider
import com.jakewharton.retrofit2.converter.kotlinx.serialization.asConverterFactory
import kotlinx.coroutines.runBlocking
import kotlinx.serialization.ExperimentalSerializationApi
import kotlinx.serialization.json.Json
import okhttp3.MediaType.Companion.toMediaType
import okhttp3.OkHttpClient
import okhttp3.logging.HttpLoggingInterceptor
import retrofit2.Retrofit

object AuthorizedRetrofit {

    lateinit var tokenProvider: TokenProvider

    private const val BASE_URL = "http://localhost:5000/"

    private val json = Json { ignoreUnknownKeys = true }

    fun initializeTokenProvider(provider: TokenProvider) {
        tokenProvider = provider
    }

    @OptIn(ExperimentalSerializationApi::class)
    private val okHttp by lazy {
        val logging = HttpLoggingInterceptor().apply {
            level = HttpLoggingInterceptor.Level.BODY
        }

        OkHttpClient.Builder()
            .addInterceptor(logging)
            .addInterceptor(
                AuthInterceptor(
                    tokenProvider,
                    onRefreshToken = { refreshToken ->
                        runBlocking {
                            val api = AuthRetrofit.create(AuthApiService::class.java)
                            val resp = api.refresh(RefreshRequestDto(refreshToken))

                            tokenProvider.saveAccessToken(resp.accessToken)
                            tokenProvider.saveRefreshToken(resp.refreshToken)

                            resp.accessToken
                        }
                    }
                )
            )
            .build()
    }

    @OptIn(ExperimentalSerializationApi::class)
    val retrofit: Retrofit by lazy {
        Retrofit.Builder()
            .baseUrl(BASE_URL)
            .client(okHttp)
            .addConverterFactory(json.asConverterFactory("application/json".toMediaType()))
            .build()
    }

    fun <T> create(service: Class<T>): T = retrofit.create(service)
}