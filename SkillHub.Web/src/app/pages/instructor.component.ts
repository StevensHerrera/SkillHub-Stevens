import { DatePipe } from '@angular/common';
import { Component, OnInit, computed, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../core/api.service';
import { errorMessage } from '../core/error-message';
import { Workshop, WorkshopEnrollment } from '../core/models';

@Component({
  standalone: true,
  imports: [DatePipe, FormsModule],
  template: `
    <section class="page-head">
      <div>
        <span class="eyebrow">PANEL DE INSTRUCTOR</span>
        <h1>Inscritos por taller</h1>
        <p class="muted">Selecciona un taller para consultar quiénes están inscritos.</p>
      </div>
      <div class="employee-summary">
        <div><strong>{{ workshops().length }}</strong><span>talleres</span></div>
        <div><strong>{{ totalActive() }}</strong><span>inscripciones activas</span></div>
      </div>
    </section>

    @if (error()) { <div class="alert alert-error" role="alert">{{ error() }}</div> }

    @if (loading()) {
      <p class="muted" role="status">Cargando talleres…</p>
    } @else {
      <div class="employee-layout">
        <aside class="workshop-picker" aria-label="Lista de talleres">
          <label class="search-box">
            <svg viewBox="0 0 24 24" aria-hidden="true"><circle cx="10.8" cy="10.8" r="6.8"></circle><path d="m16 16 4.5 4.5"></path></svg>
            <input type="search" [(ngModel)]="search" placeholder="Buscar taller o código" aria-label="Buscar talleres">
          </label>

          <div class="workshop-options">
            @for (workshop of filteredWorkshops(); track workshop.id) {
              <button
                type="button"
                class="workshop-option"
                [class.selected]="selectedId() === workshop.id"
                (click)="select(workshop)">
                <span class="option-top">
                  <span class="code">{{ workshop.code }}</span>
                  <span class="option-count">{{ workshop.activeEnrollments }}/{{ workshop.capacity }}</span>
                </span>
                <strong>{{ workshop.title }}</strong>
                <span class="option-meta">{{ workshop.startAt | date:'d MMM y, HH:mm' }} · {{ workshop.instructorName }}</span>
              </button>
            } @empty {
              <p class="muted small">No hay talleres con ese criterio.</p>
            }
          </div>
        </aside>

        <section class="attendee-panel" aria-live="polite">
          @if (selected(); as workshop) {
            <header class="attendee-head">
              <div>
                <span class="code">{{ workshop.code }}</span>
                <h2>{{ workshop.title }}</h2>
                <p class="muted small">
                  {{ workshop.instructorName }} · {{ workshop.startAt | date:'EEEE d MMM y, HH:mm' }}
                </p>
              </div>
              <div class="attendee-capacity">
                <strong>{{ workshop.activeEnrollments }}</strong>
                <span>de {{ workshop.capacity }} cupos</span>
              </div>
            </header>

            @if (loadingEnrollments()) {
              <p class="muted" role="status">Cargando inscritos…</p>
            } @else if (enrollments().length === 0) {
              <p class="empty-state">Este taller todavía no tiene inscripciones.</p>
            } @else {
              <div class="table-wrap">
                <table class="attendee-table">
                  <thead>
                    <tr>
                      <th>Participante</th>
                      <th>Correo</th>
                      <th>Estado</th>
                      <th>Inscripción</th>
                      <th></th>
                    </tr>
                  </thead>
                  <tbody>
                    @for (enrollment of enrollments(); track enrollment.id) {
                      <tr>
                        <td><strong>{{ enrollment.participantName }}</strong></td>
                        <td><a [href]="'mailto:' + enrollment.participantEmail">{{ enrollment.participantEmail }}</a></td>
                        <td>
                          <span class="badge" [class.badge-active]="enrollment.status === 'Active'" [class.badge-cancelled]="enrollment.status === 'Cancelled'">
                            {{ enrollment.status === 'Active' ? 'Activa' : 'Cancelada' }}
                          </span>
                          @if (enrollment.cancellationReason) {
                            <small class="cancel-reason">{{ enrollment.cancellationReason }}</small>
                          }
                        </td>
                        <td>{{ enrollment.enrolledAt | date:'d MMM y, HH:mm' }}</td>
                        <td class="row-actions">
                          @if (enrollment.status === 'Active') {
                            <button
                              type="button"
                              class="btn btn-secondary btn-sm"
                              [disabled]="busyId() === enrollment.id"
                              (click)="download(enrollment)">
                              Constancia
                            </button>
                          }
                        </td>
                      </tr>
                    }
                  </tbody>
                </table>
              </div>
            }
          } @else {
            <p class="empty-state">Selecciona un taller para ver sus inscritos.</p>
          }
        </section>
      </div>
    }
  `
})
export class InstructorComponent implements OnInit {
  readonly workshops = signal<Workshop[]>([]);
  readonly selectedId = signal<string | null>(null);
  readonly enrollments = signal<WorkshopEnrollment[]>([]);
  readonly loading = signal(true);
  readonly loadingEnrollments = signal(false);
  readonly error = signal('');
  readonly busyId = signal('');
  readonly selected = computed(() =>
    this.workshops().find(workshop => workshop.id === this.selectedId()) ?? null);
  search = '';

  constructor(private readonly api: ApiService) {}

  ngOnInit(): void {
    this.load();
  }

  totalActive(): number {
    return this.workshops().reduce((total, workshop) => total + workshop.activeEnrollments, 0);
  }

  filteredWorkshops(): Workshop[] {
    const query = this.search.trim().toLowerCase();
    if (!query) return this.workshops();
    return this.workshops().filter(workshop =>
      workshop.title.toLowerCase().includes(query) ||
      workshop.code.toLowerCase().includes(query) ||
      workshop.instructorName.toLowerCase().includes(query)
    );
  }

  load(): void {
    this.loading.set(true);
    this.error.set('');
    this.api.managedWorkshops().subscribe({
      next: workshops => {
        this.workshops.set(workshops);
        this.loading.set(false);
        if (workshops.length > 0) this.select(workshops[0]);
      },
      error: failure => {
        this.error.set(errorMessage(failure, 'No pudimos cargar los talleres.'));
        this.loading.set(false);
      }
    });
  }

  select(workshop: Workshop): void {
    this.selectedId.set(workshop.id);
    this.enrollments.set([]);
    this.loadingEnrollments.set(true);
    this.error.set('');
    this.api.workshopEnrollments(workshop.id).subscribe({
      next: enrollments => {
        this.enrollments.set(enrollments);
        this.loadingEnrollments.set(false);
      },
      error: failure => {
        this.error.set(errorMessage(failure, 'No pudimos cargar los inscritos.'));
        this.loadingEnrollments.set(false);
      }
    });
  }

  download(enrollment: WorkshopEnrollment): void {
    this.busyId.set(enrollment.id);
    this.error.set('');
    this.api.certificate(enrollment.id).subscribe({
      next: blob => {
        const url = URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `skillhub-${this.selected()?.code ?? 'constancia'}.pdf`;
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
