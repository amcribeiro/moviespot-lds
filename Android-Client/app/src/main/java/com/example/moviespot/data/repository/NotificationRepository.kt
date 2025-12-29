package com.example.moviespot.data.repository

import com.example.moviespot.data.dto.NotificationRequest
import com.example.moviespot.data.dto.TopicNotificationRequest
import com.example.moviespot.data.remote.api.NotificationApi

class NotificationRepository(private val api: NotificationApi) {

    suspend fun sendToDevice(token: String, title: String, body: String) {
        api.sendToToken(NotificationRequest(token, title, body))
    }

    suspend fun sendToTopic(topic: String, title: String, body: String) {
        api.sendToTopic(TopicNotificationRequest(topic, title, body))
    }
}