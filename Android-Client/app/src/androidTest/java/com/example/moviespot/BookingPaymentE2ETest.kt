
package com.example.moviespot

import androidx.compose.ui.semantics.SemanticsProperties
import androidx.compose.ui.semantics.getOrNull
import androidx.compose.ui.test.SemanticsMatcher
import androidx.compose.ui.test.*
import androidx.compose.ui.test.junit4.createAndroidComposeRule
import androidx.test.ext.junit.runners.AndroidJUnit4
import androidx.test.filters.LargeTest
import org.junit.Rule
import org.junit.Test
import org.junit.runner.RunWith

private fun hasTextMatching(regex: Regex): SemanticsMatcher =
    SemanticsMatcher("Has text matching $regex") { node ->
        val texts = node.config.getOrNull(SemanticsProperties.Text)
        texts?.any { it.text.matches(regex) } ?: false
    }

@RunWith(AndroidJUnit4::class)
@LargeTest
class BookingPaymentE2ETest {

    @get:Rule
    val composeRule = createAndroidComposeRule<MainActivity>()

    @Test
    fun fullBookingFlow_untilStripePaymentIntent() {

        loginAsValidUser(composeRule)


        composeRule.waitUntil(10_000) {
            composeRule.onAllNodes(hasText("Trending"))
                .fetchSemanticsNodes()
                .isNotEmpty()
        }

        composeRule
            .onAllNodesWithText("xXx", substring = true)
            .filter(hasClickAction())
            .onFirst()
            .performClick()

        composeRule.waitUntil(timeoutMillis = 10_000) {
            composeRule.onAllNodes(hasText("Watch", substring = true))
                .fetchSemanticsNodes()
                .isNotEmpty()
        }

        composeRule.onNodeWithText("Watch ▶", substring = true)
            .performClick()

        composeRule.waitUntil(10_000) {
            composeRule.onAllNodes(hasText("Lisboa", substring = true))
                .fetchSemanticsNodes().isNotEmpty()
        }

        composeRule
            .onNode(hasText("Lisboa", substring = true) and hasClickAction())
            .performClick()

        val dayRegex = Regex("\\d{4}-\\d{2}-\\d{2}")

        composeRule.waitUntil(10_000) {
            composeRule.onAllNodes(hasTextMatching(dayRegex))
                .fetchSemanticsNodes().isNotEmpty()
        }

        composeRule.onNode(hasTextMatching(dayRegex))
            .performClick()

        composeRule.waitUntil {
            composeRule.onAllNodes(hasText("Sala", substring = true))
                .fetchSemanticsNodes().isNotEmpty()
        }

        composeRule
            .onNode(hasText("Sala", substring = true) and hasClickAction())
            .performClick()

        val hour = Regex("\\d{2}:\\d{2}")

        composeRule.waitUntil(10_000) {
            composeRule.onAllNodes(hasTextMatching(hour))
                .fetchSemanticsNodes().isNotEmpty()
        }

        composeRule
            .onNode(hasTextMatching(hour))
            .performClick()

        composeRule.waitUntil(10_000) {
            composeRule.onAllNodes(hasText("Selecionar Lugares"))
                .fetchSemanticsNodes()
                .isNotEmpty()
        }

        val seatMatcher =
            hasTextMatching(Regex("\\d+")) and
                    hasClickAction() and
                    isEnabled()

        composeRule.waitUntil(10_000) {
            composeRule.onAllNodes(seatMatcher)
                .fetchSemanticsNodes()
                .isNotEmpty()
        }

        composeRule.onAllNodes(seatMatcher)[0].performClick()
        composeRule.onAllNodes(seatMatcher)[1].performClick()

        composeRule.onNodeWithText("Continuar")
            .performClick()

        composeRule.waitUntil(10_000) {
            composeRule.onAllNodes(hasText("Booking Summary"))
                .fetchSemanticsNodes()
                .isNotEmpty()
        }

        composeRule.onNodeWithText("Proceed to Payment")
            .performClick()

        composeRule.waitUntil(10_000) {
            composeRule.onAllNodesWithText("Pagar")
                .fetchSemanticsNodes()
                .isNotEmpty()
        }

        composeRule.onNodeWithText("Pagar")
            .performClick()


        // ===============================================================
        // VERIFICAR INÍCIO DO STRIPE INTENT
        // ===============================================================
        // Neste ponto o Stripe PaymentSheet é aberto.
        // Espresso não consegue controlar a UI do Stripe,
        // mas conseguimos validar que chegámos até este passo.
    }
}
