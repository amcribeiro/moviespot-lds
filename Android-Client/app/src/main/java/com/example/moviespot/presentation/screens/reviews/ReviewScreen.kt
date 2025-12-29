package com.example.moviespot.presentation.screens.reviews

import androidx.compose.foundation.background
import androidx.compose.foundation.layout.*
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.unit.dp

@Composable
fun ReviewScreen(
    modifier: Modifier = Modifier,
    bookingId: Int,
    viewModel: ReviewViewModel,
    onSuccessNavigateBack: () -> Unit
) {

    LaunchedEffect(viewModel.submitSuccess.value) {
        if (viewModel.submitSuccess.value) {
            onSuccessNavigateBack()
        }
    }

    Box(
        modifier = modifier
            .fillMaxSize()
            .background(Color(0xFF121212))
            .padding(16.dp)
    ) {

        Column(
            modifier = Modifier
                .fillMaxWidth()
                .align(Alignment.Center),
            verticalArrangement = Arrangement.spacedBy(14.dp)
        ) {

            Text(
                text = "Avaliar sessão",
                style = MaterialTheme.typography.titleLarge,
                color = Color.White
            )

            // -----------------------------------------
            // RATING 1..5
            // -----------------------------------------

            Text(
                text = "Classificação",
                color = Color.LightGray
            )

            Row(
                horizontalArrangement = Arrangement.spacedBy(6.dp)
            ) {

                (1..5).forEach { star ->

                    Button(
                        onClick = { viewModel.rating.intValue = star },
                        colors = ButtonDefaults.buttonColors(
                            containerColor =
                                if (viewModel.rating.intValue >= star)
                                    Color(0xFFFFC107)
                                else
                                    Color(0xFF2A2A2A)
                        )
                    ) {
                        Text("★")
                    }
                }
            }

            // -----------------------------------------
            // COMMENT
            // -----------------------------------------

            OutlinedTextField(
                value = viewModel.comment.value,
                onValueChange = { viewModel.comment.value = it },
                placeholder = {
                    Text("Escreve o teu comentário...")
                },
                modifier = Modifier.fillMaxWidth(),
                maxLines = 4
            )

            // -----------------------------------------
            // ERROR
            // -----------------------------------------

            viewModel.submitError.value?.let {
                Text(
                    text = it,
                    color = Color.Red
                )
            }

            // -----------------------------------------
            // SUBMIT
            // -----------------------------------------

            Button(
                modifier = Modifier.fillMaxWidth(),
                enabled = !viewModel.isSubmitting.value,
                onClick = {
                    viewModel.submitReview(bookingId)
                }
            ) {

                if (viewModel.isSubmitting.value) {
                    CircularProgressIndicator(
                        modifier = Modifier.size(18.dp),
                        color = Color.White,
                        strokeWidth = 2.dp
                    )
                } else {
                    Text("Enviar avaliação")
                }
            }
        }
    }
}
