import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuditLogItem } from '../models/activity.models';
import { PagedResult } from '../models/transfer.models';

@Injectable({ providedIn: 'root' })
export class AuditApiService {
  private readonly http = inject(HttpClient);

  list(page = 1, pageSize = 20): Observable<PagedResult<AuditLogItem>> {
    const params = new HttpParams().set('page', page).set('pageSize', pageSize);
    return this.http.get<PagedResult<AuditLogItem>>(`${environment.apiBaseUrl}/admin/audit-logs`, {
      params,
    });
  }
}
