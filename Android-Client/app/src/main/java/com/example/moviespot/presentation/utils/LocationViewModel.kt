package com.example.moviespot.presentation.utils

import android.app.Activity
import android.content.Context
import android.location.Location
import com.google.android.gms.location.LocationServices
import com.google.android.gms.location.LocationRequest
import com.google.android.gms.location.LocationSettingsRequest
import com.google.android.gms.location.Priority
import com.google.android.gms.common.api.ResolvableApiException
import androidx.lifecycle.ViewModel
import kotlinx.coroutines.flow.MutableStateFlow

class LocationViewModel : ViewModel() {

    val userLat = MutableStateFlow<Double?>(null)
    val userLon = MutableStateFlow<Double?>(null)
    val error = MutableStateFlow<String?>(null)

    /**
     * 🔥 Obtém a última localização conhecida
     */
    fun fetchUserLocation(
        context: Context,
        onSuccess: (() -> Unit)? = null,
        onFail: (() -> Unit)? = null
    ) {
        val fused = LocationServices.getFusedLocationProviderClient(context)

        try {
            fused.lastLocation
                .addOnSuccessListener { location: Location? ->
                    if (location != null) {
                        userLat.value = location.latitude
                        userLon.value = location.longitude
                        onSuccess?.invoke()
                    } else {
                        error.value = "Localização indisponível"
                        onFail?.invoke()
                    }
                }.addOnFailureListener {
                    error.value = "Erro ao obter localização"
                    onFail?.invoke()
                }
        } catch (_: SecurityException) {
            error.value = "Permissão de localização negada"
            onFail?.invoke()
        }
    }

    /**
     * 🔥 Pede ao utilizador para ativar o GPS (se estiver desligado)
     */
    fun requestEnableGps(
        context: Context,
        onEnabled: (() -> Unit)? = null,
        onCancelled: (() -> Unit)? = null
    ) {
        val locationRequest = LocationRequest.Builder(
            Priority.PRIORITY_HIGH_ACCURACY,
            1200L
        ).build()

        val builder = LocationSettingsRequest.Builder()
            .addLocationRequest(locationRequest)
            .setAlwaysShow(true)

        val client = LocationServices.getSettingsClient(context)

        val task = client.checkLocationSettings(builder.build())

        task.addOnSuccessListener {
            onEnabled?.invoke()
        }

        task.addOnFailureListener { ex ->
            if (ex is ResolvableApiException && context is Activity) {
                try {
                    ex.startResolutionForResult(context, 55)
                } catch (_: Exception) {
                    onCancelled?.invoke()
                }
            } else {
                onCancelled?.invoke()
            }
        }
    }
}
