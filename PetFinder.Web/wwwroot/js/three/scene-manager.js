// ═══════════════════════════════════════════════════════════════
// PetFinder — SceneManager (Base class para escenas Three.js)
// Provee: WebGL fallback, IntersectionObserver, reduced-motion,
//         ciclo de vida init/update/resize/dispose
// ═══════════════════════════════════════════════════════════════

import * as THREE from 'three';

/**
 * Clase base abstracta para todas las escenas 3D de PetFinder.
 * Subclases deben sobrescribir: init(), update(delta), onDispose()
 * y opcionalmente: onResize(w, h), renderFallback(ctx, w, h)
 */
export class SceneManager {

    /**
     * @param {string} canvasId  - ID del <canvas> en el DOM
     * @param {object} [opts]
     * @param {boolean} [opts.alpha=true]     - fondo transparente
     * @param {number}  [opts.clearColor=0x000000]
     * @param {number}  [opts.clearAlpha=0]
     * @param {boolean} [opts.antialias=true]
     */
    constructor(canvasId, opts = {}) {
        this.canvas = document.getElementById(canvasId);
        if (!this.canvas) return;

        this._opts = { alpha: true, clearColor: 0x000000, clearAlpha: 0, antialias: true, ...opts };
        this._animId = 0;
        this._isVisible = false;
        this._disposed = false;

        // ── Accesibilidad: prefers-reduced-motion ───────────────
        this.reducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;

        // ── Detectar WebGL ──────────────────────────────────────
        if (!SceneManager.hasWebGL()) {
            this._initFallback();
            return;
        }

        this._webgl = true;
        this._setupThree();
        this.init();                     // ← subclase implementa

        // Único frame si reduced-motion
        if (this.reducedMotion) {
            this._render(0);
            return;
        }

        this._setupObserver();
        this._setupResize();
    }

    // ── Métodos abstractos (sobrescribir en subclase) ──────────
    /** Construye escena, luces, geometrías. */
    init() {}
    /** Lógica por frame. @param {number} delta en segundos */
    update(_delta) {}
    /** Limpia geometrías/materiales/texturas propias. */
    onDispose() {}
    /** Hook al redimensionar. @param {number} w @param {number} h */
    onResize(_w, _h) {}
    /** Fallback Canvas 2D. @param {CanvasRenderingContext2D} ctx @param {number} w @param {number} h */
    renderFallback(_ctx, _w, _h) {}

    // ────────────────────────────────────────────────────────────
    // Detección de WebGL (estática, cacheable)
    // ────────────────────────────────────────────────────────────
    static _webglSupported = null;
    static hasWebGL() {
        if (SceneManager._webglSupported !== null) return SceneManager._webglSupported;
        try {
            const c = document.createElement('canvas');
            SceneManager._webglSupported = !!(
                window.WebGLRenderingContext &&
                (c.getContext('webgl2') || c.getContext('webgl') || c.getContext('experimental-webgl'))
            );
        } catch { SceneManager._webglSupported = false; }
        return SceneManager._webglSupported;
    }

    // ────────────────────────────────────────────────────────────
    // Inicialización Three.js
    // ────────────────────────────────────────────────────────────
    _setupThree() {
        const rect = this.canvas.parentElement.getBoundingClientRect();
        const w = rect.width || this.canvas.clientWidth || 300;
        const h = rect.height || this.canvas.clientHeight || 300;

        this.renderer = new THREE.WebGLRenderer({
            canvas: this.canvas,
            alpha: this._opts.alpha,
            antialias: this._opts.antialias,
        });
        this.renderer.setPixelRatio(Math.min(window.devicePixelRatio, 2));
        this.renderer.setSize(w, h, false);
        this.renderer.setClearColor(this._opts.clearColor, this._opts.clearAlpha);

        this.scene  = new THREE.Scene();
        this.camera = new THREE.PerspectiveCamera(50, w / h, 0.1, 100);
        this.camera.position.set(0, 0, 5);
        this.clock  = new THREE.Clock(false); // no auto-start
    }

    // ────────────────────────────────────────────────────────────
    // Loop principal (pausa cuando no visible)
    // ────────────────────────────────────────────────────────────
    _render(forceDelta) {
        if (this._disposed) return;
        const delta = forceDelta !== undefined ? forceDelta : this.clock.getDelta();
        this.update(delta);
        this.renderer.render(this.scene, this.camera);
    }

    _loop = () => {
        if (this._disposed || !this._isVisible) return;
        this._animId = requestAnimationFrame(this._loop);
        this._render();
    };

    _start() {
        if (this._disposed || this._isVisible) return;
        this._isVisible = true;
        this.clock.start();
        this._loop();
    }

    _stop() {
        this._isVisible = false;
        this.clock.stop();
        cancelAnimationFrame(this._animId);
    }

    // ────────────────────────────────────────────────────────────
    // IntersectionObserver para pausar/reanudar
    // ────────────────────────────────────────────────────────────
    _setupObserver() {
        this._observer = new IntersectionObserver(
            entries => {
                entries.forEach(e => {
                    if (e.isIntersecting) this._start();
                    else this._stop();
                });
            },
            { threshold: 0.05 }
        );
        this._observer.observe(this.canvas);
    }

    // ────────────────────────────────────────────────────────────
    // Resize
    // ────────────────────────────────────────────────────────────
    _setupResize() {
        this._onResize = () => {
            if (this._disposed) return;
            const rect = this.canvas.parentElement.getBoundingClientRect();
            const w = rect.width || 300;
            const h = rect.height || 300;
            this.renderer.setSize(w, h, false);
            this.camera.aspect = w / h;
            this.camera.updateProjectionMatrix();
            this.onResize(w, h);
            // Re-render frame inmediato si está pausado por reduced-motion
            if (this.reducedMotion) this._render(0);
        };
        window.addEventListener('resize', this._onResize);
    }

    // ────────────────────────────────────────────────────────────
    // Dispose público
    // ────────────────────────────────────────────────────────────
    dispose() {
        this._disposed = true;
        this._stop();
        if (this._observer) { this._observer.disconnect(); this._observer = null; }
        if (this._onResize) window.removeEventListener('resize', this._onResize);
        this.onDispose();
        if (this.renderer) this.renderer.dispose();
    }

    // ────────────────────────────────────────────────────────────
    // Fallback Canvas 2D (cuando no hay WebGL)
    // ────────────────────────────────────────────────────────────
    _initFallback() {
        this._webgl = false;
        const ctx = this.canvas.getContext('2d');
        if (!ctx) return;

        const dpr = window.devicePixelRatio || 1;
        const resize = () => {
            const rect = this.canvas.parentElement.getBoundingClientRect();
            const w = rect.width || 300;
            const h = rect.height || 300;
            this.canvas.width  = w * dpr;
            this.canvas.height = h * dpr;
            this.canvas.style.width  = w + 'px';
            this.canvas.style.height = h + 'px';
            ctx.setTransform(dpr, 0, 0, dpr, 0, 0);
        };
        resize();
        window.addEventListener('resize', resize);

        const w = () => this.canvas.width / dpr;
        const h = () => this.canvas.height / dpr;

        if (this.reducedMotion) {
            this.renderFallback(ctx, w(), h());
            return;
        }

        const tick = () => {
            ctx.clearRect(0, 0, w(), h());
            this.renderFallback(ctx, w(), h());
            requestAnimationFrame(tick);
        };
        tick();
    }

    // ── Utilidades ──────────────────────────────────────────────
    /** @returns {boolean} True si el dispositivo es táctil primario */
    static isTouchDevice() {
        return window.matchMedia('(pointer: coarse)').matches;
    }
}
