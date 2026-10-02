// Header clock, the kind every warehouse floor screen has.
(function () {
    const clock = document.getElementById("shift-clock");
    if (!clock) return;
    const tick = () => {
        clock.textContent = new Date().toLocaleTimeString([], { hour: "2-digit", minute: "2-digit", hour12: false });
    };
    tick();
    setInterval(tick, 15000);
})();
