// PetFinder — Card tilt effect (placeholder)
// Full Three.js tilt comes in Fase 4. This is a CSS-transform fallback.

document.querySelectorAll('.pf-tilt-card').forEach(card => {
    if (window.matchMedia('(prefers-reduced-motion: reduce)').matches) return;

    card.addEventListener('mousemove', e => {
        const rect = card.getBoundingClientRect();
        const x = (e.clientX - rect.left) / rect.width - 0.5;
        const y = (e.clientY - rect.top) / rect.height - 0.5;
        card.style.transform = `perspective(600px) rotateY(${x * 8}deg) rotateX(${-y * 8}deg) translateY(-4px)`;
    });

    card.addEventListener('mouseleave', () => {
        card.style.transform = '';
    });
});
