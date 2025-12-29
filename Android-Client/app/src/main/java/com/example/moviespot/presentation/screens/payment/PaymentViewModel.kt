package com.example.moviespot.presentation.screens.payment

import androidx.compose.runtime.mutableStateOf
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.example.moviespot.data.repository.PaymentRepository
import kotlinx.coroutines.launch

class PaymentViewModel(
    private val repo: PaymentRepository
) : ViewModel() {

    var isLoading = mutableStateOf(false)
    var error = mutableStateOf<String?>(null)

    fun startPayment(
        bookingId: Int,
        voucherId: Int?,
        onClientSecret: (String) -> Unit
    ) {
        isLoading.value = true
        error.value = null

        viewModelScope.launch {
            try {
                val secret = repo.createPaymentIntent(bookingId, voucherId)
                onClientSecret(secret)
            } catch (e: Exception) {
                error.value = e.message ?: "Erro ao iniciar pagamento"
            } finally {
                isLoading.value = false
            }
        }
    }

    fun confirmStripePayment(
        paymentIntentId: String,
        onSuccess: () -> Unit
    ) {
        isLoading.value = true
        error.value = null

        viewModelScope.launch {
            try {
                val status = repo.confirmPayment(paymentIntentId)

                if (status == "Paid") {
                    onSuccess()
                } else {
                    error.value = "Pagamento não concluído: $status"
                }

            } catch (e: Exception) {
                error.value = e.message ?: "Erro ao confirmar pagamento"
            } finally {
                isLoading.value = false
            }
        }
    }
}
