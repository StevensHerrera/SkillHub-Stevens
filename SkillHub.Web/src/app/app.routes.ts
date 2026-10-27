import { Routes } from '@angular/router';
import { guestGuard, participantGuard } from './core/guards';
import { LoginComponent } from './pages/login.component';
import { MyEnrollmentsComponent } from './pages/my-enrollments.component';
import { RegisterComponent } from './pages/register.component';
import { WorkshopsComponent } from './pages/workshops.component';

export const routes: Routes = [
  { path: '', pathMatch: 'full', component: WorkshopsComponent },
  { path: 'login', component: LoginComponent, canActivate: [guestGuard] },
  { path: 'register', component: RegisterComponent, canActivate: [guestGuard] },
  { path: 'my-enrollments', component: MyEnrollmentsComponent, canActivate: [participantGuard] },
  { path: '**', redirectTo: '' }
];
