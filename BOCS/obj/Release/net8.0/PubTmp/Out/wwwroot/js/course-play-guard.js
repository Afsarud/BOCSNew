(function () {
    //Admin কে allow করতে চাইলে Razor থেকে true  করতে হবে 
    const elAdmin = document.getElementById('IS_ADMIN_FLAG');
    const isAdmin = elAdmin ? elAdmin.value === 'true' : false;

    const items = document.querySelectorAll('.lesson-item');

    items.forEach(li => {
        li.addEventListener('click', function (e) {
            const canPlay = (li.dataset.canplay === 'true') || isAdmin;
            if (!canPlay) {
                e.preventDefault();
                e.stopImmediatePropagation();
                alert('Class not permitted.');
            }
        }, true);
    });

    const active = document.querySelector('.lesson-item.active');
    if (!active || (active.dataset.canplay !== 'true' && !isAdmin)) {
        const firstPlayable = Array.from(items).find(x => x.dataset.canplay === 'true' || isAdmin);
        if (firstPlayable) firstPlayable.click();
    }
})();