// PetFinder — Landing scene (Three.js placeholder)
// Full Three.js implementation comes in Fase 4.
// This module provides a fallback canvas animation for the hero section.

const canvas = document.getElementById('hero-canvas');
if (canvas && window.matchMedia('(prefers-reduced-motion: no-preference)').matches) {
    const ctx = canvas.getContext('2d');
    const dpr = window.devicePixelRatio || 1;

    function resize() {
        const rect = canvas.parentElement.getBoundingClientRect();
        canvas.width = rect.width * dpr;
        canvas.height = rect.height * dpr;
        canvas.style.width = rect.width + 'px';
        canvas.style.height = rect.height + 'px';
        ctx.scale(dpr, dpr);
    }

    const paws = Array.from({ length: 12 }, () => ({
        x: Math.random(),
        y: Math.random(),
        size: 14 + Math.random() * 18,
        speed: 0.3 + Math.random() * 0.4,
        opacity: 0.06 + Math.random() * 0.12,
        angle: Math.random() * Math.PI * 2
    }));

    function draw() {
        const w = canvas.width / dpr;
        const h = canvas.height / dpr;
        ctx.clearRect(0, 0, w, h);

        for (const p of paws) {
            p.y -= p.speed * 0.003;
            p.angle += 0.005;
            if (p.y < -0.1) { p.y = 1.1; p.x = Math.random(); }

            ctx.save();
            ctx.globalAlpha = p.opacity;
            ctx.font = `${p.size}px serif`;
            ctx.translate(p.x * w, p.y * h);
            ctx.rotate(p.angle);
            ctx.fillText('🐾', 0, 0);
            ctx.restore();
        }

        requestAnimationFrame(draw);
    }

    resize();
    window.addEventListener('resize', resize);
    draw();
}
