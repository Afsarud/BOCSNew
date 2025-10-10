// Course Attachments Viewer JavaScript

let currentImageIndex = 0;
let images = [];
let currentZoom = 1;

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', function() {
    initializeImageViewer();
    initializeDocumentViewer();
    initializeImageDownload();
});

// Image Viewer Functions
function initializeImageViewer() {
    // Collect all images
    images = Array.from(document.querySelectorAll('.image-card')).map(card => ({
        src: card.getAttribute('data-image-src'),
        name: card.getAttribute('data-image-name')
    }));

    if (images.length === 0) return;

    // Add click event listeners to image cards
    document.querySelectorAll('.image-card').forEach((card, index) => {
        card.addEventListener('click', () => openImageViewer(index));
    });

    // Add event listeners for navigation
    document.getElementById('prevImageBtn')?.addEventListener('click', showPreviousImage);
    document.getElementById('nextImageBtn')?.addEventListener('click', showNextImage);
    
    // Add event listeners for zoom controls
    document.getElementById('zoomInBtn')?.addEventListener('click', zoomIn);
    document.getElementById('zoomOutBtn')?.addEventListener('click', zoomOut);
    document.getElementById('resetZoomBtn')?.addEventListener('click', resetZoom);

    // Keyboard navigation
    document.addEventListener('keydown', handleKeyboardNavigation);
}

function openImageViewer(index) {
    currentImageIndex = index;
    currentZoom = 1;
    
    const modal = new bootstrap.Modal(document.getElementById('imageViewerModal'));
    updateImageDisplay();
    modal.show();
}

function updateImageDisplay() {
    if (images.length === 0) return;
    
    const currentImage = images[currentImageIndex];
    const modalImage = document.getElementById('modalImage');
    const imageCounter = document.getElementById('imageCounter');
    const imageName = document.getElementById('imageName');
    
    modalImage.src = currentImage.src;
    modalImage.style.transform = `scale(${currentZoom})`;
    imageCounter.textContent = `${currentImageIndex + 1} / ${images.length}`;
    imageName.textContent = currentImage.name;
    
    // Update navigation button states
    const prevBtn = document.getElementById('prevImageBtn');
    const nextBtn = document.getElementById('nextImageBtn');
    
    if (prevBtn) prevBtn.style.display = images.length > 1 ? 'block' : 'none';
    if (nextBtn) nextBtn.style.display = images.length > 1 ? 'block' : 'none';
}

function showPreviousImage() {
    if (images.length === 0) return;
    currentImageIndex = (currentImageIndex - 1 + images.length) % images.length;
    updateImageDisplay();
}

function showNextImage() {
    if (images.length === 0) return;
    currentImageIndex = (currentImageIndex + 1) % images.length;
    updateImageDisplay();
}

function zoomIn() {
    currentZoom = Math.min(currentZoom * 1.2, 5);
    updateImageZoom();
}

function zoomOut() {
    currentZoom = Math.max(currentZoom / 1.2, 0.1);
    updateImageZoom();
}

function resetZoom() {
    currentZoom = 1;
    updateImageZoom();
}

function updateImageZoom() {
    const modalImage = document.getElementById('modalImage');
    if (modalImage) {
        modalImage.style.transform = `scale(${currentZoom})`;
    }
}

function handleKeyboardNavigation(e) {
    const modal = document.getElementById('imageViewerModal');
    if (!modal.classList.contains('show')) return;
    
    switch(e.key) {
        case 'ArrowLeft':
            e.preventDefault();
            showPreviousImage();
            break;
        case 'ArrowRight':
            e.preventDefault();
            showNextImage();
            break;
        case 'Escape':
            e.preventDefault();
            bootstrap.Modal.getInstance(modal)?.hide();
            break;
        case '+':
        case '=':
            e.preventDefault();
            zoomIn();
            break;
        case '-':
            e.preventDefault();
            zoomOut();
            break;
        case '0':
            e.preventDefault();
            resetZoom();
            break;
    }
}

// Document Viewer Functions
function initializeDocumentViewer() {
    // Add click event listeners to document view buttons
    document.querySelectorAll('.view-document-btn').forEach(btn => {
        btn.addEventListener('click', (e) => {
            e.preventDefault();
            const docSrc = btn.getAttribute('data-doc-src');
            openDocumentViewer(docSrc);
        });
    });
}

function openDocumentViewer(docSrc) {
    // Open document in new tab
    window.open(docSrc, '_blank');
}

// Image Download Functions
function initializeImageDownload() {
    // Add click event listeners to download buttons
    document.querySelectorAll('.download-image-btn').forEach(btn => {
        btn.addEventListener('click', (e) => {
            e.preventDefault();
            e.stopPropagation(); // Prevent opening the image viewer
            const imageSrc = btn.getAttribute('data-image-src');
            const imageName = btn.getAttribute('data-image-name');
            downloadImage(imageSrc, imageName);
        });
    });
}

function downloadImage(imageSrc, imageName) {
    // Create a temporary anchor element to trigger download
    const link = document.createElement('a');
    link.href = imageSrc;
    link.download = imageName || 'image';
    
    // Append to body, click, and remove
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
}

// Clean up event listeners when modal is hidden
document.getElementById('imageViewerModal')?.addEventListener('hidden.bs.modal', function() {
    // Reset zoom when modal is closed
    currentZoom = 1;
    const modalImage = document.getElementById('modalImage');
    if (modalImage) {
        modalImage.style.transform = 'scale(1)';
    }
});
