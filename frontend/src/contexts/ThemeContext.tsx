import React, { createContext, useContext, useEffect, useState } from 'react'
import { api } from '../services/authService'

interface ThemeContextType {
  theme: 'light' | 'dark'
  toggleTheme: () => void
  isLoading: boolean
}

const ThemeContext = createContext<ThemeContextType | undefined>(undefined)

export const ThemeProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [theme, setTheme] = useState<'light' | 'dark'>('light')
  const [isLoading, setIsLoading] = useState(true)

  useEffect(() => {
    // Load theme preference from API
    const loadTheme = async () => {
      try {
        const response = await api.get<{ theme: string }>('/userpreferences/theme')
        const savedTheme = response.data.theme as 'light' | 'dark'
        setTheme(savedTheme)
        applyTheme(savedTheme)
      } catch (error) {
        // Default to light theme if API call fails
        applyTheme('light')
      } finally {
        setIsLoading(false)
      }
    }

    loadTheme()
  }, [])

  const applyTheme = (newTheme: 'light' | 'dark') => {
    const root = document.documentElement
    if (newTheme === 'dark') {
      root.classList.add('dark')
    } else {
      root.classList.remove('dark')
    }
  }

  const toggleTheme = async () => {
    const newTheme = theme === 'light' ? 'dark' : 'light'
    setTheme(newTheme)
    applyTheme(newTheme)

    // Save to backend
    try {
      await api.put('/userpreferences/theme', { theme: newTheme })
    } catch (error) {
      console.error('Failed to save theme preference:', error)
    }
  }

  return (
    <ThemeContext.Provider value={{ theme, toggleTheme, isLoading }}>
      {children}
    </ThemeContext.Provider>
  )
}

export const useTheme = () => {
  const context = useContext(ThemeContext)
  if (context === undefined) {
    throw new Error('useTheme must be used within a ThemeProvider')
  }
  return context
}


