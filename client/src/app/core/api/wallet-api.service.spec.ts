import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { WalletApiService } from './wallet-api.service';

describe('WalletApiService', () => {
  let api: WalletApiService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    api = TestBed.inject(WalletApiService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('getBalance_getsWalletBalanceEndpoint', () => {
    api.getBalance().subscribe();

    const req = http.expectOne('/api/v1/wallets/me/balance');
    expect(req.request.method).toBe('GET');
    req.flush({ walletId: 'w-1', balance: 100, currency: 'USD', status: 'Active' });
  });

  it('changeStatus_postsStatusPayload', () => {
    api.changeStatus('Frozen').subscribe();

    const req = http.expectOne('/api/v1/wallets/me/status');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ status: 'Frozen' });
    req.flush({
      id: 'w-1',
      userId: 'u-1',
      balance: 100,
      currency: 'USD',
      status: 'Frozen',
      createdAtUtc: '2026-01-01T00:00:00Z',
    });
  });

  it('credit_postsAmount', () => {
    api.credit(50).subscribe();

    const req = http.expectOne('/api/v1/wallets/me/credit');
    expect(req.request.body).toEqual({ amount: 50 });
    req.flush({
      id: 'w-1',
      userId: 'u-1',
      balance: 150,
      currency: 'USD',
      status: 'Active',
      createdAtUtc: '2026-01-01T00:00:00Z',
    });
  });
});
