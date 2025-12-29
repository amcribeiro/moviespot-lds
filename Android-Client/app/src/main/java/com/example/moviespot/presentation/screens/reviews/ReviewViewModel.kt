package com.example.moviespot.presentation.screens.reviews

import androidx.compose.runtime.mutableIntStateOf
import androidx.compose.runtime.mutableStateOf
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.example.moviespot.data.dto.ReviewCreateDto
import com.example.moviespot.data.repository.ReviewRepository
import kotlinx.coroutines.launch

class ReviewViewModel(
    private val repository: ReviewRepository
) : ViewModel() {

    // =====================================================
    // FORM STATE
    // =====================================================

    val rating = mutableIntStateOf(0)
    val comment = mutableStateOf("")

    // =====================================================
    // SUBMIT STATE
    // =====================================================

    val isSubmitting = mutableStateOf(false)
    val submitError = mutableStateOf<String?>(null)
    val submitSuccess = mutableStateOf(false)

    // =====================================================
    // ACTION
    // =====================================================

    fun submitReview(bookingId: Int) {
        if (rating.intValue !in 1..5) {
            submitError.value = "Escolhe uma classificação entre 1 e 5."
            return
        }

        viewModelScope.launch {
            try {
                isSubmitting.value = true
                submitError.value = null
                submitSuccess.value = false

                repository.createReview(
                    ReviewCreateDto(
                        bookingId = bookingId,
                        rating = rating.intValue,
                        comment = comment.value.ifBlank { null }
                    )
                )

                submitSuccess.value = true
            }
            catch (ex: Exception){
                submitError.value = ex.message ?: "Erro ao enviar a review."
            }
            finally {
                isSubmitting.value = false
            }
        }
    }
}
