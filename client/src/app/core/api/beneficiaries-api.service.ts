import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Beneficiary, BeneficiaryCandidate, PagedResult } from '../models/transfer.models';

@Injectable({ providedIn: 'root' })
export class BeneficiariesApiService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/beneficiaries`;

  list(page = 1, pageSize = 50): Observable<PagedResult<Beneficiary>> {
    const params = new HttpParams().set('page', page).set('pageSize', pageSize);
    return this.http.get<PagedResult<Beneficiary>>(this.base, { params });
  }

  lookup(userId: string): Observable<BeneficiaryCandidate> {
    const params = new HttpParams().set('userId', userId);
    return this.http.get<BeneficiaryCandidate>(`${this.base}/lookup`, { params });
  }

  add(beneficiaryUserId: string, displayName?: string): Observable<Beneficiary> {
    return this.http.post<Beneficiary>(this.base, { beneficiaryUserId, displayName });
  }

  remove(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }
}
