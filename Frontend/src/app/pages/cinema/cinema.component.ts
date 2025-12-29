import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';

import { CinemaService } from '../../services/cinema.service';
import { CinemaHallService } from '../../services/cinema-hall.service';
import { SeatService } from '../../services/seat.service';

import { CinemaResponseDto } from '../../models/cinema.model';
import { SidebarComponent } from '../sidebar/sidebar.component';

@Component({
  selector: 'app-cinemas',
  standalone: true,
  imports: [CommonModule, RouterModule, SidebarComponent],
  templateUrl: './cinema.component.html',
  styleUrls: ['./cinema.component.css']
})
export class CinemaComponent implements OnInit {

  cinemas: CinemaResponseDto[] = [];

  expanded: Record<number, boolean> = {};

  cinemaHalls: Record<number, {
    hall: import('../../models/cinema-hall.model').CinemaHallReadDto,
    seats: import('../../models/seat.model').SeatResponseDto[],
    total: number,
    vip: number,
    reduced: number,
    normal: number
  }[]> = {};

  private cinemaService = inject(CinemaService);
  private hallService = inject(CinemaHallService);
  private seatService = inject(SeatService);
  private router = inject(Router);

  ngOnInit(): void {
    this.loadCinemas();
  }

  private loadCinemas() {
    this.cinemaService.getAllCinemas()
      .subscribe((r: CinemaResponseDto[]) => this.cinemas = r);
  }

  toggle(cinemaId: number) {
    this.expanded[cinemaId] = !this.expanded[cinemaId];

    if (this.cinemaHalls[cinemaId]) return;

    this.hallService.getHallsByCinemaId(cinemaId).subscribe({
      next: halls => {
        if (!halls.length) {
          this.cinemaHalls[cinemaId] = [];
          return;
        }

        halls.forEach((hall: import('../../models/cinema-hall.model').CinemaHallReadDto) => {
          this.seatService.getSeatsByHall(hall.id).subscribe(
            (seats: import('../../models/seat.model').SeatResponseDto[]) => {

              const total = seats.length;
              const vip = seats.filter(s => s.seatType === 'VIP').length;
              const reduced = seats.filter(s => s.seatType === 'Reduced').length;
              const normal = seats.filter(s => s.seatType === 'Normal').length;

              if (!this.cinemaHalls[cinemaId]) {
                this.cinemaHalls[cinemaId] = [];
              }

              this.cinemaHalls[cinemaId].push({ hall, seats, total, vip, reduced, normal });
            }
          );
        });
      },
      error: () => {
        this.cinemaHalls[cinemaId] = [];
      }
    });
  }

  deleteHall(hallId: number, cinemaId: number) {
    if (!confirm('Queres mesmo apagar esta sala e todos os lugares?')) return;

    this.seatService.getSeatsByHall(hallId).subscribe({
      next: seats => {
        if (!seats.length) {
          this.deleteHallEntity(hallId, cinemaId);
          return;
        }

        let done = 0;
        const total = seats.length;

        const onSeatDone = () => {
          done++;
          if (done === total) {
            this.deleteHallEntity(hallId, cinemaId);
          }
        };

        seats.forEach(seat => {
          this.seatService.deleteSeat(seat.id).subscribe({
            next: () => onSeatDone(),
            error: err => {
              console.error('Erro ao apagar lugar', seat.id, err);
              onSeatDone();
            }
          });
        });
      },
      error: err => {
        console.error('Erro ao carregar lugares.', err);
        this.deleteHallEntity(hallId, cinemaId);
      }
    });
  }

  private deleteHallEntity(hallId: number, cinemaId: number) {
    this.hallService.deleteHall(hallId).subscribe({
      next: () => {
        this.cinemaHalls[cinemaId] =
          (this.cinemaHalls[cinemaId] || []).filter(h => h.hall.id !== hallId);

        alert('Sala apagada com sucesso!');
      },
      error: err => {
        console.error('Erro ao apagar sala no backend:', err);

        this.cinemaHalls[cinemaId] =
          (this.cinemaHalls[cinemaId] || []).filter(h => h.hall.id !== hallId);
      }
    });
  }

  private removeHallFromUI(hallId: number, cinemaId: number) {
    this.cinemaHalls[cinemaId] =
      (this.cinemaHalls[cinemaId] || []).filter(h => h.hall.id !== hallId);
  }

  goCreate() {
    this.router.navigate(['/cinemas/create']);
  }

  editCinema(cinemaId: number) {
    this.router.navigate([`/cinemas/edit/${cinemaId}`]);
  }

  addHall(cinemaId: number) {
    this.router.navigate([`/cinemas/${cinemaId}/halls/create`]);
  }

  editHall(hallId: number, cinemaId: number) {
    this.router.navigate([`/cinemas/${cinemaId}/halls/${hallId}/edit`]);
  }

  deleteCinema(cinemaId: number) {
    if (!confirm('Tens a certeza que queres apagar este cinema?')) return;

    this.cinemaService.deleteCinema(cinemaId).subscribe({
      next: () => {
        this.cinemas = this.cinemas.filter(c => c.id !== cinemaId);
      },
      error: () => alert('Erro ao apagar cinema')
    });
  }
}
