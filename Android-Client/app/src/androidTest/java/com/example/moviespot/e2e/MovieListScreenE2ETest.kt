package com.example.moviespot.e2e

import androidx.activity.ComponentActivity
import androidx.compose.ui.test.*
import androidx.compose.ui.test.junit4.createAndroidComposeRule
import androidx.test.ext.junit.runners.AndroidJUnit4
import com.example.moviespot.di.FakeDiModule
import com.example.moviespot.presentation.screens.movie.MovieViewModel
import com.example.moviespot.presentation.screens.movie.movie_list.MovieListScreen
import okhttp3.mockwebserver.MockResponse
import okhttp3.mockwebserver.MockWebServer
import org.junit.*
import org.junit.runner.RunWith

@RunWith(AndroidJUnit4::class)
class MovieListE2ETest {

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

    // JSON de exemplo para simular a resposta da API
    private val moviesJson = """
        [
            {
                "id": 1,
                "title": "Inception",
                "description": "Dreams...",
                "language": "en",
                "releaseDate": "2010-07-16",
                "country": "USA",
                "posterPath": "/inception.jpg",
                "genres": ["Sci-Fi"]
            },
            {
                "id": 2,
                "title": "Matrix",
                "description": "Simulation...",
                "language": "en",
                "releaseDate": "1999-03-31",
                "country": "USA",
                "posterPath": "/matrix.jpg",
                "genres": ["Sci-Fi"]
            }
        ]
    """.trimIndent()

    @Test
    fun shows_movies_list_success() {
        // Simular resposta 200 OK
        server.enqueue(MockResponse().setResponseCode(200).setBody(moviesJson))

        val vm = MovieViewModel(FakeDiModule.movieRepository)

        composeRule.setContent {
            MovieListScreen(
                viewModel = vm,
                onMovieClick = {},
                onOpenFilter = {}
            )
        }

        // O AsyncImage usa o título do filme como contentDescription
        composeRule.waitUntil(5000) {
            composeRule.onAllNodesWithContentDescription("Inception").fetchSemanticsNodes().isNotEmpty()
        }

        composeRule.onNodeWithContentDescription("Inception").assertIsDisplayed()
        composeRule.onNodeWithContentDescription("Matrix").assertIsDisplayed()
    }

    @Test
    fun shows_server_error_500() {
        // Simular Erro 500
        server.enqueue(MockResponse().setResponseCode(500))

        val vm = MovieViewModel(FakeDiModule.movieRepository)

        composeRule.setContent {
            MovieListScreen(
                viewModel = vm,
                onMovieClick = {},
                onOpenFilter = {}
            )
        }

        composeRule.waitUntil(5000) {
            composeRule.onAllNodesWithText("Erro interno do servidor.").fetchSemanticsNodes().isNotEmpty()
        }

        composeRule.onNodeWithText("Erro interno do servidor.").assertIsDisplayed()
    }

    @Test
    fun shows_network_error() {
        // Simular falta de rede (servidor desligado)
        server.shutdown()

        val vm = MovieViewModel(FakeDiModule.movieRepository)

        composeRule.setContent {
            MovieListScreen(
                viewModel = vm,
                onMovieClick = {},
                onOpenFilter = {}
            )
        }

        composeRule.waitUntil(5000) {
            composeRule.onAllNodesWithText("Sem ligação ao servidor.").fetchSemanticsNodes().isNotEmpty()
        }

        composeRule.onNodeWithText("Sem ligação ao servidor.").assertIsDisplayed()
    }

    @Test
    fun navigation_actions_work() {
        server.enqueue(MockResponse().setBody(moviesJson))

        val vm = MovieViewModel(FakeDiModule.movieRepository)
        var movieClickedId: Int? = null

        composeRule.setContent {
            MovieListScreen(
                viewModel = vm,
                onMovieClick = { movieClickedId = it },
                onOpenFilter = {}
            )
        }

        composeRule.waitUntil(5000) {
            composeRule.onAllNodesWithContentDescription("Inception").fetchSemanticsNodes().isNotEmpty()
        }

        // Simular clique no filme
        composeRule.onNodeWithContentDescription("Inception").performClick()

        // Verificar se o ID correto foi capturado
        Assert.assertEquals(1, movieClickedId)
    }
}