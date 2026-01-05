export interface User {
  id: number
  email: string
  firstName?: string
  lastName?: string
  emailVerified: boolean
}

export interface AuthResponse {
  token: string
  refreshToken: string
  expiresAt: string
  user: User
}

