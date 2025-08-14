import { Component, OnInit } from '@angular/core';
import { FormBuilder, Validators, ReactiveFormsModule } from '@angular/forms'; // Thêm ReactiveFormsModule
import { ActivatedRoute, Router } from '@angular/router';
import { AnnouncementService } from '../../../../services/announcement.service';
import { AnnouncementDetailService } from '../../../..//services/announcement-detail.service';
import { Announcement } from '../../../../models/Announcement.model';

@Component({
  selector: 'app-announcement-form',
  templateUrl: './announcement-form.component.html',
  styleUrls: ['./announcement-form.component.scss'],
  standalone: true,
  imports: [ReactiveFormsModule],
})
export class AnnouncementFormComponent implements OnInit {
  id?: number;
  mode: 'create' | 'edit' = 'create';
  loading = false;
  saving = false;
  form: ReturnType<FormBuilder['group']>; // Khai báo thuộc tính form

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private announcementService: AnnouncementService,
    private announcementDetailService: AnnouncementDetailService
  ) {
    this.form = this.fb.group({
      title: ['', [Validators.required, Validators.maxLength(200)]],
      content: ['', [Validators.required]],
      expiryDate: [''],
      createdBy: ['', Validators.required],

      // bulk targeting (CSV ids)
      roleIds: [''],
      classIds: [''],
      courseIds: [''],
      userIds: [''],

      overwriteDetails: [true]
    });
  }

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      this.id = +idParam;
      this.mode = 'edit';
      this.loadDetail(this.id);
    }
  }

  private loadDetail(id: number) {
    this.loading = true;
    this.announcementService.getAnnouncementById(id).subscribe({
      next: (a: Announcement) => {
        this.form.patchValue({
          title: a.title,
          content: a.content,
          expiryDate: a.expiryDate ?? '',
          createdBy: a.createdBy
        });

        // lấy sẵn chi tiết để suggest CSV
        this.announcementDetailService.getDetailsByAnnouncementId(id).subscribe({
          next: (details) => {
            const roleIds = details.filter(d => d.roleId).map(d => d.roleId).join(',');
            const classIds = details.filter(d => d.classId).map(d => d.classId).join(',');
            const courseIds = details.filter(d => d.courseId).map(d => d.courseId).join(',');
            const userIds = details.filter(d => d.userId).map(d => d.userId).join(',');
            this.form.patchValue({ roleIds, classIds, courseIds, userIds });
            this.loading = false;
          },
          error: () => (this.loading = false)
        });
      },
      error: () => (this.loading = false)
    });
  }

  onSubmit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const { title, content, expiryDate, createdBy } = this.form.value;
    const payload = {
      title: title!.trim(),
      content: content!.trim(),
      createdBy: createdBy!.trim(),
      expiryDate: expiryDate ? new Date(expiryDate as string).toISOString() : null
    } as Omit<Announcement, 'announcementId' | 'user' | 'announcementDetails' | 'createdAt'>;

    this.saving = true;

    const doBulk = (announcementId: number) => {
      const bulkData = this.toBulkPayload();
      if (!bulkData) {
        this.finish();
        return;
      }
      this.announcementDetailService.bulkCreateAnnouncementDetails(announcementId, bulkData).subscribe({
        next: () => this.finish(),
        error: () => this.finish()
      });
    };

    if (this.mode === 'create') {
      this.announcementService.createAnnouncement(payload).subscribe({
        next: (a) => doBulk(a.announcementId),
        error: () => this.finish()
      });
    } else if (this.id) {
      this.announcementService.updateAnnouncement(this.id, payload).subscribe({
        next: () => {
          if (this.form.value.overwriteDetails) {
            this.announcementDetailService.deleteDetailsByAnnouncementId(this.id!).subscribe({
              next: () => doBulk(this.id!),
              error: () => this.finish()
            });
          } else {
            doBulk(this.id!);
          }
        },
        error: () => this.finish()
      });
    }
  }

  private toBulkPayload() {
    const parseCsv = (s?: string | null) =>
      (s || '')
        .split(',')
        .map(x => x.trim())
        .filter(Boolean);

    const roleIds = parseCsv(this.form.value.roleIds);
    const classIds = parseCsv(this.form.value.classIds);
    const courseIds = parseCsv(this.form.value.courseIds);
    const userIds = parseCsv(this.form.value.userIds);

    if (!roleIds.length && !classIds.length && !courseIds.length && !userIds.length) return null;

    return { roleIds, classIds, courseIds, userIds };
  }

  private finish() {
    this.saving = false;
    this.router.navigate(['/admin/announcement-management']);
  }
}
