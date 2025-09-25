(function () {
    const el = document.getElementById('LESSON_IDS_DATA');
    try {
        window.__LESSON_IDS__ = el ? JSON.parse(el.textContent || "[]") : [];
    } catch {
        window.__LESSON_IDS__ = [];
    }
})();