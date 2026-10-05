import { Component, signal, inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  standalone: true,
  selector: 'app-account-language',
  imports: [],
  templateUrl: './account-language.html',
  styleUrl: './account-language.css'
})
export class AccountLanguage {
  private auth = inject(AuthService);
  private router = inject(Router);

  selected = signal(0);
  success = signal('');
  error = signal('');

  ngOnInit() {
    this.auth.getProfile().subscribe({
      next: (p) => this.selected.set(p.preferredLanguage),
      error: () => {}
    });
  }

  select(lang: number) {
    this.selected.set(lang);
    this.auth.updateProfile({ preferredLanguage: lang }).subscribe({
      next: () => this.success.set(lang === 0 ? 'Language set to English' : 'ቋንቋ ወደ አማርኛ ተቀናብሯል'),
      error: (err: any) => this.error.set(err.error?.title || 'Failed to update language')
    });
  }

  goBack() { this.router.navigate(['/account']); }
}
