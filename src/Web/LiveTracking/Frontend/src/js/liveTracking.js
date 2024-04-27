import $ from "jquery";

// wheretofly-weblib
import { MapView } from "wheretofly-weblib/mapView.js";
import "wheretofly-weblib/mapView.css";

export default class LiveTracking {

    /**
     * Console log style
     */
    static consoleLogStyle = "background: yellow; color: indigo; padding: 1px 3px; border-radius: 3px;";

    /**
     * Creates a new live tracking object
     */
    constructor() {

        LiveTracking.log("initializing live tracking site");

        this.liveWaypointToIdMapping = {};
        this.liveTrackToIdMapping = {};
        this.updateTimeoutMapping = {};

        this.initPage();

        this.map = new MapView({
            id: "mapElement",
            liveTrackToolbarId: "liveTrackToolbar",
            heightProfileElementId: "heightProfileView",
            initialCenterPoint: { latitude: 47.083, longitude: 12.178 },
            initialViewingDistance: 5000.0,
            hasMouse: true,
            useAsynchronousPrimitives: true,
            useEntityClustering: false,
            callback: this.callMapAction
        });

        this.map.setShadingMode("Fixed10Am");

        this.addDefaultLayerAndLocationListsAndTracks();
    }

    /**
     * Logs a message to the console, just like console.log, but with styled output.
     * @param {string} message message to log
     */
    static log(message) {
        console.log("%cLiveTracking%c" + message, LiveTracking.consoleLogStyle);
    }

    /**
     * Initializes find box and sidebar
     */
    initPage() {

        const that = this;
        $("#findForm").submit(function(event) {

            event.preventDefault();

            that.geocodeAndShow($("#findValue")[0].value);
        });
    }

    /**
     * Function for MapView for map actions
     * @param {string} funcName function name of action
     * @param {object} params action params
     */
    callMapAction(funcName, params) {
        LiveTracking.log("call action: " + funcName + ", params: " + JSON.stringify(params));
    }

    /**
     * Adds all default layers, locations and tracks to show in the live tracking map
     */
    addDefaultLayerAndLocationListsAndTracks() {

        LiveTracking.log("loading default data...");

        this.addCrossingTheAlpsLayer();
    }

    /**
     * Adds "X-Lakes" layer
     */
    addXLakesLayer() {

        const that = this;
        $.get("/data/x-lakes-2020-wainwrights.czml",
            null,
            function(data) {
                LiveTracking.log("successfully loaded czml file, adding layer");
                that.map.addLayer({
                    id: "x-lakes-2020-layer",
                    name: "X-Lakes 2020 Wainwrights",
                    type: "",
                    isVisible: true,
                    data
                });
                that.map.zoomToLayer("x-lakes-2020-layer");
            },
            "text");
    }

    /**
     * Adds "Fjällräven Classic" layer
     */
    async addFjallravenClassicLayer() {

        const result = await fetch("/data/fjallraven-classic-2022.czml");

        const czml = await result.text();

        LiveTracking.log("successfully loaded czml file, adding layer");

        await this.map.addLayer({
            id: "fjallraven-classic-2022-layer",
            name: "Fjällräven Classic 2022",
            type: "CzmlLayer",
            isVisible: true,
            data: czml
        });

        this.map.zoomToLayer("fjallraven-classic-2022-layer");
    }

    /**
     * Adds "crossing the alps" layer
     */
    addCrossingTheAlpsLayer() {

        const that = this;
        $.get("/data/crossing-the-alps-2021.czml",
            null,
            function(data) {
                LiveTracking.log("successfully loaded czml file, adding layer");
                that.map.addLayer({
                    id: "crossing-the-alps-2021-layer",
                    name: "Crossing the Alps 2021",
                    type: "",
                    isVisible: true,
                    data
                });
                that.map.zoomToLayer("crossing-the-alps-2021-layer");
            },
            "text");
    }

    /**
     * Adds a single live waypoint to track
     * @param {string} name Name of live waypoint
     * @param {string} liveWaypointId Live waypoint ID
     */
    addLiveWaypoint(name, liveWaypointId) {

        const pageIdPrefix = "liveData";
        this.liveWaypointToIdMapping[liveWaypointId] = pageIdPrefix;

        const idName = "#" + pageIdPrefix + "Name";
        $(idName)[0].textContent = name;

        this.map.addLocationList([{
            id: liveWaypointId,
            name,
            description: "",
            type: "LiveWaypoint",
            latitude: 0.0,
            longitude: 0.0,
            altitude: 0.0,
            isPlanTourLocation: false
        }]);

        this.updateLiveWaypoint(liveWaypointId);
    }

    /**
     * Adds a single live track to map
     * @param {string} name Name of live track
     * @param {string} liveTrackId Live track ID
     * @param {boolean} isFlightTrack Indicates if it's a flight track
     */
    addLiveTrack(name, liveTrackId, isFlightTrack) {

        const pageIdPrefix = "liveData";
        this.liveTrackToIdMapping[liveTrackId] = pageIdPrefix;

        const idName = "#" + pageIdPrefix + "Name";
        $(idName)[0].textContent = name;

        this.map.addTrack({
            id: liveTrackId,
            name,
            description: "",
            isFlightTrack,
            isLiveTrack: true,
            listOfTrackPoints: [],
            listOfTimePoints: [],
            groundHeightProfile: null,
            color: "4080ff"
        });

        this.updateLiveTrack(liveTrackId);
    }

    /**
     * Geocodes entered address and shows pin
     * @param {string} address entered address to find
     */
    geocodeAndShow(address) {

        LiveTracking.log("geocoding find text: " + address);

        const endpoint = "https://nominatim.openstreetmap.org/search";

        const that = this;
        $.ajax({
            url: endpoint,
            data: {
                q: address
            }
        })
            .done(function(result) {

                return result.data.map(function(resultObject) {
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
     * @param {string} pageIdPrefix page ID prefix of live waypoint
     */
    zoomToByPrefix(pageIdPrefix) {

        const liveWaypointUri =
            Object.keys(this.liveWaypointToIdMapping).find(
                key => this.liveWaypointToIdMapping[key] === pageIdPrefix);

        if (liveWaypointUri !== undefined)
            this.map.zoomToLiveWaypointCurrentPos(liveWaypointUri);

        const liveTrackUri =
            Object.keys(this.liveTrackToIdMapping).find(
                key => this.liveTrackToIdMapping[key] === pageIdPrefix);

        if (liveTrackUri !== undefined)
            this.map.zoomToTrack(liveTrackUri);
    }

    /**
     * Updates a live waypoint or track by given page ID prefix
     * @param {string} pageIdPrefix page ID prefix of live waypoint
     */
    updateByPrefix(pageIdPrefix) {

        const liveWaypointUri =
            Object.keys(this.liveWaypointToIdMapping).find(
                key => this.liveWaypointToIdMapping[key] === pageIdPrefix);

        if (liveWaypointUri !== undefined)
            this.updateLiveWaypoint(liveWaypointUri);

        const liveTrackUri =
            Object.keys(this.liveTrackToIdMapping).find(
                key => this.liveTrackToIdMapping[key] === pageIdPrefix);

        if (liveTrackUri !== undefined)
            this.updateLiveTrack(liveTrackUri);
    }

    /**
     * Updates a live waypoint
     * @param {string} liveWaypointUri live waypoint uri to update
     */
    updateLiveWaypoint(liveWaypointUri) {

        LiveTracking.log("updating live waypoint " + liveWaypointUri);

        const that = this;
        $.ajax({
            url: "/?handler=UpdateLiveWaypoint",
            data: {
                Uri: liveWaypointUri
            }
        })
            .done(function(result) {
                that.onUpdateLiveWaypointResult(liveWaypointUri, result);
            });
    }

    /**
     * Called when updated live waypoint data is available
     * @param {string} liveWaypointUri live waypoint uri to update
     * @param {object} result ajax result object with updated data, or a string as error message
     */
    onUpdateLiveWaypointResult(liveWaypointUri, result) {

        LiveTracking.log("update result: " + JSON.stringify(result));

        if (result.data !== undefined) {
            result.data.id = liveWaypointUri;
            result.data.type = "LiveWaypoint";

            this.map.updateLocation(result.data);

            if (this.liveWaypointToIdMapping[liveWaypointUri] !== undefined) {
                const pageIdPrefix = this.liveWaypointToIdMapping[liveWaypointUri];

                // var idName = '#' + pageIdPrefix + 'Name';
                // $(idName)[0].textContent = result.data.name;

                const idDesc = "#" + pageIdPrefix + "Description";
                $(idDesc)[0].innerHTML = result.data.description;

                const idLastUpdate = "#" + pageIdPrefix + "LastUpdate";
                $(idLastUpdate)[0].textContent = "Last update: " + new Date().toLocaleTimeString();
            }
        }

        if (typeof result === "string") {

            if (this.liveWaypointToIdMapping[liveWaypointUri] !== undefined) {
                const idDesc2 = "#" + this.liveWaypointToIdMapping[liveWaypointUri] + "Description";

                $(idDesc2)[0].textContent = "Error: " + result;
            }
        }

        // schedule next update based on reported date
        if (result.nextRequestDate !== undefined)
            this.scheduleNextUpdate(liveWaypointUri, result.nextRequestDate);
    }

    /**
     * Updates a live track
     * @param {string} liveTrackUri live track uri to update
     */
    updateLiveTrack(liveTrackUri) {

        LiveTracking.log("updating live track " + liveTrackUri);

        const that = this;
        $.ajax({
            url: "/?handler=UpdateLiveTrack",
            data: {
                Uri: liveTrackUri,
                LastTrackPointTime: that.map.getTrackLastTrackPointTime(liveTrackUri)
            }
        })
            .done(function(result) {
                that.onUpdateLiveTrackResult(liveTrackUri, result);
            });
    }

    /**
     * Called when updated live waypoint data is available
     * @param {string} liveTrackUri live track uri to update
     * @param {object} result ajax result object with updated data, or a string as error message
     */
    onUpdateLiveTrackResult(liveTrackUri, result) {

        if (result.data !== undefined) {
            result.data.id = liveTrackUri;
            result.data.groundHeightProfile = null;

            this.map.updateLiveTrack(result.data);

            if (this.liveTrackToIdMapping[liveTrackUri] !== undefined) {
                const pageIdPrefix = this.liveTrackToIdMapping[liveTrackUri];

                const idDesc = "#" + pageIdPrefix + "Description";
                $(idDesc)[0].innerHTML = result.data.description;

                const idLastUpdate = "#" + pageIdPrefix + "LastUpdate";
                $(idLastUpdate)[0].textContent = "Last update: " + new Date().toLocaleTimeString();
            }
        }

        if (typeof result === "string") {

            if (this.liveTrackToIdMapping[liveTrackUri] !== undefined) {
                const idDesc2 = "#" + this.liveTrackToIdMapping[liveTrackUri] + "Description";

                $(idDesc2)[0].textContent = "Error: " + result;
            }
        }

        // schedule next update based on reported date
        if (result.nextRequestDate !== undefined)
            this.scheduleNextUpdate(liveTrackUri, result.nextRequestDate);
    }

    /**
     * Schedules next update for live waypoint, using next request date from query result
     * @param {string} liveDataUri live waypoint or track uri to use
     * @param {string} nextRequestDate ISO 8601 formatted next request date
     */
    scheduleNextUpdate(liveDataUri, nextRequestDate) {

        LiveTracking.log("scheduling next update for live waypoint or track " + liveDataUri);

        if (this.updateTimeoutMapping[liveDataUri] !== undefined)
            clearTimeout(this.updateTimeoutMapping[liveDataUri]);

        const now = new Date();
        const nextRequest = new Date(nextRequestDate);

        let millisTillUpdate = nextRequest - now;
        if (millisTillUpdate < 0)
            millisTillUpdate = 10 * 1000; // schedule in 10 seconds

        LiveTracking.log("scheduling update in " + (millisTillUpdate / 1000.0).toFixed(1) + " seconds");

        const that = this;
        const myTimeout = setTimeout(function() {
            LiveTracking.log("next update for " + liveDataUri + " is due!");
            if (liveDataUri in that.liveTrackToIdMapping)
                that.updateLiveTrack(liveDataUri);
            else
                that.updateLiveWaypoint(liveDataUri);
        }, millisTillUpdate);

        this.updateTimeoutMapping[liveDataUri] = myTimeout;
    }
}
