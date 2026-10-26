// PetFinder.Web/wwwroot/js/scroll/back-to-top.js

/**
 * Botón flotante para regresar suavemente al inicio (#back-to-top).
 * Aparece cuando el usuario ha descendido más de 400px en la página.
 */
export function initBackToTop() {
    const btn = document.getElementById('back-to-top');
    if (!btn) return;

    let ticking = false;

    const checkVisibility = () => {
        if (window.scrollY > 400) {
            btn.classList.add('is-visible');
        } else {
            btn.classList.remove('is-visible');
        }
        ticking = false;
    };

    window.addEventListener('scroll', () => {
        if (!ticking) {
            window.requestAnimationFrame(checkVisibility);
            ticking = true;
        }
    }, { passive: true });

    btn.addEventListener('click', (e) => {
        e.preventDefault();
        window.scrollTo({
            top: 0,
            behavior: 'smooth'
        });
    });

    checkVisibility();
}
