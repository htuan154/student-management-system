import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Schedule } from '../models';

export interface PagedScheduleResponse {
  data: Schedule[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

@Injectable({
  providedIn: 'root'
})
export class ScheduleService {
  private apiUrl = `${environment.apiUrl}/Schedule`;

  constructor(private http: HttpClient) { }

  /** Lấy lịch học cá nhân (SV đang đăng nhập) */
  getMySchedule(semesterId?: number): Observable<Schedule[]> {
    let params = new HttpParams();
    if (semesterId != null) {
      params = params.set('semesterId', semesterId.toString());
    }
    return this.http.get<Schedule[]>(`${this.apiUrl}/my`, { params });
  }

  /** Lấy tất cả lịch học */
  getAllSchedules(): Observable<Schedule[]> {
    return this.http.get<Schedule[]>(this.apiUrl);
  }

  /** Lấy lịch học theo ID */
  getScheduleById(id: number): Observable<Schedule> {
    return this.http.get<Schedule>(`${this.apiUrl}/${id}`);
  }

  /** Lấy lịch học theo Teacher Course ID */
  getSchedulesByTeacherCourseId(teacherCourseId: number): Observable<Schedule[]> {
    return this.http.get<Schedule[]>(`${this.apiUrl}/teacher-course/${teacherCourseId}`);
  }

  /** Lấy lịch học theo Teacher ID */
  getSchedulesByTeacherId(teacherId: string): Observable<Schedule[]> {
    return this.http.get<Schedule[]>(`${this.apiUrl}/teacher/${teacherId}`);
  }

  /** Lấy lịch học theo Course ID */
  getSchedulesByCourseId(courseId: string): Observable<Schedule[]> {
    return this.http.get<Schedule[]>(`${this.apiUrl}/course/${courseId}`);
  }

  /** Lấy lịch học theo phòng */
  getSchedulesByRoom(roomNumber: string): Observable<Schedule[]> {
    return this.http.get<Schedule[]>(`${this.apiUrl}/room/${roomNumber}`);
  }

  /** Lấy lịch học theo ngày trong tuần */
  getSchedulesByDay(dayOfWeek: number): Observable<Schedule[]> {
    return this.http.get<Schedule[]>(`${this.apiUrl}/day/${dayOfWeek}`);
  }

  /** Tạo lịch học mới */
  createSchedule(scheduleData: Omit<Schedule, 'scheduleId' | 'teacherCourse'>): Observable<Schedule> {
    return this.http.post<Schedule>(this.apiUrl, scheduleData);
  }

  /** Cập nhật lịch học */
  updateSchedule(id: number, scheduleData: any): Observable<Schedule> {
    return this.http.put<Schedule>(`${this.apiUrl}/${id}`, scheduleData);
  }

  /** Xóa lịch học */
  deleteSchedule(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }

  /** Lấy lịch học phân trang */
  getPagedSchedules(pageNumber: number, pageSize: number, searchTerm?: string): Observable<PagedScheduleResponse> {
    let params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    if (searchTerm) {
      params = params.set('searchTerm', searchTerm);
    }

    return this.http.get<PagedScheduleResponse>(`${this.apiUrl}/paged`, { params });
  }

  /** Kiểm tra xung đột thời gian */
  checkTimeConflict(teacherCourseId: number, dayOfWeek: number, startTime: string, endTime: string, excludeScheduleId?: number): Observable<{hasConflict: boolean}> {
    let params = new HttpParams()
      .set('teacherCourseId', teacherCourseId.toString())
      .set('dayOfWeek', dayOfWeek.toString())
      .set('startTime', startTime)
      .set('endTime', endTime);

    if (excludeScheduleId) {
      params = params.set('excludeScheduleId', excludeScheduleId.toString());
    }

    return this.http.get<{hasConflict: boolean}>(`${this.apiUrl}/check-time-conflict`, { params });
  }

  /** Kiểm tra xung đột phòng học */
  checkRoomConflict(roomNumber: string, dayOfWeek: number, startTime: string, endTime: string, excludeScheduleId?: number): Observable<{hasConflict: boolean}> {
    let params = new HttpParams()
      .set('roomNumber', roomNumber)
      .set('dayOfWeek', dayOfWeek.toString())
      .set('startTime', startTime)
      .set('endTime', endTime);

    if (excludeScheduleId) {
      params = params.set('excludeScheduleId', excludeScheduleId.toString());
    }

    return this.http.get<{hasConflict: boolean}>(`${this.apiUrl}/check-room-conflict`, { params });
  }

  /** Lấy lịch học theo tuần (dành cho GV/quản trị) */
  getWeeklySchedule(teacherId?: string, courseId?: string): Observable<Schedule[]> {
    let params = new HttpParams();
    if (teacherId) params = params.set('teacherId', teacherId);
    if (courseId) params = params.set('courseId', courseId);
    return this.http.get<Schedule[]>(`${this.apiUrl}/weekly`, { params });
  }

  /** Lấy lịch học theo ngày (dành cho GV/quản trị) */
  getDailySchedule(dayOfWeek: number, teacherId?: string): Observable<Schedule[]> {
    let params = new HttpParams();
    if (teacherId) params = params.set('teacherId', teacherId);
    return this.http.get<Schedule[]>(`${this.apiUrl}/daily/${dayOfWeek}`, { params });
  }

  /** Lịch phòng */
  getRoomSchedule(roomNumber: string, dayOfWeek?: number): Observable<Schedule[]> {
    let params = new HttpParams();
    if (dayOfWeek != null) params = params.set('dayOfWeek', dayOfWeek.toString());
    return this.http.get<Schedule[]>(`${this.apiUrl}/room-schedule/${roomNumber}`, { params });
  }
}
