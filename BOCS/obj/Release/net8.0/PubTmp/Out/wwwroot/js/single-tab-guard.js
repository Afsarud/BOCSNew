//(function () {
//    const KEY = "bocs_single_tab_owner";
//    const LOGOUT_URL = "/Account/Logout?reason=multi-tab";

//    const tabId = (crypto && crypto.randomUUID) ? crypto.randomUUID() :
//        Math.random().toString(36).slice(2);

//    try {
//        const existing = localStorage.getItem(KEY);

//        if (existing && existing !== tabId) {
//            window.location.replace(LOGOUT_URL);
//            return;
//        }

//        localStorage.setItem(KEY, tabId);

//        window.addEventListener("storage", function (e) {
//            if (e.key === KEY && e.newValue && e.newValue !== tabId) {
//                window.location.replace(LOGOUT_URL);
//            }
//        });
//        window.addEventListener("beforeunload", function () {
//            const cur = localStorage.getItem(KEY);
//            if (cur === tabId) localStorage.removeItem(KEY);
//        });
//    } catch (e) {
       
//    }
//})();