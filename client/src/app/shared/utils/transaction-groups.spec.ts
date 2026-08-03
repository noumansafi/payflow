import { TransactionItem } from '../../core/models/activity.models';
import { counterpartyLabel, formatTxTime, groupTransactionsByDate } from './transaction-groups';

function tx(partial: Partial<TransactionItem>): TransactionItem {
  return {
    id: '1',
    referenceNumber: 'PF-1',
    direction: 'Sent',
    counterpartyWalletId: 'w-2',
    counterpartyName: null,
    amount: 10,
    fee: 0,
    status: 'Completed',
    transactionType: 'Transfer',
    note: null,
    createdAtUtc: new Date().toISOString(),
    completedAtUtc: null,
    ...partial,
  };
}

describe('transaction-groups', () => {
  it('groupTransactionsByDate_bucketsTodayAndYesterday', () => {
    const now = new Date(2026, 6, 30, 15, 0, 0);
    vi.setSystemTime(now);

    const todayIso = new Date(2026, 6, 30, 10, 0, 0).toISOString();
    const yesterdayIso = new Date(2026, 6, 29, 10, 0, 0).toISOString();

    const groups = groupTransactionsByDate([
      tx({ id: 't1', createdAtUtc: todayIso }),
      tx({ id: 't2', createdAtUtc: yesterdayIso }),
    ]);

    expect(groups.map((g) => g.label)).toEqual(['Today', 'Yesterday']);
    expect(groups[0].items.map((i) => i.id)).toEqual(['t1']);
    expect(groups[1].items.map((i) => i.id)).toEqual(['t2']);

    vi.useRealTimers();
  });

  it('counterpartyLabel_prefersNameThenNoteThenDirection', () => {
    expect(
      counterpartyLabel(tx({ direction: 'Sent', counterpartyName: 'Receiver Two' })),
    ).toBe('To Receiver Two');
    expect(
      counterpartyLabel(tx({ direction: 'Received', counterpartyName: 'Sender One' })),
    ).toBe('From Sender One');
    expect(counterpartyLabel(tx({ counterpartyName: null, note: 'lunch' }))).toBe('lunch');
    expect(counterpartyLabel(tx({ direction: 'Received', counterpartyName: null, note: null }))).toBe(
      'Money received',
    );
  });

  it('formatTxTime_returnsLocalizedTime', () => {
    const formatted = formatTxTime('2026-07-30T14:05:00.000Z');
    expect(formatted.length).toBeGreaterThan(0);
    expect(formatted).toMatch(/\d/);
  });
});
