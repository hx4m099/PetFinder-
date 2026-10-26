// PetFinder.Web/wwwroot/js/scroll/steps-line.js

/**
 * Animación de la línea discontinua conectora entre los 3 pasos (#pasos).
 * Modifica 'stroke-dashoffset' proporcionalmente al avance del scroll
 * sobre la sección de pasos.
 */
export function initStepsLine() {
    const section = document.querySelector('.steps-section');
    const path = document.querySelector('.steps-connector-path');
    if (!section || !path) return;

    const prefersReducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;

    // Obtener longitud real del trazado SVG
    let pathLength = 0;
    try {
        pathLength = path.getTotalLength();
    } catch {
        pathLength = 600;
    }

    path.style.strokeDasharray = `${pathLength} ${pathLength}`;

    if (prefersReducedMotion) {
        path.style.strokeDashoffset = '0';
        return;
    }

    path.style.strokeDashoffset = `${pathLength}`;

    let ticking = false;

    const updateLine = () => {
        const rect = section.getBoundingClientRect();
        const windowHeight = window.innerHeight;

        // Calcular progreso dentro de la sección
        // Inicia cuando la sección entra al tercio inferior de la pantalla
        const startOffset = windowHeight * 0.7;
        const totalDistance = rect.height + startOffset * 0.5;
        const currentDistance = startOffset - rect.top;

        const progress = Math.min(1, Math.max(0, currentDistance / totalDistance));
        const drawLength = pathLength * progress;

        path.style.strokeDashoffset = `${pathLength - drawLength}`;
        ticking = false;
    };

    window.addEventListener('scroll', () => {
        if (!ticking) {
            window.requestAnimationFrame(updateLine);
            ticking = true;
        }
    }, { passive: true });

    updateLine();
}
