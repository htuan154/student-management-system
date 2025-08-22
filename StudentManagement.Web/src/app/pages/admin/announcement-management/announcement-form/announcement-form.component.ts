import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { FormBuilder, Validators, ReactiveFormsModule, FormGroup, FormControl } from '@angular/forms';
import { FormsModule } from '@angular/forms';
import { forkJoin, of } from 'rxjs';
import { catchError, switchMap } from 'rxjs/operators';

import { Announcement } from '../../../../models/Announcement.model';
import { AnnouncementDetail } from '../../../../models/AnnouncementDetail.model';

import { AnnouncementService, CreateAnnouncementDto } from '../../../../services/announcement.service';
import { AnnouncementDetailService } from '../../../../services/announcement-detail.service';
import { RoleService } from '../../../../services/role.service';
import { ClassService } from '../../../../services/class.service';
import { CourseService } from '../../../../services/course.service';
import { UserService } from '../../../../services/user.service';
import { AuthService } from '../../../../services/auth.service';

type Option = { id: string; name: string };
type IdField = 'roleIds' | 'classIds' | 'courseIds' | 'userIds';

@Component({
  selector: 'app-announcement-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule, RouterModule],
  templateUrl: './announcement-form.component.html',
  styleUrls: ['./announcement-form.component.scss']
})
export class AnnouncementFormComponent implements OnInit {
  // Chế độ
  mode: 'create' | 'edit' = 'create';
  id?: number;

  // Trạng thái UI
  loading = false;
  isSubmitting = false;
  submitError: string | null = null;

  // Alias để khớp template cũ
  get saving() { return this.isSubmitting; }
  get errorMessage() { return this.submitError; }

  // Form
  form!: FormGroup;

  // Nguồn dữ liệu lựa chọn
  roleOptions:   Option[] = [];
  classOptions:  Option[] = [];
  courseOptions: Option[] = [];
  userOptions:   Option[] = [];

  // Hiển thị “người tạo” (FE chỉ show; BE lấy từ token)
  currentUserName: string | null = null;

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private announcementService: AnnouncementService,
    private detailService: AnnouncementDetailService,
    private roleService: RoleService,
    private classService: ClassService,
    private courseService: CourseService,
    private userService: UserService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.currentUserName =
      this.authService.getCurrentUsername?.() ||
      this.authService.getCurrentUserEmail?.() ||
      this.authService.getCurrentUserId?.() ||
      null;

    this.initForm();
    this.loadReceiverOptions();

    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      this.id = +idParam;
      this.mode = 'edit';
      this.loadAnnouncementForEdit(this.id);
    }

    // Bật thông báo chung -> xoá hết selections
    this.form.get('isGlobal')!.valueChanges.subscribe((isGlobal: boolean) => {
      if (isGlobal) {
        (['roleIds','classIds','courseIds','userIds'] as IdField[]).forEach(n => {
          this.arrCtrl(n).setValue([]);
        });
      }
    });
  }

  private initForm(): void {
    this.form = this.fb.group({
      title: ['', [Validators.required, Validators.maxLength(200)]],
      content: ['', [Validators.required]],
      expiryDate: [null], // string ISO | null
      isGlobal: [true],

      // 👇 Các control mà template đang dùng qua onAdd/removeItem/selectAll/clearAll
      roleIds:   this.fb.control<string[]>([]),
      classIds:  this.fb.control<string[]>([]),
      courseIds: this.fb.control<string[]>([]),
      userIds:   this.fb.control<string[]>([])
    });
  }

  private loadReceiverOptions(): void {
    this.loading = true;
    forkJoin({
      roles:   this.roleService.getAllRoles().pipe(catchError(() => of([]))),
      classes: this.classService.getAllClasses().pipe(catchError(() => of([]))),
      courses: this.courseService.getAllCourses().pipe(catchError(() => of([]))),
      users:   this.userService.getAllUsers().pipe(catchError(() => of([])))
    }).subscribe({
      next: ({ roles, classes, courses, users }: any) => {
        this.roleOptions   = (roles   || []).map((r: any) => ({ id: r.roleId   ?? r.id, name: r.roleName   ?? r.name ?? r.roleId }));
        this.classOptions  = (classes || []).map((c: any) => ({ id: c.classId  ?? c.id, name: c.className  ?? c.name ?? c.classId }));
        this.courseOptions = (courses || []).map((k: any) => ({ id: k.courseId ?? k.id, name: k.courseName ?? k.name ?? k.courseId }));
        this.userOptions   = (users   || []).map((u: any) => ({ id: u.userId   ?? u.id, name: u.fullName   ?? u.userName ?? u.email ?? u.userId }));
        this.loading = false;
      },
      error: () => (this.loading = false)
    });
  }

  private loadAnnouncementForEdit(id: number): void {
    this.loading = true;
    this.announcementService.getAnnouncementById(id).subscribe({
      next: (a: Announcement) => {
        this.form.patchValue({
          title: a.title ?? '',
          content: a.content ?? '',
          expiryDate: a.expiryDate ?? null
        });

        // Nạp detail để chỉnh sửa
        if (this.detailService.getDetailsByAnnouncementId) {
          this.detailService.getDetailsByAnnouncementId(id).subscribe({
            next: (details: AnnouncementDetail[]) => {
              const roleIds   = details.map(d => d.roleId).filter((x): x is string => !!x);
              const classIds  = details.map(d => d.classId).filter((x): x is string => !!x);
              const courseIds = details.map(d => d.courseId).filter((x): x is string => !!x);
              const userIds   = details.map(d => d.userId).filter((x): x is string => !!x);

              this.arrCtrl('roleIds').setValue(roleIds);
              this.arrCtrl('classIds').setValue(classIds);
              this.arrCtrl('courseIds').setValue(courseIds);
              this.arrCtrl('userIds').setValue(userIds);

              const isGlobal = !roleIds.length && !classIds.length && !courseIds.length && !userIds.length;
              this.form.patchValue({ isGlobal });

              this.loading = false;
            },
            error: () => { this.loading = false; }
          });
        } else {
          this.loading = false;
        }
      },
      error: () => (this.loading = false)
    });
  }

  // ======== Helpers để KHỚP TEMPLATE ========
  private arrCtrl(name: IdField): FormControl<string[]> {
    return this.form.get(name) as FormControl<string[]>;
  }

  /** Template: (change)="onAdd('roleIds', select.value); select.value=''" */
  onAdd(name: IdField, value: string | null | undefined): void {
    if (!value) return;
    const ctrl = this.arrCtrl(name);
    const cur  = [...(ctrl.value ?? [])];
    if (!cur.includes(value)) {
      cur.push(value);
      ctrl.setValue(cur);
      ctrl.markAsDirty();
    }
  }

  /** Template: (click)="removeItem('roleIds', id)" */
  removeItem(name: IdField, id: string): void {
    const ctrl = this.arrCtrl(name);
    ctrl.setValue((ctrl.value ?? []).filter(v => v !== id));
    ctrl.markAsDirty();
  }

  /** Template: (click)="selectAll('roleIds', roleOptions)" */
  selectAll(name: IdField, options: Option[]): void {
    const ctrl = this.arrCtrl(name);
    ctrl.setValue(options.map(o => o.id));
    ctrl.markAsDirty();
  }

  /** Template: (click)="clearAll('roleIds')" */
  clearAll(name: IdField): void {
    const ctrl = this.arrCtrl(name);
    ctrl.setValue([]);
    ctrl.markAsDirty();
  }

  /** Label hiển thị chip */
  labelOf(id: string, source: Option[]): string {
    return source.find(x => x.id === id)?.name ?? id;
  }
  // =========================================

  /** Luôn build payload bulk để chắc chắn có AnnouncementDetails */
  private buildBulkPayload(isGlobal: boolean) {
    if (isGlobal) {
      // Gửi toàn bộ roleIds để BE expand ra mọi user theo vai trò (hoặc đổi sang all userIds tuỳ BE)
      return { roleIds: this.roleOptions.map(r => r.id) };
      // return { userIds: this.userOptions.map(u => u.id) };
    }

    const roleIds   = this.arrCtrl('roleIds').value ?? [];
    const classIds  = this.arrCtrl('classIds').value ?? [];
    const courseIds = this.arrCtrl('courseIds').value ?? [];
    const userIds   = this.arrCtrl('userIds').value ?? [];

    const nothingSelected =
      roleIds.length === 0 &&
      classIds.length === 0 &&
      courseIds.length === 0 &&
      userIds.length === 0;

    if (nothingSelected) {
      throw new Error('Hãy chọn ít nhất 1 Vai trò/Lớp/Môn/Người dùng hoặc bật “Thông báo chung”.');
    }

    return { roleIds, classIds, courseIds, userIds };
  }

  onSubmit(): void {
    this.submitError = null;

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const { title, content, expiryDate, isGlobal } = this.form.getRawValue();
    const dto: CreateAnnouncementDto = {
      title: (title ?? '').trim(),
      content: (content ?? '').trim(),
      expiryDate: expiryDate ? new Date(expiryDate as string).toISOString() : null
    };

    this.isSubmitting = true;

    // CREATE
    if (this.mode === 'create') {
      this.announcementService.createAnnouncement(dto).pipe(
        switchMap((a: Announcement) => {
          const annId = a.announcementId!;
          const bulk  = this.buildBulkPayload(!!isGlobal);
          return this.detailService.bulkCreateAnnouncementDetails(annId, bulk);
        })
      ).subscribe({
        next: () => {
          this.isSubmitting = false;
          alert('Tạo thông báo + người nhận thành công!');
          this.router.navigate(['/admin/announcement-management']);
        },
        error: (err) => {
          this.submitError = err?.error?.message || err?.message || 'Có lỗi khi tạo chi tiết thông báo.';
          this.isSubmitting = false;
        }
      });
      return;
    }

    // EDIT
    if (this.id != null) {
      this.announcementService.updateAnnouncement(this.id, dto).pipe(
        switchMap(() => {
          const bulk = this.buildBulkPayload(!!isGlobal);
          // Xoá detail cũ -> tạo lại
          return this.detailService.deleteDetailsByAnnouncementId(this.id!).pipe(
            switchMap(() => this.detailService.bulkCreateAnnouncementDetails(this.id!, bulk))
          );
        })
      ).subscribe({
        next: () => {
          this.isSubmitting = false;
          alert('Cập nhật thông báo thành công!');
          this.router.navigate(['/admin/announcement-management']);
        },
        error: (err) => {
          this.submitError = err?.error?.message || err?.message || 'Có lỗi xảy ra khi cập nhật thông báo.';
          this.isSubmitting = false;
        }
      });
    }
  }
}
