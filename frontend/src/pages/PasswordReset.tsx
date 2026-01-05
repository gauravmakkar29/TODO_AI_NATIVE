import { useState } from 'react'
import { Link } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { authService } from '../services/authService'
import './Auth.css'

const passwordResetRequestSchema = z.object({
  email: z.string().email('Invalid email address'),
})

const passwordResetConfirmSchema = z
  .object({
    email: z.string().email('Invalid email address'),
    token: z.string().min(1, 'Token is required'),
    newPassword: z
      .string()
      .min(8, 'Password must be at least 8 characters')
      .regex(
        /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]/,
        'Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character'
      ),
    confirmPassword: z.string(),
  })
  .refine((data) => data.newPassword === data.confirmPassword, {
    message: "Passwords don't match",
    path: ['confirmPassword'],
  })

type PasswordResetRequestFormData = z.infer<typeof passwordResetRequestSchema>
type PasswordResetConfirmFormData = z.infer<typeof passwordResetConfirmSchema>

const PasswordReset = () => {
  const [step, setStep] = useState<'request' | 'confirm'>('request')
  const [message, setMessage] = useState<string>('')
  const [error, setError] = useState<string>('')
  const [loading, setLoading] = useState(false)

  const requestForm = useForm<PasswordResetRequestFormData>({
    resolver: zodResolver(passwordResetRequestSchema),
  })

  const confirmForm = useForm<PasswordResetConfirmFormData>({
    resolver: zodResolver(passwordResetConfirmSchema),
  })

  const onRequestSubmit = async (data: PasswordResetRequestFormData) => {
    setError('')
    setMessage('')
    setLoading(true)
    try {
      const response = await authService.requestPasswordReset(data.email)
      setMessage(response)
      setStep('confirm')
      confirmForm.setValue('email', data.email)
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to request password reset.')
    } finally {
      setLoading(false)
    }
  }

  const onConfirmSubmit = async (data: PasswordResetConfirmFormData) => {
    setError('')
    setMessage('')
    setLoading(true)
    try {
      await authService.confirmPasswordReset(
        data.email,
        data.token,
        data.newPassword,
        data.confirmPassword
      )
      setMessage('Password reset successfully! You can now login with your new password.')
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to reset password.')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="auth-container">
      <div className="auth-card">
        <h1>Password Reset</h1>

        {message && <div className="success-message">{message}</div>}
        {error && <div className="error-message">{error}</div>}

        {step === 'request' ? (
          <>
            <p className="auth-subtitle">Enter your email to receive a password reset token.</p>
            <form onSubmit={requestForm.handleSubmit(onRequestSubmit)} className="auth-form">
              <div className="form-group">
                <label htmlFor="email">Email</label>
                <input
                  id="email"
                  type="email"
                  {...requestForm.register('email')}
                  className={requestForm.formState.errors.email ? 'error' : ''}
                />
                {requestForm.formState.errors.email && (
                  <span className="field-error">
                    {requestForm.formState.errors.email.message}
                  </span>
                )}
              </div>

              <button type="submit" className="auth-button" disabled={loading}>
                {loading ? 'Sending...' : 'Send Reset Token'}
              </button>
            </form>
          </>
        ) : (
          <>
            <p className="auth-subtitle">Enter the token and your new password.</p>
            <form onSubmit={confirmForm.handleSubmit(onConfirmSubmit)} className="auth-form">
              <div className="form-group">
                <label htmlFor="confirm-email">Email</label>
                <input
                  id="confirm-email"
                  type="email"
                  {...confirmForm.register('email')}
                  className={confirmForm.formState.errors.email ? 'error' : ''}
                  readOnly
                />
                {confirmForm.formState.errors.email && (
                  <span className="field-error">
                    {confirmForm.formState.errors.email.message}
                  </span>
                )}
              </div>

              <div className="form-group">
                <label htmlFor="token">Reset Token</label>
                <input
                  id="token"
                  type="text"
                  {...confirmForm.register('token')}
                  className={confirmForm.formState.errors.token ? 'error' : ''}
                />
                {confirmForm.formState.errors.token && (
                  <span className="field-error">
                    {confirmForm.formState.errors.token.message}
                  </span>
                )}
              </div>

              <div className="form-group">
                <label htmlFor="newPassword">New Password</label>
                <input
                  id="newPassword"
                  type="password"
                  {...confirmForm.register('newPassword')}
                  className={confirmForm.formState.errors.newPassword ? 'error' : ''}
                />
                {confirmForm.formState.errors.newPassword && (
                  <span className="field-error">
                    {confirmForm.formState.errors.newPassword.message}
                  </span>
                )}
                <small className="password-hint">
                  Must be at least 8 characters with uppercase, lowercase, number, and special
                  character
                </small>
              </div>

              <div className="form-group">
                <label htmlFor="confirmPassword">Confirm New Password</label>
                <input
                  id="confirmPassword"
                  type="password"
                  {...confirmForm.register('confirmPassword')}
                  className={confirmForm.formState.errors.confirmPassword ? 'error' : ''}
                />
                {confirmForm.formState.errors.confirmPassword && (
                  <span className="field-error">
                    {confirmForm.formState.errors.confirmPassword.message}
                  </span>
                )}
              </div>

              <button type="submit" className="auth-button" disabled={loading}>
                {loading ? 'Resetting...' : 'Reset Password'}
              </button>
            </form>
          </>
        )}

        <p className="auth-footer">
          <Link to="/login">Back to Login</Link>
        </p>
      </div>
    </div>
  )
}

export default PasswordReset

