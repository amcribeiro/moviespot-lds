package com.example.moviespot.data.remote.auth

import android.content.Context
import androidx.core.content.edit

class AuthManager(context: Context) : TokenProvider {

    private val prefs = context.getSharedPreferences("auth_prefs", Context.MODE_PRIVATE)

    override fun getAccessToken(): String? =
        prefs.getString("access_token", null)

    override fun getRefreshToken(): String? =
        prefs.getString("refresh_token", null)

    override fun saveAccessToken(token: String) {
        prefs.edit { putString("access_token", token) }
    }

    override fun saveRefreshToken(token: String) {
        prefs.edit { putString("refresh_token", token) }
    }

    override fun saveTokens(access: String, refresh: String) {
        prefs.edit {
            putString("access_token", access)
            putString("refresh_token", refresh)
        }
    }

    override fun clear() {
        prefs.edit {
            remove("access_token")
            remove("refresh_token")
        }
    }

    override fun saveRememberedPassword(password: String) {
        prefs.edit { putString("remember_password", password) }
    }

    override fun getRememberedPassword(): String? =
        prefs.getString("remember_password", null)

    override fun clearRememberedPassword() {
        prefs.edit { remove("remember_password") }
    }

    override fun saveRememberMe(enabled: Boolean) {
        prefs.edit { putBoolean("remember_me", enabled) }
    }

    override fun getRememberMe(): Boolean =
        prefs.getBoolean("remember_me", false)

    override fun clearRememberMe() {
        prefs.edit { remove("remember_me") }
    }
    override fun getUserId(): Int? {
        val token = getAccessToken() ?: return null

        return try {
            val parts = token.split(".")
            if (parts.size != 3) return null

            val payloadJson = String(
                android.util.Base64.decode(
                    parts[1],
                    android.util.Base64.URL_SAFE or android.util.Base64.NO_PADDING or android.util.Base64.NO_WRAP
                )
            )

            val payload = org.json.JSONObject(payloadJson)
            payload.optString("id").toIntOrNull()

        } catch (e: Exception) {
            null
        }
    }

}
