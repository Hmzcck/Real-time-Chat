import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthenticationService } from '../services/authentication.service';

export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthenticationService);
  const router = inject(Router);

  if (authService.isLoggedIn) {
    return true;
  }

  // Redirect to login page with return url
  router.navigate(['/login'], { queryParams: { returnUrl: state.url }});
  return false;
};
