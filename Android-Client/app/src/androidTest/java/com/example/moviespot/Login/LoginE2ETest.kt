package com.example.moviespot.Login

import androidx.compose.ui.test.assertIsDisplayed
import androidx.compose.ui.test.hasSetTextAction
import androidx.compose.ui.test.hasText
import androidx.compose.ui.test.junit4.createAndroidComposeRule
import androidx.compose.ui.test.onAllNodesWithText
import androidx.compose.ui.test.onNodeWithText
import androidx.compose.ui.test.performClick
import androidx.compose.ui.test.performTextReplacement
import androidx.test.ext.junit.runners.AndroidJUnit4
import androidx.test.filters.LargeTest
import com.example.moviespot.MainActivity
import org.junit.Rule
import org.junit.Test
import org.junit.runner.RunWith

@RunWith(AndroidJUnit4::class)
@LargeTest
class LoginE2ETest {

    @get:Rule
    val composeRule = createAndroidComposeRule<MainActivity>()

    private fun waitForLoginScreen() {
        composeRule.waitUntil(timeoutMillis = 5_000) {
            composeRule
                .onAllNodes(hasText("Email") and hasSetTextAction())
                .fetchSemanticsNodes()
                .isNotEmpty()
        }
    }

    @Test
    fun login_withValidCredentials_navigatesToHome() {

        val email = "Amc2003@gmail.com"
        val password = "Andreziki"

        waitForLoginScreen()

        composeRule
            .onNode(hasText("Email") and hasSetTextAction())
            .performTextReplacement(email)

        composeRule
            .onNode(hasText("Password") and hasSetTextAction())
            .performTextReplacement(password)

        composeRule.onNodeWithText("Login")
            .performClick()

        composeRule.waitUntil(10_000) {
            composeRule
                .onAllNodes(hasText("Trending"))
                .fetchSemanticsNodes()
                .isNotEmpty()
        }

        composeRule
            .onNodeWithText("Trending")
            .assertIsDisplayed()
    }

    @Test
    fun login_withWrongPassword_showsErrorMessage() {

        val email = "Amc2003@gmail.com"
        val password = "password_errada"

        waitForLoginScreen()

        composeRule
            .onNode(hasText("Email") and hasSetTextAction())
            .performTextReplacement(email)

        composeRule
            .onNode(hasText("Password") and hasSetTextAction())
            .performTextReplacement(password)

        composeRule.onNodeWithText("Login")
            .performClick()

        composeRule.waitUntil(timeoutMillis = 8_000) {
            composeRule
                .onAllNodesWithText("Invalid credentials.")
                .fetchSemanticsNodes()
                .isNotEmpty()
        }

        composeRule
            .onNodeWithText("Invalid credentials.")
            .assertIsDisplayed()
    }

    @Test
    fun login_withInvalidEmail_showsErrorMessage() {

        val email = "email-invalido"
        val password = "Andreziki"

        waitForLoginScreen()

        composeRule
            .onNode(hasText("Email") and hasSetTextAction())
            .performTextReplacement(email)

        composeRule
            .onNode(hasText("Password") and hasSetTextAction())
            .performTextReplacement(password)

        composeRule.onNodeWithText("Login")
            .performClick()

        composeRule.waitUntil(timeoutMillis = 8_000) {
            composeRule
                .onAllNodesWithText("Invalid credentials.")
                .fetchSemanticsNodes()
                .isNotEmpty()
        }

        composeRule
            .onNodeWithText("Invalid credentials.")
            .assertIsDisplayed()
    }

    @Test
    fun login_withEmptyFields_showsValidationErrors() {

        waitForLoginScreen()

        composeRule.onNodeWithText("Login")
            .performClick()

        composeRule.waitUntil(timeoutMillis = 2_000) {
            composeRule.onAllNodesWithText("Preenche todos os campos.")
                .fetchSemanticsNodes()
                .isNotEmpty()
        }

        composeRule
            .onNodeWithText("Preenche todos os campos.")
            .assertIsDisplayed()

    }
}