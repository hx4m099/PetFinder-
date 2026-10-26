// PetFinder.Web/wwwroot/js/scroll/counters.js

/**
 * Módulo de contadores numéricos animados por scroll.
 * Anima suavemente elementos con [data-counter] usando requestAnimationFrame
 * y una función de suavizado (ease-out cubic).
 */
export function initCounters() {
    const counterElements = document.querySelectorAll('[data-counter]');
    if (counterElements.length === 0) return;

    const prefersReducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;

    // Función de interpolación easeOutCubic
    const easeOutCubic = (t) => 1 - Math.pow(1 - t, 3);

    const animateCounter = (el) => {
        const target = parseInt(el.getAttribute('data-target') || el.textContent.trim(), 10);
        if (isNaN(target)) return;

        const prefix = el.getAttribute('data-prefix') || '';
        const suffix = el.getAttribute('data-suffix') || '';

        if (prefersReducedMotion) {
            el.textContent = `${prefix}${target.toLocaleString()}${suffix}`;
            return;
        }

        const duration = 1800; // ms
        let startTime = null;

        const step = (timestamp) => {
            if (!startTime) startTime = timestamp;
            const progress = Math.min((timestamp - startTime) / duration, 1);
            const easedProgress = easeOutCubic(progress);
            const currentVal = Math.floor(easedProgress * target);

            el.textContent = `${prefix}${currentVal.toLocaleString()}${suffix}`;

            if (progress < 1) {
                requestAnimationFrame(step);
            } else {
                el.textContent = `${prefix}${target.toLocaleString()}${suffix}`;
            }
        };

        requestAnimationFrame(step);
    };

    const observer = new IntersectionObserver((entries, obs) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                animateCounter(entry.target);
                obs.unobserve(entry.target);
            }
        });
    }, {
        threshold: 0.3
    });

    counterElements.forEach(el => observer.observe(el));
}
