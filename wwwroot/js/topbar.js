// ═══════════════════════════════════════════════════════════
// TOPBAR INTERACTIONS
// ═══════════════════════════════════════════════════════════

document.addEventListener('DOMContentLoaded', function () {
    // Mobile menu toggle
    const menuToggle = document.querySelector('.topbar-toggle');
    const topbarMenu = document.querySelector('.topbar-menu');

    if (menuToggle && topbarMenu) {
        menuToggle.addEventListener('click', function () {
            topbarMenu.classList.toggle('active');
            menuToggle.classList.toggle('active');
        });
    }

    // Smooth scroll to sections
    const navLinks = document.querySelectorAll('.topbar-nav-link');
    navLinks.forEach(link => {
        link.addEventListener('click', function (e) {
            const href = this.getAttribute('href');
            if (href.startsWith('#')) {
                e.preventDefault();
                const target = document.querySelector(href);
                if (target) {
                    target.scrollIntoView({ behavior: 'smooth', block: 'start' });
                    // Close mobile menu only on mobile (1024px and below)
                    if (window.innerWidth <= 1024) {
                        topbarMenu.classList.remove('active');
                        menuToggle.classList.remove('active');
                    }
                }
            }
        });
    });

    // Search functionality (placeholder)
    const searchBtn = document.querySelector('.search-btn');
    const searchInput = document.querySelector('.search-input');

    if (searchBtn && searchInput) {
        searchBtn.addEventListener('click', function () {
            const query = searchInput.value.trim();
            if (query) {
                console.log('Search query:', query);
                // Implement your search logic here
            }
        });

        searchInput.addEventListener('keypress', function (e) {
            if (e.key === 'Enter') {
                searchBtn.click();
            }
        });
    }

    // Scroll animations for hero section
    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('visible');
            }
        });
    }, { threshold: 0.1 });

    document.querySelectorAll('.hero-content, .service-card, .ded-text').forEach(el => {
        observer.observe(el);
    });
});

// ═══════════════════════════════════════════════════════════
// HERO SECTION ANIMATIONS
// ═══════════════════════════════════════════════════════════

window.addEventListener('scroll', function () {
    const hero = document.querySelector('.hero-section');
    if (hero) {
        const scrollPosition = window.scrollY;
        const parallaxEffect = scrollPosition * 0.5;
        hero.style.transform = `translateY(${parallaxEffect}px)`;
    }
});
