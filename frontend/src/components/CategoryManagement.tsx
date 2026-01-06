import { useState, useEffect } from 'react'
import { Category, CreateCategoryRequest } from '../types/category'
import { categoryService } from '../services/categoryService'
import './CategoryManagement.css'

interface CategoryManagementProps {
  onCategoriesChange: (categories: Category[]) => void
}

const PREDEFINED_COLORS = [
  '#667eea', // Purple
  '#f093fb', // Pink
  '#4facfe', // Blue
  '#43e97b', // Green
  '#fa709a', // Rose
  '#fee140', // Yellow
  '#30cfd0', // Cyan
  '#a8edea', // Aqua
  '#ff9a9e', // Coral
  '#fecfef', // Lavender
]

const CategoryManagement = ({ onCategoriesChange }: CategoryManagementProps) => {
  const [categories, setCategories] = useState<Category[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [showCreateForm, setShowCreateForm] = useState(false)
  const [editingCategory, setEditingCategory] = useState<Category | null>(null)
  const [formData, setFormData] = useState<CreateCategoryRequest>({
    name: '',
    color: PREDEFINED_COLORS[0],
  })

  useEffect(() => {
    loadCategories()
  }, [])

  const loadCategories = async () => {
    try {
      setLoading(true)
      setError(null)
      const data = await categoryService.getCategories()
      setCategories(data)
      onCategoriesChange(data)
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to load categories')
      console.error('Error loading categories:', err)
    } finally {
      setLoading(false)
    }
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    try {
      setError(null)
      if (editingCategory) {
        await categoryService.updateCategory(editingCategory.id, formData)
      } else {
        await categoryService.createCategory(formData)
      }
      setShowCreateForm(false)
      setEditingCategory(null)
      setFormData({ name: '', color: PREDEFINED_COLORS[0] })
      await loadCategories()
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to save category')
      console.error('Error saving category:', err)
    }
  }

  const handleEdit = (category: Category) => {
    setEditingCategory(category)
    setFormData({ name: category.name, color: category.color })
    setShowCreateForm(true)
  }

  const handleDelete = async (id: number) => {
    if (!window.confirm('Are you sure you want to delete this category? This will remove it from all todos.')) {
      return
    }
    try {
      setError(null)
      await categoryService.deleteCategory(id)
      await loadCategories()
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to delete category')
      console.error('Error deleting category:', err)
    }
  }

  const cancelForm = () => {
    setShowCreateForm(false)
    setEditingCategory(null)
    setFormData({ name: '', color: PREDEFINED_COLORS[0] })
  }

  if (loading) {
    return <div className="category-management-loading">Loading categories...</div>
  }

  return (
    <div className="category-management">
      <div className="category-management-header">
        <h3>Manage Categories</h3>
        <button
          onClick={() => {
            cancelForm()
            setShowCreateForm(!showCreateForm)
          }}
          className="add-category-button"
        >
          {showCreateForm ? 'Cancel' : '+ Add Category'}
        </button>
      </div>

      {error && <div className="error-message">{error}</div>}

      {showCreateForm && (
        <form onSubmit={handleSubmit} className="category-form">
          <div className="form-group">
            <label htmlFor="categoryName">Category Name *</label>
            <input
              type="text"
              id="categoryName"
              value={formData.name}
              onChange={(e) => setFormData({ ...formData, name: e.target.value })}
              required
              placeholder="Enter category name"
            />
          </div>
          <div className="form-group">
            <label>Color</label>
            <div className="color-picker">
              {PREDEFINED_COLORS.map((color) => (
                <button
                  key={color}
                  type="button"
                  className={`color-option ${formData.color === color ? 'selected' : ''}`}
                  style={{ backgroundColor: color }}
                  onClick={() => setFormData({ ...formData, color })}
                  aria-label={`Select color ${color}`}
                />
              ))}
            </div>
            <input
              type="color"
              value={formData.color}
              onChange={(e) => setFormData({ ...formData, color: e.target.value })}
              className="color-input"
            />
          </div>
          <div className="form-actions">
            <button type="submit" className="submit-button">
              {editingCategory ? 'Update Category' : 'Create Category'}
            </button>
            <button type="button" onClick={cancelForm} className="cancel-button">
              Cancel
            </button>
          </div>
        </form>
      )}

      <div className="categories-list">
        {categories.length === 0 ? (
          <div className="no-categories">No categories yet. Create your first category!</div>
        ) : (
          categories.map((category) => (
            <div key={category.id} className="category-item">
              <div className="category-info">
                <span
                  className="category-color-indicator"
                  style={{ backgroundColor: category.color }}
                />
                <span className="category-name">{category.name}</span>
              </div>
              <div className="category-actions">
                <button onClick={() => handleEdit(category)} className="edit-button">
                  Edit
                </button>
                <button onClick={() => handleDelete(category.id)} className="delete-button">
                  Delete
                </button>
              </div>
            </div>
          ))
        )}
      </div>
    </div>
  )
}

export default CategoryManagement

