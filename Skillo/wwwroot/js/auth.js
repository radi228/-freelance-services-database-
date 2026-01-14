// API Configuration
const API_BASE_URL = '/api';
const AUTH_API = `${API_BASE_URL}/auth`;

// Local Storage Keys
const USER_KEY = 'currentUser';
const TOKEN_KEY = 'authToken';
const ACCOUNT_TYPE_KEY = 'selectedAccountType';

// Navigate to home page
function goHome() {
    closeAllModals();
    const user = JSON.parse(localStorage.getItem(USER_KEY));
    if (user) {
        // If logged in, show the appropriate dashboard
        showDashboard(user.AccountType);
    } else {
        // If not logged in, hide dashboards and show home page
        hideDashboard();
    }
    window.scrollTo(0, 0);
}

// Initialize on page load
document.addEventListener('DOMContentLoaded', () => {
    checkAuthStatus();
    setupEventListeners();
});

// Setup event listeners
function setupEventListeners() {
    // Close modals on overlay click
    const overlay = document.getElementById('modalOverlay');
    overlay.addEventListener('click', () => {
        closeAllModals();
    });

    // Close modals with Escape key
    document.addEventListener('keydown', (e) => {
        if (e.key === 'Escape') {
            closeAllModals();
        }
    });

    // Add click handlers for account type buttons
    const accountTypeButtons = document.querySelectorAll('.account-type-btn');
    accountTypeButtons.forEach(btn => {
        btn.addEventListener('click', function(e) {
            e.preventDefault();
            const buttonText = this.textContent.toLowerCase();
            let accountType = 'offering';
            if (buttonText.includes('both')) {
                accountType = 'both';
            } else if (buttonText.includes('receive')) {
                accountType = 'receiving';
            } else if (buttonText.includes('offer')) {
                accountType = 'offering';
            }
            selectAccountType(accountType);
        });
    });
}

// Check authentication status on page load
function checkAuthStatus() {
    const user = localStorage.getItem(USER_KEY);
    if (user) {
        try {
            const userData = JSON.parse(user);
            updateUIForLoggedIn(userData);
            // Show dashboard if user is logged in
            if (userData.AccountType) {
                showDashboard(userData.AccountType);
            }
        } catch (e) {
            console.error('Error parsing user data:', e);
            logout();
        }
    }
}

// Modal Management
function showLoginModal() {
    const modal = document.getElementById('loginModal');
    const overlay = document.getElementById('modalOverlay');
    modal.classList.add('show');
    overlay.classList.add('show');
}

function closeLoginModal() {
    const modal = document.getElementById('loginModal');
    const overlay = document.getElementById('modalOverlay');
    modal.classList.remove('show');
    overlay.classList.remove('show');
    clearErrors('login');
    document.getElementById('loginForm').reset();
}

function showRegisterModal() {
    const modal = document.getElementById('registerModal');
    const overlay = document.getElementById('modalOverlay');
    modal.classList.add('show');
    overlay.classList.add('show');
}

function closeRegisterModal() {
    const modal = document.getElementById('registerModal');
    const overlay = document.getElementById('modalOverlay');
    modal.classList.remove('show');
    overlay.classList.remove('show');
    clearErrors('register');
    document.getElementById('registerForm').reset();
    // Reset to step 1
    resetRegistrationSteps();
}

function closeAllModals() {
    closeLoginModal();
    closeRegisterModal();
}

function switchToRegister() {
    closeLoginModal();
    showRegisterModal();
}

function switchToLogin() {
    closeRegisterModal();
    showLoginModal();
}

// Registration Step Management
function selectAccountType(type) {
    localStorage.setItem(ACCOUNT_TYPE_KEY, type);
    updateAccountTypeUI(type);
    showRegistrationStep2(type);
}

function updateAccountTypeUI(type) {
    const buttons = document.querySelectorAll('.account-type-btn');
    buttons.forEach(btn => btn.classList.remove('selected'));
    
    buttons.forEach(btn => {
        if (btn.textContent.toLowerCase().includes(type) || 
            (type === 'both' && btn.textContent.includes('Both'))) {
            btn.classList.add('selected');
        }
    });
}

function showRegistrationStep2(accountType) {
    const step1 = document.getElementById('registerStep1');
    const step2 = document.getElementById('registerStep2');
    const offeringSection = document.getElementById('offeringSection');
    const receivingSection = document.getElementById('receivingSection');
    
    // Show/hide sections based on account type
    if (accountType === 'offering') {
        offeringSection.style.display = 'block';
        receivingSection.style.display = 'none';
        document.getElementById('registerUsernameOffering').setAttribute('required', 'required');
        document.getElementById('registerUsernameReceiving').removeAttribute('required');
    } else if (accountType === 'receiving') {
        offeringSection.style.display = 'none';
        receivingSection.style.display = 'block';
        document.getElementById('registerUsernameReceiving').setAttribute('required', 'required');
        document.getElementById('registerUsernameOffering').removeAttribute('required');
    } else if (accountType === 'both') {
        offeringSection.style.display = 'block';
        receivingSection.style.display = 'block';
        document.getElementById('registerUsernameOffering').setAttribute('required', 'required');
        document.getElementById('registerUsernameReceiving').setAttribute('required', 'required');
    }
    
    step1.style.display = 'none';
    step2.style.display = 'block';
}

function backToStep1() {
    const step1 = document.getElementById('registerStep1');
    const step2 = document.getElementById('registerStep2');
    
    step1.style.display = 'block';
    step2.style.display = 'none';
    
    // Reset form but keep selected type highlighted
    clearErrors('register');
    document.getElementById('registerForm').reset();
    
    const selectedType = localStorage.getItem(ACCOUNT_TYPE_KEY);
    if (selectedType) {
        updateAccountTypeUI(selectedType);
    }
}

function resetRegistrationSteps() {
    const step1 = document.getElementById('registerStep1');
    const step2 = document.getElementById('registerStep2');
    const buttons = document.querySelectorAll('.account-type-btn');
    
    step1.style.display = 'block';
    step2.style.display = 'none';
    buttons.forEach(btn => btn.classList.remove('selected'));
    localStorage.removeItem(ACCOUNT_TYPE_KEY);
}

// Form Validation
function validateLoginForm(username, password) {
    const errors = {};

    if (!username || !username.trim()) {
        errors.username = 'Username is required';
    } else if (username.length < 3) {
        errors.username = 'Username must be at least 3 characters';
    }

    if (!password) {
        errors.password = 'Password is required';
    } else if (password.length < 8) {
        errors.password = 'Password must be at least 8 characters';
    }

    return errors;
}

function validateRegisterForm(formData) {
    const errors = {};

    if (!formData.username || !formData.username.trim()) {
        errors.username = 'Username is required';
    } else if (formData.username.length < 3) {
        errors.username = 'Username must be at least 3 characters';
    }

    if (!formData.email || !formData.email.trim()) {
        errors.email = 'Email is required';
    } else if (!isValidEmail(formData.email)) {
        errors.email = 'Please enter a valid email';
    }

    if (!formData.password) {
        errors.password = 'Password is required';
    } else if (formData.password.length < 8) {
        errors.password = 'Password must be at least 8 characters, with 1 capital letter and 1 number';
    } else {
        const hasUppercase = /[A-Z]/.test(formData.password);
        const hasNumber = /\d/.test(formData.password);
        if (!hasUppercase) {
            errors.password = 'Password must contain at least 1 capital letter';
        } else if (!hasNumber) {
            errors.password = 'Password must contain at least 1 number';
        }
    }

    if (formData.password !== formData.confirmPassword) {
        errors.confirmPassword = 'Passwords do not match';
    }

    return errors;
}

function isValidEmail(email) {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
}

// Check password strength in real-time
function checkPasswordStrength() {
    const password = document.getElementById('registerPassword').value;
    const reqLength = document.getElementById('req-length');
    const reqUppercase = document.getElementById('req-uppercase');
    const reqNumber = document.getElementById('req-number');
    
    // Check length requirement
    if (password.length >= 8) {
        reqLength.classList.add('met');
    } else {
        reqLength.classList.remove('met');
    }
    
    // Check uppercase requirement
    if (/[A-Z]/.test(password)) {
        reqUppercase.classList.add('met');
    } else {
        reqUppercase.classList.remove('met');
    }
    
    // Check number requirement
    if (/\d/.test(password)) {
        reqNumber.classList.add('met');
    } else {
        reqNumber.classList.remove('met');
    }
}

// Handle Login
async function handleLogin(event) {
    event.preventDefault();
    clearErrors('login');

    const username = document.getElementById('loginUsername').value;
    const password = document.getElementById('loginPassword').value;
    const accountType = document.querySelector('input[name="accountType"]:checked').value;

    // Validate form
    const errors = validateLoginForm(username, password);
    if (Object.keys(errors).length > 0) {
        displayErrors(errors, 'login');
        return;
    }

    // Disable submit button
    const loginBtn = document.getElementById('loginBtn');
    loginBtn.disabled = true;
    loginBtn.textContent = 'Signing in...';

    try {
        const response = await fetch(`${AUTH_API}/login`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                username: username.trim(),
                password: password,
                accountType: accountType
            })
        });

        const data = await response.json();

        if (!response.ok) {
            if (data.message) {
                document.getElementById('loginGeneralError').textContent = data.message;
            } else {
                document.getElementById('loginGeneralError').textContent = 'Login failed. Please try again.';
            }
            return;
        }

        if (data.success && data.user) {
            // Store user in local storage
            localStorage.setItem(USER_KEY, JSON.stringify(data.user));
            if (data.token) {
                localStorage.setItem(TOKEN_KEY, data.token);
            }

            // Update UI
            updateUIForLoggedIn(data.user);

            // Close modal
            closeLoginModal();

            // Show dashboard based on account type
            if (data.user.AccountType) {
                showDashboard(data.user.AccountType);
            }

            // Show success message
            showNotification('Successfully logged in!', 'success');
        } else {
            document.getElementById('loginGeneralError').textContent = data.message || 'Login failed';
        }
    } catch (error) {
        console.error('Login error:', error);
        document.getElementById('loginGeneralError').textContent = 'An error occurred. Please try again.';
    } finally {
        loginBtn.disabled = false;
        loginBtn.textContent = 'Sign In';
    }
}

// Handle Register
async function handleRegister(event) {
    event.preventDefault();
    clearErrors('register');

    const formData = {
        username: document.getElementById('registerUsername').value,
        email: document.getElementById('registerEmail').value,
        firstName: document.getElementById('registerFirstName').value,
        lastName: document.getElementById('registerLastName').value,
        password: document.getElementById('registerPassword').value,
        confirmPassword: document.getElementById('registerConfirmPassword').value
    };

    // Validate form
    const errors = validateRegisterForm(formData);
    if (Object.keys(errors).length > 0) {
        displayErrors(errors, 'register');
        return;
    }

    // Disable submit button
    const registerBtn = document.getElementById('registerBtn');
    registerBtn.disabled = true;
    registerBtn.textContent = 'Creating account...';

    try {
        // Prepare request body
        let requestBody = {
            username: formData.username.trim(),
            email: formData.email.trim(),
            firstName: formData.firstName.trim() || null,
            lastName: formData.lastName.trim() || null,
            password: formData.password,
            confirmPassword: formData.confirmPassword
        };

        const response = await fetch(`${AUTH_API}/register`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(requestBody)
        });

        const data = await response.json();

        if (!response.ok) {
            if (data.message) {
                document.getElementById('registerGeneralError').textContent = data.message;
            } else {
                document.getElementById('registerGeneralError').textContent = 'Registration failed. Please try again.';
            }
            return;
        }

        if (data.success && data.user) {
            // Just show success message, don't log them in
            closeRegisterModal();
            document.getElementById('registerForm').reset();
            resetRegistrationSteps();
            
            showNotification('Account created successfully! Please log in.', 'success');
            
            // Show login modal after a short delay
            setTimeout(() => {
                showLoginModal();
            }, 500);
        } else {
            document.getElementById('registerGeneralError').textContent = data.message || 'Registration failed';
        }
    } catch (error) {
        console.error('Register error:', error);
        document.getElementById('registerGeneralError').textContent = 'An error occurred. Please try again.';
    } finally {
        registerBtn.disabled = false;
        registerBtn.textContent = 'Register';
    }
}

// Update UI for logged-in users
function updateUIForLoggedIn(user) {
    const authMenu = document.getElementById('authMenu');
    const userMenu = document.getElementById('userMenu');
    const welcomeUser = document.getElementById('welcomeUser');

    authMenu.style.display = 'none';
    userMenu.style.display = 'flex';
    
    const displayName = user.firstName || user.currentUsername || user.email;
    welcomeUser.textContent = `Welcome, ${displayName}`;
}

// Update UI for logged-out users
function updateUIForLoggedOut() {
    const authMenu = document.getElementById('authMenu');
    const userMenu = document.getElementById('userMenu');

    authMenu.style.display = 'flex';
    userMenu.style.display = 'none';
}

// Logout
function logout() {
    localStorage.removeItem(USER_KEY);
    localStorage.removeItem(TOKEN_KEY);
    updateUIForLoggedOut();
    hideDashboard();
    showNotification('You have been logged out', 'info');
}

// Clear form errors
function clearErrors(formType) {
    const errorElements = document.querySelectorAll(`#${formType}Modal .error-message`);
    errorElements.forEach(el => {
        el.textContent = '';
    });

    const inputs = document.querySelectorAll(`#${formType}Modal input`);
    inputs.forEach(input => {
        input.classList.remove('error');
    });
}

// Display validation errors
function displayErrors(errors, formType) {
    Object.keys(errors).forEach(field => {
        const errorElement = document.getElementById(`${formType}${field.charAt(0).toUpperCase() + field.slice(1)}Error`);
        if (errorElement) {
            errorElement.textContent = errors[field];
            const inputElement = document.getElementById(`${formType}${field.charAt(0).toUpperCase() + field.slice(1)}`);
            if (inputElement) {
                inputElement.classList.add('error');
            }
        }
    });
}

// Show notification
function showNotification(message, type = 'info') {
    // Create notification element
    const notification = document.createElement('div');
    notification.className = `notification notification-${type}`;
    notification.textContent = message;
    notification.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        padding: 16px 24px;
        background: ${type === 'success' ? '#31a24c' : type === 'error' ? '#d92e2e' : '#1dbf73'};
        color: white;
        border-radius: 4px;
        z-index: 10000;
        box-shadow: 0 4px 6px rgba(0,0,0,0.1);
        animation: slideInRight 0.3s ease-out;
    `;

    document.body.appendChild(notification);

    // Remove after 3 seconds
    setTimeout(() => {
        notification.style.animation = 'slideOutRight 0.3s ease-in';
        setTimeout(() => {
            notification.remove();
        }, 300);
    }, 3000);
}

// Add animation styles
if (!document.getElementById('notificationStyles')) {
    const style = document.createElement('style');
    style.id = 'notificationStyles';
    style.textContent = `
        @keyframes slideInRight {
            from {
                transform: translateX(400px);
                opacity: 0;
            }
            to {
                transform: translateX(0);
                opacity: 1;
            }
        }
        
        @keyframes slideOutRight {
            from {
                transform: translateX(0);
                opacity: 1;
            }
            to {
                transform: translateX(400px);
                opacity: 0;
            }
        }
    `;
    document.head.appendChild(style);
}
