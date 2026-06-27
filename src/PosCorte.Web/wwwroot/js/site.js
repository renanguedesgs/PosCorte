// PósCorte — interações globais

(function () {
    "use strict";

    function initReveal() {
        var els = document.querySelectorAll(".reveal");
        if (!els.length) return;
        if (!("IntersectionObserver" in window)) {
            els.forEach(function (el) { el.classList.add("in"); });
            return;
        }
        var io = new IntersectionObserver(function (entries) {
            entries.forEach(function (entry) {
                if (entry.isIntersecting) {
                    entry.target.classList.add("in");
                    io.unobserve(entry.target);
                }
            });
        }, { threshold: 0.12 });
        els.forEach(function (el) { io.observe(el); });
    }

    function initActiveNav() {
        var path = window.location.pathname.toLowerCase();
        document.querySelectorAll(".pc-navbar .nav-link").forEach(function (a) {
            var href = (a.getAttribute("href") || "").toLowerCase();
            if (href && href !== "/" && path.indexOf(href.split("#")[0]) === 0) {
                a.classList.add("active");
            }
        });
    }

    function initCopy() {
        document.querySelectorAll("[data-copy]").forEach(function (btn) {
            btn.addEventListener("click", function () {
                var target = document.querySelector(btn.getAttribute("data-copy"));
                if (!target) return;
                var text = target.value || target.textContent || "";
                navigator.clipboard.writeText(text.trim()).then(function () {
                    var original = btn.innerHTML;
                    btn.innerHTML = '<i class="bi bi-check2 me-1"></i>Copiado!';
                    setTimeout(function () { btn.innerHTML = original; }, 1800);
                });
            });
        });
    }

    // Sticky CTA na landing (aparece após scroll)
    function initStickyCta() {
        var bar = document.getElementById("stickyCta");
        if (!bar) return;
        var hero = document.querySelector(".pc-hero");
        if (!hero) return;

        function toggle() {
            var past = window.scrollY > hero.offsetHeight * 0.55;
            bar.hidden = false;
            bar.classList.toggle("show", past);
        }
        window.addEventListener("scroll", toggle, { passive: true });
        toggle();
    }

    // Contadores animados na landing
    function initCounters() {
        var els = document.querySelectorAll("[data-count]");
        if (!els.length) return;

        function animate(el) {
            var target = parseInt(el.getAttribute("data-count"), 10) || 0;
            var suffix = el.getAttribute("data-suffix") || "";
            var start = 0;
            var duration = 1200;
            var startTime = null;

            function step(ts) {
                if (!startTime) startTime = ts;
                var p = Math.min((ts - startTime) / duration, 1);
                var eased = 1 - Math.pow(1 - p, 3);
                el.textContent = Math.round(start + (target - start) * eased) + suffix;
                if (p < 1) requestAnimationFrame(step);
            }
            requestAnimationFrame(step);
        }

        if (!("IntersectionObserver" in window)) {
            els.forEach(animate);
            return;
        }

        var io = new IntersectionObserver(function (entries) {
            entries.forEach(function (entry) {
                if (entry.isIntersecting) {
                    animate(entry.target);
                    io.unobserve(entry.target);
                }
            });
        }, { threshold: 0.5 });
        els.forEach(function (el) { io.observe(el); });
    }

    document.addEventListener("DOMContentLoaded", function () {
        initReveal();
        initActiveNav();
        initCopy();
        initStickyCta();
        initCounters();
    });
})();
