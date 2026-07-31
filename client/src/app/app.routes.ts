import { Routes } from '@angular/router';
import { adminGuard, authGuard, guestGuard } from './core/auth/auth.guards';
import { ForgotPasswordPage } from './features/auth/forgot-password-page';
import { LoginPage } from './features/auth/login-page';
import { RegisterPage } from './features/auth/register-page';
import { ResetPasswordPage } from './features/auth/reset-password-page';
import { VerifyEmailPage } from './features/auth/verify-email-page';
import { HomePage } from './features/home/home-page';
import { PlaceholderPage } from './features/placeholder/placeholder-page';
import { WalletPage } from './features/wallet/wallet-page';
import { AuthLayout } from './layout/auth-layout/auth-layout';
import { Shell } from './layout/shell/shell';

const authChildren = (component: Routes[number]['component'], title: string): Routes => [
  { path: '', component, title },
];

export const routes: Routes = [
  {
    path: 'login',
    component: AuthLayout,
    canActivate: [guestGuard],
    children: authChildren(LoginPage, 'Sign in · PayFlow'),
  },
  {
    path: 'register',
    component: AuthLayout,
    canActivate: [guestGuard],
    children: authChildren(RegisterPage, 'Register · PayFlow'),
  },
  {
    path: 'forgot-password',
    component: AuthLayout,
    canActivate: [guestGuard],
    children: authChildren(ForgotPasswordPage, 'Forgot password · PayFlow'),
  },
  {
    path: 'reset-password',
    component: AuthLayout,
    canActivate: [guestGuard],
    children: authChildren(ResetPasswordPage, 'Reset password · PayFlow'),
  },
  {
    path: 'verify-email',
    component: AuthLayout,
    canActivate: [guestGuard],
    children: authChildren(VerifyEmailPage, 'Verify email · PayFlow'),
  },
  {
    path: '',
    component: Shell,
    canActivate: [authGuard],
    children: [
      { path: '', component: HomePage, title: 'Home · PayFlow' },
      {
        path: 'transfer',
        component: PlaceholderPage,
        data: {
          title: 'Send money',
          description: 'Lookup a recipient, confirm who they are, then send.',
        },
        title: 'Send · PayFlow',
      },
      {
        path: 'transactions',
        component: PlaceholderPage,
        data: {
          title: 'Activity',
          description: 'Your transfer history with filters and reference lookup.',
        },
        title: 'Activity · PayFlow',
      },
      {
        path: 'beneficiaries',
        component: PlaceholderPage,
        data: {
          title: 'People',
          description: 'Save frequent PayFlow recipients after a quick lookup.',
        },
        title: 'People · PayFlow',
      },
      {
        path: 'notifications',
        component: PlaceholderPage,
        data: {
          title: 'Alerts',
          description: 'In-app notifications from transfers and account events.',
        },
        title: 'Alerts · PayFlow',
      },
      { path: 'wallet', component: WalletPage, title: 'Wallet · PayFlow' },
      {
        path: 'profile',
        component: PlaceholderPage,
        data: {
          title: 'Profile',
          description: 'Your account, password, and sign out.',
        },
        title: 'Profile · PayFlow',
      },
      {
        path: 'admin/audit-logs',
        canActivate: [adminGuard],
        component: PlaceholderPage,
        data: {
          title: 'Audit logs',
          description: 'Admin-only security trail for demos and reviews.',
        },
        title: 'Audit · PayFlow',
      },
    ],
  },
  { path: '**', redirectTo: 'login' },
];
