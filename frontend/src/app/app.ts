import { Component, signal, inject } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive, Router } from '@angular/router';
import { TokenService } from './services/token.service';

@Component({
  standalone: true,
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  private tokenService = inject(TokenService);
  private router = inject(Router);

  showNav = signal(false);

  constructor() {
    this.router.events.subscribe(() => {
      const url = this.router.url;
      const publicRoutes = ['/login', '/register'];
      const isPublic = publicRoutes.some(r => url.startsWith(r));
      this.showNav.set(this.tokenService.isAuthenticated() && !isPublic);
    });
  }
}
