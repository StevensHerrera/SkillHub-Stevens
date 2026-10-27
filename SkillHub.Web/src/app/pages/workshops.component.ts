import { DatePipe } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { ApiService } from '../core/api.service';
import { AuthService } from '../core/auth.service';
import { errorMessage } from '../core/error-message';
import { Workshop } from '../core/models';

@Component({
  standalone: true,
  imports: [DatePipe, FormsModule, RouterLink],
  template: `
    <section class="hero">
      <div class="hero-copy">
        <span class="eyebrow">SKILLHUB · APRENDE EN PERSONA</span>
        <h1>Talleres que <em>despiertan</em> tu curiosidad.</h1>
        <p class="hero-text">
          Fotografía, cocina, oratoria, diseño, música y marketing. Elige tu taller,
          reserva tu cupo y lleva tu aprendizaje al siguiente nivel.
        </p>
        <div class="hero-stats">
          <div class="hero-stat"><strong>{{ workshops().length }}</strong><span>talleres disponibles</span></div>
          <div class="hero-stat"><strong>{{ totalSeats() }}</strong><span>cupos libres</span></div>
        </div>
      </div>
      <div class="hero-art" aria-hidden="true">
        <span class="hero-ring"></span>
        <span class="hero-dot hero-dot-1"></span>
        <span class="hero-dot hero-dot-2"></span>
        <div class="hero-card">
          <span>Incluido</span>
          <strong>PDF</strong>
          <small>constancia de inscripción</small>
        </div>
      </div>
    </section>

    <section class="catalog-toolbar">
      <div>
        <span class="eyebrow">CATÁLOGO</span>
        <h2>Próximos talleres</h2>
        <p class="muted">{{ filtered().length }} talleres para explorar</p>
      </div>
      <label class="search-box">
        <svg viewBox="0 0 24 24" aria-hidden="true"><circle cx="10.8" cy="10.8" r="6.8"></circle><path d="m16 16 4.5 4.5"></path></svg>
        <input type="search" [(ngModel)]="search" placeholder="Buscar por taller, código o instructor" aria-label="Buscar talleres">
      </label>
    </section>

    @if (notice()) { <div class="alert alert-success" role="status">{{ notice() }}</div> }
    @if (error()) { <div class="alert alert-error" role="alert">{{ error() }}</div> }
    @if (auth.isEmployee()) {
      <div class="alert alert-info" role="note">
        Sesión de empleado: la creación de talleres, la importación de Excel y los reportes se gestionan desde Swagger/Postman.
      </div>
    }

    @if (loading()) {
      <p class="muted" role="status">Cargando talleres…</p>
    } @else if (filtered().length === 0) {
      <p class="empty-state">No encontramos talleres con ese criterio.</p>
    } @else {
      <div class="workshop-grid">
        @for (workshop of filtered(); track workshop.id) {
          <article class="workshop-card">
            <header class="workshop-top">
              <span class="code">{{ workshop.code }}</span>
              <span class="seats" [class.seats-empty]="workshop.availableSeats === 0">
                {{ workshop.availableSeats === 0 ? 'Sin cupos' : workshop.availableSeats + ' cupos disponibles' }}
              </span>
            </header>

            <h2>{{ workshop.title }}</h2>
            <p class="description">{{ workshop.description }}</p>

            <dl class="meta">
              <div><dt>Instructor</dt><dd>{{ workshop.instructorName }} · {{ workshop.instructorSpecialty }}</dd></div>
              <div><dt>Inicio</dt><dd>{{ workshop.startAt | date:'EEE d MMM y, HH:mm' }}</dd></div>
              <div><dt>Fin</dt><dd>{{ workshop.endAt | date:'EEE d MMM y, HH:mm' }}</dd></div>
            </dl>

            <div class="capacity">
              <div class="capacity-track"><span [style.width.%]="capacityPercent(workshop)"></span></div>
              <span class="capacity-label">{{ workshop.activeEnrollments }} de {{ workshop.capacity }} inscritos · {{ workshop.availableSeats }} disponibles</span>
            </div>

            @if (auth.isEmployee()) {
              <span class="hint">Los empleados no se inscriben como participantes.</span>
            } @else if (enrolledIds().has(workshop.id)) {
              <button type="button" class="btn btn-muted btn-block" disabled>Ya estás inscrito</button>
            } @else if (!auth.isAuthenticated()) {
              <a class="btn btn-primary btn-block" routerLink="/login">Inicia sesión para inscribirte</a>
            } @else {
              <button
                type="button"
                class="btn btn-primary btn-block"
                [disabled]="workshop.availableSeats === 0 || enrollingId() === workshop.id"
                (click)="enroll(workshop)">
                {{ enrollingId() === workshop.id ? 'Inscribiendo…' : (workshop.availableSeats === 0 ? 'Taller lleno' : 'Inscribirme') }}
              </button>
            }
          </article>
        }
      </div>
    }
  `
})
export class WorkshopsComponent implements OnInit {
  readonly workshops = signal<Workshop[]>([]);
  readonly enrolledIds = signal<Set<string>>(new Set());
  readonly loading = signal(true);
  readonly error = signal('');
  readonly notice = signal('');
  readonly enrollingId = signal('');
  search = '';

  constructor(
    private readonly api: ApiService,
    readonly auth: AuthService,
    private readonly router: Router
  ) {}

  ngOnInit(): void {
    this.load();
  }

  filtered(): Workshop[] {
    const query = this.search.trim().toLowerCase();
    if (!query) return this.workshops();
    return this.workshops().filter(workshop =>
      workshop.title.toLowerCase().includes(query) ||
      workshop.code.toLowerCase().includes(query) ||
      workshop.instructorName.toLowerCase().includes(query) ||
      workshop.instructorSpecialty.toLowerCase().includes(query)
    );
  }

  totalSeats(): number {
    return this.workshops().reduce((total, workshop) => total + workshop.availableSeats, 0);
  }

  capacityPercent(workshop: Workshop): number {
    if (workshop.capacity === 0) return 0;
    return Math.min(100, Math.round((workshop.activeEnrollments / workshop.capacity) * 100));
  }

  load(): void {
    this.loading.set(true);
    this.error.set('');
    this.api.workshops().subscribe({
      next: workshops => {
        this.workshops.set(workshops);
        this.loading.set(false);
        if (this.auth.isParticipant()) this.loadMine();
      },
      error: failure => {
        this.error.set(errorMessage(failure, 'No pudimos cargar los talleres.'));
        this.loading.set(false);
      }
    });
  }

  enroll(workshop: Workshop): void {
    if (!this.auth.isAuthenticated()) {
      void this.router.navigate(['/login']);
      return;
    }

    this.enrollingId.set(workshop.id);
    this.error.set('');
    this.notice.set('');

    this.api.enroll(workshop.id).subscribe({
      next: () => {
        this.enrollingId.set('');
        this.notice.set(`¡Quedaste inscrito en "${workshop.title}"!`);
        this.load();
      },
      error: failure => {
        this.enrollingId.set('');
        this.error.set(errorMessage(failure, 'No pudimos completar la inscripción.'));
      }
    });
  }

  private loadMine(): void {
    this.api.myEnrollments().subscribe({
      next: enrollments => {
        const active = enrollments
          .filter(enrollment => enrollment.status === 'Active')
          .map(enrollment => enrollment.workshopId);
        this.enrolledIds.set(new Set(active));
      }
    });
  }
}
