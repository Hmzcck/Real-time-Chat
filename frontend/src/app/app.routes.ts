import { Routes } from '@angular/router';
import { authGuard } from './guards/auth.guard';

export const routes: Routes = [
    {
        path: '',
        redirectTo: 'login',
        pathMatch: 'full'
    },
    {
        path: 'login',
        loadComponent: () => import('./pages/login/login.component').then(c => c.LoginComponent)
    },
    {
        path: 'register',
        loadComponent: () => import('./pages/register/register.component').then(c => c.RegisterComponent)
    },
    {
        path: '**',
        loadComponent: () => import('./pages/not-found/not-found.component').then(c => c.NotFoundComponent)
    }
];
