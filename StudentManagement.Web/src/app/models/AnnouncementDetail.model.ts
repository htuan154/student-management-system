// AnnouncementDetail.model.ts
import {Announcement} from './Announcement.model';
import {Role }  from './role.model';
import {Class} from './class.model';
import {Course} from './course.model';
import {User} from './user.model';
export interface AnnouncementDetail {
  announcementDetailId: number;
  announcementId: number;
  roleId?: string | null;
  classId?: string | null;
  courseId?: string | null;
  userId?: string | null;
  announcement?: Announcement | null;
  role?: Role | null;
  class?: Class | null;
  course?: Course | null;
  user?: User | null;
}
