/**
 * BlazorSupercookie JavaScript interop module
 * Handles favicon caching detection and manipulation
 */

let faviconCache = new Map();
let checkPromises = new Map();

/**
 * Checks which favicons are cached by attempting to load them
 * @param {DotNetObjectReference} dotNetRef - Reference to Blazor component
 * @param {string} baseUrl - Base URL for favicon routes
 * @param {string[]} routes - Array of route paths to check
 * @returns {Promise<string[]>} Array of cached route paths
 */
export async function checkFaviconCache(dotNetRef, baseUrl, routes) {
    const cachedRoutes = [];
    const total = routes.length;
    
    for (let i = 0; i < routes.length; i++) {
        const route = routes[i];
        const url = `${baseUrl}f/${route}`;
        
        try {
            const isCached = await checkSingleFavicon(url);
            if (isCached) {
                cachedRoutes.push(route);
            }
            
            if (dotNetRef) {
                dotNetRef.invokeMethodAsync('UpdateProgress', i + 1, total, `Checking ${i + 1}/${total}...`);
            }
        } catch (error) {
            console.warn(`Failed to check favicon for route ${route}:`, error);
        }
    }
    
    return cachedRoutes;
}

/**
 * Checks if a single favicon is cached
 * @param {string} url - URL of the favicon to check
 * @returns {Promise<boolean>} True if cached
 */
async function checkSingleFavicon(url) {
    // Check if we already know the result
    if (faviconCache.has(url)) {
        return faviconCache.get(url);
    }
    
    // Check if there's already a pending check
    if (checkPromises.has(url)) {
        return checkPromises.get(url);
    }
    
    const checkPromise = new Promise((resolve) => {
        const img = new Image();
        const startTime = performance.now();
        
        img.onload = () => {
            const loadTime = performance.now() - startTime;
            // Cached favicons load very quickly (< 10ms typically)
            const isCached = loadTime < 50;
            faviconCache.set(url, isCached);
            checkPromises.delete(url);
            resolve(isCached);
        };
        
        img.onerror = () => {
            faviconCache.set(url, false);
            checkPromises.delete(url);
            resolve(false);
        };
        
        // Set a timeout to avoid hanging
        setTimeout(() => {
            if (checkPromises.has(url)) {
                faviconCache.set(url, false);
                checkPromises.delete(url);
                resolve(false);
            }
        }, 1000);
        
        img.src = url + '?t=' + Date.now();
    });
    
    checkPromises.set(url, checkPromise);
    return checkPromise;
}

/**
 * Caches favicons by loading them sequentially
 * @param {DotNetObjectReference} dotNetRef - Reference to Blazor component
 * @param {string} baseUrl - Base URL for favicon routes
 * @param {string[]} routes - Array of route paths to cache
 * @param {number} total - Total number of routes
 */
export async function cacheFavicons(dotNetRef, baseUrl, routes, total) {
    for (let i = 0; i < routes.length; i++) {
        const route = routes[i];
        const url = `${baseUrl}f/${route}`;
        
        try {
            await loadFavicon(url);
            
            if (dotNetRef) {
                dotNetRef.invokeMethodAsync('UpdateProgress', i + 1, total, `Caching ${i + 1}/${total}...`);
            }
            
            // Small delay to ensure cache is written
            await new Promise(resolve => setTimeout(resolve, 100));
        } catch (error) {
            console.warn(`Failed to cache favicon for route ${route}:`, error);
        }
    }
}

/**
 * Loads a favicon to cache it
 * @param {string} url - URL of the favicon to load
 * @returns {Promise<void>}
 */
function loadFavicon(url) {
    return new Promise((resolve, reject) => {
        const link = document.createElement('link');
        link.rel = 'icon';
        link.type = 'image/png';
        link.href = url;
        
        link.onload = () => {
            document.head.removeChild(link);
            resolve();
        };
        
        link.onerror = () => {
            document.head.removeChild(link);
            reject(new Error(`Failed to load favicon: ${url}`));
        };
        
        document.head.appendChild(link);
        
        // Fallback timeout
        setTimeout(() => {
            if (document.head.contains(link)) {
                document.head.removeChild(link);
                resolve(); // Resolve anyway, might be cached
            }
        }, 2000);
    });
}

/**
 * Clears the favicon cache map (for testing/debugging)
 */
export function clearCache() {
    faviconCache.clear();
    checkPromises.clear();
}
