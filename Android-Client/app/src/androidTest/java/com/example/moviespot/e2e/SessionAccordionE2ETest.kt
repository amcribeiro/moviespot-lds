package com.example.moviespot.e2e

import android.os.Build
import androidx.activity.ComponentActivity
import androidx.compose.ui.test.*
import androidx.compose.ui.test.junit4.createAndroidComposeRule
import androidx.test.ext.junit.runners.AndroidJUnit4
import androidx.test.filters.SdkSuppress
import com.example.moviespot.di.FakeDiModule
import com.example.moviespot.presentation.screens.session.SessionAccordionScreen
import com.example.moviespot.presentation.screens.session.SessionViewModel
import okhttp3.mockwebserver.MockResponse
import okhttp3.mockwebserver.MockWebServer
import org.junit.*
import org.junit.runner.RunWith

@RunWith(AndroidJUnit4::class)
@SdkSuppress(minSdkVersion = Build.VERSION_CODES.O)
class SessionAccordionE2ETest {

    @get:Rule
    val composeRule = createAndroidComposeRule<ComponentActivity>()

    private lateinit var server: MockWebServer

    private val cinemasJson = """
[
  {
    "id": 1,
    "name": "NOS Colombo",
    "street": "Colombo",
    "city": "Lisboa",
    "state": null,
    "zipCode": null,
    "country": "PT",
    "latitude": 38.754,
    "longitude": -9.171,
    "createdAt": "2024-01-01T00:00:00",
    "updatedAt": "2024-01-01T00:00:00",
    "totalCinemaHalls": 1
  }
]
"""

    private val hallsJson = """
    [
      {
        "id": 10,
        "name": "Sala 1",
        "cinemaId": 1
      }
    ]
    """

    private val sessionsJson = """
[
  {
    "id": 100,
    "movieId": 99,
    "movieTitle": "Duna",
    "cinemaHallId": 10,
    "cinemaHallName": "Sala 1",
    "createdBy": 1,
    "createdByName": "Admin",
    "startDate": "2099-12-31T22:00:00Z",
    "endDate": "2099-12-31T23:30:00Z",
    "price": 8.50,
    "createdAt": "2024-01-01T00:00:00Z",
    "updatedAt": "2024-01-01T00:00:00Z"
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
    fun sessions_screen_loads_successfully() {

        server.enqueue(MockResponse().setResponseCode(200).setBody(cinemasJson))
        server.enqueue(MockResponse().setResponseCode(200).setBody(hallsJson))
        server.enqueue(MockResponse().setResponseCode(200).setBody(sessionsJson))

        val vm = SessionViewModel(
            FakeDiModule.cinemaRepository,
            FakeDiModule.cinemaHallRepository,
            FakeDiModule.sessionRepository
        )

        composeRule.setContent {
            SessionAccordionScreen(
                movieId = 99,
                viewModel = vm,
                onBack = {},
                onSessionClick = {}
            )
        }

        composeRule.waitUntil(timeoutMillis = 10_000) {
            composeRule
                .onAllNodesWithText("NOS Colombo")
                .fetchSemanticsNodes()
                .isNotEmpty()
        }

        assert(
            composeRule
                .onAllNodesWithText("NOS Colombo")
                .fetchSemanticsNodes()
                .isNotEmpty()
        ) { "Cinema data not rendered" }
    }

    @Test
    fun sessions_screen_shows_empty_message() {

        server.enqueue(MockResponse().setResponseCode(200).setBody(cinemasJson))
        server.enqueue(MockResponse().setResponseCode(200).setBody(hallsJson))
        server.enqueue(MockResponse().setResponseCode(200).setBody("[]"))

        val vm = SessionViewModel(
            FakeDiModule.cinemaRepository,
            FakeDiModule.cinemaHallRepository,
            FakeDiModule.sessionRepository
        )

        composeRule.setContent {
            SessionAccordionScreen(
                movieId = 123,
                viewModel = vm,
                onBack = {},
                onSessionClick = {}
            )
        }

        composeRule.waitUntil(timeoutMillis = 10_000) {
            composeRule
                .onAllNodesWithText("Não existem sessões disponíveis para este filme.")
                .fetchSemanticsNodes()
                .isNotEmpty()
        }

        assert(
            composeRule
                .onAllNodesWithText("Não existem sessões disponíveis para este filme.")
                .fetchSemanticsNodes()
                .isNotEmpty()
        ) { "Empty state message not shown" }
    }

    @Test
    fun sessions_screen_shows_error() {

        server.enqueue(MockResponse().setResponseCode(500))

        val vm = SessionViewModel(
            FakeDiModule.cinemaRepository,
            FakeDiModule.cinemaHallRepository,
            FakeDiModule.sessionRepository
        )

        composeRule.setContent {
            SessionAccordionScreen(
                movieId = 1,
                viewModel = vm,
                onBack = {},
                onSessionClick = {}
            )
        }

        composeRule.waitUntil(timeoutMillis = 10_000) {
            vm.error.value != null
        }

        assert(
            composeRule.onAllNodesWithText(vm.error.value!!)
                .fetchSemanticsNodes()
                .isNotEmpty()
        ) { "Error message not displayed" }
    }
}
