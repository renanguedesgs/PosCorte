// Stripe-inspired mesh gradient — forest + copper (lightweight canvas)
(function () {
    "use strict";

    function initMesh(canvas) {
        if (!canvas || !window.requestAnimationFrame) return;

        var ctx = canvas.getContext("2d");
        var blobs = [
            { x: 0.25, y: 0.35, r: 0.45, dx: 0.00008, dy: 0.00006, color: [27, 94, 75] },
            { x: 0.72, y: 0.22, r: 0.38, dx: -0.00006, dy: 0.00009, color: [36, 122, 99] },
            { x: 0.55, y: 0.68, r: 0.42, dx: 0.00005, dy: -0.00007, color: [193, 125, 74] },
            { x: 0.15, y: 0.75, r: 0.35, dx: 0.00007, dy: 0.00004, color: [15, 61, 50] },
            { x: 0.88, y: 0.55, r: 0.28, dx: -0.00004, dy: -0.00005, color: [232, 169, 98] }
        ];
        var w = 0, h = 0, t = 0;
        var reduced = window.matchMedia("(prefers-reduced-motion: reduce)").matches;

        function resize() {
            var rect = canvas.parentElement.getBoundingClientRect();
            w = canvas.width = Math.floor(rect.width);
            h = canvas.height = Math.floor(rect.height);
        }

        function draw() {
            if (!w || !h) return;
            ctx.fillStyle = "#141210";
            ctx.fillRect(0, 0, w, h);

            blobs.forEach(function (b, i) {
                if (!reduced) {
                    b.x += b.dx;
                    b.y += b.dy;
                    if (b.x < 0.05 || b.x > 0.95) b.dx *= -1;
                    if (b.y < 0.05 || b.y > 0.95) b.dy *= -1;
                }
                var pulse = reduced ? 1 : 1 + Math.sin(t * 0.001 + i) * 0.06;
                var cx = b.x * w;
                var cy = b.y * h;
                var radius = b.r * Math.min(w, h) * pulse;
                var g = ctx.createRadialGradient(cx, cy, 0, cx, cy, radius);
                var c = b.color;
                g.addColorStop(0, "rgba(" + c[0] + "," + c[1] + "," + c[2] + ",0.55)");
                g.addColorStop(0.45, "rgba(" + c[0] + "," + c[1] + "," + c[2] + ",0.18)");
                g.addColorStop(1, "rgba(" + c[0] + "," + c[1] + "," + c[2] + ",0)");
                ctx.globalCompositeOperation = "lighter";
                ctx.fillStyle = g;
                ctx.beginPath();
                ctx.arc(cx, cy, radius, 0, Math.PI * 2);
                ctx.fill();
            });
            ctx.globalCompositeOperation = "source-over";
        }

        function loop(ts) {
            t = ts;
            draw();
            if (!reduced) requestAnimationFrame(loop);
        }

        resize();
        window.addEventListener("resize", resize);
        requestAnimationFrame(loop);
    }

    document.addEventListener("DOMContentLoaded", function () {
        document.querySelectorAll(".pc-hero-mesh-canvas").forEach(initMesh);
    });
})();
