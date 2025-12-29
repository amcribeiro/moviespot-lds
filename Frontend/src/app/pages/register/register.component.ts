import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { UserService } from '../../services/user.service';
import { UserCreateDto } from '../../models/user.model';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent {
  name = '';
  email = '';
  password = '';
  phone = '';
  readonly role = 'Staff' as const;
  loading = false;
  errorMessage = '';

  private userService = inject(UserService);
  private router = inject(Router);

  handleSubmit() {
    if (!this.name || !this.email || !this.password) {
      this.errorMessage = 'Preenche todos os campos obrigatórios.';
      return;
    }

    this.loading = true;
    this.errorMessage = '';

    const data: UserCreateDto = {
      name: this.name,
      email: this.email,
      password: this.password,
      phone: this.phone,
      role: this.role,
    };

    this.userService.register(data).subscribe({
      next: (response) => {
        console.log('Staff registado com sucesso:', response);
        this.router.navigate(['/login']);
      },
      error: (err) => {
        this.errorMessage =
          err.error?.message || 'Erro ao criar conta. Tenta novamente.';
      },
      complete: () => {
        this.loading = false;
      },
    });
  }

  goToLogin() {
    this.router.navigate(['/login']);
  }
}
