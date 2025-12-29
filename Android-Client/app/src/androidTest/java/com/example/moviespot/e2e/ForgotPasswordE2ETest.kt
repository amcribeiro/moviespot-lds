package com.example.moviespot.e2e

import androidx.activity.ComponentActivity
import androidx.compose.ui.test.*
import androidx.compose.ui.test.junit4.createAndroidComposeRule
import androidx.test.ext.junit.runners.AndroidJUnit4
import com.example.moviespot.data.remote.auth.AuthManager
import com.example.moviespot.di.FakeDiModule
import com.example.moviespot.presentation.screens.auth.AuthViewModel
import com.example.moviespot.presentation.screens.auth.forgot_password.ForgotPasswordScreen
import com.example.moviespot.presentation.screens.booking.BookingViewModel
import okhttp3.mockwebserver.MockResponse
import okhttp3.mockwebserver.MockWebServer
import org.junit.*
import org.junit.runner.RunWith

@RunWith(AndroidJUnit4::class)
class ForgotPasswordE2ETest {

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

    private fun createViewModel() = AuthViewModel(
        FakeDiModule.authRepository, FakeDiModule.userRepository,
        AuthManager(composeRule.activity),
        BookingViewModel(FakeDiModule.bookingRepository, FakeDiModule.sessionRepository, FakeDiModule.seatRepository, FakeDiModule.reviewRepository, tokenProvider = AuthManager(composeRule.activity))
    )

    @Test
    fun fails_empty_email() {
        val vm = createViewModel()
        composeRule.setContent { ForgotPasswordScreen(vm, {}) }

        composeRule.onNodeWithText("Send Reset Email").performClick()
        composeRule.onNodeWithText("Insere o teu email.").assertIsDisplayed()
    }

    @Test
    fun success_valid_email() {
        server.enqueue(MockResponse().setResponseCode(200).setBody("\"OK\""))
        val vm = createViewModel()

        var success = false
        composeRule.setContent {
            ForgotPasswordScreen(vm, { success = true })
        }

        composeRule.onNodeWithText("Email").performTextInput("valid@email.com")
        composeRule.onNodeWithText("Send Reset Email").performClick()

        composeRule.waitUntil { success }
    }

    @Test
    fun fails_user_not_found() {
        server.enqueue(MockResponse().setResponseCode(404).setBody("\"No user found\""))
        val vm = createViewModel()
        composeRule.setContent { ForgotPasswordScreen(vm, {}) }

        composeRule.onNodeWithText("Email").performTextInput("unknown@email.com")
        composeRule.onNodeWithText("Send Reset Email").performClick()

        composeRule.waitUntil {
            composeRule.onAllNodesWithText("Utilizador não encontrado.").fetchSemanticsNodes().isNotEmpty()
        }
    }
}