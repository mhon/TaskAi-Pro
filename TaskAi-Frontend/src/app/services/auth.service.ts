import { Injectable, PLATFORM_ID, inject } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { LoginResponse } from '../models/auth.model';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private platformId = inject(PLATFORM_ID)
  private http = inject(HttpClient);
  private apiUrl = 'http://localhost:5094/api/auth'; // Double-check your .NET port!

  login(credentials: any): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/login`, credentials).pipe(
      tap(res => {
        localStorage.setItem('token', res.token); // Save JWT to browser
      })
    );
  }

  register(userData: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/register`, userData);
  }

  getToken(): string | null {
    // Check if we are in the browser
    if (isPlatformBrowser(this.platformId)) {
      return localStorage.getItem('token');
    }
    return null; // Return null if on the server
  }

  logout() {
    localStorage.removeItem('token');
  }
}