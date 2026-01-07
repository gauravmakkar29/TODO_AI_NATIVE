// Local cache service using IndexedDB and localStorage
const DB_NAME = 'TodoAppDB';
const DB_VERSION = 1;
const STORE_NAME = 'todos';
const CACHE_PREFIX = 'todo_cache_';
const CACHE_EXPIRY_PREFIX = 'todo_cache_expiry_';

interface CacheEntry<T> {
  data: T;
  timestamp: number;
  expiry: number;
}

class CacheService {
  private db: IDBDatabase | null = null;

  async init(): Promise<void> {
    return new Promise((resolve, reject) => {
      const request = indexedDB.open(DB_NAME, DB_VERSION);

      request.onerror = () => reject(request.error);
      request.onsuccess = () => {
        this.db = request.result;
        resolve();
      };

      request.onupgradeneeded = (event) => {
        const db = (event.target as IDBOpenDBRequest).result;
        if (!db.objectStoreNames.contains(STORE_NAME)) {
          db.createObjectStore(STORE_NAME, { keyPath: 'id' });
        }
      };
    });
  }

  // Get from cache
  async get<T>(key: string): Promise<T | null> {
    try {
      // Check localStorage first (faster for small data)
      const cached = localStorage.getItem(CACHE_PREFIX + key);
      if (cached) {
        const entry: CacheEntry<T> = JSON.parse(cached);
        const now = Date.now();
        
        if (entry.expiry && now > entry.expiry) {
          // Expired, remove it
          localStorage.removeItem(CACHE_PREFIX + key);
          localStorage.removeItem(CACHE_EXPIRY_PREFIX + key);
          return null;
        }
        
        return entry.data;
      }

      // Fallback to IndexedDB for larger data
      if (this.db) {
        const transaction = this.db.transaction([STORE_NAME], 'readonly');
        const store = transaction.objectStore(STORE_NAME);
        const request = store.get(key);
        
        return new Promise((resolve) => {
          request.onsuccess = () => {
            const entry = request.result as CacheEntry<T> | undefined;
            if (entry) {
              const now = Date.now();
              if (entry.expiry && now > entry.expiry) {
                this.remove(key);
                resolve(null);
              } else {
                resolve(entry.data);
              }
            } else {
              resolve(null);
            }
          };
          request.onerror = () => resolve(null);
        });
      }
    } catch (error) {
      console.error('Cache get error:', error);
    }
    
    return null;
  }

  // Set cache
  async set<T>(key: string, data: T, expiryMinutes: number = 5): Promise<void> {
    try {
      const now = Date.now();
      const expiry = expiryMinutes > 0 ? now + (expiryMinutes * 60 * 1000) : 0;
      const entry: CacheEntry<T> = {
        data,
        timestamp: now,
        expiry,
      };

      // Use localStorage for small data (< 5MB limit)
      const serialized = JSON.stringify(entry);
      if (serialized.length < 4 * 1024 * 1024) { // 4MB threshold
        localStorage.setItem(CACHE_PREFIX + key, serialized);
        if (expiry > 0) {
          localStorage.setItem(CACHE_EXPIRY_PREFIX + key, expiry.toString());
        }
      } else {
        // Use IndexedDB for larger data
        if (this.db) {
          const transaction = this.db.transaction([STORE_NAME], 'readwrite');
          const store = transaction.objectStore(STORE_NAME);
          await store.put({ id: key, ...entry });
        }
      }
    } catch (error) {
      console.error('Cache set error:', error);
      // If quota exceeded, clear old entries
      if (error instanceof DOMException && error.name === 'QuotaExceededError') {
        this.clear();
      }
    }
  }

  // Remove from cache
  async remove(key: string): Promise<void> {
    try {
      localStorage.removeItem(CACHE_PREFIX + key);
      localStorage.removeItem(CACHE_EXPIRY_PREFIX + key);
      
      if (this.db) {
        const transaction = this.db.transaction([STORE_NAME], 'readwrite');
        const store = transaction.objectStore(STORE_NAME);
        await store.delete(key);
      }
    } catch (error) {
      console.error('Cache remove error:', error);
    }
  }

  // Clear all cache
  async clear(): Promise<void> {
    try {
      // Clear localStorage
      const keys = Object.keys(localStorage);
      keys.forEach(key => {
        if (key.startsWith(CACHE_PREFIX) || key.startsWith(CACHE_EXPIRY_PREFIX)) {
          localStorage.removeItem(key);
        }
      });

      // Clear IndexedDB
      if (this.db) {
        const transaction = this.db.transaction([STORE_NAME], 'readwrite');
        const store = transaction.objectStore(STORE_NAME);
        await store.clear();
      }
    } catch (error) {
      console.error('Cache clear error:', error);
    }
  }

  // Remove expired entries
  async cleanup(): Promise<void> {
    try {
      const now = Date.now();
      const keys = Object.keys(localStorage);
      
      keys.forEach(key => {
        if (key.startsWith(CACHE_PREFIX)) {
          const cached = localStorage.getItem(key);
          if (cached) {
            try {
              const entry: CacheEntry<any> = JSON.parse(cached);
              if (entry.expiry && now > entry.expiry) {
                const cacheKey = key.replace(CACHE_PREFIX, '');
                this.remove(cacheKey);
              }
            } catch {
              // Invalid entry, remove it
              localStorage.removeItem(key);
            }
          }
        }
      });
    } catch (error) {
      console.error('Cache cleanup error:', error);
    }
  }
}

export const cacheService = new CacheService();

