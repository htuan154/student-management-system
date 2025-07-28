export interface UserCreateDto {
  userId: string;
  username: string;
  email: string;
  password?: string; // Mật khẩu là bắt buộc khi tạo mới
  roleId: string;
  studentId?: string | null;
  employeeId?: string | null;
  teacherId?: string | null;
  isActive: boolean;
}
