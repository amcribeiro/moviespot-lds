package com.example.moviespot.presentation.navigation

import android.annotation.SuppressLint
import android.content.Context
import android.os.Build
import androidx.annotation.RequiresApi
import androidx.compose.foundation.layout.padding
import androidx.compose.material3.Scaffold
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.getValue
import androidx.compose.runtime.remember
import androidx.compose.runtime.rememberUpdatedState
import androidx.compose.ui.Modifier
import androidx.compose.ui.platform.LocalContext
import androidx.lifecycle.viewmodel.compose.viewModel
import androidx.navigation.NavBackStackEntry
import androidx.navigation.NavHostController
import androidx.navigation.NavType
import androidx.navigation.compose.NavHost
import androidx.navigation.compose.composable
import androidx.navigation.compose.navigation
import androidx.navigation.compose.rememberNavController
import androidx.navigation.navArgument
import com.example.moviespot.data.remote.auth.AuthManager

import com.example.moviespot.presentation.screens.auth.*
import com.example.moviespot.presentation.screens.auth.login.*
import com.example.moviespot.presentation.screens.auth.signup.*
import com.example.moviespot.presentation.screens.auth.forgot_password.*
import com.example.moviespot.presentation.screens.auth.reset_password.*

import com.example.moviespot.presentation.screens.movie.*
import com.example.moviespot.presentation.screens.movie.movie_detail.MovieDetailsScreen
import com.example.moviespot.presentation.screens.movie.movie_list.MovieFilterScreen
import com.example.moviespot.presentation.screens.movie.movie_list.MovieListScreen
import com.example.moviespot.presentation.screens.movie.movie_list_home.Movie_List_Home_Screen

import com.example.moviespot.presentation.screens.session.*
import com.example.moviespot.presentation.screens.seats.*
import com.example.moviespot.presentation.screens.booking.*
import com.example.moviespot.presentation.screens.payment.*

import com.example.moviespot.presentation.screens.cinemas.*
import com.example.moviespot.presentation.screens.reviews.ReviewScreen
import com.example.moviespot.presentation.screens.reviews.ReviewViewModel
import com.example.moviespot.presentation.utils.LocationViewModel
import com.example.moviespot.presentation.screens.welcome.WelcomeScreen

/* ===============================================================
   ✅ FUNÇÃO PARA PARTILHAR MovieViewModel NO moviesGraph
   =============================================================== */

@Composable
fun NavBackStackEntry.sharedMovieVM(
    navController: NavHostController,
    context: Context
): MovieViewModel {

    val parent = remember(this) {
        navController.getBackStackEntry("moviesGraph")
    }

    return viewModel(parent, factory = MovieVMFactory(context))
}

@Composable
fun NavBackStackEntry.sharedBookingVM(
    navController: NavHostController,
    context: Context
): BookingViewModel {

    val parent = remember(this) {
        navController.getBackStackEntry("root")
    }

    return viewModel(parent, factory = BookingVMFactory(context))
}

/* ===============================================================
   ====================== APP NAVIGATION =========================
   =============================================================== */

@RequiresApi(Build.VERSION_CODES.O)
@SuppressLint("UnrememberedGetBackStackEntry")
@Composable
fun AppNavigation() {

    val navController = rememberNavController()
    val ctx = LocalContext.current

    Scaffold(
        bottomBar = { BottomNavBar(navController) }
    ) { innerPadding ->

        NavHost(
            modifier = Modifier.padding(innerPadding),
            route = "root",
            navController = navController,
            startDestination = "welcome"
        ) {

            /* ------------------- WELCOME ------------------- */

            composable("welcome") {
                WelcomeScreen(
                    onLoginClick = { navController.navigate("login") },
                    onSignUpClick = { navController.navigate("signup") },
                    onNavigateHome = {
                        navController.navigate("home") {
                            popUpTo("welcome") { inclusive = true }
                        }
                    }
                )
            }

            /* ------------------- AUTH ------------------- */

            composable("login") {
                val vm: AuthViewModel = viewModel(factory = AuthVMFactory(ctx))

                LoginScreen(
                    viewModel = vm,
                    onLoginSuccess = {
                        navController.navigate("home") {
                            popUpTo("login") { inclusive = true }
                        }
                    },
                    onNavigateToSignUp = { navController.navigate("signup") },
                    onForgotPassword = { navController.navigate("forgot-password") }
                )
            }

            composable("signup") {
                val vm: AuthViewModel = viewModel(factory = AuthVMFactory(ctx))

                SignUpScreen(
                    viewModel = vm,
                    onSignUpSuccess = {
                        navController.navigate("home") {
                            popUpTo("signup") { inclusive = true }
                        }
                    },
                    onBackToLogin = { navController.navigate("login") }
                )
            }

            composable("forgot-password") {
                val vm: AuthViewModel = viewModel(factory = AuthVMFactory(ctx))
                ForgotPasswordScreen(vm) { navController.navigate("login") }
            }

            composable(
                "reset-password/{token}",
                arguments = listOf(navArgument("token") { type = NavType.StringType })
            ) {
                val vm: AuthViewModel = viewModel(factory = AuthVMFactory(ctx))

                ResetPasswordScreen(
                    viewModel = vm,
                    token = it.arguments?.getString("token") ?: "",
                    onResetSuccess = { navController.navigate("login") }
                )
            }

            /* ------------------- HOME ------------------- */

            composable("home") { entry ->

                val movieVM: MovieViewModel =
                    viewModel(factory = MovieVMFactory(ctx))

                val bookingVM =
                    entry.sharedBookingVM(navController, ctx)

                val tokenProvider = remember { AuthManager(ctx) }

                // Dispara a verificação
                LaunchedEffect(Unit) {
                    bookingVM.loadBookingsForReviewCheck()
                }

                // ✅ Observação correta do STATE
                val pendingId by bookingVM.pendingReviewBookingId

                LaunchedEffect(pendingId) {
                    pendingId?.let { bookingId ->
                        navController.navigate("create-review/$bookingId")
                        bookingVM.clearPendingReview()
                    }
                }

                Movie_List_Home_Screen(
                    viewModel = movieVM,
                    onMovieClick = { id ->
                        navController.navigate("movie/$id")
                    },
                    onNavigateToFullList = {
                        navController.navigate("moviesGraph/list")
                    }
                )
            }

            /* ------------------- MOVIE DETAILS ------------------- */

            composable(
                "movie/{id}",
                arguments = listOf(navArgument("id") { type = NavType.IntType })
            ) {

                val vm: MovieViewModel = viewModel(factory = MovieVMFactory(ctx))
                val movieId = it.arguments!!.getInt("id")

                LaunchedEffect(movieId) {
                    vm.loadMovieDetails(movieId)
                }

                MovieDetailsScreen(
                    viewModel = vm,
                    onBack = { navController.popBackStack() },
                    onWatchClick = {
                        vm.selectedMovie?.let { mov ->
                            navController.navigate("select-session/${mov.id}")
                        }
                    }
                )
            }

            /* ------------------- MOVIES GRAPH ------------------- */

            navigation(
                route = "moviesGraph",
                startDestination = "moviesGraph/list"
            ) {

                composable("moviesGraph/list") { entry ->

                    val vm = entry.sharedMovieVM(navController, ctx)

                    MovieListScreen(
                        viewModel = vm,
                        onMovieClick = { id ->
                            navController.navigate("movie/$id")
                        },
                        onOpenFilter = {
                            navController.navigate("moviesGraph/filters")
                        }
                    )
                }

                composable("moviesGraph/filters") { entry ->

                    val vm = entry.sharedMovieVM(navController, ctx)
                    val original = vm.originalMovies

                    MovieFilterScreen(
                        availableGenres = original.flatMap { it.genres }.distinct().sorted(),
                        availableCountries = original.map { it.country }.distinct().sorted(),
                        availableYears = original.mapNotNull { it.releaseDate?.take(4) }
                            .distinct().sorted(),

                        selectedGenres = vm.selectedGenres,
                        selectedCountries = vm.selectedCountries,
                        selectedYears = vm.selectedYears,

                        onApplyFilters = {
                            vm.applyFilters()
                            navController.popBackStack()
                        },
                        onBack = {
                            navController.popBackStack()
                        }
                    )
                }
            }

            /* ------------------- SESSIONS ------------------- */

            composable(
                "select-session/{movieId}",
                arguments = listOf(navArgument("movieId") { type = NavType.IntType })
            ) {

                val movieId = it.arguments?.getInt("movieId") ?: 0
                val context by rememberUpdatedState(LocalContext.current)

                val vmFactory = remember { SessionVMFactory(context) }
                val vm: SessionViewModel = viewModel(factory = vmFactory)

                SessionAccordionScreen(
                    movieId = movieId,
                    viewModel = vm,
                    onBack = { navController.popBackStack() },
                    onSessionClick = { sessionId ->
                        navController.navigate("seat-selection/$sessionId")
                    }
                )
            }

            /* ------------------- SEAT SELECTION ------------------- */

            composable(
                "seat-selection/{sessionId}",
                arguments = listOf(navArgument("sessionId") { type = NavType.IntType })
            ) {

                val sessionId = it.arguments?.getInt("sessionId") ?: 0
                val context by rememberUpdatedState(LocalContext.current)

                val vmFactory = remember { SeatSelectionVMFactory(context) }
                val vm: SeatSelectionViewModel = viewModel(factory = vmFactory)

                SeatSelectionRoute(
                    sessionId = sessionId,
                    viewModel = vm,
                    onBack = { navController.popBackStack() },
                    onConfirm = { seatIds ->

                        val seatsEncoded = seatIds.joinToString("-")

                        navController.navigate(
                            "booking-summary/$sessionId/$seatsEncoded"
                        )
                    }
                )
            }

            /* ------------------- BOOKING SUMMARY ------------------- */

            composable(
                "booking-summary/{sessionId}/{seatIds}",
                arguments = listOf(
                    navArgument("sessionId") { type = NavType.IntType },
                    navArgument("seatIds") { type = NavType.StringType }
                )
            ) { entry ->

                val sessionId = entry.arguments?.getInt("sessionId") ?: 0
                val seatsString = entry.arguments?.getString("seatIds") ?: ""
                val seatIds = seatsString.split("-").mapNotNull { s -> s.toIntOrNull() }

                val bookingVM: BookingViewModel =
                    entry.sharedBookingVM(navController, ctx)

                val voucherVM: VoucherViewModel =
                    viewModel(factory = VoucherVMFactory(ctx))

                LaunchedEffect(sessionId, seatsString) {
                    bookingVM.loadSessionForSummary(sessionId)
                    bookingVM.setSeatsForSummary(seatIds, sessionId)
                }

                BookingSummaryScreen(
                    bookingViewModel = bookingVM,
                    voucherViewModel = voucherVM,
                    sessionId = sessionId,
                    seatIds = seatIds,
                    onProceedPayment = { bookingId, voucherId, userId ->

                        navController.navigate(
                            "payment/$bookingId?voucherId=${voucherId ?: 0}&userId=$userId"
                        )
                    },
                    onBack = { navController.popBackStack() }
                )
            }

            /* ------------------- CINEMAS ------------------- */

            composable("cinemas") {
                val context = LocalContext.current

                val cinemaVM: CinemaViewModel = viewModel(
                    factory = CinemaVMFactory(context)
                )
                val locationVM: LocationViewModel = viewModel()

                CinemaListScreen(
                    context = context,
                    viewModel = cinemaVM,
                    locationVM = locationVM,
                    onCinemaClick = { cinemaId ->
                        navController.navigate("cinema/$cinemaId")
                    }
                )
            }

            /* ------------------- PAYMENT ------------------- */

            composable(
                route = "payment/{bookingId}?voucherId={voucherId}&userId={userId}",
                arguments = listOf(
                    navArgument("bookingId") { type = NavType.IntType },
                    navArgument("voucherId") {
                        defaultValue = 0
                        type = NavType.IntType
                    },
                    navArgument("userId") { type = NavType.IntType }
                )
            ) {

                val bookingId = it.arguments!!.getInt("bookingId")
                val voucherId = it.arguments!!.getInt("voucherId").let { vid ->
                    if (vid == 0) null else vid
                }

                val paymentVM: PaymentViewModel =
                    viewModel(factory = PaymentVMFactory(ctx))

                PaymentScreen(
                    bookingId = bookingId,
                    voucherId = voucherId,
                    viewModel = paymentVM,
                    onSuccessNavigate = {
                        navController.navigate("home")
                    }
                )
            }

            /* ------------------- CREATE REVIEW ------------------- */

            composable(
                "create-review/{bookingId}",
                arguments = listOf(
                    navArgument("bookingId") { type = NavType.IntType }
                )
            ) {

                val bookingId = it.arguments!!.getInt("bookingId")

                val reviewVM: ReviewViewModel =
                    viewModel(factory = ReviewVMFactory(ctx))

                ReviewScreen(
                    bookingId = bookingId,
                    viewModel = reviewVM,
                    onSuccessNavigateBack = {
                        navController.popBackStack()
                    }
                )
            }

            composable("profile") { }
        }
    }
}
