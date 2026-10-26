// PetFinder.Web/wwwroot/js/scroll/navbar.js

/**
 * Control del comportamiento de navegación:
 * - Detección de scroll para sombra y blur (.is-scrolled)
 * - Ocultar al deslizar hacia abajo rápidamente y reaparecer al deslizar hacia arriba (.is-hidden)
 * - Scrollspy para resaltar enlaces activos de la sección actual
 */
export function initNavbar() {
    const navbar = document.querySelector('.site-header') || document.querySelector('.navbar');
    if (!navbar) return;

    let lastScrollY = window.scrollY;
    let ticking = false;
    const scrollDelta = 10;
    const navLinks = document.querySelectorAll('.nav-link[href^="#"]');
    const sections = Array.from(navLinks)
        .map(link => {
            const id = link.getAttribute('href');
            return id && id !== '#' ? document.querySelector(id) : null;
        })
        .filter(sec => sec !== null);

    const onScroll = () => {
        const currentScrollY = window.scrollY;

        // Estado compacto/vidriado con sombra
        if (currentScrollY > 30) {
            navbar.classList.add('is-scrolled');
        } else {
            navbar.classList.remove('is-scrolled');
        }

        // Ocultar/mostrar según dirección de scroll (después de 150px)
        if (currentScrollY > 150) {
            if (currentScrollY - lastScrollY > scrollDelta) {
                // Scroll hacia abajo: ocultar
                navbar.classList.add('is-hidden');
            } else if (lastScrollY - currentScrollY > scrollDelta) {
                // Scroll hacia arriba: mostrar
                navbar.classList.remove('is-hidden');
            }
        } else {
            navbar.classList.remove('is-hidden');
        }

        lastScrollY = currentScrollY;

        // Scrollspy de secciones
        let currentSectionId = '';
        const scrollPosition = currentScrollY + 200;

        sections.forEach(sec => {
            if (sec.offsetTop <= scrollPosition && (sec.offsetTop + sec.offsetHeight) > scrollPosition) {
                currentSectionId = `#${sec.id}`;
            }
        });

        navLinks.forEach(link => {
            if (link.getAttribute('href') === currentSectionId) {
                link.classList.add('active');
            } else {
                link.classList.remove('active');
            }
        });

        ticking = false;
    };

    window.addEventListener('scroll', () => {
        if (!ticking) {
            window.requestAnimationFrame(onScroll);
            ticking = true;
        }
    }, { passive: true });

    // Smooth scroll al hacer click en enlaces internos
    navLinks.forEach(link => {
        link.addEventListener('click', (e) => {
            const href = link.getAttribute('href');
            if (href && href.startsWith('#')) {
                const target = document.querySelector(href);
                if (target) {
                    e.preventDefault();
                    target.scrollIntoView({ behavior: 'smooth' });
                }
            }
        });
    });

    onScroll();
}
