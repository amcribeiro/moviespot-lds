package com.example.moviespot.presentation.navigation

import androidx.compose.foundation.layout.height
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Airplay
import androidx.compose.material.icons.filled.Movie
import androidx.compose.material.icons.filled.Person
import androidx.compose.material3.Icon
import androidx.compose.material3.NavigationBar
import androidx.compose.material3.NavigationBarItem
import androidx.compose.material3.NavigationBarItemDefaults
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.unit.dp
import androidx.navigation.NavHostController
import androidx.navigation.compose.currentBackStackEntryAsState

@Composable
fun BottomNavBar(navController: NavHostController) {

    val currentRoute = navController.currentBackStackEntryAsState().value?.destination?.route

    val hiddenRoutes = listOf("welcome", "login", "signup", "forgot-password", "reset-password")
    if (currentRoute in hiddenRoutes) return

    NavigationBar(
        modifier = Modifier.height(120.dp),
        containerColor = Color(0xFF000000)
    ) {

        @Composable
        fun itemColors() = NavigationBarItemDefaults.colors(
            selectedIconColor = Color(0xFFFF3B3B),
            selectedTextColor = Color(0xFFFF3B3B),
            unselectedIconColor = Color(0xFFFF3B3B).copy(alpha = 0.6f),
            unselectedTextColor = Color(0xFFFF3B3B).copy(alpha = 0.6f),
            indicatorColor = Color.Transparent
        )

        NavigationBarItem(
            selected = currentRoute == "home",
            onClick = { navController.navigate("home") },
            icon = { Icon(Icons.Default.Movie, contentDescription = "Filmes") },
            label = { Text("Filmes") },
            colors = itemColors()
        )

        NavigationBarItem(
            selected = currentRoute == "cinemas",
            onClick = { navController.navigate("cinemas") },
            icon = { Icon(Icons.Default.Airplay, contentDescription = "Cinemas") },
            label = { Text("Cinemas") },
            colors = itemColors()
        )

        NavigationBarItem(
            selected = currentRoute == "profile",
            onClick = { navController.navigate("profile") },
            icon = { Icon(Icons.Default.Person, contentDescription = "Perfil") },
            label = { Text("Perfil") },
            colors = itemColors()
        )
    }
}