import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  CinemaHallReadDto,
  CinemaHallCreateDto,
  CinemaHallUpdateDto,
  CinemaHallDetailsDto
} from '../models/cinema-hall.model';

@Injectable({
  providedIn: 'root',
})
export class CinemaHallService {
  private baseUrl = 'http://localhost:5000/CinemaHall';

  private http = inject(HttpClient);

  getAllHalls(): Observable<CinemaHallReadDto[]> {
    return this.http.get<CinemaHallReadDto[]>(this.baseUrl);
  }

  getByCinema(cinemaId: number): Observable<CinemaHallReadDto[]> {
    return this.http.get<CinemaHallReadDto[]>(`${this.baseUrl}/cinema/${cinemaId}`);
  }

  getById(id: number): Observable<CinemaHallDetailsDto> {
    return this.http.get<CinemaHallDetailsDto>(`${this.baseUrl}/${id}`);
  }

  getHallsByCinemaId(cinemaId: number) {
  return this.http.get<CinemaHallReadDto[]>(`${this.baseUrl}/cinema/${cinemaId}`);
  }
  createHall(dto: CinemaHallCreateDto): Observable<CinemaHallReadDto> {
    return this.http.post<CinemaHallReadDto>(this.baseUrl, dto);
  }

  updateHall(id: number, dto: CinemaHallUpdateDto): Observable<CinemaHallReadDto> {
    return this.http.put<CinemaHallReadDto>(`${this.baseUrl}/${id}`, dto);
  }

  deleteHall(id: number): Observable<unknown> {
    return this.http.delete(`${this.baseUrl}/${id}`);
  }
}
