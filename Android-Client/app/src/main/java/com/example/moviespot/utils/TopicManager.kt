package com.example.moviespot.utils

import android.util.Log
import com.google.firebase.messaging.FirebaseMessaging

object TopicManager {

    fun subscribeToPromotions() {
        FirebaseMessaging.getInstance().subscribeToTopic("promotions")
            .addOnCompleteListener {
                Log.d("FCM", "Subscribed to promotions")
            }
    }

    fun subscribeToSession(sessionId: Int) {
        val topic = "session_$sessionId"

        FirebaseMessaging.getInstance().subscribeToTopic(topic)
            .addOnCompleteListener {
                Log.d("FCM", "Subscribed to $topic")
            }
    }

    fun subscribeToSessions(sessionIds: List<Int>) {
        sessionIds.forEach { subscribeToSession(it) }
    }
}
