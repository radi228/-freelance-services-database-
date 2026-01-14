// Dashboard Management

const OFFERS_API = '/api/offers';

// Show the appropriate dashboard based on account type
function showDashboard(accountType) {
    const offeringDashboard = document.getElementById('offeringDashboard');
    const receivingDashboard = document.getElementById('receivingDashboard');
    const heroSection = document.querySelector('.hero');
    const categoriesSection = document.querySelector('.categories');
    const featuredSection = document.querySelector('.featured');
    const footer = document.querySelector('footer');

    if (accountType === 'offering') {
        if (offeringDashboard) offeringDashboard.style.display = 'block';
        if (receivingDashboard) receivingDashboard.style.display = 'none';
    } else if (accountType === 'receiving') {
        if (offeringDashboard) offeringDashboard.style.display = 'none';
        if (receivingDashboard) receivingDashboard.style.display = 'block';
    }

    // Hide home page sections when logged in
    if (heroSection) heroSection.style.display = 'none';
    if (categoriesSection) categoriesSection.style.display = 'none';
    if (featuredSection) featuredSection.style.display = 'none';
    if (footer) footer.style.display = 'none';

    // Load appropriate content
    if (accountType === 'offering') {
        loadMyOffers();
    } else if (accountType === 'receiving') {
        loadAllOffers();
    }
}

// Hide dashboards and show home page
function hideDashboard() {
    const offeringDashboard = document.getElementById('offeringDashboard');
    const receivingDashboard = document.getElementById('receivingDashboard');
    const heroSection = document.querySelector('.hero');
    const categoriesSection = document.querySelector('.categories');
    const featuredSection = document.querySelector('.featured');
    const footer = document.querySelector('footer');

    if (offeringDashboard) offeringDashboard.style.display = 'none';
    if (receivingDashboard) receivingDashboard.style.display = 'none';
    if (heroSection) heroSection.style.display = 'block';
    if (categoriesSection) categoriesSection.style.display = 'block';
    if (featuredSection) featuredSection.style.display = 'block';
    if (footer) footer.style.display = 'block';

    window.scrollTo(0, 0);
}

// Create a new offer
async function handleCreateOffer(event) {
    event.preventDefault();

    const user = JSON.parse(localStorage.getItem(USER_KEY));
    if (!user) {
        showNotification('Please log in first', 'error');
        return;
    }

    const title = document.getElementById('offerTitle').value;
    const description = document.getElementById('offerDescription').value;
    const category = document.getElementById('offerCategory').value;
    const price = parseFloat(document.getElementById('offerPrice').value);
    const location = document.getElementById('offerLocation').value;

    const createBtn = event.target.querySelector('button[type="submit"]');
    createBtn.disabled = true;
    createBtn.textContent = 'Posting...';

    try {
        const response = await fetch(OFFERS_API, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                userId: user.Id,
                title,
                description,
                category,
                price,
                location: location || null
            })
        });

        const data = await response.json();

        if (!response.ok) {
            document.getElementById('createOfferError').textContent = data.message || 'Failed to create offer';
            return;
        }

        showNotification('Offer posted successfully!', 'success');
        document.getElementById('createOfferForm').reset();
        loadMyOffers();
    } catch (error) {
        console.error('Error creating offer:', error);
        document.getElementById('createOfferError').textContent = 'An error occurred. Please try again.';
    } finally {
        createBtn.disabled = false;
        createBtn.textContent = 'Post Your Offer';
    }
}

// Load all offers for receiving users
async function loadAllOffers() {
    try {
        const response = await fetch(OFFERS_API);
        const offers = await response.json();

        const offersGrid = document.getElementById('offersGrid');
        if (!offersGrid) return;

        if (!Array.isArray(offers) || offers.length === 0) {
            offersGrid.innerHTML = '<div class="empty-state" style="grid-column: 1/-1;"><div class="empty-state-icon">📭</div><p>No offers available yet</p></div>';
            return;
        }

        offersGrid.innerHTML = offers.map(offer => createOfferCard(offer, 'view')).join('');

        // Add event listeners for search and filter
        const searchInput = document.getElementById('searchOffers');
        const filterSelect = document.getElementById('filterCategory');

        if (searchInput) searchInput.addEventListener('input', () => filterOffers());
        if (filterSelect) filterSelect.addEventListener('change', () => filterOffers());
    } catch (error) {
        console.error('Error loading offers:', error);
    }
}

// Load user's own offers
async function loadMyOffers() {
    const user = JSON.parse(localStorage.getItem(USER_KEY));
    if (!user) return;

    try {
        const response = await fetch(`${OFFERS_API}/user/${user.Id}`);
        const offers = await response.json();

        const myOffersList = document.getElementById('myOffersList');
        if (!myOffersList) return;

        if (!Array.isArray(offers) || offers.length === 0) {
            myOffersList.innerHTML = '<div class="empty-state" style="grid-column: 1/-1;"><div class="empty-state-icon">📝</div><p>You haven\'t posted any offers yet</p></div>';
            return;
        }

        myOffersList.innerHTML = offers.map(offer => createOfferCard(offer, 'edit')).join('');
    } catch (error) {
        console.error('Error loading my offers:', error);
    }
}

// Create HTML for an offer card
function createOfferCard(offer, mode) {
    const createdDate = new Date(offer.createdAt).toLocaleDateString();
    let actionsHTML = '';

    if (mode === 'view') {
        actionsHTML = `
            <button class="offer-actions-btn btn-contact" onclick="contactUser(${offer.userId})">
                Contact Seller
            </button>
        `;
    } else if (mode === 'edit') {
        actionsHTML = `
            <button class="offer-actions-btn btn-edit" onclick="editOffer(${offer.id})">
                Edit
            </button>
            <button class="offer-actions-btn btn-delete" onclick="deleteOffer(${offer.id})">
                Delete
            </button>
        `;
    }

    return `
        <div class="offer-card">
            <div class="offer-header">
                <div class="offer-title">${escapeHtml(offer.title)}</div>
                <span class="offer-category">${escapeHtml(offer.category)}</span>
            </div>
            <div class="offer-body">
                <p class="offer-description">${escapeHtml(offer.description)}</p>
                <div class="offer-details">
                    <div class="offer-price">$${offer.price.toFixed(2)}</div>
                    ${offer.location ? `<div class="offer-location">📍 ${escapeHtml(offer.location)}</div>` : ''}
                </div>
                <div class="offer-details">
                    <small style="color: #999;">Posted: ${createdDate}</small>
                </div>
                <div class="offer-actions">
                    ${actionsHTML}
                </div>
            </div>
        </div>
    `;
}

// Filter offers based on search and category
function filterOffers() {
    const searchTerm = document.getElementById('searchOffers')?.value.toLowerCase() || '';
    const selectedCategory = document.getElementById('filterCategory')?.value || '';
    const cards = document.querySelectorAll('#offersGrid .offer-card');

    cards.forEach(card => {
        const title = card.querySelector('.offer-title').textContent.toLowerCase();
        const description = card.querySelector('.offer-description').textContent.toLowerCase();
        const category = card.querySelector('.offer-category').textContent;

        const matchesSearch = title.includes(searchTerm) || description.includes(searchTerm);
        const matchesCategory = !selectedCategory || category === selectedCategory;

        card.style.display = matchesSearch && matchesCategory ? 'block' : 'none';
    });
}

// Delete an offer
async function deleteOffer(offerId) {
    if (!confirm('Are you sure you want to delete this offer?')) return;

    const user = JSON.parse(localStorage.getItem(USER_KEY));
    if (!user) return;

    try {
        const response = await fetch(`${OFFERS_API}/${offerId}`, {
            method: 'DELETE',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ userId: user.Id })
        });

        if (!response.ok) {
            showNotification('Failed to delete offer', 'error');
            return;
        }

        showNotification('Offer deleted successfully', 'success');
        loadMyOffers();
    } catch (error) {
        console.error('Error deleting offer:', error);
        showNotification('An error occurred', 'error');
    }
}

// Edit offer (placeholder - can be expanded)
function editOffer(offerId) {
    showNotification('Edit feature coming soon!', 'info');
}

// Contact user (placeholder - can be expanded)
function contactUser(userId) {
    showNotification('Contact feature coming soon! You can message this seller.', 'info');
}

// Escape HTML to prevent XSS
function escapeHtml(text) {
    const map = {
        '&': '&amp;',
        '<': '&lt;',
        '>': '&gt;',
        '"': '&quot;',
        "'": '&#039;'
    };
    return text.replace(/[&<>"']/g, m => map[m]);
}
