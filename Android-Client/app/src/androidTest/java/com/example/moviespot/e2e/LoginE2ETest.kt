package com.example.moviespot.e2e

import androidx.activity.ComponentActivity
import androidx.compose.ui.test.*
import androidx.compose.ui.test.junit4.createAndroidComposeRule
import androidx.test.ext.junit.runners.AndroidJUnit4
import com.example.moviespot.data.remote.auth.AuthManager
import com.example.moviespot.di.FakeDiModule
import com.example.moviespot.presentation.screens.auth.AuthViewModel
import com.example.moviespot.presentation.screens.auth.login.LoginScreen
import com.example.moviespot.presentation.screens.booking.BookingViewModel
import okhttp3.mockwebserver.MockResponse
import okhttp3.mockwebserver.MockWebServer
import org.junit.*
import org.junit.runner.RunWith

@RunWith(AndroidJUnit4::class)
class LoginE2ETest {

    @get:Rule
    val composeRule = createAndroidComposeRule<ComponentActivity>()

    private lateinit var server: MockWebServer

    @Before
    fun setup() {
        server = MockWebServer()
        server.start()
        FakeDiModule.init(server.url("/").toString())
    }

    @After
    fun teardown() {
        server.shutdown()
    }

    // Helper para instanciar o ViewModel com as dependências do FakeDi
    private fun createViewModel(): AuthViewModel {
        return AuthViewModel(
            authRepo = FakeDiModule.authRepository,
            userRepo = FakeDiModule.userRepository,
            tokenProvider = AuthManager(composeRule.activity),
            bookingViewModel = BookingViewModel(
                FakeDiModule.bookingRepository,
                FakeDiModule.sessionRepository,
                FakeDiModule.seatRepository,
                FakeDiModule.reviewRepository,
                tokenProvider = AuthManager(composeRule.activity)
            )
        )
    }

    @Test
    fun login_fails_with_empty_fields() {
        val vm = createViewModel()

        composeRule.setContent {
            LoginScreen(
                viewModel = vm,
                onLoginSuccess = {},
                onNavigateToSignUp = {},
                onForgotPassword = {}
            )
        }

        // Clicar em Login sem preencher nada
        composeRule.onNodeWithText("Login").performClick()

        // Verificar validação local
        composeRule.onNodeWithText("Preenche todos os campos.").assertIsDisplayed()
    }

    @Test
    fun login_success() {
        // Mock de resposta de sucesso com tokens
        val successBody = """
            {
                "email": "teste@gmail.com",
                "accessToken": "fake_access_token",
                "expiresIn": 3600,
                "refreshToken": "fake_refresh_token"
            }
        """.trimIndent()

        server.enqueue(MockResponse().setResponseCode(200).setBody(successBody))

        val vm = createViewModel()
        var successCalled = false

        composeRule.setContent {
            LoginScreen(
                viewModel = vm,
                onLoginSuccess = { successCalled = true },
                onNavigateToSignUp = {},
                onForgotPassword = {}
            )
        }

        composeRule.onNodeWithText("Email").performTextInput("teste@gmail.com")
        composeRule.onNodeWithText("Password").performTextInput("123456")
        composeRule.onNodeWithText("Login").performClick()

        // Esperar pela callback de sucesso
        composeRule.waitUntil { successCalled }
    }

    @Test
    fun login_fails_invalid_credentials() {
        // Mock de erro 401
        server.enqueue(MockResponse().setResponseCode(401).setBody("\"Invalid credentials\""))

        val vm = createViewModel()

        composeRule.setContent {
            LoginScreen(
                viewModel = vm,
                onLoginSuccess = {},
                onNavigateToSignUp = {},
                onForgotPassword = {}
            )
        }

        composeRule.onNodeWithText("Email").performTextInput("errado@gmail.com")
        composeRule.onNodeWithText("Password").performTextInput("errada")
        composeRule.onNodeWithText("Login").performClick()

        // Verificar a mensagem traduzida pelo ViewModel
        composeRule.waitUntil {
            composeRule.onAllNodesWithText("Credenciais inválidas.").fetchSemanticsNodes().isNotEmpty()
        }
    }

    @Test
    fun login_fails_network_error() {
        // Simular falha de rede
        server.shutdown()

        val vm = createViewModel()

        composeRule.setContent {
            LoginScreen(
                viewModel = vm,
                onLoginSuccess = {},
                onNavigateToSignUp = {},
                onForgotPassword = {}
            )
        }

        composeRule.onNodeWithText("Email").performTextInput("net@gmail.com")
        composeRule.onNodeWithText("Password").performTextInput("123456")
        composeRule.onNodeWithText("Login").performClick()

        // Verificar erro de conexão
        composeRule.waitUntil {
            composeRule.onAllNodesWithText("Sem ligação ao servidor.").fetchSemanticsNodes().isNotEmpty()
        }
    }

    @Test
    fun login_fails_generic_server_error() {
        // Simular erro 500
        server.enqueue(MockResponse().setResponseCode(500))

        val vm = createViewModel()

        composeRule.setContent {
            LoginScreen(
                viewModel = vm,
                onLoginSuccess = {},
                onNavigateToSignUp = {},
                onForgotPassword = {}
            )
        }

        composeRule.onNodeWithText("Email").performTextInput("server@gmail.com")
        composeRule.onNodeWithText("Password").performTextInput("123456")
        composeRule.onNodeWithText("Login").performClick()

        // Verificar erro genérico
        composeRule.waitUntil {
            composeRule.onAllNodesWithText("Erro interno do servidor.").fetchSemanticsNodes().isNotEmpty()
        }
    }
}