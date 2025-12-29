import { Routes } from '@angular/router';
import { LoginComponent } from './pages/login/login.component';
import { RegisterComponent } from './pages/register/register.component';
import { DashboardComponent } from './pages/dashboard/dashboard.component';
import { AuthGuard } from './guards/auth.guard';

import { CinemaComponent } from './pages/cinema/cinema.component';
import { CinemaFormComponent } from './pages/cinema-form/cinema-form.component';
import { HallFormComponent } from './pages/hall-form/hall-form.component';
import { VoucherComponent } from './pages/voucher/voucher.component';

import { SessionComponent } from './pages/session/session.component';
import { SessionFormComponent } from './pages/session-form/session-form.component';
import { MovieComponent } from './pages/movies/movie.component';
import { ProfileComponent } from './pages/profile/profile.component';

export const routes: Routes = [
  { path: 'login', component: LoginComponent },

  { path: '', redirectTo: 'login', pathMatch: 'full' },
  
  { path: 'register', component: RegisterComponent },

  { path: 'dashboard', component: DashboardComponent, canActivate: [AuthGuard] },

  { path: 'cinemas', component: CinemaComponent, canActivate: [AuthGuard] },

  { path: 'cinemas/create', component: CinemaFormComponent, canActivate: [AuthGuard] },

  { path: 'cinemas/edit/:cinemaId', component: CinemaFormComponent, canActivate: [AuthGuard] },

  { path: 'cinemas/:cinemaId/halls/create', component: HallFormComponent, canActivate: [AuthGuard] },

  { path: 'cinemas/:cinemaId/halls/:hallId/edit', component: HallFormComponent, canActivate: [AuthGuard] },

  { path: 'sessions', component: SessionComponent, canActivate: [AuthGuard] },

  { path: 'sessions/create', component: SessionFormComponent, canActivate: [AuthGuard] },

  { path: 'sessions/edit/:id', component: SessionFormComponent, canActivate: [AuthGuard] },

  { path: 'voucher', component: VoucherComponent, canActivate: [AuthGuard] },

  { path: 'movies', component: MovieComponent, canActivate: [AuthGuard] },

  { path: 'profile', component: ProfileComponent, canActivate: [AuthGuard] }
];
