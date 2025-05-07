import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { AuthenticationService } from '../services/authentication.service';

export const guestGuard: CanActivateFn = () => {
  const authService = inject(AuthenticationService);
  const router = inject(Router);

  if (!authService.isLoggedIn) {
    return true;
  }

  router.navigate(['/chats']);
  return false;
};
