import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Subscription, timer } from 'rxjs';

declare global {
  interface Window {
    Cypress?: unknown;
  }
}

@Injectable({ providedIn: 'root' })
export class AuthTokenService {

  private http = inject(HttpClient);
  private refreshTimer?: Subscription;

  private isCypress(): boolean {
    return typeof window !== 'undefined' && !!window.Cypress;
  }

  decodeToken(token: string): Record<string, unknown> | null {
    try {
      const payload = token.split('.')[1];
      return JSON.parse(atob(payload));
    } catch {
      return null;
    }
  }

  getUserId(): number | null {
    const token = localStorage.getItem('token');
    if (!token) return null;

    const payload = this.decodeToken(token);
    const id = payload?.['id'] as number | undefined;
    const userId = payload?.['userId'] as number | undefined;
    return id ?? userId ?? null;
  }

  getUser(): Record<string, unknown> | null {
    const token = localStorage.getItem('token');
    if (!token) return null;

    return this.decodeToken(token);
  }

  isLoggedIn(): boolean {
    return !!localStorage.getItem('token');
  }

  logout() {
  localStorage.removeItem('token');
  localStorage.removeItem('refreshToken');
  window.dispatchEvent(new Event('forceLogout'));
}

  startTokenWatcher() {
  if (this.isCypress()) return;

  const token = localStorage.getItem('token');
  if (!token) return;

  const payload = this.decodeToken(token);
  const exp = payload?.['exp'] as number | undefined;
  if (!exp) return;

  const expiresAt = exp * 1000;
  const now = Date.now();

  const msUntilRefresh = expiresAt - now - 60_000;

  this.refreshTimer?.unsubscribe();

  if (msUntilRefresh <= 0) {

    this.refreshTimer = timer(2000).subscribe(() => this.refreshToken());
    return;
  }

  this.refreshTimer = timer(msUntilRefresh).subscribe(() => {
    this.refreshToken();
  });
}


  stopTokenWatcher() {
    this.refreshTimer?.unsubscribe();
  }

  private refreshToken() {
    const refreshToken = localStorage.getItem('refreshToken');
    if (!refreshToken) return;

    if (this.isCypress()) {
      return;
    }

    this.http
      .post<{ accessToken: string; refreshToken: string }>(
        'http://localhost:5000/User/refresh',
        `"${refreshToken}"`,
        { headers: { 'Content-Type': 'application/json' } }
      )
      .subscribe({
        next: (res) => {
          localStorage.setItem('token', res.accessToken);
          localStorage.setItem('refreshToken', res.refreshToken);
          this.startTokenWatcher();
        },
        error: () => {
          this.logout();
        },
      });
  }
}
