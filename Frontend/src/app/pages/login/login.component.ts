import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';

import { UserService } from '../../services/user.service';
import { LoginRequest } from '../../models/user.model';
import { AuthTokenService } from '../../services/auth-token.service'; 

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css'],
})
export class LoginComponent {

  email = '';
  password = '';
  loading = false;
  errorMessage = '';

  private userService = inject(UserService);
  private router = inject(Router);
  private tokenWatcher = inject(AuthTokenService);

  handleSubmit() {

    if (this.loading) return;

    if (!this.email || !this.password) {
      this.errorMessage = 'Preenche todos os campos.';
      return;
    }

    if (!this.validateEmail(this.email)) {
      this.errorMessage = 'Formato de email inválido.';
      return;
    }

    this.loading = true;
    this.errorMessage = '';

    const data: LoginRequest = {
      email: this.email.trim(),
      password: this.password
    };

    this.userService.login(data).subscribe({

      next: (response) => {

        if (!response?.accessToken || !response?.refreshToken) {
          this.errorMessage = 'Resposta inválida do servidor.';
          this.loading = false;
          return;
        }

        localStorage.setItem('token', response.accessToken);
        localStorage.setItem('refreshToken', response.refreshToken);

        setTimeout(() => {
          this.tokenWatcher.startTokenWatcher();
          this.router.navigate(['/dashboard']);
        }, 50);
      },

      error: (err: HttpErrorResponse) => {
        this.errorMessage = this.resolveErrorMessage(err);
        this.loading = false;
      },

      complete: () => {
        this.loading = false;
      }
    });
  }

  goToRegister() {
    this.router.navigate(['/register']);
  }

  private validateEmail(email: string): boolean {
    return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
  }

  private resolveErrorMessage(err: HttpErrorResponse): string {

    if (err.status === 0) {
      return 'Não foi possível ligar ao servidor. Verifica a conexão.';
    }

    if (err.status === 400 || err.status === 401) {
      return err.error?.message || 'Email ou password incorretos.';
    }

    if (err.status === 404) {
      return 'Conta não encontrada.';
    }

    if (err.status >= 500) {
      return 'Erro no servidor. Tenta novamente mais tarde.';
    }

    return 'Falha inesperada no login.';
  }

}
