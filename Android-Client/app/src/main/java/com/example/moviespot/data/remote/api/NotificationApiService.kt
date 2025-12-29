package com.example.moviespot.data.remote.api

import com.example.moviespot.data.dto.NotificationRequest
import com.example.moviespot.data.dto.TopicNotificationRequest
import retrofit2.http.Body
import retrofit2.http.POST

interface NotificationApi {

    @POST("notification/token")
    suspend fun sendToToken(@Body request: NotificationRequest)

    @POST("notification/topic")
    suspend fun sendToTopic(@Body request: TopicNotificationRequest)
}