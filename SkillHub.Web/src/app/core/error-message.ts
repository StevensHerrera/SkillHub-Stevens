import { HttpErrorResponse } from '@angular/common/http';

export function errorMessage(
  error: unknown,
  fallback = 'No pudimos completar la solicitud. Intenta de nuevo.'
): string {
  if (!(error instanceof HttpErrorResponse)) return fallback;
  const body = error.error as
    | { title?: string; detail?: string; message?: string; errors?: Record<string, string[]> }
    | null;
  if (body?.detail) return body.detail;
  if (body?.title) return body.title;
  if (body?.message) return body.message;
  const firstValidation = body?.errors ? Object.values(body.errors).flat()[0] : null;
  if (firstValidation) return firstValidation;
  if (error.status === 0) return 'No hay conexión con el servidor. Revisa que la API esté disponible.';
  return fallback;
}
