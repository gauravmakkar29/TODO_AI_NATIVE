import { useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import { useAuth } from '../contexts/AuthContext'
import { todoService } from '../services/todoService'
import { Todo, CreateTodoRequest, UpdateTodoRequest } from '../types/todo'
import './Dashboard.css'

const Dashboard = () => {
  const { user, logout } = useAuth()
  const navigate = useNavigate()
  const [todos, setTodos] = useState<Todo[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [showAddForm, setShowAddForm] = useState(false)
  const [editingTodo, setEditingTodo] = useState<Todo | null>(null)
  const [formData, setFormData] = useState<CreateTodoRequest>({
    title: '',
    description: '',
    dueDate: '',
    priority: 0,
  })

  useEffect(() => {
    loadTodos()
  }, [])

  const loadTodos = async () => {
    try {
      setLoading(true)
      setError(null)
      const data = await todoService.getTodos()
      setTodos(data)
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to load todos')
      console.error('Error loading todos:', err)
    } finally {
      setLoading(false)
    }
  }

  const handleLogout = async () => {
    await logout()
    navigate('/login')
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    try {
      setError(null)
      if (editingTodo) {
        await todoService.updateTodo(editingTodo.id, formData as UpdateTodoRequest)
      } else {
        await todoService.createTodo(formData)
      }
      setShowAddForm(false)
      setEditingTodo(null)
      setFormData({ title: '', description: '', dueDate: '', priority: 0 })
      await loadTodos()
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to save todo')
      console.error('Error saving todo:', err)
    }
  }

  const handleEdit = (todo: Todo) => {
    setEditingTodo(todo)
    setFormData({
      title: todo.title,
      description: todo.description || '',
      dueDate: todo.dueDate ? todo.dueDate.split('T')[0] : '',
      priority: todo.priority,
    })
    setShowAddForm(true)
  }

  const handleDelete = async (id: number) => {
    if (!window.confirm('Are you sure you want to delete this todo?')) {
      return
    }
    try {
      setError(null)
      await todoService.deleteTodo(id)
      await loadTodos()
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to delete todo')
      console.error('Error deleting todo:', err)
    }
  }

  const handleToggleComplete = async (todo: Todo) => {
    try {
      setError(null)
      await todoService.updateTodo(todo.id, { isCompleted: !todo.isCompleted })
      await loadTodos()
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to update todo')
      console.error('Error updating todo:', err)
    }
  }

  const cancelForm = () => {
    setShowAddForm(false)
    setEditingTodo(null)
    setFormData({ title: '', description: '', dueDate: '', priority: 0 })
  }

  const getPriorityLabel = (priority: number) => {
    switch (priority) {
      case 2:
        return 'High'
      case 1:
        return 'Medium'
      default:
        return 'Low'
    }
  }

  const getPriorityClass = (priority: number) => {
    switch (priority) {
      case 2:
        return 'priority-high'
      case 1:
        return 'priority-medium'
      default:
        return 'priority-low'
    }
  }

  return (
    <div className="dashboard-container">
      <div className="dashboard-card">
        <div className="dashboard-header">
          <h1>My Todo List</h1>
          <button onClick={handleLogout} className="logout-button">
            Logout
          </button>
        </div>

        <div className="user-info">
          <p className="welcome-text">Welcome, {user?.email}!</p>
        </div>

        {error && <div className="error-message">{error}</div>}

        <div className="todos-section">
          <div className="todos-header">
            <h2>Your Todos</h2>
            <button
              onClick={() => {
                cancelForm()
                setShowAddForm(!showAddForm)
              }}
              className="add-todo-button"
            >
              {showAddForm ? 'Cancel' : '+ Add Todo'}
            </button>
          </div>

          {showAddForm && (
            <form onSubmit={handleSubmit} className="todo-form">
              <div className="form-group">
                <label htmlFor="title">Title *</label>
                <input
                  type="text"
                  id="title"
                  value={formData.title}
                  onChange={(e) => setFormData({ ...formData, title: e.target.value })}
                  required
                  placeholder="Enter todo title"
                />
              </div>
              <div className="form-group">
                <label htmlFor="description">Description</label>
                <textarea
                  id="description"
                  value={formData.description}
                  onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                  placeholder="Enter todo description"
                  rows={3}
                />
              </div>
              <div className="form-row">
                <div className="form-group">
                  <label htmlFor="dueDate">Due Date</label>
                  <input
                    type="date"
                    id="dueDate"
                    value={formData.dueDate}
                    onChange={(e) => setFormData({ ...formData, dueDate: e.target.value })}
                  />
                </div>
                <div className="form-group">
                  <label htmlFor="priority">Priority</label>
                  <select
                    id="priority"
                    value={formData.priority}
                    onChange={(e) => setFormData({ ...formData, priority: parseInt(e.target.value) })}
                  >
                    <option value={0}>Low</option>
                    <option value={1}>Medium</option>
                    <option value={2}>High</option>
                  </select>
                </div>
              </div>
              <div className="form-actions">
                <button type="submit" className="submit-button">
                  {editingTodo ? 'Update Todo' : 'Add Todo'}
                </button>
                <button type="button" onClick={cancelForm} className="cancel-button">
                  Cancel
                </button>
              </div>
            </form>
          )}

          {loading ? (
            <div className="loading">Loading todos...</div>
          ) : todos.length === 0 ? (
            <div className="no-todos">No todos yet. Create your first todo!</div>
          ) : (
            <div className="todos-list">
              {todos.map((todo) => (
                <div key={todo.id} className={`todo-item ${todo.isCompleted ? 'completed' : ''}`}>
                  <div className="todo-content">
                    <div className="todo-header">
                      <input
                        type="checkbox"
                        checked={todo.isCompleted}
                        onChange={() => handleToggleComplete(todo)}
                        className="todo-checkbox"
                      />
                      <h3 className="todo-title">{todo.title}</h3>
                      <span className={`priority-badge ${getPriorityClass(todo.priority)}`}>
                        {getPriorityLabel(todo.priority)}
                      </span>
                    </div>
                    {todo.description && <p className="todo-description">{todo.description}</p>}
                    <div className="todo-meta">
                      {todo.dueDate && (
                        <span className="todo-due-date">
                          Due: {new Date(todo.dueDate).toLocaleDateString()}
                        </span>
                      )}
                      <span className="todo-date">
                        Created: {new Date(todo.createdAt).toLocaleDateString()}
                      </span>
                    </div>
                  </div>
                  <div className="todo-actions">
                    <button onClick={() => handleEdit(todo)} className="edit-button">
                      Edit
                    </button>
                    <button onClick={() => handleDelete(todo.id)} className="delete-button">
                      Delete
                    </button>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      </div>
    </div>
  )
}

export default Dashboard
