import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

import { AnnouncementManagementRoutingModule } from './announcement-management-routing.module';
import { AnnouncementManagementComponent } from './announcement-management.component';

import { AnnouncementsListComponent } from './announcements-list/announcements-list.component';
import { AnnouncementFormComponent } from './announcement-form/announcement-form.component';

@NgModule({
  
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    AnnouncementManagementRoutingModule,
    AnnouncementManagementComponent,
    AnnouncementsListComponent,
    AnnouncementFormComponent,
  ],
})
export class AnnouncementManagementModule {}
