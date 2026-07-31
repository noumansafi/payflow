import { HttpErrorResponse } from '@angular/common/http';
import { ProblemDetails } from '../models/auth.models';

export function readProblem(error: unknown): ProblemDetails | null {
  if (!(error instanceof HttpErrorResponse) || !error.error || typeof error.error !== 'object') {
    return null;
  }

  return error.error as ProblemDetails;
}

export function problemMessage(error: unknown, fallback = 'Something went wrong. Please try again.'): string {
  const problem = readProblem(error);
  if (!problem) {
    return fallback;
  }

  if (problem.errors) {
    const first = Object.values(problem.errors).flat()[0];
    if (first) {
      return first;
    }
  }

  return problem.detail || problem.title || fallback;
}
