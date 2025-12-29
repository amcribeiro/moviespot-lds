package com.example.moviespot.presentation.screens.auth.signup

import androidx.compose.foundation.Image
import androidx.compose.foundation.background
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.*
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.res.painterResource
import androidx.compose.ui.text.TextStyle
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.text.input.PasswordVisualTransformation
import androidx.compose.ui.text.input.VisualTransformation
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import com.example.moviespot.R
import com.example.moviespot.presentation.screens.auth.AuthScreenState
import com.example.moviespot.presentation.screens.auth.AuthViewModel

@Composable
fun SignUpScreen(
    viewModel: AuthViewModel,
    onSignUpSuccess: () -> Unit,
    onBackToLogin: () -> Unit
) {
    LaunchedEffect(viewModel.state) {
        if (viewModel.state is AuthScreenState.Success) {
            viewModel.clearState()
            onSignUpSuccess()
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

            Image(
                painter = painterResource(id = R.drawable.moviespot_logo),
                contentDescription = "MovieSpot Logo",
                modifier = Modifier.size(180.dp)
            )

            Spacer(modifier = Modifier.height(40.dp))

            OutlinedTextField(
                value = viewModel.name,
                onValueChange = viewModel::onNameChanged,
                label = { Text("Name", color = Color.White) },
                leadingIcon = { Icon(Icons.Default.Person, null, tint = Color.White) },
                textStyle = TextStyle(color = Color.White),
                modifier = Modifier.fillMaxWidth()
            )

            Spacer(modifier = Modifier.height(12.dp))

            OutlinedTextField(
                value = viewModel.email,
                onValueChange = viewModel::onEmailChanged,
                label = { Text("Email", color = Color.White) },
                leadingIcon = { Icon(Icons.Default.Email, null, tint = Color.White) },
                textStyle = TextStyle(color = Color.White),
                modifier = Modifier.fillMaxWidth()
            )

            Spacer(modifier = Modifier.height(12.dp))

            OutlinedTextField(
                value = viewModel.password,
                onValueChange = viewModel::onPasswordChanged,
                label = { Text("Password", color = Color.White) },
                leadingIcon = { Icon(Icons.Default.Lock, null, tint = Color.White) },
                trailingIcon = {
                    val icon = if (viewModel.passwordVisible) Icons.Filled.Visibility else Icons.Filled.VisibilityOff
                    IconButton(onClick = { viewModel.togglePasswordVisibility() }) {
                        Icon(icon, null, tint = Color.White)
                    }
                },
                visualTransformation =
                    if (viewModel.passwordVisible) VisualTransformation.None else PasswordVisualTransformation(),
                textStyle = TextStyle(color = Color.White),
                modifier = Modifier.fillMaxWidth()
            )

            Spacer(modifier = Modifier.height(12.dp))

            OutlinedTextField(
                value = viewModel.phone,
                onValueChange = viewModel::onPhoneChanged,
                label = { Text("Phone", color = Color.White) },
                leadingIcon = { Icon(Icons.Default.Phone, null, tint = Color.White) },
                keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Phone),
                textStyle = TextStyle(color = Color.White),
                modifier = Modifier.fillMaxWidth()
            )

            Spacer(modifier = Modifier.height(24.dp))

            Button(
                onClick = { viewModel.signup() },
                enabled = viewModel.state !is AuthScreenState.Loading,
                colors = ButtonDefaults.buttonColors(containerColor = Color(0xFFD32F2F)),
                modifier = Modifier.fillMaxWidth().height(50.dp)
            ) {
                if (viewModel.state is AuthScreenState.Loading)
                    CircularProgressIndicator(Modifier.size(20.dp), color = Color.White)
                else
                    Text("Create Account", color = Color.White, fontSize = 16.sp)
            }

            Spacer(modifier = Modifier.height(16.dp))

            TextButton(onClick = onBackToLogin) {
                Text("Already have an account?", color = Color.White, fontSize = 14.sp)
            }
        }
    }
}
