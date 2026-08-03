import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { AuthService } from './auth.service';
import { authInterceptor } from './auth.interceptor';

describe('authInterceptor', () => {
  let http: HttpClient;
  let httpMock: HttpTestingController;
  let auth: {
    accessToken: ReturnType<typeof vi.fn>;
    refreshAccessToken: ReturnType<typeof vi.fn>;
  };

  beforeEach(() => {
    auth = {
      accessToken: vi.fn(() => 'access-token'),
      refreshAccessToken: vi.fn(() => of('refreshed-token')),
    };

    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting(),
        { provide: AuthService, useValue: auth },
      ],
    });

    http = TestBed.inject(HttpClient);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('attachesBearerToken_forProtectedRequests', () => {
    http.get('/api/v1/wallets/me').subscribe();

    const req = httpMock.expectOne('/api/v1/wallets/me');
    expect(req.request.headers.get('Authorization')).toBe('Bearer access-token');
    req.flush({});
  });

  it('skipsBearer_forAuthLoginUrl', () => {
    http.post('/api/v1/auth/login', {}).subscribe();

    const req = httpMock.expectOne('/api/v1/auth/login');
    expect(req.request.headers.has('Authorization')).toBe(false);
    req.flush({});
  });

  it('on401_refreshesOnceAndRetriesWithXRetry', () => {
    let body: unknown;
    http.get('/api/v1/wallets/me/balance').subscribe((r) => (body = r));

    const first = httpMock.expectOne('/api/v1/wallets/me/balance');
    expect(first.request.headers.get('Authorization')).toBe('Bearer access-token');
    first.flush({ title: 'Unauthorized' }, { status: 401, statusText: 'Unauthorized' });

    expect(auth.refreshAccessToken).toHaveBeenCalledTimes(1);

    const retry = httpMock.expectOne('/api/v1/wallets/me/balance');
    expect(retry.request.headers.get('Authorization')).toBe('Bearer refreshed-token');
    expect(retry.request.headers.get('X-Retry')).toBe('1');
    retry.flush({ balance: 10 });

    expect(body).toEqual({ balance: 10 });
  });

  it('on401_whenAlreadyRetried_doesNotRefreshAgain', () => {
    let status: number | undefined;
    http.get('/api/v1/wallets/me', { headers: { 'X-Retry': '1' } }).subscribe({
      error: (err: { status: number }) => (status = err.status),
    });

    const req = httpMock.expectOne('/api/v1/wallets/me');
    req.flush({ title: 'Unauthorized' }, { status: 401, statusText: 'Unauthorized' });

    expect(auth.refreshAccessToken).not.toHaveBeenCalled();
    expect(status).toBe(401);
  });
});
