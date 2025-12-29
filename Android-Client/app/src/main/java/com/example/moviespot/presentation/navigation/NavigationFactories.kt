package com.example.moviespot.presentation.navigation

import android.content.Context
import androidx.lifecycle.ViewModel
import androidx.lifecycle.ViewModelProvider
import com.example.moviespot.data.remote.api.AuthApiService
import com.example.moviespot.data.remote.api.BookingApiService
import com.example.moviespot.data.remote.api.CinemaApiService
import com.example.moviespot.data.remote.api.CinemaHallApiService
import com.example.moviespot.data.remote.api.MovieApiService
import com.example.moviespot.data.remote.api.PaymentApiService
import com.example.moviespot.data.remote.api.ReviewApiService
import com.example.moviespot.data.remote.api.SeatApiService
import com.example.moviespot.data.remote.api.SessionApiService
import com.example.moviespot.data.remote.api.UserApiService
import com.example.moviespot.data.remote.api.VoucherApiService
import com.example.moviespot.data.remote.auth.AuthManager
import com.example.moviespot.data.remote.network.AuthRetrofit
import com.example.moviespot.data.remote.network.AuthorizedRetrofit
import com.example.moviespot.data.repository.AuthRepository
import com.example.moviespot.data.repository.BookingRepository
import com.example.moviespot.data.repository.CinemaHallRepository
import com.example.moviespot.data.repository.CinemaRepository
import com.example.moviespot.data.repository.MovieRepository
import com.example.moviespot.data.repository.PaymentRepository
import com.example.moviespot.data.repository.ReviewRepository
import com.example.moviespot.data.repository.SeatRepository
import com.example.moviespot.data.repository.SessionRepository
import com.example.moviespot.data.repository.UserRepository
import com.example.moviespot.data.repository.VoucherRepository
import com.example.moviespot.presentation.screens.auth.AuthViewModel
import com.example.moviespot.presentation.screens.booking.BookingViewModel
import com.example.moviespot.presentation.screens.movie.MovieViewModel
import com.example.moviespot.presentation.screens.payment.PaymentViewModel
import com.example.moviespot.presentation.screens.booking.VoucherViewModel
import com.example.moviespot.presentation.screens.cinemas.CinemaViewModel
import com.example.moviespot.presentation.screens.reviews.ReviewViewModel
import com.example.moviespot.presentation.screens.seats.SeatSelectionViewModel
import com.example.moviespot.presentation.screens.session.SessionViewModel

class MovieVMFactory(private val context: Context) : ViewModelProvider.Factory {
    override fun <T : ViewModel> create(modelClass: Class<T>): T {

        val tokenProvider = AuthManager(context)

        AuthorizedRetrofit.initializeTokenProvider(tokenProvider)

        val api = AuthorizedRetrofit.create(MovieApiService::class.java)
        val repo = MovieRepository(api)
        @Suppress("UNCHECKED_CAST")
        return MovieViewModel(repo) as T
    }
}

class SessionVMFactory(private val context: Context) : ViewModelProvider.Factory {
    override fun <T : ViewModel> create(modelClass: Class<T>): T {

        val tokenProvider = AuthManager(context)
        AuthorizedRetrofit.initializeTokenProvider(tokenProvider)

        val cinemaApi = AuthorizedRetrofit.create(CinemaApiService::class.java)
        val hallApi = AuthorizedRetrofit.create(CinemaHallApiService::class.java)
        val sessionApi = AuthorizedRetrofit.create(SessionApiService::class.java)

        val cinemaRepo = CinemaRepository(cinemaApi)
        val hallRepo = CinemaHallRepository(hallApi)
        val sessionRepo = SessionRepository(sessionApi)

        @Suppress("UNCHECKED_CAST")
        return SessionViewModel(cinemaRepo, hallRepo, sessionRepo) as T
    }
}

class SeatSelectionVMFactory(private val context: Context) : ViewModelProvider.Factory {
    override fun <T : ViewModel> create(modelClass: Class<T>): T {

        val tokenProvider = AuthManager(context)
        AuthorizedRetrofit.initializeTokenProvider(tokenProvider)

        val sessionApi = AuthorizedRetrofit.create(SessionApiService::class.java)
        val seatApi = AuthorizedRetrofit.create(SeatApiService::class.java)

        val sessionRepo = SessionRepository(sessionApi)
        val seatRepo = SeatRepository(seatApi)

        @Suppress("UNCHECKED_CAST")
        return SeatSelectionViewModel(sessionRepo, seatRepo) as T
    }
}

class VoucherVMFactory(private val context: Context) : ViewModelProvider.Factory {
    override fun <T : ViewModel> create(modelClass: Class<T>): T {

        val tokenProvider = AuthManager(context)
        AuthorizedRetrofit.initializeTokenProvider(tokenProvider)

        val api = AuthorizedRetrofit.create(VoucherApiService::class.java)
        val repo = VoucherRepository(api)

        @Suppress("UNCHECKED_CAST")
        return VoucherViewModel(repo) as T
    }
}

class BookingVMFactory(private val context: Context) : ViewModelProvider.Factory {
    override fun <T : ViewModel> create(modelClass: Class<T>): T {

        val tokenProvider = AuthManager(context)
        AuthorizedRetrofit.initializeTokenProvider(tokenProvider)

        val api = AuthorizedRetrofit.create(BookingApiService::class.java)
        val session_api = AuthorizedRetrofit.create(SessionApiService::class.java)
        val seat_api = AuthorizedRetrofit.create(SeatApiService::class.java)
        val review_api = AuthorizedRetrofit.create(ReviewApiService::class.java)
        val repo = BookingRepository(api)
        val session = SessionRepository(session_api)
        val seat = SeatRepository(seat_api)
        val review = ReviewRepository(review_api)

        @Suppress("UNCHECKED_CAST")
        return BookingViewModel(repo, session, seat, review, tokenProvider) as T
    }
}

class PaymentVMFactory(private val context: Context) : ViewModelProvider.Factory {
    override fun <T : ViewModel> create(modelClass: Class<T>): T {

        val tokenProvider = AuthManager(context)
        AuthorizedRetrofit.initializeTokenProvider(tokenProvider)

        val paymentApi = AuthorizedRetrofit.create(PaymentApiService::class.java)

        val repo = PaymentRepository(paymentApi)

        @Suppress("UNCHECKED_CAST")
        return PaymentViewModel(repo) as T
    }
}

class AuthVMFactory(private val context: Context) : ViewModelProvider.Factory {

    override fun <T : ViewModel> create(modelClass: Class<T>): T {

        val tokenProvider = AuthManager(context)
        AuthorizedRetrofit.initializeTokenProvider(tokenProvider)
        val authApi = AuthRetrofit.create(AuthApiService::class.java)
        val userApi = AuthRetrofit.create(UserApiService::class.java)
        val authRepo = AuthRepository(authApi, tokenProvider)
        val userRepo = UserRepository(userApi)
        val bookingViewModel = BookingVMFactory(context).create(BookingViewModel::class.java)

        @Suppress("UNCHECKED_CAST")
        return AuthViewModel(authRepo, userRepo, tokenProvider, bookingViewModel) as T
    }
}

class CinemaVMFactory(private val context: Context) : ViewModelProvider.Factory {
    override fun <T : ViewModel> create(modelClass: Class<T>): T {

        val tokenProvider = AuthManager(context)
        AuthorizedRetrofit.initializeTokenProvider(tokenProvider)

        val cinemaApi = AuthorizedRetrofit.create(CinemaApiService::class.java)
        val cinemaRepo = CinemaRepository(cinemaApi)

        @Suppress("UNCHECKED_CAST")
        return CinemaViewModel(cinemaRepo) as T
    }
}

class ReviewVMFactory(private val context: Context) : ViewModelProvider.Factory {

    override fun <T : ViewModel> create(modelClass: Class<T>): T {

        val tokenProvider = AuthManager(context)
        AuthorizedRetrofit.initializeTokenProvider(tokenProvider)

        val api = AuthorizedRetrofit.create(ReviewApiService::class.java)
        val repo = ReviewRepository(api)

        @Suppress("UNCHECKED_CAST")
        return ReviewViewModel(repo) as T
    }
}









