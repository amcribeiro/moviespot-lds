package com.example.moviespot.presentation.screens.booking

import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.setValue
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.example.moviespot.data.dto.VoucherResponseDto
import com.example.moviespot.data.repository.VoucherRepository
import kotlinx.coroutines.launch

class VoucherViewModel(
    private val repository: VoucherRepository
) : ViewModel() {

    var voucher: VoucherResponseDto? by mutableStateOf(null)
        private set

    var discountedTotal: Double? by mutableStateOf(null)
        private set

    var error: String? by mutableStateOf(null)
        private set

    var isLoading by mutableStateOf(false)
        private set


    /**
     * ✅ Aplica voucher usando código de validação no backend.
     * Estado completo + tratamento correto de erros HTTP.
     */
    fun applyVoucher(voucherCode: String, originalTotal: Double) {

        if (voucherCode.isBlank()) {
            error = "Insere um código válido."
            return
        }

        isLoading = true
        error = null

        viewModelScope.launch {

            try {

                val result = repository.getByCode(voucherCode.trim())

                error = null
                voucher = result

                discountedTotal = calculateDiscount(
                    originalTotal,
                    result.value
                )

            }
            catch (e: retrofit2.HttpException) {

                error = when (e.code()) {
                    404 -> "Voucher não encontrado."
                    400 -> "Voucher inválido, expirado ou sem usos."
                    401 -> "Sessão expirada. Faz login novamente."
                    else -> "Erro no servidor (${e.code()})."
                }

                voucher = null
                discountedTotal = null
            }
            catch (e: Exception) {

                val isNetworkError =
                    e is java.io.IOException ||
                            e is kotlinx.serialization.SerializationException ||
                            e.cause is java.io.EOFException

                error =
                    if (isNetworkError)
                        "Sem ligação à internet."
                    else
                        e.message ?: "Erro inesperado ao validar voucher."

                voucher = null
                discountedTotal = null
            }
            finally {
                isLoading = false
            }
        }
    }


    fun removeVoucher() {
        voucher = null
        discountedTotal = null
        error = null
    }


    private fun calculateDiscount(original: Double, discountPercent: Double): Double {

        val discountValue = original * discountPercent

        val finalValue = original - discountValue

        return if (finalValue < 0) 0.0 else finalValue
    }
}

