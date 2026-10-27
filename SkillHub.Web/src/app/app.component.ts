import { Component } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from './core/auth.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  template: `
    <div class="app">
      <header class="site-header">
        <div class="container header-inner">
          <a class="brand" routerLink="/" aria-label="SkillHub inicio">
            <span class="brand-mark">S</span>
            <span>SkillHub<small>Talleres presenciales</small></span>
          </a>

          <nav class="main-nav" aria-label="Navegación principal">
            <a routerLink="/" routerLinkActive="active" [routerLinkActiveOptions]="{ exact: true }">Talleres</a>
            @if (auth.isParticipant()) {
              <a routerLink="/my-enrollments" routerLinkActive="active">Mis inscripciones</a>
            }
            @if (auth.isEmployee()) {
              <a routerLink="/instructor" routerLinkActive="active">Inscritos</a>
            }
          </nav>

          <div class="session">
            @if (auth.isAuthenticated()) {
              <span class="user-name">{{ auth.session()?.fullName }}</span>
              @if (auth.isEmployee()) { <span class="role-badge">Empleado</span> }
              <button type="button" class="btn btn-ghost btn-sm" (click)="logout()">Cerrar sesión</button>
            } @else {
              <a class="link" routerLink="/login">Iniciar sesión</a>
              <a class="btn btn-primary btn-sm" routerLink="/register">Crear cuenta</a>
            }
          </div>
        </div>
      </header>

      <main class="container content">
        <router-outlet />
      </main>

      <footer class="site-footer">
        <div class="container">SkillHub · Centro de formación · Talleres presenciales · © {{ year }}</div>
      </footer>
    </div>
  `
})
export class AppComponent {
  readonly year = new Date().getFullYear();

  constructor(readonly auth: AuthService, private readonly router: Router) {}

  logout(): void {
    this.auth.logout();
    void this.router.navigate(['/']);
  }
}
