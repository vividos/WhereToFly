// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

/**
 * Creates a new live tracking object
 */
function LiveTracking() {

    console.log("initializing live tracking site");

    this.liveWaypointToIdMapping = [];
    this.updateTimeoutMapping = [];

    this.initPage();

    this.map = new MapView({
        id: 'mapElement',
        initialCenterPoint: { latitude: 47.083, longitude: 12.178 },
        initialZoomLevel: 5,
        hasMouse: true,
        callback: this.callMapAction
    });

    this.map.setShadingMode('Fixed10Am');

    this.addDefaultLocationListsAndTracks();

}

/**
 * Initializes find box and sidebar
 */
LiveTracking.prototype.initPage = function () {

    var that = this;
    $('#findForm').submit(function (event) {

        event.preventDefault();

        that.geocodeAndShow($('#findValue')[0].value);
    });

    $(document).ready(function () {

        $('#sidebarCollapse').on('click', function () {
            $('#sidebar').toggleClass('sidebar-hidden');
        });
    });
};

/**
 * Function for MapView for map actions
 * @param {String} funcName function name of action
 * @param {object} params action params
 */
LiveTracking.prototype.callMapAction = function (funcName, params) {
    console.log('call action: ' + funcName + ', params:' + JSON.stringify(params));
};

/**
 * Adds all default locations and tracks to show in the live tracking map
 */
LiveTracking.prototype.addDefaultLocationListsAndTracks = function () {

    this.map.addLocationList([{
        id: 'crossingthealps2019-start',
        name: 'Start: Kampenwand',
        description: 'Start of the Crossing the Alps 2019 tour',
        type: 'Turnpoint',
        latitude: 47.754076,
        longitude: 12.352277,
        altitude: 0.0,
        isPlanTourLocation: false
    }]);

    this.map.addLocationList([{
        id: 'crossingthealps2019-end',
        name: 'End: Feltre',
        description: 'End of the Crossing the Alps 2019 tour',
        type: 'Turnpoint',
        latitude: 46.017779,
        longitude: 11.900711,
        altitude: 0.0,
        isPlanTourLocation: false
    }]);

    this.map.addTrack({
        id: 'crossingthealps2019',
        name: 'Crossing the Alps 2019',
        isFlightTrack: false,
        color: 'FF0000',
        listOfTrackPoints: [
            12.352277, 47.754076, 0.0, // Kampenwand
            12.431815, 47.631745, 0.0, // Kössen
            12.297016, 47.285720, 0.0, // Wildkogel
            12.183008, 47.090525, 0.0, // Alpenhauptkamm
            11.958434, 46.738669, 0.0, // Kronplatz
            11.828376, 46.508371, 0.0, // Sellastock
            11.870709, 46.251668, 0.0, // Pala
            11.900711, 46.017779, 0.0 // Feltre
        ]
    });
};

/**
 * Adds a single live waypoint to track
 * @param {String} name Name of live waypoint
 * @param {String} liveWaypointId Live waypoint ID
 */
LiveTracking.prototype.addLiveWaypoint = function (name, liveWaypointId) {

    var pageIdPrefix = 'liveWaypoint';
    this.liveWaypointToIdMapping[liveWaypointId] = pageIdPrefix;

    this.map.addLocationList([{
        id: liveWaypointId,
        name: name,
        description: '',
        type: 'LiveWaypoint',
        latitude: 0.0,
        longitude: 0.0,
        altitude: 0.0,
        isPlanTourLocation: false
    }]);

    this.updateLiveWaypoint(liveWaypointId);
};

/**
 * Geocodes entered address and shows pin
 * @param {String} address entered address to find
 */
LiveTracking.prototype.geocodeAndShow = function (address) {

    console.log("geocoding find text: " + address);

    var endpoint = 'https://nominatim.openstreetmap.org/search';
    var resource = new Cesium.Resource({
        url: endpoint,
        queryParameters: {
            format: 'json',
            q: address
        }
    });

    var that = this;
    resource.fetchJson()
        .then(function (results) {
            return results.map(function (resultObject) {
                that.map.showFindResult({
                    name: address,
                    description: resultObject.display_name,
                    latitude: resultObject.lat,
                    longitude: resultObject.lon,
                    displayLatitude: resultObject.lat,
                    displayLongitude: resultObject.lon
                });
            });
        });
};

/**
 * Zooms to a live waypoint by given page ID prefix
 * @param {String} pageIdPrefix page ID prefix of live waypoint
 */
LiveTracking.prototype.zoomToByPrefix = function (pageIdPrefix) {

    var liveWaypointUri =
        Object.keys(this.liveWaypointToIdMapping).find(
            key => this.liveWaypointToIdMapping[key] === pageIdPrefix);

    var entity = this.map.viewer.entities.getById(liveWaypointUri);

    var position = entity.position.getValue(this.map.viewer.clock.currentTime);
    var location = Cesium.Cartographic.fromCartesian(position);

    this.map.zoomToLocation({
        longitude: Cesium.Math.toDegrees(location.longitude),
        latitude: Cesium.Math.toDegrees(location.latitude),
        altitude: location.height
    });
};

/**
 * Updates a live waypoint by given page ID prefix
 * @param {String} pageIdPrefix page ID prefix of live waypoint
 */
LiveTracking.prototype.updateByPrefix = function (pageIdPrefix) {

    var liveWaypointUri =
        Object.keys(this.liveWaypointToIdMapping).find(
            key => this.liveWaypointToIdMapping[key] === pageIdPrefix);

    this.updateLiveWaypoint(liveWaypointUri);
};

/**
 * Zooms to a live waypoint
 * @param {String} liveWaypointUri live waypoint uri to update
 */
LiveTracking.prototype.updateLiveWaypoint = function (liveWaypointUri) {

    console.log('updating live waypoint ' + liveWaypointUri);

    var that = this;
    $.ajax({
        url: '/?handler=UpdateLiveWaypoint',
        data: {
            Uri: liveWaypointUri
        }
    })
        .done(function (result) {
            that.onUpdateLiveWaypointResult(liveWaypointUri, result);
        });
};

/**
 * Called whwn updated live waypoint data is available
 * @param {String} liveWaypointUri live waypoint uri to update
 * @param {Object} result ajax result object with updated data, or a string as error message
 */
LiveTracking.prototype.onUpdateLiveWaypointResult = function (liveWaypointUri, result) {

    console.log('update result: ' + JSON.stringify(result));

    if (result.data !== undefined) {
        result.data.id = liveWaypointUri;
        this.map.updateLocation(result.data);

        if (this.liveWaypointToIdMapping[liveWaypointUri] !== undefined) {
            var pageIdPrefix = this.liveWaypointToIdMapping[liveWaypointUri];

            var idName = '#' + pageIdPrefix + 'Name';
            $(idName)[0].textContent = result.data.name;

            var idDesc = '#' + pageIdPrefix + 'Description';
            $(idDesc)[0].textContent = result.data.description;

            var idLastUpdate = '#' + pageIdPrefix + 'LastUpdate';
            $(idLastUpdate)[0].textContent = 'Last update: ' + new Date().toLocaleTimeString();
        }
    }

    if (typeof result === 'string') {

        if (this.liveWaypointToIdMapping[liveWaypointUri] !== undefined) {
            var idDesc2 = '#' + this.liveWaypointToIdMapping[liveWaypointUri] + 'Description';

            $(idDesc2)[0].textContent = 'Error: ' + result;
        }
    }

    // schedule next update based on reported date
    if (result.nextRequestDate !== undefined)
        this.scheduleNextUpdate(liveWaypointUri, result.nextRequestDate);
};

/**
 * Schedules next update for live waypoint, using next request date from query result
 * @param {String} liveWaypointUri live waypoint uri to use
 * @param {String} nextRequestDate ISO 8601 formatted next request date
 */
LiveTracking.prototype.scheduleNextUpdate = function (liveWaypointUri, nextRequestDate) {

    console.log('scheduling next update for live waypoint ' + liveWaypointUri);

    if (this.updateTimeoutMapping[liveWaypointUri] !== undefined)
        clearTimeout(this.updateTimeoutMapping[liveWaypointUri]);

    var now = new Date();
    var nextRequest = new Date(nextRequestDate);

    var millisTillUpdate = nextRequest - now;
    if (millisTillUpdate < 0)
        millisTillUpdate = 10 * 1000; // schedule in 10 seconds

    console.log("scheduling update in " + millisTillUpdate + " milliseconds");

    var that = this;
    var myTimeout = setTimeout(function () {
        console.log("next update for " + liveWaypointUri + " is due!");
        that.updateLiveWaypoint(liveWaypointUri);
    }, millisTillUpdate);

    this.updateTimeoutMapping[liveWaypointUri] = myTimeout;
};
