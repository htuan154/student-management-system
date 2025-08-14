// Announcement.model.ts
import { User } from './user.model';
import { AnnouncementDetail } from './AnnouncementDetail.model';
export interface Announcement {
  announcementId: number;
  title: string;
  content: string;
  createdBy: string;
  createdAt: string; // ISO date string
  expiryDate?: string | null;
  user?: User | null;
  announcementDetails?: AnnouncementDetail[];
}
