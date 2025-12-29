package com.example.moviespot.presentation.screens.payment

import androidx.compose.foundation.layout.*
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import com.stripe.android.paymentsheet.PaymentSheet
import com.stripe.android.paymentsheet.PaymentSheetResult
import com.stripe.android.paymentsheet.rememberPaymentSheet

@Composable
fun PaymentScreen(
    bookingId: Int,
    voucherId: Int?,
    viewModel: PaymentViewModel,
    onSuccessNavigate: () -> Unit
) {

    val lastClientSecret = remember { mutableStateOf<String?>(null) }

    val paymentSheet = rememberPaymentSheet { result ->
        when (result) {

            is PaymentSheetResult.Completed -> {

                val secret = lastClientSecret.value ?: run {
                    viewModel.error.value = "Erro interno (client secret em falta)."
                    return@rememberPaymentSheet
                }

                val paymentIntentId = secret.substringBefore("_secret")

                viewModel.confirmStripePayment(
                    paymentIntentId = paymentIntentId
                ) {
                    onSuccessNavigate()
                }
            }

            is PaymentSheetResult.Failed -> {
                viewModel.error.value =
                    result.error.message ?: "Erro no pagamento"
            }

            is PaymentSheetResult.Canceled -> {
                viewModel.error.value = "Pagamento cancelado"
            }
        }
    }

    Column(
        Modifier
            .fillMaxSize()
            .padding(24.dp),
        verticalArrangement = Arrangement.Center,
        horizontalAlignment = Alignment.CenterHorizontally
    ) {

        viewModel.error.value?.let {
            Text(it, color = MaterialTheme.colorScheme.error)
            Spacer(Modifier.height(8.dp))
        }

        Button(
            enabled = !viewModel.isLoading.value,
            onClick = {
                viewModel.startPayment(
                    bookingId = bookingId,
                    voucherId = voucherId
                ) { clientSecret ->

                    lastClientSecret.value = clientSecret

                    paymentSheet.presentWithPaymentIntent(
                        clientSecret,
                        PaymentSheet.Configuration(
                            merchantDisplayName = "MovieSpot"
                        )
                    )
                }
            }
        ) {
            if (viewModel.isLoading.value)
                CircularProgressIndicator(Modifier.size(18.dp))
            else
                Text("Pagar")
        }
    }
}
