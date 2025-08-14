import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';

import { ScoreManagementRoutingModule } from './score-management-routing.module';
import { ScoreListComponent } from './score-list/score-list.component';
import { ScoreFormComponent } from './score-form/score-form.component';

@NgModule({
  declarations: [
    // ❌ XÓA ScoreListComponent và ScoreFormComponent vì chúng là standalone
  ],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    RouterModule,
    ScoreManagementRoutingModule
    // ❌ KHÔNG import ScoreListComponent vào đây vì nó là standalone component
  ]
})
export class ScoreManagementModule { }
