package com.example.moviespot.e2e

import androidx.activity.ComponentActivity
import androidx.compose.ui.test.*
import androidx.compose.ui.test.junit4.createAndroidComposeRule
import androidx.test.ext.junit.runners.AndroidJUnit4
import com.example.moviespot.di.FakeDiModule
import com.example.moviespot.presentation.screens.cinemas.CinemaListScreen
import com.example.moviespot.presentation.screens.cinemas.CinemaViewModel
import com.example.moviespot.presentation.utils.LocationViewModel
import okhttp3.mockwebserver.MockResponse
import okhttp3.mockwebserver.MockWebServer
import org.junit.*
import org.junit.runner.RunWith

@RunWith(AndroidJUnit4::class)
class CinemaListE2ETest {

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
    fun shows_cinema_list_successfully() {
        // 1. Mock de resposta com 2 cinemas
        val cinemasJson = """
            [
              {
                "id": 1,
                "name": "Cinema City Leiria",
                "street": "Rua A",
                "city": "Leiria",
                "country": "PT",
                "latitude": 39.7,
                "longitude": -8.8,
                "createdAt": "2024-01-01",
                "updatedAt": "2024-01-01",
                "totalCinemaHalls": 5
              },
              {
                "id": 2,
                "name": "NOS Colombo",
                "street": "Av. Lusíada",
                "city": "Lisboa",
                "country": "PT",
                "latitude": 38.7,
                "longitude": -9.1,
                "createdAt": "2024-01-01",
                "updatedAt": "2024-01-01",
                "totalCinemaHalls": 10
              }
            ]
        """.trimIndent()

        server.enqueue(MockResponse().setBody(cinemasJson))

        val cinemaVM = CinemaViewModel(FakeDiModule.cinemaRepository)
        val locationVM = LocationViewModel() // Instância real (sem location mockada inicialmente)

        composeRule.setContent {
            CinemaListScreen(
                context = composeRule.activity,
                viewModel = cinemaVM,
                locationVM = locationVM,
                onCinemaClick = {}
            )
        }

        // 2. Verificar se os cinemas aparecem
        composeRule.waitUntil(5000) {
            composeRule.onAllNodesWithText("Cinema City Leiria").fetchSemanticsNodes().isNotEmpty()
        }

        composeRule.onNodeWithText("Cinema City Leiria").assertIsDisplayed()
        composeRule.onNodeWithText("Leiria").assertIsDisplayed()

        composeRule.onNodeWithText("NOS Colombo").assertIsDisplayed()
        composeRule.onNodeWithText("Lisboa").assertIsDisplayed()

        // 3. Verificar botão de Mapa
        composeRule.onAllNodesWithText("Ver no Maps").onFirst().assertIsDisplayed()
    }

    @Test
    fun shows_distance_when_location_available() {
        // 1. Mock de um cinema em coordenadas conhecidas (ex: (0,0))
        val cinemasJson = """
            [
              {
                "id": 1,
                "name": "Cinema Perto",
                "street": "Rua X",
                "city": "TestCity",
                "country": "PT",
                "latitude": 0.0,
                "longitude": 0.0,
                "createdAt": "2024-01-01",
                "updatedAt": "2024-01-01",
                "totalCinemaHalls": 1
              }
            ]
        """.trimIndent()

        server.enqueue(MockResponse().setBody(cinemasJson))

        val cinemaVM = CinemaViewModel(FakeDiModule.cinemaRepository)
        val locationVM = LocationViewModel()

        composeRule.setContent {
            CinemaListScreen(
                context = composeRule.activity,
                viewModel = cinemaVM,
                locationVM = locationVM,
                onCinemaClick = {}
            )
        }

        // Esperar carregar
        composeRule.waitUntil(5000) {
            composeRule.onAllNodesWithText("Cinema Perto").fetchSemanticsNodes().isNotEmpty()
        }

        // 2. Simular localização do utilizador (ex: (1,1) ~157km de distância)
        // Como o fetchUserLocation pode falhar no emulador, injetamos valores diretamente nos StateFlows
        locationVM.userLat.value = 1.0
        locationVM.userLon.value = 1.0

        // 3. Verificar se a distância aparece (regex para "km")
        composeRule.waitUntil(2000) {
            composeRule.onAllNodesWithText("km", substring = true).fetchSemanticsNodes().isNotEmpty()
        }

        composeRule.onNodeWithText("km", substring = true).assertIsDisplayed()
    }

    @Test
    fun shows_empty_state_message() {
        // Mock Lista Vazia
        server.enqueue(MockResponse().setBody("[]"))

        val cinemaVM = CinemaViewModel(FakeDiModule.cinemaRepository)
        val locationVM = LocationViewModel()

        composeRule.setContent {
            CinemaListScreen(
                context = composeRule.activity,
                viewModel = cinemaVM,
                locationVM = locationVM,
                onCinemaClick = {}
            )
        }

        // Verificar mensagem de vazio
        composeRule.waitUntil(5000) {
            composeRule.onAllNodesWithText("Não existem cinemas para mostrar.")
                .fetchSemanticsNodes().isNotEmpty()
        }

        composeRule.onNodeWithText("Não existem cinemas para mostrar.").assertIsDisplayed()
    }

    @Test
    fun shows_error_message_on_server_fail() {
        // Mock Erro 500
        server.enqueue(MockResponse().setResponseCode(500))

        val cinemaVM = CinemaViewModel(FakeDiModule.cinemaRepository)
        val locationVM = LocationViewModel()

        composeRule.setContent {
            CinemaListScreen(
                context = composeRule.activity,
                viewModel = cinemaVM,
                locationVM = locationVM,
                onCinemaClick = {}
            )
        }

        // Verificar mensagem de erro genérica do ViewModel
        composeRule.waitUntil(5000) {
            composeRule.onAllNodesWithText("Erro ao carregar cinemas.")
                .fetchSemanticsNodes().isNotEmpty()
        }

        composeRule.onNodeWithText("Erro ao carregar cinemas.").assertIsDisplayed()
    }
}