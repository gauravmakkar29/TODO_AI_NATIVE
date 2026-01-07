# Performance Testing Documentation

## Overview
This document outlines the performance testing scenarios and requirements for the Todo Application as per AIRE-21 (Performance Optimization and Caching).

## Acceptance Criteria

### 1. Initial Page Load Time < 2 seconds
**Test Scenario:**
- Clear browser cache
- Navigate to the application
- Measure time from navigation start to load event end

**Expected Result:** Page load time should be less than 2000ms

**Measurement:**
```javascript
const perfData = window.performance.timing;
const pageLoadTime = perfData.loadEventEnd - perfData.navigationStart;
```

### 2. API Response Times < 500ms
**Test Scenarios:**

#### 2.1 Get Todos List
- **Endpoint:** `GET /api/todo`
- **Expected:** Response time < 500ms
- **Test with:** 100, 500, 1000 todos

#### 2.2 Get Single Todo
- **Endpoint:** `GET /api/todo/{id}`
- **Expected:** Response time < 500ms

#### 2.3 Create Todo
- **Endpoint:** `POST /api/todo`
- **Expected:** Response time < 500ms

#### 2.4 Update Todo
- **Endpoint:** `PUT /api/todo/{id}`
- **Expected:** Response time < 500ms

#### 2.5 Delete Todo
- **Endpoint:** `DELETE /api/todo/{id}`
- **Expected:** Response time < 500ms

**Measurement:**
- Use browser DevTools Network tab
- Check `X-Response-Time` header
- Monitor backend logs for slow requests (>500ms)

### 3. Smooth Scrolling and Interactions (60fps)
**Test Scenario:**
- Open browser DevTools Performance tab
- Record performance while scrolling through todo list
- Record performance while interacting with UI (clicking, typing)

**Expected Result:** 
- Frame rate should maintain 60fps
- No frame drops below 60fps
- Each frame should render in < 16.67ms

### 4. Efficient Data Loading
**Test Scenarios:**

#### 4.1 Pagination
- **Test:** Load todos with pagination (page size: 50)
- **Expected:** Only requested page is loaded
- **Verify:** Network tab shows only one page of data

#### 4.2 Lazy Loading
- **Test:** Navigate between routes
- **Expected:** Components load on-demand
- **Verify:** Code splitting works, chunks load separately

### 5. Offline Functionality
**Test Scenarios:**

#### 5.1 Service Worker Registration
- **Test:** Check if service worker is registered
- **Expected:** Service worker active in DevTools Application tab

#### 5.2 Offline Cache
- **Test:** 
  1. Load application while online
  2. Go offline (Network tab > Offline)
  3. Navigate and interact with app
- **Expected:** 
  - App works offline
  - Cached data is displayed
  - Offline indicator shown

#### 5.3 Cache Expiration
- **Test:** Wait for cache expiry time
- **Expected:** Cache expires and refreshes on next request

### 6. Optimistic UI Updates
**Test Scenarios:**

#### 6.1 Create Todo
- **Test:** Create a new todo
- **Expected:** Todo appears immediately in UI before API response

#### 6.2 Update Todo
- **Test:** Toggle todo completion
- **Expected:** UI updates immediately, rollback on error

#### 6.3 Delete Todo
- **Test:** Delete a todo
- **Expected:** Todo disappears immediately, rollback on error

### 7. Image and Asset Optimization
**Test Scenarios:**
- Check bundle size in build output
- Verify images are optimized/compressed
- Check CDN usage for static assets (if configured)

**Expected:**
- Bundle size < 1MB (gzipped)
- Images optimized
- Assets cached properly

### 8. Code Splitting and Bundle Optimization
**Test Scenarios:**
- Build the application
- Check bundle analysis
- Verify code splitting works

**Expected:**
- Multiple chunks created
- Vendor code separated
- Lazy loaded routes work

## Load Testing

### Tools
- **Apache JMeter**
- **k6**
- **Artillery**
- **Postman/Newman**

### Test Scenarios

#### 1. Concurrent Users
- **Test:** 100 concurrent users
- **Duration:** 5 minutes
- **Expected:** 
  - Response time < 500ms (p95)
  - Error rate < 1%
  - No memory leaks

#### 2. Stress Testing
- **Test:** Gradually increase load to 500 users
- **Expected:** System handles load gracefully
- **Monitor:** CPU, memory, response times

#### 3. Spike Testing
- **Test:** Sudden spike to 200 users
- **Expected:** System recovers quickly

## Performance Profiling

### Backend Profiling
- Use Application Insights or similar
- Monitor:
  - Database query times
  - Cache hit rates
  - API response times
  - Memory usage
  - CPU usage

### Frontend Profiling
- Use Chrome DevTools Performance tab
- Monitor:
  - JavaScript execution time
  - Rendering time
  - Memory usage
  - Network requests

## Cache Testing

### Redis Cache
- **Test:** Verify cache hits/misses
- **Monitor:** Cache hit rate should be > 70% for read operations

### Browser Cache
- **Test:** Verify localStorage/IndexedDB usage
- **Monitor:** Cache size, expiration

## Performance Metrics Dashboard

### Key Metrics to Track:
1. **Page Load Time** (Target: < 2s)
2. **API Response Time** (Target: < 500ms p95)
3. **Time to First Byte (TTFB)** (Target: < 200ms)
4. **First Contentful Paint (FCP)** (Target: < 1.5s)
5. **Largest Contentful Paint (LCP)** (Target: < 2.5s)
6. **First Input Delay (FID)** (Target: < 100ms)
7. **Cumulative Layout Shift (CLS)** (Target: < 0.1)
8. **Cache Hit Rate** (Target: > 70%)

## Test Execution Checklist

- [ ] Initial page load time < 2 seconds
- [ ] All API endpoints respond < 500ms
- [ ] Smooth scrolling (60fps)
- [ ] Pagination works correctly
- [ ] Lazy loading works
- [ ] Service worker registered
- [ ] Offline functionality works
- [ ] Optimistic UI updates work
- [ ] Bundle size optimized
- [ ] Code splitting works
- [ ] Cache hit rate > 70%
- [ ] Load testing passed (100 concurrent users)
- [ ] No memory leaks
- [ ] Performance monitoring active

## Performance Monitoring Tools

### Backend
- Serilog with performance logging
- PerformanceMonitoringMiddleware
- Redis cache metrics

### Frontend
- Chrome DevTools Performance tab
- Lighthouse
- Web Vitals
- Custom performance monitoring utilities

## Continuous Monitoring

### Production Monitoring
- Set up alerts for:
  - API response time > 500ms
  - Page load time > 2s
  - Cache hit rate < 50%
  - Error rate > 1%

### Regular Testing
- Weekly performance regression tests
- Monthly load testing
- Quarterly performance audit

## Notes

- All performance tests should be run in a production-like environment
- Clear cache between test runs for accurate measurements
- Test with realistic data volumes
- Monitor both client and server-side metrics

