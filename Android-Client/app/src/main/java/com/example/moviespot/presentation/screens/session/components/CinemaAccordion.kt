package com.example.moviespot.presentation.screens.sessions.components

import androidx.compose.animation.AnimatedVisibility
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.ExpandLess
import androidx.compose.material.icons.filled.ExpandMore
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import com.example.moviespot.presentation.screens.session.AccordionCinemaData
import com.example.moviespot.presentation.screens.session.components.DayAccordion

@Composable
fun CinemaAccordion(
    cinemaData: AccordionCinemaData,
    distanceText: String?,
    onOpenMaps: () -> Unit,
    onSessionClick: (sessionId: Int) -> Unit
) {
    var expanded by remember { mutableStateOf(false) }
    val c = cinemaData.cinema

    Card(
        modifier = Modifier
            .fillMaxWidth()
            .padding(vertical = 8.dp)
            .clickable { expanded = !expanded },
        colors = CardDefaults.cardColors(containerColor = Color(0xFF1E1E1E)),
        shape = RoundedCornerShape(12.dp)
    ) {
        Column(Modifier.padding(12.dp)) {

            Row(Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.SpaceBetween) {
                Column {
                    Text(c.name, color = Color.White, fontSize = 18.sp)
                    Text(c.city, color = Color(0xFFB0B0B0), fontSize = 14.sp)
                    if (distanceText != null)
                        Text(distanceText, color = Color(0xFF80CBC4), fontSize = 13.sp)
                }
                Icon(
                    imageVector = if (expanded) Icons.Default.ExpandLess else Icons.Default.ExpandMore,
                    contentDescription = null,
                    tint = Color.White
                )
            }


            AssistChip(
                onClick = onOpenMaps,
                label = { Text("Ver no Maps", color = Color.Black) },
                leadingIcon = { Text("📍") },
                colors = AssistChipDefaults.assistChipColors(containerColor = Color(0xFFFFEB3B)),
                modifier = Modifier
                    .align(Alignment.End)
                    .padding(top = 6.dp)
            )


            AnimatedVisibility(visible = expanded) {
                Column(Modifier.padding(top = 8.dp)) {
                    val groupedByDay = cinemaData.sessionsByHall.values.flatten()
                        .groupBy { it.startDate.take(10) }

                    groupedByDay.forEach { (day, _) ->
                        DayAccordion(
                            day = day,
                            halls = cinemaData.halls,
                            sessionsByHall = cinemaData.sessionsByHall,
                            onSessionClick = onSessionClick
                        )
                    }
                }
            }
        }
    }
}
