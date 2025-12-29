package com.example.moviespot.e2e

import androidx.activity.ComponentActivity
import androidx.compose.ui.test.*
import androidx.compose.ui.test.junit4.createAndroidComposeRule
import androidx.test.ext.junit.runners.AndroidJUnit4
import com.example.moviespot.di.FakeDiModule
import com.example.moviespot.presentation.screens.reviews.ReviewScreen
import com.example.moviespot.presentation.screens.reviews.ReviewViewModel
import okhttp3.mockwebserver.MockResponse
import okhttp3.mockwebserver.MockWebServer
import org.junit.*
import org.junit.runner.RunWith

@RunWith(AndroidJUnit4::class)
class ReviewScreenE2ETest {

    @get:Rule
    val composeRule = createAndroidComposeRule<ComponentActivity>()

    private lateinit var server: MockWebServer

    @Before
    fun setup() {
        server = MockWebServer()
        server.start()
        // Inicializar o FakeDiModule para termos acesso ao reviewRepository
        FakeDiModule.init(server.url("/").toString())
    }

    @After
    fun teardown() {
        server.shutdown()
    }

    @Test
    fun submit_review_success() {
        // 1. Mock da resposta de sucesso (JSON da review criada)
        val reviewResponse = """
            {
                "id": 1,
                "bookingId": 100,
                "rating": 5,
                "comment": "Filme incrível!",
                "reviewDate": "2024-01-01",
                "createdAt": "2024-01-01",
                "updatedAt": "2024-01-01"
            }
        """.trimIndent()

        server.enqueue(MockResponse().setResponseCode(200).setBody(reviewResponse))

        val reviewVM = ReviewViewModel(FakeDiModule.reviewRepository)
        var successCalled = false

        composeRule.setContent {
            ReviewScreen(
                bookingId = 100,
                viewModel = reviewVM,
                onSuccessNavigateBack = { successCalled = true }
            )
        }

        // 2. Interagir com a UI
        // Selecionar 5 estrelas: Na UI, as estrelas são botões com texto "★".
        // Existem 5 botões. Vamos clicar no último para selecionar 5 estrelas.
        composeRule.onAllNodesWithText("★").onLast().performClick()

        // Escrever comentário
        composeRule.onNodeWithText("Escreve o teu comentário...")
            .performTextInput("Filme incrível!")

        // Enviar
        composeRule.onNodeWithText("Enviar avaliação").performClick()

        // 3. Verificar sucesso
        composeRule.waitUntil(5000) { successCalled }
        Assert.assertTrue(successCalled)
    }

    @Test
    fun submit_review_fails_no_rating() {
        // Não precisamos de mock server aqui porque é uma validação local do ViewModel
        val reviewVM = ReviewViewModel(FakeDiModule.reviewRepository)

        composeRule.setContent {
            ReviewScreen(
                bookingId = 100,
                viewModel = reviewVM,
                onSuccessNavigateBack = {}
            )
        }

        // Tentar enviar sem selecionar estrelas
        composeRule.onNodeWithText("Enviar avaliação").performClick()

        // Verificar mensagem de erro de validação
        composeRule.onNodeWithText("Escolhe uma classificação entre 1 e 5.").assertIsDisplayed()
    }

    @Test
    fun submit_review_fails_server_error() {
        // 1. Mock de erro 500
        server.enqueue(MockResponse().setResponseCode(500))

        val reviewVM = ReviewViewModel(FakeDiModule.reviewRepository)

        composeRule.setContent {
            ReviewScreen(
                bookingId = 100,
                viewModel = reviewVM,
                onSuccessNavigateBack = {}
            )
        }

        // Preencher dados válidos
        composeRule.onAllNodesWithText("★").onFirst().performClick() // 1 estrela
        composeRule.onNodeWithText("Escreve o teu comentário...").performTextInput("Mau serviço.")

        // Enviar
        composeRule.onNodeWithText("Enviar avaliação").performClick()

        // Verificar mensagem de erro genérica capturada pelo ViewModel
        // O ReviewViewModel apanha exceções e mete no state 'submitError'
        composeRule.waitUntil(5000) {
            composeRule.onAllNodesWithText("Erro", substring = true).fetchSemanticsNodes().isNotEmpty()
        }

        // Verifica se alguma mensagem de erro aparece (o texto exato depende da implementação do parse de erro ou da mensagem da exceção)
        // No ReviewViewModel.kt: "Erro ao enviar a review." ou a mensagem da exceção
        composeRule.onNodeWithText("Erro", substring = true).assertIsDisplayed()
    }
}