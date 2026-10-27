import { Component, signal } from '@angular/core';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../core/auth.service';
import { errorMessage } from '../core/error-message';

function passwordsMatch(control: AbstractControl): ValidationErrors | null {
  const password = control.get('password')?.value;
  const confirm = control.get('confirmPassword')?.value;
  return password && confirm && password !== confirm ? { passwordsMismatch: true } : null;
}

@Component({
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  template: `
    <section class="auth-card auth-wide">
      <span class="eyebrow">ÚNETE A SKILLHUB</span>
      <h1>Crear cuenta</h1>
      <p class="muted">Regístrate como participante para inscribirte en los talleres.</p>

      @if (error()) { <div class="alert alert-error" role="alert">{{ error() }}</div> }

      <form [formGroup]="form" (ngSubmit)="submit()" novalidate>
        <label>
          Nombre completo
          <input formControlName="fullName" autocomplete="name" placeholder="Tu nombre y apellidos">
          @if (form.controls.fullName.touched && form.controls.fullName.invalid) {
            <span class="field-error">Escribe tu nombre completo (mínimo 3 caracteres).</span>
          }
        </label>
        <label>
          Correo electrónico
          <input type="email" formControlName="email" autocomplete="email" placeholder="tu@correo.com">
          @if (form.controls.email.touched && form.controls.email.invalid) {
            <span class="field-error">Ingresa un correo válido.</span>
          }
        </label>
        <div class="form-row">
          <label>
            Contraseña
            <input type="password" formControlName="password" autocomplete="new-password" placeholder="Mínimo 10 caracteres">
            @if (form.controls.password.touched && form.controls.password.invalid) {
              <span class="field-error">Usa 10+ caracteres con mayúscula, minúscula, número y símbolo.</span>
            }
          </label>
          <label>
            Confirmar contraseña
            <input type="password" formControlName="confirmPassword" autocomplete="new-password" placeholder="Repite tu contraseña">
            @if (form.controls.confirmPassword.touched && form.hasError('passwordsMismatch')) {
              <span class="field-error">Las contraseñas no coinciden.</span>
            }
          </label>
        </div>
        <button type="submit" class="btn btn-primary btn-block" [disabled]="form.invalid || loading()">
          {{ loading() ? 'Creando cuenta…' : 'Crear cuenta' }}
        </button>
      </form>

      <p class="switch">¿Ya tienes cuenta? <a routerLink="/login">Inicia sesión</a></p>
    </section>
  `
})
export class RegisterComponent {
  readonly loading = signal(false);
  readonly error = signal('');
  readonly form = this.fb.nonNullable.group(
    {
      fullName: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(200)]],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [
        Validators.required,
        Validators.minLength(10),
        Validators.pattern(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z\d]).+$/)
      ]],
      confirmPassword: ['', Validators.required]
    },
    { validators: passwordsMatch }
  );

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
    const { fullName, email, password } = this.form.getRawValue();

    this.auth.register({ fullName, email, password }).subscribe({
      next: () => {
        this.loading.set(false);
        void this.router.navigate(['/my-enrollments']);
      },
      error: failure => {
        this.error.set(errorMessage(failure, 'No pudimos crear tu cuenta. Revisa tus datos.'));
        this.loading.set(false);
      }
    });
  }
}
