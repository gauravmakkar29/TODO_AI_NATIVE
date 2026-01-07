import { Category, Tag } from './category'

export enum TodoStatus {
  Pending = 0,
  Completed = 1,
  Archived = 2
}

export interface Todo {
  id: number
  title: string
  description?: string
  isCompleted: boolean
  status: TodoStatus
  isArchived: boolean
  createdAt: string
  updatedAt?: string
  completedAt?: string
  archivedAt?: string
  dueDate?: string
  reminderDate?: string
  priority: number // 0 = Low, 1 = Medium, 2 = High
  isOverdue?: boolean
  isApproachingDue?: boolean
  categories?: Category[]
  tags?: Tag[]
}

export interface CreateTodoRequest {
  title: string
  description?: string
  dueDate?: string
  reminderDate?: string
  priority?: number
  categoryIds?: number[]
  tagIds?: number[]
}

export interface UpdateTodoRequest {
  title?: string
  description?: string
  isCompleted?: boolean
  dueDate?: string
  reminderDate?: string
  priority?: number
  categoryIds?: number[]
  tagIds?: number[]
}

export interface BulkTodoRequest {
  todoIds: number[]
  isCompleted: boolean
}

export interface TodoStatistics {
  totalTodos: number
  completedTodos: number
  pendingTodos: number
  archivedTodos: number
  completionRate: number
  overdueTodos: number
  highPriorityTodos: number
  mediumPriorityTodos: number
  lowPriorityTodos: number
  completionByDate: Record<string, number>
}



