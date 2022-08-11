// Bootstrap & jQuery
import 'bootstrap';
import $ from 'jquery';
import 'bootstrap/dist/css/bootstrap.css';

// Fontawesome
import '@fortawesome/fontawesome-free/js/solid.js';
import '@fortawesome/fontawesome-free/js/fontawesome.js';

// Site
import '../css/site.css';
import LiveTracking from './liveTracking.js';

// Site initialisation
$(function () {
    initLiveTracking();
});

function initLiveTracking() {
    var liveTracking = new LiveTracking();

    var liveTrackingInfoList = getLiveTrackingInfoList();
    for (var key in liveTrackingInfoList) {
        var item = liveTrackingInfoList[key];
        if (item.isLiveTrack)
            liveTracking.addLiveTrack(item.name, item.uri, item.isFlightTrack);
        else
            liveTracking.addLiveWaypoint(item.name, item.uri);
    }

    window.liveTracking = liveTracking;
}
