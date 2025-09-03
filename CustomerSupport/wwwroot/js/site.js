// Chat scroll functionality with smooth scrolling
window.scrollToBottom = (element) => {
    if (element) {
        element.scrollTo({
            top: element.scrollHeight,
            behavior: 'smooth'
        });
    }
};

// Force immediate scroll (no animation)
window.scrollToBottomImmediate = (element) => {
    if (element) {
        element.scrollTop = element.scrollHeight;
    }
};

// Auto-scroll with retry mechanism
window.autoScrollToBottom = (element) => {
    if (element) {
        // Try immediate scroll first
        element.scrollTop = element.scrollHeight;
        
        // Then try smooth scroll after a small delay
        setTimeout(() => {
            element.scrollTo({
                top: element.scrollHeight,
                behavior: 'smooth'
            });
        }, 100);
    }
};

// Auto-resize text areas (if needed in future)
window.autoResize = (element) => {
    element.style.height = 'auto';
    element.style.height = element.scrollHeight + 'px';
};

// Show notifications (future feature)
window.showNotification = (message, type = 'info') => {
    // Simple notification system
    const notification = document.createElement('div');
    notification.className = `alert alert-${type} position-fixed`;
    notification.style.cssText = 'top: 20px; right: 20px; z-index: 9999; opacity: 0; transition: opacity 0.3s;';
    notification.innerHTML = `
        <div class="d-flex justify-content-between align-items-center">
            <span>${message}</span>
            <button type="button" class="btn-close" onclick="this.parentElement.parentElement.remove()"></button>
        </div>
    `;
    
    document.body.appendChild(notification);
    
    // Fade in
    setTimeout(() => notification.style.opacity = '1', 100);
    
    // Auto remove after 5 seconds
    setTimeout(() => {
        notification.style.opacity = '0';
        setTimeout(() => notification.remove(), 300);
    }, 5000);
};

// Copy text to clipboard
window.copyToClipboard = async (text) => {
    try {
        await navigator.clipboard.writeText(text);
        return true;
    } catch (err) {
        console.error('Clipboard copy failed:', err);
        return false;
    }
};

// Focus element
window.focusElement = (element) => {
    if (element) {
        element.focus();
    }
};

// Local storage helpers
window.setLocalStorage = (key, value) => {
    localStorage.setItem(key, JSON.stringify(value));
};

window.getLocalStorage = (key) => {
    const item = localStorage.getItem(key);
    return item ? JSON.parse(item) : null;
};

window.removeLocalStorage = (key) => {
    localStorage.removeItem(key);
};

// Manual Enter key handling for Blazor inputs
window.setupEnterKeyHandling = (inputId, submitFunction) => {
    const input = document.getElementById(inputId);
    if (input) {
        // Remove existing listeners to prevent duplicates
        input.removeEventListener('keydown', input._enterHandler);
        
        // Create new handler
        const handler = (e) => {
            if (e.key === 'Enter' && !e.shiftKey && !e.ctrlKey && !e.altKey) {
                e.preventDefault();
                e.stopPropagation();
                
                // Call Blazor function
                if (typeof submitFunction === 'function') {
                    submitFunction();
                }
                
                return false;
            }
        };
        
        // Store handler reference for cleanup
        input._enterHandler = handler;
        
        // Add event listener
        input.addEventListener('keydown', handler, true);
        
        console.log(`Enter key handler setup for: ${inputId}`);
    }
};

// Clean up enter key handling
window.cleanupEnterKeyHandling = (inputId) => {
    const input = document.getElementById(inputId);
    if (input && input._enterHandler) {
        input.removeEventListener('keydown', input._enterHandler, true);
        delete input._enterHandler;
    }
};

// Initialize application when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    console.log('Customer Support Chat System loaded');
    
    // Add any global event listeners here
    
    // Handle page visibility changes
    document.addEventListener('visibilitychange', () => {
        if (document.visibilityState === 'visible') {
            console.log('Page became visible');
        } else {
            console.log('Page became hidden');
        }
    });
});
