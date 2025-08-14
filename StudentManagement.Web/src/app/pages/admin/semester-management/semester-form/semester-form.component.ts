import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { SemesterService } from '../../../../services/semester.service';
import { Semester } from '../../../../models/Semester.model';

@Component({
  selector: 'app-semester-form',
  templateUrl: './semester-form.component.html',
  styleUrls: ['./semester-form.component.scss'],
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
})
export class SemesterFormComponent implements OnInit {
  form: ReturnType<FormBuilder['group']>;
  isEdit = false;
  id?: number;

  constructor(
    private fb: FormBuilder,
    private semesterService: SemesterService,
    private route: ActivatedRoute,
    private router: Router
  ) {
    this.form = this.fb.group({
      semesterName: ['', Validators.required],
      academicYear: ['', Validators.required],
      startDate: ['', Validators.required],
      endDate: ['', Validators.required],
      isActive: [true]
    });
  }

  ngOnInit(): void {
    this.id = Number(this.route.snapshot.paramMap.get('id'));
    if (this.id) {
      this.isEdit = true;
      this.semesterService.getSemesterById(this.id).subscribe(s => {
        this.form.patchValue(s);
      });
    }
  }

  onSubmit() {
    if (this.form.invalid) return;
    const value = {
      ...this.form.value,
      semesterName: this.form.value.semesterName || '',
      academicYear: this.form.value.academicYear || '',
      startDate: this.form.value.startDate || '',
      endDate: this.form.value.endDate || '',
      isActive: this.form.value.isActive ?? true
    };

    if (this.isEdit && this.id) {
      this.semesterService.updateSemester(this.id, value).subscribe(() => {
        this.router.navigate(['/admin/semester-management']);
      });
    } else {
      this.semesterService.createSemester(value).subscribe(() => {
        this.router.navigate(['/admin/semester-management']);
      });
    }
  }
}
