import { Component, signal, inject } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { TokenService } from '../../services/token.service';
import { UserProfile } from '../../models/models';
import { ToastService } from '../../services/toast.service';

@Component({
  standalone: true,
  selector: 'app-account',
  imports: [RouterLink],
  templateUrl: './account.html',
  styleUrl: './account.css'
})
export class Account {
  private auth = inject(AuthService);
  private tokenService = inject(TokenService);
  private router = inject(Router);

  private toast = inject(ToastService);
  profile = signal<UserProfile | null>(null);
  loading = signal(true);

  ngOnInit() {
    this.auth.getProfile().subscribe({
      next: (p) => { this.profile.set(p); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  go(path: string) {
    this.router.navigate([path]);
  }

  logout() {
    this.tokenService.removeToken();
    this.toast.info('You have been logged out');
    this.router.navigate(['/login']);
  }
}
