package com.example.moviespot.presentation.screens.session.components

import androidx.compose.runtime.*
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.background
import androidx.compose.foundation.clickable
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.ExpandLess
import androidx.compose.material.icons.filled.ExpandMore
import androidx.compose.material3.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import com.example.moviespot.data.dto.SessionResponseDto

@Composable
fun HallAccordion(
    hallName: String,
    sessions: List<SessionResponseDto>,
    onSessionClick: (Int) -> Unit
) {
    var expanded by remember { mutableStateOf(false) }

    Column(
        Modifier
            .fillMaxWidth()
            .padding(top = 4.dp)
            .background(Color(0xFF2A2A2A), RoundedCornerShape(8.dp))
    ) {
        Row(
            Modifier
                .fillMaxWidth()
                .clickable { expanded = !expanded }
                .padding(horizontal = 12.dp, vertical = 10.dp),
            horizontalArrangement = Arrangement.SpaceBetween,
            verticalAlignment = Alignment.CenterVertically
        ) {
            Text(hallName, color = Color.Cyan, fontSize = 16.sp)
            Icon(
                if (expanded) Icons.Default.ExpandLess else Icons.Default.ExpandMore,
                contentDescription = "",
                tint = Color.White
            )
        }

        if (expanded) {
            Column(Modifier.padding(bottom = 6.dp)) {
                sessions.forEach { s -> SessionItem(session = s, onSessionClick = onSessionClick) }
            }
        }
    }
}
