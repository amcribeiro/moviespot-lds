package com.example.moviespot.data.dto

data class MovieCinemaSessions(
    val cinemaName: String,
    val rooms: List<CinemaRoomSessions>
)

data class CinemaRoomSessions(
    val roomName: String,
    val sessions: List<SessionResponseDto>
)