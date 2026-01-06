export interface Todo {
  id: number
  title: string
  description?: string
  isCompleted: boolean
  createdAt: string
  updatedAt?: string
  dueDate?: string
  priority: number // 0 = Low, 1 = Medium, 2 = High
}

export interface CreateTodoRequest {
  title: string
  description?: string
  dueDate?: string
  priority?: number
}

export interface UpdateTodoRequest {
  title?: string
  description?: string
  isCompleted?: boolean
  dueDate?: string
  priority?: number
}



