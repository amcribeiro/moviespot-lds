package com.example.moviespot.data.remote.network

import com.example.moviespot.data.remote.auth.TokenProvider
import kotlinx.coroutines.runBlocking
import okhttp3.Interceptor
import okhttp3.Response

class AuthInterceptor(
    private val tokenProvider: TokenProvider,
    private val onRefreshToken: (String) -> String?
) : Interceptor {

    override fun intercept(chain: Interceptor.Chain): Response {
        val access = tokenProvider.getAccessToken()
        val refresh = tokenProvider.getRefreshToken()

        val requestBuilder = chain.request().newBuilder()

        if (!access.isNullOrBlank()) {
            requestBuilder.addHeader("Authorization", "Bearer $access")
        }

        val response = chain.proceed(requestBuilder.build())

        if (response.code == 401 && !refresh.isNullOrBlank()) {

            val newAccess = runBlocking { onRefreshToken(refresh) }

            if (!newAccess.isNullOrBlank()) {
                tokenProvider.saveTokens(newAccess, refresh)

                val newRequest = chain.request().newBuilder()
                    .addHeader("Authorization", "Bearer $newAccess")
                    .build()

                response.close()
                return chain.proceed(newRequest)
            } else {
                tokenProvider.clear()
            }
        }

        return response
    }
}
