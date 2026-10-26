// ═══════════════════════════════════════════════════════════════
// PetFinder — Landing Scene  (Three.js  ·  Fase 4)
// Perrito low-poly interactivo con:
//   • Ojos que siguen el cursor y parpadean
//   • Cola que se menea (clic para acelerar)
//   • Cabeza/cuerpo siguen suavemente el ratón
//   • Campo de partículas cálidas flotantes
//   • Iluminación de tres puntos
// ═══════════════════════════════════════════════════════════════

import * as THREE from 'three';
import { SceneManager } from './scene-manager.js';

class LandingScene extends SceneManager {

    // ── Init ─────────────────────────────────────────────────
    init() {
        // Estado
        this._mouse = new THREE.Vector2(0, 0);
        this._target = new THREE.Vector2(0, 0);
        this._tailSpeed = 1.0;        // velocidad base de la cola
        this._tailBoostUntil = 0;     // timestamp hasta el que dura el boost
        this._blinkTimer = 0;
        this._blinkDuration = 0.12;
        this._blinkInterval = 3 + Math.random() * 2;
        this._isBlinking = false;
        this._elapsed = 0;

        // Cámara
        this.camera.position.set(0, 0.5, 6);
        this.camera.lookAt(0, 0, 0);

        // ── Luces ────────────────────────────────────────────
        this.scene.add(new THREE.AmbientLight(0xFFF3E0, 0.6));

        const key = new THREE.DirectionalLight(0xFFFFFF, 0.9);
        key.position.set(3, 4, 5);
        this.scene.add(key);

        const rim = new THREE.DirectionalLight(0xFF7043, 0.4);
        rim.position.set(-3, 2, -3);
        this.scene.add(rim);

        // ── Construir perrito ─────────────────────────────────
        this._buildDog();

        // ── Partículas ───────────────────────────────────────
        this._buildParticles();

        // ── Eventos ──────────────────────────────────────────
        this._onMouse = e => {
            const rect = this.canvas.getBoundingClientRect();
            this._target.x = ((e.clientX - rect.left) / rect.width) * 2 - 1;
            this._target.y = -((e.clientY - rect.top) / rect.height) * 2 + 1;
        };
        this._onClick = () => {
            this._tailBoostUntil = performance.now() + 2000;  // 2 s de boost
        };
        window.addEventListener('mousemove', this._onMouse);
        this.canvas.addEventListener('click', this._onClick);
        this.canvas.style.cursor = 'pointer';
    }

    // ════════════════════════════════════════════════════════
    //  MODELO DEL PERRITO
    // ════════════════════════════════════════════════════════
    _buildDog() {
        // Materiales reutilizables
        const mat = {
            body:   new THREE.MeshStandardMaterial({ color: 0xD4845A, roughness: 0.7 }),
            bodyLt: new THREE.MeshStandardMaterial({ color: 0xF0C8A0, roughness: 0.7 }),
            nose:   new THREE.MeshStandardMaterial({ color: 0x3E2723, roughness: 0.3 }),
            eye:    new THREE.MeshStandardMaterial({ color: 0xFFFFFF, roughness: 0.2 }),
            pupil:  new THREE.MeshStandardMaterial({ color: 0x1B0E04, roughness: 0.2 }),
            collar: new THREE.MeshStandardMaterial({ color: 0xFF7043, roughness: 0.4 }),
            medal:  new THREE.MeshStandardMaterial({ color: 0xFFD54F, roughness: 0.3, metalness: 0.6 }),
            ear:    new THREE.MeshStandardMaterial({ color: 0xA0623A, roughness: 0.8 }),
            tongue: new THREE.MeshStandardMaterial({ color: 0xF48FB1, roughness: 0.4 }),
        };
        this._mats = Object.values(mat);

        // Grupo raíz del perrito (para mover todo junto)
        this.dog = new THREE.Group();
        this.dog.position.set(0.6, -0.3, 0);

        // ── Cuerpo ────────────────────────
        this.bodyGroup = new THREE.Group();

        const torso = new THREE.Mesh(
            new THREE.CapsuleGeometry(0.55, 1.0, 6, 12),
            mat.body
        );
        torso.rotation.z = Math.PI / 2;
        torso.position.set(0, 0.2, 0);
        this.bodyGroup.add(torso);

        // Pecho más claro
        const chest = new THREE.Mesh(
            new THREE.SphereGeometry(0.42, 10, 10),
            mat.bodyLt
        );
        chest.position.set(0.45, 0.15, 0);
        chest.scale.set(0.9, 0.95, 0.8);
        this.bodyGroup.add(chest);

        // ── Patas ─────────────────────────
        const legGeo = new THREE.CylinderGeometry(0.09, 0.11, 0.65, 8);
        const pawGeo = new THREE.SphereGeometry(0.12, 8, 6);
        const legPositions = [
            [ 0.35,  -0.4,  0.25],
            [ 0.35,  -0.4, -0.25],
            [-0.4 ,  -0.4,  0.25],
            [-0.4 ,  -0.4, -0.25],
        ];
        legPositions.forEach(p => {
            const leg = new THREE.Mesh(legGeo, mat.body);
            leg.position.set(...p);
            this.bodyGroup.add(leg);
            const paw = new THREE.Mesh(pawGeo, mat.bodyLt);
            paw.position.set(p[0], p[1] - 0.35, p[2]);
            this.bodyGroup.add(paw);
        });

        // ── Collar ────────────────────────
        const collar = new THREE.Mesh(
            new THREE.TorusGeometry(0.33, 0.04, 8, 24),
            mat.collar
        );
        collar.position.set(0.5, 0.2, 0);
        collar.rotation.y = Math.PI / 2;
        this.bodyGroup.add(collar);

        // Medalla
        const medal = new THREE.Mesh(
            new THREE.CircleGeometry(0.08, 12),
            mat.medal
        );
        medal.position.set(0.5, -0.05, 0.32);
        medal.rotation.y = 0.3;
        this.bodyGroup.add(medal);

        // ── Cola ──────────────────────────
        this.tailGroup = new THREE.Group();
        this.tailGroup.position.set(-0.65, 0.45, 0);

        const tailSegs = 4;
        const tailGeo = new THREE.CylinderGeometry(0.06, 0.03, 0.22, 6);
        this._tailBones = [];
        let parent = this.tailGroup;
        for (let i = 0; i < tailSegs; i++) {
            const seg = new THREE.Mesh(tailGeo, mat.body);
            seg.position.set(0, 0.18, 0);
            seg.geometry.translate(0, 0.11, 0);
            const pivot = new THREE.Group();
            pivot.add(seg);
            parent.add(pivot);
            this._tailBones.push(pivot);
            parent = pivot;
            // Posicionar siguiente segmento al final del anterior
            pivot.position.set(0, i === 0 ? 0 : 0.18, 0);
        }
        // Bolita de la punta
        const tailTip = new THREE.Mesh(new THREE.SphereGeometry(0.06, 6, 6), mat.bodyLt);
        tailTip.position.set(0, 0.22, 0);
        this._tailBones[tailSegs - 1].add(tailTip);

        this.bodyGroup.add(this.tailGroup);

        // ── Cabeza ────────────────────────
        this.headGroup = new THREE.Group();
        this.headGroup.position.set(0.75, 0.65, 0);

        const skull = new THREE.Mesh(
            new THREE.SphereGeometry(0.4, 12, 10),
            mat.body
        );
        this.headGroup.add(skull);

        // Hocico
        const snout = new THREE.Mesh(
            new THREE.BoxGeometry(0.28, 0.22, 0.26, 2, 2, 2),
            mat.bodyLt
        );
        snout.position.set(0.3, -0.08, 0);
        this.headGroup.add(snout);

        // Nariz
        const nose = new THREE.Mesh(
            new THREE.SphereGeometry(0.07, 8, 6),
            mat.nose
        );
        nose.position.set(0.46, 0, 0);
        this.headGroup.add(nose);

        // Boca / sonrisa (pequeña línea)
        const tongueGeo = new THREE.BoxGeometry(0.06, 0.1, 0.04);
        const tongue = new THREE.Mesh(tongueGeo, mat.tongue);
        tongue.position.set(0.35, -0.2, 0.02);
        this.headGroup.add(tongue);

        // ── Orejas ────────────────────────
        const earGeo = new THREE.ConeGeometry(0.15, 0.35, 6);
        const earL = new THREE.Mesh(earGeo, mat.ear);
        earL.position.set(-0.05, 0.35, 0.28);
        earL.rotation.set(-0.2, 0, -0.4);
        this.headGroup.add(earL);

        const earR = new THREE.Mesh(earGeo, mat.ear);
        earR.position.set(-0.05, 0.35, -0.28);
        earR.rotation.set(0.2, 0, -0.4);
        this.headGroup.add(earR);
        this._ears = [earL, earR];

        // ── Ojos ──────────────────────────
        this._buildEye(mat, 0.2, 0.12, 0.22);    // izquierdo
        this._buildEye(mat, 0.2, 0.12, -0.22);   // derecho

        this.bodyGroup.add(this.headGroup);
        this.dog.add(this.bodyGroup);
        this.scene.add(this.dog);
    }

    _buildEye(mat, x, y, z) {
        const eyeGroup = new THREE.Group();
        eyeGroup.position.set(x, y, z);

        const white = new THREE.Mesh(
            new THREE.SphereGeometry(0.1, 10, 8),
            mat.eye
        );
        eyeGroup.add(white);

        const pupil = new THREE.Mesh(
            new THREE.SphereGeometry(0.055, 8, 6),
            mat.pupil
        );
        pupil.position.set(0.05, 0, 0);
        eyeGroup.add(pupil);

        // Párpado (para parpadeo: escalar en Y)
        const lid = new THREE.Mesh(
            new THREE.SphereGeometry(0.105, 10, 8, 0, Math.PI * 2, 0, Math.PI / 2),
            mat.body
        );
        lid.scale.y = 0;  // oculto por defecto
        lid.rotation.x = Math.PI;
        eyeGroup.add(lid);

        if (!this._eyes) this._eyes = [];
        this._eyes.push({ group: eyeGroup, pupil, lid, white });
        this.headGroup.add(eyeGroup);
    }

    // ════════════════════════════════════════════════════════
    //  PARTÍCULAS FLOTANTES
    // ════════════════════════════════════════════════════════
    _buildParticles() {
        const count = 60;
        const positions = new Float32Array(count * 3);
        const sizes = new Float32Array(count);
        this._particleData = [];

        for (let i = 0; i < count; i++) {
            const px = (Math.random() - 0.5) * 12;
            const py = (Math.random() - 0.5) * 8;
            const pz = (Math.random() - 0.5) * 6 - 2;
            positions[i * 3] = px;
            positions[i * 3 + 1] = py;
            positions[i * 3 + 2] = pz;
            sizes[i] = 3 + Math.random() * 6;
            this._particleData.push({
                base: new THREE.Vector3(px, py, pz),
                speed: 0.2 + Math.random() * 0.3,
                phase: Math.random() * Math.PI * 2
            });
        }

        const geo = new THREE.BufferGeometry();
        geo.setAttribute('position', new THREE.BufferAttribute(positions, 3));
        geo.setAttribute('size', new THREE.BufferAttribute(sizes, 1));

        // Textura procedural (glow circular)
        const pCanvas = document.createElement('canvas');
        pCanvas.width = 64; pCanvas.height = 64;
        const pCtx = pCanvas.getContext('2d');
        const grad = pCtx.createRadialGradient(32, 32, 0, 32, 32, 32);
        grad.addColorStop(0, 'rgba(255,180,100,0.8)');
        grad.addColorStop(0.5, 'rgba(255,112,67,0.3)');
        grad.addColorStop(1, 'rgba(255,112,67,0)');
        pCtx.fillStyle = grad;
        pCtx.fillRect(0, 0, 64, 64);
        const tex = new THREE.CanvasTexture(pCanvas);

        const mat = new THREE.PointsMaterial({
            map: tex,
            size: 0.15,
            sizeAttenuation: true,
            transparent: true,
            opacity: 0.5,
            depthWrite: false,
            blending: THREE.AdditiveBlending,
            color: 0xFFD54F,
        });

        this._particles = new THREE.Points(geo, mat);
        this.scene.add(this._particles);
        this._pGeo = geo;
        this._pMat = mat;
        this._pTex = tex;
    }

    // ════════════════════════════════════════════════════════
    //  UPDATE  (cada frame)
    // ════════════════════════════════════════════════════════
    update(delta) {
        this._elapsed += delta;

        // ── Suavizar ratón ───────────────
        const lerp = 1 - Math.pow(0.05, delta);
        this._mouse.lerp(this._target, lerp);

        // ── Cabeza sigue cursor ──────────
        this.headGroup.rotation.y = this._mouse.x * 0.3;
        this.headGroup.rotation.x = -this._mouse.y * 0.2;

        // ── Cuerpo sigue suavemente ──────
        this.bodyGroup.rotation.y = this._mouse.x * 0.1;
        this.bodyGroup.rotation.x = -this._mouse.y * 0.05;

        // ── Orejas se mueven ligeramente ─
        const earBounce = Math.sin(this._elapsed * 2) * 0.08;
        this._ears[0].rotation.z = -0.4 + earBounce;
        this._ears[1].rotation.z = -0.4 - earBounce;

        // ── Ojos siguen cursor ───────────
        this._updateEyes();

        // ── Parpadeo ─────────────────────
        this._updateBlink(delta);

        // ── Cola ─────────────────────────
        this._updateTail(delta);

        // ── Flotación del perrito ────────
        this.dog.position.y = -0.3 + Math.sin(this._elapsed * 1.2) * 0.08;
        this.dog.rotation.z = Math.sin(this._elapsed * 0.8) * 0.02;

        // ── Partículas ───────────────────
        this._updateParticles(delta);
    }

    _updateEyes() {
        for (const eye of this._eyes) {
            // Pupila se desplaza hacia la dirección del cursor
            const maxShift = 0.035;
            eye.pupil.position.x = 0.05 + this._mouse.x * maxShift;
            eye.pupil.position.y = this._mouse.y * maxShift;
        }
    }

    _updateBlink(delta) {
        this._blinkTimer += delta;

        if (!this._isBlinking && this._blinkTimer >= this._blinkInterval) {
            this._isBlinking = true;
            this._blinkTimer = 0;
            this._blinkInterval = 2.5 + Math.random() * 3;
        }

        if (this._isBlinking) {
            // Cerrar y abrir rápidamente
            const t = this._blinkTimer / this._blinkDuration;
            const lidScale = t < 0.5
                ? t * 2          // cerrando
                : 2 - t * 2;    // abriendo

            for (const eye of this._eyes) {
                eye.lid.scale.y = Math.max(0, lidScale);
            }

            if (t >= 1) {
                this._isBlinking = false;
                this._blinkTimer = 0;
                for (const eye of this._eyes) eye.lid.scale.y = 0;
            }
        }
    }

    _updateTail(delta) {
        const now = performance.now();
        const boosted = now < this._tailBoostUntil;
        const targetSpeed = boosted ? 4.5 : 1.0;
        this._tailSpeed += (targetSpeed - this._tailSpeed) * delta * 3;

        const t = this._elapsed * this._tailSpeed * 4;
        const amplitude = boosted ? 0.6 : 0.35;
        for (let i = 0; i < this._tailBones.length; i++) {
            const wave = Math.sin(t + i * 0.6) * amplitude * (1 + i * 0.25);
            this._tailBones[i].rotation.z = wave;
            // Ligera elevación base
            if (i === 0) this._tailBones[i].rotation.x = -0.5;
        }
    }

    _updateParticles(delta) {
        const pos = this._pGeo.attributes.position.array;
        for (let i = 0; i < this._particleData.length; i++) {
            const p = this._particleData[i];
            const t = this._elapsed * p.speed + p.phase;
            pos[i * 3]     = p.base.x + Math.sin(t) * 0.3;
            pos[i * 3 + 1] = p.base.y + Math.cos(t * 0.7) * 0.4;
            pos[i * 3 + 2] = p.base.z + Math.sin(t * 0.5) * 0.2;
        }
        this._pGeo.attributes.position.needsUpdate = true;
    }

    // ════════════════════════════════════════════════════════
    //  RESIZE
    // ════════════════════════════════════════════════════════
    onResize(_w, _h) { /* la base maneja cámara y renderer */ }

    // ════════════════════════════════════════════════════════
    //  DISPOSE
    // ════════════════════════════════════════════════════════
    onDispose() {
        window.removeEventListener('mousemove', this._onMouse);
        this.canvas.removeEventListener('click', this._onClick);

        // Dispose materiales
        if (this._mats) this._mats.forEach(m => m.dispose());
        if (this._pMat) this._pMat.dispose();
        if (this._pTex) this._pTex.dispose();
        if (this._pGeo) this._pGeo.dispose();

        // Recorrer escena y liberar geometrías
        this.scene.traverse(obj => {
            if (obj.geometry) obj.geometry.dispose();
        });
    }

    // ════════════════════════════════════════════════════════
    //  FALLBACK Canvas 2D (sin WebGL)
    // ════════════════════════════════════════════════════════
    renderFallback(ctx, w, h) {
        // Versión simplificada: emojis flotantes de huellas
        if (!this._fbPaws) {
            this._fbPaws = Array.from({ length: 12 }, () => ({
                x: Math.random(),
                y: Math.random(),
                size: 14 + Math.random() * 18,
                speed: 0.3 + Math.random() * 0.4,
                opacity: 0.06 + Math.random() * 0.12,
                angle: Math.random() * Math.PI * 2
            }));
        }

        for (const p of this._fbPaws) {
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
    }
}

// ── Instanciar ───────────────────────────────────────────────
new LandingScene('hero-canvas');
