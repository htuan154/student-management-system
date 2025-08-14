import { Routes } from '@angular/router';

export const SCHEDULE_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./schedule-list/schedule-list.component').then(c => c.ScheduleListComponent)
  },
  {
    path: 'new',
    loadComponent: () => import('./schedule-form/schedule-form.component').then(c => c.ScheduleFormComponent)
  },
  {
    path: 'edit/:id',
    loadComponent: () => import('./schedule-form/schedule-form.component').then(c => c.ScheduleFormComponent)
  }
];
