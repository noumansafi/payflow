import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Wallet, WalletBalance, WalletStatus } from '../models/wallet.models';

@Injectable({ providedIn: 'root' })
export class WalletApiService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/wallets`;

  getMine(): Observable<Wallet> {
    return this.http.get<Wallet>(`${this.base}/me`);
  }

  getBalance(): Observable<WalletBalance> {
    return this.http.get<WalletBalance>(`${this.base}/me/balance`);
  }

  changeStatus(status: WalletStatus): Observable<Wallet> {
    return this.http.post<Wallet>(`${this.base}/me/status`, { status });
  }

  credit(amount: number): Observable<Wallet> {
    return this.http.post<Wallet>(`${this.base}/me/credit`, { amount });
  }
}
