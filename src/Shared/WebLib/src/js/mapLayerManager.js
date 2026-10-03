// mapLayerManager.js - manages map base layer and overlay layers

// Cesium.js
import {
    BingMapsImageryProvider,
    BingMapsStyle,
    Color,
    Credit,
    ImageryLayer,
    IonImageryProvider,
    Material,
    OpenStreetMapImageryProvider,
    Scene,
    UrlTemplateImageryProvider,
    WebMercatorTilingScheme,
} from "cesium";

/**
 * Map layer manager
 * @class
 */
export class MapLayerManager {

    /**
     * Console log style
     */
    static consoleLogStyle = "background: DarkMagenta; color: white; padding: 1px 3px; border-radius: 3px;";

    /**
     * Logs a message to the console, just like console.log, but with styled output.
     * @param {string} message to log
     */
    static log(message) {
        console.log("%cMapLayerManager", MapLayerManager.consoleLogStyle, message);
    }

    /**
     * Creates a new map layer manager
     * @param {object} [options] Options to use for initializing map layer manager
     * @param {string} [options.bingMapsApiKey] Bing maps API key to use
     * @param {string} [options.legendElement] DOM id of element where legends
     * can be added to
     */
    constructor(options) {
        this.scene = null;

        const defaultOptions = {
            bingMapsApiKey: null,
            legendElement: "legendElement"
        };

        this.options = { ...defaultOptions, ...options };
    }

    /**
     * Initializes map layer manager.
     */
    init() {
        this.imageryLayerInfos = {
            OpenStreetMap: {
                layer: null,
                provider: new OpenStreetMapImageryProvider({
                    url: "https://{s}.tile.openstreetmap.org/",
                    subdomains: "abc",
                    maximumLevel: 18
                })
            },
            BingMapsAerialWithLabels: {
                layer: null,
                provider: null,
                getProvider: async() => {
                    return await BingMapsImageryProvider.fromUrl(
                        "https://dev.virtualearth.net",
                        {
                            key: this.options.bingMapsApiKey,
                            mapStyle: BingMapsStyle.AERIAL_WITH_LABELS_ON_DEMAND
                        });
                }
            },
            OpenTopoMap: {
                layer: null,
                provider: new OpenStreetMapImageryProvider({
                    url: "https://{s}.tile.opentopomap.org/",
                    subdomains: "abc",
                    maximumLevel: 18,
                    credits: "<code>Kartendaten: &copy; <a href=\"https://openstreetmap.org/copyright\">OpenStreetMap</a>-Mitwirkende, SRTM | Kartendarstellung: &copy; <a href=\"https://opentopomap.org\">OpenTopoMap</a> (<a href=\"https://creativecommons.org/licenses/by-sa/3.0/\">CC-BY-SA</a>)</code>"
                })
            },
            Sentinel2: {
                layer: null,
                provider: null,
                getProvider: async() => {
                    return await IonImageryProvider.fromAssetId(3954);
                }
            }
        };

        this.setupSlopeAndContourLines();
        this.setupSlopeDirectionMaterial();

        this.imageryOverlayInfos = {
            ThermalSkywaysKk7: {
                layer: null,
                provider: null,
                getProvider: () => this.createThermalImageryProvider(),
                configLayer: (layer) => {
                    layer.alpha = 0.2; // 0.0 is transparent.  1.0 is opaque.
                    layer.brightness = 2; // > 1.0 increases brightness.  < 1.0 decreases.
                }
            },
            BlackMarble: {
                layer: null,
                provider: null,
                getProvider: () => {
                    // The Earth at Night, also known as Black Marble 2017 and Night Lights
                    return IonImageryProvider.fromAssetId(3812);
                },
                configLayer: (layer) => {
                    layer.alpha = 0.5; // 0.0 is transparent.  1.0 is opaque.
                    layer.brightness = 2; // > 1.0 increases brightness.  < 1.0 decreases.
                }
            },
            WaymarkedTrailsHiking: {
                layer: null,
                provider: null,
                getProvider: () => this.createWaymarkedTrailsHikingImageryProvider(),
                configLayer: (layer) => {
                    layer.alpha = 0.8; // 0.0 is transparent.  1.0 is opaque.
                    layer.brightness = 1; // > 1.0 increases brightness.  < 1.0 decreases.
                }
            },
            OpenFlightMaps: {
                layer: null,
                provider: null,
                getProvider: () => {
                    const airacId = MapLayerManager.calcCurrentAiracId();
                    return new UrlTemplateImageryProvider({
                        url: "https://nwy-tiles-api.prod.newaydata.com/tiles/{z}/{x}/{y}.png?path=" + airacId + "/aero/latest",
                        tileWidth: 512,
                        tileHeight: 512,
                        maximumLevel: 11,
                        enablePickFeatures: false,
                        credit: "(c) <a href=\"https://openflightmaps.org/\" target=\"_blank\">Open Flightmaps association</a>, (c) OpenStreetMap contributors, NASA elevation data"
                    });
                },
                configLayer: () => { }
            }
        };

        this.imageryLayerInfos.OpenStreetMap.layer =
            new ImageryLayer(
                this.imageryLayerInfos.OpenStreetMap.provider);
    }

    /**
     * Creates a new imagery provider that uses the Thermal Skyways from https://thermal.kk7.ch/.
     * @returns {object} generated imagery provider object
     */
    async createThermalImageryProvider() {

        const url = "https://thermal.kk7.ch/tiles/skyways_all/{z}/{x}/{reverseY}.png?src=https://github.com/vividos/WhereToFly";
        MapLayerManager.log("thermal maps url: " + url);

        const creditText = "Skyways &copy; <a href=\"https://thermal.kk7.ch/\">thermal.kk7.ch</a>";

        const tilingScheme = new WebMercatorTilingScheme();

        const provider = new UrlTemplateImageryProvider({
            url,
            credit: new Credit(creditText, false),
            tilingScheme,
            tileWidth: 256,
            tileHeight: 256,
            minimumLevel: 0,
            maximumLevel: 12,
            rectangle: tilingScheme.rectangle
        });

        return provider;
    }

    /**
     * Creates an imagery provider for the Waymarked Trails route overlay, subtype hiking
     * @returns {object} imagery provider
     */
    createWaymarkedTrailsHikingImageryProvider() {

        const creditText = "The Waymarked Trails route overlay is licensed under the " +
            "<a href=\"https://creativecommons.org/licenses/by-sa/3.0/de/deed.de\">CC BY-SA 3.0 DE</a> license";

        return new OpenStreetMapImageryProvider({
            url: "https://tile.waymarkedtrails.org/hiking/",
            credit: new Credit(creditText, false),
            maximumLevel: 17
        });
    }

    /**
     * Returns the base layer to display
     * @returns {ImageryLayer} base layer
     */
    getBaseLayer() {
        return this.imageryLayerInfos.OpenStreetMap.layer;
    }

    /**
     * Sets scene object as soon as it is created
     * @param {Scene} scene scene object
     */
    setScene(scene) {
        this.scene = scene;
    }

    /**
     * Updates scene by requesting rendering from scene. This can be used to
     * update the view, e.g. when a pin was added or removed, and CesiumJS
     * wouldn't update the scene itself.
     */
    updateScene() {
        this.scene.requestRender();
    }

    /**
     * Sets new map imagery type
     * @param {string} imageryType imagery type constant; the following constants currently can be
     * used: 'OpenStreetMap', 'BingMapsAerialWithLabels', 'OpenTopoMap', 'Sentinel2'.
     */
    async setMapImageryType(imageryType) {

        MapLayerManager.log("setting new imagery type: " + imageryType);

        const layers = this.scene.imageryLayers;

        for (const key in this.imageryLayerInfos) {
            if (this.imageryLayerInfos[key].layer !== null)
                layers.remove(this.imageryLayerInfos[key].layer, false);
        }

        if (imageryType in this.imageryLayerInfos) {

            const imageryTypeInfo = this.imageryLayerInfos[imageryType];
            if (imageryTypeInfo.layer === null) {

                if (imageryTypeInfo.provider === null)
                    imageryTypeInfo.provider = await imageryTypeInfo.getProvider();

                imageryTypeInfo.layer =
                    layers.addImageryProvider(imageryTypeInfo.provider, 0);
            } else
                layers.add(imageryTypeInfo.layer, 0);

            this.updateScene();

        } else
            console.warn("MapLayerManager.setMapImageryType: invalid imagery type: " + imageryType);
    }

    static slopeRamp = [0, 0.29, 0.5, Math.sqrt(2) / 2, 0.87, 0.91, 1];

    /**
     * Generates a color ramp canvas element and returns it. From
     * https://sandcastle.cesium.com/?src=Globe%20Materials.html
     * @returns {object} generated canvas object
     */
    static getColorRamp() {
        const ramp = document.createElement("canvas");
        ramp.width = 100;
        ramp.height = 1;
        const ctx = ramp.getContext("2d");

        const values = MapLayerManager.slopeRamp;

        const grd = ctx.createLinearGradient(0, 0, 100, 0);
        grd.addColorStop(values[0], "#000000"); // black
        grd.addColorStop(values[1], "#2747E0"); // blue
        grd.addColorStop(values[2], "#D33B7D"); // pink
        grd.addColorStop(values[3], "#D33038"); // red
        grd.addColorStop(values[4], "#FF9742"); // orange
        grd.addColorStop(values[5], "#ffd700"); // yellow
        grd.addColorStop(values[6], "#ffffff"); // white

        ctx.fillStyle = grd;
        ctx.fillRect(0, 0, 100, 1);

        return ramp;
    }

    /**
     * Sets up slope and contour lines materials for overlay types 'ContourLines' and
     * 'SlopeAndContourLines'. From
     * https://sandcastle.cesium.com/?src=Globe%20Materials.html
     */
    setupSlopeAndContourLines() {

        // Creates a material with contour lines only
        this.contourLinesMaterial = Material.fromType("ElevationContour");

        this.contourLinesMaterial.uniforms.width = 1; // in pixels
        this.contourLinesMaterial.uniforms.spacing = 100; // in meters
        this.contourLinesMaterial.uniforms.color = Color.BLUE.clone();

        // Creates a composite material with both slope shading and contour lines
        const material = new Material({
            fabric: {
                type: "SlopeColorContour",
                materials: {
                    contourMaterial: {
                        type: "ElevationContour"
                    },
                    slopeRampMaterial: {
                        type: "SlopeRamp"
                    }
                },
                components: {
                    diffuse: "contourMaterial.alpha == 0.0 ? slopeRampMaterial.diffuse : contourMaterial.diffuse",
                    alpha: "max(contourMaterial.alpha, slopeRampMaterial.alpha)"
                }
            },
            translucent: false
        });

        const contourUniforms = material.materials.contourMaterial.uniforms;
        contourUniforms.width = 1; // in pixels
        contourUniforms.spacing = 100; // in meters
        contourUniforms.color = Color.BLUE.clone();

        const shadingUniforms = material.materials.slopeRampMaterial.uniforms;
        shadingUniforms.image = MapLayerManager.getColorRamp();

        this.slopeAndContourLinesMaterial = material;
    }

    /**
     * Sets up aspect material for overlay types 'SlopeDirection'. From
     * https://sandcastle.cesium.com/?src=Globe%20Materials.html
     */
    setupSlopeDirectionMaterial() {

        const material = Material.fromType(Material.AspectRampMaterialType);
        this.slopeDirectionMaterial = material;

        material.translucent = true;

        const shadingUniforms = material.uniforms;
        shadingUniforms.image = MapLayerManager.getAspectColorRamp();

        // also create a legend canvas
        this.slopeDirectionLegendCanvas =
            MapLayerManager.createLegendRampFromColors(MapLayerManager.aspectColors);
    }

    /**
     * Colors for terrain aspect; colors are ordered clockwise:
     * N, NE, E, SE, S, SW, W, NW
     */
    static aspectColors = [
        "108   0 163", // violet
        "202   0 156", // pink
        "255  85 104", // red
        "255 171  71", // orange
        "244 250   0", // yellow
        "132 214   0", // light green
        "  0 171  68", // dark green
        "  0 104 192", // blue
    ];

    /**
     * Generates a color ramp canvas element for coloring terrain using aspect
     * (slope of terrain) and returns it.
     * @returns {object} generated canvas object
     */
    static getAspectColorRamp() {
        const ramp = document.createElement("canvas");
        ramp.width = 360;
        ramp.height = 1;
        const ctx = ramp.getContext("2d");

        const grd = ctx.createLinearGradient(0, 0, 360, 0);

        // gradient stops are ordered counter-clockwise:
        // 0.0-0.25: E to N
        // 0.25-0.5: N to W
        // 0.5-0.75: W to S
        // 0.75-1.0: S to E

        /**
         * converts from degrees to gradient stop values [0.0; 1.0[
         * @param {number} degrees degrees value
         * @returns {number} gradient stop value
         */
        function degreesToStop(degrees) {
            return (((90 - degrees) / 360) + 1) % 1;
        }

        const colors = MapLayerManager.aspectColors;

        for (let degrees = 0; degrees < 360; degrees += 45) {
            const color = "rgb(" + colors[degrees / 45] + " / 50%)";
            grd.addColorStop(degreesToStop(degrees - 22.49), color);
            grd.addColorStop(degreesToStop(degrees + 22.49), color);

            if (degrees == 90) {
                grd.addColorStop(0, color);
                grd.addColorStop(1, color);
            }
        }

        ctx.fillStyle = grd;
        ctx.fillRect(0, 0, 360, 1);

        return ramp;
    }

    /**
     * Displays the legend for the terrain slope overlay
     */
    showTerrainSlopeLegend() {

        let legend = document.getElementById(this.options.legendElement);
        legend.replaceChildren();

        // add title
        const titleDiv = document.createElement("div");
        titleDiv.classList.add("legend-title");
        titleDiv.innerText = "Legend";
        legend.appendChild(titleDiv);

        // add ramp image
        legend.appendChild(this.slopeDirectionLegendCanvas);
        this.slopeDirectionLegendCanvas.style = "width: 300px; height: 32px";

        // add legend text
        const textDiv = document.createElement("div");
        textDiv.style = "width: 300px; display: flex; justify-content: space-around;"
        textDiv.innerHTML = "<div>N</div><div>NE</div><div>E</div><div>SE</div><div>S</div><div>SW</div><div>W</div><div>NW</div>";
        legend.appendChild(textDiv);

        legend.style.display = "block";
    }

    /**
     * Hides the legend again
     */
    hideLegend() {

        let legend = document.getElementById(this.options.legendElement);
        legend.style.display = "none";
    }

    /**
     * Creates a canvas element with a list of colors, evenly spaced, usable
     * for displaying a legend. The image is 1 pixel in height, and 10 pixels
     * for every color wide.
     * @param {Array.<string>} colors array of colors to use
     * @returns {HTMLCanvasElement} canvas element
     */
    static createLegendRampFromColors(colors) {
        const legendRamp = document.createElement("canvas");
        legendRamp.width = colors.length * 10;
        legendRamp.height = 1;
        const legendContext = legendRamp.getContext("2d");

        const legendGradient = legendContext.createLinearGradient(0, 0, colors.length * 10, 0);

        for (let colorIndex = 0; colorIndex < colors.length; colorIndex++) {

            const color = "rgb(" + colors[colorIndex] + ")";
            legendGradient.addColorStop(colorIndex / colors.length, color);
            legendGradient.addColorStop((colorIndex + 0.999) / colors.length, color);
        }

        legendContext.fillStyle = legendGradient;
        legendContext.fillRect(0, 0, legendRamp.width, legendRamp.height);

        return legendRamp;
    }

    /**
     * Sets new map overlay type
     * @param {string} overlayType overlay type constant; the following constants currently can be
     * used: 'None', 'ContourLines', 'SlopeAndContourLines', 'ThermalSkywaysKk7', 'BlackMarble',
     * 'WaymarkedTrailsHiking', 'OpenFlightMaps', 'SlopeDirection'.
     */
    async setMapOverlayType(overlayType) {

        MapLayerManager.log("setting new map overlay type: " + overlayType);

        const layers = this.scene.imageryLayers;

        for (const key in this.imageryOverlayInfos) {
            if (this.imageryOverlayInfos[key].layer !== null)
                layers.remove(this.imageryOverlayInfos[key].layer, false);
        }

        this.scene.globe.material = null;

        switch (overlayType) {
        case "None":
            break;

        case "ContourLines":
            this.scene.globe.material = this.contourLinesMaterial;
            break;

        case "SlopeAndContourLines":
            this.scene.globe.material = this.slopeAndContourLinesMaterial;
            break;

        case "SlopeDirection":
            this.scene.globe.material = this.slopeDirectionMaterial;
            this.showTerrainSlopeLegend();
            break;

        default:
            if (overlayType in this.imageryOverlayInfos) {

                const overlayTypeInfo = this.imageryOverlayInfos[overlayType];
                if (overlayTypeInfo.layer === null) {

                    if (overlayTypeInfo.provider === null)
                        overlayTypeInfo.provider = await overlayTypeInfo.getProvider();

                    overlayTypeInfo.layer =
                            layers.addImageryProvider(overlayTypeInfo.provider, 1);

                    overlayTypeInfo.configLayer(overlayTypeInfo.layer);

                } else
                    layers.add(overlayTypeInfo.layer, 1);

            } else
                console.warn("MapLayerManager.setMapOverlayType: invalid map overlay type: " + overlayType);
            break;
        }

        if (overlayType !== "SlopeDirection")
            this.hideLegend();

        this.updateScene();
    }

    /**
     * AIRAC cycle start dates, by year:
     * https://www.nm.eurocontrol.int/RAD/common/airac_dates.html
     */
    static airacStartDates = {
        2020: "2020-01-02",
        2021: "2021-01-28",
        2022: "2022-01-27",
        2023: "2023-01-26",
        2024: "2024-01-25",
        2025: "2025-01-23",
        2026: "2026-01-22"
    };

    /**
     * Calculates the current airac ID, based on the current date. The first two
     * digits represent the last two year digits. The remaining two digits are
     * counted up from 1, every 28 days, starting on a specific date.
     * @returns {number} current airac ID
     */
    static calcCurrentAiracId() {
        const now = new Date();
        let currentYear = now.getFullYear();

        let baseAirac = new Date(MapLayerManager.airacStartDates[currentYear]);
        if (now < baseAirac) {
            currentYear--;
            baseAirac = new Date(MapLayerManager.airacStartDates[currentYear]);
        }

        const diffInDays = (now - baseAirac) / 86400000;

        const airacId = ((currentYear - 2000) * 100) + 1 + Math.floor(diffInDays / 28);
        return airacId;
    }
}
