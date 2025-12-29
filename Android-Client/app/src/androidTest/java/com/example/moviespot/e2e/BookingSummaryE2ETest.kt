package com.example.moviespot.e2e

import androidx.activity.ComponentActivity
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.ui.semantics.SemanticsProperties
import androidx.compose.ui.semantics.getOrNull
import androidx.compose.ui.test.*
import androidx.compose.ui.test.junit4.createAndroidComposeRule
import androidx.test.ext.junit.runners.AndroidJUnit4
import com.example.moviespot.data.dto.BookingCreateDto
import com.example.moviespot.data.remote.auth.TokenProvider
import com.example.moviespot.di.FakeDiModule
import com.example.moviespot.presentation.screens.booking.BookingSummaryScreen
import com.example.moviespot.presentation.screens.booking.BookingViewModel
import com.example.moviespot.presentation.screens.booking.VoucherViewModel
import kotlinx.serialization.json.Json
import okhttp3.mockwebserver.*
import org.junit.*
import org.junit.runner.RunWith

@RunWith(AndroidJUnit4::class)
class BookingSummaryE2ETest {

    @get:Rule
    val composeRule = createAndroidComposeRule<ComponentActivity>()

    private lateinit var server: MockWebServer

    // Variáveis para capturar o resultado da navegação (callback)
    private var proceedCalled = false
    private var proceedBookingId: Int? = null
    private var proceedVoucherId: Int? = null
    private var proceedUserId: Int? = null

    @Before
    fun setup() {
        server = MockWebServer()
        server.dispatcher = dispatcher
        server.start()
        // Inicializar o FakeDiModule para usar o URL deste MockServer
        FakeDiModule.init(server.url("/").toString())
    }

    @After
    fun teardown() {
        server.shutdown()
    }

    // --- MOCK TOKEN PROVIDER ---
    // Simula um utilizador autenticado (ID 1) para evitar o erro "Sessão expirada"
    private val mockTokenProvider = object : TokenProvider {
        override fun getUserId(): Int? = 1
        override fun getAccessToken(): String? = "fake_access"
        override fun getRefreshToken(): String? = "fake_refresh"
        override fun saveAccessToken(token: String) {}
        override fun saveRefreshToken(token: String) {}
        override fun saveTokens(access: String, refresh: String) {}
        override fun clear() {}
        override fun saveRememberedPassword(password: String) {}
        override fun getRememberedPassword(): String? = null
        override fun clearRememberedPassword() {}
        override fun saveRememberMe(enabled: Boolean) {}
        override fun getRememberMe(): Boolean = false
        override fun clearRememberMe() {}
    }

    // --- Helper para Regex (Ignora diferenças de Ponto vs Vírgula) ---
    private fun hasTextMatching(regex: Regex): SemanticsMatcher =
        SemanticsMatcher("Has text matching $regex") { node ->
            val textList = node.config.getOrNull(SemanticsProperties.Text)
            textList?.any { it.text.matches(regex) } ?: false
        }

    // --- JSON MOCKS ---
    private val sessionJson = """
    {
      "id":99,
      "movieId":1,
      "movieTitle":"Duna",
      "cinemaHallId":10,
      "cinemaHallName":"NOS Colombo",
      "createdBy":1,
      "createdByName":"Admin",
      "startDate":"2099-12-31T21:30:00",
      "endDate":"2099-12-31T23:30:00",
      "price":10.0,
      "createdAt":"2025-01-01T12:00:00",
      "updatedAt":"2025-01-01T12:00:00"
    }
    """

    private val seat1Json = """
    {
      "id":1,
      "cinemaHallId":10,
      "seatNumber":"A1",
      "seatType":"Normal",
      "price":10.00,
      "createdAt":"2025-01-01T12:00:00",
      "updatedAt":"2025-01-01T12:00:00"
    }
    """

    private val seat2Json = """
    {
      "id":2,
      "cinemaHallId":10,
      "seatNumber":"A2",
      "seatType":"Normal",
      "price":10.00,
      "createdAt":"2025-01-01T12:00:00",
      "updatedAt":"2025-01-01T12:00:00"
    }
    """

    private val voucherJson = """
    {
      "id":5,
      "code":"PROMO10",
      "value":0.1,
      "validUntil":"2099-12-31",
      "maxUsages":10,
      "usages":2,
      "createdAt":"2025-01-01T12:00:00",
      "updatedAt":"2025-01-01T12:00:00"
    }
    """

    private val createBookingJson = """
    {
      "id":777,
      "userId":1,
      "sessionId":99,
      "bookingDate":"2025-01-01T12:00:00",
      "status":true,
      "totalAmount":18.00,
      "createdAt":"2025-01-01T12:00:00",
      "updatedAt":"2025-01-01T12:00:00"
    }
    """

    // Flags para controlar comportamento do servidor nos testes de erro
    private var bookingErrorCode: Int? = null
    private var voucherErrorCode: Int? = null
    private var simulateVoucherNetworkError = false

    private val dispatcher = object : Dispatcher() {
        override fun dispatch(request: RecordedRequest): MockResponse {
            val path = request.path ?: ""

            return when {
                path == "/Session/99" && request.method == "GET" ->
                    MockResponse().setResponseCode(200).setBody(sessionJson)

                path.contains("/seat/") && path.contains("/price/") && request.method == "GET" -> {
                    when {
                        path.contains("/1/price/") -> MockResponse().setBody(seat1Json)
                        path.contains("/2/price/") -> MockResponse().setBody(seat2Json)
                        else -> MockResponse().setResponseCode(404)
                    }
                }

                path.startsWith("/Voucher/validate/") && request.method == "GET" ->
                    when {
                        simulateVoucherNetworkError ->
                            MockResponse().setSocketPolicy(SocketPolicy.DISCONNECT_AT_START)
                        voucherErrorCode != null ->
                            MockResponse().setResponseCode(voucherErrorCode!!).setBody("{}")
                        else ->
                            MockResponse().setResponseCode(200).setBody(voucherJson)
                    }

                path == "/Booking" && request.method == "POST" ->
                    if (bookingErrorCode != null) {
                        MockResponse().setResponseCode(bookingErrorCode!!).setBody("{}")
                    } else {
                        val body = request.body.readUtf8()
                        val received = Json.decodeFromString<BookingCreateDto>(body)
                        Assert.assertEquals(1, received.userId)
                        Assert.assertEquals(99, received.sessionId)
                        Assert.assertEquals(listOf(1, 2), received.seatIds)
                        MockResponse().setResponseCode(200).setBody(createBookingJson)
                    }

                path.startsWith("/Booking/user/") -> MockResponse().setBody("[]")
                else -> MockResponse().setResponseCode(404)
            }
        }
    }

    private fun launchScreen() {
        // Criar ViewModels com dependências injetadas do FakeDiModule + MockProvider
        val bookingViewModel = BookingViewModel(
            FakeDiModule.bookingRepository,
            FakeDiModule.sessionRepository,
            FakeDiModule.seatRepository,
            FakeDiModule.reviewRepository,
            mockTokenProvider // Injeção do mock aqui
        )

        val voucherViewModel = VoucherViewModel(
            FakeDiModule.voucherRepository
        )

        composeRule.setContent {
            // Disparar o carregamento dos dados
            LaunchedEffect(Unit) {
                bookingViewModel.loadSessionForSummary(99)
                bookingViewModel.setSeatsForSummary(listOf(1, 2), 99)
            }

            BookingSummaryScreen(
                bookingViewModel = bookingViewModel,
                voucherViewModel = voucherViewModel,
                sessionId = 99,
                seatIds = listOf(1, 2),
                onProceedPayment = { bId, vId, uId ->
                    proceedCalled = true
                    proceedBookingId = bId
                    proceedVoucherId = vId
                    proceedUserId = uId
                },
                onBack = {}
            )
        }

        // Aguardar que o carregamento termine (20.00€ ou 20,00€)
        composeRule.waitUntil(timeoutMillis = 5000) {
            composeRule.onAllNodes(hasTextMatching(Regex("20[.,]00€")))
                .fetchSemanticsNodes().isNotEmpty()
        }
    }

    private fun resetFlags() {
        proceedCalled = false
        proceedBookingId = null
        proceedVoucherId = null
        proceedUserId = null
        bookingErrorCode = null
        voucherErrorCode = null
        simulateVoucherNetworkError = false
    }

    @Test
    fun success_withVoucher() {
        resetFlags()
        launchScreen()

        composeRule.onNodeWithTag("VoucherCodeInput").performTextInput("PROMO10")
        composeRule.onNodeWithText("Apply Voucher").performClick()

        composeRule.waitUntil {
            composeRule.onAllNodes(hasTextMatching(Regex("18[.,]00€")))
                .fetchSemanticsNodes().isNotEmpty()
        }

        composeRule.onNodeWithText("Proceed to Payment").performClick()

        composeRule.waitUntil { proceedCalled }
        Assert.assertEquals(777, proceedBookingId)
        Assert.assertEquals(5, proceedVoucherId)
        Assert.assertEquals(1, proceedUserId)
    }

    @Test
    fun success_withoutVoucher() {
        resetFlags()
        launchScreen()

        composeRule.onNodeWithText("Proceed to Payment").performClick()

        composeRule.waitUntil { proceedCalled }
        Assert.assertEquals(777, proceedBookingId)
        Assert.assertNull(proceedVoucherId)
    }

    @Test
    fun voucher_not_found_404() {
        resetFlags()
        voucherErrorCode = 404
        launchScreen()

        composeRule.onNodeWithTag("VoucherCodeInput").performTextInput("INVALID")
        composeRule.onNodeWithText("Apply Voucher").performClick()

        composeRule.waitUntil {
            composeRule.onAllNodesWithText("Voucher não encontrado.").fetchSemanticsNodes().isNotEmpty()
        }
    }

    @OptIn(ExperimentalTestApi::class)
    @Test
    fun voucher_invalid_400() {
        resetFlags()
        voucherErrorCode = 400
        launchScreen()

        composeRule.onNodeWithTag("VoucherCodeInput").performTextInput("EXPIRED")
        composeRule.onNodeWithText("Apply Voucher").performClick()

        composeRule.waitUntilAtLeastOneExists(
            hasText("Voucher inválido, expirado ou sem usos."),
            timeoutMillis = 5000
        )
    }

    @Test
    fun voucher_network_error() {
        resetFlags()
        simulateVoucherNetworkError = true
        launchScreen()

        composeRule.onNodeWithTag("VoucherCodeInput").performTextInput("PROMO10")
        composeRule.onNodeWithText("Apply Voucher").performClick()

        composeRule.waitUntil {
            composeRule.onAllNodesWithText("Sem ligação à internet.").fetchSemanticsNodes().isNotEmpty()
        }
    }

    @Test
    fun booking_400_invalid_seats() {
        resetFlags()
        bookingErrorCode = 400
        launchScreen()

        composeRule.onNodeWithText("Proceed to Payment").performClick()

        composeRule.waitUntil {
            composeRule.onAllNodesWithText("Lugares inválidos ou já ocupados.")
                .fetchSemanticsNodes().isNotEmpty()
        }
    }

    @Test
    fun booking_generic_error_500() {
        resetFlags()
        bookingErrorCode = 500
        launchScreen()

        composeRule.onNodeWithText("Proceed to Payment").performClick()

        composeRule.waitUntil {
            composeRule.onAllNodes(
                hasText("Erro no servidor", substring = true)
            ).fetchSemanticsNodes().isNotEmpty()
        }
    }
}