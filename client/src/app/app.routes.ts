import { Routes } from '@angular/router';
import { adminGuard, authGuard, guestGuard } from './core/auth/auth.guards';
import { AuditLogsPage } from './features/admin/audit-logs-page';
import { ForgotPasswordPage } from './features/auth/forgot-password-page';
import { LoginPage } from './features/auth/login-page';
import { RegisterPage } from './features/auth/register-page';
import { ResetPasswordPage } from './features/auth/reset-password-page';
import { VerifyEmailPage } from './features/auth/verify-email-page';
import { BeneficiariesPage } from './features/beneficiaries/beneficiaries-page';
import { HomePage } from './features/home/home-page';
import { NotificationsPage } from './features/notifications/notifications-page';
import { ProfilePage } from './features/profile/profile-page';
import { TransactionsPage } from './features/transactions/transactions-page';
import { TransferPage } from './features/transfer/transfer-page';
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
      { path: 'transfer', component: TransferPage, title: 'Send · PayFlow' },
      { path: 'transactions', component: TransactionsPage, title: 'Activity · PayFlow' },
      { path: 'beneficiaries', component: BeneficiariesPage, title: 'People · PayFlow' },
      { path: 'notifications', component: NotificationsPage, title: 'Alerts · PayFlow' },
      { path: 'wallet', component: WalletPage, title: 'Wallet · PayFlow' },
      { path: 'profile', component: ProfilePage, title: 'Profile · PayFlow' },
      {
        path: 'admin/audit-logs',
        canActivate: [adminGuard],
        component: AuditLogsPage,
        title: 'Audit · PayFlow',
      },
    ],
  },
  { path: '**', redirectTo: 'login' },
];
