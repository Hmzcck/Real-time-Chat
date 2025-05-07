import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthenticationService } from '../services/authentication.service';
import { catchError, throwError } from 'rxjs';

export const authInterceptor: HttpInterceptorFn = (request, next) => {
  const authService = inject(AuthenticationService);
  
  // Add auth header if user is logged in
  const currentUser = authService.currentUserValue;
  if (currentUser?.token) {
    request = request.clone({
      setHeaders: {
        Authorization: `Bearer ${currentUser.token}`
      }
    });
  }

  return next(request).pipe(
    catchError(error => {
      if (error.status === 401) {
        // Auto logout if 401 response returned from api
        authService.logout();
        location.reload();
      }
      
      const errorMessage = error.error?.detail || error.statusText;
      return throwError(() => errorMessage);
    })
  );
};
