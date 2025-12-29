package com.example.moviespot.e2e

import androidx.activity.ComponentActivity
import androidx.compose.ui.test.*
import androidx.compose.ui.test.junit4.createAndroidComposeRule
import androidx.test.ext.junit.runners.AndroidJUnit4
import com.example.moviespot.di.FakeDiModule
import com.example.moviespot.presentation.screens.seats.SeatSelectionRoute
import com.example.moviespot.presentation.screens.seats.SeatSelectionViewModel
import com.example.moviespot.presentation.screens.seats.SeatUiModel
import okhttp3.mockwebserver.MockResponse
import okhttp3.mockwebserver.MockWebServer
import org.junit.*
import org.junit.runner.RunWith

@RunWith(AndroidJUnit4::class)
class SeatSelectionE2ETest {

    @get:Rule
    val composeRule = createAndroidComposeRule<ComponentActivity>()

    private lateinit var server: MockWebServer

    private val sessionJson = """
    {
      "id": 99,
      "movieId": 1,
      "movieTitle": "Duna",
      "cinemaHallId": 10,
      "cinemaHallName": "Sala 1",
      "createdBy": 1,
      "createdByName": "Admin",
      "startDate": "2099-12-31T20:00:00Z",
      "endDate": "2099-12-31T22:00:00Z",
      "price": 8.50,
      "createdAt": "2024-01-01T00:00:00Z",
      "updatedAt": "2024-01-01T00:00:00Z"
    }
    """

    private val hallSeatsJson = """
    [
      {
        "id": 1,
        "cinemaHallId": 10,
        "seatNumber": "A1",
        "seatType": "Normal",
        "createdAt": "2024-01-01",
        "updatedAt": "2024-01-01"
      },
      {
        "id": 2,
        "cinemaHallId": 10,
        "seatNumber": "A2",
        "seatType": "VIP",
        "createdAt": "2024-01-01",
        "updatedAt": "2024-01-01"
      },
      {
        "id": 3,
        "cinemaHallId": 10,
        "seatNumber": "B1",
        "seatType": "Reduced",
        "createdAt": "2024-01-01",
        "updatedAt": "2024-01-01"
      }
    ]
    """

    private val availableSeatsJson = """
    [
      {"id":1,"seatNumber":"A1","seatType":"Normal"},
      {"id":3,"seatNumber":"B1","seatType":"Reduced"}
    ]
    """

    private fun priceJson(id: Int, type: String, price: Double) =
        """
    {
      "id": $id,
      "cinemaHallId": 10,
      "seatNumber": "X",
      "seatType": "$type",
      "price": $price
    }
    """

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
    fun seat_screen_loads_successfully() {

        enqueueHappyPath()

        val vm = SeatSelectionViewModel(
            FakeDiModule.sessionRepository,
            FakeDiModule.seatRepository
        )

        composeRule.setContent {
            SeatSelectionRoute(
                sessionId = 99,
                viewModel = vm,
                onBack = {},
                onConfirm = {}
            )
        }

        composeRule.waitUntil(timeoutMillis = 10_000) {
            composeRule
                .onAllNodesWithText("Duna")
                .fetchSemanticsNodes()
                .isNotEmpty()
        }

        assertTextExists("Duna")
        assertTextExists("Sala: Sala 1")

        assertTextExists("1")
        assertTextExists("2")
        assertTextExists("1", minCount = 2)
    }

    @Test
    fun can_select_and_unselect_seat() {

        enqueueHappyPath()

        val vm = SeatSelectionViewModel(
            FakeDiModule.sessionRepository,
            FakeDiModule.seatRepository
        )

        composeRule.setContent {
            SeatSelectionRoute(99, vm, {}, {})
        }

        composeRule.waitUntil {
            composeRule
                .onAllNodes(hasText("1"))
                .fetchSemanticsNodes()
                .isNotEmpty()
        }

        composeRule.onAllNodesWithText("1")[0].performClick()

        composeRule.waitUntil {
            vm.uiState.selectedSeatIds.isNotEmpty()
        }

        Assert.assertEquals(1, vm.uiState.selectedSeatIds.size)

        composeRule.onAllNodesWithText("1")[0].performClick()

        composeRule.waitUntil {
            vm.uiState.selectedSeatIds.isEmpty()
        }

        Assert.assertTrue(vm.uiState.selectedSeatIds.isEmpty())
    }

    @Test
    fun max_5_seats_limit_triggered() {

        enqueueHappyPath()

        val vm = SeatSelectionViewModel(
            FakeDiModule.sessionRepository,
            FakeDiModule.seatRepository
        )

        vm.uiState = vm.uiState.copy(
            seats = listOf(
                SeatUiModel(1, "A1", 'A', 1, "N", true, false, 1.0),
                SeatUiModel(2,"A2",'A',2,"N",true,false,1.0),
                SeatUiModel(3,"A3",'A',3,"N",true,false,1.0),
                SeatUiModel(4,"A4",'A',4,"N",true,false,1.0),
                SeatUiModel(5,"A5",'A',5,"N",true,false,1.0),
                SeatUiModel(6,"A6",'A',6,"N",true,false,1.0)
            )
        )

        repeat(5) { vm.onSeatClicked(it+1) }
        vm.onSeatClicked(6)

        Assert.assertEquals(
            "Só podes selecionar até 5 lugares.",
            vm.uiState.maxSelectedMessage
        )
    }

    @Test
    fun shows_error_when_api_fails() {

        server.enqueue(MockResponse().setResponseCode(500))

        val vm = SeatSelectionViewModel(
            FakeDiModule.sessionRepository,
            FakeDiModule.seatRepository
        )

        composeRule.setContent {
            SeatSelectionRoute(99, vm, {}, {})
        }

        composeRule.waitForIdle()

        composeRule.waitUntil(timeoutMillis = 10_000) {
            vm.uiState.error != null
        }

        composeRule.waitForIdle()

        val errorText = vm.uiState.error!!

        composeRule
            .onNodeWithText(errorText)
            .assertExists()
    }

    private fun enqueueHappyPath() {

        server.enqueue(MockResponse().setBody(sessionJson))
        server.enqueue(MockResponse().setBody(hallSeatsJson))
        server.enqueue(MockResponse().setBody(availableSeatsJson))

        server.enqueue(MockResponse().setBody(priceJson(1,"Normal",7.5)))
        server.enqueue(MockResponse().setBody(priceJson(2,"VIP",9.0)))
        server.enqueue(MockResponse().setBody(priceJson(3,"Reduced",6.0)))
    }

    private fun assertTextExists(text:String, minCount:Int = 1) {
        assert(
            composeRule.onAllNodesWithText(text).fetchSemanticsNodes().size >= minCount
        ) { "Expected text not found → `$text`" }
    }
}

