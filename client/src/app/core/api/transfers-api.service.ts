import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { TransferResult } from '../models/transfer.models';

@Injectable({ providedIn: 'root' })
export class TransfersApiService {
  private readonly http = inject(HttpClient);

  send(receiverUserId: string, amount: number, note?: string): Observable<TransferResult> {
    return this.http.post<TransferResult>(`${environment.apiBaseUrl}/transfers`, {
      receiverUserId,
      amount,
      note,
    });
  }
}
