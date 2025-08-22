import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { forkJoin, of } from 'rxjs';
import { catchError } from 'rxjs/operators';

import { Announcement } from '../../../models/Announcement.model';
import { AnnouncementDetail } from '../../../models/AnnouncementDetail.model';
import { AnnouncementService } from '../../../services/announcement.service';
import { AnnouncementDetailService } from '../../../services/announcement-detail.service';
import { AuthService } from '../../../services/auth.service';

type UiAnnouncement = Announcement & { isRead?: boolean; isExpanded?: boolean };

@Component({
  selector: 'app-student-announcements',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './student-announcements.component.html',
  styleUrls: ['./student-announcements.component.scss']
})
export class StudentAnnouncementsComponent implements OnInit {
  // UI state
  isLoading = false;
  errorMessage: string | null = null;

  // Current user
  userId: string | null = null;

  // Data
  details: AnnouncementDetail[] = [];
  announcements: UiAnnouncement[] = [];
  filtered: UiAnnouncement[] = [];

  // Filters
  searchTerm = '';
  showOnlyUnread = false;

  constructor(
    private announcementSvc: AnnouncementService,
    private detailSvc: AnnouncementDetailService,
    private auth: AuthService
  ) {}

  /** key lưu trạng thái đã đọc cho riêng từng user */
  private get storageKey(): string {
    return `student_read_announcements_${this.userId ?? 'unknown'}`;
  }

  // -------------------- INIT --------------------
  ngOnInit(): void {
    const p = this.auth.getDecodedToken();
    this.userId =
      p?.['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] ||
      p?.userId || p?.sub || p?.id || null;

    console.log('[Student Announcements] resolved userId =>', this.userId);

    if (!this.userId) {
      this.errorMessage = 'Không thể xác định người dùng.';
      return;
    }
    this.loadAnnouncements();
  }

  // -------------------- LOAD DATA --------------------
  private fetchDetailsForUser(userId: string) {
    const svc: any = this.detailSvc;
    if (typeof svc.getUserAnnouncementDetails === 'function') return svc.getUserAnnouncementDetails(userId);
    if (typeof svc.getByUser === 'function') return svc.getByUser(userId);
    return svc.getAll().pipe(catchError(() => of<AnnouncementDetail[]>([])));
  }

  private loadAnnouncements(): void {
    if (!this.userId) return;

    this.isLoading = true;
    this.errorMessage = null;

    this.fetchDetailsForUser(this.userId).subscribe({
      next: (details: AnnouncementDetail[]) => {
        this.details = (details || []).filter(d => d?.userId === this.userId);
        console.log('[Student Announcements] detail rows:', this.details);

        const ids = Array.from(new Set(this.details.map(d => d.announcementId).filter(x => x != null)));
        console.log('[Student Announcements] announcement ids:', ids);

        if (ids.length === 0) {
          this.announcements = [];
          this.applyFilter();
          this.isLoading = false;
          return;
        }

        const reqs = ids.map(id =>
          this.announcementSvc.getAnnouncementById(id).pipe(catchError(() => of(null)))
        );

        forkJoin(reqs).subscribe({
          next: (arr) => {
            const byId = (arr || []).filter((x): x is Announcement => !!x);

            if (byId.length > 0) {
              this.finishLoad(byId);
              return;
            }

            // Fallback: lấy all rồi lọc theo ids
            this.announcementSvc.getAllAnnouncements().subscribe({
              next: (all) => {
                const filtered = (all || []).filter(a => ids.includes(a.announcementId));
                this.finishLoad(filtered);
              },
              error: (err: any) => this.handleError(err, 'Không thể tải danh sách thông báo.'),
            });
          },
          error: (err: any) => this.handleError(err, 'Không thể tải danh sách thông báo.'),
        });
      },
      error: (err: any) => this.handleError(err, 'Không thể tải danh sách thông báo.'),
    });
  }

  /** Gắn isRead, sắp xếp, rồi apply filter */
  private finishLoad(list: Announcement[]): void {
    const readSet = this.getReadSet();
    const detailMap = new Map<number, AnnouncementDetail>();
    this.details.forEach(d => detailMap.set(d.announcementId, d));

    const enriched: UiAnnouncement[] = (list || []).map(a => {
      const d = detailMap.get(a.announcementId);
      const localRead = readSet.has(a.announcementId);
      const beRead = (d as any)?.isRead ?? false;
      return {
        ...a,
        isRead: localRead || beRead,
        isExpanded: false
      };
    });

    // Sắp xếp mới nhất trước
    enriched.sort((a, b) => {
      const ta = new Date(
        (a as any).createdAt ?? (a as any).createAt ?? (a as any)['createdDate'] ?? 0
      ).getTime();
      const tb = new Date(
        (b as any).createdAt ?? (b as any).createAt ?? (b as any)['createdDate'] ?? 0
      ).getTime();
      return tb - ta;
    });

    this.announcements = enriched;
    this.applyFilter();
    this.isLoading = false;

    console.log('[Student Announcements] announcements after merge:', this.announcements);
  }

  // -------------------- READ STATE --------------------
  toggleExpand(item: UiAnnouncement): void {
    item.isExpanded = !item.isExpanded;
    if (!item.isRead) {
      this.markAsRead(item);
    }
  }

  private getReadSet(): Set<number> {
    try {
      const raw = localStorage.getItem(this.storageKey);
      const arr = raw ? (JSON.parse(raw) as number[]) : [];
      return new Set(arr);
    } catch { return new Set<number>(); }
  }

  private saveReadSet(set: Set<number>): void {
    try {
      localStorage.setItem(this.storageKey, JSON.stringify(Array.from(set)));
    } catch { /* noop */ }
  }

  markAsRead(item: UiAnnouncement): void {
    item.isRead = true;

    const set = this.getReadSet();
    set.add(item.announcementId);
    this.saveReadSet(set);

    const detail = this.details.find(d => d.announcementId === item.announcementId);
    const svc: any = this.detailSvc;
    if (detail && typeof svc.markAsRead === 'function') {
      svc.markAsRead(detail.announcementDetailId).pipe(catchError(() => of(null))).subscribe();
    }
  }

  // -------------------- FILTER --------------------
  applyFilter(): void {
    const q = (this.searchTerm || '').trim().toLowerCase();
    const onlyUnread = this.showOnlyUnread;

    this.filtered = (this.announcements || []).filter(a => {
      if (onlyUnread && a.isRead) return false;
      if (!q) return true;

      const t = (a.title ?? '').toLowerCase();
      const c = (a.content ?? '').toLowerCase();
      return t.includes(q) || c.includes(q);
    });
  }

  /** Dùng cho nút xóa nhanh ô search trên template */
  clearSearch(): void {
    this.searchTerm = '';
    this.applyFilter();
  }

  /** Hiển thị nhãn "Mới" nếu thông báo trong N ngày gần đây */
  isNew(date: string | Date | undefined, days = 7): boolean {
    if (!date) return false;
    const ts = new Date(date).getTime();
    if (isNaN(ts)) return false;
    const diff = Date.now() - ts;
    return diff <= days * 24 * 60 * 60 * 1000;
  }

  // -------------------- HELPERS --------------------
  trackById(_: number, item: UiAnnouncement) {
    return item.announcementId;
  }

  private handleError(err: any, fallback: string): void {
    console.error(err);
    this.errorMessage = fallback;
    this.isLoading = false;
  }
}
