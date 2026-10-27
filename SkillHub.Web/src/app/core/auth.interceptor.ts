import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { environment } from '../../environments/environment';
import { AuthService } from './auth.service';

export const authInterceptor: HttpInterceptorFn = (request, next) => {
  const auth = inject(AuthService);
  const router = inject(Router);
  const token = auth.token();

  if (!token || !request.url.startsWith(environment.apiUrl)) return next(request);

  return next(request.clone({ setHeaders: { Authorization: `Bearer ${token}` } })).pipe(
    catchError(error => {
      if (error.status === 401) {
        auth.logout();
        void router.navigate(['/login']);
      }
      return throwError(() => error);
    })
  );
};
