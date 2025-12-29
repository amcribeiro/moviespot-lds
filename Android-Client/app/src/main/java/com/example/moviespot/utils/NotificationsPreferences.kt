package com.example.moviespot.utils

import android.content.Context
import androidx.datastore.preferences.core.booleanPreferencesKey
import androidx.datastore.preferences.core.edit
import androidx.datastore.preferences.preferencesDataStore
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.map

class NotificationPreferences(private val context: Context) {

    private val Context.dataStore by preferencesDataStore("notification_settings")

    private val sessionsKey = booleanPreferencesKey("sessions_enabled")
    private val promotionsKey = booleanPreferencesKey("promotions_enabled")

    val sessionsEnabled: Flow<Boolean> =
        context.dataStore.data.map { it[sessionsKey] ?: false }

    val promotionsEnabled: Flow<Boolean> =
        context.dataStore.data.map { it[promotionsKey] ?: false }

    suspend fun setSessions(enabled: Boolean) {
        context.dataStore.edit { it[sessionsKey] = enabled }
    }

    suspend fun setPromotions(enabled: Boolean) {
        context.dataStore.edit { it[promotionsKey] = enabled }
    }
}
