// PósCorte — interações globais

(function () {
    "use strict";

    // Scroll reveal (IntersectionObserver)
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

    // Active nav link highlight
    function initActiveNav() {
        var path = window.location.pathname.toLowerCase();
        document.querySelectorAll(".pc-navbar .nav-link").forEach(function (a) {
            var href = (a.getAttribute("href") || "").toLowerCase();
            if (href && href !== "/" && path.indexOf(href.split("#")[0]) === 0) {
                a.classList.add("active");
            }
        });
    }

    // Copy-to-clipboard buttons: [data-copy="#targetId"]
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

    document.addEventListener("DOMContentLoaded", function () {
        initReveal();
        initActiveNav();
        initCopy();
    });
})();
