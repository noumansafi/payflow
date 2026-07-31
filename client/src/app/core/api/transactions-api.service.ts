import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PagedResult } from '../models/transfer.models';
import { TransactionItem } from '../models/activity.models';

@Injectable({ providedIn: 'root' })
export class TransactionsApiService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/transactions`;

  list(options?: {
    page?: number;
    pageSize?: number;
    direction?: string;
    status?: string;
  }): Observable<PagedResult<TransactionItem>> {
    let params = new HttpParams()
      .set('page', options?.page ?? 1)
      .set('pageSize', options?.pageSize ?? 20);

    if (options?.direction) {
      params = params.set('direction', options.direction);
    }
    if (options?.status) {
      params = params.set('status', options.status);
    }

    return this.http.get<PagedResult<TransactionItem>>(this.base, { params });
  }

  getById(id: string): Observable<TransactionItem> {
    return this.http.get<TransactionItem>(`${this.base}/${id}`);
  }
}
