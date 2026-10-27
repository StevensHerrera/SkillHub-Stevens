import { Component, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../core/auth.service';
import { errorMessage } from '../core/error-message';

@Component({
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  template: `
    <section class="auth-card">
      <span class="eyebrow">BIENVENIDO DE NUEVO</span>
      <h1>Iniciar sesión</h1>
      <p class="muted">Ingresa para inscribirte en talleres y consultar tus constancias.</p>

      @if (error()) { <div class="alert alert-error" role="alert">{{ error() }}</div> }

      <form [formGroup]="form" (ngSubmit)="submit()" novalidate>
        <label>
          Correo electrónico
          <input type="email" formControlName="email" autocomplete="username" placeholder="tu@correo.com" autofocus>
          @if (form.controls.email.touched && form.controls.email.invalid) {
            <span class="field-error">Ingresa un correo válido.</span>
          }
        </label>
        <label>
          Contraseña
          <input type="password" formControlName="password" autocomplete="current-password" placeholder="Tu contraseña">
          @if (form.controls.password.touched && form.controls.password.invalid) {
            <span class="field-error">Escribe tu contraseña.</span>
          }
        </label>
        <button type="submit" class="btn btn-primary btn-block" [disabled]="form.invalid || loading()">
          {{ loading() ? 'Ingresando…' : 'Iniciar sesión' }}
        </button>
      </form>

      <p class="switch">¿Aún no tienes cuenta? <a routerLink="/register">Regístrate</a></p>
    </section>
  `
})
export class LoginComponent {
  readonly loading = signal(false);
  readonly error = signal('');
  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', Validators.required]
  });

  constructor(
    private readonly fb: FormBuilder,
    private readonly auth: AuthService,
    private readonly router: Router
  ) {}

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    this.error.set('');
    const { email, password } = this.form.getRawValue();

    this.auth.login(email, password).subscribe({
      next: () => {
        this.loading.set(false);
        void this.router.navigate([this.auth.isParticipant() ? '/my-enrollments' : '/']);
      },
      error: failure => {
        this.error.set(errorMessage(failure, 'No pudimos iniciar sesión. Revisa tus datos.'));
        this.loading.set(false);
      }
    });
  }
}
