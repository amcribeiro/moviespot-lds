package com.example.moviespot.e2e

import androidx.activity.ComponentActivity
import androidx.compose.ui.test.*
import androidx.compose.ui.test.junit4.createAndroidComposeRule
import com.example.moviespot.di.FakeDiModule
import com.example.moviespot.presentation.screens.movie.MovieViewModel
import com.example.moviespot.presentation.screens.movie.movie_list_home.Movie_List_Home_Screen
import okhttp3.mockwebserver.MockResponse
import okhttp3.mockwebserver.MockWebServer
import org.junit.*
import java.util.concurrent.TimeUnit

class MovieHomeE2ETest {

    @get:Rule
    val composeRule = createAndroidComposeRule<ComponentActivity>()

    private lateinit var server: MockWebServer

    private val successJson = """
    [
      {
        "id": 1,
        "title": "Duna",
        "description": "desc",
        "language": "en",
        "releaseDate": "2024-01-01",
        "country": "USA",
        "posterPath": "/duna.jpg",
        "genres": ["Sci-Fi"]
      }
    ]
    """

    @Before
    fun setup() {
        server = MockWebServer()
        server.start()
        FakeDiModule.init(server.url("/").toString())
    }

    @After
    fun teardown() {
        try {
            server.shutdown()
        } catch (_: Exception) {}
    }

    @Test
    fun screen_shows_loading() {

        server.enqueue(
            MockResponse()
                .setBodyDelay(3, TimeUnit.SECONDS)
                .setResponseCode(200)
                .setBody(successJson)
        )

        val repo = FakeDiModule.movieRepository
        val viewModel = MovieViewModel(repo)

        composeRule.setContent {
            Movie_List_Home_Screen(
                viewModel = viewModel,
                onMovieClick = {},
                onNavigateToFullList = {}
            )
        }

        composeRule.waitUntil(timeoutMillis = 2_000) {
            viewModel.isLoadingMovies
        }

        assert(viewModel.isLoadingMovies) {
            "Loading state not triggered"
        }
    }


    @Test
    fun screen_loads_movies_successfully() {

        server.enqueue(
            MockResponse()
                .setResponseCode(200)
                .setBody(successJson)
        )

        val repo = FakeDiModule.movieRepository
        val viewModel = MovieViewModel(repo)

        composeRule.setContent {
            Movie_List_Home_Screen(
                viewModel = viewModel,
                onMovieClick = {},
                onNavigateToFullList = {}
            )
        }

        composeRule.waitUntil(timeoutMillis = 10_000) {
            composeRule
                .onAllNodesWithText("Duna")
                .fetchSemanticsNodes()
                .isNotEmpty()
        }

        assert(
            composeRule.onAllNodesWithText("Duna")
                .fetchSemanticsNodes()
                .isNotEmpty()
        ) { "Movie not rendered on UI" }
    }


    @Test
    fun screen_shows_http_error() {

        server.enqueue(
            MockResponse().setResponseCode(500)
        )

        val repo = FakeDiModule.movieRepository
        val viewModel = MovieViewModel(repo)

        composeRule.setContent {
            Movie_List_Home_Screen(
                viewModel = viewModel,
                onMovieClick = {},
                onNavigateToFullList = {}
            )
        }

        composeRule.waitUntil(timeoutMillis = 10_000) {
            composeRule
                .onAllNodesWithText("Erro interno do servidor.")
                .fetchSemanticsNodes()
                .isNotEmpty()
        }

        assert(
            composeRule
                .onAllNodesWithText("Erro interno do servidor.")
                .fetchSemanticsNodes()
                .isNotEmpty()
        ) { "Empty state not displayed" }
    }

    @Test
    fun screen_shows_connection_error() {

        server.shutdown()

        val repo = FakeDiModule.movieRepository
        val viewModel = MovieViewModel(repo)

        composeRule.setContent {
            Movie_List_Home_Screen(
                viewModel = viewModel,
                onMovieClick = {},
                onNavigateToFullList = {}
            )
        }

        composeRule.waitUntil(timeoutMillis = 10_000) {
            viewModel.moviesError != null
        }

        assert(
            composeRule
                .onAllNodesWithText("Sem ligação ao servidor.")
                .fetchSemanticsNodes()
                .isNotEmpty()
        ) { "Connection error not shown" }
    }

    @Test
    fun screen_shows_unexpected_error() {

        server.enqueue(
            MockResponse()
                .setResponseCode(200)
                .setBody("NOT_JSON")
        )

        val repo = FakeDiModule.movieRepository
        val viewModel = MovieViewModel(repo)

        composeRule.setContent {
            Movie_List_Home_Screen(
                viewModel = viewModel,
                onMovieClick = {},
                onNavigateToFullList = {}
            )
        }

        composeRule.waitUntil(timeoutMillis = 10_000) {
            viewModel.moviesError != null
        }

        assert(
            composeRule
                .onAllNodesWithText("Erro inesperado ao carregar filmes.")
                .fetchSemanticsNodes()
                .isNotEmpty()
        ) { "Unexpected error not shown" }
    }
}
