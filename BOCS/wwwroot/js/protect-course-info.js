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