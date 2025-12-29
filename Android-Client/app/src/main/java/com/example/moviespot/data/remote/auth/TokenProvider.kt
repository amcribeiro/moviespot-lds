package com.example.moviespot.data.remote.auth

interface TokenProvider {
    fun getAccessToken(): String?
    fun getRefreshToken(): String?
    fun saveAccessToken(token: String)
    fun saveRefreshToken(token: String)
    fun saveTokens(access: String, refresh: String)
    fun clear()
    fun saveRememberedPassword(password: String)
    fun getRememberedPassword(): String?
    fun clearRememberedPassword()
    fun saveRememberMe(enabled: Boolean)
    fun getRememberMe(): Boolean
    fun clearRememberMe()
    fun getUserId(): Int?
}

