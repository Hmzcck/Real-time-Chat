import { HttpClient } from '@angular/common/http';
import { Injectable, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { BehaviorSubject, Observable, Subscription, tap, timer } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  LoginRequest,
  LoginResponse,
  RegisterRequest,
  RegisterResponse,
} from '../models/auth.models';

@Injectable({
  providedIn: 'root',
})
export class AuthenticationService {
  private apiUrl = `${environment.apiUrl}/auth`;
  private tokenExpiryTimer?: Subscription;
  private currentUserSubject = new BehaviorSubject<LoginResponse | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor(
    private http: HttpClient,
    private router: Router
  ) {
    const user = localStorage.getItem('currentUser');
    if (user) {
      const parsedUser = JSON.parse(user);
      this.currentUserSubject.next(parsedUser);
      this.startTokenExpiryTimer(parsedUser.token);
    }
  }

  login(credentials: LoginRequest): Observable<LoginResponse> {
    return this.http
      .post<LoginResponse>(`${this.apiUrl}/login`, credentials)
      .pipe(
        tap((user) => {
          localStorage.setItem('currentUser', JSON.stringify(user));
          this.currentUserSubject.next(user);
          this.startTokenExpiryTimer(user.token);
        })
      );
  }

  register(userData: RegisterRequest): Observable<RegisterResponse> {
    return this.http.post<RegisterResponse>(
      `${this.apiUrl}/register`,
      userData
    );
  }

  get currentUserValue(): LoginResponse | null {
    return this.currentUserSubject.value;
  }

  get isLoggedIn(): boolean {
    return !!this.currentUserValue?.token;
  }

  logout(): void {
    // Stop the token expiry timer
    if (this.tokenExpiryTimer) {
      this.tokenExpiryTimer.unsubscribe();
    }
    
    // Remove user from local storage
    localStorage.removeItem('currentUser');
    this.currentUserSubject.next(null);
    
    // Navigate to login
    this.router.navigate(['/login']);
  }

  private startTokenExpiryTimer(token: string): void {
    // Parse the JWT token to get expiration time
    const tokenData = this.parseJwt(token);
    if (!tokenData?.exp) return;

    // Calculate time until token expires
    const expiresIn = (tokenData.exp * 1000) - Date.now();
    const warningTime = environment.tokenExpiryNotificationTime * 1000;

    // Set timer to notify before token expires
    this.tokenExpiryTimer?.unsubscribe();
    this.tokenExpiryTimer = timer(expiresIn - warningTime).subscribe(() => {
      this.logout();
      alert('Your session is about to expire. Please log in again.');
    });
  }

  private parseJwt(token: string): any {
    try {
      const base64Url = token.split('.')[1];
      const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
      return JSON.parse(window.atob(base64));
    } catch (e) {
      return null;
    }
  }

  ngOnDestroy(): void {
    if (this.tokenExpiryTimer) {
      this.tokenExpiryTimer.unsubscribe();
    }
  }
}
