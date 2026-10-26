// PetFinder.Web/wwwroot/js/scroll/scroll-progress.js

/**
 * Barra indicadora de progreso de lectura superior (#scroll-progress).
 * Actualiza el ancho en porcentaje relativo al scroll total de la página.
 */
export function initScrollProgress() {
    const progressBar = document.getElementById('scroll-progress');
    if (!progressBar) return;

    let ticking = false;

    const updateProgress = () => {
        const scrollTop = window.scrollY || document.documentElement.scrollTop;
        const scrollHeight = document.documentElement.scrollHeight - document.documentElement.clientHeight;
        const progress = scrollHeight > 0 ? (scrollTop / scrollHeight) * 100 : 0;

        progressBar.style.width = `${Math.min(100, Math.max(0, progress))}%`;
        ticking = false;
    };

    window.addEventListener('scroll', () => {
        if (!ticking) {
            window.requestAnimationFrame(updateProgress);
            ticking = true;
        }
    }, { passive: true });

    // Ejecución inicial
    updateProgress();
}
