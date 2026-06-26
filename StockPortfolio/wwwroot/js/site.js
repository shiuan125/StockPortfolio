// ── Dark / Light mode toggle ──────────────────────────────────────────────
(function () {
    var checkbox = document.getElementById('theme-checkbox');

    function applyTheme(light) {
        if (light) {
            document.documentElement.classList.add('light');
        } else {
            document.documentElement.classList.remove('light');
        }
        if (checkbox) checkbox.checked = light;
        localStorage.setItem('theme', light ? 'light' : 'dark');
        document.dispatchEvent(new CustomEvent('themechange', { detail: { light: light } }));
    }

    // 同步 checkbox 與目前狀態（prevent-flash script 已在 <head> 設定 class）
    if (checkbox) {
        checkbox.checked = document.documentElement.classList.contains('light');
        checkbox.addEventListener('change', function () {
            applyTheme(this.checked);
        });
    }
})();

document.addEventListener('DOMContentLoaded', function () {

    // data-confirm on <form>: 送出前顯示確認對話框
    document.addEventListener('submit', function (e) {
        var msg = e.target.dataset.confirm;
        if (msg && !window.confirm(msg)) {
            e.preventDefault();
        }
    });

    // js-no-bubble: 阻止 click 事件冒泡（用於可收合列內的按鈕）
    document.querySelectorAll('.js-no-bubble').forEach(function (el) {
        el.addEventListener('click', function (e) {
            e.stopPropagation();
        });
    });

    // data-dismiss-alert: 關閉警示框
    document.addEventListener('click', function (e) {
        var btn = e.target.closest('[data-dismiss-alert]');
        if (btn) {
            var alert = btn.closest('[role="alert"]');
            if (alert) alert.remove();
        }
    });

    // data-collapse-target: 收合/展開 <tbody>（取代 Bootstrap collapse）
    document.querySelectorAll('[data-collapse-target]').forEach(function (trigger) {
        trigger.addEventListener('click', function () {
            var targetId = this.dataset.collapseTarget;
            var target = document.getElementById(targetId);
            if (!target) return;
            var isExpanded = this.getAttribute('aria-expanded') === 'true';
            this.setAttribute('aria-expanded', isExpanded ? 'false' : 'true');
            target.hidden = isExpanded;
        });
    });

});
