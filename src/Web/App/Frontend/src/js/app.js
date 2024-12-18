// local
import "../css/app.css";

// wheretofly-weblib
import { MapView } from "wheretofly-weblib/mapView.js";
import "wheretofly-weblib/mapView.css";

/**
 * Where-To-Fly Web App
 */
export class App {

    /**
     * Console log style
     */
    static consoleLogStyle = "background: darkslateblue; color: white; padding: 1px 3px; border-radius: 3px;";

    /**
     * Creates a new app object
     * @param {object} [options] Options to use for initializing the app
     * @param {string} [options.mapElementId] DOM ID of the div element to create map view in
     * @param {Function} [options.callback] callback function to use for calling back to C# code
     */
    constructor(options) {

        App.log("initializing web app");

        this.options = Object.assign({
            mapElementId: "mapElement",
            liveTrackToolbarId: "liveTrackToolbar",
            heightProfileElementId: "heightProfileView"
        }, options);

        this.map = new MapView({
            id: this.options.mapElementId,
            liveTrackToolbarId: this.options.liveTrackToolbarId,
            heightProfileElementId: this.options.heightProfileElementId,
            initialCenterPoint: { latitude: 47.083, longitude: 12.178 },
            initialViewingDistance: 50000.0,
            hasMouse: true,
            useAsynchronousPrimitives: true,
            useEntityClustering: false,
            callback: this.callMapAction.bind(this)
        });
    }

    /**
     * Logs a message to the console, just like console.log, but with styled output.
     * @param {string} message message to log
     */
    static log(message) {
        console.log("%cWebApp%c" + message, App.consoleLogStyle);
    }

    /**
     * Function for MapView for map actions
     * @param {string} action callback action name
     * @param {object} params action params
     */
    callMapAction(action, params) {
        App.log("call action: " + action + ", params: " + JSON.stringify(params));

        this.options.callback(action, params);
    }
}

export default (options) => new App(options);
