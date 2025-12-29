package com.example.moviespot.data.repository

import com.example.moviespot.data.dto.ReviewCreateDto
import com.example.moviespot.data.dto.ReviewResponseDto
import com.example.moviespot.data.dto.ReviewUpdateDto
import com.example.moviespot.data.remote.api.ReviewApiService

class ReviewRepository(
    private val api: ReviewApiService
) {

    suspend fun getAllReviews(): List<ReviewResponseDto> {
        return api.getAllReviews()
    }

    suspend fun getReview(id: Int): ReviewResponseDto {
        return api.getReviewById(id)
    }

    suspend fun getReviewsByUser(userId: Int): List<ReviewResponseDto> {
        return api.getReviewsByUser(userId)
    }

    suspend fun createReview(dto: ReviewCreateDto): ReviewResponseDto {
        return api.createReview(dto)
    }

    suspend fun updateReview(id: Int, dto: ReviewUpdateDto): ReviewResponseDto {
        return api.updateReview(id, dto)
    }
}
