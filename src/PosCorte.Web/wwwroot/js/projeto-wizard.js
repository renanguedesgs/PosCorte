// Wizard criar projeto + orçamento ao vivo + ViaCEP + validação por passo
(function () {
    "use strict";

    var current = 1;
    var maxStep = 3;

    function showStep(n) {
        current = n;
        document.querySelectorAll(".pc-wizard-pane").forEach(function (p) {
            p.classList.toggle("active", +p.dataset.pane === n);
        });
        document.querySelectorAll(".pc-wizard-steps .step").forEach(function (s) {
            var sn = +s.dataset.step;
            s.classList.remove("active", "done");
            if (sn < n) s.classList.add("done");
            if (sn === n) s.classList.add("active");
        });
    }

    function setFeedback(id, msg, invalid) {
        var el = document.getElementById(id);
        if (!el) return;
        el.textContent = msg || "";
        el.classList.toggle("is-invalid", invalid && msg);
        el.classList.toggle("text-danger", invalid && msg);
    }

    function getNomeProjeto() {
        var el = document.querySelector("[name='NomeProjeto']");
        return (el && el.value || "").trim();
    }

    function getUrlCorte() {
        var el = document.getElementById("urlCorteCloud");
        return (el && el.value || "").trim();
    }

    function isUrlValid(url) {
        if (!url) return false;
        try {
            var u = new URL(url);
            return u.protocol === "http:" || u.protocol === "https:";
        } catch (e) {
            return false;
        }
    }

    function getQuantidades() {
        var pecas = parseInt(document.getElementById("qtdPecas")?.value, 10) || 0;
        var gavetas = parseInt(document.getElementById("qtdGavetas")?.value, 10) || 0;
        return { pecas: pecas, gavetas: gavetas };
    }

    function validateStep1() {
        var ok = getNomeProjeto().length >= 3;
        setFeedback("feedbackNome", ok ? "" : "Informe o nome do projeto (mín. 3 caracteres).", !ok);
        return ok;
    }

    function validateStep2() {
        var url = getUrlCorte();
        var urlOk = isUrlValid(url);
        setFeedback("feedbackUrl", urlOk ? "" : "Cole o link de compartilhamento do Corte Cloud (https://...).", !urlOk);

        var q = getQuantidades();
        var qOk = q.pecas > 0 || q.gavetas > 0;
        setFeedback("feedbackQtd", qOk ? "" : "Informe ao menos 1 peça ou 1 gaveta do plano de corte.", !qOk);

        var urlEl = document.getElementById("urlCorteCloud");
        if (urlEl) urlEl.classList.toggle("is-invalid", !urlOk);
        var pecasEl = document.getElementById("qtdPecas");
        var gavetasEl = document.getElementById("qtdGavetas");
        if (pecasEl) pecasEl.classList.toggle("is-invalid", !qOk);
        if (gavetasEl) gavetasEl.classList.toggle("is-invalid", !qOk);

        return urlOk && qOk;
    }

    function validateStep3() {
        var cep = (document.getElementById("cepObra")?.value || "").replace(/\D/g, "");
        var cepOk = cep.length === 8;
        setFeedback("feedbackCep", cepOk ? "" : "CEP inválido (8 dígitos).", !cepOk);

        var end = (document.getElementById("enderecoObra")?.value || "").trim();
        var endOk = end.length >= 10;
        setFeedback("feedbackEndereco", endOk ? "" : "Informe o endereço completo da obra.", !endOk);

        return cepOk && endOk;
    }

    function calcular() {
        var q = getQuantidades();
        var custo = q.pecas * 12.5 + q.gavetas * 40;
        var total = custo > 0 ? custo / 0.8 : 0;
        var fmt = function (v) { return v.toLocaleString("pt-BR", { style: "currency", currency: "BRL" }); };
        var elTotal = document.getElementById("valorTotal");
        if (elTotal) elTotal.textContent = fmt(total);
        var elCusto = document.getElementById("custoPrestador");
        if (elCusto) elCusto.textContent = fmt(custo);
        var elMargem = document.getElementById("margemLucro");
        if (elMargem) elMargem.textContent = fmt(total - custo);
    }

    function buscarCep(cep) {
        var digits = (cep || "").replace(/\D/g, "");
        var status = document.getElementById("cepStatus");
        var endereco = document.getElementById("enderecoObra");
        if (digits.length !== 8) return;

        if (status) status.textContent = "Buscando CEP...";
        fetch("https://viacep.com.br/ws/" + digits + "/json/")
            .then(function (r) { return r.json(); })
            .then(function (data) {
                if (data.erro) {
                    if (status) status.textContent = "CEP não encontrado.";
                    return;
                }
                if (endereco && !endereco.value.trim()) {
                    endereco.value = [data.logradouro, data.bairro, data.localidade + " - " + data.uf].filter(Boolean).join(", ");
                }
                if (status) status.textContent = data.localidade + " / " + data.uf;
            })
            .catch(function () {
                if (status) status.textContent = "";
            });
    }

    document.addEventListener("DOMContentLoaded", function () {
        var pecas = document.getElementById("qtdPecas");
        var gavetas = document.getElementById("qtdGavetas");
        var url = document.getElementById("urlCorteCloud");

        if (pecas) {
            pecas.addEventListener("input", function () { calcular(); validateStep2(); });
            pecas.addEventListener("change", calcular);
        }
        if (gavetas) {
            gavetas.addEventListener("input", function () { calcular(); validateStep2(); });
            gavetas.addEventListener("change", calcular);
        }
        if (url) url.addEventListener("input", validateStep2);
        calcular();

        document.querySelectorAll("[data-wizard-next]").forEach(function (btn) {
            btn.addEventListener("click", function () {
                var ok = true;
                if (current === 1) ok = validateStep1();
                if (current === 2) ok = validateStep2();
                if (!ok) return;
                if (current < maxStep) showStep(current + 1);
            });
        });

        document.querySelectorAll("[data-wizard-prev]").forEach(function (btn) {
            btn.addEventListener("click", function () {
                if (current > 1) showStep(current - 1);
            });
        });

        var form = document.getElementById("formProjeto");
        if (form) {
            form.addEventListener("submit", function (e) {
                if (!validateStep1() || !validateStep2() || !validateStep3()) {
                    e.preventDefault();
                    if (!validateStep1()) showStep(1);
                    else if (!validateStep2()) showStep(2);
                    else showStep(3);
                }
            });
        }

        var cep = document.getElementById("cepObra");
        if (cep) {
            cep.addEventListener("blur", function () { buscarCep(cep.value); validateStep3(); });
            cep.addEventListener("input", function () {
                var d = cep.value.replace(/\D/g, "");
                if (d.length === 8) buscarCep(d);
            });
        }

        var endereco = document.getElementById("enderecoObra");
        if (endereco) endereco.addEventListener("input", validateStep3);
    });
})();
