package com.example.moviespot.e2e

import androidx.activity.ComponentActivity
import androidx.compose.ui.test.*
import androidx.compose.ui.test.junit4.createAndroidComposeRule
import androidx.test.ext.junit.runners.AndroidJUnit4
import com.example.moviespot.di.FakeDiModule
import com.example.moviespot.presentation.screens.movie.MovieViewModel
import com.example.moviespot.presentation.screens.movie.movie_detail.MovieDetailsScreen
import okhttp3.mockwebserver.MockResponse
import okhttp3.mockwebserver.MockWebServer
import org.junit.*
import org.junit.runner.RunWith

@RunWith(AndroidJUnit4::class)
class MovieDetailsE2ETest {

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

    @Test
    fun loads_details_success() {
        // 1. Mock de resposta com sucesso
        val movieJson = """
            {
                "id": 1,
                "title": "Filme Teste",
                "description": "Descrição do filme teste.",
                "language": "en",
                "releaseDate": "2024-01-01",
                "country": "US",
                "posterPath": "/path.jpg",
                "genres": ["Action", "Sci-Fi"]
            }
        """.trimIndent()
        server.enqueue(MockResponse().setResponseCode(200).setBody(movieJson))

        val vm = MovieViewModel(FakeDiModule.movieRepository)

        composeRule.setContent {
            // Simular o comportamento do NavHost que dispara o load
            androidx.compose.runtime.LaunchedEffect(Unit) {
                vm.loadMovieDetails(1)
            }
            MovieDetailsScreen(
                viewModel = vm,
                onBack = {},
                onWatchClick = {}
            )
        }

        // 2. Verificar Título
        composeRule.waitUntil(5000) {
            composeRule.onAllNodesWithText("Filme Teste").fetchSemanticsNodes().isNotEmpty()
        }

        // 3. Verificar Descrição (Tab Overview é a padrão)
        composeRule.onNodeWithText("Descrição do filme teste.").assertIsDisplayed()

        // 4. Verificar Géneros (joinToString() usa ", " por defeito)
        composeRule.onNodeWithText("Action, Sci-Fi").assertIsDisplayed()

        // 5. Verificar Botão Watch
        composeRule.onNodeWithText("Watch ▶").assertIsDisplayed()
    }

    @Test
    fun show_error_404_not_found() {
        // Mock 404
        server.enqueue(MockResponse().setResponseCode(404))

        val vm = MovieViewModel(FakeDiModule.movieRepository)

        composeRule.setContent {
            androidx.compose.runtime.LaunchedEffect(Unit) {
                vm.loadMovieDetails(999)
            }
            MovieDetailsScreen(vm, {}, {})
        }

        // Verificar mensagem tratada no parseBackendError
        composeRule.waitUntil(5000) {
            composeRule.onAllNodesWithText("Recurso não encontrado.").fetchSemanticsNodes().isNotEmpty()
        }
        composeRule.onNodeWithText("Recurso não encontrado.").assertIsDisplayed()
    }

    @Test
    fun show_network_error() {
        // Simular falha de rede
        server.shutdown()

        val vm = MovieViewModel(FakeDiModule.movieRepository)

        composeRule.setContent {
            androidx.compose.runtime.LaunchedEffect(Unit) {
                vm.loadMovieDetails(1)
            }
            MovieDetailsScreen(vm, {}, {})
        }

        // Verificar mensagem de ConnectException
        composeRule.waitUntil(5000) {
            composeRule.onAllNodesWithText("Sem ligação ao servidor.").fetchSemanticsNodes().isNotEmpty()
        }
        composeRule.onNodeWithText("Sem ligação ao servidor.").assertIsDisplayed()
    }

    @Test
    fun show_server_error_500() {
        // Mock Erro 500
        server.enqueue(MockResponse().setResponseCode(500))

        val vm = MovieViewModel(FakeDiModule.movieRepository)

        composeRule.setContent {
            androidx.compose.runtime.LaunchedEffect(Unit) {
                vm.loadMovieDetails(1)
            }
            MovieDetailsScreen(vm, {}, {})
        }

        // Verificar mensagem genérica para códigos de erro HTTP
        composeRule.waitUntil(5000) {
            composeRule.onAllNodesWithText("Erro interno do servidor.").fetchSemanticsNodes().isNotEmpty()
        }
        composeRule.onNodeWithText("Erro interno do servidor.").assertIsDisplayed()
    }
}