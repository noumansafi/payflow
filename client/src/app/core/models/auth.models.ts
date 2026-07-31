export interface AuthUser {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  role: string;
  isEmailVerified: boolean;
}

export interface AuthTokens {
  accessToken: string;
  refreshToken: string;
  accessTokenExpiresAtUtc: string;
}

export interface AuthResponse {
  user: AuthUser;
  tokens: AuthTokens;
}

export interface RegisterResponse {
  user: AuthUser;
  emailVerificationToken: string;
}

export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  errors?: Record<string, string[]>;
}
