import { HttpClient } from '@angular/common/http';
import { Injectable, computed, signal } from '@angular/core';
import { tap } from 'rxjs';
import { environment } from '../../environments/environment';
import { AuthResponse, RegisterRequest } from './models';

const SESSION_KEY = 'skillhub.session';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly sessionState = signal<AuthResponse | null>(this.readSession());
  private expiryTimer?: ReturnType<typeof setTimeout>;

  readonly session = this.sessionState.asReadonly();
  readonly token = computed(() => this.sessionState()?.accessToken ?? null);
  readonly isAuthenticated = computed(() => {
    const session = this.sessionState();
    return !!session && new Date(session.expiresAt).getTime() > Date.now();
  });
  readonly isEmployee = computed(() =>
    this.isAuthenticated() && this.sessionState()?.roles.includes('Employee') === true);
  readonly isParticipant = computed(() =>
    this.isAuthenticated() && this.sessionState()?.roles.includes('Participant') === true);

  constructor(private readonly http: HttpClient) {
    this.armExpiry(this.sessionState());
  }

  login(email: string, password: string) {
    return this.http.post<AuthResponse>(`${environment.apiUrl}/auth/login`, { email, password })
      .pipe(tap(session => this.save(session)));
  }

  register(data: RegisterRequest) {
    return this.http.post<AuthResponse>(`${environment.apiUrl}/auth/register`, data)
      .pipe(tap(session => this.save(session)));
  }

  logout(): void {
    if (this.expiryTimer) clearTimeout(this.expiryTimer);
    sessionStorage.removeItem(SESSION_KEY);
    this.sessionState.set(null);
  }

  private save(session: AuthResponse): void {
    sessionStorage.setItem(SESSION_KEY, JSON.stringify(session));
    this.sessionState.set(session);
    this.armExpiry(session);
  }

  private armExpiry(session: AuthResponse | null): void {
    if (this.expiryTimer) clearTimeout(this.expiryTimer);
    if (!session) return;
    const delay = new Date(session.expiresAt).getTime() - Date.now();
    if (delay <= 0) {
      this.logout();
      return;
    }
    this.expiryTimer = setTimeout(() => this.logout(), delay);
  }

  private readSession(): AuthResponse | null {
    try {
      const raw = sessionStorage.getItem(SESSION_KEY);
      if (!raw) return null;
      const value = JSON.parse(raw) as AuthResponse;
      if (new Date(value.expiresAt).getTime() <= Date.now()) {
        sessionStorage.removeItem(SESSION_KEY);
        return null;
      }
      return value;
    } catch {
      return null;
    }
  }
}
