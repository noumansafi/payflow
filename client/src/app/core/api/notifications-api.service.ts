import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { NotificationItem } from '../models/activity.models';
import { PagedResult } from '../models/transfer.models';

@Injectable({ providedIn: 'root' })
export class NotificationsApiService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/notifications`;

  list(page = 1, pageSize = 20, isRead?: boolean): Observable<PagedResult<NotificationItem>> {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);
    if (isRead !== undefined) {
      params = params.set('isRead', isRead);
    }
    return this.http.get<PagedResult<NotificationItem>>(this.base, { params });
  }

  markRead(id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/${id}/read`, {});
  }

  markAllRead(): Observable<{ markedCount: number }> {
    return this.http.post<{ markedCount: number }>(`${this.base}/read-all`, {});
  }
}
