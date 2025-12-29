import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';

import { SidebarComponent } from '../sidebar/sidebar.component';

import { CinemaHallService } from '../../services/cinema-hall.service';
import { CinemaService } from '../../services/cinema.service';
import { SeatService } from '../../services/seat.service';

import {
  CinemaHallUpdateDto,
  CinemaHallDetailsDto,
} from '../../models/cinema-hall.model';
import { CinemaResponseDto } from '../../models/cinema.model';
import { SeatResponseDto } from '../../models/seat.model';

interface SeatPreview {
  row: string;
  number: number;
  seatNumber: string;
  type: 'Normal' | 'VIP' | 'Reduced';
}

@Component({
  selector: 'app-hall-form',
  standalone: true,
  imports: [CommonModule, FormsModule, SidebarComponent],
  templateUrl: './hall-form.component.html',
  styleUrls: ['./hall-form.component.css'],
})
export class HallFormComponent implements OnInit {
  private hallService = inject(CinemaHallService);
  private cinemaService = inject(CinemaService);
  private seatService = inject(SeatService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  cinemaId = 0;
  hallId: number | null = null;
  cinemaName = '';

  isEditMode = false;

  form = {
    name: '',
    rows: 0,    
    seatsPerRow: 0,
  };

  seatMap: SeatPreview[][] = [];

  stats = { normal: 0, vip: 0, reduced: 0, total: 0 };

  loading = false;
  errorMessage = '';

  ngOnInit(): void {
    this.cinemaId = Number(this.route.snapshot.paramMap.get('cinemaId'));
    const hallParam = this.route.snapshot.paramMap.get('hallId');
    this.hallId = hallParam ? Number(hallParam) : null;
    this.isEditMode = !!this.hallId;

    this.cinemaService.getCinemaById(this.cinemaId).subscribe({
      next: (c: CinemaResponseDto) => (this.cinemaName = c.name),
      error: () => (this.cinemaName = ''),
    });

    if (this.isEditMode && this.hallId) {
      this.loadHallForEdit(this.hallId);
    } else {
      this.generateMap();
    }
  }

  generateMap() {
    this.seatMap = [];
    const rows = this.form.rows;
    const cols = this.form.seatsPerRow;

    if (!rows || !cols) return;

    for (let r = 0; r < rows; r++) {
      const rowSeats: SeatPreview[] = [];
      const rowLabel = String.fromCharCode(65 + r); 

      let initialType: 'Normal' | 'VIP' | 'Reduced' = 'Normal';
      if (r === 0) {
        initialType = 'Reduced';
      }

      for (let c = 1; c <= cols; c++) {
        rowSeats.push({
          row: rowLabel,
          number: c,
          seatNumber: `${rowLabel}${c}`,
          type: initialType
        });
      }
      this.seatMap.push(rowSeats);
    }
    this.updateStats();
  }

  toggleSeatType(seat: SeatPreview) {
    if (seat.row === 'A') return;

    if (seat.type === 'Normal') {
      seat.type = 'VIP';
    } else if (seat.type === 'VIP') {
      seat.type = 'Normal';
    }
    
    this.updateStats();
  }

  updateStats() {
    let normal = 0, vip = 0, reduced = 0;
    this.seatMap.flat().forEach(s => {
      if (s.type === 'Normal') normal++;
      if (s.type === 'VIP') vip++;
      if (s.type === 'Reduced') reduced++;
    });
    this.stats = { normal, vip, reduced, total: normal + vip + reduced };
  }

  private loadHallForEdit(hallId: number) {
    this.hallService.getById(hallId).subscribe({
      next: (hall: CinemaHallDetailsDto) => {
        this.form.name = hall.name;
      },
      error: () => {
        this.errorMessage = 'Erro ao carregar dados da sala.';
      },
    });

    this.seatService.getSeatsByHall(hallId).subscribe({
      next: (seats: SeatResponseDto[]) => {
        if (!seats.length) {
          this.generateMap();
          return;
        }

        let maxRowIndex = 0;
        let maxSeatNumber = 0;

        seats.forEach((seat) => {
          const seatNumber = seat.seatNumber || '';
          const rowLetter = seatNumber.charAt(0);
          const colPart = seatNumber.slice(1);
          const colNumber = parseInt(colPart, 10) || 0;
          const rowIndex = rowLetter.toUpperCase().charCodeAt(0) - 65;

          if (rowIndex > maxRowIndex) maxRowIndex = rowIndex;
          if (colNumber > maxSeatNumber) maxSeatNumber = colNumber;
        });

        this.form.rows = maxRowIndex + 1;
        this.form.seatsPerRow = maxSeatNumber;

        this.generateMap();

        this.seatMap.forEach(row => {
          row.forEach(visualSeat => {
            const dbSeat = seats.find(s => s.seatNumber === visualSeat.seatNumber);
            if (dbSeat) {
              visualSeat.type = dbSeat.seatType as 'Normal' | 'VIP' | 'Reduced';
            }
          });
        });
        
        this.updateStats();
      },
      error: () => {
        this.errorMessage = 'Erro ao carregar lugares.';
      },
    });
  }

  submit() {
    this.loading = true;
    this.errorMessage = '';

    if (this.isEditMode && this.hallId) {
      this.updateHallAndSeats();
    } else {
      this.createHallAndSeats();
    }
  }

  private createHallAndSeats() {
    const hallDto = {
      name: this.form.name,
      cinemaId: this.cinemaId,
    };

    this.hallService.createHall(hallDto).subscribe({
      next: (hall: unknown) => {
        const typed = hall as { id: number };
        this.generateSeats(typed.id);
      },
      error: (err) => {
        this.errorMessage = err?.error || 'Erro ao criar sala.';
        this.loading = false;
      },
    });
  }

  private updateHallAndSeats() {
    const dto: CinemaHallUpdateDto = {
      id: this.hallId!,
      name: this.form.name,
      cinemaId: this.cinemaId,
    };

    this.hallService.updateHall(this.hallId!, dto).subscribe({
      next: () => {
        this.regenerateSeats();
      },
      error: () => {
        this.errorMessage = 'Erro ao atualizar sala.';
        this.loading = false;
      },
    });
  }

  private buildSeatsPayload(hallId: number) {
    const seatsPayload: {
      cinemaHallId: number;
      seatNumber: string;
      seatType: 'Normal' | 'VIP' | 'Reduced';
    }[] = [];

    this.seatMap.forEach(row => {
      row.forEach(seat => {
        seatsPayload.push({
          cinemaHallId: hallId,
          seatNumber: seat.seatNumber,
          seatType: seat.type
        });
      });
    });

    return seatsPayload;
  }

  private generateSeats(hallId: number) {
    const seatsPayload = this.buildSeatsPayload(hallId);

    if (!seatsPayload.length) {
      this.loading = false;
      this.router.navigate(['/cinemas']);
      return;
    }

    let created = 0;
    const total = seatsPayload.length;

    seatsPayload.forEach((seat) => {
      this.seatService.addSeat(seat).subscribe({
        next: () => {
          created++;
          if (created === total) {
            this.loading = false;
            this.router.navigate(['/cinemas']);
          }
        },
        error: () => {
          this.errorMessage = 'Erro ao gerar lugares.';
          this.loading = false;
        },
      });
    });
  }

  private regenerateSeats() {
    const hallId = this.hallId!;
    const seatsPayload = this.buildSeatsPayload(hallId);

    const createNewSeats = () => {
      if (!seatsPayload.length) {
        this.loading = false;
        this.router.navigate(['/cinemas']);
        return;
      }

      let created = 0;
      const total = seatsPayload.length;

      seatsPayload.forEach((seat) => {
        this.seatService.addSeat(seat).subscribe({
          next: () => {
            created++;
            if (created === total) {
              this.loading = false;
              this.router.navigate(['/cinemas']);
            }
          },
          error: () => {
            this.errorMessage = 'Erro ao regenerar lugares.';
            this.loading = false;
          },
        });
      });
    };

    this.seatService.getSeatsByHall(hallId).subscribe({
      next: (seats) => {
        if (!seats.length) {
          createNewSeats();
          return;
        }

        let deleted = 0;
        const totalToDelete = seats.length;

        seats.forEach((seat) => {
          this.seatService.deleteSeat(seat.id).subscribe({
            next: () => {
              deleted++;
              if (deleted === totalToDelete) {
                createNewSeats();
              }
            },
            error: () => {
              this.errorMessage = 'Erro ao apagar lugares antigos.';
              this.loading = false;
            },
          });
        });
      },
      error: () => {
        this.errorMessage = 'Erro ao carregar lugares antigos.';
        this.loading = false;
      },
    });
  }

  onCancel() {
    this.router.navigate(['/cinemas']);
  }
}