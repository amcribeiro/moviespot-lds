package com.example.moviespot.di

import com.example.moviespot.data.remote.api.*
import com.example.moviespot.data.repository.*
import com.example.moviespot.data.remote.auth.TokenProvider
import com.jakewharton.retrofit2.converter.kotlinx.serialization.asConverterFactory
import kotlinx.serialization.ExperimentalSerializationApi
import kotlinx.serialization.json.Json
import okhttp3.MediaType.Companion.toMediaType
import retrofit2.Retrofit

/**
 * Módulo de injeção de dependências FALSO (Fake) para testes instrumentados.
 *
 * Objetivo: Fornecer instâncias de Repositórios e APIs que comunicam com um
 * MockWebServer local em vez do servidor real backend.
 */
object FakeDiModule {

    // APIs Retrofit
    lateinit var movieApi: MovieApiService
    lateinit var cinemaApi: CinemaApiService
    lateinit var cinemaHallApi: CinemaHallApiService
    lateinit var sessionApi: SessionApiService
    lateinit var seatApi: SeatApiService
    lateinit var bookingApi: BookingApiService
    lateinit var voucherApi: VoucherApiService
    lateinit var authApi: AuthApiService
    lateinit var userApi: UserApiService
    lateinit var reviewApi: ReviewApiService
    lateinit var paymentApi: PaymentApiService
    lateinit var notificationApi: NotificationApi

    // Repositórios
    lateinit var movieRepository: MovieRepository
    lateinit var cinemaRepository: CinemaRepository
    lateinit var cinemaHallRepository: CinemaHallRepository
    lateinit var sessionRepository: SessionRepository
    lateinit var seatRepository: SeatRepository
    lateinit var bookingRepository: BookingRepository
    lateinit var voucherRepository: VoucherRepository
    lateinit var authRepository: AuthRepository
    lateinit var userRepository: UserRepository
    lateinit var reviewRepository: ReviewRepository
    lateinit var paymentRepository: PaymentRepository
    lateinit var notificationRepository: NotificationRepository

    @OptIn(ExperimentalSerializationApi::class)
    fun init(baseUrl: String) {

        val json = Json {
            isLenient = true
            ignoreUnknownKeys = true
            explicitNulls = false
            coerceInputValues = true
        }

        val retrofit = Retrofit.Builder()
            .baseUrl(baseUrl)
            .addConverterFactory(
                json.asConverterFactory("application/json".toMediaType())
            )
            .build()

        movieApi      = retrofit.create(MovieApiService::class.java)
        cinemaApi     = retrofit.create(CinemaApiService::class.java)
        cinemaHallApi = retrofit.create(CinemaHallApiService::class.java)
        sessionApi    = retrofit.create(SessionApiService::class.java)
        seatApi       = retrofit.create(SeatApiService::class.java)
        bookingApi    = retrofit.create(BookingApiService::class.java)
        voucherApi    = retrofit.create(VoucherApiService::class.java)
        authApi       = retrofit.create(AuthApiService::class.java)
        userApi       = retrofit.create(UserApiService::class.java)
        reviewApi     = retrofit.create(ReviewApiService::class.java)
        paymentApi    = retrofit.create(PaymentApiService::class.java)
        notificationApi = retrofit.create(NotificationApi::class.java)

        val mockTokenProvider = object : TokenProvider {
            override fun getAccessToken() = "fake_access_token_para_testes"
            override fun getRefreshToken() = "fake_refresh_token_para_testes"
            override fun saveTokens(a: String, r: String) {}
            override fun clear() {}
            override fun saveAccessToken(t: String) {}
            override fun saveRefreshToken(t: String) {}
            override fun saveRememberedPassword(p: String) {}
            override fun getRememberedPassword() = null
            override fun clearRememberedPassword() {}
            override fun saveRememberMe(e: Boolean) {}
            override fun getRememberMe() = false
            override fun clearRememberMe() {}
            override fun getUserId() = 1
        }

        movieRepository      = MovieRepository(movieApi)
        cinemaRepository     = CinemaRepository(cinemaApi)
        cinemaHallRepository = CinemaHallRepository(cinemaHallApi)
        sessionRepository    = SessionRepository(sessionApi)
        seatRepository       = SeatRepository(seatApi)
        bookingRepository    = BookingRepository(bookingApi)
        voucherRepository    = VoucherRepository(voucherApi)
        authRepository       = AuthRepository(authApi, mockTokenProvider)
        userRepository       = UserRepository(userApi)
        reviewRepository     = ReviewRepository(reviewApi)
        paymentRepository    = PaymentRepository(paymentApi)
        notificationRepository = NotificationRepository(notificationApi)
    }
}