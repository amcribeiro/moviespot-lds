import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { MovieDto } from '../models/movie.model';

@Injectable({
  providedIn: 'root',
})
export class MovieService {
  private baseUrl = 'http://localhost:5000/Movie';

  private http = inject(HttpClient);

  getAllMovies(): Observable<MovieDto[]> {
    return this.http.get<MovieDto[]>(this.baseUrl);
  }

  getMovieById(id: number): Observable<MovieDto> {
    return this.http.get<MovieDto>(`${this.baseUrl}/${id}`);
  }
}
