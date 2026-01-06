import { Todo } from '../types/todo'
import './CalendarView.css'

interface CalendarViewProps {
  todos: Todo[]
  onTodoClick: (todo: Todo) => void
}

const CalendarView = ({ todos, onTodoClick }: CalendarViewProps) => {
  const today = new Date()
  const currentMonth = today.getMonth()
  const currentYear = today.getFullYear()

  // Get first day of month and number of days
  const firstDay = new Date(currentYear, currentMonth, 1)
  const lastDay = new Date(currentYear, currentMonth + 1, 0)
  const daysInMonth = lastDay.getDate()
  const startingDayOfWeek = firstDay.getDay()

  // Group todos by date
  const todosByDate = new Map<string, Todo[]>()
  todos.forEach((todo) => {
    if (todo.dueDate) {
      const dateKey = new Date(todo.dueDate).toISOString().split('T')[0]
      if (!todosByDate.has(dateKey)) {
        todosByDate.set(dateKey, [])
      }
      todosByDate.get(dateKey)!.push(todo)
    }
  })

  // Generate calendar days
  const days: (Date | null)[] = []
  
  // Add empty cells for days before month starts
  for (let i = 0; i < startingDayOfWeek; i++) {
    days.push(null)
  }
  
  // Add days of the month
  for (let day = 1; day <= daysInMonth; day++) {
    days.push(new Date(currentYear, currentMonth, day))
  }

  const monthNames = [
    'January', 'February', 'March', 'April', 'May', 'June',
    'July', 'August', 'September', 'October', 'November', 'December'
  ]

  const dayNames = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat']

  const isToday = (date: Date | null) => {
    if (!date) return false
    const today = new Date()
    return (
      date.getDate() === today.getDate() &&
      date.getMonth() === today.getMonth() &&
      date.getFullYear() === today.getFullYear()
    )
  }

  const getDateKey = (date: Date | null) => {
    if (!date) return ''
    return date.toISOString().split('T')[0]
  }

  return (
    <div className="calendar-view">
      <h3 className="calendar-month">
        {monthNames[currentMonth]} {currentYear}
      </h3>
      <div className="calendar-grid">
        {dayNames.map((day) => (
          <div key={day} className="calendar-day-header">
            {day}
          </div>
        ))}
        {days.map((date, index) => {
          const dateKey = getDateKey(date)
          const dayTodos = date ? todosByDate.get(dateKey) || [] : []
          const hasTodos = dayTodos.length > 0
          const hasOverdue = dayTodos.some((t) => t.isOverdue)
          const hasApproaching = dayTodos.some((t) => t.isApproachingDue)

          return (
            <div
              key={index}
              className={`calendar-day ${!date ? 'empty' : ''} ${isToday(date) ? 'today' : ''} ${hasTodos ? 'has-todos' : ''} ${hasOverdue ? 'has-overdue' : ''} ${hasApproaching ? 'has-approaching' : ''}`}
            >
              {date && (
                <>
                  <div className="calendar-day-number">{date.getDate()}</div>
                  {hasTodos && (
                    <div className="calendar-todos">
                      {dayTodos.slice(0, 3).map((todo) => (
                        <div
                          key={todo.id}
                          className={`calendar-todo-item ${todo.isOverdue ? 'overdue' : ''} ${todo.isApproachingDue ? 'approaching' : ''}`}
                          onClick={() => onTodoClick(todo)}
                          title={todo.title}
                        >
                          <span className={`priority-dot priority-${todo.priority}`}></span>
                          <span className="calendar-todo-title">{todo.title}</span>
                        </div>
                      ))}
                      {dayTodos.length > 3 && (
                        <div className="calendar-todo-more">
                          +{dayTodos.length - 3} more
                        </div>
                      )}
                    </div>
                  )}
                </>
              )}
            </div>
          )
        })}
      </div>
    </div>
  )
}

export default CalendarView

