import { useState, useEffect } from 'react'
import { sharingService, User } from '../services/sharingService'
import { SharePermission, ShareTodoRequest, ShareTodoResponse } from '../types/sharing'
import './ShareTask.css'

interface ShareTaskProps {
  todoId: number
  onClose: () => void
  onShareSuccess?: () => void
}

const ShareTask = ({ todoId, onClose, onShareSuccess }: ShareTaskProps) => {
  const [permission, setPermission] = useState<SharePermission>(SharePermission.ViewOnly)
  const [isAssigned, setIsAssigned] = useState(false)
  const [shares, setShares] = useState<ShareTodoResponse[]>([])
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [users, setUsers] = useState<User[]>([])
  const [searchQuery, setSearchQuery] = useState('')
  const [selectedUser, setSelectedUser] = useState<User | null>(null)
  const [searching, setSearching] = useState(false)

  useEffect(() => {
    loadShares()
  }, [todoId])

  useEffect(() => {
    if (searchQuery.trim().length >= 2) {
      searchUsers()
    } else {
      setUsers([])
      setSelectedUser(null)
    }
  }, [searchQuery])

  const loadShares = async () => {
    try {
      const data = await sharingService.getTodoShares(todoId)
      setShares(data)
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to load shares')
    }
  }

  const searchUsers = async () => {
    setSearching(true)
    try {
      const results = await sharingService.searchUsers(searchQuery)
      setUsers(results)
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to search users')
    } finally {
      setSearching(false)
    }
  }

  const handleShare = async () => {
    if (!selectedUser) {
      setError('Please select a user')
      return
    }

    setLoading(true)
    setError(null)

    try {
      const request: ShareTodoRequest = {
        todoId,
        sharedWithUserId: selectedUser.id,
        permission,
        isAssigned,
      }

      await sharingService.shareTodo(request)
      await loadShares()
      setSearchQuery('')
      setSelectedUser(null)
      onShareSuccess?.()
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to share task')
    } finally {
      setLoading(false)
    }
  }

  const handleUserSelect = (user: User) => {
    setSelectedUser(user)
    setUsers([])
    setSearchQuery(user.email)
  }

  const handleUnshare = async (sharedWithUserId: number) => {
    if (!confirm('Are you sure you want to unshare this task?')) {
      return
    }

    try {
      await sharingService.unshareTodo(todoId, sharedWithUserId)
      await loadShares()
      onShareSuccess?.()
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to unshare task')
    }
  }

  const handleUpdatePermission = async (sharedWithUserId: number, newPermission: SharePermission) => {
    try {
      await sharingService.updatePermission(todoId, sharedWithUserId, { permission: newPermission })
      await loadShares()
      onShareSuccess?.()
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to update permission')
    }
  }

  const getPermissionLabel = (perm: SharePermission) => {
    switch (perm) {
      case SharePermission.ViewOnly:
        return 'View Only'
      case SharePermission.Edit:
        return 'Edit'
      case SharePermission.Admin:
        return 'Admin'
      default:
        return 'Unknown'
    }
  }

  return (
    <div className="share-task-overlay" onClick={onClose}>
      <div className="share-task-modal" onClick={(e) => e.stopPropagation()}>
        <div className="share-task-header">
          <h2>Share Task</h2>
          <button className="close-button" onClick={onClose}>×</button>
        </div>

        {error && <div className="error-message">{error}</div>}

        <div className="share-task-content">
          <div className="share-form">
            <div className="form-group">
              <label htmlFor="email">Search User</label>
              <div className="user-search-container">
                <input
                  id="email"
                  type="text"
                  value={searchQuery}
                  onChange={(e) => setSearchQuery(e.target.value)}
                  placeholder="Search by email or name..."
                  disabled={loading}
                  autoComplete="off"
                />
                {searching && <div className="search-loading">Searching...</div>}
                {users.length > 0 && !selectedUser && (
                  <ul className="user-search-results">
                    {users.map((user) => (
                      <li
                        key={user.id}
                        className="user-search-item"
                        onClick={() => handleUserSelect(user)}
                      >
                        <div className="user-email">{user.email}</div>
                        {(user.firstName || user.lastName) && (
                          <div className="user-name">
                            {user.firstName} {user.lastName}
                          </div>
                        )}
                      </li>
                    ))}
                  </ul>
                )}
              </div>
              {selectedUser && (
                <div className="selected-user">
                  Selected: {selectedUser.email}
                  <button
                    type="button"
                    onClick={() => {
                      setSelectedUser(null)
                      setSearchQuery('')
                    }}
                    className="clear-selection"
                  >
                    ×
                  </button>
                </div>
              )}
            </div>

            <div className="form-group">
              <label htmlFor="permission">Permission</label>
              <select
                id="permission"
                value={permission}
                onChange={(e) => setPermission(Number(e.target.value) as SharePermission)}
                disabled={loading}
              >
                <option value={SharePermission.ViewOnly}>View Only</option>
                <option value={SharePermission.Edit}>Edit</option>
                <option value={SharePermission.Admin}>Admin</option>
              </select>
            </div>

            <div className="form-group">
              <label>
                <input
                  type="checkbox"
                  checked={isAssigned}
                  onChange={(e) => setIsAssigned(e.target.checked)}
                  disabled={loading}
                />
                Assign task to this user
              </label>
            </div>

            <button
              className="share-button"
              onClick={handleShare}
              disabled={loading || !selectedUser}
            >
              {loading ? 'Sharing...' : 'Share'}
            </button>
          </div>

          <div className="shares-list">
            <h3>Shared With</h3>
            {shares.length === 0 ? (
              <p className="no-shares">No one has access to this task yet.</p>
            ) : (
              <ul className="shares-list-items">
                {shares.map((share) => (
                  <li key={share.id} className="share-item">
                    <div className="share-info">
                      <div className="share-user">
                        <strong>{share.sharedWithUserEmail}</strong>
                        {share.isAssigned && <span className="assigned-badge">Assigned</span>}
                      </div>
                      <div className="share-meta">
                        <span className="permission-badge">{getPermissionLabel(share.permission)}</span>
                        <span className="shared-by">Shared by {share.sharedByUserEmail}</span>
                      </div>
                    </div>
                    <div className="share-actions">
                      <select
                        value={share.permission}
                        onChange={(e) =>
                          handleUpdatePermission(share.sharedWithUserId, Number(e.target.value) as SharePermission)
                        }
                        className="permission-select"
                      >
                        <option value={SharePermission.ViewOnly}>View Only</option>
                        <option value={SharePermission.Edit}>Edit</option>
                        <option value={SharePermission.Admin}>Admin</option>
                      </select>
                      <button
                        className="unshare-button"
                        onClick={() => handleUnshare(share.sharedWithUserId)}
                      >
                        Remove
                      </button>
                    </div>
                  </li>
                ))}
              </ul>
            )}
          </div>
        </div>
      </div>
    </div>
  )
}

export default ShareTask

