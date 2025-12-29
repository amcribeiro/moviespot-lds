import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  SeatCreateDto,
  SeatResponseDto,
  SeatUpdateDto
} from '../models/seat.model';

@Injectable({
  providedIn: 'root',
})
export class SeatService {
  private baseUrl = 'http://localhost:5000/Seat';
  private http = inject(HttpClient);

  addSeat(dto: SeatCreateDto): Observable<SeatResponseDto> {
    return this.http.post<SeatResponseDto>(this.baseUrl, dto);
  }

  updateSeat(dto: SeatUpdateDto): Observable<SeatResponseDto> {
    return this.http.put<SeatResponseDto>(this.baseUrl, dto);
  }

  getSeatsByHall(hallId: number) {
  return this.http.get<SeatResponseDto[]>(`${this.baseUrl}/hall/${hallId}`);
 }
  getByHall(hallId: number): Observable<SeatResponseDto[]> {
    return this.http.get<SeatResponseDto[]>(`${this.baseUrl}/hall/${hallId}`);
  }

  deleteSeat(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  deleteSeatsByHall(hallId: number) {
  return this.http.delete(`http://localhost:5000/Seat/hall/${hallId}`);
 }
 
}
