// Lesson Management JavaScript Functions

// Delete attachment functionality
function deleteAttachment(attachmentId, type) {
    if (confirm('Are you sure you want to delete this attachment?')) {
        // Create a form to submit the delete request
        const form = document.createElement('form');
        form.method = 'POST';
        form.action = `/admin/course-lessons/delete-attachment/${attachmentId}`;
        
        // Add anti-forgery token
        const tokenInput = document.createElement('input');
        tokenInput.type = 'hidden';
        tokenInput.name = '__RequestVerificationToken';
        tokenInput.value = document.querySelector('input[name="__RequestVerificationToken"]').value;
        form.appendChild(tokenInput);
        
        // Submit the form
        document.body.appendChild(form);
        form.submit();
    }
}

function showAlert(message, type) {
    const alertDiv = document.createElement('div');
    alertDiv.className = `alert alert-${type} alert-dismissible fade show`;
    alertDiv.innerHTML = `
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `;
    
    const container = document.querySelector('.container');
    if (container) {
        container.insertBefore(alertDiv, container.firstChild);
        
        // Auto-dismiss after 3 seconds
        setTimeout(() => {
            alertDiv.remove();
        }, 3000);
    }
}

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', function() {
    initializeDeleteButtons();
});

// Initialize delete button event listeners
function initializeDeleteButtons() {
    // Use event delegation to handle delete button clicks
    document.addEventListener('click', function(e) {
        if (e.target.closest('.delete-attachment-btn')) {
            e.preventDefault();
            const button = e.target.closest('.delete-attachment-btn');
            const attachmentId = button.getAttribute('data-attachment-id');
            const attachmentType = button.getAttribute('data-attachment-type');
            deleteAttachment(attachmentId, attachmentType);
        }
    });
}
