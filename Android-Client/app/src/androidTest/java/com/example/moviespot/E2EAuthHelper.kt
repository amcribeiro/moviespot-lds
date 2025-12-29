package com.example.moviespot

import androidx.compose.ui.test.*
import androidx.compose.ui.test.junit4.ComposeTestRule

fun loginAsValidUser(rule: ComposeTestRule) {

    rule.waitUntil(5_000) {
        rule.onAllNodes(hasText("Email") and hasSetTextAction())
            .fetchSemanticsNodes()
            .isNotEmpty()
    }

    rule.onNode(hasText("Email") and hasSetTextAction())
        .performTextReplacement("Amc2003@gmail.com")

    rule.onNode(hasText("Password") and hasSetTextAction())
        .performTextReplacement("Andreziki")

    rule.onNodeWithText("Login")
        .performClick()

    rule.waitUntil(10_000) {
        rule.onAllNodes(hasText("Trending"))
            .fetchSemanticsNodes()
            .isNotEmpty()
    }
}
