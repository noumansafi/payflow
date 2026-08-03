import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { TransfersApiService } from './transfers-api.service';

describe('TransfersApiService', () => {
  let api: TransfersApiService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    api = TestBed.inject(TransfersApiService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('send_postsReceiverAmountAndNote', () => {
    let result: unknown;
    api.send('user-2', 25.5, 'lunch').subscribe((r) => (result = r));

    const req = http.expectOne('/api/v1/transfers');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({
      receiverUserId: 'user-2',
      amount: 25.5,
      note: 'lunch',
    });
    req.flush({
      transactionId: 'tx-1',
      referenceNumber: 'PF-1',
      amount: 25.5,
      fee: 0,
      status: 'Completed',
      completedAtUtc: '2026-07-30T12:00:00Z',
    });

    expect(result).toMatchObject({ referenceNumber: 'PF-1', amount: 25.5 });
  });
});
