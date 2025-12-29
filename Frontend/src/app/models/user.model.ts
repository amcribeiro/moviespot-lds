export interface UserCreateDto {
  name: string;
  email: string;
  password: string;
  phone?: string;
  role: 'Staff' | 'User';
}

export interface UserUpdateDto {
  id: number;
  name: string;
  email: string;
  password?: string;
  phone?: string;
  role: 'Staff' | 'User';
  accountStatus?: string;
}

export interface UserResponseDto {
  id: number;
  name: string;
  email: string;
  phone?: string;
  role: string;
  accountStatus: string;
  createdAt: string;
  updatedAt: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  accessToken: string;
  refreshToken?: string;
}

export interface ForgotPasswordRequest {
  email: string;
}

export interface ResetPasswordRequest {
  token: string;
  newPassword: string;
}
