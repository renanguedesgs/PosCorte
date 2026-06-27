// Premium UI — spotlight cards, nav glow, smooth anchors
(function () {
    "use strict";

    function initSpotlight() {
        document.querySelectorAll("[data-spotlight]").forEach(function (card) {
            card.addEventListener("mousemove", function (e) {
                var rect = card.getBoundingClientRect();
                var x = ((e.clientX - rect.left) / rect.width) * 100;
                var y = ((e.clientY - rect.top) / rect.height) * 100;
                card.style.setProperty("--spot-x", x + "%");
                card.style.setProperty("--spot-y", y + "%");
            });
        });
    }

    function initSmoothAnchors() {
        document.querySelectorAll('a[href^="#"]').forEach(function (a) {
            var id = a.getAttribute("href");
            if (id.length < 2) return;
            a.addEventListener("click", function (e) {
                var el = document.querySelector(id);
                if (!el) return;
                e.preventDefault();
                el.scrollIntoView({ behavior: "smooth", block: "start" });
            });
        });
    }

    function initNavIndicator() {
        var nav = document.querySelector(".pc-nav-pill");
        if (!nav) return;
        var links = nav.querySelectorAll(".nav-link");
        links.forEach(function (link) {
            link.addEventListener("mouseenter", function () {
                links.forEach(function (l) { l.classList.remove("nav-hover"); });
                link.classList.add("nav-hover");
            });
        });
        nav.addEventListener("mouseleave", function () {
            links.forEach(function (l) { l.classList.remove("nav-hover"); });
        });
    }

    document.addEventListener("DOMContentLoaded", function () {
        initSpotlight();
        initSmoothAnchors();
        initNavIndicator();
    });
})();
