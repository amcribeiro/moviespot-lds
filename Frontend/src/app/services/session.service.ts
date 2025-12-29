  import { Injectable, inject } from '@angular/core';
  import { HttpClient, HttpParams } from '@angular/common/http'; 
  import { Observable } from 'rxjs';

  import {
    SessionCreateDto,
    SessionUpdateDto,
    SessionResponseDto,
    AvailableSeatDto,
    AvailableTimesResponse,
  } from '../models/session.model';

  @Injectable({
    providedIn: 'root',
  })
  export class SessionService {
    private baseUrl = 'http://localhost:5000/Session';
    private http = inject(HttpClient);

    createSession(data: SessionCreateDto): Observable<SessionResponseDto> {
      return this.http.post<SessionResponseDto>(this.baseUrl, data);
    }

    getAllSessions(): Observable<SessionResponseDto[]> {
      return this.http.get<SessionResponseDto[]>(this.baseUrl);
    }

    getSessionById(id: number): Observable<SessionResponseDto> {
      return this.http.get<SessionResponseDto>(`${this.baseUrl}/${id}`);
    }

    updateSession(id: number, data: SessionUpdateDto): Observable<SessionResponseDto> {
      return this.http.put<SessionResponseDto>(`${this.baseUrl}/${id}`, data);
    }

    deleteSession(id: number): Observable<SessionResponseDto> {
      return this.http.delete<SessionResponseDto>(`${this.baseUrl}/${id}`);
    }

    getAvailableTimes(
    cinemaHallId: number,
    date: Date,
    runtimeMinutes: number
  ): Observable<AvailableTimesResponse> {

    const utcDate = new Date(Date.UTC(
      date.getFullYear(),
      date.getMonth(),
      date.getDate()
    ));

    const params = new HttpParams()
      .set('cinemaHallId', cinemaHallId.toString())
      .set('date', utcDate.toISOString()) 
      .set('runtimeMinutes', runtimeMinutes.toString());

    return this.http.get<AvailableTimesResponse>(
      `${this.baseUrl}/available-times`,
      { params }
    );
  }

    getAvailableSeats(sessionId: number): Observable<AvailableSeatDto[]> {
      return this.http.get<AvailableSeatDto[]>(
        `${this.baseUrl}/${sessionId}/available-seats`
      );
    }
  }
