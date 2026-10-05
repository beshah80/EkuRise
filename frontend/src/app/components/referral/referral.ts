import { Component, signal, inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  standalone: true,
  selector: 'app-referral',
  imports: [],
  templateUrl: './referral.html',
  styleUrl: './referral.css'
})
export class Referral {
  private auth = inject(AuthService);
  private router = inject(Router);

  code = signal('');
  copied = signal(false);
  toast = signal('');

  ngOnInit() {
    this.auth.getProfile().subscribe({
      next: (p) => this.code.set(p.referralCode),
      error: () => {}
    });
  }

  copy() {
    navigator.clipboard.writeText(this.code()).then(() => {
      this.copied.set(true);
      this.showToast('Code copied to clipboard!');
      setTimeout(() => this.copied.set(false), 2500);
    });
  }

  share() {
    const text = `Join me on EkubCircle — the digital Ekub savings app! Use my referral code: ${this.code()}\nDownload: http://localhost:4200`;
    if (navigator.share) {
      navigator.share({ title: 'EkubCircle Referral', text });
    } else {
      navigator.clipboard.writeText(text).then(() => {
        this.showToast('Share link copied!');
      });
    }
  }

  showToast(msg: string) {
    this.toast.set(msg);
    setTimeout(() => this.toast.set(''), 2500);
  }

  goBack() { this.router.navigate(['/account']); }
}
