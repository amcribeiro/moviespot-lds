package com.example.moviespot.presentation.screens.session.components

import androidx.compose.runtime.*
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.clickable
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.ExpandLess
import androidx.compose.material.icons.filled.ExpandMore
import androidx.compose.material3.*
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import com.example.moviespot.data.dto.CinemaHallReadDto
import com.example.moviespot.data.dto.SessionResponseDto

@Composable
fun DayAccordion(
    day: String,
    halls: List<CinemaHallReadDto>,
    sessionsByHall: Map<Int, List<SessionResponseDto>>,
    onSessionClick: (Int) -> Unit
) {
    var expanded by remember { mutableStateOf(false) }

    val filteredByDay = sessionsByHall.mapValues { (_, list) ->
        list.filter { it.startDate.startsWith(day) }
    }.filterValues { it.isNotEmpty() }

    if (filteredByDay.isEmpty()) return

    Card(
        modifier = Modifier
            .fillMaxWidth()
            .padding(top = 8.dp)
            .clickable { expanded = !expanded },
        colors = CardDefaults.cardColors(containerColor = Color(0xFF181818)),
        shape = RoundedCornerShape(10.dp)
    ) {
        Column(Modifier.padding(10.dp)) {

            Row(Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.SpaceBetween) {
                Text(day, color = Color.White, fontSize = 16.sp)
                Icon(
                    if (expanded) Icons.Default.ExpandLess else Icons.Default.ExpandMore,
                    contentDescription = "",
                    tint = Color.White
                )
            }

            if (expanded) {
                Spacer(Modifier.height(6.dp))
                filteredByDay.forEach { (hallId, sessions) ->
                    val hall = halls.firstOrNull { it.id == hallId }
                    HallAccordion(
                        hallName = hall?.name ?: "Sala",
                        sessions = sessions,
                        onSessionClick = onSessionClick
                    )
                }
            }
        }
    }
}
