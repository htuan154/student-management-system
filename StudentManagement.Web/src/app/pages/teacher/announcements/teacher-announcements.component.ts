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
  selector: 'app-teacher-announcements',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './teacher-announcements.component.html',
  styleUrls: ['./teacher-announcements.component.scss']
})
export class TeacherAnnouncementsComponent implements OnInit {
  isLoading = false;
  errorMessage: string | null = null;

  userId: string | null = null;
  announcements: UiAnnouncement[] = [];
  filtered: UiAnnouncement[] = [];

  searchTerm = '';
  showOnlyUnread = false;
  hideExpired = false;

  private get storageKey(): string {
    return `teacher_read_announcements_${this.userId ?? 'unknown'}`;
  }

  constructor(
    private anns: AnnouncementService,
    private annDetails: AnnouncementDetailService,
    private auth: AuthService
  ) {}

  ngOnInit(): void {
    const p = this.auth.getDecodedToken();

    this.userId =
      p?.['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] ||
      p?.userId || p?.sub || p?.id || null;

    if (!this.userId) {
      this.errorMessage = 'Không thể xác định người dùng.';
      return;
    }
    this.loadAnnouncements();
  }

  loadAnnouncements(): void {
    this.isLoading = true;
    this.errorMessage = null;

    this.annDetails
      .getUserAnnouncementDetails(this.userId!)
      .pipe(catchError(() => of([] as AnnouncementDetail[])))
      .subscribe({
        next: (details) => {
          const ids = Array.from(
            new Set((details || []).map(d => d.announcementId).filter((x): x is number => x != null))
          );

          if (ids.length === 0) {
            this.announcements = [];
            this.applyFilter();
            this.isLoading = false;
            return;
          }

          forkJoin(
            ids.map(id =>
              this.anns.getAnnouncementById(id).pipe(catchError(() => of(null as unknown as Announcement)))
            )
          ).subscribe({
            next: (list) => {
              const readSet = new Set<number>(JSON.parse(localStorage.getItem(this.storageKey) || '[]'));

              const items: UiAnnouncement[] = (list || [])
                .filter(a => !!a)

                .map(a => ({
                  ...a,
                  isRead: readSet.has(a.announcementId),
                  isExpanded: false
                }))
                .sort((a, b) => +new Date(b.createdAt) - +new Date(a.createdAt));

              this.announcements = items;
              this.applyFilter();
              this.isLoading = false;
            },
            error: () => {
              this.errorMessage = 'Không thể tải nội dung thông báo.';
              this.isLoading = false;
            }
          });
        },
        error: () => {
          this.errorMessage = 'Không thể tải danh sách thông báo.';
          this.isLoading = false;
        }
      });
  }

  toggleExpand(item: UiAnnouncement): void {
    item.isExpanded = !item.isExpanded;
    if (!item.isRead) this.markAsRead(item);
  }

  markAsRead(item: UiAnnouncement): void {
    item.isRead = true;
    const set = new Set<number>(JSON.parse(localStorage.getItem(this.storageKey) || '[]'));
    set.add(item.announcementId);
    localStorage.setItem(this.storageKey, JSON.stringify(Array.from(set)));
    this.applyFilter();
  }


  private isSameOrAfterToday(dateStr: string): boolean {
    const d = new Date(dateStr);
    const dd = new Date(d.getFullYear(), d.getMonth(), d.getDate());
    const today = new Date();
    const t0 = new Date(today.getFullYear(), today.getMonth(), today.getDate());
    return dd >= t0;
  }

  applyFilter(): void {
    const kw = this.searchTerm.trim().toLowerCase();

    this.filtered = this.announcements.filter(a => {
      const matchKw =
        !kw ||
        a.title?.toLowerCase().includes(kw) ||
        a.content?.toLowerCase().includes(kw);

      const matchUnread = !this.showOnlyUnread || !a.isRead;

      const matchExpired =
        !this.hideExpired ||
        !a.expiryDate ||
        this.isSameOrAfterToday(a.expiryDate);

      return matchKw && matchUnread && matchExpired;
    });
  }

  clearSearch(): void {
    this.searchTerm = '';
    this.applyFilter();
  }

  isNew(createdAt: string): boolean {
    const created = new Date(createdAt);
    const diff = (Date.now() - +created) / (1000 * 60 * 60 * 24);
    return diff <= 3;
  }
}
