import { TestBed } from '@angular/core/testing';
import { ToastService } from './toast.service';

describe('ToastService', () => {
  let toasts: ToastService;

  beforeEach(() => {
    vi.useFakeTimers();
    TestBed.configureTestingModule({});
    toasts = TestBed.inject(ToastService);
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  it('success_pushesToastOntoQueue', () => {
    toasts.success('Transfer sent');
    expect(toasts.toasts()).toEqual([
      expect.objectContaining({ message: 'Transfer sent', tone: 'success' }),
    ]);
  });

  it('dismiss_removesToastById', () => {
    toasts.error('Nope');
    const id = toasts.toasts()[0].id;

    toasts.dismiss(id);
    expect(toasts.toasts()).toEqual([]);
  });

  it('autoDismisses_afterTimeout', () => {
    toasts.info('Saved');
    expect(toasts.toasts()).toHaveLength(1);

    vi.advanceTimersByTime(4200);
    expect(toasts.toasts()).toHaveLength(0);
  });
});
