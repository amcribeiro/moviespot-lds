package com.example.moviespot.presentation.screens.booking

import android.annotation.SuppressLint
import androidx.compose.foundation.background
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.verticalScroll
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.ConfirmationNumber
import androidx.compose.material.icons.filled.EventSeat
import androidx.compose.material.icons.filled.LocalMovies
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.platform.testTag
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp

@SuppressLint("DefaultLocale")
@Composable
fun BookingSummaryScreen(
    bookingViewModel: BookingViewModel,
    voucherViewModel: VoucherViewModel,
    sessionId: Int,
    seatIds: List<Int>,
    onProceedPayment: (bookingId: Int, voucherId: Int?, userId: Int) -> Unit,
    onBack: () -> Unit
) {

    val ui = bookingViewModel.summaryState

    if (ui.movieTitle.isBlank()) {
        Box(
            modifier = Modifier.fillMaxSize().background(Color(0xFF121212)),
            contentAlignment = Alignment.Center
        ) {
            CircularProgressIndicator(color = Color.White)
        }
        return
    }

    val originalTotal = ui.totalPrice
    val discountedTotal = voucherViewModel.discountedTotal ?: originalTotal
    var voucherCode by remember { mutableStateOf("") }
    val voucherError = voucherViewModel.error

    Box(
        modifier = Modifier
            .fillMaxSize()
            .background(Color(0xFF121212))
            .padding(24.dp)
    ) {

        Column(
            horizontalAlignment = Alignment.CenterHorizontally,
            modifier = Modifier
                .fillMaxSize()
                .verticalScroll(rememberScrollState())
        ) {

            Text(
                "Booking Summary",
                color = Color.White,
                fontSize = 22.sp,
                fontWeight = FontWeight.Bold
            )

            Spacer(Modifier.height(20.dp))

            SummaryCard(Icons.Default.LocalMovies, ui.movieTitle, ui.cinemaName)
            Spacer(Modifier.height(10.dp))

            SeatsWithPriceCard(ui.seatsWithPrice)
            Spacer(Modifier.height(10.dp))

            SummaryCard(
                Icons.Default.ConfirmationNumber,
                "Session",
                "${ui.sessionDate} • ${ui.sessionTime}"
            )

            Spacer(Modifier.height(20.dp))

            voucherViewModel.voucher?.let {
                PriceRow("Discount (${it.code})", "-${(originalTotal * it.value).format(2)}€")
            }

            Divider(color = Color.Gray)

            PriceRow(
                "Total",
                "${String.format("%.2f", discountedTotal)}€",
                bold = true
            )

            Spacer(Modifier.height(20.dp))

            OutlinedTextField(
                value = voucherCode,
                onValueChange = { voucherCode = it },
                label = { Text("Voucher Code", color = Color.White) },
                textStyle = LocalTextStyle.current.copy(color = Color.White),
                modifier = Modifier.fillMaxWidth().testTag("VoucherCodeInput")
            )

            voucherError?.let {
                Spacer(Modifier.height(6.dp))
                Text(it, color = Color.Red, fontSize = 13.sp)
            }

            Spacer(Modifier.height(10.dp))

            Button(
                onClick = {
                    voucherViewModel.applyVoucher(
                        voucherCode = voucherCode,
                        originalTotal = originalTotal
                    )
                },
                enabled = !voucherViewModel.isLoading,
                modifier = Modifier.fillMaxWidth(),
                colors = ButtonDefaults.buttonColors(
                    containerColor = Color(0xFF424242)
                )
            ) {

                if (voucherViewModel.isLoading) {
                    CircularProgressIndicator(
                        modifier = Modifier.size(20.dp),
                        color = Color.White,
                        strokeWidth = 2.dp
                    )
                } else {
                    Text("Apply Voucher", color = Color.White)
                }
            }

            voucherViewModel.voucher?.let {
                Spacer(Modifier.height(6.dp))
                TextButton(
                    onClick = {
                        voucherViewModel.removeVoucher()
                        voucherCode = ""
                    }
                ) {
                    Text("Remove Voucher", color = Color(0xFFFF5252))
                }
            }

            Spacer(Modifier.height(25.dp))


            bookingViewModel.createError?.let {
                Spacer(Modifier.height(8.dp))
                Text(
                    text = it,
                    color = Color.Red,
                    fontSize = 13.sp,
                    modifier = Modifier.testTag("BookingErrorText")
                )
            }

            Button(
                onClick = {
                    // ALTERADO: Já não passamos tokenProvider
                    bookingViewModel.createBooking(
                        sessionId = sessionId,
                        seatIds = seatIds,
                        voucherId = voucherViewModel.voucher?.id,
                        onProceedPayment = onProceedPayment
                    )
                },
                modifier = Modifier
                    .fillMaxWidth()
                    .height(55.dp)
                    .padding(horizontal = 2.dp),
                colors = ButtonDefaults.buttonColors(
                    containerColor = Color(0xFFD32F2F)
                )
            ) {
                Text("Proceed to Payment", color = Color.White, fontSize = 16.sp)
            }

            Spacer(Modifier.height(10.dp))

            TextButton(onClick = onBack) {
                Text("Back", color = Color.White)
            }
        }
    }
}

fun Double.format(decimals: Int) = "%.${decimals}f".format(this)

@Composable
fun SummaryCard(icon: androidx.compose.ui.graphics.vector.ImageVector, title: String, subtitle: String) {
    Card(
        colors = CardDefaults.cardColors(containerColor = Color(0xFF1E1E1E)),
        modifier = Modifier.fillMaxWidth()
    ) {
        Row(modifier = Modifier.padding(16.dp), verticalAlignment = Alignment.CenterVertically) {
            Icon(icon, null, tint = Color.White)
            Spacer(Modifier.width(12.dp))
            Column {
                Text(title, color = Color.White, fontSize = 16.sp, fontWeight = FontWeight.Bold)
                Text(subtitle, color = Color.Gray, fontSize = 14.sp)
            }
        }
    }
}

@Composable
fun PriceRow(label: String, value: String, bold: Boolean = false) {
    Row(Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.SpaceBetween) {
        Text(label, color = Color.White, fontSize = 16.sp)
        Text(
            value,
            color = Color.White,
            fontSize = 16.sp,
            fontWeight = if (bold) FontWeight.Bold else FontWeight.Normal
        )
    }
}

@SuppressLint("DefaultLocale")
@Composable
fun SeatsWithPriceCard(seats: List<Pair<String, Double>>) {
    Card(
        colors = CardDefaults.cardColors(containerColor = Color(0xFF1E1E1E)),
        modifier = Modifier.fillMaxWidth()
    ) {
        Column(modifier = Modifier.padding(16.dp)) {
            Row(verticalAlignment = Alignment.CenterVertically) {
                Icon(Icons.Default.EventSeat, null, tint = Color.White)
                Spacer(Modifier.width(12.dp))
                Text("Seats", color = Color.White, fontSize = 16.sp, fontWeight = FontWeight.Bold)
            }

            Spacer(Modifier.height(8.dp))

            seats.forEach { (name, price) ->
                Text(
                    text = "$name — ${String.format("%.2f", price)}€",
                    color = Color.Gray,
                    fontSize = 14.sp
                )
            }
        }
    }
}