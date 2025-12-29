  import { Component, OnInit, inject } from '@angular/core';
  import { CommonModule } from '@angular/common';
  import { FormsModule } from '@angular/forms';
  import { ActivatedRoute, Router } from '@angular/router';

  import {
    CinemaCreateDto,
    CinemaResponseDto,
    CinemaUpdateDto,
  } from '../../models/cinema.model';
  import { CinemaService } from '../../services/cinema.service';
  import { SidebarComponent } from '../sidebar/sidebar.component';
  import { MapPickerComponent } from '../../components/map-picker/map-picker';

  @Component({
    selector: 'app-cinema-form',
    standalone: true,
    imports: [CommonModule, FormsModule, SidebarComponent, MapPickerComponent],
    templateUrl: './cinema-form.component.html',
    styleUrls: ['./cinema-form.component.css'],
  })
  export class CinemaFormComponent implements OnInit {
    private cinemaService = inject(CinemaService);
    private route = inject(ActivatedRoute);
    private router = inject(Router);

    cinemaId: number | null = null;
    isEditMode = false;

    form: CinemaCreateDto = {
      name: '',
      street: '',
      city: '',
      state: '',
      zipCode: '',
      country: '',
      latitude: 0,
      longitude: 0,
    };

    loading = false;
    errorMessage = '';

    ngOnInit(): void {
      const param = this.route.snapshot.paramMap.get('cinemaId');
      this.cinemaId = param ? Number(param) : null;
      this.isEditMode = !!this.cinemaId;

      if (this.isEditMode && this.cinemaId) {
        this.cinemaService.getCinemaById(this.cinemaId).subscribe({
          next: (cinema: CinemaResponseDto) => {
            this.form = {
              name: cinema.name,
              street: cinema.street,
              city: cinema.city,
              state: cinema.state,
              zipCode: cinema.zipCode,
              country: cinema.country,
              latitude: cinema.latitude,
              longitude: cinema.longitude,
            };
          },
          error: () => {
            this.errorMessage = 'Erro ao carregar dados do cinema.';
          },
        });
      }
    }

    submit() {
      this.loading = true;
      this.errorMessage = '';

      if (this.isEditMode && this.cinemaId) {
        const dto: CinemaUpdateDto = {
          id: this.cinemaId,
          ...this.form,
        };

        this.cinemaService.updateCinema(this.cinemaId, dto).subscribe({
          next: () => {
            alert('Cinema atualizado com sucesso!');
            this.loading = false;
            this.router.navigate(['/cinemas']);
          },
          error: (err) => {
            this.errorMessage = err?.error || 'Erro ao atualizar cinema.';
            this.loading = false;
          },
        });
      } else {
        this.cinemaService.createCinema(this.form).subscribe({
          next: () => {
            alert('Cinema criado com sucesso!');
            this.loading = false;
            this.router.navigate(['/cinemas']);
          },
          error: () => {
            this.errorMessage = 'Erro ao criar cinema.';
            this.loading = false;
          },
        });
      }
    }

    onCancel() {
      this.router.navigate(['/cinemas']);
    }

    updateLocation(event: { lat: number; lng: number; address?: Record<string, string> }) {
      this.form.latitude = event.lat;
      this.form.longitude = event.lng;

      const a: Record<string, string> = event.address ?? {};

      this.form.street = `${a['road'] ?? ''} ${a['house_number'] ?? ''}`.trim();
      this.form.city =
        a['city'] ??
        a['town'] ??
        a['village'] ??
        a['hamlet'] ??
        '';

      this.form.state =
        a['state'] ??
        a['state_district'] ??
        a['region'] ??
        a['province'] ??
        a['county'] ??
        '';

      this.form.zipCode = a['postcode'] ?? '';
      this.form.country = a['country'] ?? '';
    }
  }
