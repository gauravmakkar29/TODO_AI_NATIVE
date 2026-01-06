import './SearchBar.css'

interface SearchBarProps {
  searchQuery: string
  onSearchChange: (query: string) => void
  placeholder?: string
}

const SearchBar = ({ searchQuery, onSearchChange, placeholder = 'Search todos...' }: SearchBarProps) => {
  return (
    <div className="search-bar-container">
      <div className="search-bar-wrapper">
        <svg
          className="search-icon"
          xmlns="http://www.w3.org/2000/svg"
          width="20"
          height="20"
          viewBox="0 0 24 24"
          fill="none"
          stroke="currentColor"
          strokeWidth="2"
          strokeLinecap="round"
          strokeLinejoin="round"
        >
          <circle cx="11" cy="11" r="8"></circle>
          <path d="m21 21-4.35-4.35"></path>
        </svg>
        <input
          type="text"
          className="search-input"
          placeholder={placeholder}
          value={searchQuery}
          onChange={(e) => onSearchChange(e.target.value)}
        />
        {searchQuery && (
          <button
            type="button"
            className="clear-search-button"
            onClick={() => onSearchChange('')}
            aria-label="Clear search"
          >
            ×
          </button>
        )}
      </div>
    </div>
  )
}

export default SearchBar

