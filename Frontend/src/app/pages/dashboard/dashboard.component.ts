import { Component, OnInit, AfterViewInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { SidebarComponent } from '../../pages/sidebar/sidebar.component';
import { Chart } from 'chart.js/auto';

import { UserService } from '../../services/user.service';
import { StatsService } from '../../services/stats.service';
import { StatsResponseDto } from '../../models/stats.model';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, SidebarComponent],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css'],
})
export class DashboardComponent implements OnInit, AfterViewInit {

  userName = '';

  stats: StatsResponseDto = {
    totalSessions: 0,
    activeRooms: 0,
    todaysSessions: 0,
    movies: 0,
  };

  private userService = inject(UserService);
  private statsService = inject(StatsService);
  private router = inject(Router);

  private viewInitialized = false;

  private sessionsChart?: Chart;
  private dailyChart?: Chart;
  private moviesChart?: Chart;

  ngOnInit(): void {
    const token = localStorage.getItem('token');

    if (!token) {
      this.router.navigate(['/login']);
      return;
    }

    const userId = this.getUserIdFromToken(token);

    if (!userId) {
      console.error('Token inválido: não contém ID');
      this.router.navigate(['/login']);
      return;
    }

    this.loadUserProfile(userId);
    this.loadStats();
  }

  ngAfterViewInit(): void {
    this.viewInitialized = true;
    this.initChartsIfPossible();
  }

  private getUserIdFromToken(token: string): number | null {
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      return payload.id || payload.nameid || null;
    } catch {
      return null;
    }
  }

  loadUserProfile(userId: number): void {
    this.userService.getById(userId).subscribe({
      next: (res) => (this.userName = res.name || 'Utilizador'),
      error: () => this.router.navigate(['/login']),
    });
  }

  loadStats(): void {
    this.statsService.getStats().subscribe({
      next: (res) => {
        this.stats = res;
        this.initChartsIfPossible();
      },
      error: (err) => console.error('Erro ao buscar estatísticas:', err),
    });
  }

  private initChartsIfPossible(): void {
    if (!this.viewInitialized) return;

    const sessionsCanvas = document.getElementById('sessionsChart') as HTMLCanvasElement | null;
    const dailyCanvas = document.getElementById('dailyChart') as HTMLCanvasElement | null;
    const moviesCanvas = document.getElementById('moviesChart') as HTMLCanvasElement | null;

    if (!sessionsCanvas || !dailyCanvas || !moviesCanvas) return;

    this.sessionsChart?.destroy();
    this.dailyChart?.destroy();
    this.moviesChart?.destroy();

    const getScale = (max: number) => {
      if (max <= 5) return { step: 1, max: 5 };
      if (max <= 20) return { step: 5, max: Math.ceil(max / 5) * 5 };
      return { step: 10, max: Math.ceil(max / 10) * 10 };
    };

    const mainMax = Math.max(
      this.stats.totalSessions,
      this.stats.activeRooms,
      this.stats.movies
    );

    const mainScale = getScale(mainMax);

    this.sessionsChart = new Chart(sessionsCanvas, {
      type: 'bar',
      data: {
        labels: ['Total Sessions', 'Active Rooms', 'Movies'],
        datasets: [
          {
            label: 'Quantidade',
            data: [
              this.stats.totalSessions,
              this.stats.activeRooms,
              this.stats.movies,
            ],
            backgroundColor: ['#4c6ef5', '#ff6b6b', '#ffa94d'],
          },
        ],
      },
      options: {
        responsive: true,
        plugins: { legend: { display: false } },
        scales: {
          x: {
            ticks: { color: '#ffffff' },
            grid: { display: false },
          },
          y: {
            beginAtZero: true,
            ticks: {
              color: '#ffffff',
              stepSize: mainScale.step,
            },
            max: mainScale.max,
          },
        },
      },
    });

    const others =
      this.stats.totalSessions - this.stats.todaysSessions >= 0
        ? this.stats.totalSessions - this.stats.todaysSessions
        : 0;

    const dayScale = getScale(Math.max(this.stats.todaysSessions, others));

    this.dailyChart = new Chart(dailyCanvas, {
      type: 'bar',
      data: {
        labels: ['Hoje', 'Outros Dias'],
        datasets: [
          {
            label: 'Sessions',
            data: [this.stats.todaysSessions, others],
            backgroundColor: ['#20c997', '#495057'],
          },
        ],
      },
      options: {
        responsive: true,
        plugins: { legend: { display: false } },
        scales: {
          x: {
            ticks: { color: '#ffffff' },
            grid: { display: false },
          },
          y: {
            beginAtZero: true,
            ticks: {
              color: '#ffffff',
              stepSize: dayScale.step,
            },
            max: dayScale.max,
          },
        },
      },
    });

    const movieScale = getScale(this.stats.movies);

    this.moviesChart = new Chart(moviesCanvas, {
      type: 'bar',
      data: {
        labels: ['Movies'],
        datasets: [
          {
            label: 'Total Movies',
            data: [this.stats.movies],
            backgroundColor: ['#4dabf7'],
          },
        ],
      },
      options: {
        responsive: true,
        plugins: { legend: { display: false } },
        scales: {
          x: {
            ticks: { color: '#ffffff' },
            grid: { display: false },
          },
          y: {
            beginAtZero: true,
            ticks: {
              color: '#ffffff',
              stepSize: movieScale.step,
            },
            max: movieScale.max,
          },
        },
      },
    });
  }

  logout(): void {
    localStorage.removeItem('token');
    this.router.navigate(['/login']);
  }

  goToProfile(): void {
    this.router.navigate(['/profile']);
  }
}
