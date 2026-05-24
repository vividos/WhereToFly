/* eslint-disable space-before-function-paren */
// Bootstrap & jQuery
import "bootstrap";
import $ from "jquery";
import "bootstrap/dist/css/bootstrap.css";

// Fontawesome
import "@fortawesome/fontawesome-free/js/solid.js";
import "@fortawesome/fontawesome-free/js/fontawesome.js";

// Site
import "../css/site.css";
import LiveTracking from "./liveTracking.js";

// Site initialisation
$(function () {
    initLiveTracking();
});

/**
 * Initializes live tracking
 */
async function initLiveTracking() {
    const options = {
        apiKeys: await getApiKeys(),
    };
    const liveTracking = new LiveTracking(options);

    const liveTrackingInfoList = getLiveTrackingInfoList();
    for (const key in liveTrackingInfoList) {
        const item = liveTrackingInfoList[key];
        if (item.isLiveTrack)
            liveTracking.addLiveTrack(item.name, item.uri, item.isFlightTrack);
        else
            liveTracking.addLiveWaypoint(item.name, item.uri);
    }

    window.liveTracking = liveTracking;
}
