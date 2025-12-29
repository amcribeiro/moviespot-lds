import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { StatsResponseDto } from '../models/stats.model';

@Injectable({
  providedIn: 'root',
})
export class StatsService {

  private baseUrl = 'http://localhost:5000/Stats';
  private http = inject(HttpClient);

  getStats(): Observable<StatsResponseDto> {
    return this.http.get<StatsResponseDto>(this.baseUrl);
  }
}
