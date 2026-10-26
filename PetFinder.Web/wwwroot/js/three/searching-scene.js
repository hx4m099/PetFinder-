// ═══════════════════════════════════════════════════════════════
// PetFinder — Searching Scene  (Three.js  ·  Fase 4)
// Escáner holográfico 3D con:
//   • Foto subida como textura central (si disponible)
//   • Línea de escaneo horizontal animada
//   • Anillos giroscópicos orbitando
//   • Partículas de datos convergentes
//   • Esfera núcleo pulsante
// ═══════════════════════════════════════════════════════════════

import * as THREE from 'three';
import { SceneManager } from './scene-manager.js';

class SearchingScene extends SceneManager {

    // ── Init ─────────────────────────────────────────────────
    init() {
        this._elapsed = 0;

        // Cámara
        this.camera.position.set(0, 0, 5);
        this.camera.lookAt(0, 0, 0);

        // ── Luces ────────────────────────────────────────────
        this.scene.add(new THREE.AmbientLight(0xFFE0B2, 0.3));

        const point = new THREE.PointLight(0xFF7043, 1.5, 15);
        point.position.set(0, 0, 3);
        this.scene.add(point);

        const point2 = new THREE.PointLight(0x66BB6A, 0.8, 12);
        point2.position.set(-2, 2, 1);
        this.scene.add(point2);

        // ── Núcleo central ───────────────────────────────────
        this._buildCore();

        // ── Anillos giroscópicos ──────────────────────────────
        this._buildRings();

        // ── Partículas convergentes ──────────────────────────
        this._buildParticles();

        // ── Línea de escaneo ─────────────────────────────────
        this._buildScanLine();
    }

    // ════════════════════════════════════════════════════════
    //  NÚCLEO CENTRAL (con foto como textura si disponible)
    // ════════════════════════════════════════════════════════
    _buildCore() {
        const photoUrl = this.canvas.dataset.photo || null;

        // Esfera interna pulsante (glow)
        const glowMat = new THREE.MeshBasicMaterial({
            color: 0xFF7043,
            transparent: true,
            opacity: 0.25,
        });
        this._glowSphere = new THREE.Mesh(
            new THREE.SphereGeometry(0.85, 20, 16),
            glowMat
        );
        this.scene.add(this._glowSphere);

        // Esfera principal
        if (photoUrl) {
            // Cargar la foto como textura
            const loader = new THREE.TextureLoader();
            loader.load(
                photoUrl,
                (texture) => {
                    texture.colorSpace = THREE.SRGBColorSpace;
                    this._coreMat = new THREE.MeshStandardMaterial({
                        map: texture,
                        roughness: 0.3,
                        metalness: 0.2,
                        transparent: true,
                        opacity: 0.92,
                    });
                    this._coreSphere.material = this._coreMat;
                    this._photoTexture = texture;
                },
                undefined,
                () => this._createDefaultCore()
            );
            // Placeholder mientras carga
            this._coreMat = new THREE.MeshStandardMaterial({
                color: 0xFF7043,
                roughness: 0.4,
                metalness: 0.3,
                transparent: true,
                opacity: 0.8,
                wireframe: true,
            });
        } else {
            this._createDefaultCore();
        }

        this._coreSphere = new THREE.Mesh(
            new THREE.SphereGeometry(0.65, 24, 18),
            this._coreMat
        );
        this.scene.add(this._coreSphere);

        this._mats = [glowMat, this._coreMat];
    }

    _createDefaultCore() {
        this._coreMat = new THREE.MeshStandardMaterial({
            color: 0xFF7043,
            roughness: 0.4,
            metalness: 0.3,
            transparent: true,
            opacity: 0.85,
        });
    }

    // ════════════════════════════════════════════════════════
    //  ANILLOS GIROSCÓPICOS
    // ════════════════════════════════════════════════════════
    _buildRings() {
        this._rings = [];
        const colors = [0xFF7043, 0x66BB6A, 0xFFB74D];
        const radii = [1.2, 1.5, 1.8];

        for (let i = 0; i < 3; i++) {
            const ringMat = new THREE.MeshBasicMaterial({
                color: colors[i],
                transparent: true,
                opacity: 0.35,
                side: THREE.DoubleSide,
            });
            const ring = new THREE.Mesh(
                new THREE.TorusGeometry(radii[i], 0.02, 8, 60),
                ringMat
            );
            // Cada anillo en un eje diferente
            ring.rotation.x = (i * Math.PI) / 3;
            ring.rotation.y = (i * Math.PI) / 4;
            this.scene.add(ring);
            this._rings.push({ mesh: ring, axis: i, speed: 0.4 + i * 0.2 });
            this._mats.push(ringMat);
        }
    }

    // ════════════════════════════════════════════════════════
    //  PARTÍCULAS CONVERGENTES
    // ════════════════════════════════════════════════════════
    _buildParticles() {
        const count = 80;
        const positions = new Float32Array(count * 3);
        this._pData = [];

        for (let i = 0; i < count; i++) {
            // Empiezan en posiciones aleatorias lejanas
            const theta = Math.random() * Math.PI * 2;
            const phi = Math.acos(2 * Math.random() - 1);
            const r = 2.5 + Math.random() * 2;
            const x = r * Math.sin(phi) * Math.cos(theta);
            const y = r * Math.sin(phi) * Math.sin(theta);
            const z = r * Math.cos(phi);
            positions[i * 3]     = x;
            positions[i * 3 + 1] = y;
            positions[i * 3 + 2] = z;
            this._pData.push({
                origin: new THREE.Vector3(x, y, z),
                speed: 0.3 + Math.random() * 0.7,
                phase: Math.random() * Math.PI * 2,
                radius: r,
            });
        }

        const geo = new THREE.BufferGeometry();
        geo.setAttribute('position', new THREE.BufferAttribute(positions, 3));

        // Textura glow
        const pCanvas = document.createElement('canvas');
        pCanvas.width = 32; pCanvas.height = 32;
        const pCtx = pCanvas.getContext('2d');
        const grad = pCtx.createRadialGradient(16, 16, 0, 16, 16, 16);
        grad.addColorStop(0, 'rgba(102,187,106,0.9)');
        grad.addColorStop(0.5, 'rgba(102,187,106,0.3)');
        grad.addColorStop(1, 'rgba(102,187,106,0)');
        pCtx.fillStyle = grad;
        pCtx.fillRect(0, 0, 32, 32);
        const tex = new THREE.CanvasTexture(pCanvas);

        const mat = new THREE.PointsMaterial({
            map: tex,
            size: 0.12,
            sizeAttenuation: true,
            transparent: true,
            opacity: 0.7,
            depthWrite: false,
            blending: THREE.AdditiveBlending,
            color: 0x66BB6A,
        });

        this._particles = new THREE.Points(geo, mat);
        this.scene.add(this._particles);
        this._pGeo = geo;
        this._pMat = mat;
        this._pTex = tex;
    }

    // ════════════════════════════════════════════════════════
    //  LÍNEA DE ESCANEO
    // ════════════════════════════════════════════════════════
    _buildScanLine() {
        const scanGeo = new THREE.PlaneGeometry(2.2, 0.04);
        const scanMat = new THREE.MeshBasicMaterial({
            color: 0x66BB6A,
            transparent: true,
            opacity: 0.7,
            side: THREE.DoubleSide,
        });
        this._scanLine = new THREE.Mesh(scanGeo, scanMat);
        this._scanLine.position.z = 0.7;
        this.scene.add(this._scanLine);

        // Glow de la línea
        const scanGlowGeo = new THREE.PlaneGeometry(2.4, 0.2);
        const scanGlowMat = new THREE.MeshBasicMaterial({
            color: 0x66BB6A,
            transparent: true,
            opacity: 0.15,
            side: THREE.DoubleSide,
        });
        this._scanGlow = new THREE.Mesh(scanGlowGeo, scanGlowMat);
        this._scanGlow.position.z = 0.69;
        this.scene.add(this._scanGlow);

        this._mats.push(scanMat, scanGlowMat);
    }

    // ════════════════════════════════════════════════════════
    //  UPDATE
    // ════════════════════════════════════════════════════════
    update(delta) {
        this._elapsed += delta;
        const t = this._elapsed;

        // ── Núcleo: pulso ────────────────
        const pulse = 1 + Math.sin(t * 2.5) * 0.08;
        this._coreSphere.scale.setScalar(pulse);
        this._coreSphere.rotation.y += delta * 0.3;

        // Glow pulsante
        const glowPulse = 1 + Math.sin(t * 3) * 0.15;
        this._glowSphere.scale.setScalar(glowPulse);
        this._glowSphere.material.opacity = 0.15 + Math.sin(t * 2) * 0.1;

        // ── Anillos giroscópicos ─────────
        for (const r of this._rings) {
            switch (r.axis) {
                case 0: r.mesh.rotation.x += delta * r.speed; break;
                case 1: r.mesh.rotation.y += delta * r.speed; break;
                case 2: r.mesh.rotation.z += delta * r.speed; break;
            }
            // Pulso de tamaño en los anillos
            const s = 1 + Math.sin(t * 1.5 + r.axis) * 0.05;
            r.mesh.scale.setScalar(s);
        }

        // ── Partículas convergen/divergen ─
        const pos = this._pGeo.attributes.position.array;
        for (let i = 0; i < this._pData.length; i++) {
            const p = this._pData[i];
            const cycleT = (t * p.speed + p.phase) % (Math.PI * 2);
            // Convergen hacia el centro y vuelven a salir
            const rFactor = 0.3 + Math.abs(Math.sin(cycleT)) * (p.radius - 0.3);
            const theta = cycleT * 2 + p.phase;
            const phi = Math.acos(Math.sin(cycleT * 0.5 + p.phase));
            pos[i * 3]     = rFactor * Math.sin(phi) * Math.cos(theta);
            pos[i * 3 + 1] = rFactor * Math.sin(phi) * Math.sin(theta);
            pos[i * 3 + 2] = rFactor * Math.cos(phi);
        }
        this._pGeo.attributes.position.needsUpdate = true;

        // ── Línea de escaneo ─────────────
        const scanY = Math.sin(t * 1.8) * 0.8;
        this._scanLine.position.y = scanY;
        this._scanGlow.position.y = scanY;
        this._scanLine.material.opacity = 0.5 + Math.sin(t * 6) * 0.2;
    }

    // ════════════════════════════════════════════════════════
    //  DISPOSE
    // ════════════════════════════════════════════════════════
    onDispose() {
        if (this._mats) this._mats.forEach(m => m.dispose());
        if (this._pMat) this._pMat.dispose();
        if (this._pTex) this._pTex.dispose();
        if (this._pGeo) this._pGeo.dispose();
        if (this._photoTexture) this._photoTexture.dispose();
        this.scene.traverse(obj => {
            if (obj.geometry) obj.geometry.dispose();
        });
    }

    // ════════════════════════════════════════════════════════
    //  FALLBACK Canvas 2D
    // ════════════════════════════════════════════════════════
    renderFallback(ctx, w, h) {
        if (!this._fbAngle) this._fbAngle = 0;
        const cx = w / 2, cy = h / 2;
        const emojis = ['🐾', '🐶', '🐱', '🔍'];
        const r = Math.min(w, h) * 0.25;

        emojis.forEach((e, i) => {
            const a = this._fbAngle + (Math.PI * 2 / emojis.length) * i;
            const x = cx + Math.cos(a) * r;
            const y = cy + Math.sin(a) * r;
            ctx.font = '28px serif';
            ctx.globalAlpha = 0.7 + 0.3 * Math.sin(this._fbAngle + i);
            ctx.fillText(e, x - 14, y + 10);
        });

        // Centro pulsante
        ctx.globalAlpha = 0.15 + 0.1 * Math.sin(this._fbAngle * 3);
        ctx.beginPath();
        ctx.arc(cx, cy, 30 + 8 * Math.sin(this._fbAngle * 2), 0, Math.PI * 2);
        ctx.fillStyle = '#FF7043';
        ctx.fill();

        this._fbAngle += 0.025;
    }
}

// ── Instanciar ───────────────────────────────────────────────
new SearchingScene('searching-canvas');
