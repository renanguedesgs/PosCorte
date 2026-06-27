// Auth: máscaras, validação CPF/CNPJ, senha forte, ViaCEP
(function () {
    "use strict";

    function onlyDigits(v) {
        return (v || "").replace(/\D/g, "");
    }

    function formatCpf(d) {
        d = onlyDigits(d).slice(0, 11);
        return d.replace(/(\d{3})(\d)/, "$1.$2")
            .replace(/(\d{3})(\d)/, "$1.$2")
            .replace(/(\d{3})(\d{1,2})$/, "$1-$2");
    }

    function formatCnpj(d) {
        d = onlyDigits(d).slice(0, 14);
        return d.replace(/^(\d{2})(\d)/, "$1.$2")
            .replace(/^(\d{2})\.(\d{3})(\d)/, "$1.$2.$3")
            .replace(/\.(\d{3})(\d)/, ".$1/$2")
            .replace(/(\d{4})(\d)/, "$1-$2");
    }

    function formatDocumento(v) {
        var d = onlyDigits(v);
        return d.length <= 11 ? formatCpf(d) : formatCnpj(d);
    }

    function formatTelefone(v) {
        var d = onlyDigits(v).slice(0, 11);
        if (d.length <= 10) {
            return d.replace(/^(\d{2})(\d)/, "($1) $2")
                .replace(/(\d{4})(\d)/, "$1-$2");
        }
        return d.replace(/^(\d{2})(\d)/, "($1) $2")
            .replace(/(\d{5})(\d)/, "$1-$2");
    }

    function formatCep(v) {
        var d = onlyDigits(v).slice(0, 8);
        return d.length > 5 ? d.replace(/^(\d{5})(\d)/, "$1-$2") : d;
    }

    function calcCpfDigit(cpf, len, peso) {
        var sum = 0;
        for (var i = 0; i < len; i++) sum += parseInt(cpf[i], 10) * (peso - i);
        var mod = sum % 11;
        return mod < 2 ? 0 : 11 - mod;
    }

    function isCpfValid(cpf) {
        cpf = onlyDigits(cpf);
        if (cpf.length !== 11 || /^(\d)\1+$/.test(cpf)) return false;
        if (calcCpfDigit(cpf, 9, 10) !== parseInt(cpf[9], 10)) return false;
        return calcCpfDigit(cpf, 10, 11) === parseInt(cpf[10], 10);
    }

    function isCnpjValid(cnpj) {
        cnpj = onlyDigits(cnpj);
        if (cnpj.length !== 14 || /^(\d)\1+$/.test(cnpj)) return false;
        var m1 = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
        var m2 = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
        var sum = 0;
        for (var i = 0; i < 12; i++) sum += parseInt(cnpj[i], 10) * m1[i];
        var d1 = sum % 11 < 2 ? 0 : 11 - sum % 11;
        if (parseInt(cnpj[12], 10) !== d1) return false;
        sum = 0;
        for (var j = 0; j < 13; j++) sum += parseInt(cnpj[j], 10) * m2[j];
        var d2 = sum % 11 < 2 ? 0 : 11 - sum % 11;
        return parseInt(cnpj[13], 10) === d2;
    }

    function isDocumentoValid(v) {
        var d = onlyDigits(v);
        if (d.length === 11) return isCpfValid(d);
        if (d.length === 14) return isCnpjValid(d);
        return false;
    }

    function isTelefoneValid(v) {
        var d = onlyDigits(v);
        if (d.length !== 10 && d.length !== 11) return false;
        var ddd = parseInt(d.slice(0, 2), 10);
        return ddd >= 11 && ddd <= 99;
    }

    function isCepValid(v) {
        var d = onlyDigits(v);
        return d.length === 8 && d !== "00000000";
    }

    function passwordRules(s) {
        return {
            length: s.length >= 8,
            lower: /[a-z]/.test(s),
            upper: /[A-Z]/.test(s),
            digit: /[0-9]/.test(s),
            special: /[^A-Za-z0-9]/.test(s)
        };
    }

    function passwordScore(s) {
        var r = passwordRules(s);
        var score = 0;
        if (r.length) score++;
        if (r.lower && r.upper) score++;
        if (r.digit) score++;
        if (r.special) score++;
        return score;
    }

    function isPasswordStrong(s) {
        var r = passwordRules(s);
        return r.length && r.lower && r.upper && r.digit && r.special;
    }

    function setFeedback(el, ok, msg) {
        if (!el) return;
        el.textContent = msg || "";
        el.classList.toggle("is-valid", ok && msg);
        el.classList.toggle("is-invalid", !ok && msg);
    }

    function buscarCep(cep) {
        var digits = onlyDigits(cep);
        var status = document.getElementById("cepStatus");
        var endereco = document.getElementById("Endereco");
        if (digits.length !== 8) return;

        if (status) status.textContent = "Buscando CEP...";
        fetch("https://viacep.com.br/ws/" + digits + "/json/")
            .then(function (r) { return r.json(); })
            .then(function (data) {
                if (data.erro) {
                    if (status) status.textContent = "CEP não encontrado.";
                    return;
                }
                if (endereco) {
                    var parts = [data.logradouro, data.bairro, data.localidade + " - " + data.uf];
                    endereco.value = parts.filter(Boolean).join(", ");
                }
                if (status) status.textContent = data.localidade + " / " + data.uf;
            })
            .catch(function () {
                if (status) status.textContent = "";
            });
    }

    function initMasks() {
        document.querySelectorAll("[data-mask]").forEach(function (input) {
            input.addEventListener("input", function () {
                var type = input.getAttribute("data-mask");
                if (type === "documento") input.value = formatDocumento(input.value);
                if (type === "telefone") input.value = formatTelefone(input.value);
                if (type === "cep") input.value = formatCep(input.value);
            });
        });

        var cep = document.getElementById("Cep");
        if (cep) {
            cep.addEventListener("blur", function () { buscarCep(cep.value); });
            cep.addEventListener("input", function () {
                if (onlyDigits(cep.value).length === 8) buscarCep(cep.value);
            });
        }
    }

    function initTogglePassword() {
        document.querySelectorAll(".pc-toggle-password").forEach(function (btn) {
            btn.addEventListener("click", function () {
                var id = btn.getAttribute("data-target");
                var input = document.getElementById(id);
                if (!input) return;
                var show = input.type === "password";
                input.type = show ? "text" : "password";
                var icon = btn.querySelector("i");
                if (icon) {
                    icon.classList.toggle("bi-eye", !show);
                    icon.classList.toggle("bi-eye-slash", show);
                }
            });
        });
    }

    function updatePasswordUi(senha) {
        var strength = document.getElementById("passwordStrength");
        var label = document.getElementById("passwordStrengthLabel");
        var rulesEl = document.getElementById("passwordRules");
        if (!senha) return;

        if (strength) strength.hidden = senha.length === 0;

        var r = passwordRules(senha);
        var score = passwordScore(senha);

        if (rulesEl) {
            rulesEl.querySelectorAll("li").forEach(function (li) {
                var rule = li.getAttribute("data-rule");
                var ok = r[rule];
                li.classList.toggle("ok", ok);
                var icon = li.querySelector("i");
                if (icon) {
                    icon.className = ok ? "bi bi-check-circle-fill" : "bi bi-circle";
                }
            });
        }

        var labels = ["Senha fraca", "Senha razoável", "Senha boa", "Senha forte", "Senha excelente"];
        var colors = ["#dc2626", "#f59e0b", "#eab308", "#16a34a", "#059669"];
        if (label) {
            label.textContent = labels[score] || labels[0];
            label.style.color = colors[score] || colors[0];
        }

        if (strength) {
            strength.querySelectorAll(".pc-password-strength-seg").forEach(function (seg) {
                var n = parseInt(seg.getAttribute("data-seg"), 10);
                seg.classList.toggle("active", n <= score);
                seg.style.background = n <= score ? (colors[score] || colors[0]) : "";
            });
        }
    }

    function validateRegisterForm() {
        var cpf = document.getElementById("CpfCnpj");
        var tel = document.getElementById("Telefone");
        var cep = document.getElementById("Cep");
        var senha = document.getElementById("Senha");
        var conf = document.getElementById("SenhaConfirmacao");
        var submit = document.getElementById("registerSubmit");

        var cpfOk = cpf && isDocumentoValid(cpf.value);
        var telOk = tel && isTelefoneValid(tel.value);
        var cepOk = cep && isCepValid(cep.value);
        var senhaOk = senha && isPasswordStrong(senha.value);
        var confOk = conf && senha && conf.value === senha.value && conf.value.length > 0;

        setFeedback(document.getElementById("cpfCnpjFeedback"),
            cpfOk, cpf && onlyDigits(cpf.value).length > 0 && !cpfOk ? "CPF ou CNPJ inválido" : "");
        setFeedback(document.getElementById("telefoneFeedback"),
            telOk, tel && onlyDigits(tel.value).length > 0 && !telOk ? "Telefone inválido" : "");
        setFeedback(document.getElementById("senhaConfirmFeedback"),
            confOk, conf && conf.value && !confOk ? "As senhas não conferem" : "");

        if (submit) submit.disabled = !cpfOk || !telOk || !cepOk || !senhaOk || !confOk;

        return cpfOk && telOk && cepOk && senhaOk && confOk;
    }

    function initRegister() {
        var form = document.getElementById("registerForm");
        if (!form) return;

        initMasks();
        initTogglePassword();

        var senha = document.getElementById("Senha");
        if (senha) {
            senha.addEventListener("input", function () {
                updatePasswordUi(senha.value);
                validateRegisterForm();
            });
        }

        ["CpfCnpj", "Telefone", "Cep", "SenhaConfirmacao"].forEach(function (id) {
            var el = document.getElementById(id);
            if (el) el.addEventListener("input", validateRegisterForm);
        });

        form.addEventListener("submit", function (e) {
            if (!validateRegisterForm()) e.preventDefault();
        });

        validateRegisterForm();
    }

    function initLogin() {
        initTogglePassword();
    }

    document.addEventListener("DOMContentLoaded", function () {
        if (document.getElementById("registerForm")) initRegister();
        else if (document.getElementById("loginForm")) initLogin();
    });
})();
