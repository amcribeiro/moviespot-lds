import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  CinemaResponseDto,
  CinemaCreateDto,
  CinemaUpdateDto
} from '../models/cinema.model';

@Injectable({
  providedIn: 'root',
})
export class CinemaService {

  private http = inject(HttpClient);
  private baseUrl = 'http://localhost:5000/Cinemas';

  getAllCinemas(): Observable<CinemaResponseDto[]> {
    return this.http.get<CinemaResponseDto[]>(this.baseUrl);
  }

  getCinemaById(id: number): Observable<CinemaResponseDto> {
    return this.http.get<CinemaResponseDto>(`${this.baseUrl}/${id}`);
  }

  createCinema(dto: CinemaCreateDto): Observable<CinemaResponseDto> {
    return this.http.post<CinemaResponseDto>(this.baseUrl, dto);
  }

  updateCinema(id: number, dto: CinemaUpdateDto): Observable<CinemaResponseDto> {
    return this.http.put<CinemaResponseDto>(`${this.baseUrl}/${id}`, dto);
  }

  deleteCinema(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
