package com.example.moviespot.presentation.screens.auth.reset_password

import androidx.compose.foundation.background
import androidx.compose.foundation.layout.*
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Lock
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.text.TextStyle
import androidx.compose.ui.text.input.PasswordVisualTransformation
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import com.example.moviespot.presentation.screens.auth.AuthScreenState
import com.example.moviespot.presentation.screens.auth.AuthViewModel

@Composable
fun ResetPasswordScreen(
    viewModel: AuthViewModel,
    token: String,
    onResetSuccess: () -> Unit
) {
    LaunchedEffect(token) {
        viewModel.resetToken = token
    }

    LaunchedEffect(viewModel.state) {
        if (viewModel.state is AuthScreenState.Success) {
            viewModel.clearState()
            onResetSuccess()
        }
    }

    Box(
        modifier = Modifier
            .fillMaxSize()
            .background(Color(0xFF121212))
            .padding(24.dp),
        contentAlignment = Alignment.Center
    ) {
        Column(horizontalAlignment = Alignment.CenterHorizontally) {

            if (viewModel.state is AuthScreenState.Error) {
                Text(
                    (viewModel.state as AuthScreenState.Error).message,
                    color = Color.Red,
                    modifier = Modifier.padding(bottom = 8.dp)
                )
            }

            Text("Reset Password", color = Color.White, fontSize = 24.sp)
            Spacer(modifier = Modifier.height(20.dp))

            OutlinedTextField(
                value = viewModel.password,
                onValueChange = viewModel::onPasswordChanged,
                label = { Text("New Password", color = Color.White) },
                leadingIcon = { Icon(Icons.Default.Lock, null, tint = Color.White) },
                textStyle = TextStyle(color = Color.White),
                visualTransformation = PasswordVisualTransformation(),
                modifier = Modifier.fillMaxWidth()
            )

            Spacer(modifier = Modifier.height(24.dp))

            Button(
                onClick = { viewModel.resetPassword() },
                enabled = viewModel.state !is AuthScreenState.Loading,
                colors = ButtonDefaults.buttonColors(containerColor = Color(0xFFD32F2F)),
                modifier = Modifier.fillMaxWidth().height(50.dp)
            ) {
                if (viewModel.state is AuthScreenState.Loading)
                    CircularProgressIndicator(Modifier.size(20.dp), color = Color.White)
                else
                    Text("Reset Password", color = Color.White, fontSize = 16.sp)
            }
        }
    }
}

