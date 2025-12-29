package com.example.moviespot.data.remote.api

import com.example.moviespot.data.dto.VoucherResponseDto
import com.example.moviespot.data.dto.VoucherUpdateDto
import retrofit2.http.*

interface VoucherApiService {

    @GET("Voucher/{id}")
    suspend fun getVoucherById(@Path("id") id: Int): VoucherResponseDto

    @PUT("Voucher/{id}")
    suspend fun updateVoucher(@Path("id") id: Int, @Body dto: VoucherUpdateDto): VoucherResponseDto

    @GET("Voucher/validate/{code}")
    suspend fun validateVoucherByCode(@Path("code") code: String): VoucherResponseDto
}
