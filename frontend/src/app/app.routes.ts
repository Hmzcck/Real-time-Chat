import { Routes } from '@angular/router';
import { guestGuard } from './guards/guest.guard';

export const routes: Routes = [
    {
        path: '',
        redirectTo: 'login',
        pathMatch: 'full'
    },
    {
        path: 'login',
        loadComponent: () => import('./pages/login/login.component').then(c => c.LoginComponent),
        canActivate: [guestGuard]
    },
    {
        path: 'register',
        loadComponent: () => import('./pages/register/register.component').then(c => c.RegisterComponent),
        canActivate: [guestGuard]
    },
    {
        path: '**',
        loadComponent: () => import('./pages/not-found/not-found.component').then(c => c.NotFoundComponent)
    }
];
