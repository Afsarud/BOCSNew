(function () {
    const container = document.getElementById('videoTop');

    function readJsonFromScript(id, fallback) {
        const el = document.getElementById(id);
        if (!el) return fallback;
        try { return JSON.parse(el.textContent || ''); }
        catch { return fallback; }
    }

    // Razor থেকে আসা ডেটা
    const initialId = readJsonFromScript('INITIAL_ID', '');
    const listFromScript = readJsonFromScript('LESSON_IDS_DATA', []);

    // ফ্যালব্যাক: DOM থেকে লিস্ট বানাও
    const LESSON_IDS = (Array.isArray(listFromScript) && listFromScript.length)
        ? listFromScript
        : Array.from(document.querySelectorAll('.lesson-item'))
            .map(li => li.getAttribute('data-yt') || '');

    // admin flag (ভিউতে hidden input আছে)
    const isAdmin = (document.getElementById('IS_ADMIN_FLAG')?.value === 'true');

    function toYtId(input) {
        if (!input) return "";
        input = String(input).trim();
        if (/^[A-Za-z0-9_-]{11}$/.test(input)) return input;
        let m = input.match(/[?&]v=([A-Za-z0-9_-]{11})/); if (m) return m[1];
        m = input.match(/youtu\.be\/([A-Za-z0-9_-]{11})/); if (m) return m[1];
        m = input.match(/embed\/([A-Za-z0-9_-]{11})/); if (m) return m[1];
        return "";
    }

    function idByIndex(idx) {
        const i = parseInt(idx, 10);
        if (Number.isNaN(i) || i < 0 || i >= LESSON_IDS.length) return "";
        return LESSON_IDS[i] || "";
    }

    function renderPlayer(id) {
        const yt = toYtId(id);
        if (!yt || !container) return;

        const params = new URLSearchParams({
            rel: '0', controls: '1', modestbranding: '1',
            fs: '0', disablekb: '0', iv_load_policy: '3', cc_load_policy: '0',
            playsinline: '1', autoplay: '1', mute: '1',
            origin: location.origin, enablejsapi: '1'
        });

        container.innerHTML =
            `<iframe id="ytPlayer"
        src="https://www.youtube-nocookie.com/embed/${yt}?${params.toString()}&_=${Date.now()}"
        title="Class player" frameborder="0"
        allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share"
        sandbox="allow-scripts allow-same-origin allow-presentation"
        allowfullscreen></iframe>`;
    }

    // --- Click delegation (accordion-এর ভেতরেও কাজ করবে) ---
    document.addEventListener('click', function (e) {
        const li = e.target.closest('.lesson-item');
        if (!li) return;

        // IsPlay চেক — admin হলে bypass
        const allowed = (li.dataset.canplay === 'true') || isAdmin;
        if (!allowed) {
            // guard.js না থাকলেও এখানে ব্লক থাকবে
            alert('Class not permitted.');
            return;
        }

        const direct = li.getAttribute('data-yt');
        const byIdx = idByIndex(li.getAttribute('data-idx'));
        const id = direct || byIdx;
        if (!id) return;

        document.querySelectorAll('.lesson-item.active').forEach(x => x.classList.remove('active'));
        li.classList.add('active');

        renderPlayer(id);
    });

    // কিবোর্ড সাপোর্ট
    document.addEventListener('keydown', function (e) {
        if ((e.key === 'Enter' || e.key === ' ') && document.activeElement?.classList.contains('lesson-item')) {
            e.preventDefault();
            document.activeElement.click();
        }
    });

    // --- Auto load ---
    let startId = initialId;
    if (!startId) {
        // প্রথম play-able আইটেম নাও (অথবা admin হলে প্রথম যেটা)
        const firstPlayable = document.querySelector('.lesson-item[data-canplay="true"]')
            || (isAdmin ? document.querySelector('.lesson-item') : null);

        if (firstPlayable) {
            startId = firstPlayable.getAttribute('data-yt') || idByIndex(firstPlayable.getAttribute('data-idx'));
            firstPlayable.classList.add('active');
        }
    }
    if (startId) renderPlayer(startId);
})();