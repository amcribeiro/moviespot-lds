import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SidebarComponent } from "../sidebar/sidebar.component";
import { MovieService } from '../../services/movie.service';
import { MovieDto } from '../../models/movie.model';

@Component({
  selector: 'app-movie-list',
  standalone: true,
  imports: [CommonModule, SidebarComponent],
  templateUrl: './movie.component.html',
  styleUrls: ['./movie.component.css'],
})
export class MovieComponent implements OnInit {
    
    movies: MovieDto[] = [];
    loading = true;
    errorMessage = '';

    private movieService = inject(MovieService);

    ngOnInit(): void {
        this.loadMovies();
    }

    loadMovies() {
        this.loading = true;
        this.movieService.getAllMovies().subscribe({
            next: (data) => {
                this.movies = data;
                this.loading = false;
                if (this.movies.length === 0) {
                    this.errorMessage = 'Ainda não existem filmes na base de dados.';
                }
            },
            error: (err) => {
                console.error('Error loading movies', err);
                this.errorMessage = 'Erro ao carregar os filmes.';
                this.loading = false;
            }
        });
    }
}