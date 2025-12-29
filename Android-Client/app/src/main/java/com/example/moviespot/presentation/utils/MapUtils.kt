package com.example.moviespot.presentation.utils

import android.content.Context
import android.content.Intent
import android.net.Uri
import androidx.core.net.toUri

fun Context.openInMaps(latitude: Double, longitude: Double, name: String) {
    val encoded = Uri.encode(name)
    val uri = "geo:$latitude,$longitude?q=$latitude,$longitude($encoded)".toUri()

    val intent = Intent(Intent.ACTION_VIEW, uri).apply {
        setPackage("com.google.android.apps.maps")
    }
    startActivity(intent)
}
