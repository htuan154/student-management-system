import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { FormBuilder, Validators, ReactiveFormsModule, FormGroup, FormControl } from '@angular/forms';
import { FormsModule } from '@angular/forms';
import { forkJoin, of } from 'rxjs';
import { catchError } from 'rxjs/operators';

import { Announcement } from '../../../../models/Announcement.model';
import { AnnouncementDetail } from '../../../../models/AnnouncementDetail.model';

import { AnnouncementService } from '../../../../services/announcement.service';
import { AnnouncementDetailService } from '../../../../services/announcement-detail.service';

import { RoleService } from '../../../../services/role.service';
import { ClassService } from '../../../../services/class.service';
import { CourseService } from '../../../../services/course.service';
import { UserService } from '../../../../services/user.service';
import { AuthService } from '../../../../services/auth.service';

type Option = { id: string; name: string };

@Component({
  selector: 'app-announcement-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule, RouterModule],
  templateUrl: './announcement-form.component.html',
  styleUrls: ['./announcement-form.component.scss']
})
export class AnnouncementFormComponent implements OnInit {
  // ===== Route / UI state =====
  id?: number;
  mode: 'create' | 'edit' = 'create';
  loading = false;
  saving  = false;
  errorMessage: string | null = null;

  // ===== Current user =====
  currentUserName: string | null = null;
  existingCreatedBy: string | null = null;

  // ===== Dropdown sources =====
  roleOptions:   Option[] = [];
  classOptions:  Option[] = [];
  courseOptions: Option[] = [];
  userOptions:   Option[] = [];

  // ===== Reactive form =====
  form!: FormGroup;

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private announcementService: AnnouncementService,
    private announcementDetailService: AnnouncementDetailService,
    private roleService: RoleService,
    private classService: ClassService,
    private courseService: CourseService,
    private userService: UserService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.currentUserName =
    this.authService.getCurrentUsername() ||
    this.authService.getCurrentUserEmail() ||
    this.authService.getCurrentUserId() ||
    null;
    this.initForm();
    this.loadReceivers();

    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      this.id = +idParam;
      this.mode = 'edit';
      this.loadAnnouncement(this.id);
    }

    // Bật thông báo chung -> reset filter
    this.form.get('isGlobal')!.valueChanges.subscribe((on: boolean) => {
      if (on) {
        this.form.patchValue(
          { roleIds: [], classIds: [], courseIds: [], userIds: [] },
          { emitEvent: false }
        );
      }
    });
  }

  // ---------- Form ----------
  private initForm(): void {
    this.form = this.fb.group({
      title: ['', [Validators.required, Validators.maxLength(200)]],
      content: ['', [Validators.required]],
      expiryDate: [''],

      isGlobal: [true],
      roleIds:   this.fb.control<string[]>([]),
      classIds:  this.fb.control<string[]>([]),
      courseIds: this.fb.control<string[]>([]),
      userIds:   this.fb.control<string[]>([]),

      overwriteDetails: [true],
    });
  }

  // ---------- Load dropdown data ----------
  private loadReceivers(): void {
    this.loading = true;
    forkJoin({
      roles:   this.roleService.getAllRoles().pipe(catchError(() => of([]))),
      classes: this.classService.getAllClasses().pipe(catchError(() => of([]))),
      courses: this.courseService.getAllCourses().pipe(catchError(() => of([]))),
      users:   this.userService.getAllUsers().pipe(catchError(() => of([])))
    }).subscribe({
      next: ({ roles, classes, courses, users }: any) => {
        this.roleOptions = (roles || []).map((r: any) => ({
          id: r.roleId ?? r.id, name: r.roleName ?? r.name ?? r.roleId
        }));
        this.classOptions = (classes || []).map((c: any) => ({
          id: c.classId ?? c.id, name: c.className ?? c.name ?? c.classId
        }));
        this.courseOptions = (courses || []).map((c: any) => ({
          id: c.courseId ?? c.id, name: c.courseName ?? c.name ?? c.courseId
        }));
        this.userOptions = (users || []).map((u: any) => ({
          id: u.userId ?? u.id, name: u.fullName ?? u.userName ?? u.email ?? u.userId
        }));
        this.loading = false;
      },
      error: () => (this.loading = false)
    });
  }

  // ---------- Load detail when edit ----------
  private loadAnnouncement(id: number): void {
    this.loading = true;
    this.announcementService.getAnnouncementById(id).subscribe({
      next: (a: Announcement) => {
        // lưu người tạo gốc để khỏi thay đổi khi update
        this.existingCreatedBy = a.createdBy ?? null;

        this.form.patchValue({
          title: a.title ?? '',
          content: a.content ?? '',
          expiryDate: a.expiryDate ?? ''
        });

        this.announcementDetailService.getDetailsByAnnouncementId(id).subscribe({
          next: (details: AnnouncementDetail[]) => {
            const roleIds   = details.map(d => d.roleId).filter((x): x is string => !!x);
            const classIds  = details.map(d => d.classId).filter((x): x is string => !!x);
            const courseIds = details.map(d => d.courseId).filter((x): x is string => !!x);
            const userIds   = details.map(d => d.userId).filter((x): x is string => !!x);
            const isGlobal = !roleIds.length && !classIds.length && !courseIds.length && !userIds.length;

            this.form.patchValue({ roleIds, classIds, courseIds, userIds, isGlobal });
            this.loading = false;
          },
          error: () => (this.loading = false)
        });
      },
      error: () => (this.loading = false)
    });
  }

  // ---------- Submit ----------
  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving = true; this.errorMessage = null;
    const raw = this.form.getRawValue();

    // Base payload
    const base = {
      title: (raw.title ?? '').trim(),
      content: (raw.content ?? '').trim(),
      expiryDate: raw.expiryDate ? new Date(raw.expiryDate as string).toISOString() : null
    };

    // Luôn đảm bảo createdBy là string
    const createdBy: string =
      this.mode === 'create'
        ? (this.currentUserName ?? '')
        : (this.existingCreatedBy ?? this.currentUserName ?? '');

    const payload: Omit<Announcement, 'announcementId' | 'announcementDetails' | 'user' | 'createdAt'> = {
      ...base,
      createdBy
    };

    const finalize = () => {
      this.saving = false;
      this.router.navigate(['/admin/announcement-management']);
    };

    const doBulk = (announcementId: number) => {
      const bulk = this.toBulkPayload();
      if (!bulk) return finalize(); // global -> không tạo details
      this.announcementDetailService.bulkCreateAnnouncementDetails(announcementId, bulk).subscribe({
        next: () => finalize(),
        error: () => finalize()
      });
    };

    if (this.mode === 'create') {
      this.announcementService.createAnnouncement(payload).subscribe({
        next: (created: Announcement) => doBulk(created.announcementId),
        error: (err) => {
          this.errorMessage = err?.error?.message || 'Tạo thông báo thất bại.';
          this.saving = false;
        }
      });
    } else if (this.id) {
      this.announcementService.updateAnnouncement(this.id, payload).subscribe({
        next: () => {
          if (this.form.value.overwriteDetails) {
            this.announcementDetailService.deleteDetailsByAnnouncementId(this.id!).subscribe({
              next: () => doBulk(this.id!),
              error: () => finalize()
            });
          } else {
            doBulk(this.id!);
          }
        },
        error: (err) => {
          this.errorMessage = err?.error?.message || 'Cập nhật thông báo thất bại.';
          this.saving = false;
        }
      });
    }
  }

  // ---------- Build bulk payload ----------
  private toBulkPayload() {
    if (this.form.value.isGlobal) return null;
    const pick = (arr?: (string | null | undefined)[] | null) =>
      (arr ?? []).filter((v): v is string => !!v);

    const roleIds   = pick(this.form.value.roleIds as any);
    const classIds  = pick(this.form.value.classIds as any);
    const courseIds = pick(this.form.value.courseIds as any);
    const userIds   = pick(this.form.value.userIds as any);

    if (!roleIds.length && !classIds.length && !courseIds.length && !userIds.length) return null;
    return { roleIds, classIds, courseIds, userIds };
  }

  // ---------- Helpers cho dropdown + chip ----------
  private arrCtrl(name: 'roleIds' | 'classIds' | 'courseIds' | 'userIds'): FormControl<string[]> {
    return this.form.get(name) as FormControl<string[]>;
  }

  onAdd(name: 'roleIds' | 'classIds' | 'courseIds' | 'userIds', value: string | null) {
    if (!value) return;
    const ctrl = this.arrCtrl(name);
    const cur = [...(ctrl.value ?? [])];
    if (!cur.includes(value)) {
      cur.push(value);
      ctrl.setValue(cur);
      ctrl.markAsDirty();
    }
  }

  removeItem(name: 'roleIds' | 'classIds' | 'courseIds' | 'userIds', value: string) {
    const ctrl = this.arrCtrl(name);
    ctrl.setValue((ctrl.value ?? []).filter(v => v !== value));
    ctrl.markAsDirty();
  }

  selectAll(name: 'roleIds' | 'classIds' | 'courseIds' | 'userIds', options: Option[]) {
    const ctrl = this.arrCtrl(name);
    ctrl.setValue(options.map(o => o.id));
    ctrl.markAsDirty();
  }

  clearAll(name: 'roleIds' | 'classIds' | 'courseIds' | 'userIds') {
    const ctrl = this.arrCtrl(name);
    ctrl.setValue([]);
    ctrl.markAsDirty();
  }

  labelOf(id: string, source: Option[]) {
    return source.find(x => x.id === id)?.name ?? id;
  }
}
