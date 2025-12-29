package com.example.moviespot.presentation.screens.session.components

import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.*
import androidx.compose.material3.Text
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import com.example.moviespot.data.dto.SessionResponseDto
import androidx.compose.runtime.Composable

@Composable
fun SessionItem(session: SessionResponseDto, onSessionClick: (Int) -> Unit) {
    Row(
        Modifier
            .fillMaxWidth()
            .clickable { onSessionClick(session.id) }
            .padding(vertical = 6.dp),
        horizontalArrangement = Arrangement.SpaceBetween
    ) {
        Text(session.startDate.substring(11, 16), color = Color.White, fontSize = 15.sp)
        Text("${session.price}€", color = Color(0xFF81C784), fontSize = 15.sp)
    }
}
