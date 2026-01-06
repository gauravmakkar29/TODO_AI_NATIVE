import { useState, useRef, useEffect } from 'react'
import { Tag } from '../types/category'
import './TagInput.css'

interface TagInputProps {
  availableTags: Tag[]
  selectedTags: Tag[]
  onTagsChange: (tags: Tag[]) => void
  onCreateTag?: (tagName: string) => Promise<Tag>
}

const TagInput = ({ availableTags, selectedTags, onTagsChange, onCreateTag }: TagInputProps) => {
  const [inputValue, setInputValue] = useState('')
  const [showSuggestions, setShowSuggestions] = useState(false)
  const inputRef = useRef<HTMLInputElement>(null)

  const filteredTags = availableTags.filter(
    (tag) =>
      !selectedTags.some((selected) => selected.id === tag.id) &&
      tag.name.toLowerCase().includes(inputValue.toLowerCase())
  )

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setInputValue(e.target.value)
    setShowSuggestions(true)
  }

  const handleTagSelect = (tag: Tag) => {
    if (!selectedTags.some((t) => t.id === tag.id)) {
      onTagsChange([...selectedTags, tag])
    }
    setInputValue('')
    setShowSuggestions(false)
    inputRef.current?.blur()
  }

  const handleTagRemove = (tagId: number) => {
    onTagsChange(selectedTags.filter((t) => t.id !== tagId))
  }

  const handleKeyDown = async (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === 'Enter' && inputValue.trim()) {
      e.preventDefault()
      const existingTag = availableTags.find(
        (tag) => tag.name.toLowerCase() === inputValue.trim().toLowerCase()
      )

      if (existingTag) {
        handleTagSelect(existingTag)
      } else if (onCreateTag) {
        try {
          const newTag = await onCreateTag(inputValue.trim())
          onTagsChange([...selectedTags, newTag])
          setInputValue('')
        } catch (error) {
          console.error('Failed to create tag:', error)
        }
      }
    } else if (e.key === 'Escape') {
      setShowSuggestions(false)
      inputRef.current?.blur()
    }
  }

  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (inputRef.current && !inputRef.current.contains(event.target as Node)) {
        setShowSuggestions(false)
      }
    }

    document.addEventListener('mousedown', handleClickOutside)
    return () => document.removeEventListener('mousedown', handleClickOutside)
  }, [])

  return (
    <div className="tag-input-container">
      <label className="tag-input-label">Tags</label>
      <div className="tag-input-wrapper" ref={inputRef}>
        <div className="selected-tags">
          {selectedTags.map((tag) => (
            <span key={tag.id} className="tag-badge">
              {tag.name}
              <button
                type="button"
                className="tag-remove"
                onClick={() => handleTagRemove(tag.id)}
                aria-label={`Remove ${tag.name}`}
              >
                ×
              </button>
            </span>
          ))}
          <input
            type="text"
            className="tag-input"
            placeholder={selectedTags.length === 0 ? 'Add tags...' : ''}
            value={inputValue}
            onChange={handleInputChange}
            onKeyDown={handleKeyDown}
            onFocus={() => setShowSuggestions(true)}
          />
        </div>
        {showSuggestions && (filteredTags.length > 0 || (inputValue.trim() && onCreateTag)) && (
          <div className="tag-suggestions">
            {filteredTags.map((tag) => (
              <div
                key={tag.id}
                className="tag-suggestion-item"
                onClick={() => handleTagSelect(tag)}
              >
                {tag.name}
              </div>
            ))}
            {inputValue.trim() &&
              onCreateTag &&
              !availableTags.some(
                (tag) => tag.name.toLowerCase() === inputValue.trim().toLowerCase()
              ) && (
                <div
                  className="tag-suggestion-item create-new"
                  onClick={async () => {
                    try {
                      const newTag = await onCreateTag(inputValue.trim())
                      onTagsChange([...selectedTags, newTag])
                      setInputValue('')
                    } catch (error) {
                      console.error('Failed to create tag:', error)
                    }
                  }}
                >
                  + Create "{inputValue.trim()}"
                </div>
              )}
          </div>
        )}
      </div>
    </div>
  )
}

export default TagInput

