import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';

import { ScoreManagementRoutingModule } from './score-management-routing.module';
import { ScoreListComponent } from './score-list/score-list.component';
import { ScoreFormComponent } from './score-form/score-form.component';

@NgModule({
  
  imports: [
    CommonModule,
    ScoreManagementRoutingModule,
    ReactiveFormsModule,
    ScoreListComponent,
    ScoreFormComponent
  ]
})
export class ScoreManagementModule { }
