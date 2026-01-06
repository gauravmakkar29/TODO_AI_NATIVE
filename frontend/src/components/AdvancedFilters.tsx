import { useState } from 'react'
import './AdvancedFilters.css'

export interface FilterOptions {
  priority?: number | null
  isCompleted?: boolean | 'overdue' | null
  dueDateFrom?: string
  dueDateTo?: string
  createdAtFrom?: string
  createdAtTo?: string
  sortBy?: 'createdAt' | 'dueDate' | 'priority' | 'title'
  sortOrder?: 'asc' | 'desc'
}

interface AdvancedFiltersProps {
  filters: FilterOptions
  onFiltersChange: (filters: FilterOptions) => void
  onClear: () => void
}

const AdvancedFilters = ({ filters, onFiltersChange, onClear }: AdvancedFiltersProps) => {
  const [isExpanded, setIsExpanded] = useState(false)

  const updateFilter = (key: keyof FilterOptions, value: any) => {
    onFiltersChange({ ...filters, [key]: value })
  }

  const hasActiveFilters = () => {
    return (
      filters.priority !== null &&
      filters.priority !== undefined &&
      filters.isCompleted !== null &&
      filters.isCompleted !== undefined &&
      filters.dueDateFrom !== undefined &&
      filters.dueDateTo !== undefined
    )
  }

  return (
    <div className="advanced-filters">
      <button
        type="button"
        className="toggle-filters-button"
        onClick={() => setIsExpanded(!isExpanded)}
      >
        <span>Advanced Filters</span>
        <svg
          className={`filter-icon ${isExpanded ? 'expanded' : ''}`}
          xmlns="http://www.w3.org/2000/svg"
          width="16"
          height="16"
          viewBox="0 0 24 24"
          fill="none"
          stroke="currentColor"
          strokeWidth="2"
          strokeLinecap="round"
          strokeLinejoin="round"
        >
          <polyline points="6 9 12 15 18 9"></polyline>
        </svg>
      </button>

      {isExpanded && (
        <div className="filters-panel">
          <div className="filters-grid">
            <div className="filter-group">
              <label htmlFor="priority-filter">Priority</label>
              <select
                id="priority-filter"
                value={filters.priority ?? ''}
                onChange={(e) =>
                  updateFilter('priority', e.target.value === '' ? null : parseInt(e.target.value))
                }
              >
                <option value="">All Priorities</option>
                <option value="0">Low</option>
                <option value="1">Medium</option>
                <option value="2">High</option>
              </select>
            </div>

            <div className="filter-group">
              <label htmlFor="status-filter">Status</label>
              <select
                id="status-filter"
                value={
                  filters.isCompleted === null || filters.isCompleted === undefined
                    ? ''
                    : filters.isCompleted === 'overdue'
                    ? 'overdue'
                    : filters.isCompleted === true
                    ? 'completed'
                    : 'pending'
                }
                onChange={(e) => {
                  if (e.target.value === '') {
                    updateFilter('isCompleted', null)
                  } else if (e.target.value === 'overdue') {
                    updateFilter('isCompleted', 'overdue' as any)
                  } else {
                    updateFilter('isCompleted', e.target.value === 'completed')
                  }
                }}
              >
                <option value="">All Status</option>
                <option value="pending">Pending</option>
                <option value="completed">Completed</option>
                <option value="overdue">Overdue</option>
              </select>
            </div>

            <div className="filter-group">
              <label htmlFor="due-date-from">Due Date From</label>
              <input
                type="date"
                id="due-date-from"
                value={filters.dueDateFrom || ''}
                onChange={(e) => updateFilter('dueDateFrom', e.target.value || undefined)}
              />
            </div>

            <div className="filter-group">
              <label htmlFor="due-date-to">Due Date To</label>
              <input
                type="date"
                id="due-date-to"
                value={filters.dueDateTo || ''}
                onChange={(e) => updateFilter('dueDateTo', e.target.value || undefined)}
              />
            </div>

            <div className="filter-group">
              <label htmlFor="created-date-from">Created Date From</label>
              <input
                type="date"
                id="created-date-from"
                value={filters.createdAtFrom || ''}
                onChange={(e) => updateFilter('createdAtFrom', e.target.value || undefined)}
              />
            </div>

            <div className="filter-group">
              <label htmlFor="created-date-to">Created Date To</label>
              <input
                type="date"
                id="created-date-to"
                value={filters.createdAtTo || ''}
                onChange={(e) => updateFilter('createdAtTo', e.target.value || undefined)}
              />
            </div>

            <div className="filter-group">
              <label htmlFor="sort-by">Sort By</label>
              <select
                id="sort-by"
                value={filters.sortBy || 'createdAt'}
                onChange={(e) => updateFilter('sortBy', e.target.value as any)}
              >
                <option value="createdAt">Created Date</option>
                <option value="dueDate">Due Date</option>
                <option value="priority">Priority</option>
                <option value="title">Title</option>
              </select>
            </div>

            <div className="filter-group">
              <label htmlFor="sort-order">Sort Order</label>
              <select
                id="sort-order"
                value={filters.sortOrder || 'desc'}
                onChange={(e) => updateFilter('sortOrder', e.target.value as 'asc' | 'desc')}
              >
                <option value="desc">Descending</option>
                <option value="asc">Ascending</option>
              </select>
            </div>
          </div>

          <div className="filters-actions">
            <button type="button" onClick={onClear} className="clear-filters-btn">
              Clear All Filters
            </button>
          </div>
        </div>
      )}
    </div>
  )
}

export default AdvancedFilters

