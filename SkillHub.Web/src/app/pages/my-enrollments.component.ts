import { DatePipe } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ApiService } from '../core/api.service';
import { errorMessage } from '../core/error-message';
import { Enrollment } from '../core/models';

@Component({
  standalone: true,
  imports: [DatePipe, RouterLink],
  template: `
    <section class="page-head">
      <div>
        <span class="eyebrow">MI ESPACIO</span>
        <h1>Mis inscripciones</h1>
        <p class="muted">Consulta tus talleres, cancela una inscripción o descarga tu constancia.</p>
      </div>
      <a class="btn btn-secondary" routerLink="/">Explorar talleres</a>
    </section>

    @if (notice()) { <div class="alert alert-success" role="status">{{ notice() }}</div> }
    @if (error()) { <div class="alert alert-error" role="alert">{{ error() }}</div> }

    @if (loading()) {
      <p class="muted" role="status">Cargando inscripciones…</p>
    } @else {
      <h2 class="section-title">Inscripciones activas</h2>
      @if (active().length === 0) {
        <p class="empty-state">No tienes inscripciones activas. <a routerLink="/">Explora los talleres disponibles</a>.</p>
      } @else {
        <div class="enrollment-list">
          @for (enrollment of active(); track enrollment.id) {
            <article class="enrollment-card">
              <div class="date-badge" aria-hidden="true">
                <span>{{ enrollment.startAt | date:'MMM' }}</span>
                <strong>{{ enrollment.startAt | date:'dd' }}</strong>
              </div>
              <div class="enrollment-main">
                <div class="enrollment-top">
                  <span class="code">{{ enrollment.workshopCode }}</span>
                  <span class="badge badge-active">Activa</span>
                </div>
                <h3>{{ enrollment.workshopTitle }}</h3>
                <dl class="meta">
                  <div><dt>Instructor</dt><dd>{{ enrollment.instructorName }}</dd></div>
                  <div><dt>Inicio</dt><dd>{{ enrollment.startAt | date:'EEEE d MMM y, HH:mm' }}</dd></div>
                  <div><dt>Fin</dt><dd>{{ enrollment.endAt | date:'EEEE d MMM y, HH:mm' }}</dd></div>
                  <div><dt>Inscripción</dt><dd>{{ enrollment.enrolledAt | date:'d MMM y, HH:mm' }}</dd></div>
                </dl>
              </div>
              <div class="enrollment-actions">
                <button type="button" class="btn btn-secondary btn-sm" [disabled]="busyId() === enrollment.id" (click)="download(enrollment)">
                  Descargar constancia
                </button>
                <button type="button" class="btn btn-danger btn-sm" [disabled]="busyId() === enrollment.id" (click)="cancel(enrollment)">
                  Cancelar inscripción
                </button>
              </div>
            </article>
          }
        </div>
      }

      @if (cancelled().length > 0) {
        <h2 class="section-title">Historial de cancelaciones</h2>
        <div class="enrollment-list">
          @for (enrollment of cancelled(); track enrollment.id) {
            <article class="enrollment-card enrollment-cancelled">
              <div class="enrollment-main">
                <div class="enrollment-top">
                  <span class="code">{{ enrollment.workshopCode }}</span>
                  <span class="badge badge-cancelled">Cancelada</span>
                </div>
                <h3>{{ enrollment.workshopTitle }}</h3>
                <p class="muted small">
                  Cancelada el {{ enrollment.cancelledAt | date:'d MMM y, HH:mm' }}
                  @if (enrollment.cancellationReason) { · {{ enrollment.cancellationReason }} }
                </p>
              </div>
            </article>
          }
        </div>
      }
    }
  `
})
export class MyEnrollmentsComponent implements OnInit {
  readonly enrollments = signal<Enrollment[]>([]);
  readonly loading = signal(true);
  readonly error = signal('');
  readonly notice = signal('');
  readonly busyId = signal('');

  constructor(private readonly api: ApiService) {}

  ngOnInit(): void {
    this.load();
  }

  active(): Enrollment[] {
    return this.enrollments().filter(enrollment => enrollment.status === 'Active');
  }

  cancelled(): Enrollment[] {
    return this.enrollments().filter(enrollment => enrollment.status === 'Cancelled');
  }

  load(): void {
    this.loading.set(true);
    this.error.set('');
    this.api.myEnrollments().subscribe({
      next: enrollments => {
        this.enrollments.set(enrollments);
        this.loading.set(false);
      },
      error: failure => {
        this.error.set(errorMessage(failure, 'No pudimos cargar tus inscripciones.'));
        this.loading.set(false);
      }
    });
  }

  cancel(enrollment: Enrollment): void {
    const confirmed = window.confirm(`¿Cancelar tu inscripción en "${enrollment.workshopTitle}"?`);
    if (!confirmed) return;

    this.busyId.set(enrollment.id);
    this.error.set('');
    this.notice.set('');
    this.api.cancel(enrollment.id, 'Cancelled from the SkillHub web portal').subscribe({
      next: () => {
        this.busyId.set('');
        this.notice.set('La inscripción fue cancelada y el cupo quedó liberado.');
        this.load();
      },
      error: failure => {
        this.busyId.set('');
        this.error.set(errorMessage(failure, 'No pudimos cancelar la inscripción.'));
      }
    });
  }

  download(enrollment: Enrollment): void {
    this.busyId.set(enrollment.id);
    this.error.set('');
    this.api.certificate(enrollment.id).subscribe({
      next: blob => {
        const url = URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `skillhub-${enrollment.workshopCode}.pdf`;
        link.click();
        URL.revokeObjectURL(url);
        this.busyId.set('');
      },
      error: failure => {
        this.busyId.set('');
        this.error.set(errorMessage(failure, 'No pudimos descargar la constancia.'));
      }
    });
  }
}
