export interface AuthResponse {
  accessToken: string;
  expiresAt: string;
  email: string;
  fullName: string;
  roles: string[];
}

export interface RegisterRequest {
  fullName: string;
  email: string;
  password: string;
}

export interface Workshop {
  id: string;
  code: string;
  title: string;
  description: string;
  instructorName: string;
  instructorEmail: string;
  instructorSpecialty: string;
  startAt: string;
  endAt: string;
  capacity: number;
  activeEnrollments: number;
  availableSeats: number;
}

export interface WorkshopEnrollment {
  id: string;
  participantId: string;
  participantName: string;
  participantEmail: string;
  status: 'Active' | 'Cancelled';
  enrolledAt: string;
  cancelledAt: string | null;
  cancellationReason: string | null;
}

export interface Enrollment {
  id: string;
  workshopId: string;
  workshopCode: string;
  workshopTitle: string;
  instructorName: string;
  startAt: string;
  endAt: string;
  status: 'Active' | 'Cancelled';
  enrolledAt: string;
  cancelledAt: string | null;
  cancellationReason: string | null;
}
