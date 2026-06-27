// Mapa região do montador + trajeto até a obra (OSM + Nominatim + OSRM)
(function () {
    "use strict";

    var leafletLoaded = false;

    function onlyDigits(v) {
        return (v || "").replace(/\D/g, "");
    }

    function buildMontadorLabel(el) {
        var parts = [];
        if (el.dataset.bairro) parts.push(el.dataset.bairro);
        var city = el.dataset.cidade || "";
        var uf = el.dataset.estado || "";
        if (city) parts.push(city + (uf ? " - " + uf : ""));
        return parts.filter(Boolean).join(", ") || "Grande São Paulo";
    }

    function buildMontadorQuery(el) {
        var cep = onlyDigits(el.dataset.cep);
        if (cep.length === 8) return cep + ", Brasil";
        var parts = [];
        if (el.dataset.bairro) parts.push(el.dataset.bairro);
        if (el.dataset.cidade) parts.push(el.dataset.cidade);
        if (el.dataset.estado) parts.push(el.dataset.estado);
        parts.push("Brasil");
        return parts.filter(Boolean).join(", ");
    }

    function buildObraQuery(el) {
        var cep = onlyDigits(el.dataset.obraCep);
        if (cep.length === 8) return cep + ", Brasil";
        var end = (el.dataset.obraEndereco || "").trim();
        if (end) return end + ", Brasil";
        return null;
    }

    function cardRoot(el) {
        return el.closest(".pc-marceneiro-map-card");
    }

    function setExternalLinks(card, originLat, originLon, destLat, destLon, label, isRoute) {
        var google = card.querySelector(".pc-map-open-google");
        if (google) {
            if (isRoute && originLat && destLat) {
                google.href = "https://www.google.com/maps/dir/?api=1&origin=" + originLat + "," + originLon
                    + "&destination=" + destLat + "," + destLon + "&travelmode=driving";
            } else if (originLat && originLon) {
                google.href = "https://www.google.com/maps/search/?api=1&query=" + originLat + "," + originLon;
            } else {
                google.href = "https://www.google.com/maps/search/?api=1&query=" + encodeURIComponent(label);
            }
        }
        var waze = card.querySelector(".pc-map-open-waze");
        if (waze && destLat && destLon) {
            waze.href = isRoute
                ? "https://waze.com/ul?ll=" + destLat + "," + destLon + "&navigate=yes"
                : "https://waze.com/ul?ll=" + destLat + "," + destLon + "&navigate=no";
            waze.hidden = false;
        }
    }

    function showRouteMeta(card, distanceM, durationS) {
        var meta = card.querySelector(".pc-marceneiro-map-route-meta");
        if (!meta) return;
        meta.hidden = false;
        var km = (distanceM / 1000).toFixed(1);
        var mins = Math.round(durationS / 60);
        var distEl = meta.querySelector(".pc-map-route-distance");
        var durEl = meta.querySelector(".pc-map-route-duration");
        if (distEl) distEl.textContent = km + " km";
        if (durEl) durEl.textContent = "~" + mins + " min de carro";
    }

    function loadLeaflet() {
        return new Promise(function (resolve, reject) {
            if (window.L) {
                resolve();
                return;
            }
            if (leafletLoaded) {
                var t = setInterval(function () {
                    if (window.L) {
                        clearInterval(t);
                        resolve();
                    }
                }, 50);
                setTimeout(function () {
                    clearInterval(t);
                    reject();
                }, 8000);
                return;
            }
            leafletLoaded = true;
            var css = document.createElement("link");
            css.rel = "stylesheet";
            css.href = "https://unpkg.com/leaflet@1.9.4/dist/leaflet.css";
            document.head.appendChild(css);
            var js = document.createElement("script");
            js.src = "https://unpkg.com/leaflet@1.9.4/dist/leaflet.js";
            js.onload = function () { resolve(); };
            js.onerror = function () { reject(); };
            document.head.appendChild(js);
        });
    }

    function geocode(query) {
        var url = "https://nominatim.openstreetmap.org/search?format=json&limit=1&countrycodes=br&addressdetails=0&q="
            + encodeURIComponent(query);
        return fetch(url, { headers: { "Accept-Language": "pt-BR,pt;q=0.9" } })
            .then(function (r) { return r.json(); })
            .then(function (data) {
                if (!data || !data.length) return null;
                return {
                    lat: parseFloat(data[0].lat),
                    lon: parseFloat(data[0].lon),
                    display: data[0].display_name
                };
            });
    }

    function fetchRoute(fromLon, fromLat, toLon, toLat) {
        var url = "https://router.project-osrm.org/route/v1/driving/"
            + fromLon + "," + fromLat + ";" + toLon + "," + toLat
            + "?overview=full&geometries=geojson";
        return fetch(url).then(function (r) { return r.json(); }).then(function (data) {
            if (!data.routes || !data.routes.length) return null;
            return data.routes[0];
        });
    }

    function hammerIcon() {
        return L.divIcon({
            className: "pc-map-pin",
            html: "<i class=\"bi bi-hammer\"></i>",
            iconSize: [36, 36],
            iconAnchor: [18, 36]
        });
    }

    function obraIcon() {
        return L.divIcon({
            className: "pc-map-pin obra",
            html: "<i class=\"bi bi-building\"></i>",
            iconSize: [36, 36],
            iconAnchor: [18, 36]
        });
    }

    function showRegionMap(container, canvas, lat, lon, label) {
        container.querySelector(".pc-marceneiro-map-loading").hidden = true;
        canvas.hidden = false;

        var map = L.map(canvas, { scrollWheelZoom: false }).setView([lat, lon], 13);
        L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {
            maxZoom: 18,
            attribution: "&copy; OpenStreetMap"
        }).addTo(map);

        L.circle([lat, lon], {
            color: "#1B5E4B",
            fillColor: "#1B5E4B",
            fillOpacity: 0.18,
            weight: 2,
            radius: 1500
        }).addTo(map);

        L.marker([lat, lon], { icon: hammerIcon() }).addTo(map)
            .bindPopup("<strong>" + (container.dataset.nome || "Montador") + "</strong><br><span class=\"small\">" + label + "</span>");

        setTimeout(function () { map.invalidateSize(); }, 200);
    }

    function showRouteMap(container, canvas, origin, dest, route) {
        container.querySelector(".pc-marceneiro-map-loading").hidden = true;
        canvas.hidden = false;

        var coords = route.geometry.coordinates.map(function (c) { return [c[1], c[0]]; });
        var map = L.map(canvas, { scrollWheelZoom: false });
        L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {
            maxZoom: 18,
            attribution: "&copy; OpenStreetMap"
        }).addTo(map);

        var line = L.polyline(coords, {
            color: "#1B5E4B",
            weight: 5,
            opacity: 0.85
        }).addTo(map);

        L.marker([origin.lat, origin.lon], { icon: hammerIcon() }).addTo(map)
            .bindPopup("<strong>" + (container.dataset.nome || "Montador") + "</strong><br><span class=\"small\">Região de atuação</span>");

        L.marker([dest.lat, dest.lon], { icon: obraIcon() }).addTo(map)
            .bindPopup("<strong>Obra</strong><br><span class=\"small\">" + (dest.display || "Local da montagem") + "</span>");

        map.fitBounds(line.getBounds(), { padding: [28, 28] });
        setTimeout(function () { map.invalidateSize(); }, 200);

        return route;
    }

    function showFallback(container, label) {
        container.querySelector(".pc-marceneiro-map-loading").hidden = true;
        var fb = container.querySelector(".pc-marceneiro-map-fallback");
        if (fb) {
            fb.hidden = false;
            var lbl = fb.querySelector(".pc-marceneiro-map-label");
            if (lbl) lbl.textContent = label;
        }
    }

    function initMap(el) {
        var card = cardRoot(el);
        var label = buildMontadorLabel(el);
        var mostrarTrajeto = el.dataset.mostrarTrajeto === "true";
        var obraQuery = buildObraQuery(el);

        setExternalLinks(card, null, null, null, null, label, false);

        var originQuery = buildMontadorQuery(el);

        geocode(originQuery).then(function (origin) {
            if (!origin) {
                showFallback(el, label);
                return;
            }

            if (mostrarTrajeto && obraQuery) {
                return geocode(obraQuery).then(function (dest) {
                    if (!dest) {
                        return loadLeaflet().then(function () {
                            var canvas = el.querySelector(".pc-marceneiro-map-canvas");
                            if (canvas) showRegionMap(el, canvas, origin.lat, origin.lon, label);
                            setExternalLinks(card, origin.lat, origin.lon, null, null, label, false);
                        });
                    }

                    return fetchRoute(origin.lon, origin.lat, dest.lon, dest.lat).then(function (route) {
                        return loadLeaflet().then(function () {
                            var canvas = el.querySelector(".pc-marceneiro-map-canvas");
                            if (!canvas) return;
                            if (route) {
                                showRouteMap(el, canvas, origin, dest, route);
                                showRouteMeta(card, route.distance, route.duration);
                                setExternalLinks(card, origin.lat, origin.lon, dest.lat, dest.lon, label, true);
                            } else {
                                showRegionMap(el, canvas, origin.lat, origin.lon, label);
                                setExternalLinks(card, origin.lat, origin.lon, dest.lat, dest.lon, label, true);
                            }
                        });
                    });
                });
            }

            setExternalLinks(card, origin.lat, origin.lon, null, null, label, false);
            return loadLeaflet().then(function () {
                var canvas = el.querySelector(".pc-marceneiro-map-canvas");
                if (!canvas) return;
                showRegionMap(el, canvas, origin.lat, origin.lon, label || origin.display);
            });
        }).catch(function () {
            showFallback(el, label);
        });
    }

    document.addEventListener("DOMContentLoaded", function () {
        document.querySelectorAll(".pc-marceneiro-map").forEach(initMap);
    });
})();
