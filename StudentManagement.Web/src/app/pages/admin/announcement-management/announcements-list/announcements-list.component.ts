import { Component, OnDestroy, OnInit } from '@angular/core';
import { AnnouncementService, PagedAnnouncementResponse } from '../../../../services/announcement.service';
import { Announcement } from '../../../../models/Announcement.model';
import { Router } from '@angular/router';
import { Subject, debounceTime } from 'rxjs';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
@Component({
  selector: 'app-announcements-list',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './announcements-list.component.html',
  styleUrls: ['./announcements-list.component.scss']
})
export class AnnouncementsListComponent implements OnInit, OnDestroy {
  items: Announcement[] = [];
  loading = false;

  // paging
  pageNumber = 1;
  pageSize = 10;
  totalCount = 0;
  totalPages = 0;

  // search
  searchTerm = '';
  private search$ = new Subject<string>();

  constructor(
    private announcementService: AnnouncementService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadData();

    // debounce search
    this.search$.pipe(debounceTime(350)).subscribe((term) => {
      this.searchTerm = term;
      this.pageNumber = 1;
      this.loadData();
    });
  }

  ngOnDestroy(): void {
    this.search$.complete();
  }

  onSearchChange(value: string) {
    this.search$.next(value);
  }

  loadData() {
    this.loading = true;
    this.announcementService
      .getPagedAnnouncements(this.pageNumber, this.pageSize, this.searchTerm || undefined)
      .subscribe({
        next: (res: PagedAnnouncementResponse) => {
          this.items = res.data;
          this.totalCount = res.totalCount;
          this.totalPages = res.totalPages;
          this.loading = false;
        },
        error: () => (this.loading = false),
      });
  }

  onCreate() {
    this.router.navigate(['/admin/announcement-management/create']);
  }

  onEdit(item: Announcement) {
    this.router.navigate(['/admin/announcement-management', item.announcementId, 'edit']);
  }

  onDelete(item: Announcement) {
    if (!confirm(`Xoá thông báo "${item.title}"?`)) return;
    this.loading = true;
    this.announcementService.deleteAnnouncement(item.announcementId).subscribe({
      next: () => this.loadData(),
      error: () => (this.loading = false),
    });
  }

  goPage(p: number) {
    if (p < 1 || p > this.totalPages) return;
    this.pageNumber = p;
    this.loadData();
  }

  getFromToLabel(): string {
    if (!this.items?.length) return '0 / 0';
    const from = (this.pageNumber - 1) * this.pageSize + 1;
    const to = from + this.items.length - 1;
    return `${from}–${to} / ${this.totalCount}`;
    }
}
