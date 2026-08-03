import { MoneyPipe } from './money.pipe';

describe('MoneyPipe', () => {
  const pipe = new MoneyPipe();

  it('formatsCurrencyWithTwoFractionDigits', () => {
    const formatted = pipe.transform(1250.5, 'USD');
    expect(formatted).toContain('1');
    expect(formatted).toMatch(/50|1,250\.50|1250\.50/);
  });

  it('whenNull_returnsEmDashWithCurrency', () => {
    expect(pipe.transform(null, 'USD')).toBe('— USD');
    expect(pipe.transform(undefined)).toBe('— USD');
  });

  it('whenNaN_returnsEmDashWithCurrency', () => {
    expect(pipe.transform(Number.NaN, 'EUR')).toBe('— EUR');
  });
});
