// PetFinder — Searching animation (placeholder)
// Full Three.js orbiting paws come in Fase 4.

const canvas = document.getElementById('searching-canvas');
if (canvas) {
    const ctx = canvas.getContext('2d');
    const dpr = window.devicePixelRatio || 1;
    canvas.width = 300 * dpr;
    canvas.height = 300 * dpr;
    ctx.scale(dpr, dpr);

    const emojis = ['🐾', '🐶', '🐱', '🔍'];
    let angle = 0;

    function draw() {
        ctx.clearRect(0, 0, 300, 300);
        const cx = 150, cy = 150, r = 80;

        emojis.forEach((e, i) => {
            const a = angle + (Math.PI * 2 / emojis.length) * i;
            const x = cx + Math.cos(a) * r;
            const y = cy + Math.sin(a) * r;
            ctx.font = '28px serif';
            ctx.globalAlpha = 0.7 + 0.3 * Math.sin(angle + i);
            ctx.fillText(e, x - 14, y + 10);
        });

        // Center pulsing circle
        ctx.globalAlpha = 0.15 + 0.1 * Math.sin(angle * 3);
        ctx.beginPath();
        ctx.arc(cx, cy, 30 + 8 * Math.sin(angle * 2), 0, Math.PI * 2);
        ctx.fillStyle = '#FF7043';
        ctx.fill();

        angle += 0.025;
        requestAnimationFrame(draw);
    }

    draw();
}
