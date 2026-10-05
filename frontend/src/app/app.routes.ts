import { Routes } from '@angular/router';
import { authGuard, adminGuard } from './guards/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: '/home', pathMatch: 'full' },
  { path: 'login', loadComponent: () => import('./components/login/login').then(m => m.Login) },
  { path: 'register', loadComponent: () => import('./components/register/register').then(m => m.Register) },
  { path: 'register/verify', loadComponent: () => import('./components/verify/verify').then(m => m.Verify) },
  { path: 'home', loadComponent: () => import('./components/home/home').then(m => m.Home), canActivate: [authGuard] },
  { path: 'catalog/:id', loadComponent: () => import('./components/sub-category-detail/sub-category-detail').then(m => m.SubCategoryDetail), canActivate: [authGuard] },
  { path: 'circles/new', loadComponent: () => import('./components/create-circle/create-circle').then(m => m.CreateCircle), canActivate: [authGuard] },
  { path: 'circles/:id', loadComponent: () => import('./components/circle-detail/circle-detail').then(m => m.CircleDetail), canActivate: [authGuard] },
  { path: 'circles/:id/rounds', loadComponent: () => import('./components/round-history/round-history').then(m => m.RoundHistory), canActivate: [authGuard] },
  { path: 'circles/:id/rounds/:roundId', loadComponent: () => import('./components/round-detail/round-detail').then(m => m.RoundDetail), canActivate: [authGuard] },
  { path: 'notifications', loadComponent: () => import('./components/notifications/notifications').then(m => m.Notifications), canActivate: [authGuard] },
  { path: 'account', loadComponent: () => import('./components/account/account').then(m => m.Account), canActivate: [authGuard] },
  { path: 'account/edit', loadComponent: () => import('./components/edit-profile/edit-profile').then(m => m.EditProfile), canActivate: [authGuard] },
  { path: 'account/security', loadComponent: () => import('./components/account-security/account-security').then(m => m.AccountSecurity), canActivate: [authGuard] },
  { path: 'account/language', loadComponent: () => import('./components/account-language/account-language').then(m => m.AccountLanguage), canActivate: [authGuard] },
  { path: 'account/my-ekubs', loadComponent: () => import('./components/my-ekubs/my-ekubs').then(m => m.MyEkubs), canActivate: [authGuard] },
  { path: 'account/feedback', loadComponent: () => import('./components/account-feedback/account-feedback').then(m => m.AccountFeedback), canActivate: [authGuard] },
  { path: 'account/stories', loadComponent: () => import('./components/success-stories/success-stories').then(m => m.SuccessStories), canActivate: [authGuard] },
  { path: 'account/referral', loadComponent: () => import('./components/referral/referral').then(m => m.Referral), canActivate: [authGuard] },
  { path: 'account/about', loadComponent: () => import('./components/about/about').then(m => m.About), canActivate: [authGuard] },
  { path: 'admin', loadComponent: () => import('./components/admin-dashboard/admin-dashboard').then(m => m.AdminDashboard), canActivate: [authGuard, adminGuard] },
  { path: 'admin/catalog', redirectTo: '/admin' },
  { path: '**', redirectTo: '/home' }
];
