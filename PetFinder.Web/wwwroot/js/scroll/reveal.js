// PetFinder.Web/wwwroot/js/scroll/reveal.js

/**
 * Módulo de animación de aparición por scroll (Scroll Reveal).
 * Utiliza IntersectionObserver para añadir la clase 'is-revealed' a los elementos
 * con atributo [data-reveal] o grupos con [data-reveal-group].
 * Respeta la preferencia 'prefers-reduced-motion'.
 */
export function initReveal() {
    const prefersReducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;

    // Si el usuario prefiere movimiento reducido, mostrar todo de inmediato
    if (prefersReducedMotion) {
        document.querySelectorAll('[data-reveal], [data-reveal-group] > *').forEach(el => {
            el.classList.add('is-revealed');
        });
        return;
    }

    const observerOptions = {
        root: null,
        rootMargin: '0px 0px -60px 0px',
        threshold: 0.15
    };

    const revealObserver = new IntersectionObserver((entries, observer) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                const target = entry.target;

                if (target.hasAttribute('data-reveal-group')) {
                    const children = target.querySelectorAll(':scope > *');
                    children.forEach((child, index) => {
                        const delay = (index * 120);
                        setTimeout(() => {
                            child.classList.add('is-revealed');
                        }, delay);
                    });
                } else {
                    target.classList.add('is-revealed');
                }

                observer.unobserve(target);
            }
        });
    }, observerOptions);

    // Observar elementos individuales
    document.querySelectorAll('[data-reveal]').forEach(el => {
        revealObserver.observe(el);
    });

    // Observar grupos escalonados
    document.querySelectorAll('[data-reveal-group]').forEach(group => {
        revealObserver.observe(group);
    });
}
