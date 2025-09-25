(function () {
    // Right-click/context menu বন্ধ
    document.addEventListener('contextmenu', function (e) {
        e.preventDefault();
    }, { capture: true });

    // Inspect/Save/Source শর্টকাট ব্লক
    document.addEventListener('keydown', function (e) {
        const k = (e.key || '').toLowerCase();
        const ctrl = e.ctrlKey || e.metaKey;
        if (k === 'f12' || (ctrl && (k === 'u' || k === 's')) ||
            (ctrl && e.shiftKey && (k === 'i' || k === 'j'))) {
            e.preventDefault(); e.stopPropagation();
        }
    }, true);

    // ইমেজ drag বন্ধ (অতিরিক্ত সেফটি)
    document.querySelectorAll('img').forEach(el => {
        el.setAttribute('draggable', 'false');
        el.addEventListener('dragstart', ev => ev.preventDefault());
    });
})();