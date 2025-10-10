(function () {
    // ========= CONFIG =========
    const AUTO_SAVE = true; // অটো-সেভ চাইলে true, না হলে false

    const tbody = document.getElementById('lessonTbody');
    if (!tbody) return;

    const saveBtn = document.getElementById('saveOrderBtn');
    const tokenEl = document.querySelector('#reorderForm input[name="__RequestVerificationToken"]');
    const token = tokenEl ? tokenEl.value : '';
    const courseId = (document.getElementById('courseId') || {}).value || '';

    // আপনার controller class এ [Route("admin/course-lessons")] আছে ধরে রিলেটিভ পাথ:
    const reorderPath = '/admin/course-lessons/reorder';

    let dragEl = null;
    let dirty = false;
    let saveTimer = null;

    // কেবল হ্যান্ডেল থেকে ড্র্যাগ শুরু
    tbody.addEventListener('mousedown', (e) => {
        const handle = e.target.closest('.drag-handle');
        if (!handle) return;
        const row = handle.closest('tr.dr-row');
        if (row) row.setAttribute('draggable', 'true');
    });

    // ড্র্যাগ শুরু
    tbody.addEventListener('dragstart', (e) => {
        const row = e.target.closest('tr.dr-row');
        if (!row) return;
        dragEl = row;
        // Firefox/Edge সাপোর্টের জন্য
        e.dataTransfer.setData('text/plain', row.dataset.id);
        e.dataTransfer.effectAllowed = 'move';
        row.classList.add('dragging');
    });

    // ড্র্যাগ চলাকালীন অবস্থান পরিবর্তন
    tbody.addEventListener('dragover', (e) => {
        if (!dragEl) return;
        e.preventDefault();

        const afterEl = getDragAfterElement(tbody, e.clientY);
        if (afterEl == null) {
            tbody.appendChild(dragEl);
        } else if (afterEl !== dragEl) {
            tbody.insertBefore(dragEl, afterEl);
        }

        [...tbody.querySelectorAll('tr.dr-row')].forEach(r => r.classList.remove('drop-target'));
        if (afterEl) afterEl.classList.add('drop-target');

        markChanged();
    });

    // ড্র্যাগ শেষ
    tbody.addEventListener('dragend', () => {
        if (dragEl) {
            dragEl.classList.remove('dragging');
            dragEl.removeAttribute('draggable');
        }
        dragEl = null;
        [...tbody.querySelectorAll('tr.drop-target')].forEach(r => r.classList.remove('drop-target'));

        // ডিবাউন্সড অটো-সেভ
        if (dirty && AUTO_SAVE) {
            if (saveTimer) clearTimeout(saveTimer);
            saveTimer = setTimeout(saveOrder, 400);
        }
    });

    function getDragAfterElement(container, y) {
        const els = [...container.querySelectorAll('tr.dr-row:not(.dragging)')];
        let closest = { offset: Number.NEGATIVE_INFINITY, element: null };
        for (const child of els) {
            const box = child.getBoundingClientRect();
            const offset = y - box.top - box.height / 2;
            if (offset < 0 && offset > closest.offset) {
                closest = { offset, element: child };
            }
        }
        return closest.element;
    }

    function markChanged() {
        // 0-based (i) বা 1-based (i+1) যেটা চাই
        [...tbody.querySelectorAll('tr.dr-row')].forEach((tr, i) => {
            const cell = tr.querySelector('.order-cell');
            if (cell) cell.textContent = i;
        });
        dirty = true;
        if (!AUTO_SAVE) saveBtn && saveBtn.classList.remove('d-none');
    }

    // ম্যানুয়াল সেভ বাটন
    saveBtn && saveBtn.addEventListener('click', async () => { await saveOrder(); });

    async function saveOrder() {
        const ids = [...tbody.querySelectorAll('tr.dr-row')].map(tr => parseInt(tr.dataset.id));

        if (!AUTO_SAVE && saveBtn) {
            saveBtn.disabled = true;
            saveBtn.textContent = 'Saving...';
        }

        try {
            const res = await fetch(`${reorderPath}?courseId=${courseId}`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': token
                },
                body: JSON.stringify({ ids })
            });
            if (!res.ok) throw new Error('Failed to save order');

            dirty = false;
            showToast();
            if (!AUTO_SAVE && saveBtn) saveBtn.classList.add('d-none');
        } catch (err) {
            alert('❌ Could not save order. ' + err.message);
        } finally {
            if (!AUTO_SAVE && saveBtn) {
                saveBtn.disabled = false;
                saveBtn.textContent = 'Save order';
            }
        }
    }

    // Toast helpers
    window.showToast = function () {
        const t = document.getElementById('saveToast');
        if (!t) return;
        t.classList.remove('d-none');
        setTimeout(window.hideToast, 1800);
    };
    window.hideToast = function () {
        const t = document.getElementById('saveToast');
        if (!t) return;
        t.classList.add('d-none');
    };

    //check/UnCheck
    // ---- check / uncheck (reuse existing vars) ----
    const chkAll = document.getElementById('chkAll');
    if (chkAll && tbody) {
        // tick API route
        const tickPath = '/admin/course-lessons/tick';

        async function saveTick(ids, value) {
            const res = await fetch(`${tickPath}?courseId=${encodeURIComponent(courseId)}`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': token
                },
                body: JSON.stringify({ ids, value })
            });
            if (!res.ok) {
                const t = await res.text();
                throw new Error(t || 'Failed to save tick');
            }
            return res.json();
        }

        // master checkbox → all on/off
        chkAll.addEventListener('change', async () => {
            const boxes = [...tbody.querySelectorAll('input.chk-one')];
            const ids = boxes.map(b => parseInt(b.closest('tr').dataset.id, 10));
            const val = chkAll.checked;

            // UI first
            boxes.forEach(b => { b.checked = val; });

            try {
                await saveTick(ids, val);
                if (typeof showToast === 'function') showToast();
            } catch (e) {
                alert('❌ Could not save: ' + e.message);
                boxes.forEach(b => { b.checked = !val; });
                chkAll.checked = !val;
            }
        });

        // per-row checkbox
        tbody.addEventListener('change', async (e) => {
            const box = e.target.closest('input.chk-one');
            if (!box) return;

            const tr = box.closest('tr');
            const id = parseInt(tr.dataset.id, 10);
            const val = box.checked;

            // sync master state
            const all = [...tbody.querySelectorAll('input.chk-one')];
            chkAll.checked = all.every(b => b.checked);

            try {
                await saveTick([id], val);
            } catch (err) {
                alert('❌ Could not save: ' + err.message);
                box.checked = !val;
                chkAll.checked = all.every(b => b.checked);
            }
        });
    }
})();