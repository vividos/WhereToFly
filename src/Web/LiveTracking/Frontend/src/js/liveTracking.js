import $ from "jquery";
import MapView from './mapView3D.js';
import * as Cesium from 'cesium';

export default class LiveTracking {
    /**
     * Creates a new live tracking object
     */
    constructor() {

        console.log("LiveTracking: initializing live tracking site");

        this.liveWaypointToIdMapping = {};
        this.liveTrackToIdMapping = {};
        this.updateTimeoutMapping = {};

        this.initPage();

        this.map = new MapView({
            id: 'mapElement',
            liveTrackToolbarId: 'liveTrackToolbar',
            initialCenterPoint: { latitude: 47.083, longitude: 12.178 },
            initialViewingDistance: 5000.0,
            hasMouse: true,
            useAsynchronousPrimitives: true,
            useEntityClustering: false,
            callback: this.callMapAction
        });

        this.map.setShadingMode('Fixed10Am');

        this.addDefaultLayerAndLocationListsAndTracks();
    }

    /**
     * Initializes find box and sidebar
     */
    initPage() {

        var that = this;
        $('#findForm').submit(function (event) {

            event.preventDefault();

            that.geocodeAndShow($('#findValue')[0].value);
        });
    }

    /**
     * Function for MapView for map actions
     * @param {String} funcName function name of action
     * @param {object} params action params
     */
    callMapAction(funcName, params) {
        console.log("LiveTracking: call action: " + funcName + ", params: " + JSON.stringify(params));
    }

    /**
     * Adds all default layers, locations and tracks to show in the live tracking map
     */
    addDefaultLayerAndLocationListsAndTracks() {

        console.log("LiveTracking: loading default data...");

        this.addCrossingTheAlpsLayer();
    }

    /**
     * Adds "X-Lakes" layer
     */
    addXLakesLayer() {

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
    }

    /**
     * Adds "crossing the alps" layer
     */
    addCrossingTheAlpsLayer() {

        var that = this;
        $.get("/data/crossing-the-alps-2021.czml",
            null,
            function (data) {
                console.log("LiveTracking: successfully loaded czml file, adding layer");
                that.map.addLayer({
                    id: 'crossing-the-alps-2021-layer',
                    name: 'Crossing the Alps 2021',
                    type: '',
                    isVisible: true,
                    data: data
                });
                that.map.zoomToLayer('crossing-the-alps-2021-layer');
            },
            "text");
    }

    /**
     * Adds a single live waypoint to track
     * @param {String} name Name of live waypoint
     * @param {String} liveWaypointId Live waypoint ID
     */
    addLiveWaypoint = function (name, liveWaypointId) {

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
    }

    /**
     * Adds a single live track to map
     * @param {String} name Name of live track
     * @param {String} liveTrackId Live track ID
     */
    addLiveTrack(name, liveTrackId) {

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
    geocodeAndShow(address) {

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
    }

    /**
     * Zooms to a live waypoint by given page ID prefix
     * @param {String} pageIdPrefix page ID prefix of live waypoint
     */
    zoomToByPrefix(pageIdPrefix) {

        var liveWaypointUri =
            Object.keys(this.liveWaypointToIdMapping).find(
                key => this.liveWaypointToIdMapping[key] === pageIdPrefix);

        if (liveWaypointUri !== undefined) {

            var entity = this.map.locationDataSource.entities.getById(liveWaypointUri);

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
    }

    /**
     * Updates a live waypoint or track by given page ID prefix
     * @param {String} pageIdPrefix page ID prefix of live waypoint
     */
    updateByPrefix(pageIdPrefix) {

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
    }

    /**
     * Updates a live waypoint
     * @param {String} liveWaypointUri live waypoint uri to update
     */
    updateLiveWaypoint(liveWaypointUri) {

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
    }

    /**
     * Called when updated live waypoint data is available
     * @param {String} liveWaypointUri live waypoint uri to update
     * @param {Object} result ajax result object with updated data, or a string as error message
     */
    onUpdateLiveWaypointResult(liveWaypointUri, result) {

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
    }

    /**
     * Updates a live track
     * @param {String} liveTrackUri live track uri to update
     */
    LupdateLiveTrack(liveTrackUri) {

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
    onUpdateLiveTrackResult(liveTrackUri, result) {

        //console.log("LiveTracking: update result: " + JSON.stringify(result));

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
    scheduleNextUpdate(liveDataUri, nextRequestDate) {

        console.log("LiveTracking: scheduling next update for live waypoint or track " + liveDataUri);

        if (this.updateTimeoutMapping[liveDataUri] !== undefined)
            clearTimeout(this.updateTimeoutMapping[liveDataUri]);

        var now = new Date();
        var nextRequest = new Date(nextRequestDate);

        var millisTillUpdate = nextRequest - now;
        if (millisTillUpdate < 0)
            millisTillUpdate = 10 * 1000; // schedule in 10 seconds

        console.log("LiveTracking: scheduling update in " + (millisTillUpdate / 1000.0).toFixed(1) + " seconds");

        var that = this;
        var myTimeout = setTimeout(function () {
            console.log("LiveTracking: next update for " + liveDataUri + " is due!");
            if (liveDataUri in that.liveTrackToIdMapping)
                that.updateLiveTrack(liveDataUri);
            else
                that.updateLiveWaypoint(liveDataUri);
        }, millisTillUpdate);

        this.updateTimeoutMapping[liveDataUri] = myTimeout;
    }
}
