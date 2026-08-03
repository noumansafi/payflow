import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { AuthService } from './auth.service';
import { AuthResponse, AuthUser } from '../models/auth.models';

const REFRESH_KEY = 'payflow.refreshToken';

describe('AuthService', () => {
  let service: AuthService;
  let http: HttpTestingController;
  let router: Router;

  const user: AuthUser = {
    id: 'user-1',
    email: 'a@payflow.test',
    firstName: 'Ada',
    lastName: 'Lovelace',
    role: 'User',
    isEmailVerified: true,
  };

  const authResponse: AuthResponse = {
    user,
    tokens: {
      accessToken: 'access-1',
      refreshToken: 'refresh-1',
      accessTokenExpiresAtUtc: '2099-01-01T00:00:00Z',
    },
  };

  beforeEach(() => {
    localStorage.clear();

    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
    });

    service = TestBed.inject(AuthService);
    http = TestBed.inject(HttpTestingController);
    router = TestBed.inject(Router);
  });

  afterEach(() => {
    http.verify();
    localStorage.clear();
  });

  it('login_appliesSessionAndStoresRefreshToken', () => {
    let result: AuthResponse | undefined;
    service.login('a@payflow.test', 'Password1!').subscribe((r) => (result = r));

    const req = http.expectOne('/api/v1/auth/login');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ email: 'a@payflow.test', password: 'Password1!' });
    req.flush(authResponse);

    expect(result).toEqual(authResponse);
    expect(service.accessToken()).toBe('access-1');
    expect(service.user()).toEqual(user);
    expect(service.isAuthenticated()).toBe(true);
    expect(localStorage.getItem(REFRESH_KEY)).toBe('refresh-1');
  });

  it('isAdmin_whenRoleAdmin_returnsTrue', () => {
    service.login('admin@payflow.test', 'Password1!').subscribe();
    http.expectOne('/api/v1/auth/login').flush({
      ...authResponse,
      user: { ...user, role: 'Admin' },
    });

    expect(service.isAdmin()).toBe(true);
  });

  it('hydrate_whenNoRefreshToken_returnsNull', () => {
    let result: AuthUser | null | undefined = user;
    service.hydrate().subscribe((r) => (result = r));

    expect(result).toBeNull();
    http.expectNone(() => true);
  });

  it('hydrate_whenRefreshSucceeds_loadsCurrentUser', () => {
    localStorage.setItem(REFRESH_KEY, 'refresh-1');

    let result: AuthUser | null | undefined;
    service.hydrate().subscribe((r) => (result = r));

    http.expectOne('/api/v1/auth/refresh').flush({
      accessToken: 'access-2',
      refreshToken: 'refresh-2',
      accessTokenExpiresAtUtc: '2099-01-01T00:00:00Z',
    });
    http.expectOne('/api/v1/auth/me').flush(user);

    expect(result).toEqual(user);
    expect(service.accessToken()).toBe('access-2');
    expect(localStorage.getItem(REFRESH_KEY)).toBe('refresh-2');
  });

  it('refreshAccessToken_coalescesInFlightRequests', () => {
    localStorage.setItem(REFRESH_KEY, 'refresh-1');

    const first: string[] = [];
    const second: string[] = [];
    service.refreshAccessToken().subscribe((t) => first.push(t ?? ''));
    service.refreshAccessToken().subscribe((t) => second.push(t ?? ''));

    const requests = http.match('/api/v1/auth/refresh');
    expect(requests).toHaveLength(1);
    requests[0].flush({
      accessToken: 'access-shared',
      refreshToken: 'refresh-shared',
      accessTokenExpiresAtUtc: '2099-01-01T00:00:00Z',
    });

    expect(first).toEqual(['access-shared']);
    expect(second).toEqual(['access-shared']);
  });

  it('logout_clearsSessionAndNavigatesToLogin', () => {
    const navigateSpy = vi.spyOn(router, 'navigateByUrl').mockResolvedValue(true);
    localStorage.setItem(REFRESH_KEY, 'refresh-1');
    service.login('a@payflow.test', 'Password1!').subscribe();
    http.expectOne('/api/v1/auth/login').flush(authResponse);

    service.logout().subscribe();
    http.expectOne('/api/v1/auth/logout').flush(null);

    expect(service.isAuthenticated()).toBe(false);
    expect(service.user()).toBeNull();
    expect(localStorage.getItem(REFRESH_KEY)).toBeNull();
    expect(navigateSpy).toHaveBeenCalledWith('/login');
  });
});
