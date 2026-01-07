// Performance monitoring utilities

export const performanceMonitor = {
  // Measure page load time
  measurePageLoad() {
    if (typeof window !== 'undefined' && 'performance' in window) {
      window.addEventListener('load', () => {
        const perfData = window.performance.timing;
        const pageLoadTime = perfData.loadEventEnd - perfData.navigationStart;
        
        console.log(`Page Load Time: ${pageLoadTime}ms`);
        
        // Log to analytics if needed
        if (pageLoadTime > 2000) {
          console.warn(`Slow page load detected: ${pageLoadTime}ms`);
        }
      });
    }
  },

  // Measure API call performance
  async measureApiCall<T>(
    apiCall: () => Promise<T>,
    endpoint: string
  ): Promise<T> {
    const startTime = performance.now();
    
    try {
      const result = await apiCall();
      const duration = performance.now() - startTime;
      
      console.log(`API Call: ${endpoint} took ${duration.toFixed(2)}ms`);
      
      if (duration > 500) {
        console.warn(`Slow API call detected: ${endpoint} took ${duration.toFixed(2)}ms`);
      }
      
      return result;
    } catch (error) {
      const duration = performance.now() - startTime;
      console.error(`API Call failed: ${endpoint} after ${duration.toFixed(2)}ms`, error);
      throw error;
    }
  },

  // Measure render performance
  measureRender(componentName: string, renderFn: () => void) {
    const startTime = performance.now();
    renderFn();
    const duration = performance.now() - startTime;
    
    if (duration > 16) { // 60fps = 16.67ms per frame
      console.warn(`Slow render detected: ${componentName} took ${duration.toFixed(2)}ms`);
    }
  },

  // Get performance metrics
  getMetrics() {
    if (typeof window !== 'undefined' && 'performance' in window) {
      const perfData = window.performance.timing;
      return {
        pageLoadTime: perfData.loadEventEnd - perfData.navigationStart,
        domContentLoaded: perfData.domContentLoadedEventEnd - perfData.navigationStart,
        firstPaint: 0, // Would need PerformanceObserver for this
        firstContentfulPaint: 0, // Would need PerformanceObserver for this
      };
    }
    return null;
  },
};

// Initialize performance monitoring
if (typeof window !== 'undefined') {
  performanceMonitor.measurePageLoad();
}

