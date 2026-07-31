import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from './auth.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const token = auth.accessToken();
  const isAuthApi = /\/auth\/(login|register|refresh|password\/forgot|password\/reset|email\/verify)/.test(
    req.url,
  );

  const authReq =
    token && !isAuthApi
      ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
      : req;

  return next(authReq).pipe(
    catchError((error: unknown) => {
      if (
        !(error instanceof HttpErrorResponse) ||
        error.status !== 401 ||
        isAuthApi ||
        req.headers.has('X-Retry')
      ) {
        return throwError(() => error);
      }

      return auth.refreshAccessToken().pipe(
        switchMap((accessToken) => {
          if (!accessToken) {
            return throwError(() => error);
          }

          return next(
            req.clone({
              setHeaders: {
                Authorization: `Bearer ${accessToken}`,
                'X-Retry': '1',
              },
            }),
          );
        }),
      );
    }),
  );
};
