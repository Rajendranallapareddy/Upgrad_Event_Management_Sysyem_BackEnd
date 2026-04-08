// Admin Panel JavaScript Functions

// Global variables
let currentPage = 1;
let pageSize = 10;
let searchTerm = '';

// Document ready function
$(document).ready(function() {
    // Initialize tooltips
    $('[data-toggle="tooltip"]').tooltip();
    
    // Auto-hide alerts after 5 seconds
    setTimeout(function() {
        $('.alert').fadeOut('slow', function() {
            $(this).remove();
        });
    }, 5000);
    
    // Initialize DataTable if exists
    if ($('#dataTable').length) {
        initializeDataTable();
    }
    
    // Search functionality
    $('#searchInput').on('keyup', function() {
        searchTerm = $(this).val();
        filterTable();
    });
    
    // Confirm delete
    $('.confirm-delete').on('click', function(e) {
        e.preventDefault();
        const itemName = $(this).data('item-name');
        const itemId = $(this).data('item-id');
        confirmDelete(itemName, itemId);
    });
});

// Initialize DataTable
function initializeDataTable() {
    $('#dataTable').DataTable({
        responsive: true,
        language: {
            search: "Search:",
            lengthMenu: "Show _MENU_ entries",
            info: "Showing _START_ to _END_ of _TOTAL_ entries",
            paginate: {
                first: "First",
                last: "Last",
                next: "Next",
                previous: "Previous"
            }
        },
        order: [[0, 'desc']]
    });
}

// Confirm delete function
function confirmDelete(itemName, itemId) {
    Swal.fire({
        title: 'Are you sure?',
        text: `You are about to delete "${itemName}". This action cannot be undone!`,
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        cancelButtonColor: '#3085d6',
        confirmButtonText: 'Yes, delete it!',
        cancelButtonText: 'Cancel'
    }).then((result) => {
        if (result.isConfirmed) {
            $(`#deleteForm-${itemId}`).submit();
        }
    });
}

// Show success message
function showSuccessMessage(message) {
    Swal.fire({
        title: 'Success!',
        text: message,
        icon: 'success',
        timer: 3000,
        showConfirmButton: false
    });
}

// Show error message
function showErrorMessage(message) {
    Swal.fire({
        title: 'Error!',
        text: message,
        icon: 'error',
        confirmButtonText: 'OK'
    });
}

// Filter table rows
function filterTable() {
    const filter = searchTerm.toLowerCase();
    const rows = $('#dataTable tbody tr');
    
    rows.each(function() {
        const text = $(this).text().toLowerCase();
        if (text.includes(filter)) {
            $(this).show();
        } else {
            $(this).hide();
        }
    });
}

// Validate event dates
function validateEventDates() {
    const eventDate = $('#EventDate').val();
    if (eventDate && new Date(eventDate) <= new Date()) {
        showErrorMessage('Event date must be in the future');
        return false;
    }
    return true;
}

// Validate session times
function validateSessionTimes() {
    const startTime = $('#SessionStart').val();
    const endTime = $('#SessionEnd').val();
    
    if (startTime && endTime && new Date(endTime) <= new Date(startTime)) {
        showErrorMessage('Session end time must be after start time');
        return false;
    }
    return true;
}

// Preview event image
function previewImage(input) {
    if (input.files && input.files[0]) {
        const reader = new FileReader();
        reader.onload = function(e) {
            $('#imagePreview').attr('src', e.target.result).show();
        };
        reader.readAsDataURL(input.files[0]);
    }
}

// Export table to Excel
function exportToExcel(tableId, filename) {
    const table = document.getElementById(tableId);
    const html = table.outerHTML;
    const url = 'data:application/vnd.ms-excel,' + encodeURIComponent(html);
    const downloadLink = document.createElement('a');
    downloadLink.href = url;
    downloadLink.download = `${filename}.xls`;
    document.body.appendChild(downloadLink);
    downloadLink.click();
    document.body.removeChild(downloadLink);
}

// Print table
function printTable(tableId) {
    const printContents = document.getElementById(tableId).outerHTML;
    const originalContents = document.body.innerHTML;
    document.body.innerHTML = printContents;
    window.print();
    document.body.innerHTML = originalContents;
    location.reload();
}

// Toggle status
function toggleStatus(eventId, currentStatus) {
    const newStatus = currentStatus === 'Active' ? 'In-Active' : 'Active';
    Swal.fire({
        title: 'Confirm Status Change',
        text: `Do you want to change status to ${newStatus}?`,
        icon: 'question',
        showCancelButton: true,
        confirmButtonText: 'Yes, change it!',
        cancelButtonText: 'No, cancel'
    }).then((result) => {
        if (result.isConfirmed) {
            $(`#toggleForm-${eventId}`).submit();
        }
    });
}

// Load content via AJAX
function loadContent(url, targetId) {
    $(`#${targetId}`).html('<div class="text-center"><div class="spinner-border-custom"></div></div>');
    $.ajax({
        url: url,
        type: 'GET',
        success: function(data) {
            $(`#${targetId}`).html(data);
        },
        error: function() {
            $(`#${targetId}`).html('<div class="alert alert-danger">Error loading content</div>');
        }
    });
}

// Submit form via AJAX
function submitFormAjax(formId, successCallback) {
    const formData = new FormData($(`#${formId}`)[0]);
    $.ajax({
        url: $(`#${formId}`).attr('action'),
        type: 'POST',
        data: formData,
        processData: false,
        contentType: false,
        success: function(response) {
            if (response.success) {
                showSuccessMessage(response.message);
                if (successCallback) successCallback();
            } else {
                showErrorMessage(response.message);
            }
        },
        error: function() {
            showErrorMessage('An error occurred. Please try again.');
        }
    });
}

// Dashboard statistics refresh
function refreshDashboardStats() {
    $.ajax({
        url: '/Admin/GetDashboardStats',
        type: 'GET',
        success: function(data) {
            $('#totalEvents').text(data.totalEvents);
            $('#activeEvents').text(data.activeEvents);
            $('#totalSessions').text(data.totalSessions);
            $('#totalSpeakers').text(data.totalSpeakers);
            $('#totalParticipants').text(data.totalParticipants);
        }
    });
}

// Initialize chart if on dashboard
function initializeDashboardCharts() {
    if ($('#eventsChart').length) {
        const ctx = document.getElementById('eventsChart').getContext('2d');
        new Chart(ctx, {
            type: 'bar',
            data: {
                labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun'],
                datasets: [{
                    label: 'Events Created',
                    data: [12, 19, 15, 17, 14, 18],
                    backgroundColor: 'rgba(102, 126, 234, 0.5)',
                    borderColor: 'rgba(102, 126, 234, 1)',
                    borderWidth: 1
                }]
            },
            options: {
                responsive: true,
                scales: {
                    y: {
                        beginAtZero: true
                    }
                }
            }
        });
    }
}