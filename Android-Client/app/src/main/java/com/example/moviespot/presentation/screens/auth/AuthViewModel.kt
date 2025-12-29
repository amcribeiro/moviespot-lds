package com.example.moviespot.presentation.screens.auth

import androidx.compose.runtime.*
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.example.moviespot.data.remote.auth.TokenProvider
import com.example.moviespot.data.repository.AuthRepository
import com.example.moviespot.data.repository.UserRepository
import com.example.moviespot.presentation.screens.booking.BookingViewModel
import com.example.moviespot.utils.TopicManager
import kotlinx.coroutines.launch
import kotlinx.serialization.json.Json
import retrofit2.HttpException
import java.net.ConnectException

sealed class AuthScreenState {
    data object Idle : AuthScreenState()
    data object Loading : AuthScreenState()
    data object Success : AuthScreenState()
    data class Error(val message: String) : AuthScreenState()
}

class AuthViewModel(
    private val authRepo: AuthRepository,
    private val userRepo: UserRepository,
    private val tokenProvider: TokenProvider,
    private val bookingViewModel: BookingViewModel
) : ViewModel() {

    var name by mutableStateOf("")
    var email by mutableStateOf("")
    var password by mutableStateOf("")
    var phone by mutableStateOf("")
    var passwordVisible by mutableStateOf(false)

    var rememberMe by mutableStateOf(false)

    var resetToken by mutableStateOf("")

    var state: AuthScreenState by mutableStateOf(AuthScreenState.Idle)
        private set

    init {
        rememberMe = tokenProvider.getRememberMe()
        if (rememberMe) {
            password = tokenProvider.getRememberedPassword() ?: ""
        }
    }

    fun login() {
        if (email.isBlank() || password.isBlank()) {
            state = AuthScreenState.Error("Preenche todos os campos.")
            return
        }

        viewModelScope.launch {
            state = AuthScreenState.Loading

            try {
                val success = authRepo.login(email, password)

                if (success) {

                    if (rememberMe) {
                        tokenProvider.saveRememberedPassword(password)
                        tokenProvider.saveRememberMe(true)
                    } else {
                        tokenProvider.clearRememberedPassword()
                        tokenProvider.saveRememberMe(false)
                    }

                    TopicManager.subscribeToPromotions()
                    bookingViewModel.subscribeToUserSessionTopics()

                    state = AuthScreenState.Success
                }

            } catch (e: HttpException) {
                state = AuthScreenState.Error(parseBackendError(e))
            } catch (_: ConnectException) {
                state = AuthScreenState.Error("Sem ligação ao servidor.")
            } catch (_: Exception) {
                state = AuthScreenState.Error("Erro desconhecido.")
            }
        }
    }

    fun signup() {
        if (name.isBlank() || email.isBlank() || password.isBlank() || phone.isBlank()) {
            state = AuthScreenState.Error("Preenche todos os campos.")
            return
        }

        viewModelScope.launch {
            state = AuthScreenState.Loading

            try {
                authRepo.register(name, email, password, phone)
                TopicManager.subscribeToPromotions()
                state = AuthScreenState.Success

            } catch (e: HttpException) {
                state = AuthScreenState.Error(parseBackendError(e))
            } catch (_: ConnectException) {
                state = AuthScreenState.Error("Sem ligação ao servidor.")
            } catch (_: Exception) {
                state = AuthScreenState.Error("Erro desconhecido.")
            }
        }
    }
    fun forgotPassword() {
        if (email.isBlank()) {
            state = AuthScreenState.Error("Insere o teu email.")
            return
        }

        viewModelScope.launch {
            state = AuthScreenState.Loading

            try {
                userRepo.forgotPassword(email)
                state = AuthScreenState.Success

            } catch (e: HttpException) {
                state = AuthScreenState.Error(parseBackendError(e))
            } catch (_: ConnectException) {
                state = AuthScreenState.Error("Sem ligação ao servidor.")
            } catch (_: Exception) {
                state = AuthScreenState.Error("Erro desconhecido.")
            }
        }
    }
    fun resetPassword() {
        if (resetToken.isBlank() || password.isBlank()) {
            state = AuthScreenState.Error("Preenche todos os campos.")
            return
        }

        viewModelScope.launch {
            state = AuthScreenState.Loading

            try {
                userRepo.resetPassword(resetToken, password)
                state = AuthScreenState.Success

            } catch (e: HttpException) {
                state = AuthScreenState.Error(parseBackendError(e))
            } catch (_: ConnectException) {
                state = AuthScreenState.Error("Sem ligação ao servidor.")
            } catch (_: Exception) {
                state = AuthScreenState.Error("Erro desconhecido.")
            }
        }
    }

    fun togglePasswordVisibility() {
        passwordVisible = !passwordVisible
    }

    fun onNameChanged(value: String) { name = value }
    fun onEmailChanged(value: String) { email = value }
    fun onPasswordChanged(value: String) { password = value }
    fun onPhoneChanged(value: String) { phone = value }

    fun onRememberMeChanged(value: Boolean) {
        rememberMe = value
    }

    fun clearState() {
        state = AuthScreenState.Idle
    }

    private fun parseBackendError(e: HttpException): String {
        return try {

            val body = e.response()?.errorBody()?.string()?.trim()

            if (body.isNullOrBlank()) {
                return when (e.code()) {
                    400 -> "Dados inválidos."
                    401 -> "Não autorizado."
                    404 -> "Recurso não encontrado."
                    else -> "Erro interno do servidor."
                }
            }

            val clean = body
                .removePrefix("\"")
                .removeSuffix("\"")
                .removePrefix("Erro interno: ")
                .trim()

            if (clean.startsWith("[") && clean.endsWith("]")) {
                val errors = Json.decodeFromString<List<String>>(clean)
                return errors.joinToString("\n")
            }

            return when {

                clean.contains("Invalid credentials", true) ->
                    "Credenciais inválidas."

                clean.contains("Email and password", true) ->
                    "Preenche email e password."

                clean.contains("already exists", true) ->
                    "Já existe uma conta com este email."

                clean.contains("Password is required", true) ->
                    "A password é obrigatória."

                clean.contains("Email format", true) ->
                    "Formato de email inválido."

                clean.contains("Email is required", true) ->
                    "O email é obrigatório."

                clean.contains("Role is required", true) ->
                    "O perfil é obrigatório."

                clean.contains("Invalid role", true) ->
                    "Perfil inválido (apenas User ou Staff)."

                clean.contains("Phone number", true) ||
                        clean.contains("Portuguese phone", true) ->
                    "Número de telefone inválido."

                clean.contains("No user found", true) ->
                    "Utilizador não encontrado."

                clean.contains("O email é obrigatório", true) ->
                    "O email é obrigatório."

                clean.contains("Token inválido", true) ||
                        clean.contains("Invalid token", true) ||
                        clean.contains("expired", true) ->
                    "Token inválido ou expirado."

                clean.contains("Password redefinida", true) ->
                    "Password alterada com sucesso."

                clean.contains("nova password", true) ->
                    "Preenche todos os campos."

                clean.contains("refresh token", true) ->
                    "Sessão expirada. Inicia sessão novamente."

                else -> clean
            }

        } catch (_: Exception) {
            "Erro ao interpretar resposta."
        }
    }
}
