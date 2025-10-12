(function () {
    // Full page right click block
    document.addEventListener('contextmenu', function (e) {
        e.preventDefault();
    }, true);

    // Inspect / Save Source / F12 ইত্যাদি block
    document.addEventListener('keydown', function (e) {
        const k = (e.key || '').toLowerCase();
        const ctrl = e.ctrlKey || e.metaKey;
        if (k === 'f12' || (ctrl && (k === 'u' || k === 's')) ||
            (ctrl && e.shiftKey && (k === 'i' || k === 'j'))) {
            e.preventDefault();
            e.stopPropagation();
            return false;
        }
    }, true);

    // Drag বন্ধ
    document.addEventListener('dragstart', function (e) {
        e.preventDefault();
    }, true);

    // Guard div এও right click block
    const guard = document.getElementById("videoGuard");
    if (guard) {
        guard.addEventListener("contextmenu", function (e) {
            e.preventDefault();
        }, true);
    }
})();




//(function () {
//    document.addEventListener('contextmenu', e => { e.preventDefault(); }, { capture: true });
//    document.addEventListener('keydown', function (e) {
//        const k = (e.key || '').toLowerCase();
//        const ctrl = e.ctrlKey || e.metaKey;
//        if (k === 'f12' || (ctrl && (k === 'u' || k === 's')) ||
//            (ctrl && e.shiftKey && (k === 'i' || k === 'j'))) {
//            e.preventDefault(); e.stopPropagation();
//        }
//    }, true);

//    // safety: stop all drags (esp. images)
//    document.addEventListener('dragstart', e => e.preventDefault(), true);
//})();

