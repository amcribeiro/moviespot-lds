package com.example.moviespot.data.remote.api

import com.example.moviespot.data.dto.ReviewCreateDto
import com.example.moviespot.data.dto.ReviewResponseDto
import com.example.moviespot.data.dto.ReviewUpdateDto
import retrofit2.http.*

interface ReviewApiService {

    @GET("Review")
    suspend fun getAllReviews(): List<ReviewResponseDto>

    @GET("Review/{id}")
    suspend fun getReviewById(
        @Path("id") id: Int
    ): ReviewResponseDto


    @GET("Review/user/{userId}")
    suspend fun getReviewsByUser(
        @Path("userId") userId: Int
    ): List<ReviewResponseDto>


    @POST("Review")
    suspend fun createReview(
        @Body dto: ReviewCreateDto
    ): ReviewResponseDto

    @PUT("Review/{id}")
    suspend fun updateReview(
        @Path("id") id: Int,
        @Body dto: ReviewUpdateDto
    ): ReviewResponseDto
}
