// PetFinder.Web/wwwroot/js/map/refugios-map.js

/**
 * Módulo de integración con Leaflet para el mapa interactivo de refugios asociados.
 * Ofrece sincronización bidireccional entre marcadores y lista de tarjetas,
 * filtros por especie y buscador de texto, y cálculo de cercanía con geolocalización.
 */
document.addEventListener('DOMContentLoaded', () => {
    const mapContainer = document.getElementById('mapa-refugios-container');
    if (!mapContainer) return;

    // Verificar si Leaflet está disponible globalmente
    if (typeof L === 'undefined') {
        console.warn('Leaflet no está cargado. Mostrando modo estático.');
        const fallback = document.getElementById('mapa-fallback');
        if (fallback) fallback.style.display = 'block';
        return;
    }

    const defaultLat = parseFloat(mapContainer.dataset.lat || '19.4172');
    const defaultLon = parseFloat(mapContainer.dataset.lon || '-99.1643');
    const defaultZoom = parseInt(mapContainer.dataset.zoom || '13', 10);

    // Inicializar mapa con scroll desactivado para no bloquear en móviles
    const map = L.map(mapContainer, {
        center: [defaultLat, defaultLon],
        zoom: defaultZoom,
        scrollWheelZoom: false
    });

    // Añadir capa de teselas (CartoDB Positron: estética limpia y moderna)
    L.tileLayer('https://{s}.basemaps.cartocdn.com/rastertiles/voyager/{z}/{x}/{y}{r}.png', {
        attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> &copy; <a href="https://carto.com/attributions">CARTO</a>',
        subdomains: 'abcd',
        maxZoom: 19
    }).addTo(map);

    // Ícono personalizado de huellita naranja (#F26B3A)
    const customIcon = L.divIcon({
        className: 'custom-paw-marker',
        html: `
            <div class="marker-pin">
                <svg width="22" height="22" viewBox="0 0 24 24" fill="currentColor">
                    <circle cx="7" cy="8.5" r="2.5" />
                    <circle cx="17" cy="8.5" r="2.5" />
                    <circle cx="12" cy="5" r="2.5" />
                    <path d="M12 9.5c-3.5 0-6 2.5-6 6.5 0 2.2 2.5 3.5 6 3.5s6-1.3 6-3.5c0-4-2.5-6.5-6-6.5z" />
                </svg>
            </div>
        `,
        iconSize: [38, 46],
        iconAnchor: [19, 44],
        popupAnchor: [0, -42]
    });

    // Ícono para la ubicación del usuario
    const userLocationIcon = L.divIcon({
        className: 'custom-user-marker',
        html: `<div class="user-location-pulse"></div>`,
        iconSize: [24, 24],
        iconAnchor: [12, 12]
    });

    let markers = [];
    let currentRefugios = [];
    let userMarker = null;
    let userCoords = null;
    let activeEspecie = '';

    const listContainer = document.getElementById('refugios-lista-dinamica');
    const searchInput = document.getElementById('buscar-refugio-input');
    const filterButtons = document.querySelectorAll('.especie-filter-btn');
    const locateBtn = document.getElementById('btn-mi-ubicacion');

    // Función para renderizar marcadores en el mapa
    function renderMarkers(refugios) {
        // Limpiar marcadores anteriores
        markers.forEach(m => map.removeLayer(m));
        markers = [];

        refugios.forEach(refugio => {
            if (refugio.latitud == null || refugio.longitud == null) return;

            const popupContent = `
                <div class="map-popup-card">
                    <h4 class="popup-title">${refugio.nombre}</h4>
                    <p class="popup-address"><i class="bi bi-geo-alt"></i> ${refugio.direccion}</p>
                    <div class="popup-meta">
                        <span class="badge-mascotas"><i class="bi bi-heart-fill"></i> ${(refugio.mascotasDisponibles ?? refugio.totalMascotasDisponibles ?? 0)} mascotas</span>
                        ${refugio.distanciaKm != null ? `<span class="badge-distancia"><i class="bi bi-compass"></i> a ${refugio.distanciaKm} km</span>` : ''}
                    </div>
                    <div class="popup-actions mt-2">
                        <a href="tel:${refugio.telefono}" class="btn-popup-call"><i class="bi bi-telephone"></i> ${refugio.telefono}</a>
                        <a href="/Adopcion?refugioId=${refugio.id}" class="btn-popup-adopt">Ver disponibles &rarr;</a>
                    </div>
                </div>
            `;

            const marker = L.marker([refugio.latitud, refugio.longitud], { icon: customIcon })
                .addTo(map)
                .bindPopup(popupContent, { maxWidth: 280, className: 'refugio-custom-popup' });

            marker.refugioId = refugio.id;

            marker.on('click', () => {
                highlightCard(refugio.id, false);
            });

            markers.push(marker);
        });
    }

    // Función para renderizar la lista de tarjetas
    function renderList(refugios) {
        if (!listContainer) return;

        if (refugios.length === 0) {
            listContainer.innerHTML = `
                <div class="text-center py-5 text-muted">
                    <i class="bi bi-search display-6 d-block mb-3"></i>
                    <p class="fw-semibold">No se encontraron refugios con los criterios seleccionados.</p>
                </div>
            `;
            return;
        }

        listContainer.innerHTML = refugios.map(r => `
            <div class="refugio-card-item" data-refugio-id="${r.id}" data-lat="${r.latitud}" data-lon="${r.longitud}">
                <div class="refugio-card-body">
                    <div class="d-flex justify-content-between align-items-start mb-1">
                        <h4 class="refugio-card-title">${r.nombre}</h4>
                        ${r.distanciaKm != null ? `<span class="refugio-distancia-tag"><i class="bi bi-compass"></i> ${r.distanciaKm} km</span>` : ''}
                    </div>
                    <p class="refugio-card-address"><i class="bi bi-geo-alt"></i> ${r.direccion}</p>
                    <div class="refugio-card-footer">
                        <span class="refugio-pets-count"><i class="bi bi-heart-fill text-danger me-1"></i>${(r.mascotasDisponibles ?? r.totalMascotasDisponibles ?? 0)} en adopción</span>
                        <a href="/Adopcion?refugioId=${r.id}" class="refugio-card-link">Conocer &rarr;</a>
                    </div>
                </div>
            </div>
        `).join('');

        // Vincular eventos de click a cada tarjeta
        listContainer.querySelectorAll('.refugio-card-item').forEach(card => {
            card.addEventListener('click', () => {
                const refugioId = parseInt(card.dataset.refugioId, 10);
                const lat = parseFloat(card.dataset.lat);
                const lon = parseFloat(card.dataset.lon);

                highlightCard(refugioId, true);

                if (!isNaN(lat) && !isNaN(lon)) {
                    map.flyTo([lat, lon], 15, { duration: 1.2 });
                    const marker = markers.find(m => m.refugioId === refugioId);
                    if (marker) marker.openPopup();
                }
            });
        });
    }

    // Resaltar tarjeta seleccionada
    function highlightCard(refugioId, shouldScroll) {
        document.querySelectorAll('.refugio-card-item').forEach(card => {
            if (parseInt(card.dataset.refugioId, 10) === refugioId) {
                card.classList.add('is-active');
                if (shouldScroll) {
                    card.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
                }
            } else {
                card.classList.remove('is-active');
            }
        });
    }

    // Cargar datos desde el endpoint API
    async function loadRefugios() {
        try {
            let url = '/api/refugios?';
            const params = new URLSearchParams();
            if (userCoords) {
                params.append('lat', userCoords.lat);
                params.append('lon', userCoords.lon);
            }
            if (activeEspecie) {
                params.append('especie', activeEspecie);
            }

            const response = await fetch(`${url}${params.toString()}`);
            if (!response.ok) throw new Error('Error al consultar refugios');

            currentRefugios = await response.json();
            applyFilters();
        } catch (error) {
            console.error('Error cargando refugios:', error);
        }
    }

    // Filtrar localmente por búsqueda de texto
    function applyFilters() {
        const query = (searchInput?.value || '').trim().toLowerCase();
        let filtered = currentRefugios;

        if (query) {
            filtered = filtered.filter(r =>
                r.nombre.toLowerCase().includes(query) ||
                r.direccion.toLowerCase().includes(query)
            );
        }

        renderMarkers(filtered);
        renderList(filtered);
    }

    // Filtros de botones de especie
    filterButtons.forEach(btn => {
        btn.addEventListener('click', () => {
            filterButtons.forEach(b => b.classList.remove('active'));
            btn.classList.add('active');
            activeEspecie = btn.dataset.especie || '';
            loadRefugios();
        });
    });

    // Input de búsqueda con debounce
    let searchDebounce = null;
    searchInput?.addEventListener('input', () => {
        clearTimeout(searchDebounce);
        searchDebounce = setTimeout(applyFilters, 250);
    });

    // Geolocalización del usuario
    locateBtn?.addEventListener('click', () => {
        if (!navigator.geolocation) {
            alert('La geolocalización no es compatible con este navegador.');
            return;
        }

        locateBtn.classList.add('is-loading');
        locateBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-1"></span> Ubicando...';

        navigator.geolocation.getCurrentPosition(
            (pos) => {
                userCoords = {
                    lat: pos.coords.latitude,
                    lon: pos.coords.longitude
                };

                if (userMarker) map.removeLayer(userMarker);
                userMarker = L.marker([userCoords.lat, userCoords.lon], { icon: userLocationIcon })
                    .addTo(map)
                    .bindPopup('<b>Tu ubicación actual</b>')
                    .openPopup();

                map.flyTo([userCoords.lat, userCoords.lon], 14, { duration: 1.5 });

                locateBtn.classList.remove('is-loading');
                locateBtn.innerHTML = '<i class="bi bi-geo-alt-fill text-success me-1"></i> Cerca de ti';

                loadRefugios();
            },
            (err) => {
                console.warn('Geolocalización rechazada o inaccesible:', err);
                locateBtn.classList.remove('is-loading');
                locateBtn.innerHTML = '<i class="bi bi-crosshair me-1"></i> Usar mi ubicación';
                alert('No fue posible obtener tu ubicación. Por favor verifica tus permisos.');
            },
            { enableHighAccuracy: true, timeout: 8000 }
        );
    });

    // Carga inicial
    loadRefugios();
});
