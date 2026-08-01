import { TransactionItem } from '../../core/models/activity.models';

export interface TransactionGroup {
  label: string;
  items: TransactionItem[];
}

function startOfDay(date: Date): number {
  return new Date(date.getFullYear(), date.getMonth(), date.getDate()).getTime();
}

function groupLabel(iso: string, now = new Date()): string {
  const date = new Date(iso);
  const day = startOfDay(date);
  const today = startOfDay(now);
  const yesterday = today - 86_400_000;
  const weekAgo = today - 6 * 86_400_000;

  if (day === today) {
    return 'Today';
  }
  if (day === yesterday) {
    return 'Yesterday';
  }
  if (day >= weekAgo) {
    return 'Earlier this week';
  }
  return new Intl.DateTimeFormat(undefined, {
    month: 'long',
    year: date.getFullYear() === now.getFullYear() ? undefined : 'numeric',
  }).format(date);
}

/** Group transactions newest-first into Today / Yesterday / Earlier this week / month. */
export function groupTransactionsByDate(items: TransactionItem[]): TransactionGroup[] {
  const groups = new Map<string, TransactionItem[]>();
  const order: string[] = [];

  for (const item of items) {
    const label = groupLabel(item.createdAtUtc);
    if (!groups.has(label)) {
      groups.set(label, []);
      order.push(label);
    }
    groups.get(label)!.push(item);
  }

  return order.map((label) => ({ label, items: groups.get(label)! }));
}

/**
 * Activity row title.
 * API currently returns counterpartyWalletId only (no display name), so we keep
 * direction-first copy. Prefer a short note when present; counterparty names are
 * a follow-up once the API exposes them (or wallet→beneficiary mapping).
 */
export function counterpartyLabel(tx: TransactionItem): string {
  const note = tx.note?.trim();
  if (note) {
    return note.length > 40 ? `${note.slice(0, 37)}…` : note;
  }

  if (tx.direction === 'Received') {
    return 'Money received';
  }
  if (tx.direction === 'Sent') {
    return 'Money sent';
  }
  return tx.transactionType || 'Transfer';
}

export function formatTxTime(iso: string): string {
  return new Intl.DateTimeFormat(undefined, {
    hour: 'numeric',
    minute: '2-digit',
  }).format(new Date(iso));
}
