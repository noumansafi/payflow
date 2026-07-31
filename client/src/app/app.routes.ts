import { Routes } from '@angular/router';
import { Shell } from './layout/shell/shell';
import { HomePage } from './features/home/home-page';
import { PlaceholderPage } from './features/placeholder/placeholder-page';

export const routes: Routes = [
  {
    path: '',
    component: Shell,
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
      {
        path: 'wallet',
        component: PlaceholderPage,
        data: {
          title: 'Wallet',
          description: 'Check status, freeze/activate, and (in Development) credit funds.',
        },
        title: 'Wallet · PayFlow',
      },
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
        component: PlaceholderPage,
        data: {
          title: 'Audit logs',
          description: 'Admin-only security trail for demos and reviews.',
        },
        title: 'Audit · PayFlow',
      },
    ],
  },
  {
    path: 'login',
    component: PlaceholderPage,
    data: {
      title: 'Sign in',
      description: 'Auth screens land in the next commit.',
    },
    title: 'Sign in · PayFlow',
  },
  {
    path: 'register',
    component: PlaceholderPage,
    data: {
      title: 'Create account',
      description: 'Auth screens land in the next commit.',
    },
    title: 'Register · PayFlow',
  },
  { path: '**', redirectTo: '' },
];
