import { Category, Tag } from './category'

export interface Todo {
  id: number
  title: string
  description?: string
  isCompleted: boolean
  createdAt: string
  updatedAt?: string
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



