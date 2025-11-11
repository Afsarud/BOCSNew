function readJsonFromScript(id, fallback) {
    const el = document.getElementById(id);
    if (!el) return fallback;
    try {
        return JSON.parse(el.textContent || "");
    } catch {
        return fallback;
    }
}

function toYtId(input) {
    if (!input) return "";
    input = String(input).trim();
    if (/^[A-Za-z0-9_-]{11}$/.test(input)) return input;
    let m = input.match(/[?&]v=([A-Za-z0-9_-]{11})/);
    if (m) return m[1];
    m = input.match(/youtu\.be\/([A-Za-z0-9_-]{11})/);
    if (m) return m[1];
    m = input.match(/embed\/([A-Za-z0-9_-]{11})/);
    if (m) return m[1];
    return "";
}

const INITIAL_ID_RAW = readJsonFromScript("INITIAL_ID", "");
const LIST_FROM_SCRIPT = readJsonFromScript("LESSON_IDS_DATA", []);
const IS_ADMIN = document.getElementById("IS_ADMIN_FLAG")?.value === "true";

const LESSON_IDS =
    Array.isArray(LIST_FROM_SCRIPT) && LIST_FROM_SCRIPT.length
        ? LIST_FROM_SCRIPT
        : Array.from(document.querySelectorAll(".lesson-item")).map(
            (li) => li.getAttribute("data-yt") || ""
        );

let START_ID = toYtId(INITIAL_ID_RAW);
if (!START_ID) {
    const firstPlayable =
        document.querySelector('.lesson-item[data-canplay="true"]') ||
        (IS_ADMIN ? document.querySelector(".lesson-item") : null);
    if (firstPlayable) {
        START_ID = toYtId(firstPlayable.getAttribute("data-yt") || "");
        firstPlayable.classList.add("active");
    }
}
function pad2(n) { return String(Math.floor(n)).padStart(2, '0'); }
function fmtTime(sec) {
    sec = Math.max(0, sec | 0);
    const h = Math.floor(sec / 3600), m = Math.floor((sec % 3600) / 60), s = sec % 60;
    return h > 0 ? `${h}:${pad2(m)}:${pad2(s)}` : `${pad2(m)}:${pad2(s)}`;
}

let player = null;
function onYouTubeIframeAPIReady() {
    const vid = START_ID || "FIP2u36DI1w";
    player = new YT.Player("player", {
        videoId: vid,
        playerVars: { modestbranding: 1, rel: 0, controls: 0, disablekb: 1, iv_load_policy: 3, fs: 0, playsinline: 1 },
        events: {
            onReady: (e) => {
                try { e.target.playVideo(); } catch { }
                // populate playback rates once ready
                try {
                    const rates = e.target.getAvailablePlaybackRates() || [0.5, 1, 1.25, 1.5, 2, 3];
                    const sel = document.getElementById("speedSelect");
                    sel.innerHTML = rates.map(r => `<option value="${r}">${r}x</option>`).join("");
                    sel.value = (e.target.getPlaybackRate?.() || 1).toString();
                } catch { }
            },
        },
    });
}
window.onYouTubeIframeAPIReady = onYouTubeIframeAPIReady;
// One DOMContentLoaded block to wire everything safely
document.addEventListener("DOMContentLoaded", () => {
    const seekBar = document.getElementById("seekBar");
    const timeDisplay = document.getElementById("timeDisplay");
    const minuteDisplay = document.getElementById("minuteDisplay");

    let wasPlaying = false;
    setInterval(() => {
        if (!player || typeof player.getDuration !== "function") return;
        const duration = player.getDuration() || 0;
        const current = player.getCurrentTime() || 0;
        if (duration > 0) {
            seekBar.max = duration;
            if (document.activeElement !== seekBar) {
                seekBar.value = current;
            }
        }
        if (timeDisplay) timeDisplay.textContent = `${fmtTime(current)} / ${fmtTime(duration)}`;
        if (minuteDisplay) minuteDisplay.textContent =
            `${(current / 60).toFixed(1)}m / ${(duration / 60).toFixed(1)}m`;
    }, 500);
    seekBar.addEventListener("mousedown", () => {
        if (player && player.getPlayerState() === YT.PlayerState.PLAYING) {
            wasPlaying = true;
            player.pauseVideo();
        }
    });
    seekBar.addEventListener("mouseup", () => {
        if (wasPlaying) {
            player.playVideo();
            wasPlaying = false;
        }
    });
    seekBar.addEventListener("input", () => {
        if (player && typeof player.seekTo === "function") {
            const newTime = parseFloat(seekBar.value);
            player.seekTo(newTime, true);
        }
    });

    // ===== Playback rate controls =====
    const speedSelect = document.getElementById("speedSelect");
    document.getElementById("speedDown")?.addEventListener("click", () => {
        if (!player) return;
        const rates = player.getAvailablePlaybackRates?.() || [0.5, 1, 1.25, 1.5, 2, 3];
        const cur = player.getPlaybackRate?.() || 1;
        const idx = Math.max(0, rates.findIndex(r => r === cur) - 1);
        player.setPlaybackRate?.(rates[idx]);
        if (speedSelect) speedSelect.value = rates[idx].toString();
    });

    document.getElementById("speedUp")?.addEventListener("click", () => {
        if (!player) return;
        const rates = player.getAvailablePlaybackRates?.() || [0.5, 1, 1.25, 1.5, 2, 3];
        const cur = player.getPlaybackRate?.() || 1;
        const idx = Math.min(rates.length - 1, rates.findIndex(r => r === cur) + 1);
        player.setPlaybackRate?.(rates[idx]);
        if (speedSelect) speedSelect.value = rates[idx].toString();
    });

    speedSelect?.addEventListener("change", (e) => {
        const v = parseFloat(e.target.value);
        if (player && !Number.isNaN(v)) player.setPlaybackRate?.(v);
    });
});
function idByIndex(idx) {
    const i = parseInt(idx, 10);
    if (Number.isNaN(i) || i < 0 || i >= LESSON_IDS.length) return "";
    return LESSON_IDS[i] || "";
}

// 📌 LESSON CLICK HANDLER (MAIN PART)
document.addEventListener("click", function (e) {
    const li = e.target.closest(".lesson-item");
    if (!li) return;
    const allowed = li.dataset.canplay === "true" || IS_ADMIN;
    if (!allowed) {
        alert("Class not permitted.");
        return;
    }

    const direct = li.getAttribute("data-yt");
    const byIdx = idByIndex(li.getAttribute("data-idx"));
    const ytId = toYtId(direct || byIdx);
    if (!ytId) return;

    document
        .querySelectorAll(".lesson-item.active")
        .forEach((x) => x.classList.remove("active"));
    li.classList.add("active");
    const subject = li.getAttribute("data-subject") || "";
    const lesson = li.getAttribute("data-lesson") || "";
    const titleEl = document.getElementById("lessonTitle");
    if (titleEl)
        titleEl.textContent = subject && lesson ? `${subject} | ${lesson}` : "";
    const lessonId = li.getAttribute("data-lesson-id");
    if (lessonId) {
        updateAttachments(lessonId);
    }
    // ✅ Load and autoplay the selected video
    if (player && typeof player.loadVideoById === "function") {
        player.loadVideoById(ytId);
        try {
            player.playVideo();
        } catch { }
    } else {
        START_ID = ytId;
    }

    const videoShell = document.getElementById("videoShell");
    if (videoShell) {
        const navbar = document.querySelector("nav.navbar");
        const navbarHeight = navbar ? navbar.offsetHeight : 0;
        const y =
            videoShell.getBoundingClientRect().top +
            window.pageYOffset -
            (navbarHeight + 80); 
        window.scrollTo({ top: y, behavior: "smooth" });
        setTimeout(() => {
            try {
                player?.playVideo();
            } catch (err) {
                console.error("Autoplay failed:", err);
            }
        }, 400);
    }
});

document.addEventListener("keydown", function (e) {
    if (
        (e.key === "Enter" || e.key === " ") &&
        document.activeElement?.classList.contains("lesson-item")
    ) {
        e.preventDefault();
        document.activeElement.click();
    }
});

document.addEventListener("DOMContentLoaded", () => {
  const activeLi = document.querySelector(".lesson-item.active");
  if (activeLi) {
    const subject = activeLi.getAttribute("data-subject") || "";
    const lesson = activeLi.getAttribute("data-lesson") || "";
    const titleEl = document.getElementById("lessonTitle");
    if (titleEl)
      titleEl.textContent = subject && lesson ? `${subject} | ${lesson}` : "";
  }

    const video = document.getElementById('player');
    const seekBar = document.getElementById('seekBar');

  // ==== Custom Controls ====
  document
    .getElementById("customPlay")
    ?.addEventListener("click", () => player?.playVideo());
  document
    .getElementById("customPause")
    ?.addEventListener("click", () => player?.pauseVideo());

  document.getElementById("volumeUp")?.addEventListener("click", () => {
    if (player) {
      let v = player.getVolume();
      player.setVolume(Math.min(v + 10, 100));
    }
  });
  document.getElementById("volumeDown")?.addEventListener("click", () => {
    if (player) {
      let v = player.getVolume();
      player.setVolume(Math.max(v - 10, 0));
    }
  });
  document.getElementById("muteToggle")?.addEventListener("click", () => {
    if (!player) return;
    if (player.isMuted()) player.unMute();
    else player.mute();
  });

  document.getElementById("seekForward")?.addEventListener("click", () => {
    if (player) {
      let t = player.getCurrentTime();
      player.seekTo(t + 10, true);
    }
  });
  document.getElementById("seekBackward")?.addEventListener("click", () => {
    if (player) {
      let t = player.getCurrentTime();
      player.seekTo(Math.max(t - 10, 0), true);
    }
  });

  const initialActive = document.querySelector(".lesson-item.active");
  if (initialActive) {
    const lessonId = initialActive.getAttribute("data-lesson-id");
    if (lessonId) {
      updateAttachments(lessonId);
    }
  }

});

// 🎬 YouTube-style Seek Bar Integration
document.addEventListener("DOMContentLoaded", () => {
    const seekBar = document.getElementById("seekBar");
    setInterval(() => {
        if (player && typeof player.getDuration === "function") {
            const duration = player.getDuration();
            const current = player.getCurrentTime();

            if (duration > 0) {
                seekBar.max = duration;
                seekBar.value = current;
            }
        }
    }, 500); // প্রতি 0.5 সেকেন্ডে আপডেট
    seekBar.addEventListener("input", () => {
        if (player && typeof player.seekTo === "function") {
            const newTime = parseFloat(seekBar.value);
            player.seekTo(newTime, true);
        }
    });
});

let wasPlaying = false;

seekBar.addEventListener("mousedown", () => {
    if (player && player.getPlayerState() === YT.PlayerState.PLAYING) {
        wasPlaying = true;
        player.pauseVideo();
    }
});

seekBar.addEventListener("mouseup", () => {
    if (wasPlaying) {
        player.playVideo();
        wasPlaying = false;
    }
});
function updateAttachments(lessonId) {
  const container = document.getElementById("attachments-container");
  if (!container) return;
  container.innerHTML =
    '<h5 class="mb-3">Course Materials</h5><p class="text-muted">Loading materials...</p>';

  fetch(`/Courses/GetLessonAttachments?lessonId=${lessonId}`)
    .then((response) => response.json())
    .then((data) => {
      let html = '<h5 class="mb-3">Course Materials</h5>';

      const images = data.images || [];
      const documents = data.documents || [];

      if (images.length > 0) {
        html += `<div class="mb-4">
                    <h6 class="text-muted mb-3">Images (${images.length})</h6>
                    <div class="row g-2">`;
        images.forEach((image) => {
          html += `<div class="col-md-2 col-sm-3 col-4">
                        <div class="card h-100 image-card" style="cursor: pointer;" data-image-src="${image.filePath}" data-image-name="${image.fileName}">
                            <div class="card-img-container" style="height: 80px; overflow: hidden;">
                                <img src="${image.filePath}" class="card-img-top w-100 h-100" style="object-fit: cover;" alt="${image.fileName}">
                            </div>
                            <div class="card-body p-2 d-flex flex-column" style="min-height: 60px;">
                                <div class="flex-grow-1">
                                    <small class="text-muted d-block" style="font-size: 0.7rem; line-height: 1.1; word-break: break-word;">${image.fileName}</small>
                                    <small class="text-muted" style="font-size: 0.6rem;">${image.fileSize}</small>
                                </div>
                                <div class="mt-1">
                                    <button type="button" class="btn btn-sm btn-outline-success download-image-btn w-100" style="padding: 0.1rem 0.3rem; font-size: 0.7rem;" data-image-src="${image.filePath}" data-image-name="${image.fileName}">
                                        <i class="bi bi-download"></i> Download
                                    </button>
                                </div>
                            </div>
                        </div>
                    </div>`;
        });
        html += "</div></div>";
      }

      if (documents.length > 0) {
        html += `<div class="mb-4">
                    <h6 class="text-muted mb-3">Documents (${documents.length})</h6>
                    <div class="list-group">`;
        documents.forEach((doc) => {
          html += `<div class="list-group-item d-flex justify-content-between align-items-center document-item" data-doc-src="${doc.filePath}" data-doc-name="${doc.fileName}">
                        <div>
                            <strong>${doc.fileName}</strong>
                            <br>
                            <small class="text-muted">${doc.fileSize}</small>
                        </div>
                        <div>
                            <button type="button" class="btn btn-sm btn-outline-primary view-document-btn" data-doc-src="${doc.filePath}">
                                <i class="bi bi-eye"></i> View
                            </button>
                        </div>
                    </div>`;
        });
        html += "</div></div>";
      }

      if (images.length === 0 && documents.length === 0) {
        html +=
          '<p class="text-muted">No materials available for this lesson.</p>';
      }

      container.innerHTML = html;
      if (typeof initializeImageViewer === "function") initializeImageViewer();
      if (typeof initializeDocumentViewer === "function")
        initializeDocumentViewer();
      if (typeof initializeImageDownload === "function")
        initializeImageDownload();
    })
    .catch((error) => {
      console.error("Error loading attachments:", error);
      container.innerHTML =
        '<h5 class="mb-3">Course Materials</h5><p class="text-danger">Error loading materials.</p>';
    });
}
