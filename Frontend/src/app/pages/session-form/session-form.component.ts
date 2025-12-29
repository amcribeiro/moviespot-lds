import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';

import { SidebarComponent } from '../sidebar/sidebar.component';

import { MovieService } from '../../services/movie.service';
import { CinemaService } from '../../services/cinema.service';
import { CinemaHallService } from '../../services/cinema-hall.service';
import { SessionService } from '../../services/session.service';

import { MovieDto } from '../../models/movie.model';
import { CinemaResponseDto } from '../../models/cinema.model';
import { CinemaHallReadDto } from '../../models/cinema-hall.model';
import { SessionResponseDto } from '../../models/session.model';
import { SeatService } from '../../services/seat.service';
import { SeatResponseDto } from '../../models/seat.model';


@Component({
  selector: 'app-session-form',
  standalone: true,
  imports: [CommonModule, FormsModule, SidebarComponent],
  templateUrl: './session-form.component.html',
  styleUrls: ['./session-form.component.css'],
})
export class SessionFormComponent implements OnInit {

  movies: MovieDto[] = [];
  cinemas: CinemaResponseDto[] = [];
  halls: CinemaHallReadDto[] = [];
  availableTimes: string[] = [];

  selectedTime: string | null = null;
  sessionId: number | null = null;
  isEditMode = false;

  loadingTimes = false;
  errorMessage = '';

  form = {
    movieId: 0,
    cinemaId: 0,
    cinemaHallId: 0,
    startDate: '',
    price: 0,
  };

  private movieService = inject(MovieService);
  private cinemaService = inject(CinemaService);
  private hallService = inject(CinemaHallService);
  private sessionService = inject(SessionService);
  private seatService = inject(SeatService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);


  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      this.sessionId = +idParam;
      this.isEditMode = true;
    }

    this.movieService.getAllMovies().subscribe(r => this.movies = r);
    this.cinemaService.getAllCinemas().subscribe(r => this.cinemas = r);

    if (this.sessionId) {
      this.loadSessionData(this.sessionId);
    }
  }

  loadSessionData(id: number) {
    this.sessionService.getSessionById(id).subscribe({
      next: (session: SessionResponseDto & { cinemaId?: number }) => {
        this.form.movieId = session.movieId;
        this.form.price = session.price;
        this.form.cinemaHallId = session.cinemaHallId;
        this.form.cinemaId = session.cinemaId || 0;

        const dateObj = new Date(session.startDate);

        this.form.startDate = dateObj.toISOString().split('T')[0];

        const hours = dateObj.getHours().toString().padStart(2, '0');
        const minutes = dateObj.getMinutes().toString().padStart(2, '0');
        this.selectedTime = `${hours}:${minutes}`;

        this.loadHallsAndTimes(this.form.cinemaId, true);
      },
      error: () => {
        alert('Erro ao carregar sessão');
        this.router.navigate(['/sessions']);
      }
    });
  }

  onCinemaSelected() {
    this.halls = [];
    this.form.cinemaHallId = 0;
    this.availableTimes = [];
    this.selectedTime = null;

    if (!this.form.cinemaId) return;

    this.loadHallsAndTimes(this.form.cinemaId, false);
  }
  cancel(){
    this.router.navigate(['/sessions']);
  }

  loadHallsAndTimes(cinemaId: number, preserveSelection: boolean) {
    this.halls = [];
    this.availableTimes = [];

    if (!preserveSelection) {
      this.selectedTime = null;
    }

    if (!cinemaId) {
      return;
    }

    this.hallService.getHallsByCinemaId(cinemaId).subscribe({
      next: halls => {
        if (!halls || halls.length === 0) {
          this.halls = [];
          return;
        }

        const validHalls: CinemaHallReadDto[] = [];
        let processed = 0;

        const finishHallCheck = () => {
          processed++;
          if (processed === halls.length) {
            this.halls = validHalls;

            if (preserveSelection) {
              this.loadAvailableTimes(true);
            }
          }
        };

        halls.forEach(hall => {
          this.seatService.getSeatsByHall(hall.id).subscribe({
            next: (seats: SeatResponseDto[]) => {
              if (seats && seats.length > 0) {
                validHalls.push(hall);
              }
              finishHallCheck();
            },
            error: (err) => {
              console.error('Erro ao carregar lugares para a sala', hall.id, err);
              finishHallCheck();
            }
          });
        });
      },
      error: err => {
        console.error('Erro ao carregar salas', err);
        this.halls = [];
      }
    });
  }
  
  onMovieSelected() {
    this.availableTimes = [];
    this.selectedTime = null;
    this.loadAvailableTimes();
  }

  loadAvailableTimes(isInitialLoad = false) {
    this.errorMessage = '';

    if (!isInitialLoad) {
      this.availableTimes = [];
      this.selectedTime = null;
    }

    if (!this.form.cinemaHallId || !this.form.startDate || !this.form.movieId) return;

    const selectedMovieId = Number(this.form.movieId);
    const movie = this.movies.find(m => m.id === selectedMovieId);
    const runtime = movie?.duration ?? 120;

    const dateObj = new Date(this.form.startDate);
    this.loadingTimes = true;

    this.sessionService.getAvailableTimes(
      this.form.cinemaHallId,
      dateObj,
      runtime
    ).subscribe({
      next: (res) => {
        this.loadingTimes = false;

        const times = res || [];

        if (isInitialLoad && this.selectedTime) {
          if (!times.includes(this.selectedTime)) {
            times.push(this.selectedTime);
            times.sort();
          }
        }

        if (times.length === 0) {
          this.errorMessage = 'Não existem horários disponíveis.';
          this.availableTimes = [];
          return;
        }

        this.availableTimes = times;
      },
      error: (err) => {
        console.error(err);
        this.loadingTimes = false;

        if (isInitialLoad && this.selectedTime) {
          this.availableTimes = [this.selectedTime];
        } else {
          this.errorMessage = 'Erro ao carregar horários.';
        }
      }
    });
  }

  submit() {
    const selectedDate = new Date(this.form.startDate);
    const today = new Date();
    selectedDate.setHours(0, 0, 0, 0);
    today.setHours(0, 0, 0, 0);
    if (selectedDate < today) {
      alert('A data de início não pode ser no passado.');
      return;
    }
    if (!this.selectedTime) {
      alert('Escolhe um horário para a sessão.');
      return;
    }

    if (!this.form.startDate || !this.form.cinemaHallId || !this.form.movieId) {
      alert('Preenche todos os campos obrigatórios.');
      return;
    }

    const start = new Date(this.form.startDate);
    const [h, m] = this.selectedTime.split(':').map(Number);
    start.setHours(h, m);

    const selectedMovieId = Number(this.form.movieId);
    const movie = this.movies.find(m => m.id === selectedMovieId);
    const runtime = movie?.duration ?? 120;

    const end = new Date(start);
    end.setMinutes(end.getMinutes() + runtime);

    const basePayload = {
      movieId: selectedMovieId,
      cinemaHallId: this.form.cinemaHallId,
      startDate: start.toISOString(),
      endDate: end.toISOString(),
      price: this.form.price,
    };

    if (this.isEditMode && this.sessionId) {
      const payload = {
        id: this.sessionId,
        ...basePayload,
        updatedBy: 1,
      };

      this.sessionService.updateSession(this.sessionId, payload).subscribe({
        next: () => {
          alert('Sessão atualizada com sucesso!');
          this.router.navigate(['/sessions']);
        },
        error: (err) => {
          console.error(err);
          const msg = err.error?.message || err.error || 'Erro ao atualizar';
          alert('Erro: ' + msg);
        }
      });

    } else {
      const payload = {
        ...basePayload,
        createdBy: 1,
      };

      this.sessionService.createSession(payload).subscribe({
        next: () => {
          alert('Sessão criada com sucesso!');
          this.router.navigate(['/sessions']);
        },
        error: (err) => {
          console.error(err);
          const msg = err.error?.message || err.error || 'Erro ao criar';
          alert('Erro: ' + msg);
        }
      });
    }
  }
}
