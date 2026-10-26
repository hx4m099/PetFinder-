// ═══════════════════════════════════════════════════════════════
// PetFinder — Landing Scene  (Three.js  ·  Cachorro Chibi 3D)
// Modelo Chibi adorable con proporciones exactas:
//   • Cabeza grande, cuerpo redondeado y patas continuas integradas
//   • Orientado hacia +Z (frente) en vista 3/4 frontal ligeramente elevada
//   • Cámara adaptativa con fitCamera() que encuadra perro + salto + márgenes
//   • Hocico unificado, orejas caídas, collar rojo con medalla dorada
//   • Cola articulada con animación sinuosa y aceleración al interactuar
//   • Párpados anatómicos con pivote superior sin deformar la pupila
//   • Salto bajito acotado con ráfaga de corazones flotantes
// ═══════════════════════════════════════════════════════════════

import * as THREE from 'three';
import { SceneManager } from './scene-manager.js';

// ── Constantes ───────────────────────────────────────────────
const TAIL_BASE_SPEED   = 1.2;
const TAIL_HOVER_SPEED  = 3.2;
const TAIL_CLICK_SPEED  = 6.0;
const TAIL_CLICK_DUR    = 2000;   // ms
const IDLE_TIMEOUT      = 10;     // s sin interacción → sentarse
const HEAD_LOOK_LIMIT   = { x: 0.35, y: 0.22, z: 0.15 };
const ENTRANCE_DUR      = 1.5;    // s

// ── Estados de animación ─────────────────────────────────────
const STATE = { IDLE: 0, HOVER: 1, CLICK: 2, SIT: 3, ENTRANCE: 4 };

class LandingScene extends SceneManager {

    // ── Init ─────────────────────────────────────────────────
    init() {
        // Estado interno
        this._mouse           = new THREE.Vector2(0, 0);
        this._target          = new THREE.Vector2(0, 0);
        this._tailSpeed       = TAIL_BASE_SPEED;
        this._tailBoostUntil  = 0;
        this.blinkT           = 0;
        this._blinkTimer      = 0;
        this._blinkInterval   = 2.5 + Math.random() * 3.0; // cada 2.5 a 5.5 s
        this._elapsed         = 0;
        this._state           = STATE.ENTRANCE;
        this._prevState       = STATE.IDLE;
        this._lastInteraction = 0;
        this._entranceT       = 0;
        this._isHovering      = false;
        this._jumpVel         = 0;
        this._jumpY           = 0;
        this._breathPhase     = 0;
        this._sitProgress     = 0;
        this._sitTarget       = 0;
        this._raycaster       = new THREE.Raycaster();
        this._hearts          = [];

        // ── Sincronizar dimensiones y cámara sin deformación ────
        this._updateCanvasSize();
        this.fitCamera();

        // ── Iluminación cálida ──────────────────────────────
        this._setupLighting();

        // ── Construir Cachorro Chibi ────────────────────────
        this._buildDog();

        // ── Sombra de contacto ──────────────────────────────
        this._buildGroundShadow();

        // ── Partículas cálidas flotantes ────────────────────
        this._buildParticles();

        // ── Eventos de interacción ──────────────────────────
        this._setupEvents();
    }

    // ── Encuadre adaptativo de la cámara ─────────────────────
    // Garantiza que SIEMPRE quepa el perro (3 u) + salto (0.6) + márgenes
    fitCamera() {
        const cam = this.camera;
        if (!cam) return;
        const fov = THREE.MathUtils.degToRad(cam.fov);
        const needH = 4.6;   // alto a mostrar: perro (3 u) + salto (0.6) + márgenes
        const needW = 3.8;   // ancho a mostrar
        const dV = (needH / 2) / Math.tan(fov / 2);
        const dH = (needW / 2) / (Math.tan(fov / 2) * cam.aspect);
        const d = Math.max(dV, dH) * 1.1;
        const dir = new THREE.Vector3(0.38, 0.28, 0.9).normalize();
        const target = new THREE.Vector3(0, 1.9, 0);
        cam.position.copy(target).addScaledVector(dir, d);
        cam.lookAt(target);
    }

    // ── Sincronización exacta de tamaño y aspect ratio ────────
    _updateCanvasSize() {
        if (!this.canvas) return;
        const parent = this.canvas.parentElement;
        const w = this.canvas.clientWidth || (parent ? parent.clientWidth : 520) || 520;
        const h = this.canvas.clientHeight || (parent ? parent.clientHeight : 520) || 520;

        if (this.camera) {
            this.camera.aspect = w / h;
            this.camera.updateProjectionMatrix();
            this.fitCamera();
        }
        if (this.renderer) {
            this.renderer.setSize(w, h, false);
            this.renderer.setPixelRatio(Math.min(window.devicePixelRatio || 1, 2));
        }
    }

    // ── Iluminación ──────────────────────────────────────────
    _setupLighting() {
        // Ambiente cálido
        const ambient = new THREE.AmbientLight(0xFFF3E0, 0.75);
        this.scene.add(ambient);

        // Hemisférica suave
        const hemi = new THREE.HemisphereLight(0xFFF8E7, 0xDEB887, 0.5);
        this.scene.add(hemi);

        // Luz principal (key) desde arriba-frente-derecha
        const key = new THREE.DirectionalLight(0xFFF8E7, 1.3);
        key.position.set(3, 5, 4);
        this.scene.add(key);

        // Luz de relleno suave (fill)
        const fill = new THREE.DirectionalLight(0xFFE0B2, 0.6);
        fill.position.set(-3, 3, 2);
        this.scene.add(fill);

        // Luz de contorno cálida (rim)
        const rim = new THREE.DirectionalLight(0xFFA726, 0.7);
        rim.position.set(-1, 4, -4);
        this.scene.add(rim);
    }

    // ── Sombra de contacto ──────────────────────────────────
    _buildGroundShadow() {
        const shadowCanvas = document.createElement('canvas');
        shadowCanvas.width = 128;
        shadowCanvas.height = 128;
        const ctx = shadowCanvas.getContext('2d');
        const grad = ctx.createRadialGradient(64, 64, 0, 64, 64, 64);
        grad.addColorStop(0, 'rgba(0,0,0,0.32)');
        grad.addColorStop(0.5, 'rgba(0,0,0,0.12)');
        grad.addColorStop(1, 'rgba(0,0,0,0)');
        ctx.fillStyle = grad;
        ctx.fillRect(0, 0, 128, 128);
        const shadowTex = new THREE.CanvasTexture(shadowCanvas);

        const shadowGeo = new THREE.PlaneGeometry(2.6, 3.0);
        const shadowMat = new THREE.MeshBasicMaterial({
            map: shadowTex,
            transparent: true,
            depthWrite: false,
            opacity: 0.75,
        });
        this._shadowPlane = new THREE.Mesh(shadowGeo, shadowMat);
        this._shadowPlane.rotation.x = -Math.PI / 2;
        this._shadowPlane.position.set(0, 0.01, 0);
        this.scene.add(this._shadowPlane);
        this._shadowMat = shadowMat;
        this._shadowTex = shadowTex;
    }

    // ════════════════════════════════════════════════════════
    //  MODELO CACHORRO CHIBI (Especificación exacta)
    // ════════════════════════════════════════════════════════
    _buildDog() {
        // Materiales MeshStandardMaterial roughness 0.7
        const mat = {
            // Cuerpo marrón canela (#c27a3c)
            body: new THREE.MeshStandardMaterial({
                color: 0xC27A3C,
                roughness: 0.7,
                metalness: 0.05,
            }),
            // Pecho, hocico, patitas crema (#fae6cd)
            bodyLight: new THREE.MeshStandardMaterial({
                color: 0xFAE6CD,
                roughness: 0.7,
                metalness: 0.02,
            }),
            // Manto / orejas exteriores más oscuro
            bodyDark: new THREE.MeshStandardMaterial({
                color: 0x8E4D20,
                roughness: 0.75,
                metalness: 0.05,
            }),
            // Nariz oscura
            nose: new THREE.MeshStandardMaterial({
                color: 0x1A1009,
                roughness: 0.25,
                metalness: 0.1,
            }),
            // Ojos blancos
            eyeWhite: new THREE.MeshStandardMaterial({
                color: 0xFFFFFF,
                roughness: 0.2,
                metalness: 0.0,
            }),
            // Iris castaño / caramelo
            iris: new THREE.MeshStandardMaterial({
                color: 0x3E1F0A,
                roughness: 0.25,
                metalness: 0.05,
            }),
            // Pupila negra
            pupil: new THREE.MeshStandardMaterial({
                color: 0x0A0604,
                roughness: 0.1,
                metalness: 0.1,
            }),
            // Reflejo blanco specular
            highlight: new THREE.MeshBasicMaterial({
                color: 0xFFFFFF,
            }),
            // Manchas rosadas para mejillas
            blush: new THREE.MeshBasicMaterial({
                color: 0xFF8FA3,
                transparent: true,
                opacity: 0.45,
            }),
            // Lengüita rosa pequeña
            tongue: new THREE.MeshStandardMaterial({
                color: 0xFF7B93,
                roughness: 0.4,
                metalness: 0.0,
            }),
            // Oreja interior sonrosada
            earInner: new THREE.MeshStandardMaterial({
                color: 0xF3B49F,
                roughness: 0.75,
                metalness: 0.0,
            }),
            // Collar rojo
            collar: new THREE.MeshStandardMaterial({
                color: 0xD32F2F,
                roughness: 0.45,
                metalness: 0.1,
            }),
            // Medalla dorada
            medal: new THREE.MeshStandardMaterial({
                color: 0xFFD54F,
                roughness: 0.25,
                metalness: 0.85,
            }),
            // Almohadilla
            pawPad: new THREE.MeshStandardMaterial({
                color: 0x381C0E,
                roughness: 0.7,
                metalness: 0.0,
            }),
        };
        this._mats = Object.values(mat);

        // ── Grupo raíz del perro ─────────────────────────────
        this.dog = new THREE.Group();
        this.dog.position.set(0, 0, 0);
        // Base: orientación frontal +Z muy sutil para pose 3/4 balanceada
        this._baseDogRotY = 0.05;
        this.dog.rotation.y = this._baseDogRotY;
        this.dog.scale.set(0.01, 0.01, 0.01);
        this.dog.visible = true;

        // ── Grupo del cuerpo ────────────────────────────────
        this.bodyGroup = new THREE.Group();

        // 1. CUERPO: esfera radio 1, escala (0.95, 0.85, 1.15), posición (0, 0.95, -0.2), color #c27a3c
        this._bodyMesh = new THREE.Mesh(
            new THREE.SphereGeometry(1.0, 24, 20),
            mat.body
        );
        this._bodyMesh.scale.set(0.95, 0.85, 1.15);
        this._bodyMesh.position.set(0, 0.95, -0.2);
        this.bodyGroup.add(this._bodyMesh);

        // 2. PECHO CREMA: esfera r 0.7, escala (0.85, 0.85, 0.55), posición (0, 0.9, 0.75)
        this._chestLight = new THREE.Mesh(
            new THREE.SphereGeometry(0.7, 20, 16),
            mat.bodyLight
        );
        this._chestLight.scale.set(0.85, 0.85, 0.55);
        this._chestLight.position.set(0, 0.9, 0.75);
        this.bodyGroup.add(this._chestLight);

        // 3. PATAS INTEGRADAS (createLeg continua sin escalones ni tapas planas)
        this._legs = [];
        this._buildLegs(mat);

        // 4. COLLAR ROJO CON MEDALLA DORADA (visibles de frente)
        const collar = new THREE.Mesh(
            new THREE.TorusGeometry(0.66, 0.08, 16, 36),
            mat.collar
        );
        collar.position.set(0, 1.42, 0.48);
        collar.rotation.x = 0.40;
        this.bodyGroup.add(collar);

        // Aro de unión de la medalla
        const ringGeo = new THREE.TorusGeometry(0.055, 0.016, 8, 16);
        const medalRing = new THREE.Mesh(ringGeo, mat.medal);
        medalRing.position.set(0, 1.28, 1.04);
        this.bodyGroup.add(medalRing);

        // Medalla dorada reluciente (colgada al frente del pecho, visible de frente)
        const medalGeo = new THREE.CylinderGeometry(0.17, 0.17, 0.04, 24);
        const medal = new THREE.Mesh(medalGeo, mat.medal);
        medal.position.set(0, 1.14, 1.10);
        medal.rotation.x = Math.PI / 2 - 0.20;
        this.bodyGroup.add(medal);

        // Huellita grabada en la medalla (visible de frente)
        const pawPrint = new THREE.Mesh(
            new THREE.CircleGeometry(0.09, 16),
            mat.pawPad
        );
        pawPrint.position.set(0, 1.14, 1.125);
        pawPrint.rotation.x = -0.20;
        this.bodyGroup.add(pawPrint);

        // 5. COLA ARTICULADA conectada al cuerpo en (0, 1.15, -1.25)
        this._buildTail(mat);

        // 6. CABEZA: grupo en (0, 2.05, 0.55)
        this._buildHead(mat);

        this.bodyGroup.add(this.headGroup);
        this.dog.add(this.bodyGroup);
        this.scene.add(this.dog);
    }

    // ── Patas integradas como pieza continua ─────────────────
    _buildLegs(mat) {
        const brownMat = mat.body;
        const creamMat = mat.bodyLight;

        const addSphere = (parent, radius, material, scale = [1, 1, 1], pos = [0, 0, 0]) => {
            const mesh = new THREE.Mesh(new THREE.SphereGeometry(radius, 24, 16), material);
            mesh.scale.set(scale[0], scale[1], scale[2]);
            mesh.position.set(pos[0], pos[1], pos[2]);
            parent.add(mesh);
            return mesh;
        };

        const createLeg = (x, z, parent) => {
            const leg = new THREE.Group();
            leg.position.set(x, 0, z);
            const r = 0.26;
            // hombro/muslo: esfera marrón que tapa el borde superior y se funde con el cuerpo
            addSphere(leg, 0.34, brownMat, [1, 1.1, 1], [0, 0.78, 0]);
            // pata marrón cónica
            const upper = new THREE.Mesh(new THREE.CylinderGeometry(r * 1.05, r * 0.95, 0.5, 24), brownMat);
            upper.position.y = 0.5;
            leg.add(upper);
            // calcetín crema, mismo radio para que no haya escalón
            const sock = new THREE.Mesh(new THREE.CylinderGeometry(r * 0.95, r * 0.95, 0.2, 24), creamMat);
            sock.position.y = 0.2;
            leg.add(sock);
            // patita redondeada
            addSphere(leg, r, creamMat, [1, 0.62, 1.2], [0, 0.12, 0.05]);
            parent.add(leg);
            return leg;
        };

        // Delanteras: createLeg(±0.5, 0.75)
        [-0.5, 0.5].forEach(x => {
            const leg = createLeg(x, 0.75, this.bodyGroup);
            this._legs.push({ group: leg, isFront: true });
        });

        // Traseras: createLeg(±0.55, -0.85) más un muslo esférico marrón r 0.5 escala (0.7, 0.9, 1) en (x*1.2, 0.75, z+0.1)
        [-0.55, 0.55].forEach(x => {
            const leg = createLeg(x, -0.85, this.bodyGroup);
            const thigh = addSphere(
                this.bodyGroup,
                0.5,
                brownMat,
                [0.7, 0.9, 1.0],
                [x * 1.2, 0.75, -0.85 + 0.1]
            );
            this._legs.push({ group: leg, thigh, isFront: false });
        });
    }

    // ── Cola articulada ─────────────────────────────────────
    _buildTail(mat) {
        this.tailGroup = new THREE.Group();
        // Conectada al cuerpo en (0, 1.15, -1.25)
        this.tailGroup.position.set(0, 1.15, -1.25);

        const tailSegs = 5;
        this._tailBones = [];
        let parent = this.tailGroup;

        for (let i = 0; i < tailSegs; i++) {
            const r = 0.14 - i * 0.02;
            const segLen = 0.22;

            const seg = new THREE.Mesh(
                new THREE.CylinderGeometry(r * 0.85, r, segLen, 10),
                i < 3 ? mat.body : mat.bodyLight
            );
            seg.position.y = segLen / 2;

            const pivot = new THREE.Group();
            pivot.add(seg);
            parent.add(pivot);
            this._tailBones.push(pivot);
            parent = pivot;
            pivot.position.y = i === 0 ? 0 : segLen;
            // Curvatura natural hacia arriba y adelante
            pivot.rotation.x = 0.22;
        }

        // Punta esponjosa crema
        const tailTip = new THREE.Mesh(
            new THREE.SphereGeometry(0.12, 10, 8),
            mat.bodyLight
        );
        tailTip.position.y = 0.18;
        this._tailBones[tailSegs - 1].add(tailTip);

        this.bodyGroup.add(this.tailGroup);
    }

    // ── Cabeza Chibi detallada ──────────────────────────────
    _buildHead(mat) {
        this.headGroup = new THREE.Group();
        // Cabeza: grupo en (0, 2.05, 0.55)
        this.headGroup.position.set(0, 2.05, 0.55);

        // Cráneo: esfera r 0.95, escala (1.08, 0.96, 1)
        const skull = new THREE.Mesh(
            new THREE.SphereGeometry(0.95, 24, 20),
            mat.body
        );
        skull.scale.set(1.08, 0.96, 1.0);
        this.headGroup.add(skull);

        // Hocico: UNA sola esfera crema r 0.42, escala (1.05, 0.78, 0.95) en (0, -0.28, 0.78)
        const snout = new THREE.Mesh(
            new THREE.SphereGeometry(0.42, 20, 16),
            mat.bodyLight
        );
        snout.scale.set(1.05, 0.78, 0.95);
        snout.position.set(0, -0.28, 0.78);
        this.headGroup.add(snout);

        // Nariz oscura r 0.14 en (0, -0.10, 1.12)
        const noseMesh = new THREE.Mesh(
            new THREE.SphereGeometry(0.14, 16, 14),
            mat.nose
        );
        noseMesh.scale.set(1.15, 0.85, 0.95);
        noseMesh.position.set(0, -0.10, 1.12);
        this.headGroup.add(noseMesh);

        // Brillo especular en la nariz
        const noseGleam = new THREE.Mesh(
            new THREE.SphereGeometry(0.035, 8, 6),
            mat.highlight
        );
        noseGleam.position.set(0.04, -0.04, 1.22);
        this.headGroup.add(noseGleam);

        // Lengüita rosa pequeña
        this._tongue = new THREE.Mesh(
            new THREE.CapsuleGeometry(0.065, 0.12, 6, 8),
            mat.tongue
        );
        this._tongue.position.set(0, -0.44, 0.94);
        this._tongue.rotation.x = 0.35;
        this.headGroup.add(this._tongue);

        // Mejillas: solo dos manchas rosadas planas y diminutas
        [-1, 1].forEach(side => {
            const cheek = new THREE.Mesh(
                new THREE.CircleGeometry(0.11, 16),
                mat.blush
            );
            cheek.position.set(side * 0.65, -0.24, 0.72);
            cheek.rotation.y = side * 0.55;
            this.headGroup.add(cheek);
        });

        // Manchas tiernas de cachorro sobre los ojos (brow spots)
        [-1, 1].forEach(side => {
            const browSpot = new THREE.Mesh(
                new THREE.SphereGeometry(0.09, 10, 8),
                mat.bodyLight
            );
            browSpot.scale.set(1.1, 0.7, 0.8);
            browSpot.position.set(side * 0.36, 0.34, 0.84);
            this.headGroup.add(browSpot);
        });

        // Orejas caídas visibles a ambos lados
        this._ears = [];
        [-1, 1].forEach(side => {
            const earGroup = new THREE.Group();
            earGroup.position.set(side * 0.82, 0.36, 0.15);

            // Malla exterior de la oreja
            const earOuter = new THREE.Mesh(
                new THREE.CapsuleGeometry(0.23, 0.78, 8, 12),
                mat.bodyDark
            );
            earOuter.scale.set(0.55, 1.0, 0.65);
            earOuter.position.set(side * 0.07, -0.40, 0);
            earGroup.add(earOuter);

            // Interior sonrosado
            const earInner = new THREE.Mesh(
                new THREE.CapsuleGeometry(0.15, 0.60, 6, 10),
                mat.earInner
            );
            earInner.scale.set(0.48, 1.0, 0.42);
            earInner.position.set(side * 0.05, -0.36, 0.07);
            earGroup.add(earInner);

            // Rotación natural caída hacia adelante y abajo
            earGroup.rotation.set(
                0.18,               // inclinación hacia adelante
                side * 0.12,        // rotación Y
                side * 0.30         // caída hacia afuera
            );
            earGroup.userData.baseRotZ = side * 0.30;
            earGroup.userData.baseRotX = 0.18;

            this.headGroup.add(earGroup);
            this._ears.push(earGroup);
        });

        // Ojos: esfera blanca r 0.2 (escala 1, 1.1, 0.6)
        this._eyes = [];
        this._buildEye(mat, 0.36, 0.06, 0.86, 1);    // izquierdo
        this._buildEye(mat, -0.36, 0.06, 0.86, -1);  // derecho
    }

    // ── Ojo Chibi detallado con párpado superior anatómico ────
    _buildEye(mat, x, y, z, side) {
        const eyeGroup = new THREE.Group();
        eyeGroup.position.set(x, y, z);
        eyeGroup.rotation.y = side * 0.18; // ángulo frontal-lateral suave

        // Globo ocular blanco: esfera blanca r 0.2 (escala 1, 1.1, 0.6)
        const white = new THREE.Mesh(
            new THREE.SphereGeometry(0.20, 16, 14),
            mat.eyeWhite
        );
        white.scale.set(1.0, 1.1, 0.6);
        eyeGroup.add(white);

        // Iris castaño / caramelo
        const iris = new THREE.Mesh(
            new THREE.SphereGeometry(0.125, 14, 12),
            mat.iris
        );
        iris.scale.set(1.0, 1.0, 0.4);
        iris.position.set(0, 0, 0.10);
        eyeGroup.add(iris);

        // Pupila negra
        const pupil = new THREE.Mesh(
            new THREE.SphereGeometry(0.085, 12, 10),
            mat.pupil
        );
        pupil.scale.set(1.0, 1.0, 0.3);
        pupil.position.set(0, 0, 0.13);
        eyeGroup.add(pupil);

        // Brillo principal (catchlight) uniforme
        const hl1 = new THREE.Mesh(
            new THREE.SphereGeometry(0.042, 8, 6),
            mat.highlight
        );
        hl1.position.set(0.035, 0.045, 0.155);
        eyeGroup.add(hl1);

        // Brillo secundario
        const hl2 = new THREE.Mesh(
            new THREE.SphereGeometry(0.022, 6, 4),
            mat.highlight
        );
        hl2.position.set(-0.025, -0.030, 0.155);
        eyeGroup.add(hl2);

        // Párpado superior con pivote en el borde superior del ojo
        const lidPivot = new THREE.Group();
        lidPivot.position.y = 0.24;                 // borde superior del ojo
        eyeGroup.add(lidPivot);
        const lid = new THREE.Mesh(new THREE.SphereGeometry(0.2, 24, 16), mat.body);
        lid.scale.set(1.1, 1.2, 0.7);               // un poco más grande que el ojo
        lid.position.y = -0.24;                     // centrado sobre el ojo; escala hacia arriba
        lidPivot.add(lid);
        lidPivot.scale.y = 0.001;
        lidPivot.visible = false;

        this._eyes.push({ group: eyeGroup, pupil, iris, lidPivot, white, highlight: hl1 });
        this.headGroup.add(eyeGroup);
    }

    // ── Partículas flotantes cálidas ────────────────────────
    _buildParticles() {
        const count = 60;
        const positions = new Float32Array(count * 3);
        const sizes = new Float32Array(count);
        const opacities = new Float32Array(count);
        this._particleData = [];

        for (let i = 0; i < count; i++) {
            const px = (Math.random() - 0.5) * 10;
            const py = (Math.random() - 0.2) * 5;
            const pz = (Math.random() - 0.5) * 8;
            positions[i * 3]     = px;
            positions[i * 3 + 1] = py;
            positions[i * 3 + 2] = pz;
            sizes[i] = 16 + Math.random() * 24;
            opacities[i] = 0.2 + Math.random() * 0.45;

            this._particleData.push({
                base: new THREE.Vector3(px, py, pz),
                speed: 0.25 + Math.random() * 0.45,
                phase: Math.random() * Math.PI * 2,
            });
        }

        const geo = new THREE.BufferGeometry();
        geo.setAttribute('position', new THREE.BufferAttribute(positions, 3));
        geo.setAttribute('size', new THREE.BufferAttribute(sizes, 1));

        // Textura suave para partículas
        const pCanvas = document.createElement('canvas');
        pCanvas.width = 64; pCanvas.height = 64;
        const pCtx = pCanvas.getContext('2d');
        const pGrad = pCtx.createRadialGradient(32, 32, 0, 32, 32, 32);
        pGrad.addColorStop(0, 'rgba(255, 235, 190, 0.9)');
        pGrad.addColorStop(0.4, 'rgba(255, 180, 80, 0.45)');
        pGrad.addColorStop(1, 'rgba(255, 140, 40, 0)');
        pCtx.fillStyle = pGrad;
        pCtx.fillRect(0, 0, 64, 64);
        const tex = new THREE.CanvasTexture(pCanvas);

        const pMat = new THREE.PointsMaterial({
            map: tex,
            size: 0.25,
            transparent: true,
            opacity: 0.6,
            depthWrite: false,
            blending: THREE.AdditiveBlending,
        });

        this._particles = new THREE.Points(geo, pMat);
        this.scene.add(this._particles);
        this._pGeo = geo;
        this._pMat = pMat;
        this._pTex = tex;
    }

    // ── Ráfaga de corazones al clickear ─────────────────────
    _spawnHearts() {
        const heartTex = this._getHeartTexture();
        for (let i = 0; i < 7; i++) {
            const sprite = new THREE.Sprite(
                new THREE.SpriteMaterial({
                    map: heartTex,
                    transparent: true,
                    opacity: 1,
                    depthWrite: false,
                    color: new THREE.Color().setHSL(0.96 + Math.random() * 0.08, 0.85, 0.6),
                })
            );
            sprite.scale.set(0.35, 0.35, 0.35);
            sprite.position.set(
                this.dog.position.x + (Math.random() - 0.5) * 1.2,
                this.dog.position.y + 1.8 + Math.random() * 0.8,
                this.dog.position.z + 0.6 + (Math.random() - 0.5) * 0.6
            );
            this.scene.add(sprite);
            this._hearts.push({
                sprite,
                vel: new THREE.Vector3(
                    (Math.random() - 0.5) * 1.4,
                    1.6 + Math.random() * 1.8,
                    (Math.random() - 0.5) * 0.8
                ),
                life: 1.4 + Math.random() * 0.5,
                age: 0,
            });
        }
    }

    _getHeartTexture() {
        if (this._heartTex) return this._heartTex;
        const c = document.createElement('canvas');
        c.width = 64; c.height = 64;
        const ctx = c.getContext('2d');
        ctx.fillStyle = '#FF3B69';
        ctx.beginPath();
        ctx.moveTo(32, 54);
        ctx.bezierCurveTo(12, 38, 4, 24, 16, 12);
        ctx.bezierCurveTo(24, 4, 30, 10, 32, 16);
        ctx.bezierCurveTo(34, 10, 40, 4, 48, 12);
        ctx.bezierCurveTo(60, 24, 52, 38, 32, 54);
        ctx.fill();
        this._heartTex = new THREE.CanvasTexture(c);
        return this._heartTex;
    }

    // ── Setup de Eventos ────────────────────────────────────
    _setupEvents() {
        this._onMouse = e => {
            const rect = this.canvas.getBoundingClientRect();
            this._target.x = ((e.clientX - rect.left) / rect.width) * 2 - 1;
            this._target.y = -((e.clientY - rect.top) / rect.height) * 2 + 1;
            this._lastInteraction = this._elapsed;

            // Hover sobre el perrito
            this._raycaster.setFromCamera(this._target, this.camera);
            const hits = this._raycaster.intersectObject(this.dog, true);
            const wasHover = this._isHovering;
            this._isHovering = hits.length > 0;
            if (this._isHovering && !wasHover) this._onHoverEnter();
            if (!this._isHovering && wasHover) this._onHoverExit();
        };

        this._onClick = e => {
            const rect = this.canvas.getBoundingClientRect();
            const mx = ((e.clientX - rect.left) / rect.width) * 2 - 1;
            const my = -((e.clientY - rect.top) / rect.height) * 2 + 1;
            this._raycaster.setFromCamera(new THREE.Vector2(mx, my), this.camera);
            const hits = this._raycaster.intersectObject(this.dog, true);
            if (hits.length > 0) this._onClickDog();
        };

        this._onTouch = e => {
            if (e.touches.length > 0) {
                const t = e.touches[0];
                const rect = this.canvas.getBoundingClientRect();
                this._target.x = ((t.clientX - rect.left) / rect.width) * 2 - 1;
                this._target.y = -((t.clientY - rect.top) / rect.height) * 2 + 1;
                this._lastInteraction = this._elapsed;
            }
        };

        window.addEventListener('mousemove', this._onMouse);
        this.canvas.addEventListener('click', this._onClick);
        this.canvas.addEventListener('touchmove', this._onTouch, { passive: true });
        this.canvas.addEventListener('touchstart', e => {
            this._lastInteraction = this._elapsed;
            if (e.touches.length > 0) {
                const t = e.touches[0];
                const rect = this.canvas.getBoundingClientRect();
                const mx = ((t.clientX - rect.left) / rect.width) * 2 - 1;
                const my = -((t.clientY - rect.top) / rect.height) * 2 + 1;
                this._raycaster.setFromCamera(new THREE.Vector2(mx, my), this.camera);
                const hits = this._raycaster.intersectObject(this.dog, true);
                if (hits.length > 0) this._onClickDog();
            }
        }, { passive: true });

        this.canvas.style.cursor = 'pointer';
    }

    _onHoverEnter() {
        if (this._state === STATE.ENTRANCE || this._state === STATE.CLICK) return;
        this._setState(STATE.HOVER);
    }

    _onHoverExit() {
        if (this._state === STATE.ENTRANCE || this._state === STATE.CLICK) return;
        this._setState(STATE.IDLE);
    }

    _onClickDog() {
        this._lastInteraction = this._elapsed;
        // Solo salta si y es 0 (ignora el clic mientras está en el aire)
        if (this._jumpY > 0.001) return;

        this._setState(STATE.CLICK);
        this._tailBoostUntil = performance.now() + TAIL_CLICK_DUR;
        this._jumpVel = 3.6;  // salto bajito (velocidad inicial 3.6)
        this._sitTarget = 0;  // levantarse si estaba sentado
        this._spawnHearts();

        setTimeout(() => {
            if (this._state === STATE.CLICK) {
                this._setState(this._isHovering ? STATE.HOVER : STATE.IDLE);
            }
        }, TAIL_CLICK_DUR);
    }

    _setState(newState) {
        if (newState === this._state) return;
        this._prevState = this._state;
        this._state = newState;
    }

    // ════════════════════════════════════════════════════════
    //  UPDATE  (cada frame)
    // ════════════════════════════════════════════════════════
    update(delta) {
        delta = Math.min(delta, 0.1);
        this._elapsed += delta;

        // Animación de entrada
        if (this._state === STATE.ENTRANCE) {
            this._updateEntrance(delta);
            this._updateParticles(delta);
            return;
        }

        // Timeout de inactividad → Sentarse
        if (this._state !== STATE.CLICK && this._state !== STATE.SIT) {
            if (this._elapsed - this._lastInteraction > IDLE_TIMEOUT) {
                this._setState(STATE.SIT);
                this._sitTarget = 1;
            }
        }

        // Si interactúa tras estar sentado, levantarse
        if (this._state === STATE.SIT && this._elapsed - this._lastInteraction < 0.5) {
            this._setState(STATE.IDLE);
            this._sitTarget = 0;
        }

        // Suavizar posición del ratón
        const lerpFactor = 1 - Math.pow(0.04, delta);
        this._mouse.lerp(this._target, lerpFactor);

        // 1. Cabeza sigue el cursor con mirada y ladeo curioso
        this._updateHeadTracking(delta);

        // 2. Cuerpo sigue sutilmente
        this.bodyGroup.rotation.y = -this._mouse.x * 0.08;
        this.bodyGroup.rotation.x = -this._mouse.y * 0.04;

        // 3. Respiración orgánica
        this._updateBreathing(delta);

        // 4. Orejas con inercia y rebote
        this._updateEars(delta);

        // 5. Ojos siguen cursor
        this._updateEyes();

        // 6. Parpadeo con párpado superior
        this._updateBlink(delta);

        // 7. Cola articulada alegre
        this._updateTail(delta);

        // 8. Salto al hacer click
        this._updateJump(delta);

        // 9. Postura de sentado
        this._updateSit(delta);

        // 10. Flotación idle general + salto
        const floatY = Math.sin(this._elapsed * 1.2) * 0.025;
        this.dog.position.y = floatY + this._jumpY;
        this.dog.rotation.y = this._baseDogRotY + (-this._mouse.x * 0.10);

        // 11. Sombra reacciona dinámicamente al salto
        if (this._shadowPlane) {
            const shadowScale = Math.max(0.3, 1 - this._jumpY * 0.4);
            this._shadowPlane.scale.set(shadowScale, shadowScale, 1);
            this._shadowMat.opacity = 0.75 * shadowScale;
        }

        // 12. Partículas flotantes
        this._updateParticles(delta);

        // 13. Corazones flotantes
        this._updateHearts(delta);
    }

    // ── Animación de entrada con rebote ─────────────────────
    _updateEntrance(delta) {
        this._entranceT += delta;
        const t = Math.min(this._entranceT / ENTRANCE_DUR, 1);

        const easeOutBounce = (x) => {
            const n1 = 7.5625, d1 = 2.75;
            if (x < 1 / d1) return n1 * x * x;
            if (x < 2 / d1) return n1 * (x -= 1.5 / d1) * x + 0.75;
            if (x < 2.5 / d1) return n1 * (x -= 2.25 / d1) * x + 0.9375;
            return n1 * (x -= 2.625 / d1) * x + 0.984375;
        };

        const scale = easeOutBounce(t);
        this.dog.scale.set(scale, scale, scale);

        if (t >= 1) {
            this._setState(STATE.IDLE);
            this._lastInteraction = this._elapsed;
            this.dog.scale.set(1, 1, 1);
        }
    }

    // ── Head Tracking suave ─────────────────────────────────
    _updateHeadTracking(delta) {
        // Mirar izquierda / derecha (Yaw en Y)
        const targetRotY = THREE.MathUtils.clamp(
            -this._mouse.x * 0.35,
            -HEAD_LOOK_LIMIT.x,
            HEAD_LOOK_LIMIT.x
        );
        // Mirar arriba / abajo (Pitch en X)
        const targetRotX = THREE.MathUtils.clamp(
            -this._mouse.y * 0.22,
            -HEAD_LOOK_LIMIT.y,
            HEAD_LOOK_LIMIT.y
        );
        // Tierno ladeo de cabeza de cachorro (Roll en Z)
        const targetRotZ = THREE.MathUtils.clamp(
            this._mouse.x * 0.12,
            -HEAD_LOOK_LIMIT.z,
            HEAD_LOOK_LIMIT.z
        );

        const headLerp = 1 - Math.pow(0.03, delta);
        this.headGroup.rotation.y += (targetRotY - this.headGroup.rotation.y) * headLerp;
        this.headGroup.rotation.x += (targetRotX - this.headGroup.rotation.x) * headLerp;
        this.headGroup.rotation.z += (targetRotZ - this.headGroup.rotation.z) * headLerp;
    }

    // ── Respiración orgánica ────────────────────────────────
    _updateBreathing(delta) {
        this._breathPhase += delta * 2.0;
        const breath = 1 + Math.sin(this._breathPhase) * 0.02;
        if (this._bodyMesh) {
            this._bodyMesh.scale.y = 0.85 * breath;
        }
        if (this._chestLight) {
            this._chestLight.scale.y = 0.85 * breath;
        }
        if (this._tongue) {
            this._tongue.position.y = -0.44 + Math.sin(this._breathPhase) * 0.012;
        }
    }

    // ── Orejas reaccionan con balanceo ──────────────────────
    _updateEars(delta) {
        const earLerp = 1 - Math.pow(0.05, delta);
        const bounce = Math.sin(this._elapsed * 2.6) * 0.05;

        this._ears.forEach((ear, i) => {
            const targetZ = ear.userData.baseRotZ + (i === 0 ? bounce : -bounce);
            ear.rotation.z += (targetZ - ear.rotation.z) * earLerp;
            ear.rotation.x = ear.userData.baseRotX + Math.sin(this._elapsed * 1.8 + i) * 0.03;
        });
    }

    // ── Ojos siguen el cursor ───────────────────────────────
    _updateEyes() {
        for (const eye of this._eyes) {
            const maxShift = 0.035;
            const targetX = THREE.MathUtils.clamp(-this._mouse.x * maxShift, -maxShift, maxShift);
            const targetY = THREE.MathUtils.clamp(this._mouse.y * maxShift, -maxShift, maxShift);
            eye.pupil.position.x = targetX;
            eye.pupil.position.y = targetY;
            eye.iris.position.x = targetX * 0.7;
            eye.iris.position.y = targetY * 0.7;
        }
    }

    // ── Parpadeo con párpado anatómico ───────────────────────
    _updateBlink(delta) {
        if (this.blinkT > 0) {
            this.blinkT -= delta;
            if (this.blinkT < 0) this.blinkT = 0;
        } else {
            this._blinkTimer += delta;
            if (this._blinkTimer >= this._blinkInterval) {
                this.blinkT = 0.18;
                this._blinkTimer = 0;
                this._blinkInterval = 2.5 + Math.random() * 3.0; // cada 2.5 a 5.5 s
            }
        }

        const k = this.blinkT > 0 ? Math.sin((1 - this.blinkT / 0.18) * Math.PI) : 0;
        for (const eye of this._eyes) {
            if (eye.lidPivot) {
                eye.lidPivot.scale.y = Math.max(0.001, k);
                eye.lidPivot.visible = k > 0.02;
            }
        }
    }

    // ── Cola articulada con velocidad reactiva ──────────────
    _updateTail(delta) {
        const now = performance.now();
        const boosted = now < this._tailBoostUntil;

        let targetSpeed;
        if (boosted)               targetSpeed = TAIL_CLICK_SPEED;
        else if (this._isHovering) targetSpeed = TAIL_HOVER_SPEED;
        else                       targetSpeed = TAIL_BASE_SPEED;

        this._tailSpeed += (targetSpeed - this._tailSpeed) * delta * 4;

        const t = this._elapsed * this._tailSpeed * 4.5;
        const amplitude = boosted ? 0.65 : (this._isHovering ? 0.5 : 0.35);

        for (let i = 0; i < this._tailBones.length; i++) {
            const wave = Math.sin(t + i * 0.6) * amplitude * (1 + i * 0.25);
            this._tailBones[i].rotation.y = wave;
        }
    }

    // ── Salto al hacer click ────────────────────────────────
    _updateJump(delta) {
        if (this._jumpVel !== 0 || this._jumpY > 0) {
            this._jumpY += this._jumpVel * delta;
            this._jumpVel -= 14 * delta; // gravedad 14

            // Limita la altura con Math.min(y, 0.6)
            this._jumpY = Math.min(this._jumpY, 0.6);

            if (this._jumpY <= 0) {
                this._jumpY = 0;
                this._jumpVel = 0;
            }
        }
    }

    // ── Postura sentada tras inactividad ────────────────────
    _updateSit(delta) {
        const sitLerp = 1 - Math.pow(0.06, delta);
        this._sitProgress += (this._sitTarget - this._sitProgress) * sitLerp;

        if (this._sitProgress > 0.005) {
            const sit = this._sitProgress;
            // Bajar y retrasar suavemente el cuerpo
            this.bodyGroup.position.y = -sit * 0.35;
            this.bodyGroup.position.z = -sit * 0.20;
            this.bodyGroup.rotation.x = sit * 0.15;
            // Compensar cabeza para mantener contacto visual
            this.headGroup.rotation.x -= sit * 0.15;
        } else {
            this.bodyGroup.position.y = 0;
            this.bodyGroup.position.z = 0;
        }
    }

    // ── Partículas flotantes ────────────────────────────────
    _updateParticles(delta) {
        if (!this._pGeo) return;
        const pos = this._pGeo.attributes.position.array;
        for (let i = 0; i < this._particleData.length; i++) {
            const p = this._particleData[i];
            const t = this._elapsed * p.speed + p.phase;
            pos[i * 3]     = p.base.x + Math.sin(t) * 0.5;
            pos[i * 3 + 1] = p.base.y + Math.cos(t * 0.7) * 0.5;
            pos[i * 3 + 2] = p.base.z + Math.sin(t * 0.5) * 0.4;
        }
        this._pGeo.attributes.position.needsUpdate = true;
    }

    // ── Corazones flotantes ─────────────────────────────────
    _updateHearts(delta) {
        for (let i = this._hearts.length - 1; i >= 0; i--) {
            const h = this._hearts[i];
            h.age += delta;

            h.sprite.position.x += h.vel.x * delta * 0.4;
            h.sprite.position.y += h.vel.y * delta;
            h.sprite.position.z += h.vel.z * delta * 0.4;
            h.vel.y -= delta * 1.8;

            const lifeRatio = h.age / h.life;
            const scale = 0.35 + Math.sin(lifeRatio * Math.PI) * 0.2;
            h.sprite.scale.set(scale, scale, scale);
            h.sprite.material.opacity = Math.max(0, 1 - lifeRatio);

            if (h.age >= h.life) {
                this.scene.remove(h.sprite);
                h.sprite.material.dispose();
                this._hearts.splice(i, 1);
            }
        }
    }

    // ════════════════════════════════════════════════════════
    //  RESIZE
    // ════════════════════════════════════════════════════════
    onResize(w, h) {
        this._updateCanvasSize();
    }

    // ════════════════════════════════════════════════════════
    //  DISPOSE
    // ════════════════════════════════════════════════════════
    onDispose() {
        window.removeEventListener('mousemove', this._onMouse);
        this.canvas.removeEventListener('click', this._onClick);

        if (this._mats) this._mats.forEach(m => m.dispose());
        if (this._pMat) this._pMat.dispose();
        if (this._pTex) this._pTex.dispose();
        if (this._pGeo) this._pGeo.dispose();
        if (this._heartTex) this._heartTex.dispose();
        if (this._shadowMat) this._shadowMat.dispose();
        if (this._shadowTex) this._shadowTex.dispose();

        this._hearts.forEach(h => {
            this.scene.remove(h.sprite);
            h.sprite.material.dispose();
        });

        this.scene.traverse(obj => {
            if (obj.geometry) obj.geometry.dispose();
        });
    }

    // ════════════════════════════════════════════════════════
    //  FALLBACK Canvas 2D (sin WebGL)
    // ════════════════════════════════════════════════════════
    renderFallback(ctx, w, h) {
        if (!this._fbPaws) {
            this._fbPaws = Array.from({ length: 10 }, () => ({
                x: Math.random(),
                y: Math.random(),
                size: 20 + Math.random() * 20,
                speed: 0.3 + Math.random() * 0.4,
                opacity: 0.08 + Math.random() * 0.15,
                angle: Math.random() * Math.PI * 2
            }));
        }

        ctx.clearRect(0, 0, w, h);
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
