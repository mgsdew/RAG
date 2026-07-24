import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { firstValueFrom } from 'rxjs';

interface DevTokenResponse {
  accessToken?: string;
  tokenType?: string;
  expiresInSeconds?: number;
  userRole?: string[];
}

export interface LoginCredentials {
  username: string;
  password: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  // store token key in session storage
  private readonly storageKey = 'omnirag.auth.token';
  private readonly expiryKey = 'omnirag.auth.expiry';
  private readonly usernameKey = 'omnirag.auth.username';
  private readonly rolesKey = 'omnirag.auth.roles';

  constructor(private httpClient: HttpClient) {}

  async login(credentials?: LoginCredentials): Promise<void> {
    const hasCredentials = !!credentials?.username && !!credentials?.password;
    const url = hasCredentials ? `${environment.apiBaseUrl}/api/Auth/login` : `${environment.apiBaseUrl}/api/Auth/dev-token`;
    let res: DevTokenResponse | null = null;
    try {
      res = hasCredentials
        ? await firstValueFrom(this.httpClient.post<DevTokenResponse>(url, credentials))
        : await firstValueFrom(this.httpClient.get<DevTokenResponse>(url));
    } catch (err: any) {
      console.error('Token endpoint request failed', err);
      throw new Error('Failed to acquire token from auth endpoint (request failed)');
    }

    const token = res?.accessToken ?? null;
    const expiresIn = res?.expiresInSeconds ?? 3600;

    if (!token) {
      console.error('Token endpoint returned unexpected payload', res);
      throw new Error('Failed to acquire token from auth endpoint (invalid response)');
    }

    const expiresAt = Date.now() + expiresIn * 1000;
    sessionStorage.setItem(this.storageKey, token);
    sessionStorage.setItem(this.expiryKey, String(expiresAt));
    if (credentials?.username) {
      sessionStorage.setItem(this.usernameKey, credentials.username);
    }

    const roles = res?.userRole ?? [];
    sessionStorage.setItem(this.rolesKey, JSON.stringify(Array.isArray(roles) ? roles : [roles]));

    // After token is stored, call backend to sync/create the user profile.
    // The AuthInterceptor will attach the Bearer token to this request.
    // try {
    //   const profile = await firstValueFrom(this.httpClient.post(`${environment.apiBaseUrl}/api/users/login`, {}));
    //   if (profile) {
    //     sessionStorage.setItem('omnirag.auth.profile', JSON.stringify(profile));
    //   }
    // } catch (err) {
    //   // Don't block login on profile sync failure; log for debugging
    //   console.error('Profile sync failed', err);
    // }
  }

  logout(): void {
    sessionStorage.removeItem(this.storageKey);
    sessionStorage.removeItem(this.expiryKey);
    sessionStorage.removeItem(this.usernameKey);
    sessionStorage.removeItem(this.rolesKey);
  }

  isAuthenticated(): boolean {
    const token = sessionStorage.getItem(this.storageKey);
    const expiry = Number(sessionStorage.getItem(this.expiryKey) ?? '0');
    return !!token && expiry > Date.now();
  }

  async getToken(): Promise<string | null> {
    if (!this.isAuthenticated()) return null;
    return sessionStorage.getItem(this.storageKey);
  }

  // decode JWT payload to read roles claim
  getRolesFromIdToken(): string[] {
    const raw = sessionStorage.getItem(this.rolesKey);
    if (raw) {
      try {
        const parsed = JSON.parse(raw);
        return Array.isArray(parsed) ? parsed : [parsed];
      } catch {
        return [];
      }
    }

    const token = sessionStorage.getItem(this.storageKey);
    if (!token) return [];
    const parts = token.split('.');
    if (parts.length < 2) return [];
    try {
      const payload = JSON.parse(atob(parts[1].replace(/-/g, '+').replace(/_/g, '/')));
      const roles = payload['roles'] ?? payload['role'] ?? [];
      return Array.isArray(roles) ? roles : [roles];
    } catch {
      return [];
    }
  }

  getUsername(): string {
    return sessionStorage.getItem(this.usernameKey) ?? '';
  }

  getRolesText(): string {
    return this.getRolesFromIdToken().join(', ');
  }

  getProfile(): any | null {
    const raw = sessionStorage.getItem('omnirag.auth.profile');
    if (!raw) return null;
    try {
      return JSON.parse(raw);
    } catch {
      return null;
    }
  }
}
