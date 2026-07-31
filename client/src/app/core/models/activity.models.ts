export interface TransactionItem {
  id: string;
  referenceNumber: string;
  direction: string;
  counterpartyWalletId: string;
  amount: number;
  fee: number;
  status: string;
  transactionType: string;
  note: string | null;
  createdAtUtc: string;
  completedAtUtc: string | null;
}

export interface NotificationItem {
  id: string;
  title: string;
  body: string;
  type: string;
  isRead: boolean;
  relatedEntityId: string | null;
  createdAtUtc: string;
}

export interface AuditLogItem {
  id: string;
  actorUserId: string | null;
  action: string;
  entityType: string;
  entityId: string | null;
  metadata: string | null;
  ipAddress: string | null;
  createdAtUtc: string;
}
