import React from 'react'
import {
  DndContext,
  closestCenter,
  KeyboardSensor,
  PointerSensor,
  useSensor,
  useSensors,
  DragEndEvent,
} from '@dnd-kit/core'
import {
  arrayMove,
  SortableContext,
  sortableKeyboardCoordinates,
  verticalListSortingStrategy,
  useSortable,
} from '@dnd-kit/sortable'
import { CSS } from '@dnd-kit/utilities'
import { Todo } from '../types/todo'
import { todoService } from '../services/todoService'

interface DraggableTodoListProps {
  todos: Todo[]
  onReorder: (reorderedTodos: Todo[]) => void
  renderTodo: (todo: Todo, index: number) => React.ReactNode
}

interface SortableTodoItemProps {
  todo: Todo
  children: React.ReactNode
}

const SortableTodoItem: React.FC<SortableTodoItemProps> = ({ todo, children }) => {
  const {
    attributes,
    listeners,
    setNodeRef,
    transform,
    transition,
    isDragging,
  } = useSortable({ id: todo.id })

  const style = {
    transform: CSS.Transform.toString(transform),
    transition,
    opacity: isDragging ? 0.5 : 1,
  }

  return (
    <div
      ref={setNodeRef}
      style={style}
      className={isDragging ? 'cursor-grabbing' : 'cursor-grab'}
    >
      <div {...attributes} {...listeners} className="touch-none">
        {children}
      </div>
    </div>
  )
}

const DraggableTodoList: React.FC<DraggableTodoListProps> = ({
  todos,
  onReorder,
  renderTodo,
}) => {
  const sensors = useSensors(
    useSensor(PointerSensor, {
      activationConstraint: {
        distance: 8,
      },
    }),
    useSensor(KeyboardSensor, {
      coordinateGetter: sortableKeyboardCoordinates,
    })
  )

  const handleDragEnd = async (event: DragEndEvent) => {
    const { active, over } = event

    if (over && active.id !== over.id) {
      const oldIndex = todos.findIndex((todo) => todo.id === active.id)
      const newIndex = todos.findIndex((todo) => todo.id === over.id)

      const reorderedTodos = arrayMove(todos, oldIndex, newIndex)
      onReorder(reorderedTodos)

      // Update display order in backend
      const todoOrders = reorderedTodos.map((todo, index) => ({
        todoId: todo.id,
        displayOrder: index,
      }))

      try {
        await todoService.reorderTodos(todoOrders)
      } catch (error) {
        console.error('Failed to save todo order:', error)
        // Optionally revert the order on error
      }
    }
  }

  return (
    <DndContext
      sensors={sensors}
      collisionDetection={closestCenter}
      onDragEnd={handleDragEnd}
    >
      <SortableContext items={todos.map((t) => t.id)} strategy={verticalListSortingStrategy}>
        <div className="space-y-3">
          {todos.map((todo, index) => (
            <SortableTodoItem key={todo.id} todo={todo}>
              {renderTodo(todo, index)}
            </SortableTodoItem>
          ))}
        </div>
      </SortableContext>
    </DndContext>
  )
}

export default DraggableTodoList


