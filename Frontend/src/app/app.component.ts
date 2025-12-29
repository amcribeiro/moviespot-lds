import { Component, inject } from '@angular/core';
import { RouterOutlet, Router } from '@angular/router';
import { AuthTokenService } from './services/auth-token.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {

  private tokenWatcher = inject(AuthTokenService);
  private router = inject(Router);

  constructor() {
    if (this.tokenWatcher.isLoggedIn()) {
      this.tokenWatcher.startTokenWatcher();
    }

    window.addEventListener('forceLogout', () => {
      this.router.navigate(['/login']);
    });
  }

}
