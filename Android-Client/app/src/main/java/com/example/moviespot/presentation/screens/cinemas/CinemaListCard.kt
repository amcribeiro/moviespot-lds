package com.example.moviespot.presentation.screens.cinemas

import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.*
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import com.example.moviespot.data.dto.CinemaResponseDto

@Composable
fun CinemaListCard(
    cinema: CinemaResponseDto,
    distance: String?,
    onOpenMaps: () -> Unit,
    onClick: () -> Unit
) {
    Card(
        modifier = Modifier
            .fillMaxWidth()
            .padding(horizontal = 14.dp, vertical = 8.dp)
            .clickable { onClick() },
        colors = CardDefaults.cardColors(containerColor = Color(0xFF1E1E1E)),
        shape = RoundedCornerShape(12.dp)
    ) {
        Column(Modifier.padding(14.dp)) {
            Text(cinema.name, color = Color.White, fontSize = 18.sp)
            Text(cinema.city, color = Color(0xFFB0B0B0), fontSize = 14.sp)
            if (distance != null) {
                Text(distance, color = Color(0xFF80CBC4), fontSize = 14.sp)
            }

            Spacer(Modifier.height(6.dp))
            AssistChip(
                onClick = onOpenMaps,
                label = { Text("Ver no Maps", color = Color.Black) },
                leadingIcon = { Text("📍") },
                colors = AssistChipDefaults.assistChipColors(containerColor = Color(0xFFFFEB3B)),
                modifier = Modifier.align(Alignment.End)
            )
        }
    }
}
