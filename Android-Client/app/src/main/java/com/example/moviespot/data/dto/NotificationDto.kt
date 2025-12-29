package com.example.moviespot.data.dto

import kotlinx.serialization.Serializable

@Serializable
data class NotificationRequest(
    val token: String,
    val title: String,
    val body: String
)

@Serializable
data class TopicNotificationRequest(
    val topic: String,
    val title: String,
    val body: String
)