// Bootstrap & jQuery
import 'bootstrap';
import $ from 'jquery';
import 'bootstrap/dist/css/bootstrap.css';

// Custom scrollbar plugin
import 'malihu-custom-scrollbar-plugin';
import 'malihu-custom-scrollbar-plugin/jquery.mCustomScrollbar.css';

// Fontawesome
import '@fortawesome/fontawesome-free/js/solid.js';
import '@fortawesome/fontawesome-free/js/fontawesome.js';

// Cesium.js
import "cesium/Build/Cesium/Widgets/widgets.css";
import * as Cesium from 'cesium';

// Site
import '../css/site.css';
import '../css/mapView3D.css';

// make jQuery visible outside ES6 modules
window.$ = $;
// also for Cesium.js
window.Cesium = Cesium;

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
            liveTracking.addLiveTrack(item.name, item.uri);
        else
            liveTracking.addLiveWaypoint(item.name, item.uri);
    }

    window.liveTracking = liveTracking;
}
