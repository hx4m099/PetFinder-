// PetFinder.Web/wwwroot/js/scroll/petho-scroll.js

/**
 * Efecto de scroll reactivo para el lienzo 3D del héroe (#hero-canvas).
 * Realiza un suave desvanecimiento y traslación cuando el usuario hace scroll,
 * asegurando una transición elegante hacia las secciones inferiores.
 */
export function initPethoScroll() {
    const heroSection = document.querySelector('.hero-section') || document.querySelector('.hero');
    const heroCanvas = document.getElementById('hero-canvas');
    if (!heroSection || !heroCanvas) return;

    if (window.matchMedia('(prefers-reduced-motion: reduce)').matches) return;

    let ticking = false;

    const onScroll = () => {
        const scrollY = window.scrollY;
        const heroHeight = heroSection.offsetHeight;

        if (scrollY <= heroHeight) {
            // Factor de 0 (al inicio) a 1 (al terminar hero)
            const factor = Math.min(1, Math.max(0, scrollY / (heroHeight * 0.9)));
            const translateY = scrollY * 0.2; // Sutil efecto parallax
            const opacity = 1 - factor * 0.35; // Desvanecimiento suave sin desaparecer bruscamente
            const scale = 1 - factor * 0.05;

            heroCanvas.style.transform = `translate3d(0, ${translateY.toFixed(1)}px, 0) scale(${scale.toFixed(3)})`;
            heroCanvas.style.opacity = `${opacity.toFixed(2)}`;
        }

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
