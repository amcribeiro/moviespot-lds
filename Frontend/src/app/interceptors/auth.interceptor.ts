import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { catchError, switchMap, throwError } from 'rxjs';

export const AuthInterceptor: HttpInterceptorFn = (req, next) => {
  const http = inject(HttpClient);
  const token = localStorage.getItem('token');
  let cloned = req;

  if (token) {
    cloned = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`,
      },
    });
  }

  return next(cloned).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401) {
        const refreshToken = localStorage.getItem('refreshToken');
        if (!refreshToken) {
          localStorage.removeItem('token');
          localStorage.removeItem('refreshToken');
          location.href = '/login';
          return throwError(() => error);
        }

        return http
          .post<{ accessToken: string; refreshToken: string }>(
            'http://localhost:5000/User/refresh',
            `"${refreshToken}"`, 
            { headers: { 'Content-Type': 'application/json' } }
          )
          .pipe(
            switchMap((res) => {
              localStorage.setItem('token', res.accessToken);
              localStorage.setItem('refreshToken', res.refreshToken);

              const newReq = cloned.clone({
                setHeaders: {
                  Authorization: `Bearer ${res.accessToken}`,
                },
              });

              return next(newReq);
            }),
            catchError((err) => {
              localStorage.removeItem('token');
              localStorage.removeItem('refreshToken');
              location.href = '/login';
              return throwError(() => err);
            })
          );
      }

      return throwError(() => error);
    })
  );
};
