// Anima los contadores numéricos en la landing page
(function () {
    const counters = document.querySelectorAll('[data-count]');
    if (!counters.length) return;

    const animate = (el, target, duration = 1800) => {
        const start = performance.now();
        const update = (now) => {
            const elapsed = Math.min((now - start) / duration, 1);
            const ease = 1 - Math.pow(1 - elapsed, 3); // ease-out cubic
            el.textContent = Math.round(ease * target);
            if (elapsed < 1) requestAnimationFrame(update);
        };
        requestAnimationFrame(update);
    };

    const observer = new IntersectionObserver((entries) => {
        entries.forEach(e => {
            if (e.isIntersecting) {
                animate(e.target, parseInt(e.target.dataset.count));
                observer.unobserve(e.target);
            }
        });
    }, { threshold: 0.5 });

    counters.forEach(c => observer.observe(c));
})();
