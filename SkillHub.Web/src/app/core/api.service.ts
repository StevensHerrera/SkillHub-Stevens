import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { Enrollment, Workshop, WorkshopEnrollment } from './models';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly base = environment.apiUrl;

  constructor(private readonly http: HttpClient) {}

  workshops() {
    return this.http.get<Workshop[]>(`${this.base}/workshops`);
  }

  workshop(id: string) {
    return this.http.get<Workshop>(`${this.base}/workshops/${id}`);
  }

  managedWorkshops() {
    return this.http.get<Workshop[]>(`${this.base}/workshops/manage`);
  }

  workshopEnrollments(id: string) {
    return this.http.get<WorkshopEnrollment[]>(`${this.base}/workshops/${id}/enrollments`);
  }

  enroll(workshopId: string) {
    return this.http.post<Enrollment>(`${this.base}/enrollments`, { workshopId });
  }

  myEnrollments() {
    return this.http.get<Enrollment[]>(`${this.base}/enrollments/mine`);
  }

  cancel(enrollmentId: string, reason?: string) {
    return this.http.patch<Enrollment>(`${this.base}/enrollments/${enrollmentId}/cancel`, { reason });
  }

  certificate(enrollmentId: string) {
    return this.http.get(`${this.base}/enrollments/${enrollmentId}/certificate`, { responseType: 'blob' });
  }
}
