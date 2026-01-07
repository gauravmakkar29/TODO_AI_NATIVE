import { useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import { useAuth } from '../contexts/AuthContext'
import { todoService } from '../services/todoService'
import { categoryService, tagService } from '../services/categoryService'
import { Todo, CreateTodoRequest, UpdateTodoRequest } from '../types/todo'
import { Category, Tag } from '../types/category'
import CategorySelector from '../components/CategorySelector'
import TagInput from '../components/TagInput'
import CategoryManagement from '../components/CategoryManagement'
import SearchBar from '../components/SearchBar'
import AdvancedFilters, { FilterOptions } from '../components/AdvancedFilters'
import CalendarView from '../components/CalendarView'
import ThemeToggle from '../components/ThemeToggle'
import LoadingSpinner from '../components/LoadingSpinner'
import ErrorMessage from '../components/ErrorMessage'
import DraggableTodoList from '../components/DraggableTodoList'

const Dashboard = () => {
  const { user, logout } = useAuth()
  const navigate = useNavigate()
  const [todos, setTodos] = useState<Todo[]>([])
  const [filteredTodos, setFilteredTodos] = useState<Todo[]>([])
  const [categories, setCategories] = useState<Category[]>([])
  const [tags, setTags] = useState<Tag[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [showAddForm, setShowAddForm] = useState(false)
  const [showCategoryManagement, setShowCategoryManagement] = useState(false)
  const [editingTodo, setEditingTodo] = useState<Todo | null>(null)
  const [selectedCategoryFilter, setSelectedCategoryFilter] = useState<number | null>(null)
  const [selectedTagFilter, setSelectedTagFilter] = useState<number | null>(null)
  const [searchQuery, setSearchQuery] = useState<string>('')
  const [advancedFilters, setAdvancedFilters] = useState<FilterOptions>({
    priority: null,
    isCompleted: null,
    dueDateFrom: undefined,
    dueDateTo: undefined,
    createdAtFrom: undefined,
    createdAtTo: undefined,
    sortBy: 'createdAt',
    sortOrder: 'desc',
  })
 
  const [priorityFilter, setPriorityFilter] = useState<number | null>(null)
  const [showCalendarView, setShowCalendarView] = useState(false)
  const [formData, setFormData] = useState<CreateTodoRequest>({
    title: '',
    description: '',
    dueDate: '',
    reminderDate: '',
    priority: 0,
    categoryIds: [],
    tagIds: [],
  })
  const [selectedTags, setSelectedTags] = useState<Tag[]>([])

  useEffect(() => {
    loadInitialData()
  }, [])

  useEffect(() => {
    applyFilters()
  }, [todos, selectedCategoryFilter, selectedTagFilter, searchQuery, advancedFilters])

  useEffect(() => {
    loadTodos()
  }, [advancedFilters.sortBy, advancedFilters.sortOrder, priorityFilter])

  const loadInitialData = async () => {
    try {
      setLoading(true)
      setError(null)
      await Promise.all([loadTodos(), loadCategories(), loadTags()])
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to load data')
      console.error('Error loading data:', err)
    } finally {
      setLoading(false)
    }
  }

  const loadTodos = async () => {
    try {
      const sortByValue = advancedFilters.sortBy === 'createdAt' ? '' : 
                          advancedFilters.sortBy === 'priority' ? 'priority' :
                          advancedFilters.sortBy === 'dueDate' ? 'duedate' : '';
      const data = await todoService.getTodos(sortByValue || undefined, priorityFilter || undefined)
      setTodos(data)
    } catch (err: any) {
      console.error('Error loading todos:', err)
      throw err
    }
  }

  const loadCategories = async () => {
    try {
      const data = await categoryService.getCategories()
      setCategories(data)
    } catch (err: any) {
      console.error('Error loading categories:', err)
      // Don't throw - categories might not be implemented yet
    }
  }

  const loadTags = async () => {
    try {
      const data = await tagService.getTags()
      setTags(data)
    } catch (err: any) {
      console.error('Error loading tags:', err)
      // Don't throw - tags might not be implemented yet
    }
  }

  const applyFilters = () => {
    let filtered = [...todos]

    // Text search
    if (searchQuery.trim()) {
      const query = searchQuery.toLowerCase().trim()
      filtered = filtered.filter(
        (todo) =>
          todo.title.toLowerCase().includes(query) ||
          todo.description?.toLowerCase().includes(query) ||
          todo.categories?.some((cat) => cat.name.toLowerCase().includes(query)) ||
          todo.tags?.some((tag) => tag.name.toLowerCase().includes(query))
      )
    }

    // Category filter
    if (selectedCategoryFilter !== null) {
      filtered = filtered.filter(
        (todo) => todo.categories?.some((cat) => cat.id === selectedCategoryFilter)
      )
    }

    // Tag filter
    if (selectedTagFilter !== null) {
      filtered = filtered.filter(
        (todo) => todo.tags?.some((tag) => tag.id === selectedTagFilter)
      )
    }

    // Advanced filters
    if (advancedFilters.priority !== null && advancedFilters.priority !== undefined) {
      filtered = filtered.filter((todo) => todo.priority === advancedFilters.priority)
    }

    if (advancedFilters.isCompleted !== null && advancedFilters.isCompleted !== undefined) {
      if (advancedFilters.isCompleted === 'overdue') {
        const now = new Date()
        now.setHours(0, 0, 0, 0)
        filtered = filtered.filter(
          (todo) => !todo.isCompleted && todo.dueDate && new Date(todo.dueDate) < now
        )
      } else {
        filtered = filtered.filter((todo) => todo.isCompleted === advancedFilters.isCompleted)
      }
    }

    if (advancedFilters.dueDateFrom) {
      const fromDate = new Date(advancedFilters.dueDateFrom)
      filtered = filtered.filter((todo) => {
        if (!todo.dueDate) return false
        return new Date(todo.dueDate) >= fromDate
      })
    }

    if (advancedFilters.dueDateTo) {
      const toDate = new Date(advancedFilters.dueDateTo)
      toDate.setHours(23, 59, 59, 999) // Include the entire end date
      filtered = filtered.filter((todo) => {
        if (!todo.dueDate) return false
        return new Date(todo.dueDate) <= toDate
      })
    }

    if (advancedFilters.createdAtFrom) {
      const fromDate = new Date(advancedFilters.createdAtFrom)
      fromDate.setHours(0, 0, 0, 0)
      filtered = filtered.filter((todo) => {
        return new Date(todo.createdAt) >= fromDate
      })
    }

    if (advancedFilters.createdAtTo) {
      const toDate = new Date(advancedFilters.createdAtTo)
      toDate.setHours(23, 59, 59, 999) // Include the entire end date
      filtered = filtered.filter((todo) => {
        return new Date(todo.createdAt) <= toDate
      })
    }

    // Sorting
    const sortBy = advancedFilters.sortBy || 'createdAt'
    const sortOrder = advancedFilters.sortOrder || 'desc'

    filtered.sort((a, b) => {
      let comparison = 0

      switch (sortBy) {
        case 'title':
          comparison = a.title.localeCompare(b.title)
          break
        case 'priority':
          comparison = a.priority - b.priority
          break
        case 'dueDate':
          if (!a.dueDate && !b.dueDate) comparison = 0
          else if (!a.dueDate) comparison = 1
          else if (!b.dueDate) comparison = -1
          else comparison = new Date(a.dueDate).getTime() - new Date(b.dueDate).getTime()
          break
        case 'createdAt':
        default:
          comparison = new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime()
          break
      }

      return sortOrder === 'asc' ? comparison : -comparison
    })

    setFilteredTodos(filtered)
  }

  const clearAllFilters = () => {
    setSearchQuery('')
    setSelectedCategoryFilter(null)
    setSelectedTagFilter(null)
    setAdvancedFilters({
      priority: null,
      isCompleted: null,
      dueDateFrom: undefined,
      dueDateTo: undefined,
      createdAtFrom: undefined,
      createdAtTo: undefined,
      sortBy: 'createdAt',
      sortOrder: 'desc',
    })
  }

  const handleLogout = async () => {
    await logout()
    navigate('/login')
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    try {
      setError(null)
      const requestData = {
        ...formData,
        tagIds: selectedTags.map((tag) => tag.id),
      }
      if (editingTodo) {
        await todoService.updateTodo(editingTodo.id, requestData as UpdateTodoRequest)
      } else {
        await todoService.createTodo(requestData)
      }
      setShowAddForm(false)
      setEditingTodo(null)
      resetForm()
      await loadInitialData()
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
      reminderDate: todo.reminderDate ? todo.reminderDate.split('T')[0] : '',
      priority: todo.priority,
      categoryIds: todo.categories?.map((c) => c.id) || [],
      tagIds: [],
    })
    setSelectedTags(todo.tags || [])
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

  const resetForm = () => {
    setFormData({
      title: '',
      description: '',
      dueDate: '',
      reminderDate: '',
      priority: 0,
      categoryIds: [],
      tagIds: [],
    })
    setSelectedTags([])
  }

  const cancelForm = () => {
    setShowAddForm(false)
    setEditingTodo(null)
    resetForm()
  }

  const handleCreateTag = async (tagName: string): Promise<Tag> => {
    try {
      const newTag = await tagService.createTag({ name: tagName })
      setTags([...tags, newTag])
      return newTag
    } catch (err: any) {
      console.error('Error creating tag:', err)
      throw err
    }
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
    <div className="min-h-screen bg-gradient-to-br from-primary-500 to-purple-600 dark:from-gray-900 dark:to-gray-800 py-8 px-4 sm:px-6 lg:px-8">
      <div className="max-w-7xl mx-auto">
        <div className="card">
        <div className="dashboard-header flex justify-between items-center mb-6">
          <h1 className="text-3xl font-bold text-gray-900 dark:text-white">My Todo List</h1>
          <div className="flex items-center gap-4">
            <ThemeToggle />
            <button 
              onClick={handleLogout} 
              className="btn-secondary"
            >
              Logout
            </button>
          </div>
        </div>

        <div className="mb-6">
          <p className="text-gray-700 dark:text-gray-300">Welcome, {user?.email}!</p>
        </div>

        {error && (
          <ErrorMessage 
            message={error} 
            onDismiss={() => setError(null)}
            className="mb-4"
          />
        )}

        <div className="mb-6 space-y-4">
          <SearchBar searchQuery={searchQuery} onSearchChange={setSearchQuery} />
          <AdvancedFilters
            filters={advancedFilters}
            onFiltersChange={setAdvancedFilters}
            onClear={clearAllFilters}
          />
        </div>

        <div className="mb-6">
          <button
            onClick={() => setShowCategoryManagement(!showCategoryManagement)}
            className="btn-secondary mb-4"
          >
            {showCategoryManagement ? 'Hide' : 'Manage'} Categories
          </button>
          {showCategoryManagement && (
            <CategoryManagement onCategoriesChange={setCategories} />
          )}
        </div>

        <div className="mb-6 grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Filter by Category:</label>
            <select
              value={selectedCategoryFilter || ''}
              onChange={(e) =>
                setSelectedCategoryFilter(e.target.value ? parseInt(e.target.value) : null)
              }
              className="input-field"
            >
              <option value="">All Categories</option>
              {categories.map((cat) => (
                <option key={cat.id} value={cat.id}>
                  {cat.name}
                </option>
              ))}
            </select>
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Filter by Tag:</label>
            <select
              value={selectedTagFilter || ''}
              onChange={(e) =>
                setSelectedTagFilter(e.target.value ? parseInt(e.target.value) : null)
              }
              className="input-field"
            >
              <option value="">All Tags</option>
              {tags.map((tag) => (
                <option key={tag.id} value={tag.id}>
                  {tag.name}
                </option>
              ))}
            </select>
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Filter by Priority:</label>
            <select
              value={priorityFilter !== null ? priorityFilter.toString() : ''}
              onChange={(e) =>
                setPriorityFilter(e.target.value ? parseInt(e.target.value) : null)
              }
              className="input-field"
            >
              <option value="">All Priorities</option>
              <option value="2">High</option>
              <option value="1">Medium</option>
              <option value="0">Low</option>
            </select>
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Sort By:</label>
            <select
              value={advancedFilters.sortBy || 'createdAt'}
              onChange={(e) => setAdvancedFilters({ ...advancedFilters, sortBy: e.target.value })}
              className="input-field"
            >
              <option value="createdAt">Created Date (Newest)</option>
              <option value="priority">Priority (High to Low)</option>
              <option value="priority_asc">Priority (Low to High)</option>
              <option value="duedate">Due Date (Earliest)</option>
              <option value="duedate_desc">Due Date (Latest)</option>
            </select>
          </div>
        </div>
        {(selectedCategoryFilter !== null || selectedTagFilter !== null || priorityFilter !== null) && (
          <div className="mb-6">
            <button
              onClick={() => {
                setSelectedCategoryFilter(null)
                setSelectedTagFilter(null)
                setPriorityFilter(null)
              }}
              className="btn-secondary"
            >
              Clear Category/Tag Filters
            </button>
          </div>
        )}

        <div className="mt-6">
          <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center mb-4 gap-4">
            <h2 className="text-2xl font-bold text-gray-900 dark:text-white">Your Todos ({filteredTodos.length})</h2>
            <div className="flex gap-2">
              <button
                onClick={() => setShowCalendarView(!showCalendarView)}
                className="btn-secondary"
              >
                {showCalendarView ? 'List View' : 'Calendar View'}
              </button>
              <button
                onClick={() => {
                  cancelForm()
                  setShowAddForm(!showAddForm)
                }}
                className="btn-primary"
              >
                {showAddForm ? 'Cancel' : '+ Add Todo'}
              </button>
            </div>
          </div>

          {showAddForm && (
            <form onSubmit={handleSubmit} className="card mb-6 animate-slide-down">
              <div className="mb-4">
                <label htmlFor="title" className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Title *</label>
                <input
                  type="text"
                  id="title"
                  value={formData.title}
                  onChange={(e) => setFormData({ ...formData, title: e.target.value })}
                  required
                  placeholder="Enter todo title"
                  className="input-field"
                />
              </div>
              <div className="mb-4">
                <label htmlFor="description" className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Description</label>
                <textarea
                  id="description"
                  value={formData.description}
                  onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                  placeholder="Enter todo description"
                  rows={3}
                  className="input-field"
                />
              </div>
              {categories.length > 0 && (
                <CategorySelector
                  categories={categories}
                  selectedCategoryIds={formData.categoryIds || []}
                  onSelectionChange={(ids) => setFormData({ ...formData, categoryIds: ids })}
                />
              )}
              <TagInput
                availableTags={tags}
                selectedTags={selectedTags}
                onTagsChange={setSelectedTags}
                onCreateTag={handleCreateTag}
              />
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mb-4">
                <div>
                  <label htmlFor="dueDate" className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Due Date</label>
                  <input
                    type="date"
                    id="dueDate"
                    value={formData.dueDate}
                    onChange={(e) => setFormData({ ...formData, dueDate: e.target.value })}
                    className="input-field"
                  />
                </div>
                <div>
                  <label htmlFor="reminderDate" className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Reminder Date</label>
                  <input
                    type="date"
                    id="reminderDate"
                    value={formData.reminderDate}
                    onChange={(e) => setFormData({ ...formData, reminderDate: e.target.value })}
                    className="input-field"
                  />
                </div>
              </div>
              <div className="mb-4">
                <label htmlFor="priority" className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Priority</label>
                <select
                  id="priority"
                  value={formData.priority}
                  onChange={(e) => setFormData({ ...formData, priority: parseInt(e.target.value) })}
                  className="input-field"
                >
                  <option value={0}>Low</option>
                  <option value={1}>Medium</option>
                  <option value={2}>High</option>
                </select>
              </div>
              <div className="flex gap-2">
                <button type="submit" className="btn-primary">
                  {editingTodo ? 'Update Todo' : 'Add Todo'}
                </button>
                <button type="button" onClick={cancelForm} className="btn-secondary">
                  Cancel
                </button>
              </div>
            </form>
          )}

          {loading ? (
            <LoadingSpinner size="lg" className="py-12" />
          ) : showCalendarView ? (
            <CalendarView 
              todos={filteredTodos.filter(t => t.dueDate)} 
              onTodoClick={handleEdit}
            />
          ) : filteredTodos.length === 0 ? (
            <div className="text-center py-12 text-gray-600 dark:text-gray-400">
              {todos.length === 0
                ? 'No todos yet. Create your first todo!'
                : 'No todos match the selected filters.'}
            </div>
          ) : (
            <DraggableTodoList
              todos={filteredTodos}
              onReorder={(reorderedTodos) => {
                setTodos(reorderedTodos)
                setFilteredTodos(reorderedTodos)
              }}
              renderTodo={(todo) => (
                <div 
                  className={`card mb-3 transition-all duration-200 ${todo.isCompleted ? 'opacity-60' : ''} ${todo.isOverdue ? 'border-l-4 border-red-500' : ''} ${todo.isApproachingDue ? 'border-l-4 border-yellow-500' : ''}`}
                >
                  <div className="flex justify-between items-start">
                    <div className="flex-1">
                      <div className="flex items-center gap-3 mb-2">
                        <input
                          type="checkbox"
                          checked={todo.isCompleted}
                          onChange={() => handleToggleComplete(todo)}
                          className="w-5 h-5 text-primary-600 rounded focus:ring-primary-500"
                        />
                        <h3 className={`text-lg font-semibold text-gray-900 dark:text-white ${todo.isCompleted ? 'line-through' : ''}`}>
                          {todo.title}
                        </h3>
                        {todo.isOverdue && (
                          <span className="px-2 py-1 text-xs font-semibold text-white bg-red-500 rounded">Overdue</span>
                        )}
                        {todo.isApproachingDue && !todo.isOverdue && (
                          <span className="px-2 py-1 text-xs font-semibold text-white bg-yellow-500 rounded">Due Soon</span>
                        )}
                        <span className={`px-2 py-1 text-xs font-semibold rounded ${
                          todo.priority === 2 ? 'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200' :
                          todo.priority === 1 ? 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900 dark:text-yellow-200' :
                          'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200'
                        }`}>
                          {getPriorityLabel(todo.priority)}
                        </span>
                      </div>
                      {todo.description && (
                        <p className="text-gray-700 dark:text-gray-300 mb-2">{todo.description}</p>
                      )}
                      {todo.categories && todo.categories.length > 0 && (
                        <div className="flex flex-wrap gap-2 mb-2">
                          {todo.categories.map((category) => (
                            <span
                              key={category.id}
                              className="px-2 py-1 text-xs font-medium text-white rounded"
                              style={{
                                backgroundColor: category.color,
                              }}
                            >
                              {category.name}
                            </span>
                          ))}
                        </div>
                      )}
                      {todo.tags && todo.tags.length > 0 && (
                        <div className="flex flex-wrap gap-2 mb-2">
                          {todo.tags.map((tag) => (
                            <span key={tag.id} className="px-2 py-1 text-xs font-medium bg-gray-200 dark:bg-gray-700 text-gray-800 dark:text-gray-200 rounded">
                              #{tag.name}
                            </span>
                          ))}
                        </div>
                      )}
                      <div className="flex flex-wrap gap-4 text-sm text-gray-600 dark:text-gray-400">
                        {todo.dueDate && (
                          <span className={todo.isOverdue ? 'text-red-600 dark:text-red-400 font-semibold' : todo.isApproachingDue ? 'text-yellow-600 dark:text-yellow-400 font-semibold' : ''}>
                            Due: {new Date(todo.dueDate).toLocaleDateString()}
                          </span>
                        )}
                        {todo.reminderDate && (
                          <span>Reminder: {new Date(todo.reminderDate).toLocaleDateString()}</span>
                        )}
                        <span>Created: {new Date(todo.createdAt).toLocaleDateString()}</span>
                      </div>
                    </div>
                    <div className="flex gap-2 ml-4">
                      <button 
                        onClick={() => handleEdit(todo)} 
                        className="px-3 py-1 text-sm bg-blue-500 text-white rounded hover:bg-blue-600 transition-colors"
                      >
                        Edit
                      </button>
                      <button 
                        onClick={() => handleDelete(todo.id)} 
                        className="px-3 py-1 text-sm bg-red-500 text-white rounded hover:bg-red-600 transition-colors"
                      >
                        Delete
                      </button>
                    </div>
                  </div>
                </div>
              )}
            />
          )}
        </div>
      </div>
    </div>
  )
}

export default Dashboard
