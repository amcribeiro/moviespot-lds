package com.example.moviespot.data.repository

import com.example.moviespot.data.dto.VoucherResponseDto
import com.example.moviespot.data.dto.VoucherUpdateDto
import com.example.moviespot.data.remote.api.VoucherApiService
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext

class VoucherRepository(
    private val api: VoucherApiService
) {

    /*suspend fun getById(id: Int): VoucherResponseDto = withContext(Dispatchers.IO) {
        api.getVoucherById(id)
    }

    suspend fun update(id: Int, dto: VoucherUpdateDto): VoucherResponseDto = withContext(Dispatchers.IO) {
        api.updateVoucher(id, dto)
    }*/

    suspend fun getByCode(code: String): VoucherResponseDto = withContext(Dispatchers.IO) {
        api.validateVoucherByCode(code)
    }
}
