export type WalletStatus = 'Active' | 'Frozen' | 'Closed';

export interface Wallet {
  id: string;
  userId: string;
  balance: number;
  currency: string;
  status: WalletStatus | string;
  createdAtUtc: string;
}

export interface WalletBalance {
  walletId: string;
  balance: number;
  currency: string;
  status: WalletStatus | string;
}
