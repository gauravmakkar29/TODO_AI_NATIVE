import { Category } from '../types/category'
import './CategorySelector.css'

interface CategorySelectorProps {
  categories: Category[]
  selectedCategoryIds: number[]
  onSelectionChange: (categoryIds: number[]) => void
  allowMultiple?: boolean
}

const CategorySelector = ({
  categories,
  selectedCategoryIds,
  onSelectionChange,
  allowMultiple = true,
}: CategorySelectorProps) => {
  const handleCategoryToggle = (categoryId: number) => {
    if (allowMultiple) {
      if (selectedCategoryIds.includes(categoryId)) {
        onSelectionChange(selectedCategoryIds.filter((id) => id !== categoryId))
      } else {
        onSelectionChange([...selectedCategoryIds, categoryId])
      }
    } else {
      onSelectionChange(selectedCategoryIds.includes(categoryId) ? [] : [categoryId])
    }
  }

  return (
    <div className="category-selector">
      <label className="category-selector-label">Categories</label>
      <div className="category-chips">
        {categories.length === 0 ? (
          <span className="no-categories">No categories available. Create one first.</span>
        ) : (
          categories.map((category) => (
            <button
              key={category.id}
              type="button"
              className={`category-chip ${selectedCategoryIds.includes(category.id) ? 'selected' : ''}`}
              style={{
                backgroundColor: selectedCategoryIds.includes(category.id) ? category.color : 'transparent',
                borderColor: category.color,
                color: selectedCategoryIds.includes(category.id) ? '#fff' : category.color,
              }}
              onClick={() => handleCategoryToggle(category.id)}
            >
              <span
                className="category-color-indicator"
                style={{ backgroundColor: category.color }}
              />
              {category.name}
            </button>
          ))
        )}
      </div>
    </div>
  )
}

export default CategorySelector

