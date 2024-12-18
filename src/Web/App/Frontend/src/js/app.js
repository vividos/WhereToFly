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
     */
    constructor() {

        App.log("initializing web app");

        this.map = new MapView({
            id: "mapElement",
            liveTrackToolbarId: "liveTrackToolbar",
            heightProfileElementId: "heightProfileView",
            initialCenterPoint: { latitude: 47.083, longitude: 12.178 },
            initialViewingDistance: 50000.0,
            hasMouse: true,
            useAsynchronousPrimitives: true,
            useEntityClustering: false,
            callback: this.callMapAction
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
     * @param {string} funcName function name of action
     * @param {object} params action params
     */
    callMapAction(funcName, params) {
        App.log("call action: " + funcName + ", params: " + JSON.stringify(params));
    }
}

export default new App();
