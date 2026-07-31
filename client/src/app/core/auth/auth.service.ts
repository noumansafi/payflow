import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import {
  Observable,
  catchError,
  finalize,
  map,
  of,
  shareReplay,
  switchMap,
  tap,
  throwError,
} from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  AuthResponse,
  AuthTokens,
  AuthUser,
  RegisterResponse,
} from '../models/auth.models';

const REFRESH_KEY = 'payflow.refreshToken';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly base = environment.apiBaseUrl;

  private readonly userSignal = signal<AuthUser | null>(null);
  private readonly accessTokenSignal = signal<string | null>(null);
  private refreshInFlight$: Observable<string | null> | null = null;

  readonly user = this.userSignal.asReadonly();
  readonly accessToken = this.accessTokenSignal.asReadonly();
  readonly isAuthenticated = computed(() => !!this.accessTokenSignal());
  readonly isAdmin = computed(() => this.userSignal()?.role === 'Admin');

  hydrate(): Observable<AuthUser | null> {
    const refreshToken = localStorage.getItem(REFRESH_KEY);
    if (!refreshToken) {
      return of(null);
    }

    return this.refreshAccessToken().pipe(
      switchMap((token) => {
        if (!token) {
          return of(null);
        }

        return this.loadMe().pipe(
          catchError(() => {
            this.clearSession();
            return of(null);
          }),
        );
      }),
      catchError(() => {
        this.clearSession();
        return of(null);
      }),
    );
  }

  login(email: string, password: string): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${this.base}/auth/login`, { email, password })
      .pipe(tap((response) => this.applySession(response)));
  }

  register(payload: {
    email: string;
    password: string;
    firstName: string;
    lastName: string;
  }): Observable<RegisterResponse> {
    return this.http.post<RegisterResponse>(`${this.base}/auth/register`, payload);
  }

  logout(): Observable<void> {
    const refreshToken = localStorage.getItem(REFRESH_KEY);
    return this.http.post<void>(`${this.base}/auth/logout`, { refreshToken }).pipe(
      catchError(() => of(void 0)),
      finalize(() => {
        this.clearSession();
        void this.router.navigateByUrl('/login');
      }),
      map(() => void 0),
    );
  }

  forgotPassword(email: string): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.base}/auth/password/forgot`, { email });
  }

  resetPassword(token: string, newPassword: string): Observable<void> {
    return this.http.post<void>(`${this.base}/auth/password/reset`, {
      token,
      newPassword,
    });
  }

  verifyEmail(token: string): Observable<void> {
    return this.http.post<void>(`${this.base}/auth/email/verify`, { token });
  }

  refreshAccessToken(): Observable<string | null> {
    if (this.refreshInFlight$) {
      return this.refreshInFlight$;
    }

    const refreshToken = localStorage.getItem(REFRESH_KEY);
    if (!refreshToken) {
      return of(null);
    }

    this.refreshInFlight$ = this.http
      .post<AuthTokens>(`${this.base}/auth/refresh`, { refreshToken })
      .pipe(
        tap((tokens) => {
          this.accessTokenSignal.set(tokens.accessToken);
          localStorage.setItem(REFRESH_KEY, tokens.refreshToken);
        }),
        map((tokens) => tokens.accessToken),
        catchError((error) => {
          this.clearSession();
          return throwError(() => error);
        }),
        finalize(() => {
          this.refreshInFlight$ = null;
        }),
        shareReplay(1),
      );

    return this.refreshInFlight$;
  }

  private loadMe(): Observable<AuthUser> {
    return this.http
      .get<AuthUser>(`${this.base}/auth/me`)
      .pipe(tap((user) => this.userSignal.set(user)));
  }

  private applySession(response: AuthResponse): void {
    this.userSignal.set(response.user);
    this.accessTokenSignal.set(response.tokens.accessToken);
    localStorage.setItem(REFRESH_KEY, response.tokens.refreshToken);
  }

  private clearSession(): void {
    this.userSignal.set(null);
    this.accessTokenSignal.set(null);
    localStorage.removeItem(REFRESH_KEY);
  }
}
