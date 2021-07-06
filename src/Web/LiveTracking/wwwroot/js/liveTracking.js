// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

/**
 * Creates a new live tracking object
 */
function LiveTracking() {

    console.log("LiveTracking: initializing live tracking site");

    this.liveWaypointToIdMapping = {};
    this.liveTrackToIdMapping = {};
    this.updateTimeoutMapping = {};

    this.initPage();

    this.map = new MapView({
        id: 'mapElement',
        initialCenterPoint: { latitude: 47.083, longitude: 12.178 },
        initialViewingDistance: 5000.0,
        hasMouse: true,
        useAsynchronousPrimitives: true,
        useEntityClustering: false,
        callback: this.callMapAction
    });

    this.map.setShadingMode('CurrentTime');

    this.addDefaultLayerAndLocationListsAndTracks();

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

        $("#sidebar").mCustomScrollbar({
            theme: "minimal"
        });

        $('#dismiss, .overlay').on('click', function () {
            // hide sidebar
            $('#sidebar').removeClass('active');
            // hide overlay
            $('.overlay').removeClass('active');
        });

        $('#sidebarCollapse').on('click', function () {
            // open sidebar
            $('#sidebar').addClass('active');
            // fade in the overlay
            $('.overlay').addClass('active');
            $('.collapse.in').toggleClass('in');
            $('a[aria-expanded=true]').attr('aria-expanded', 'false');
        });
    });
};

/**
 * Function for MapView for map actions
 * @param {String} funcName function name of action
 * @param {object} params action params
 */
LiveTracking.prototype.callMapAction = function (funcName, params) {
    console.log("LiveTracking: call action: " + funcName + ", params: " + JSON.stringify(params));
};

/**
 * Adds all default layers, locations and tracks to show in the live tracking map
 */
LiveTracking.prototype.addDefaultLayerAndLocationListsAndTracks = function () {

    console.log("LiveTracking: loading default data...");

    this.addCrossingTheAlpsLayer();
};

/**
 * Adds "X-Lakes" layer
 */
LiveTracking.prototype.addXLakesLayer = function () {

    var that = this;
    $.get("/data/x-lakes-2020-wainwrights.czml",
        null,
        function (data) {
            console.log("LiveTracking: successfully loaded czml file, adding layer");
            that.map.addLayer({
                id: 'x-lakes-2020-layer',
                name: 'X-Lakes 2020 Wainwrights',
                type: '',
                isVisible: true,
                data: data
            });
            that.map.zoomToLayer('x-lakes-2020-layer');
        },
        "text");
};

/**
 * Adds "crossing the alps" layer
 */
LiveTracking.prototype.addCrossingTheAlpsLayer = function () {

    var that = this;
    $.get("/data/crossing-the-alps-2021.czml",
        null,
        function (data) {
            console.log("LiveTracking: successfully loaded czml file, adding layer");
            that.map.addLayer({
                id: 'crossng-the-alps-2021-layer',
                name: 'Crossing the Alps 2021',
                type: '',
                isVisible: true,
                data: data
            });
            that.map.zoomToLayer('rossng-the-alps-2021-layer');
        },
        "text");
};

/**
 * Adds a single live waypoint to track
 * @param {String} name Name of live waypoint
 * @param {String} liveWaypointId Live waypoint ID
 */
LiveTracking.prototype.addLiveWaypoint = function (name, liveWaypointId) {

    var pageIdPrefix = 'liveData';
    this.liveWaypointToIdMapping[liveWaypointId] = pageIdPrefix;

    var idName = '#' + pageIdPrefix + 'Name';
    $(idName)[0].textContent = name;

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
 * Adds a single live track to map
 * @param {String} name Name of live track
 * @param {String} liveTrackId Live track ID
 */
LiveTracking.prototype.addLiveTrack = function (name, liveTrackId) {

    var pageIdPrefix = 'liveData';
    this.liveTrackToIdMapping[liveTrackId] = pageIdPrefix;

    var idName = '#' + pageIdPrefix + 'Name';
    $(idName)[0].textContent = name;

    this.map.addTrack({
        id: liveTrackId,
        name: name,
        description: '',
        isFlightTrack: true,
        isLiveTrack: true,
        listOfTrackPoints: [],
        listOfTimePoints: [],
        groundHeightProfile: [],
        color: "ff8000"
    });

    this.updateLiveTrack(liveTrackId);
};

/**
 * Geocodes entered address and shows pin
 * @param {String} address entered address to find
 */
LiveTracking.prototype.geocodeAndShow = function (address) {

    console.log("LiveTracking: geocoding find text: " + address);

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

    if (liveWaypointUri !== undefined) {

        var entity = this.map.viewer.entities.getById(liveWaypointUri);

        if (entity === undefined) {
            console.error("LiveTracking: couldn't find entity for live waypoint id: " + liveWaypointUri);
            return;
        }

        var position = entity.position.getValue(this.map.viewer.clock.currentTime);
        var location = Cesium.Cartographic.fromCartesian(position);

        this.map.zoomToLocation({
            longitude: Cesium.Math.toDegrees(location.longitude),
            latitude: Cesium.Math.toDegrees(location.latitude),
            altitude: location.height
        });
    }

    var liveTrackUri =
        Object.keys(this.liveTrackToIdMapping).find(
            key => this.liveTrackToIdMapping[key] === pageIdPrefix);

    if (liveTrackUri !== undefined)
        this.map.zoomToTrack(liveTrackUri);
};

/**
 * Updates a live waypoint or track by given page ID prefix
 * @param {String} pageIdPrefix page ID prefix of live waypoint
 */
LiveTracking.prototype.updateByPrefix = function (pageIdPrefix) {

    var liveWaypointUri =
        Object.keys(this.liveWaypointToIdMapping).find(
            key => this.liveWaypointToIdMapping[key] === pageIdPrefix);

    if (liveWaypointUri !== undefined)
        this.updateLiveWaypoint(liveWaypointUri);

    var liveTrackUri =
        Object.keys(this.liveTrackToIdMapping).find(
            key => this.liveTrackToIdMapping[key] === pageIdPrefix);

    if (liveTrackUri !== undefined)
        this.updateLiveTrack(liveTrackUri);
};

/**
 * Updates a live waypoint
 * @param {String} liveWaypointUri live waypoint uri to update
 */
LiveTracking.prototype.updateLiveWaypoint = function (liveWaypointUri) {

    console.log("LiveTracking: updating live waypoint " + liveWaypointUri);

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
 * Called when updated live waypoint data is available
 * @param {String} liveWaypointUri live waypoint uri to update
 * @param {Object} result ajax result object with updated data, or a string as error message
 */
LiveTracking.prototype.onUpdateLiveWaypointResult = function (liveWaypointUri, result) {

    console.log("LiveTracking: update result: " + JSON.stringify(result));

    if (result.data !== undefined) {
        result.data.id = liveWaypointUri;
        result.data.type = 'LiveWaypoint';

        this.map.updateLocation(result.data);

        if (this.liveWaypointToIdMapping[liveWaypointUri] !== undefined) {
            var pageIdPrefix = this.liveWaypointToIdMapping[liveWaypointUri];

            //var idName = '#' + pageIdPrefix + 'Name';
            //$(idName)[0].textContent = result.data.name;

            var idDesc = '#' + pageIdPrefix + 'Description';
            $(idDesc)[0].innerHTML = result.data.description;

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
 * Updates a live track
 * @param {String} liveTrackUri live track uri to update
 */
LiveTracking.prototype.updateLiveTrack = function (liveTrackUri) {

    console.log("LiveTracking: updating live track " + liveTrackUri);

    var that = this;
    $.ajax({
        url: '/?handler=UpdateLiveTrack',
        data: {
            Uri: liveTrackUri,
            LastTrackPointTime: that.map.getTrackLastTrackPointTime(liveTrackUri)
        }
    })
        .done(function (result) {
            that.onUpdateLiveTrackResult(liveTrackUri, result);
        });
};

/**
 * Called when updated live waypoint data is available
 * @param {String} liveTrackUri live track uri to update
 * @param {Object} result ajax result object with updated data, or a string as error message
 */
LiveTracking.prototype.onUpdateLiveTrackResult = function (liveTrackUri, result) {

    console.log("LiveTracking: update result: " + JSON.stringify(result));

    if (result.data !== undefined) {
        result.data.id = liveTrackUri;

        this.map.updateLiveTrack(result.data);

        if (this.liveTrackToIdMapping[liveTrackUri] !== undefined) {
            var pageIdPrefix = this.liveTrackToIdMapping[liveTrackUri];

            var idDesc = '#' + pageIdPrefix + 'Description';
            $(idDesc)[0].innerHTML = result.data.description;

            var idLastUpdate = '#' + pageIdPrefix + 'LastUpdate';
            $(idLastUpdate)[0].textContent = 'Last update: ' + new Date().toLocaleTimeString();
        }
    }

    if (typeof result === 'string') {

        if (this.liveTrackToIdMapping[liveTrackUri] !== undefined) {
            var idDesc2 = '#' + this.liveTrackToIdMapping[liveTrackUri] + 'Description';

            $(idDesc2)[0].textContent = 'Error: ' + result;
        }
    }

    // schedule next update based on reported date
    if (result.nextRequestDate !== undefined)
        this.scheduleNextUpdate(liveTrackUri, result.nextRequestDate);
};

/**
 * Schedules next update for live waypoint, using next request date from query result
 * @param {String} liveDataUri live waypoint or track uri to use
 * @param {String} nextRequestDate ISO 8601 formatted next request date
 */
LiveTracking.prototype.scheduleNextUpdate = function (liveDataUri, nextRequestDate) {

    console.log("LiveTracking: scheduling next update for live waypoint or track " + liveDataUri);

    if (this.updateTimeoutMapping[liveDataUri] !== undefined)
        clearTimeout(this.updateTimeoutMapping[liveDataUri]);

    var now = new Date();
    var nextRequest = new Date(nextRequestDate);

    var millisTillUpdate = nextRequest - now;
    if (millisTillUpdate < 0)
        millisTillUpdate = 10 * 1000; // schedule in 10 seconds

    console.log("LiveTracking: scheduling update in " + millisTillUpdate + " milliseconds");

    var that = this;
    var myTimeout = setTimeout(function () {
        console.log("LiveTracking: next update for " + liveDataUri + " is due!");
        if (liveDataUri in that.liveTrackToIdMapping)
            that.updateLiveTrack(liveDataUri);
        else
            that.updateLiveWaypoint(liveDataUri);
    }, millisTillUpdate);

    this.updateTimeoutMapping[liveDataUri] = myTimeout;
};
