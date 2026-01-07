import { useState, useEffect } from 'react'
import { commentService } from '../services/commentService'
import { Comment, CreateCommentRequest } from '../types/sharing'
import './TaskComments.css'

interface TaskCommentsProps {
  todoId: number
  currentUserEmail?: string
}

const TaskComments = ({ todoId, currentUserEmail }: TaskCommentsProps) => {
  const [comments, setComments] = useState<Comment[]>([])
  const [newComment, setNewComment] = useState('')
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    loadComments()
  }, [todoId])

  const loadComments = async () => {
    try {
      const data = await commentService.getTodoComments(todoId)
      setComments(data)
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to load comments')
    }
  }

  const handleAddComment = async () => {
    if (!newComment.trim()) {
      return
    }

    setLoading(true)
    setError(null)

    try {
      const request: CreateCommentRequest = {
        todoId,
        comment: newComment.trim(),
      }
      await commentService.createComment(request)
      setNewComment('')
      await loadComments()
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to add comment')
    } finally {
      setLoading(false)
    }
  }

  const handleDeleteComment = async (commentId: number) => {
    if (!confirm('Are you sure you want to delete this comment?')) {
      return
    }

    try {
      await commentService.deleteComment(commentId)
      await loadComments()
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to delete comment')
    }
  }

  const formatDate = (dateString: string) => {
    const date = new Date(dateString)
    return date.toLocaleString()
  }

  return (
    <div className="task-comments">
      <h3 className="comments-header">Comments</h3>

      {error && <div className="error-message">{error}</div>}

      <div className="comments-list">
        {comments.length === 0 ? (
          <p className="no-comments">No comments yet. Be the first to comment!</p>
        ) : (
          comments.map((comment) => (
            <div key={comment.id} className="comment-item">
              <div className="comment-header">
                <div className="comment-author">
                  <strong>{comment.userName || comment.userEmail}</strong>
                  {comment.userEmail === currentUserEmail && (
                    <span className="you-badge">You</span>
                  )}
                </div>
                <div className="comment-meta">
                  <span className="comment-date">{formatDate(comment.createdAt)}</span>
                  {comment.updatedAt && comment.updatedAt !== comment.createdAt && (
                    <span className="comment-edited">(edited)</span>
                  )}
                </div>
              </div>
              <div className="comment-content">{comment.comment}</div>
              {comment.userEmail === currentUserEmail && (
                <button
                  className="delete-comment-button"
                  onClick={() => handleDeleteComment(comment.id)}
                >
                  Delete
                </button>
              )}
            </div>
          ))
        )}
      </div>

      <div className="add-comment">
        <textarea
          className="comment-input"
          value={newComment}
          onChange={(e) => setNewComment(e.target.value)}
          placeholder="Add a comment..."
          rows={3}
          disabled={loading}
        />
        <button
          className="add-comment-button"
          onClick={handleAddComment}
          disabled={loading || !newComment.trim()}
        >
          {loading ? 'Adding...' : 'Add Comment'}
        </button>
      </div>
    </div>
  )
}

export default TaskComments

