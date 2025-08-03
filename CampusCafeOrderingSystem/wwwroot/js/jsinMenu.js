
function updateMinuteTime() {
    const now = new Date();
    const timeDisplay = document.getElementById("time-display");

    if (timeDisplay) {
       
        timeDisplay.textContent = now.toLocaleTimeString('en-US', {
            hour: 'numeric',
            minute: '2-digit',
            hour12: true
        });
    }
}
document.addEventListener("DOMContentLoaded", () => {
    updateMinuteTime();
    setInterval(updateMinuteTime, 60000);
});