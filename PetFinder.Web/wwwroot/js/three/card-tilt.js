// ═══════════════════════════════════════════════════════════════
// PetFinder — Card Tilt Effect  (Fase 4)
// Micro-interacción 3D: inclinación CSS con reflejo especular
//   • Máximo 10° de rotación
//   • Desactivado en dispositivos táctiles (pointer: coarse)
//   • Respeta prefers-reduced-motion
//   • Glare dinámico que sigue la posición del cursor
// ═══════════════════════════════════════════════════════════════

(function () {
    'use strict';

    // ── Checks de accesibilidad ──────────────────────────────
    if (window.matchMedia('(prefers-reduced-motion: reduce)').matches) return;
    if (window.matchMedia('(pointer: coarse)').matches) return;

    const MAX_DEG = 10;

    document.querySelectorAll('.pf-tilt-card').forEach(card => {

        // Crear overlay de glare (reflejo especular)
        const glare = document.createElement('div');
        glare.style.cssText = `
            position: absolute;
            inset: 0;
            border-radius: inherit;
            pointer-events: none;
            opacity: 0;
            background: radial-gradient(
                circle at 50% 50%,
                rgba(255,255,255,0.35) 0%,
                rgba(255,255,255,0) 70%
            );
            transition: opacity 0.3s ease;
            z-index: 2;
        `;
        // Asegurar que la tarjeta puede contener el glare
        const pos = getComputedStyle(card).position;
        if (pos === 'static') card.style.position = 'relative';
        card.style.overflow = 'hidden';
        card.appendChild(glare);

        card.addEventListener('mousemove', e => {
            const rect = card.getBoundingClientRect();
            const x = (e.clientX - rect.left) / rect.width - 0.5;   // -0.5 → 0.5
            const y = (e.clientY - rect.top) / rect.height - 0.5;

            const rotateY = x * MAX_DEG;     // izq-der
            const rotateX = -y * MAX_DEG;    // arriba-abajo

            card.style.transform = `
                perspective(600px)
                rotateY(${rotateY}deg)
                rotateX(${rotateX}deg)
                translateY(-4px)
                scale3d(1.02, 1.02, 1.02)
            `;

            // Posicionar el glare
            const glareX = (x + 0.5) * 100;
            const glareY = (y + 0.5) * 100;
            glare.style.background = `radial-gradient(
                circle at ${glareX}% ${glareY}%,
                rgba(255,255,255,0.25) 0%,
                rgba(255,255,255,0) 60%
            )`;
            glare.style.opacity = '1';
        });

        card.addEventListener('mouseleave', () => {
            card.style.transform = '';
            card.style.transition = 'transform 0.5s cubic-bezier(0.4, 0, 0.2, 1)';
            glare.style.opacity = '0';

            // Quitar la transición después de completarse para no interferir con mousemove
            setTimeout(() => {
                card.style.transition = '';
            }, 500);
        });

        card.addEventListener('mouseenter', () => {
            card.style.transition = '';
        });
    });
})();
