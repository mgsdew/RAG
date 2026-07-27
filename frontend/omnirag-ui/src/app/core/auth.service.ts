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
  private readonly accessTokenKey = 'omnirag_access_token';
  private readonly tokenTypeKey = 'omnirag_token_type';
  private readonly expiresInSecondsKey = 'omnirag_expires_in_seconds';
  private readonly userEmailKey = 'omnirag_user_email';
  private readonly userRoleKey = 'omnirag_user_role';

  // store token key in session storage

  constructor(private httpClient: HttpClient) {}

  async login(credentials?: LoginCredentials): Promise<void> {
    const hasCredentials = !!credentials?.username && !!credentials?.password;
    const url = hasCredentials
      ? `${environment.apiBaseUrl}/api/Auth/login`
      : `${environment.apiBaseUrl}/api/Auth/dev-token`;

    let res: DevTokenResponse | null = null;

    try {
      res = await firstValueFrom(
        this.httpClient.post<DevTokenResponse>(url, hasCredentials ? credentials : {})
      );
    } catch (err: any) {
      throw err;
    }

    if (!res?.accessToken) {
      throw new Error('Login failed.');
    }

    sessionStorage.setItem(this.accessTokenKey, res.accessToken);
    if (res.tokenType) {
      sessionStorage.setItem(this.tokenTypeKey, res.tokenType);
    }
    if (typeof res.expiresInSeconds === 'number') {
      sessionStorage.setItem(this.expiresInSecondsKey, String(res.expiresInSeconds));
    }
    if (hasCredentials && credentials?.username) {
      sessionStorage.setItem(this.userEmailKey, credentials.username);
    }
    if (res.userRole?.length) {
      sessionStorage.setItem(this.userRoleKey, res.userRole.join(', '));
    } else {
      sessionStorage.removeItem(this.userRoleKey);
    }
  }

  isAuthenticated(): boolean {
    return !!sessionStorage.getItem(this.accessTokenKey);
  }

  async getToken(): Promise<string | null> {
    if (!this.isAuthenticated()) {
      return null;
    }

    return sessionStorage.getItem(this.accessTokenKey);
  }

  getSignedInEmail(): string {
    return sessionStorage.getItem(this.userEmailKey) ?? '';
  }

  getSignedInRole(): string {
    return sessionStorage.getItem(this.userRoleKey) ?? '';
  }

  logout(): void {
    sessionStorage.removeItem(this.accessTokenKey);
    sessionStorage.removeItem(this.tokenTypeKey);
    sessionStorage.removeItem(this.expiresInSecondsKey);
    sessionStorage.removeItem(this.userEmailKey);
    sessionStorage.removeItem(this.userRoleKey);
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
