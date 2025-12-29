import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { SessionService } from '../../services/session.service';
import { SessionResponseDto } from '../../models/session.model';
import { SidebarComponent } from "../sidebar/sidebar.component";

@Component({
  selector: 'app-session',
  standalone: true,
  imports: [CommonModule, SidebarComponent],
  templateUrl: './session.component.html',
  styleUrls: ['./session.component.css'],
})
export class SessionComponent implements OnInit {

  sessions: SessionResponseDto[] = [];
  loading = true;
  errorMessage = '';

  private sessionService = inject(SessionService);
  private router = inject(Router);

  ngOnInit(): void {
    this.loadSessions();
  }

  loadSessions() {
    this.sessionService.getAllSessions().subscribe({
      next: (data) => {
        this.sessions = data;
        this.loading = false;
        if (this.sessions.length === 0) {
          this.errorMessage = 'Ainda não existem sessões clica em + Nova Sessão.';
        }
      },
      error: (err) => {
        console.error(err);
        this.errorMessage = 'Ocorreu um erro ao carregar as sessões.';
        this.loading = false;
      }
    });
  }

  goToCreate() {
    this.router.navigate(['/sessions/create']);
  }

  goToEdit(id: number) {
    this.router.navigate(['/sessions', 'edit', id]);
  }

  deleteSession(id: number) {
    if (!confirm('Tens a certeza que queres apagar esta sessão?')) return;

    this.sessionService.deleteSession(id).subscribe({
      next: () => this.loadSessions(),
      error: () => alert('Erro ao apagar sessão.')
    });
  }
}
