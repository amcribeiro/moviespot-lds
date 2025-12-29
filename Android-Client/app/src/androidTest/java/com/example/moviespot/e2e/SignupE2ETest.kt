package com.example.moviespot.e2e

import androidx.activity.ComponentActivity
import androidx.compose.ui.test.*
import androidx.compose.ui.test.junit4.createAndroidComposeRule
import androidx.test.ext.junit.runners.AndroidJUnit4
import com.example.moviespot.data.remote.auth.AuthManager
import com.example.moviespot.di.FakeDiModule
import com.example.moviespot.presentation.screens.auth.AuthViewModel
import com.example.moviespot.presentation.screens.auth.signup.SignUpScreen
import com.example.moviespot.presentation.screens.booking.BookingViewModel
import okhttp3.mockwebserver.MockResponse
import okhttp3.mockwebserver.MockWebServer
import org.junit.*
import org.junit.runner.RunWith

@RunWith(AndroidJUnit4::class)
class SignUpE2ETest {

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

    private fun setupScreen(vm: AuthViewModel) {
        composeRule.setContent {
            SignUpScreen(
                viewModel = vm,
                onSignUpSuccess = {},
                onBackToLogin = {}
            )
        }
    }

    private fun createViewModel(): AuthViewModel {
        return AuthViewModel(
            FakeDiModule.authRepository,
            FakeDiModule.userRepository,
            AuthManager(composeRule.activity),
            BookingViewModel(
                FakeDiModule.bookingRepository,
                FakeDiModule.sessionRepository,
                FakeDiModule.seatRepository,
                FakeDiModule.reviewRepository,
                tokenProvider = AuthManager(composeRule.activity)
            )
        )
    }

    @Test
    fun signup_fails_with_empty_fields() {
        val vm = createViewModel()
        setupScreen(vm)

        // Clicar sem preencher nada
        composeRule.onNodeWithText("Create Account").performClick()

        // Verificar erro de validação local
        composeRule.onNodeWithText("Preenche todos os campos.").assertIsDisplayed()
    }

    @Test
    fun signup_success() {
        // Mock de sucesso
        server.enqueue(MockResponse().setResponseCode(200).setBody("""
            {"email":"a@b.c","accessToken":"t","expiresIn":1,"refreshToken":"r"}
        """.trimIndent()))

        val vm = createViewModel()
        setupScreen(vm)

        composeRule.onNodeWithText("Name").performTextInput("User Test")
        composeRule.onNodeWithText("Email").performTextInput("valid@test.com")
        composeRule.onNodeWithText("Password").performTextInput("123456")
        composeRule.onNodeWithText("Phone").performTextInput("910000000")

        composeRule.onNodeWithText("Create Account").performClick()

        // Verifica se o estado passou para Success (o ecrã normalmente navegaria, aqui verificamos se o loading desapareceu e não há erro)
        composeRule.waitUntil { vm.state is com.example.moviespot.presentation.screens.auth.AuthScreenState.Success }
    }

    @Test
    fun signup_fails_email_already_exists() {
        // Mock de erro 400/409 do backend
        server.enqueue(MockResponse().setResponseCode(400).setBody("\"User already exists\""))

        val vm = createViewModel()
        setupScreen(vm)

        composeRule.onNodeWithText("Name").performTextInput("User Test")
        composeRule.onNodeWithText("Email").performTextInput("existing@test.com")
        composeRule.onNodeWithText("Password").performTextInput("123456")
        composeRule.onNodeWithText("Phone").performTextInput("910000000")

        composeRule.onNodeWithText("Create Account").performClick()

        // Verificar mensagem de erro vinda do backend (parseada pelo ViewModel)
        composeRule.waitUntil {
            composeRule.onAllNodesWithText("Já existe uma conta com este email.").fetchSemanticsNodes().isNotEmpty()
        }
    }

    @Test
    fun signup_fails_network_error() {
        // Simular falha de rede
        server.shutdown()

        val vm = createViewModel()
        setupScreen(vm)

        composeRule.onNodeWithText("Name").performTextInput("User")
        composeRule.onNodeWithText("Email").performTextInput("net@test.com")
        composeRule.onNodeWithText("Password").performTextInput("123456")
        composeRule.onNodeWithText("Phone").performTextInput("912345678")

        composeRule.onNodeWithText("Create Account").performClick()

        composeRule.waitUntil {
            composeRule.onAllNodesWithText("Sem ligação ao servidor.").fetchSemanticsNodes().isNotEmpty()
        }
    }
}