package com.example.moviespot.presentation.screens.settings

import androidx.compose.foundation.layout.*
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import com.google.firebase.messaging.FirebaseMessaging

@Composable
fun NotificationSettingsScreen() {

    var sessionNotif by remember { mutableStateOf(false) }
    var promotionNotif by remember { mutableStateOf(false) }

    Column(Modifier.padding(20.dp)) {

        Text("Notificações MovieSpot", style = MaterialTheme.typography.headlineSmall)

        Spacer(Modifier.height(20.dp))

        Row(Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.SpaceBetween) {
            Text("Sessões (Notificar sessões de amanhã)")
            Switch(
                checked = sessionNotif,
                onCheckedChange = {
                    sessionNotif = it
                    if (it) FirebaseMessaging.getInstance().subscribeToTopic("sessions")
                    else FirebaseMessaging.getInstance().unsubscribeFromTopic("sessions")
                }
            )
        }

        Spacer(Modifier.height(20.dp))

        Row(Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.SpaceBetween) {
            Text("Promoções (novas sessões com desconto)")
            Switch(
                checked = promotionNotif,
                onCheckedChange = {
                    promotionNotif = it
                    if (it) FirebaseMessaging.getInstance().subscribeToTopic("promotions")
                    else FirebaseMessaging.getInstance().unsubscribeFromTopic("promotions")
                }
            )
        }
    }
}
