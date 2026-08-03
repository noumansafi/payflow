import { HttpErrorResponse } from '@angular/common/http';
import { problemMessage, readProblem } from './problem-details';

describe('problem-details', () => {
  it('readProblem_whenHttpErrorWithBody_returnsProblem', () => {
    const error = new HttpErrorResponse({
      status: 400,
      error: { title: 'Bad Request', detail: 'Invalid amount', status: 400 },
    });

    expect(readProblem(error)).toEqual({
      title: 'Bad Request',
      detail: 'Invalid amount',
      status: 400,
    });
  });

  it('readProblem_whenNonHttpError_returnsNull', () => {
    expect(readProblem(new Error('boom'))).toBeNull();
  });

  it('problemMessage_prefersFirstValidationError', () => {
    const error = new HttpErrorResponse({
      status: 400,
      error: {
        title: 'Validation',
        detail: 'One or more errors',
        errors: {
          Amount: ['Amount must be greater than zero.'],
          Note: ['Note is too long.'],
        },
      },
    });

    expect(problemMessage(error)).toBe('Amount must be greater than zero.');
  });

  it('problemMessage_fallsBackToDetailThenTitleThenDefault', () => {
    expect(
      problemMessage(
        new HttpErrorResponse({
          status: 409,
          error: { detail: 'Insufficient funds.' },
        }),
      ),
    ).toBe('Insufficient funds.');

    expect(
      problemMessage(
        new HttpErrorResponse({
          status: 500,
          error: { title: 'Server Error' },
        }),
      ),
    ).toBe('Server Error');

    expect(problemMessage(new Error('x'), 'Custom fallback')).toBe('Custom fallback');
  });
});
