// PetFinder.Web/wwwroot/js/scroll/parallax.js

/**
 * Efecto Parallax sutil y de alto rendimiento para elementos decorativos
 * con el atributo [data-parallax] (ej. huellitas flotantes, formas geométricas).
 * Respeta 'prefers-reduced-motion'.
 */
export function initParallax() {
    if (window.matchMedia('(prefers-reduced-motion: reduce)').matches) return;

    const parallaxItems = document.querySelectorAll('[data-parallax]');
    if (parallaxItems.length === 0) return;

    let ticking = false;

    const onScroll = () => {
        const scrollY = window.scrollY;
        const windowHeight = window.innerHeight;

        parallaxItems.forEach(item => {
            const speed = parseFloat(item.getAttribute('data-speed') || '0.15');
            const rect = item.getBoundingClientRect();

            // Solo calcular si está dentro o cerca del viewport
            if (rect.top < windowHeight + 100 && rect.bottom > -100) {
                const offset = (scrollY - (item.offsetTop - windowHeight / 2)) * speed;
                item.style.transform = `translate3d(0, ${offset.toFixed(1)}px, 0)`;
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

    onScroll();
}
