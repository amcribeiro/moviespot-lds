package com.example.moviespot.presentation.screens.movie.movie_list

import androidx.compose.foundation.background
import androidx.compose.foundation.border
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.verticalScroll
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.ArrowBack
import androidx.compose.material3.*
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp

@Composable
fun MovieFilterScreen(
    availableGenres: List<String>,
    availableCountries: List<String>,
    availableYears: List<String>,
    selectedGenres: MutableList<String>,
    selectedCountries: MutableList<String>,
    selectedYears: MutableList<String>,
    onApplyFilters: () -> Unit,
    onBack: () -> Unit
) {

    Column(
        modifier = Modifier
            .fillMaxSize()
            .background(Color(0xFF0F0F0F))
            .verticalScroll(rememberScrollState())
            .padding(16.dp)
    ) {

        Row(verticalAlignment = Alignment.CenterVertically) {
            IconButton(onClick = onBack) {
                Icon(
                    Icons.AutoMirrored.Filled.ArrowBack,
                    contentDescription = null,
                    tint = Color.White
                )
            }
            Text("Filter", color = Color.White, fontSize = 22.sp, fontWeight = FontWeight.Bold)
        }

        Spacer(Modifier.height(20.dp))

        Text("Genres", color = Color.White, fontWeight = FontWeight.Bold, fontSize = 18.sp)
        Spacer(Modifier.height(8.dp))

        FlowRow(horizontalArrangement = Arrangement.spacedBy(10.dp)) {
            availableGenres.forEach { g ->
                FilterChip(
                    label = g,
                    isSelected = selectedGenres.contains(g),
                    onClick = {
                        if (selectedGenres.contains(g)) selectedGenres.remove(g)
                        else selectedGenres.add(g)
                    }
                )
            }
        }

        Spacer(Modifier.height(20.dp))

        Text("Countries", color = Color.White, fontWeight = FontWeight.Bold, fontSize = 18.sp)
        Spacer(Modifier.height(8.dp))

        FlowRow(horizontalArrangement = Arrangement.spacedBy(10.dp)) {
            availableCountries.forEach { c ->
                FilterChip(
                    label = c,
                    isSelected = selectedCountries.contains(c),
                    onClick = {
                        if (selectedCountries.contains(c)) selectedCountries.remove(c)
                        else selectedCountries.add(c)
                    }
                )
            }
        }

        Spacer(Modifier.height(20.dp))

        Text("Release Year", color = Color.White, fontWeight = FontWeight.Bold, fontSize = 18.sp)
        Spacer(Modifier.height(8.dp))

        FlowRow(horizontalArrangement = Arrangement.spacedBy(10.dp)) {
            availableYears.forEach { y ->
                FilterChip(
                    label = y,
                    isSelected = selectedYears.contains(y),
                    onClick = {
                        selectedYears.clear()
                        selectedYears.add(y)
                    }
                )
            }
        }

        Spacer(Modifier.height(30.dp))

        Row(
            modifier = Modifier.fillMaxWidth(),
            horizontalArrangement = Arrangement.SpaceBetween
        ) {

            Button(
                modifier = Modifier.weight(1f),
                colors = ButtonDefaults.buttonColors(Color(0xFFFFEB3B)),
                onClick = onApplyFilters
            ) {
                Text("Submit", color = Color.Black)
            }

            Spacer(Modifier.width(12.dp))

            Button(
                modifier = Modifier.weight(1f),
                colors = ButtonDefaults.buttonColors(Color(0xFF222222)),
                onClick = {
                    selectedGenres.clear()
                    selectedCountries.clear()
                    selectedYears.clear()
                }
            ) {
                Text("Reset", color = Color.White)
            }
        }
    }
}

@Composable
fun FilterChip(label: String, isSelected: Boolean, onClick: () -> Unit) {
    Surface(
        modifier = Modifier
            .border(
                width = 1.dp,
                color = if (isSelected) Color(0xFFFFEB3B) else Color.Gray,
                shape = MaterialTheme.shapes.small
            )
            .clickable(onClick = onClick)
            .padding(horizontal = 12.dp, vertical = 6.dp),
        color = Color.Transparent
    ) {
        Text(
            label,
            color = if (isSelected) Color(0xFFFFEB3B) else Color.LightGray,
            fontSize = 14.sp
        )
    }
}
