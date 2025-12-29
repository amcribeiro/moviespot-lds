package com.example.moviespot.data.repository

import android.util.Log
import com.example.moviespot.data.dto.SessionResponseDto
import com.example.moviespot.data.remote.api.SessionApiService

class SessionRepository(
    private val api: SessionApiService
) {

    suspend fun getAllSessions(): List<SessionResponseDto>? {
        val response = api.getAllSessions()

        Log.d("Sessions", "Status: ${response.code()}")
        Log.d("Sessions", "Successful: ${response.isSuccessful}")
        Log.d("Sessions", "Body: ${response.body()}")

        return if (response.isSuccessful) response.body() else null
    }

    suspend fun getSessionById(id: Int): SessionResponseDto? {
        val response = api.getSessionById(id)

        Log.d("SessionDetail", "Status: ${response.code()}")
        Log.d("SessionDetail", "Successful: ${response.isSuccessful}")
        Log.d("SessionDetail", "Body: ${response.body()}")

        return if (response.isSuccessful) response.body() else null
    }
}
