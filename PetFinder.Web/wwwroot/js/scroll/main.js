// PetFinder.Web/wwwroot/js/scroll/main.js
import { initReveal } from './reveal.js';
import { initCounters } from './counters.js';
import { initScrollProgress } from './scroll-progress.js';
import { initNavbar } from './navbar.js';
import { initParallax } from './parallax.js';
import { initBackToTop } from './back-to-top.js';
import { initStepsLine } from './steps-line.js';
import { initPethoScroll } from './petho-scroll.js';

/**
 * Inicializador principal de las animaciones de scroll y micro-interacciones de la Landing.
 */
function initTiltEffect() {
    if (window.matchMedia('(prefers-reduced-motion: reduce)').matches) return;

    const cards = document.querySelectorAll('[data-tilt]');
    cards.forEach(card => {
        card.addEventListener('mousemove', (e) => {
            const rect = card.getBoundingClientRect();
            const x = e.clientX - rect.left;
            const y = e.clientY - rect.top;

            const centerX = rect.width / 2;
            const centerY = rect.height / 2;

            const rotateX = ((y - centerY) / centerY) * -6; // Máximo 6 grados
            const rotateY = ((x - centerX) / centerX) * 6;

            card.style.transform = `perspective(1000px) rotateX(${rotateX.toFixed(2)}deg) rotateY(${rotateY.toFixed(2)}deg) translateY(-4px)`;
        });

        card.addEventListener('mouseleave', () => {
            card.style.transform = 'perspective(1000px) rotateX(0deg) rotateY(0deg) translateY(0px)';
        });
    });
}

function initFaqAccordion() {
    const triggers = document.querySelectorAll('.faq-trigger');
    triggers.forEach(btn => {
        btn.addEventListener('click', () => {
            const isExpanded = btn.getAttribute('aria-expanded') === 'true';
            const targetId = btn.getAttribute('aria-controls');
            const content = targetId ? document.getElementById(targetId) : btn.nextElementSibling;

            // Cerrar otros acordeones si se desea comportamiento exclusivo
            triggers.forEach(otherBtn => {
                if (otherBtn !== btn) {
                    otherBtn.setAttribute('aria-expanded', 'false');
                    const otherTargetId = otherBtn.getAttribute('aria-controls');
                    const otherContent = otherTargetId ? document.getElementById(otherTargetId) : otherBtn.nextElementSibling;
                    if (otherContent) otherContent.hidden = true;
                }
            });

            btn.setAttribute('aria-expanded', String(!isExpanded));
            if (content) {
                content.hidden = isExpanded;
            }
        });
    });
}

document.addEventListener('DOMContentLoaded', () => {
    initScrollProgress();
    initNavbar();
    initReveal();
    initCounters();
    initParallax();
    initStepsLine();
    initPethoScroll();
    initBackToTop();
    initTiltEffect();
    initFaqAccordion();
});
