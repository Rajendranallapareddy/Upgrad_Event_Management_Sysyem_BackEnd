// Site-wide JavaScript functions

// Show confirmation dialog for delete actions
function confirmDelete(itemName, itemId) {
    if (confirm(`Are you sure you want to delete ${itemName}? This action cannot be undone.`)) {
        document.getElementById(`deleteForm-${itemId}`).submit();
    }
}

// Auto-hide alerts after 5 seconds
document.addEventListener('DOMContentLoaded', function() {
    setTimeout(function() {
        const alerts = document.querySelectorAll('.alert');
        alerts.forEach(function(alert) {
            const bsAlert = new bootstrap.Alert(alert);
            bsAlert.close();
        });
    }, 5000);
});

// Format date for display
function formatDate(dateString) {
    const options = { year: 'numeric', month: 'long', day: 'numeric' };
    return new Date(dateString).toLocaleDateString(undefined, options);
}

// Validate session times
function validateSessionTimes() {
    const startTime = document.getElementById('SessionStart');
    const endTime = document.getElementById('SessionEnd');
    
    if (startTime && endTime) {
        if (new Date(endTime.value) <= new Date(startTime.value)) {
            alert('Session end time must be after start time');
            return false;
        }
    }
    return true;
}

// Search/filter functionality
function filterEvents() {
    const searchTerm = document.getElementById('searchInput')?.value.toLowerCase();
    const eventCards = document.querySelectorAll('.event-card');
    
    eventCards.forEach(card => {
        const title = card.querySelector('.card-title')?.textContent.toLowerCase();
        const category = card.querySelector('.badge')?.textContent.toLowerCase();
        
        if (title?.includes(searchTerm) || category?.includes(searchTerm) || !searchTerm) {
            card.style.display = 'block';
        } else {
            card.style.display = 'none';
        }
    });
}