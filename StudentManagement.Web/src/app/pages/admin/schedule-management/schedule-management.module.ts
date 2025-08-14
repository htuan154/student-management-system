import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ReactiveFormsModule } from '@angular/forms';

import { ScheduleListComponent } from './schedule-list/schedule-list.component';
import { ScheduleFormComponent } from './schedule-form/schedule-form.component';

const routes = [
  { path: '', component: ScheduleListComponent },
  { path: 'new', component: ScheduleFormComponent },
  { path: 'edit/:id', component: ScheduleFormComponent }
];

@NgModule({
  declarations: [],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterModule.forChild(routes),
    // Import standalone components
    ScheduleListComponent,
    ScheduleFormComponent
  ]
})
export class ScheduleManagementModule { }
