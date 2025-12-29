package com.example.moviespot

import android.os.Build
import android.os.Bundle
import android.util.Log
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.core.splashscreen.SplashScreen.Companion.installSplashScreen
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import com.example.moviespot.presentation.navigation.AppNavigation
import com.example.moviespot.ui.theme.MovieSpotTheme
import com.stripe.android.PaymentConfiguration

class MainActivity : ComponentActivity() {

    override fun onCreate(savedInstanceState: Bundle?) {
        installSplashScreen()
        super.onCreate(savedInstanceState)


        PaymentConfiguration.init(
            applicationContext,
            BuildConfig.STRIPE_PUBLISHABLE_KEY
        )

        Log.d("STRIPE", "KEY = ${BuildConfig.STRIPE_PUBLISHABLE_KEY}")

        setContent {
            MovieSpotTheme {
                if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
                    AppNavigation()
                } else {
                    LegacyNavigationFallback()
                }
            }
        }
    }
}

@Composable
private fun LegacyNavigationFallback() {
    Text("Android 8.0 ou superior é necessário.")
}
