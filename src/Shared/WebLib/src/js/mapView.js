// Cesium.js
import {
    arrayRemoveDuplicates,
    createOsmBuildingsAsync,
    createWorldTerrainAsync,
    exportKml,
    getAbsoluteUri,
    sampleTerrainMostDetailed,
    viewerCesiumInspectorMixin,
    BingMapsImageryProvider,
    BingMapsStyle,
    BoundingSphere,
    BoundingSphereState,
    Cartesian2,
    Cartesian3,
    Cartographic,
    Clock,
    ClockStep,
    ClockRange,
    ClockViewModel,
    Color,
    ColorGeometryInstanceAttribute,
    ConstantProperty,
    Credit,
    CustomDataSource,
    CzmlDataSource,
    DataSource,
    DistanceDisplayCondition,
    EllipsoidTerrainProvider,
    Entity,
    EntityCluster,
    ExtrapolationType,
    GeometryInstance,
    GroundPolylineGeometry,
    GroundPolylinePrimitive,
    HeadingPitchRange,
    HeadingPitchRoll,
    HeightReference,
    ImageryLayer,
    Ion,
    IonImageryProvider,
    JulianDate,
    LabelStyle,
    LagrangePolynomialApproximation,
    Material,
    MaterialAppearance,
    Matrix3,
    Math as CesiumMath,
    OpenStreetMapImageryProvider,
    PinBuilder,
    PolygonHierarchy,
    PolylineColorAppearance,
    PolylineGeometry,
    Primitive,
    PrimitiveCollection,
    Quaternion,
    SampledPositionProperty,
    ScreenSpaceEventHandler,
    ScreenSpaceEventType,
    ShadowMode,
    TimeInterval,
    TimeIntervalCollectionProperty,
    Transforms,
    UrlTemplateImageryProvider,
    VerticalOrigin,
    Viewer,
    WallGeometry,
    WebMercatorTilingScheme,
} from "cesium";
import "cesium/Build/Cesium/Widgets/widgets.css";

// local
import * as Utils from "./utils.js";
import { HeightProfileView } from "./heightProfileView.js";
import "../css/mapView.css";

export class MapView {

    /**
     * Console log style
     */
    static consoleLogStyle = "background: lightgreen; color: darkblue; padding: 1px 3px; border-radius: 3px;";

    /**
     * Creates a new instance of MapView
     * @class
     * @param {object} [options] Options to use for initializing map view
     * @param {string} [options.id] DOM ID of the div element to create map view in
     * @param {string} [options.messageBandId] DOM ID of the message band div element
     * @param {string} [options.liveTrackToolbarId] DOM ID of the live track toolbar div element
     * @param {string} [options.heightProfileElementId] DOM ID of the height profile div element
     * @param {object} [options.initialCenterPoint] initial center point of map view
     * @param {number} [options.initialCenterPoint.latitude] latitude of center point
     * @param {number} [options.initialCenterPoint.longitude] longitude of center point
     * @param {number} [options.initialViewingDistance] initial viewing distance
     * @param {boolean} [options.hasMouse] indicates if the device this is running supports a mouse
     * @param {boolean} [options.useAsynchronousPrimitives] indicates if asynchronous primitives
     * should be used
     * @param {boolean} [options.useEntityClustering] indicates if entity clustering should be used
     * @param {string} [options.bingMapsApiKey] Bing maps API key to use
     * @param {string} [options.cesiumIonApiKey] Cesium Ion API key to use
     * @param {Function} [options.callback] callback function to use for calling back to C# code
     */
    constructor(options) {

        console.groupCollapsed("%cMapView", MapView.consoleLogStyle, "creating new 3D map view");
        console.time("ctor");

        this.options = Object.assign({
            id: "mapElement",
            liveTrackToolbarId: "liveTrackToolbar",
            heightProfileElementId: "heightProfileView",
            initialCenterPoint: { latitude: 47.67, longitude: 11.88 },
            initialViewingDistance: 5000.0,
            hasMouse: false,
            useAsynchronousPrimitives: true,
            useEntityClustering: true
        }, options);

        this.options.bingMapsApiKey = this.options.bingMapsApiKey || "AuuY8qZGx-LAeruvajcGMLnudadWlphUWdWb0k6N6lS2QUtURFk3ngCjIXqqFOoe";
        this.options.cesiumIonApiKey = this.options.cesiumIonApiKey || "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJqdGkiOiJiZWMzMjU5NC00MTg4LTQwYmEtYWNhYi01MDYwMWQyZDIxNTUiLCJpZCI6NjM2LCJpYXQiOjE1MjUzNjQ5OTN9.kXik5Mg_-01LBkN-5OTIDpwlMcuE2noRaaHrqjhbaRE";

        if (this.options.callback === undefined)
            this.options.callback = Utils.callAction;

        this.pinImageCache = {};
        this.pinBuilder = new PinBuilder();

        this.showMessageBand("Initializing map...");

        console.log("#1 imagery provider");

        Ion.defaultAccessToken = this.options.cesiumIonApiKey;

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

        this.imageryOverlayInfos = {
            ThermalSkywaysKk7: {
                layer: null,
                provider: null,
                getProvider: () => this.createThermalImageryProvider(),
                configLayer: (layer) => {
                    layer.alpha = 0.2; // 0.0 is transparent.  1.0 is opaque.
                    layer.brightness = 2.0; // > 1.0 increases brightness.  < 1.0 decreases.
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
                    layer.brightness = 2.0; // > 1.0 increases brightness.  < 1.0 decreases.
                }
            },
            WaymarkedTrailsHiking: {
                layer: null,
                provider: null,
                getProvider: () => this.createWaymarkedTrailsHikingImageryProvider(),
                configLayer: (layer) => {
                    layer.alpha = 0.8; // 0.0 is transparent.  1.0 is opaque.
                    layer.brightness = 1.0; // > 1.0 increases brightness.  < 1.0 decreases.
                }
            },
            OpenFlightMaps: {
                layer: null,
                provider: null,
                getProvider: () => {
                    const airacId = MapView.calcCurrentAiracId();
                    return new UrlTemplateImageryProvider({
                        url: "https://nwy-tiles-api.prod.newaydata.com/tiles/{z}/{x}/{y}.png?path=" + airacId + "/aero/latest",
                        tileWidth: 512,
                        tileHeight: 512,
                        maximumLevel: 11,
                        enablePickFeatures: false,
                        credit: "(c) <a href=\"https://openflightmaps.org/\" target=\"_blank\">Open Flightmaps association</a>, (c) OpenStreetMap contributors, NASA elevation data"
                    });
                },
                configLayer: () => {}
            }
        };

        console.log("#2 terrain provider");
        this.initTerrainProvider();

        console.log("#3 clock");
        const now = JulianDate.now();
        const start = JulianDate.addDays(now, -1, new JulianDate());
        const end = JulianDate.addDays(now, 1, new JulianDate());

        const clock = new Clock({
            startTime: start,
            endTime: end,
            currentTime: now.clone(),
            clockStep: ClockStep.SYSTEM_CLOCK,
            clockRange: ClockRange.CLAMPED
        });

        console.log("#4 viewer");
        const webGLPowerPreference = "low-power";

        this.imageryLayerInfos.OpenStreetMap.layer =
            new ImageryLayer(
                this.imageryLayerInfos.OpenStreetMap.provider);

        this.viewer = new Viewer(this.options.id, {
            baseLayer: this.imageryLayerInfos.OpenStreetMap.layer,
            terrainProvider: null, // is later set when readyPromise completes
            clockViewModel: new ClockViewModel(clock),
            baseLayerPicker: false,
            sceneModePicker: false,
            animation: false,
            geocoder: false,
            homeButton: false,
            fullscreenButton: false,
            timeline: false,
            skyBox: false,
            scene3DOnly: true,
            requestRenderMode: true,
            // when no animation happens, render after this number of seconds
            maximumRenderTimeChange: 60.0,
            contextOptions: {
                webgl: {
                    powerPreference: webGLPowerPreference
                }
            }
        });

        console.log("#5 globe options");

        const globe = this.viewer.scene.globe;
        globe.enableLighting = true;
        globe.backFaceCulling = false;
        globe.showSkirts = false;
        globe.dynamicAtmosphereLighting = false;

        // clip walls against terrain
        globe.depthTestAgainstTerrain = true;

        // allow scripts to run in info box
        console.log("#6 sandboxing");
        this.viewer.infoBox.frame.sandbox = this.viewer.infoBox.frame.sandbox + " allow-scripts";
        this.viewer.infoBox.frame.setAttribute("src", "about:blank"); // needed to apply new sandbox attributes

        if (!options.hasMouse) {
            // switch to Touch instructions, as the control is only used on touch devices
            this.viewer.navigationHelpButton.viewModel.showTouch();
        }

        console.log("#7 setView");
        const longitude = this.options.initialCenterPoint.longitude;
        const latitude = this.options.initialCenterPoint.latitude;

        if (longitude !== 0 && latitude !== 0) {

            const initialHeading = 0.0; // north
            const initialPitch = CesiumMath.toRadians(-35);
            const initialViewingDistance = this.options.initialViewingDistance || 5000.0;

            this.viewer.camera.setView({
                destination: Cartesian3.fromDegrees(longitude, latitude, initialViewingDistance),
                orientation: {
                    initialHeading,
                    initialPitch,
                    roll: 0.0
                }
            });
        }

        console.log("#8 location markers");
        this.initLocationMarkers();

        console.log("#9 long tap handler");

        this.pickingHandler = new ScreenSpaceEventHandler(this.viewer.scene.canvas);

        this.pickingHandler.setInputAction(this.onScreenTouchDown.bind(this), ScreenSpaceEventType.LEFT_DOWN);
        this.pickingHandler.setInputAction(this.onScreenTouchUp.bind(this), ScreenSpaceEventType.LEFT_UP);

        if (options.hasMouse) {
            this.pickingHandler.setInputAction(
                this.onScreenRightClick.bind(this),
                ScreenSpaceEventType.RIGHT_CLICK);
        }

        console.log("#10 entity clustering");
        this.setupEntityClustering();

        console.log("#11 other stuff");

        this.initCompassTarget();

        this.heightProfileView = null;

        // add a dedicated track primitives collection, as we can't call viewer.scene.primitives.removeAll()
        this.trackPrimitivesCollection = new PrimitiveCollection({
            show: true,
            destroyPrimitives: true
        });

        this.viewer.scene.primitives.add(this.trackPrimitivesCollection);

        this.trackIdToTrackDataMap = {};

        this.dataSourceMap = {};

        this.osmBuildingsTileset = null;

        this.inOnCloseHandler = false;

        this.locationDataSource = new CustomDataSource("locations");
        this.locationDataSource.clustering = this.clustering;
        this.viewer.dataSources.add(this.locationDataSource);

        this.liveTrackDataSource = new CustomDataSource("livetrack");
        this.viewer.dataSources.add(this.liveTrackDataSource);

        this.currentLiveTrackTimeOffset = -180;

        this.setupLiveTrackToolbar();

        this.setupNearbyPois();

        // swap out console.error for logging purposes
        const oldLog = console.error;
        console.error = function(message) {
            this.onConsoleErrorMessage(message);
            oldLog.apply(console, arguments);
        }.bind(this);

        this.onMapInitialized();

        this.hideMessageBand();

        console.timeEnd("ctor");
        console.groupEnd();
    }

    /**
     * Logs a message to the console, just like console.log, but with styled output.
     * @param {string} message to log
     */
    static log(message) {
        console.log("%cMapView", MapView.consoleLogStyle, message);
    }

    /**
     * Called to initialize terrain provider, which may not available when
     * offline.
     */
    async initTerrainProvider() {

        try {
            const terrainProvider = await createWorldTerrainAsync({
                requestWaterMask: false,
                requestVertexNormals: true
            });

            this.viewer.terrainProvider = terrainProvider;
            MapView.log("terrain provider is ready!");

        } catch (error) {

            // waiting for onNetworkConnectivityChanged
            console.warn("MapView.initTerrainProvider: error init'ing terrain provider, " +
                "waiting for network reconnect: " + error);
        }
    }

    /**
     * Initializes location markers.
     */
    async initLocationMarkers() {
        this.myLocationMarker = null;

        try {
            const myLocationEntity = await this.createEntity(
                undefined,
                "My Position",
                "",
                Color.GREEN,
                "images/map-marker.svg",
                0.0,
                0.0);

            myLocationEntity.show = false;
            this.myLocationMarker = this.viewer.entities.add(myLocationEntity);

        } catch (error) {
            console.error("MapView: #8 error creating my location entity: " + error);
        }

        // the zoom entity is invisible and transparent and is used for zoomToLocation() calls
        this.zoomEntity = this.viewer.entities.add({
            id: "zoomEntity",
            position: Cartesian3.fromDegrees(0.0, 0.0),
            point: {
                color: Color.TRANSPARENT,
                heightReference: HeightReference.CLAMP_TO_GROUND
            },
            show: false
        });

        // the track marker is initially invisible and only used when a track is tapped
        try {
            const trackEntity = await this.createEntity(
                "trackMarker",
                "Track point",
                "",
                Color.PURPLE,
                "images/map-marker-distance.svg",
                0.0,
                0.0);

            trackEntity.show = false;
            trackEntity.billboard.heightReference = HeightReference.NONE;
            this.trackMarker = this.viewer.entities.add(trackEntity);
        } catch (error) {
            console.error("MapView: #8 error creating find result entity: " + error);
        }

        // the find result entity is initially invisible
        try {

            const findResultEntity = await this.createEntity(
                undefined,
                "Find result",
                "",
                Color.ORANGE,
                "images/magnify.svg",
                0.0,
                0.0);
            findResultEntity.show = false;
            this.findResultMarker = this.viewer.entities.add(findResultEntity);
        } catch (error) {
            console.error("MapView: #8 error creating find result entity: " + error);
        }
    }

    /**
     * Called when the screen space event handler detected a touch-down event.
     * @param {object} movement movement info object
     */
    onScreenTouchDown(movement) {

        this.currentLeftDownPosition = movement.position;
        this.currentLeftDownTime = new Date().getTime();
    }

    /**
     * Called when the screen space event handler detected a touch-up event.
     * @param {object} movement movement info object
     */
    onScreenTouchUp(movement) {

        const deltaX = this.currentLeftDownPosition.x - movement.position.x;
        const deltaY = this.currentLeftDownPosition.y - movement.position.y;
        const deltaSquared = deltaX * deltaX + deltaY * deltaY;

        const deltaTime = new Date().getTime() - this.currentLeftDownTime;

        // check if tap was longer than 600ms and moved less than 10 pixels
        let longTapDetected = false;
        if (deltaTime > 600 && deltaSquared < 10 * 10) {

            const ray = this.viewer.camera.getPickRay(movement.position);
            const cartesian = this.viewer.scene.globe.pick(ray, this.viewer.scene);
            if (cartesian) {
                const cartographic = Cartographic.fromCartesian(cartesian);

                longTapDetected = true;
                this.onLongTap({
                    latitude: CesiumMath.toDegrees(cartographic.latitude),
                    longitude: CesiumMath.toDegrees(cartographic.longitude),
                    altitude: cartographic.height
                });
            }
        }

        // check if user tapped a track primitive
        if (!longTapDetected) {
            const feature = this.viewer.scene.pick(movement.position, 10, 10);
            if (feature !== undefined && feature.id !== undefined && typeof feature.id === "string") {
                const trackId = feature.id.replace("track-", "");
                MapView.log("picked track " + trackId);
                this.showTrackHeightProfile(trackId);
            }
        }
    }

    /**
     * Called when the screen space event handler detected a right-click event.
     * @param {object} movement movement info object
     */
    onScreenRightClick(movement) {

        const ray = this.viewer.camera.getPickRay(movement.position);
        const cartesian = this.viewer.scene.globe.pick(ray, this.viewer.scene);
        if (cartesian) {
            const cartographic = Cartographic.fromCartesian(cartesian);

            this.onLongTap({
                latitude: CesiumMath.toDegrees(cartographic.latitude),
                longitude: CesiumMath.toDegrees(cartographic.longitude),
                altitude: cartographic.height
            });
        }
    }

    /**
     * Creates a new imagery provider that uses the Thermal Skyways from https://thermal.kk7.ch/.
     * @returns {object} generated imagery provider object
     */
    async createThermalImageryProvider() {

        const url = "https://thermal.kk7.ch/tiles/skyways_all/{z}/{x}/{reverseY}.png?src=https://github.com/vividos/WhereToFly";
        MapView.log("thermal maps url: " + url);

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
     * Sets up entity clustering, showing custom pins for clustered locations when
     * too far away.
     */
    setupEntityClustering() {

        this.clustering = new EntityCluster({
            enabled: this.options.useEntityClustering,
            minimumClusterSize: 5,
            pixelRange: 40
        });

        // When EntityCluster is added to more than one DataSource, it will try to
        // destroy the EntityCluster object; prevent that here. Ugly workaround!
        // See: https://github.com/CesiumGS/cesium/issues/9336
        this.clustering.destroy = function() {
            // do nothing
        };

        const pinBuilder = new PinBuilder();
        this.clustering.pin50 = pinBuilder.fromText("50+", Color.RED, 48).toDataURL();
        this.clustering.pin40 = pinBuilder.fromText("40+", Color.RED, 48).toDataURL();
        this.clustering.pin30 = pinBuilder.fromText("30+", Color.RED, 48).toDataURL();
        this.clustering.pin20 = pinBuilder.fromText("20+", Color.RED, 48).toDataURL();
        this.clustering.pin10 = pinBuilder.fromText("10+", Color.RED, 48).toDataURL();

        this.clustering.singleDigitPins = new Array(8);
        for (let i = 0; i < this.clustering.singleDigitPins.length; ++i) {
            this.clustering.singleDigitPins[i] = pinBuilder
                .fromText("" + (i + 2), Color.RED, 48).toDataURL();
        }

        this.clustering.clusterEvent.addEventListener(
            this.onNewCluster.bind(this));
    }

    /**
     * Called when a cluster of entities will be displayed
     * @param {any} clusteredEntities list of clustered objects
     * @param {any} cluster cluster object
     */
    onNewCluster(clusteredEntities, cluster) {

        cluster.label.show = false;
        cluster.billboard.show = true;
        cluster.billboard.id = "cluster-billboard-" + cluster.label.id;
        cluster.billboard.verticalOrigin = VerticalOrigin.BOTTOM;
        cluster.billboard.heightReference = HeightReference.CLAMP_TO_GROUND;

        if (clusteredEntities.length >= 50)
            cluster.billboard.image = this.clustering.pin50;
        else if (clusteredEntities.length >= 40)
            cluster.billboard.image = this.clustering.pin40;
        else if (clusteredEntities.length >= 30)
            cluster.billboard.image = this.clustering.pin30;
        else if (clusteredEntities.length >= 20)
            cluster.billboard.image = this.clustering.pin20;
        else if (clusteredEntities.length >= 10)
            cluster.billboard.image = this.clustering.pin10;
        else {
            cluster.billboard.image =
            this.clustering.singleDigitPins[clusteredEntities.length - 2];
        }
    }

    /**
     * Sets up the live track toolbar to show when one or more live tracks are
     * present. The toolbar consists of a slider to adjust the time offset into
     * the past, a text div displaying the time offset and a button to reset the
     * time offset to the default value.
     */
    setupLiveTrackToolbar() {

        if (this.options.liveTrackToolbarId === undefined)
            return;

        const toolbar = document.getElementById(this.options.liveTrackToolbarId);

        // add title
        const titleDiv = document.createElement("div");
        titleDiv.classList.add("livetrack-toolbar-title");
        titleDiv.innerText = "Live Tracking";
        toolbar.appendChild(titleDiv);

        // add slider
        const sliderCont = document.createElement("div");
        sliderCont.classList.add("livetrack-slider-container");
        sliderCont.innerHTML = "Time offset";

        const sliderInput = document.createElement("input");
        sliderInput.type = "range";
        sliderInput.min = -900;
        sliderInput.max = 0;
        sliderInput.value = this.currentLiveTrackTimeOffset;
        sliderInput.classList.add("livetrack-slider");

        sliderInput.oninput = function() {
            const timeOffset = parseInt(this.value, 10);
            this.setLiveTrackTime(timeOffset);
        }.bind(this);
        sliderCont.appendChild(sliderInput);

        // slider text
        const sliderText = document.createElement("div");
        sliderText.id = "liveTrackSliderText";
        sliderCont.appendChild(sliderText);

        toolbar.appendChild(sliderCont);

        // add toolbar buttons
        const toolbarCont = document.createElement("div");
        toolbarCont.classList.add("livetrack-toolbar-button-container");

        // reset button
        const resetButton = document.createElement("div");
        resetButton.classList.add("livetrack-toolbar-button");
        resetButton.onclick = function() {
            this.setLiveTrackTime(-180);
            sliderInput.value = this.currentLiveTrackTimeOffset;
        }.bind(this);

        // reset button image
        const resetButtonImg = document.createElement("img");
        resetButtonImg.src = "images/timeline-clock-outline.svg";
        resetButtonImg.classList.add("livetrack-toolbar-button-image");
        resetButton.appendChild(resetButtonImg);
        toolbarCont.appendChild(resetButton);

        toolbar.appendChild(toolbarCont);
    }

    /**
     * Displays the live track toolbar, e.g. when a live track was loaded.
     */
    showLiveTrackToolbar() {

        if (this.options.liveTrackToolbarId === undefined)
            return;

        const toolbar = document.getElementById(this.options.liveTrackToolbarId);

        toolbar.style.display = "flex";
    }

    /**
     * Hides the live track toolbar, e.g. when no live tracks are displayed anymore.
     */
    hideLiveTrackToolbar() {

        if (this.options.liveTrackToolbarId === undefined)
            return;

        const toolbar = document.getElementById(this.options.liveTrackToolbarId);

        toolbar.style.display = "none";
    }

    /**
     * Updates scene by requesting rendering from scene. This can be used to
     * update the view, e.g. when a pin was added or removed, and CesiumJS
     * wouldn't update the scene itself.
     */
    updateScene() {
        this.viewer.scene.requestRender();
    }

    /**
     * Shows message band with given text.
     * @param {string} messageText message text to show
     */
    showMessageBand(messageText) {

        if (this.options.messageBandId === undefined)
            return;

        const bandElement = document.getElementById(this.options.messageBandId);
        if (bandElement === undefined)
            return;

        bandElement.style.opacity = "0";
        bandElement.style.display = "flex";

        setTimeout(function() {
            bandElement.style.opacity = "0.7";
        }, 1);

        bandElement.innerHTML = messageText;
    }

    /**
     * Hides message band again.
     */
    hideMessageBand() {

        if (this.options.messageBandId === undefined)
            return;

        const bandElement = document.getElementById(this.options.messageBandId);
        if (bandElement !== undefined) {
            bandElement.style.opacity = "0.0";

            setTimeout(function() {
                bandElement.style.display = "none";
            }, 700);
        }

    }

    /**
     * Adds the Cesium inspector window to the viewer
     */
    addViewerInspector() {
        this.viewer.extend(viewerCesiumInspectorMixin);
    }

    /**
     * Sets new map imagery type
     * @param {string} imageryType imagery type constant; the following constants currently can be
     * used: 'OpenStreetMap', 'BingMapsAerialWithLabels', 'OpenTopoMap', 'Sentinel2'.
     */
    async setMapImageryType(imageryType) {

        MapView.log("setting new imagery type: " + imageryType);

        const layers = this.viewer.scene.imageryLayers;

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
            console.warn("MapView.setMapImageryType: invalid imagery type: " + imageryType);
    }

    static slopeRamp = [0.0, 0.29, 0.5, Math.sqrt(2) / 2, 0.87, 0.91, 1.0];

    /**
     * Generates a color ramp canvas element and returns it. From
     * https://cesiumjs.org/Cesium/Build/Apps/Sandcastle/index.html?src=Globe%20Materials.html
     * @returns {object} generated canvas object
     */
    static getColorRamp() {
        const ramp = document.createElement("canvas");
        ramp.width = 100;
        ramp.height = 1;
        const ctx = ramp.getContext("2d");

        const values = MapView.slopeRamp;

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
     * https://cesiumjs.org/Cesium/Build/Apps/Sandcastle/index.html?src=Globe%20Materials.html
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
        shadingUniforms.image = MapView.getColorRamp();

        this.slopeAndContourLinesMaterial = material;
    }

    /**
     * Sets new map overlay type
     * @param {string} overlayType overlay type constant; the following constants currently can be
     * used: 'None', 'ContourLines', 'SlopeAndContourLines', 'ThermalSkywaysKk7', 'BlackMarble',
     * 'WaymarkedTrailsHiking', 'OpenFlightMaps'.
     */
    async setMapOverlayType(overlayType) {

        MapView.log("setting new map overlay type: " + overlayType);

        const layers = this.viewer.scene.imageryLayers;

        for (const key in this.imageryOverlayInfos) {
            if (this.imageryOverlayInfos[key].layer !== null)
                layers.remove(this.imageryOverlayInfos[key].layer, false);
        }

        this.viewer.scene.globe.material = null;

        switch (overlayType) {
        case "None":
            break;

        case "ContourLines":
            this.viewer.scene.globe.material = this.contourLinesMaterial;
            break;

        case "SlopeAndContourLines":
            this.viewer.scene.globe.material = this.slopeAndContourLinesMaterial;
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
                console.warn("MapView.setMapOverlayType: invalid map overlay type: " + overlayType);
            break;
        }

        this.updateScene();
    }

    /**
     * Sets new map shading mode
     * @param {string} shadingMode shading mode constant; the following constants currently can be
     * used: 'Fixed10Am', 'Fixed3Pm', 'CurrentTime', 'Ahead6Hours' and 'None'.
     */
    setShadingMode(shadingMode) {

        this.lastShadingMode = shadingMode;

        if (this.viewer.scene.requestRenderMode === false) {
            MapView.log("ignored setting shading mode; currently showing live track(s)");
            return;
        }

        MapView.log("setting new shading mode: " + shadingMode);

        switch (shadingMode) {
        case "Fixed10Am":
        case "Fixed3Pm":
            this.setupFixedClock(shadingMode === "Fixed10Am" ? 10 : 15);
            break;

        case "CurrentTime":
            this.setupCurrentTimeClock();
            break;

        case "Ahead6Hours":
            this.setupAheadCurrentTimeClock(6);
            break;

        case "None":
            this.viewer.clockViewModel.shouldAnimate = false;
            break;

        default:
            console.warn("MapView.setShadingMode: invalid shading mode: " + shadingMode);
            break;
        }

        this.viewer.scene.globe.enableLighting = shadingMode !== "None";

        this.viewer.terrainShadows =
            shadingMode === "None" ? ShadowMode.DISABLED : ShadowMode.RECEIVE_ONLY;

        this.updateScene();
    }

    /**
     * Sets up a fixed clock for the viewer, using given hour.
     * @param {number} hour hour of day to use for fixed clock
     */
    setupFixedClock(hour) {
        const today = new Date();
        today.setHours(hour, 0, 0, 0);
        const fixedTime = JulianDate.fromDate(today);

        this.viewer.clockViewModel.startTime = fixedTime;
        this.viewer.clockViewModel.currentTime = fixedTime.clone();
        this.viewer.clockViewModel.endTime = fixedTime.clone();
        this.viewer.clockViewModel.clockStep = 0;
        this.viewer.clockViewModel.shouldAnimate = false;
    }

    /**
     * Sets up a clock that follows the current time.
     */
    setupCurrentTimeClock() {
        const now = JulianDate.now();
        const start = JulianDate.addDays(now, -1, new JulianDate());
        const end = JulianDate.addDays(now, 1, new JulianDate());

        this.viewer.clockViewModel.startTime = start;
        this.viewer.clockViewModel.currentTime = now.clone();
        this.viewer.clockViewModel.endTime = end;
        this.viewer.clockViewModel.clockStep = ClockStep.SYSTEM_CLOCK;
        this.viewer.clockViewModel.shouldAnimate = true;
    }

    /**
     * Sets up a clock that is a number of hours ahead from current time.
     * @param {number} hours number of hours the clock is ahead
     */
    setupAheadCurrentTimeClock(hours) {
        const now = JulianDate.now();
        const ahead = JulianDate.addHours(now, hours, new JulianDate());
        const end = JulianDate.addDays(ahead, 1, new JulianDate());

        this.viewer.clockViewModel.startTime = ahead;
        this.viewer.clockViewModel.currentTime = ahead.clone();
        this.viewer.clockViewModel.endTime = end;
        this.viewer.clockViewModel.clockStep = ClockStep.SYSTEM_CLOCK;
        this.viewer.clockViewModel.shouldAnimate = true;
    }

    /**
     * Sets if entity clustering is used
     * @param {boolean} useEntityClustering true when entity clustering should be used
     */
    setEntityClustering(useEntityClustering) {

        this.options.useEntityClustering = useEntityClustering;
        this.clustering.enabled = useEntityClustering;

        if (useEntityClustering) {
            // force re-clustering
            const lastPixelRange = this.clustering.pixelRange;
            this.clustering.pixelRange = 0;
            this.clustering.pixelRange = lastPixelRange;
        }

        this.updateScene();
    }

    /**
     * Sets up nearby POIs data source and camera event handler
     */
    setupNearbyPois() {

        this.viewer.camera.moveEnd.addEventListener(function() {

            const rect = this.getViewRectangle();

            MapView.log("camera moved to: west: " + rect.west +
                ", south: " + rect.south +
                ", east: " + rect.east +
                ", north: " + rect.north);

        }.bind(this));

        this.nearbyPoisDataSource = new CustomDataSource("nearbyPois");
        this.viewer.dataSources.add(this.nearbyPoisDataSource);
    }

    /**
     * Returns the current camera view rectangle
     * @returns {object} current view rectangle
     */
    getViewRectangle() {

        const currentViewRectangle =
            this.viewer.camera.computeViewRectangle(
                this.viewer.scene.globe.ellipsoid);

        return {
            west: CesiumMath.toDegrees(currentViewRectangle.west).toFixed(5),
            south: CesiumMath.toDegrees(currentViewRectangle.south).toFixed(5),
            east: CesiumMath.toDegrees(currentViewRectangle.east).toFixed(5),
            north: CesiumMath.toDegrees(currentViewRectangle.north).toFixed(5)
        };
    }

    /**
     * Adds nearby POI locations
     * @param {Array} nearbyPoisList list of nearby POIs to add
     */
    async addNearbyPoiLocations(nearbyPoisList) {

        MapView.log("adding nearby POIs list, with " + nearbyPoisList.length + " entries");

        for (const index in nearbyPoisList) {

            const location = nearbyPoisList[index];

            try {
                const entity = await this.createEntityFromLocation(location);
                this.nearbyPoisDataSource.entities.add(entity);

            } catch (error) {
                console.error("MapView.addNearbyPoiLocations: error while adding location entity: " + error);
            }
        }

        this.updateScene();
    }

    /**
     * Updates the "my location" marker on the map
     * @param {object} [options] Options to use for updating "my location" pin.
     * @param {number} [options.latitude] Latitude of position
     * @param {number} [options.longitude] Longitude of position
     * @param {number} [options.displayLatitude] Display text of latitude
     * @param {number} [options.displayLongitude] Display text of longitude
     * @param {number} [options.altitude] Altitude of position
     * @param {number} [options.speed] Current speed, in km/h
     * @param {number} [options.displaySpeed] Display text for current speed
     * @param {string} [options.timestamp] Timestamp of position, as parseable date string
     * @param {string} [options.displayTimestamp] Display text of timestamp
     * @param {number} [options.positionAccuracy] Accuracy of position, in meter
     * @param {string} [options.positionAccuracyColor] Hex color in format #rrggbb for position accuracy
     * @param {boolean} [options.zoomToLocation] indicates if view should also zoom to this position
     */
    updateMyLocation(options) {

        if (this.myLocationMarker === null) {
            console.warn("MapView.updateMyLocation: myLocationMarker not initialized yet");
            return;
        }

        MapView.log("updating my location: lat=" + options.latitude + ", long=" + options.longitude);

        this.myLocationMarker.show = true;
        this.myLocationMarker.position = Cartesian3.fromDegrees(options.longitude, options.latitude);

        let text = "<h2><img height=\"48em\" width=\"48em\" src=\"images/map-marker.svg\" style=\"vertical-align:middle\" />My Position</h2>" +
            "<div>Latitude: " + options.displayLatitude + "<br/>" +
            "Longitude: " + options.displayLongitude + "<br/>" +
            "Accuracy: <span style=\"color:" + options.positionAccuracyColor + "\">+/- " + options.positionAccuracy + " m</span><br/>" +
            MapView.formatAltitude(options.altitude, "Altitude: ", " m<br/>") +
            "Speed: " + options.displaySpeed + "<br/>" +
            "Time: " + options.displayTimestamp +
            "</div>";

        text += "<img height=\"32em\" width=\"32em\" src=\"images/share-variant.svg\" style=\"vertical-align:middle\" />" +
            "<a href=\"javascript:parent.map.onShareMyLocation();\">Share position</a></p>";

        this.myLocationMarker.description = text;

        if (options.zoomToLocation) {
            MapView.log("also zooming to my location");
            this.flyTo(options);
        } else
            this.updateScene();
    }

    /**
     * Formats altitude value as text; handles special cases where altitude is
     * not set or is 0, which corresponds to "clamp to ground". When altitude
     * is set, the prefix text is prepended and altitude is formatted with one
     * fractional digit.
     * @param {number} altitude altitude value in meter
     * @param {string} prefixText prefix text
     * @param {string} suffixText suffix text; optional
     * @returns {string} formatted text, or empty string
     */
    static formatAltitude(altitude, prefixText, suffixText) {
        if (typeof altitude === "undefined" ||
            altitude === null ||
            altitude === 0)
            return "";

        suffixText = suffixText || "";
        return prefixText + altitude.toFixed(1) + suffixText;
    }

    /**
     * Initializes compass target variables and entity
     */
    async initCompassTarget() {

        this.compassTargetLocation = null;
        this.compassTargetHideTargetLocation = false;

        const pinImage = "images/map-marker.svg";
        const pinColor = Color.fromCssColorString("#00ffd5");
        const pinImageDataUrl = await this.getPinImageDataUrl(pinImage, pinColor);

        this.compassTargetEntity = this.viewer.entities.add({
            name: "Compass target",
            position: this.compassTargetLocation,
            billboard: {
                show: false,
                image: pinImageDataUrl,
                heightReference: HeightReference.NONE,
                verticalOrigin: VerticalOrigin.BOTTOM
            },
            polyline: {
                show: false,
                clampToGround: true,
                width: 4,
                material: pinColor,
                positions: [
                    Cartesian3.ZERO,
                    Cartesian3.ZERO
                ]
            }
        });
    }

    /**
     * Sets a compass target, to draw a ground polyline from the current location to the target
     * location. The polyline is shown once the "my location" is set using updateMyLocation().
     * @param {object} [options] Options to use for showing compass target.
     * @param {number} [options.title] Title of the compass target
     * @param {number} [options.latitude] Latitude of position
     * @param {number} [options.longitude] Longitude of position
     * @param {number} [options.altitude] Altitude of position; optional
     * @param {string} [options.displayLatitude] Display text of latitude
     * @param {string} [options.displayLongitude] Display text of longitude
     * @param {number} [options.distanceInKm] Distance to target, in km
     * @param {number} [options.heightDifferenceInMeter] Altitude difference, in meter
     * @param {number} [options.directionAngle] Direction angle, in degrees
     * @param {boolean} [options.zoomToPolyline] when true, also zooms to the compass target
     * @param {boolean} [options.hideTargetLocation] when true, the target location is not shown
     * polyline
     */
    async setCompassTarget(options) {

        MapView.log("setting new compass target: lat=" +
            options.latitude + ", long=" + options.longitude +
            MapView.formatAltitude(options.altitude, ", alt="));

        this.compassTargetHideTargetLocation = options.hideTargetLocation;
        this.compassTargetLocation =
            Cartesian3.fromDegrees(options.longitude, options.latitude, options.altitude);

        this.setCompassTargetPositions();
        this.updateCompassTargetDescription(options);

        if (options.zoomToPolyline &&
            this.myLocationMarker !== null &&
            this.myLocationMarker.show &&
            this.compassTargetEntity !== null &&
            this.compassTargetEntity.show)
            await this.zoomToCompassTarget();
    }

    /**
     * Sets compass target entity position values based on the currently set compass target
     * location and the current "my location" position.
     */
    setCompassTargetPositions() {

        const polylinePositions = [null, null];

        if (this.compassTargetLocation !== null) {

            this.compassTargetEntity.show = true;

            this.compassTargetEntity.position = this.compassTargetLocation;

            this.compassTargetEntity.billboard.show =
                !this.compassTargetHideTargetLocation;

            polylinePositions[0] = this.compassTargetLocation;
        }

        if (this.myLocationMarker !== null &&
            this.myLocationMarker.show) {
            polylinePositions[1] =
                this.myLocationMarker.position.getValue(this.viewer.clock.currentTime);
        }

        if (polylinePositions[0] !== null &&
            polylinePositions[1] !== null) {

            this.compassTargetEntity.show = true;

            this.compassTargetEntity.polyline.positions = polylinePositions;
            this.compassTargetEntity.polyline.show = true;
        }

        this.updateScene();
    }

    /**
     * Zooms to the compass target entity
     */
    async zoomToCompassTarget() {

        MapView.log("flying to compass target");

        const boundingSphere = BoundingSphere.fromPoints(
            this.compassTargetEntity.polyline.positions.getValue(
                this.viewer.clock.currentTime));

        this.viewer.camera.flyToBoundingSphere(boundingSphere, {
            offset: new HeadingPitchRange(
                this.viewer.camera.heading,
                this.viewer.camera.pitch,
                0.0),
            complete: function() {
                MapView.log("flying to compass target finished");

                const center = Cartographic.fromCartesian(boundingSphere.center);

                this.onUpdateLastShownLocation({
                    latitude: CesiumMath.toDegrees(center.latitude),
                    longitude: CesiumMath.toDegrees(center.longitude),
                    altitude: center.height,
                    viewingDistance: this.getCurrentViewingDistance()
                });
            }.bind(this)
        });
    }

    /**
     * Updates description of compass target entity based on the currently set compass target
     * location and the current "my location" position.
     * @param {object} [options] Options to use for updating compass target description.
     * @param {number} [options.title] Title of the compass target
     * @param {number} [options.latitude] Latitude of position
     * @param {number} [options.longitude] Longitude of position
     * @param {number} [options.altitude] Altitude of position
     * @param {string} [options.displayLatitude] Display text of latitude
     * @param {string} [options.displayLongitude] Display text of longitude
     * @param {number} [options.distanceInKm] Distance to target, in km
     * @param {number} [options.heightDifferenceInMeter] Altitude difference, in meter
     * @param {number} [options.directionAngle] Direction angle, in degrees
     */
    updateCompassTargetDescription(options) {

        if (options !== null) {
            let text = "<h2><img height=\"48em\" width=\"48em\" src=\"images/compass-rose.svg\" style=\"vertical-align:middle\" />" +
                "Compass target: " + options.title + "</h2>";

            text += "<p><img height=\"32em\" width=\"32em\" src=\"images/close-circle-outline.svg\" style=\"vertical-align:middle\" />" +
                "<a href=\"javascript:parent.map.onSetLocationAsCompassTarget(null);\">Hide</a></p>";

            if ("displayLatitude" in options &&
                "displayLongitude" in options) {
                text += "<div>Latitude: " + options.displayLatitude + "<br/>" +
                    "Longitude: " + options.displayLongitude + "<br/>";
            }

            text +=
                MapView.formatAltitude(options.altitude, "Altitude: ", " m<br/>") +
                (options.distanceInKm !== null ? "Distance: " + options.distanceInKm.toFixed(1) + " km<br/>" : "") +
                (options.heightDifferenceInMeter !== null ? "Height difference: " + options.heightDifferenceInMeter.toFixed(0) + " m<br/>" : "") +
                (options.directionAngle !== null ? "Direction: " + options.directionAngle.toFixed(0) + "&deg;<br/>" : "") +
                "</div>";

            this.compassTargetEntity.description = text;
        }
    }

    /**
     * Clears showing compass target or direction, hiding the ground polyline
     */
    clearCompass() {

        MapView.log("hiding compass target");

        if (this.compassTargetEntity !== null) {
            this.compassTargetEntity.show = false;
            this.updateScene();
        }
    }

    /**
     * Returns the current viewing distance from the camera to the terrain in the
     * center of the scene.
     * @returns {number} viewing distance
     */
    getCurrentViewingDistance() {

        const width = this.viewer.container.clientWidth;
        const height = this.viewer.container.clientHeight;

        const ray = this.viewer.camera.getPickRay(new Cartesian2(width / 2, height / 2));
        const position = this.viewer.scene.globe.pick(ray, this.viewer.scene);

        if (position !== undefined)
            return Cartesian3.distance(this.viewer.camera.positionWC, position);

        return 5000.0;
    }

    /**
     * Flies to to given location
     * @param {object} [options] Options to use for zooming
     * @param {number} [options.latitude] Latitude of zoom target
     * @param {number} [options.longitude] Longitude of zoom target
     * @param {number} [options.altitude] Altitude of zoom target; optional
     */
    async flyTo(options) {

        if (this.zoomEntity === undefined) {
            console.warn("MapView.zoomToLocation: zoomEntity not initialized yet");
            return;
        }

        MapView.log("flying to: latitude=" + options.latitude +
            ", longitude=" + options.longitude +
            MapView.formatAltitude(options.altitude, ", altitude="));

        let altitude = options.altitude || 0.0;
        if (altitude === 0.0)
            altitude = await this.findAltitude(options);

        let viewingDistance = this.getCurrentViewingDistance();

        if (viewingDistance > 10000.0)
            viewingDistance = 10000.0;

        // zooming works by assinging the zoom entity a new position, making it
        // visible (but transparent), fly there and hiding it again
        this.zoomEntity.position = Cartesian3.fromDegrees(options.longitude, options.latitude, altitude);

        this.zoomEntity.point.heightReference =
            altitude === 0.0 ? HeightReference.CLAMP_TO_GROUND : HeightReference.NONE;

        this.zoomEntity.show = true;

        await this.viewer.flyTo(
            this.zoomEntity,
            {
                offset: new HeadingPitchRange(
                    this.viewer.camera.heading,
                    this.viewer.camera.pitch,
                    viewingDistance)
            });

        this.zoomEntity.show = false;
        MapView.log("flying finished");

        this.onUpdateLastShownLocation({
            latitude: options.latitude,
            longitude: options.longitude,
            altitude,
            viewingDistance: this.getCurrentViewingDistance()
        });
    }

    /**
     * Zooms to given location, by flying to the location
     * @param {object} [options] Options to use for zooming
     * @param {number} [options.latitude] Latitude of zoom target
     * @param {number} [options.longitude] Longitude of zoom target
     * @param {number} [options.altitude] Altitude of zoom target
     */
    zoomToLocation(options) {

        MapView.log("zooming to location at: " +
            "latitude=" + options.latitude + ", longitude=" + options.longitude +
            MapView.formatAltitude(options.altitude, ", altitude="));

        this.flyTo(options);
    }

    /**
     * Zooms to given map rectangle
     * @param {object} [rectangle] map rectangle to zoom to
     * @param {number} [rectangle.minLatitude] minimum latitude
     * @param {number} [rectangle.maxLatitude] maximum latitude
     * @param {number} [rectangle.minLongitude] minimum longitude
     * @param {number} [rectangle.maxLongitude] maximum longitude
     * @param {number} [rectangle.minAltitude] minimum altitude
     * @param {number} [rectangle.maxAltitude] maximum altitude
     */
    zoomToRectangle(rectangle) {

        MapView.log("start flying to rectangle");

        const corner = Cartesian3.fromDegrees(rectangle.minLongitude, rectangle.minLatitude, rectangle.minAltitude);
        const oppositeCorner = Cartesian3.fromDegrees(rectangle.maxLongitude, rectangle.maxLatitude, rectangle.maxAltitude);

        const boundingSphere = BoundingSphere.fromCornerPoints(corner, oppositeCorner);

        this.viewer.camera.flyToBoundingSphere(boundingSphere, {
            offset: new HeadingPitchRange(
                this.viewer.camera.heading,
                this.viewer.camera.pitch,
                0.0),
            complete: function() {
                MapView.log("flying to rectangle finished");

                const center = Cartographic.fromCartesian(boundingSphere.center);

                this.onUpdateLastShownLocation({
                    latitude: CesiumMath.toDegrees(center.latitude),
                    longitude: CesiumMath.toDegrees(center.longitude),
                    altitude: center.height,
                    viewingDistance: this.getCurrentViewingDistance()
                });
            }.bind(this)
        });
    }

    /**
     * Adds a new layer to the map
     * @param {object} [layer] Layer object to add
     * @param {string} [layer.id] ID of layer
     * @param {string} [layer.name] Layer name
     * @param {string} [layer.type] Layer type
     * @param {boolean} [layer.isVisible] Indicates if layer is visible
     * @param {string} [layer.data] CZML data of layer
     */
    async addLayer(layer) {

        MapView.log("adding layer " + layer.name + ", with type " + layer.type);

        if (layer.type === "LocationLayer" || layer.type === "TrackLayer") {
            this.setLayerVisibility(layer);
            return;
        }

        if (layer.type === "OsmBuildingsLayer") {
            if (this.osmBuildingsTileset === null)
                this.osmBuildingsTileset = await createOsmBuildingsAsync();

            this.viewer.scene.primitives.add(this.osmBuildingsTileset);

            this.setLayerVisibility(layer);
            return;
        }

        MapView.log("layer data length: " + layer.data.length + " bytes");

        const czml = JSON.parse(layer.data);

        try {
            const dataSource = await CzmlDataSource.load(czml);

            // don't set clustering on CZML data sources, since the object can't be shared;
            // see https://github.com/CesiumGS/cesium/issues/9336
            // dataSource.clustering = this.clustering;
            this.viewer.dataSources.add(dataSource);
            this.dataSourceMap[layer.id] = dataSource;

            this.setLayerVisibility(layer);

        } catch (error) {
            console.error("MapView.addLayer: error while loading CZML data source: " + error);
        }
    }

    /**
     * Gets the bounding sphere of a data source
     * @param {DataSource} [dataSource] data source
     * @returns {BoundingSphere} bounding sphere object, or null when no
     * bounding sphere could be determined
     */
    getDataSourceBoundingSphere(dataSource) {

        const entities = dataSource.entities.values;
        MapView.log("getting bounding spheres of " + entities.length + " entities");

        const boundingSphereScratch = new BoundingSphere();

        const boundingSpheres = [];
        for (let i = 0, len = entities.length; i < len; i++) {
            try {
                const state = this.viewer.dataSourceDisplay.getBoundingSphere(entities[i], false, boundingSphereScratch);

                if (state === BoundingSphereState.PENDING)
                    continue;
                else if (state !== BoundingSphereState.FAILED)
                    boundingSpheres.push(BoundingSphere.clone(boundingSphereScratch));
            } catch (error) {
                console.warn("MapView.getDataSourceBoundingSphere: " + error);
            }
        }

        if (boundingSpheres.length === 0)
            return null;

        return BoundingSphere.fromBoundingSpheres(boundingSpheres);
    }

    /**
     * Zooms to layer with given layer ID
     * @param {string} [layerId] ID of layer
     */
    zoomToLayer(layerId) {

        MapView.log("zooming to layer with id " + layerId);

        if (layerId === "osmBuildingsLayer") {

            console.warn("can't zoom to OSM buildings layer");
            return;
        }

        let dataSource;
        if (layerId === "locationLayer")
            dataSource = this.locationDataSource;
        else if (layerId === "trackLayer")
            dataSource = this.trackPrimitivesCollection;
        else
            dataSource = this.dataSourceMap[layerId];

        if (dataSource !== undefined) {

            this.viewer.flyTo(dataSource);

            const boundingSphere = this.getDataSourceBoundingSphere(dataSource);
            if (boundingSphere !== null) {
                const center = Cartographic.fromCartesian(boundingSphere.center);

                this.onUpdateLastShownLocation({
                    latitude: CesiumMath.toDegrees(center.latitude),
                    longitude: CesiumMath.toDegrees(center.longitude),
                    altitude: center.height,
                    viewingDistance: this.getCurrentViewingDistance()
                });
            }
        }
    }

    /**
     * Sets layer visibility
     * @param {object} [layer] Layer object to set visibility for
     * @param {string} [layer.id] ID of layer
     * @param {boolean} [layer.isVisible] Indicates if layer is visible
     */
    setLayerVisibility(layer) {

        MapView.log("setting new visibility for layer with id " + layer.id +
            ", visibility: " + layer.isVisible);

        if (layer.id === "locationLayer")
            this.locationDataSource.show = layer.isVisible;
        else if (layer.id === "trackLayer")
            this.trackPrimitivesCollection.show = layer.isVisible;
        else if (layer.id === "osmBuildingsLayer")
            this.osmBuildingsTileset.show = layer.isVisible;
        else {
            const dataSource = this.dataSourceMap[layer.id];
            if (dataSource !== undefined)
                dataSource.show = layer.isVisible;
        }

        this.updateScene();
    }

    /**
     * Exports layer with given ID to KMZ bytestream
     * @param {string} layerId layer ID of layer to export
     */
    async exportLayer(layerId) {

        MapView.log("exporting layer with id " + layerId);

        if (layerId === "locationLayer" ||
            layerId === "trackLayer" ||
            layerId === "osmBuildingsLayer") {
            console.warn("MapView: can't export layer of type " + layerId);
            this.onExportLayer(null);
            return;
        }

        const dataSource = this.dataSourceMap[layerId];
        if (dataSource === undefined)
            return;

        try {
            const result = await exportKml({
                entities: dataSource.entities,
                kmz: true
            });

            this.onExportLayer(result.kmz);

        } catch (error) {
            console.error("MapView.exportLayer: " + error);
            this.onExportLayer(null);
        }
    }

    /**
     * Removes layer with given layer ID
     * @param {string} [layerId] ID of layer
     */
    removeLayer(layerId) {

        MapView.log("removing layer with id " + layerId);

        if (layerId === "osmBuildingsLayer" && this.osmBuildingsTileset !== null) {
            this.viewer.scene.primitives.remove(this.osmBuildingsTileset);
            this.osmBuildingsTileset = null;

            this.updateScene();
            return;
        }

        const dataSource = this.dataSourceMap[layerId];
        if (dataSource !== undefined) {
            this.viewer.dataSources.remove(dataSource);
            this.dataSourceMap[layerId] = undefined;

            // force re-clustering
            const lastPixelRange = this.clustering.pixelRange;
            this.clustering.pixelRange = 0;
            this.clustering.pixelRange = lastPixelRange;
        }

        this.updateScene();
    }

    /**
     * Clears list of layers
     */
    clearLayerList() {

        MapView.log("clearing layer list");

        if (this.osmBuildingsTileset !== null) {
            this.viewer.scene.primitives.remove(this.osmBuildingsTileset);
            this.osmBuildingsTileset = null;
        }

        for (const layerId in this.dataSourceMap) {
            const dataSource = this.dataSourceMap[layerId];
            if (dataSource !== undefined)
                this.viewer.dataSources.remove(dataSource);
        }

        this.dataSourceMap = {};

        // force re-clustering
        const lastPixelRange = this.clustering.pixelRange;
        this.clustering.pixelRange = 0;
        this.clustering.pixelRange = lastPixelRange;

        this.updateScene();
    }

    /**
     * Clears list of locations
     */
    clearLocationList() {

        MapView.log("clearing location list");

        this.locationDataSource.entities.removeAll();
    }

    /**
     * Formats description text for a location
     * @param {Location} [location] Location to format text for
     * @returns {string} formatted location text
     */
    formatLocationText(location) {

        const altitudeText =
            MapView.formatAltitude(location.altitude, " ", " m");

        let text = "<h2><img height=\"48em\" width=\"48em\" src=\"" + this.imageUrlFromLocationType(location.type) + "\" style=\"vertical-align:middle\" />" +
            location.name + altitudeText + "</h2>";

        text += "<p><img height=\"32em\" width=\"32em\" src=\"images/information-outline.svg\" style=\"vertical-align:middle\" /> " +
            "<a href=\"javascript:parent.map.onShowLocationDetails('" + location.id + "');\">Show details</a> ";

        text += "<img height=\"32em\" width=\"32em\" src=\"images/compass-rose.svg\" style=\"vertical-align:middle\" /> " +
            "<a href=\"javascript:parent.map.onSetLocationAsCompassTarget('" + location.id + "');\">Set as compass target</a> ";

        text += "<img height=\"32em\" width=\"32em\" src=\"images/directions.svg\" style=\"vertical-align:middle\" /> " +
            "<a href=\"javascript:parent.map.onNavigateToLocation('" + location.id + "');\">Navigate here</a>";

        text += "<img height=\"32em\" width=\"32em\" src=\"images/map-marker-plus.svg\" style=\"vertical-align:middle\" /> " +
            "<a href=\"javascript:parent.map.onAddTourPlanLocation('" + location.id + "');\">Plan tour</a>";

        text += "</p><p>" + location.description + "</p>";

        return text;
    }

    /**
     * @typedef {Location} Location object
     * @property {string} id ID of the location
     * @property {string} type Location type
     * @property {string} name Location name
     * @property {string} description Location description text
     * @property {number} latitude Latitude of the location
     * @property {number} longitude Longitude of the location
     * @property {number} altitude Altitude of the location
     * @property {number} takeoffDirections Takeoff directions bit values
     * @property {boolean} isPlanTourLocation Indicates if it's a tour planning
     * location known to the backend
     * @property {object} properties An object containing extra location
     * properties as string keys and string values
     */

    /**
     * Adds list of locations to the map, as marker pins
     * @param {Array} locationList An array of location, each with the following object layout:
     * { id:"location-id", name:"Location Name", type:"LocationType", latitude: 123.45678, longitude: 9.87654, altitude:1234.5 }
     */
    async addLocationList(locationList) {

        MapView.log("adding location list, with " + locationList.length + " entries");
        console.time("MapView.addLocationList");

        for (const index in locationList) {

            const location = locationList[index];

            if (location.id === undefined) {
                console.warn("MapView: ignored adding location without ID");
                continue;
            }

            try {
                const entity = await this.createEntityFromLocation(location);
                this.locationDataSource.entities.add(entity);

            } catch (error) {
                console.error("MapView.addLocationList: error while adding location entity: " + error);
            }
        }

        this.updateScene();

        console.timeEnd("MapView.addLocationList");
    }

    /**
     * Creates entity from location object
     * @param {Location} location to use
     * @returns {Entity} created entity
     */
    async createEntityFromLocation(location) {

        const text = this.formatLocationText(location);

        const imagePath = this.imageUrlFromLocationType(location.type);

        const altitudeText = MapView.formatAltitude(location.altitude, " ", " m");

        const entity = await this.createEntity(
            location.id,
            location.name + altitudeText,
            text,
            this.pinColorFromLocationType(location.type),
            imagePath,
            location.longitude,
            location.latitude);

        if (location.takeoffDirections !== undefined && location.takeoffDirections !== 0)
            this.addTakeoffEntities(entity, location);

        return entity;
    }

    /**
     * Creates an entity object with given informations that can be placed into
     * the entities list.
     * @param {string} id Unique ID of the entity; may be 'undefined'
     * @param {string} name Name of the entity
     * @param {string} description Longer description text
     * @param {string} pinColor Pin color, one of the Color.Xxx constants
     * @param {string} pinImage Relative link URL to SVG image to use in pin
     * @param {number} longitude Longitude of entity
     * @param {number} latitude Latitude of entity
     * @returns {Promise<object>} entity description, usable for viewer.entities.add()
     */
    async createEntity(id, name, description, pinColor, pinImage, longitude, latitude) {

        const pinImageDataUrl = await this.getPinImageDataUrl(pinImage, pinColor);

        try {
            return {
                id,
                name,
                description,
                position: Cartesian3.fromDegrees(longitude, latitude),
                billboard: {
                    image: pinImageDataUrl,
                    verticalOrigin: VerticalOrigin.BOTTOM,
                    heightReference: HeightReference.CLAMP_TO_GROUND,
                    disableDepthTestDistance: 5000.0
                }
            };

        } catch (error) {
            console.error("MapView.createEntity: error while generating pin from URL " + pinImageDataUrl + ": " + error);
        }
    }

    /**
     * Creates a "data:" URL containing the pin image and a pin background color
     * @param {string} pinImage relative pin image filename
     * @param {Color} pinColor pin background color
     * @returns {string} a "data:" URI
     */
    async getPinImageDataUrl(pinImage, pinColor) {

        const cacheKey = pinImage + "|" + pinColor;

        if (cacheKey in this.pinImageCache)
            return this.pinImageCache[cacheKey];

        const url = getAbsoluteUri(pinImage, document.baseURI);
        const canvas = await this.pinBuilder.fromUrl(url, pinColor, 48);
        const dataUrl = canvas.toDataURL();

        this.pinImageCache[cacheKey] = dataUrl;

        return dataUrl;
    }

    /**
     * Returns a relative image Url for given location type
     * @param {string} locationType location type
     * @returns {string} relative image Url
     */
    imageUrlFromLocationType(locationType) {

        switch (locationType) {
        case "Summit": return "images/mountain-15.svg";
        case "Pass": return "images/mountain-pass.svg";
        case "Lake": return "images/water-15.svg";
        case "Bridge": return "images/bridge.svg";
        case "Viewpoint": return "images/attraction-15.svg";
        case "AlpineHut": return "images/home-15.svg";
        case "Restaurant": return "images/restaurant-15.svg";
        case "Church": return "images/church.svg";
        case "Castle": return "images/castle.svg";
        case "Cave": return "images/cave.svg";
        case "Information": return "images/information-outline.svg";
        case "PublicTransportBus": return "images/bus.svg";
        case "PublicTransportTrain": return "images/train.svg";
        case "Parking": return "images/parking.svg";
        case "Webcam": return "images/camera-outline.svg";
            // case 'ViaFerrata': return '';
        case "CableCar": return "images/aerialway-15.svg";
        case "FlyingTakeoff": return "images/paragliding.svg";
        case "FlyingLandingPlace": return "images/paragliding.svg";
        case "FlyingWinchTowing": return "images/paragliding.svg";
            // case 'Turnpoint': return '';
        case "Thermal": return "images/weather-partly-cloudy.svg";
        case "MeteoStation": return "images/cloud-upload-outline-modified.svg";
        case "LiveWaypoint": return "images/autorenew.svg";
        default: return "images/map-marker.svg";
        }
    }

    /**
     * Returns a pin color for given location type
     * @param {string} locationType location type
     * @returns {string} Color constant
     */
    pinColorFromLocationType(locationType) {

        switch (locationType) {
        case "FlyingTakeoff": return Color.YELLOWGREEN;
        case "FlyingLandingPlace": return Color.ORANGE;
        case "FlyingWinchTowing": return Color.CORNFLOWERBLUE;
        case "Turnpoint": return Color.RED;
        default: return Color.BLUE;
        }
    }

    /**
     * Calculates a point on a circle around a given center.
     * Note: Most of the code is adapted from code in Cesium's EllipseGeometryLibrary.js
     * @param {Cartesian3} [center] Center point of the circle
     * @param {number} [radius] radius of the circle, in meter
     * @param {number} [angleDegrees] angle of point on circle, in degrees
     * @returns {Cartesian3} calculated point coordinates
     */
    static pointFromCenterRadiusAngle(center, radius, angleDegrees) {

        const unitPosScratch = new Cartesian3();
        const unitPos = Cartesian3.normalize(center, unitPosScratch);

        const eastVecScratch = new Cartesian3();
        let eastVec = Cartesian3.cross(Cartesian3.UNIT_Z, center, eastVecScratch);
        eastVec = Cartesian3.normalize(eastVec, eastVec);

        const northVecScratch = new Cartesian3();
        const northVec = Cartesian3.cross(unitPos, eastVec, northVecScratch);

        const azimuth = CesiumMath.toRadians(angleDegrees);

        const rotAxis = new Cartesian3();
        const tempVec = new Cartesian3();
        Cartesian3.multiplyByScalar(eastVec, Math.cos(azimuth), rotAxis);
        Cartesian3.multiplyByScalar(northVec, Math.sin(azimuth), tempVec);
        Cartesian3.add(rotAxis, tempVec, rotAxis);

        const mag = Cartesian3.magnitude(center);
        const angle = radius / mag;

        // Create the quaternion to rotate the position vector to the boundary of the ellipse.
        const unitQuat = new Quaternion();
        Quaternion.fromAxisAngle(rotAxis, angle, unitQuat);

        const rotMtx = new Matrix3();
        Matrix3.fromQuaternion(unitQuat, rotMtx);

        const result = new Cartesian3();
        Matrix3.multiplyByVector(rotMtx, unitPos, result);
        Cartesian3.normalize(result, result);

        Cartesian3.multiplyByScalar(result, mag, result);
        return result;
    }

    /**
     * Adds a polyline and polygon entity visualizing the takeoff directions of
     * the given location, to an existing entity.
     * @param {object} [entity] Entity object to add to
     * @param {Location} [location] An location object
     */
    addTakeoffEntities(entity, location) {

        const center = Cartesian3.fromDegrees(location.longitude, location.latitude);
        const radius = 50.0; // in meter

        const pointArray = [];
        const takeoffBits = location.takeoffDirections;
        const sliceAngle = 360.0 / 16.0;

        pointArray.push(center);

        // from the bits, calculate the takeoff angle and add polygon points
        for (let angleBit = 0; angleBit <= 16; angleBit++) {
            const bitmask = 1 << angleBit;
            const angle = 180.0 - angleBit * sliceAngle;

            if ((takeoffBits & bitmask) !== 0) {
                pointArray.push(MapView.pointFromCenterRadiusAngle(center, radius, angle - sliceAngle / 2.0));
                pointArray.push(MapView.pointFromCenterRadiusAngle(center, radius, angle));
                pointArray.push(MapView.pointFromCenterRadiusAngle(center, radius, angle + sliceAngle / 2.0));
                pointArray.push(center);
            }
        }

        const distanceDisplayCondition =
            new DistanceDisplayCondition(0.0, 5000.0);

        entity.polyline = {
            positions: pointArray,
            width: 3.0,
            material: new Color(1.0, 1.0, 0.5, 0.4), // light yellow
            clampToGround: true,
            distanceDisplayCondition
        };

        entity.polygon = {
            // note: clamping to terrain is achieved by not specifying height and heightReference at all
            hierarchy: new PolygonHierarchy(pointArray),
            material: new Color(0.0, 0.0, 0.54, 0.4), // dark blue
            outline: false, // when an outline would be present, it would not clamp to ground
            distanceDisplayCondition
        };
    }

    /**
     * Updates a single location
     * @param {Location} [location] Location object to update
     */
    async updateLocation(location) {

        MapView.log("updating location \"" + location.id +
            "\", new position at at latitude " + location.latitude +
            ", longitude " + location.longitude +
            ", altitude " + location.altitude);

        const entity = this.locationDataSource.entities.getById(location.id);

        if (entity === undefined) {
            console.error("MapView.updateLocation: couldn't find entity for id: " + location.id);
            return;
        }

        entity.position = Cartesian3.fromDegrees(location.longitude, location.latitude, location.altitude);

        entity.name = location.name;
        entity.description = this.formatLocationText(location);

        const pinImage = this.imageUrlFromLocationType(location.type);
        const pinColor = this.pinColorFromLocationType(location.type);
        const pinImageDataUrl = await this.getPinImageDataUrl(pinImage, pinColor);

        entity.billboard.image = pinImageDataUrl;

        this.updateScene();
    }

    /**
     * Removes a single location from map
     * @param {string} [locationId] ID of the location to remove
     */
    removeLocation(locationId) {

        MapView.log("removing location with ID: " + locationId);

        const entity = this.locationDataSource.entities.getById(locationId);

        this.locationDataSource.entities.remove(entity);

        this.updateScene();
    }

    /**
     * Shows a find result pin, with a link to add a waypoint for this result.
     * @param {object} [options] An object with the following properties:
     * @param {string} [options.name] Name of the find result
     * @param {string} [options.description] Description text of the find result
     * @param {number} [options.latitude] Latitude of the find result
     * @param {number} [options.longitude] Longitude of the find result
     * @param {number} [options.altitude] Altitude of the find result; optional
     * @param {number} [options.displayLatitude] Display text for latitude
     * @param {number} [options.displayLongitude] Display text for longitude
     */
    async showFindResult(options) {

        if (this.findResultMarker === undefined) {
            console.warn("MapView.showFindResult: findResultMarker not initialized yet");
            return;
        }

        MapView.log("showing find result for \"" + options.name +
            "\", at latitude " + options.latitude + ", longitude " + options.longitude);

        if (!("altitude" in options) ||
            options.altitude === null ||
            options.altitude === 0.0)
            options.altitude = await this.findAltitude(options);

        let text = "<h2><img height=\"48em\" width=\"48em\" src=\"images/magnify.svg\" style=\"vertical-align:middle\" />" + options.name + "</h2>" +
            "<div>Latitude: " + options.displayLatitude + "<br/>" +
            "Longitude: " + options.displayLongitude + "<br/>" +
            "Altitude: " + options.altitude.toFixed(1) + " m" +
            "</div>";

        const optionsText = "{ name: '" + options.name +
            "', latitude:" + options.latitude +
            ", longitude:" + options.longitude +
            ", altitude:" + options.altitude.toFixed(1) + "}";

        text += "<p><img height=\"32em\" width=\"32em\" src=\"images/map-marker-plus.svg\" style=\"vertical-align:middle\" />" +
            "<a href=\"javascript:parent.map.onAddFindResult(" + optionsText + ");\">Add as waypoint</a>";

        text += "<img height=\"32em\" width=\"32em\" src=\"images/close-circle-outline.svg\" style=\"vertical-align:middle\" />" +
            "<a href=\"javascript:parent.map.hideFindResult();\">Hide</a></p>";

        if (options.description !== undefined)
            text += "<div>" + options.description + "</div>";

        this.findResultMarker.description = text;
        this.findResultMarker.position = Cartesian3.fromDegrees(options.longitude, options.latitude);
        this.findResultMarker.show = true;

        await this.flyTo(options);
    }

    /**
     * Finds altitude at latitude and longitude, or returns 0.0 when the
     * altitude couldn't be determined.
     * @param {object} [options] Options for finding altitude:
     * @param {number} [options.latitude] Latitude of the point to use
     * @param {number} [options.longitude] Longitude of the point to use
     * @returns {Promise<number>|number} altitude in meter, or 0.0
     */
    async findAltitude(options) {

        MapView.log("finding altitude at latitude " + options.latitude +
            ", longitude " + options.longitude);

        try {
            const samples = await sampleTerrainMostDetailed(
                this.viewer.terrainProvider,
                [Cartographic.fromDegrees(options.longitude, options.latitude)]);

            const altitude = samples[0].height;

            MapView.log("findAltitude: altitude is " + MapView.formatAltitude(altitude, "", " m"));

            return altitude;

        } catch (error) {
            console.error("findAltitude: error while finding altitude: " + error);
            return 0.0;
        }
    }

    /**
     * Shows flying range for a given map point and using flying range parameters.
     * @param {object} [options] Options for showing flying range:
     * @param {number} [options.latitude] Latitude of the point to show flying range
     * @param {number} [options.longitude] Longitude of the point to show flying range
     * @param {number} [options.displayLatitude] Display text of latitude
     * @param {number} [options.displayLongitude] Display text of longitude
     * @param {number} [options.altitude] Altitude of the point to show flying range
     * @param {number} [options.glideRatio] Glide ratio, in km flying per 1000m sink
     * @param {number} [options.gliderSpeed] Glider speed, in km/h; must be above wind speed
     * @param {number} [options.windDirection] Wind direction, in degrees
     * @param {number} [options.windSpeed] Wind speed, in km/h
     */
    showFlyingRange(options) {

        if (this.flyingRangeCone !== null)
            this.viewer.entities.remove(this.flyingRangeCone);

        // limit wind speed so that we don't get a negative glide angle
        if (options.windSpeed > options.gliderSpeed - 1)
            options.windSpeed = options.gliderSpeed - 1;

        // use wind speed to calculate the angle to pitch the cone
        const glideAngle = Math.atan(options.glideRatio);
        const glideRatioWithWind = options.glideRatio * (options.gliderSpeed - options.windSpeed) / options.gliderSpeed;
        const glideAngleWithWind = Math.atan(glideRatioWithWind);
        const conePitch = glideAngle - glideAngleWithWind;

        const quat = Transforms.headingPitchRollQuaternion(
            Cartesian3.fromDegrees(options.longitude, options.latitude, options.altitude / 2.0),
            new HeadingPitchRoll(
                CesiumMath.toRadians(options.windDirection + 90.0),
                conePitch, 0.0)
        );

        let text = "<p><img height=\"32em\" width=\"32em\" src=\"images/close-circle-outline.svg\" style=\"vertical-align:middle\" />" +
            "<a href=\"javascript:parent.map.hideFlyingRangeCone();\">Hide</a></p>";

        text += "<p>Flying range for map point at<br/>Latitude: " + options.displayLatitude + "<br/>" +
            "Longitude: " + options.displayLongitude + "<br/>" +
            MapView.formatAltitude(options.altitude, "Altitude: ", " m</p>");

        text += "<p>" +
            "Glide ratio: " + options.glideRatio + "<br/>" +
            "Glide angle: " + (90.0 - CesiumMath.toDegrees(glideAngle)).toFixed(1) + " degrees<br/>" +
            // 'Glider speed: ' + options.gliderSpeed + ' km/h<br/>' +
            // 'Glide ratio with wind: ' + glideRatioWithWind.toFixed(1) + '<br/>' +
            // 'Glide angle with wind: ' + (90.0 - CesiumMath.toDegrees(glideAngleWithWind)).toFixed(1) + ' degrees<br/>' +
            // 'Wind: ' + options.windSpeed + ' km/h from ' + options.windDirection + ' degrees' +
            "</p>";

        this.flyingRangeCone = this.viewer.entities.add({
            name: "Flying range",
            description: text,
            position: Cartesian3.fromDegrees(options.longitude, options.latitude, options.altitude / 2.0),
            orientation: quat,
            cylinder: {
                length: options.altitude,
                topRadius: 0.0,
                bottomRadius: options.altitude * options.glideRatio,
                numberOfVerticalLines: 18,
                material: Color.BLUE.withAlpha(0.4),
                outline: true,
                outlineColor: Color.WHITE
            }
        });

        this.viewer.flyTo(this.flyingRangeCone,
            {
                offset: new HeadingPitchRange(
                    this.viewer.camera.heading,
                    this.viewer.camera.pitch,
                    20 * 1000.0)
            });
    }

    /**
     * Samples track point heights from actual map and adjusts the track when it goes below terrain
     * height.
     * @param {object} [track] Track object to add
     * @param {string} [track.id] unique ID of the track
     * @param {Array} [track.listOfTrackPoints] An array of track points in long, lat, alt, long, lat, alt ... order
     */
    async sampleTrackHeights(track) {

        MapView.log("sampleTrackHeights: #1 start sampling track point heights for " + track.listOfTrackPoints.length + " points...");
        console.time("MapView.sampleTrackHeights");

        if (!Entity.supportsPolylinesOnTerrain(this.viewer.scene)) {
            console.warn("MapView.sampleTrackHeights: #2: polylines on terrain are not supported");
            return;
        }

        const trackPointArray = Cartesian3.fromDegreesArrayHeights(track.listOfTrackPoints);

        const cartographicArray = [];
        for (const trackPoint of trackPointArray)
            cartographicArray.push(Cartographic.fromCartesian(trackPoint));

        MapView.log("sampleTrackHeights: #3: waiting for terrain provider to be ready");

        try {
            await this.viewer.terrainProvider.readyPromise;

        } catch (error) {
            console.error("MapView.sampleTrackHeights: #8: error while waiting for terrain provider promise: " + error);
            this.onSampledTrackHeights(null);

            console.timeEnd("MapView.sampleTrackHeights");
            return;
        }

        MapView.log("sampleTrackHeights: #4: terrain provider is ready; starting sampling terrain");

        try {
            const samples = await sampleTerrainMostDetailed(
                this.viewer.terrainProvider,
                cartographicArray);

            MapView.log("sampleTrackHeights: #5: terrain provider reports back " + samples.length + " samples");

            const trackPointHeightArray = [];

            for (const sampledValue of samples) {
                const sampledHeight = sampledValue.height;
                trackPointHeightArray.push(sampledHeight);
            }

            MapView.log("sampleTrackHeights: #6: sampling track point heights finished.");

            this.onSampledTrackHeights(trackPointHeightArray);

            console.timeEnd("MapView.sampleTrackHeights");

        } catch (error) {
            console.error("MapView.sampleTrackHeights: #9: error while sampling track point heights: " + error);
            this.onSampledTrackHeights(null);
            console.timeEnd("MapView.sampleTrackHeights");
        }

        MapView.log("sampleTrackHeights: #7: call to sampleTrackHeights() returns.");
    }

    /**
     * Called when sampling track points has finished.
     * @param {Array} [listOfTrackPointHeights] An array of track point heights for all track points
     * that were passed to sampleTrackHeights().
     */
    onSampledTrackHeights(listOfTrackPointHeights) {

        MapView.log("sampling track heights has finished");

        if (this.options.callback !== undefined)
            this.options.callback("onSampledTrackHeights", listOfTrackPointHeights);
    }

    /**
     * Calculates track color from given variometer climb/sink rate value.
     * @param {number} varioValue variometer value, in m/s
     * @returns {Color} Track color
     */
    trackColorFromVarioValue(varioValue) {

        const varioColorMapping = [
            5.0, Color.RED,
            4.5, Color.fromBytes(255, 64, 0),
            4.0, Color.fromBytes(255, 128, 0),
            3.5, Color.fromBytes(255, 192, 0),
            3.0, Color.YELLOW,
            2.5, Color.fromBytes(192, 255, 0),
            2.0, Color.fromBytes(128, 255, 0),
            1.5, Color.fromBytes(64, 255, 128),
            1.0, Color.CYAN,
            0.5, Color.fromBytes(0, 224, 255),
            0.0, Color.fromBytes(0, 192, 255),
            -0.5, Color.fromBytes(0, 160, 255),
            -1.0, Color.fromBytes(0, 128, 255),
            -1.5, Color.fromBytes(0, 96, 224),
            -2.0, Color.fromBytes(0, 64, 192),
            -3.0, Color.fromBytes(0, 32, 160),
            -3.5, Color.fromBytes(0, 0, 128),
            -4.0, Color.fromBytes(64, 0, 128)
        ];

        for (let mappingIndex = 0; mappingIndex < varioColorMapping.length; mappingIndex += 2) {
            if (varioValue >= varioColorMapping[mappingIndex])
                return varioColorMapping[mappingIndex + 1];
        }

        return Color.fromBytes(128, 0, 128); // smaller than -4.0
    }

    /**
     * Calculates an array of track colors based on the altitude changes of the given track points.
     * @param {Array} listOfTrackPoints An array of track points in long, lat, alt, long, lat, alt ... order
     * @param {Array} listOfTimePoints An array of time points in seconds; same length as listOfTrackPoints; may be null
     * @returns {Array} Array with same number of entries as track points in the given list
     */
    calcTrackColors(listOfTrackPoints, listOfTimePoints) {

        const trackColors = [];

        trackColors[0] = this.trackColorFromVarioValue(0.0);

        for (let index = 3; index < listOfTrackPoints.length; index += 3) {

            const altitudeDiff = listOfTrackPoints[index + 2] - listOfTrackPoints[index - 1];

            let timeDiff = 1.0;
            if (listOfTimePoints !== null)
                timeDiff = listOfTimePoints[index / 3] - listOfTimePoints[index / 3 - 1];

            const varioValue = altitudeDiff / timeDiff;

            const varioColor = this.trackColorFromVarioValue(varioValue);

            if (varioColor === undefined)
                MapView.log("undefined color for vario value " + varioValue);

            trackColors[index / 3] = varioColor;
        }

        return trackColors;
    }

    /**
     * Creates a primitive for a flight track
     * @param {object} [track] Track object to add
     * @param {Array} [track.listOfTrackPoints] An array of track points in long, lat, alt, long, lat, alt ... order
     * @param {Array} [track.listOfTimePoints] An array of time points in seconds; same length as listOfTrackPoints; may be null
     * @param {Array} [trackPointArray] An array of track points to use
     * @returns {Primitive} created primitive object
     */
    getFlightTrackPrimitive(track, trackPointArray) {

        const trackColors = this.calcTrackColors(track.listOfTrackPoints, track.listOfTimePoints);

        const trackPolyline = new PolylineGeometry({
            positions: trackPointArray,
            width: 5,
            vertexFormat: PolylineColorAppearance.VERTEX_FORMAT,
            colors: trackColors,
            colorsPerVertex: false
        });

        const primitive = new Primitive({
            asynchronous: this.options.useAsynchronousPrimitives,
            geometryInstances: new GeometryInstance({
                id: "track-" + track.id,
                geometry: trackPolyline
            }),
            appearance: new PolylineColorAppearance({ translucent: false })
        });

        return primitive;
    }

    /**
     * Creates a wall primitive for a flight track to display relation to ground
     * @param {string} [trackId] unique ID of the track
     * @param {Array} [trackPointArray] An array of track points to use
     * @returns {Primitive} created wall primitive object
     */
    getFlightTrackWallPrimitive(trackId, trackPointArray) {

        const wallGeometry = new WallGeometry({
            positions: trackPointArray
        });

        const wallMaterial = Material.fromType("Color");

        wallMaterial.uniforms.color = new Color(0.5, 0.5, 1, 0.4);

        const wallPrimitive = new Primitive({
            asynchronous: this.options.useAsynchronousPrimitives,
            geometryInstances: new GeometryInstance({
                id: "track-" + trackId,
                geometry: wallGeometry
            }),
            appearance: new MaterialAppearance({
                translucent: true,
                material: wallMaterial,
                faceForward: true
            })
        });

        return wallPrimitive;
    }

    /**
     * Creates a ground primitive for a non-flight track
     * @param {object} [track] Track object to add
     * @param {string} [track.id] unique ID of the track
     * @param {string} [track.color] Color as "RRGGBB" string value
     * @param {Array} [trackPointArray] An array of track points to use
     * @returns {Primitive} created primitive object
     */
    getGroundTrackPrimitive(track, trackPointArray) {

        const groundTrackPolyline = new GroundPolylineGeometry({
            positions: trackPointArray,
            width: 5
        });

        const primitive = new GroundPolylinePrimitive({
            asynchronous: this.options.useAsynchronousPrimitives,
            geometryInstances: new GeometryInstance({
                id: "track-" + track.id,
                geometry: groundTrackPolyline,
                attributes: {
                    color: ColorGeometryInstanceAttribute.fromColor(Color.fromCssColorString("#" + track.color))
                }
            }),
            appearance: new PolylineColorAppearance({ translucent: false })
        });

        return primitive;
    }

    /**
     * Adds a track to the map
     * @param {object} [track] Track object to add
     * @param {string} [track.id] unique ID of the track
     * @param {string} [track.name] track name to add
     * @param {boolean} [track.isFlightTrack] indicates if track is a flight
     * @param {boolean} [track.isLiveTrack] indicates if track is a live track
     * that is updated periodically
     * @param {Array} [track.listOfTrackPoints] An array of track points in long,
     * lat, alt, long, lat, alt ... order
     * @param {Array} [track.listOfTimePoints] An array of time points in seconds;
     * same length as listOfTrackPoints.length / 3; may be null
     * @param {number} track.trackStart track start, in seconds from epoch or as
     * ISO8601 string; optional. This is used to display correct time ticks in
     * the height profile view
     * @param {Array} [track.groundHeightProfile] An array of ground height
     * profile elevations; same length as listOfTimePoints; may be null
     * @param {string} [track.color] Color as "RRGGBB" string value, or undefined
     * when track should be colored according to climb and sink rate.
     */
    async addTrack(track) {

        this.removeTrack(track.id);

        MapView.log("adding list of track points, with ID " + track.id + " and " + track.listOfTrackPoints.length + " track points");

        this.trackIdToTrackDataMap[track.id] = {
            track,
            primitive: undefined,
            wallPrimitive: undefined,
            boundingSphere: undefined
        };

        this.addOrUpdateTrackPrimitives(track);

        if (track.isLiveTrack) {
            await this.addLiveTrackEntity(track);
            this.viewer.scene.requestRenderMode = false;

            this.showLiveTrackToolbar();

            this.setLiveTrackTime(this.currentLiveTrackTimeOffset);
        }
    }

    /**
     * Adds or updates track primitives to display ground track or flight track + wall. When the
     * primitives already exist, the primitives are recreated with the current track data
     * @param {object} [track] Track object to use
     * @param {string} [track.id] unique ID of the track
     * @param {boolean} [track.isFlightTrack] indicates if track is a flight
     * @param {Array} [track.listOfTrackPoints] An array of track points in long,
     * lat, alt, long, lat, alt ... order
     * @param {Array} [track.listOfTimePoints] An array of time points in seconds;
     * same length as listOfTrackPoints.length / 3; may be null
     */
    addOrUpdateTrackPrimitives(track) {

        let trackPointArray = track.listOfTrackPoints.length > 0
            ? Cartesian3.fromDegreesArrayHeights(track.listOfTrackPoints)
            : [];

        // remove duplicates so that color values are calculated correctly
        if (track.isFlightTrack)
            trackPointArray = this.removeTrackDuplicatePoints(track, trackPointArray);

        // need at least 2 points for the track primitives
        if (trackPointArray.length < 2)
            return;

        const trackData = this.trackIdToTrackDataMap[track.id];

        if (trackData.boundingSphere !== undefined) {
            // remove the existing primitives
            if (trackData.primitive !== undefined)
                this.trackPrimitivesCollection.remove(trackData.primitive);

            if (trackData.wallPrimitive !== undefined)
                this.trackPrimitivesCollection.remove(trackData.wallPrimitive);
        }

        // create new primitives
        if (track.isFlightTrack) {
            trackData.primitive = this.getFlightTrackPrimitive(track, trackPointArray);
            trackData.wallPrimitive = this.getFlightTrackWallPrimitive(track.id, trackPointArray);

            this.trackPrimitivesCollection.add(trackData.wallPrimitive);

        } else {
            trackData.primitive =
                this.getGroundTrackPrimitive(track, trackPointArray);
        }

        if (trackData.primitive !== undefined)
            this.trackPrimitivesCollection.add(trackData.primitive);

        trackData.boundingSphere = BoundingSphere.fromPoints(trackPointArray, null);
    }

    /**
     * Adds a live track entity for the given track object
     * @param {object} [track] track object, with at least the following properties:
     * @param {string} [track.id] unique ID of the track
     * @param {string} [track.name] track name to add
     * @param {string} [track.description] track description
     * @param {string} [track.color] Color as "RRGGBB" string value, for trailing path
     */
    async addLiveTrackEntity(track) {

        const pinColor = this.pinColorFromLocationType("LiveWaypoint");
        const pinImage = this.imageUrlFromLocationType("LiveWaypoint");

        const pinImageDataUrl = await this.getPinImageDataUrl(pinImage, pinColor);

        try {
            const sampledPos = new SampledPositionProperty();
            sampledPos.forwardExtrapolationType = ExtrapolationType.HOLD;
            sampledPos.setInterpolationOptions({
                interpolationAlgorithm: LagrangePolynomialApproximation,
                interpolationDegree: 3
            });

            const showLabelProperty = new TimeIntervalCollectionProperty();
            showLabelProperty.intervals.addInterval(
                new TimeInterval({
                    start: JulianDate.now,
                    stop: JulianDate.addDays(JulianDate.now, 365, new JulianDate()),
                    data: false // label is not visible
                }));

            const entityOptions = {
                id: "livetrackpoint-" + track.id,
                name: track.name,
                description: track.description,
                position: sampledPos,
                billboard: {
                    image: pinImageDataUrl,
                    heightReference: HeightReference.NONE,
                    verticalOrigin: VerticalOrigin.BOTTOM,
                    disableDepthTestDistance: 5000.0
                },
                path: {
                    leadTime: 0,
                    trailTime: 15 * 60,
                    resolution: 60,
                    width: 3,
                    material: Color.fromCssColorString("#" + track.color)
                },
                label: {
                    text: new ConstantProperty("out of current track data"),
                    show: showLabelProperty,
                    font: "14pt sans-serif",
                    style: LabelStyle.FILL_AND_OUTLINE,
                    fillColor: Color.WHITE,
                    showBackground: false,
                    outlineWidth: 10.0,
                    outlineColor: Color.BLACK,
                    pixelOffset: new Cartesian2(64, 0),
                    heightReference: HeightReference.NONE,
                    disableDepthTestDistance: 5000.0
                }
            };

            const entity = this.liveTrackDataSource.entities.add(entityOptions);

            const trackData = this.trackIdToTrackDataMap[track.id];
            trackData.liveTrackEntity = entity;

        } catch (error) {
            console.error("MapView.addLiveTrackEntity: error while generating pin from URL " + pinImageDataUrl + ": " + error);
        }
    }

    /**
     * Updates track infos like name and color
     * @param {object} [track] Track object to update, with at least the following properties:
     * @param {string} [track.id] unique ID of the track
     * @param {string} [track.name] track name to add
     * @param {boolean} [track.isFlightTrack] indicates if track is a flight
     * @param {string} [track.color] Color as "RRGGBB" string value, or undefined
     * when track should be colored according to climb and sink rate.
     */
    updateTrack(track) {

        const trackInfos = this.trackIdToTrackDataMap[track.id];
        if (trackInfos !== undefined) {
            trackInfos.track.name = track.name;

            if (!track.isFlightTrack) {
                const attributes = trackInfos.primitive.getGeometryInstanceAttributes(
                    "track-" + track.id);

                attributes.color = ColorGeometryInstanceAttribute.toValue(
                    Color.fromCssColorString("#" + track.color),
                    attributes.color);
            }

            this.updateScene();
        }
    }

    /**
     * Removes duplicate track points, e.g. when the track position hasn't changed
     * for several seconds. This is needed since CesiumJS removes duplicate
     * position values from tracks, but doesn't remove per-vertex color values.
     * See also: https://github.com/CesiumGS/cesium/issues/9379
     * @param {object} track Track object to modify, with at least the following
     * properties:
     * @param {Array} [track.listOfTrackPoints] An array of track points in long,
     * lat, alt, long, lat, alt ... order
     * @param {Array} [track.listOfTimePoints] An array of time points in seconds;
     * same length as listOfTrackPoints.length / 3; may be null
     * @param {Array} [track.groundHeightProfile] An array of ground height
     * profile elevations; same length as listOfTimePoints; may be null
     * @param {Array} [trackPointArray] An array of track points to modify
     * @returns {Array} new trackPointArray array
     */
    removeTrackDuplicatePoints(track, trackPointArray) {

        // add index to every track point
        for (let trackPointIndex = 0; trackPointIndex < trackPointArray.length; trackPointIndex++)
            trackPointArray[trackPointIndex].trackPointIndex = trackPointIndex;

        const modifiedTrackPointArray = arrayRemoveDuplicates(trackPointArray, Cartesian3.equalsEpsilon);

        if (trackPointArray.length === modifiedTrackPointArray.length)
            return trackPointArray; // nothing was removed

        const removedTrackPoints = trackPointArray.length - modifiedTrackPointArray.length;
        MapView.log("removed " + removedTrackPoints + " duplicate track points from track");

        const newListOfTrackPoints = [];
        const newListOfTimePoints = [];
        const newGroundHeightProfile = [];

        const hasListOfTimePoints = track.listOfTimePoints !== null;

        const hasGroundHeightProfile = track.groundHeightProfile !== null;

        for (const modifiedTrackPoint of modifiedTrackPointArray) {
            const oldTrackPointIndex = modifiedTrackPoint.trackPointIndex;

            newListOfTrackPoints.push(track.listOfTrackPoints[oldTrackPointIndex * 3]);
            newListOfTrackPoints.push(track.listOfTrackPoints[oldTrackPointIndex * 3 + 1]);
            newListOfTrackPoints.push(track.listOfTrackPoints[oldTrackPointIndex * 3 + 2]);

            if (hasListOfTimePoints)
                newListOfTimePoints.push(track.listOfTimePoints[oldTrackPointIndex]);

            if (hasGroundHeightProfile)
                newGroundHeightProfile.push(track.groundHeightProfile[oldTrackPointIndex]);
        }

        track.listOfTrackPoints = newListOfTrackPoints;

        if (hasListOfTimePoints)
            track.listOfTimePoints = newListOfTimePoints;

        if (hasGroundHeightProfile)
            track.groundHeightProfile = newGroundHeightProfile;

        return modifiedTrackPointArray;
    }

    /**
     * When getting back live track data from the web API, it uses a different
     * format for track points; convert here to the track format.
     * @param {object} track Track object to modify
     * @param {number} track.trackStart track start, in seconds from epoch or as
     * ISO8601 string
     * @param {Array} track.trackPoints track points, with latitude, longitude,
     * altitude and offset values
     */
    convertResponseDataToTrack(track) {

        const trackStart = typeof track.trackStart === "string"
            ? Math.floor(new Date(track.trackStart).getTime() / 1000.0)
            : track.trackStart;

        track.listOfTrackPoints = [];
        track.listOfTimePoints = [];

        for (const trackPointIndex in track.trackPoints) {

            const trackPoint = track.trackPoints[trackPointIndex];

            track.listOfTrackPoints.push(trackPoint.longitude);
            track.listOfTrackPoints.push(trackPoint.latitude);
            track.listOfTrackPoints.push(trackPoint.altitude);

            track.listOfTimePoints.push(trackStart + trackPoint.offset);
        }

        track.trackStart = undefined;
        track.trackPoints = undefined;
    }

    /**
     * Updates a live track with new track points. The track points are displayed
     * using a Path entity object. If the height profile view is currently open,
     * also add the track points there and update the view.
     * @param {object} track Track object to modify, with at least the following
     * @param {string} track.id unique ID of the track to add more track points
     * properties:
     * @param {Array} [track.listOfTrackPoints] An array of track points in long,
     * lat, alt, long, lat, alt ... order
     * @param {Array} [track.listOfTimePoints] An array of time points in seconds;
     * same length as listOfTrackPoints.length / 3; must not be null
     * Also the following properties can be set, which will be converted to
     * listOfTrackPoints and listOfTimePoints internally:
     * @param {number} track.trackStart track start, in seconds from epoch or as
     * ISO8601 string
     * @param {Array} track.trackPoints track points, with latitude, longitude,
     * altitude and offset values
     */
    updateLiveTrack(track) {

        if (track.trackStart !== undefined)
            this.convertResponseDataToTrack(track);

        if (!(track.id in this.trackIdToTrackDataMap)) {
            console.warn("called updateLiveTrack(), but track wasn't added with addTrack() yet");
            return;
        }

        const trackData = this.trackIdToTrackDataMap[track.id];

        if (trackData.liveTrackEntity === undefined) {
            console.warn("called updateLiveTrack(), but track is no live track");
            return;
        }

        track.color = trackData.track.color;
        track.isFlightTrack = trackData.track.isFlightTrack;
        track.isLiveTrack = trackData.track.isLiveTrack;

        trackData.liveTrackEntity.name = track.name;
        trackData.liveTrackEntity.description = track.description;

        // add new track points to track and wall primitives
        this.mergeLiveTrackPoints(track);
        this.addOrUpdateTrackPrimitives(track);

        // add points to path entity
        const trackPointArray = Cartesian3.fromDegreesArrayHeights(track.listOfTrackPoints);

        const julianTimePoints = [];
        for (const timePoint in track.listOfTimePoints) {
            julianTimePoints.push(
                JulianDate.fromDate(
                    new Date(timePoint * 1000.0)));
        }

        const lastTimePoint = track.listOfTimePoints[track.listOfTimePoints.length - 1];

        MapView.log("added new track points, from " +
            new Date(track.listOfTimePoints[0] * 1000.0) +
            " to " +
            new Date(lastTimePoint * 1000.0));

        const sampledPos = trackData.liveTrackEntity.position;
        sampledPos.addSamples(julianTimePoints, trackPointArray);

        // update visibility and text of label
        const showLabelProperty = trackData.liveTrackEntity.label.show;
        showLabelProperty.intervals.addInterval(
            new TimeInterval({
                start: julianTimePoints[0],
                stop: julianTimePoints[julianTimePoints.length - 1],
                data: false // label is not visible
            }));

        const text = "out of data\nsince " +
            new Date(lastTimePoint * 1000.0).toTimeString().substring(0, 8);

        trackData.liveTrackEntity.label.text =
            new ConstantProperty(text);

        // update height profile when shown
        if (this.heightProfileView !== null &&
            track.id === this.currentHeightProfileTrackId) {
            this.heightProfileView.addTrackPoints(track);
            this.heightProfileView.updateView();
        }
    }

    /**
     * Merges the track points in given track object with track points already
     * stored in the trackData mapping. Used for live tracking.
     * @param {object} track Track object to use
     * @param {string} track.id unique ID of the track to add more track points
     * @param {Array} [track.listOfTrackPoints] An array of track points in long,
     * lat, alt, long, lat, alt ... order
     * @param {Array} [track.listOfTimePoints] An array of time points in seconds;
     * same length as listOfTrackPoints.length / 3; must not be null
     */
    mergeLiveTrackPoints(track) {

        const trackData = this.trackIdToTrackDataMap[track.id];

        if (track.listOfTrackPoints.length === 0 ||
            track.listOfTrackPoints.length !== track.listOfTimePoints.length * 3)
            return;

        const startTimePoint = track.listOfTimePoints[0];

        const timePos = trackData.track.listOfTimePoints.indexOf(startTimePoint);

        const removedTrackPoints = (timePos === -1 ? "no " : (trackData.track.listOfTimePoints.length - timePos) + " ");
        MapView.log("removing " + removedTrackPoints +
            "live track points and adding " + track.listOfTimePoints.length + " new track points");

        if (timePos !== -1) {
            const trackPos = timePos * 3;
            trackData.track.listOfTrackPoints.splice(trackPos, trackData.track.listOfTrackPoints.length - trackPos);
            trackData.track.listOfTimePoints.splice(timePos, trackData.track.listOfTimePoints.length - timePos);
        }

        trackData.track.listOfTrackPoints = trackData.track.listOfTrackPoints.concat(track.listOfTrackPoints);
        trackData.track.listOfTimePoints = trackData.track.listOfTimePoints.concat(track.listOfTimePoints);

        track.listOfTrackPoints = trackData.track.listOfTrackPoints;
        track.listOfTimePoints = trackData.track.listOfTimePoints;
    }

    /**
     * Returns the time of the last (latest) track point of a track.
     * @param {string} trackId track ID of track
     * @returns {number} track point time, in seconds since epoch, or null when no track
     * points are available yet
     */
    getTrackLastTrackPointTime(trackId) {

        const trackData = this.trackIdToTrackDataMap[trackId];

        if (trackData === undefined ||
            trackData.track === undefined ||
            trackData.track.listOfTimePoints.length === 0)
            return null;

        return trackData.track.listOfTimePoints[trackData.track.listOfTimePoints.length - 1];
    }

    /**
     * Sets a new time offset from current time ("now")
     * @param {number} timeOffset time offset from now, in seconds; usually
     * negative, since most live tracking data is sent from the past
     */
    setLiveTrackTime(timeOffset) {

        this.currentLiveTrackTimeOffset = timeOffset;

        const now = JulianDate.now();
        const liveTrackTime = JulianDate.addSeconds(now, timeOffset, new JulianDate());
        const end = JulianDate.addDays(now, 1, new JulianDate());

        // note: set clockStep before currentTime, or it will be reset
        this.viewer.clock.clockStep = ClockStep.SYSTEM_CLOCK;
        this.viewer.clock.stopTime = end.clone();
        this.viewer.clock.currentTime = liveTrackTime.clone();

        // also update text
        if (this.options.liveTrackToolbarId === undefined)
            return;

        const liveTrackSliderText = document.getElementById("liveTrackSliderText");

        if (timeOffset === 0)
            liveTrackSliderText.innerHTML = "now";
        else
            liveTrackSliderText.innerHTML = "-" + new Date(-timeOffset * 1000).toTimeString().substring(3, 8);

    }

    /**
     * Zooms to the current position of a live waypoint with given location ID
     * @param {string} locationId unique ID of the live waypoint to zoom to
     */
    zoomToLiveWaypointCurrentPos(locationId) {

        const entity = this.locationDataSource.entities.getById(locationId);

        if (entity === undefined) {
            console.error("MapView: couldn't find entity for live waypoint id: " + locationId);
            return;
        }

        const position = entity.position.getValue(this.viewer.clock.currentTime);
        const location = Cartographic.fromCartesian(position);

        this.zoomToLocation({
            longitude: CesiumMath.toDegrees(location.longitude),
            latitude: CesiumMath.toDegrees(location.latitude),
            altitude: location.height
        });
    }

    /**
     * Zooms to a track on the map
     * @param {string} trackId unique ID of the track to zoom to
     */
    zoomToTrack(trackId) {

        const trackData = this.trackIdToTrackDataMap[trackId];

        if (trackData !== undefined) {

            MapView.log("zooming to track with ID: " + trackId);

            if (trackData.liveTrackEntity !== undefined) {

                const sampledPos = trackData.liveTrackEntity.position;

                const currentPosCartesian = sampledPos.getValue(this.viewer.clockViewModel.currentTime, new Cartesian3());
                if (currentPosCartesian !== undefined) {
                    const currentPos = Cartographic.fromCartesian(currentPosCartesian);

                    this.flyTo({
                        latitude: CesiumMath.toDegrees(currentPos.latitude),
                        longitude: CesiumMath.toDegrees(currentPos.longitude),
                        altitude: currentPos.height
                    });
                }
            } else if (trackData.boundingSphere !== undefined) {

                this.viewer.camera.flyToBoundingSphere(trackData.boundingSphere);

                const center = Cartographic.fromCartesian(trackData.boundingSphere.center);

                this.onUpdateLastShownLocation({
                    latitude: CesiumMath.toDegrees(center.latitude),
                    longitude: CesiumMath.toDegrees(center.longitude),
                    altitude: center.height,
                    viewingDistance: this.getCurrentViewingDistance()
                });
            }
        }
    }

    /**
     * Removes a track from the map
     * @param {string} trackId unique ID of the track
     */
    removeTrack(trackId) {

        const trackData = this.trackIdToTrackDataMap[trackId];

        if (trackData !== undefined) {

            MapView.log("removing track with ID: " + trackId);

            if (trackData.primitive !== undefined)
                this.trackPrimitivesCollection.remove(trackData.primitive);

            if (trackData.wallPrimitive !== undefined)
                this.trackPrimitivesCollection.remove(trackData.wallPrimitive);

            if (trackData.liveTrackEntity !== undefined)
                this.liveTrackDataSource.entities.remove(trackData.liveTrackEntity);

            this.trackIdToTrackDataMap[trackId] = undefined;
        }

        if (trackId === this.currentHeightProfileTrackId)
            this.closeHeightProfileView();

        if (this.liveTrackDataSource.entities.values.length === 0) {
            // removed the last live track entity
            this.viewer.scene.requestRenderMode = true;

            if (this.lastShadingMode !== undefined)
                this.setShadingMode(this.lastShadingMode);

            this.hideLiveTrackToolbar();
        }

        this.updateScene();
    }

    /**
     * Clears all tracks from the map
     */
    clearAllTracks() {

        MapView.log("clearing all tracks");

        this.trackPrimitivesCollection.removeAll();

        this.liveTrackDataSource.entities.removeAll();
        this.viewer.scene.requestRenderMode = true;

        if (this.lastShadingMode !== undefined)
            this.setShadingMode(this.lastShadingMode);

        this.hideLiveTrackToolbar();

        this.trackIdToTrackDataMap = {};

        if (this.heightProfileView !== null)
            this.closeHeightProfileView();

        this.updateScene();
    }

    /**
     * Shows track height profile.
     * @param {string} trackId unique ID of the track
     */
    showTrackHeightProfile(trackId) {

        if (typeof HeightProfileView !== "function") {
            console.warn("can't display track height profile; HeightProfileView class is not available");
            return;
        }

        if (this.currentHeightProfileTrackId === trackId)
            return; // already shown

        const trackData = this.trackIdToTrackDataMap[trackId];
        if (trackData === undefined) {
            console.warn("no track found for track ID " + trackId);
            return;
        }

        this.currentHeightProfileTrackId = trackId;

        if (this.heightProfileView !== null)
            this.heightProfileView.destroy();

        this.heightProfileView = new HeightProfileView({
            id: this.options.heightProfileElementId,
            setBodyBackgroundColor: false,
            useDarkTheme: true,
            showCloseButton: true,
            isFlightTrack: trackData.track.isFlightTrack,
            colorFromVarioValue: function(varioValue) {
                return this.trackColorFromVarioValue(varioValue).toCssColorString();
            }.bind(this),
            callback: this.heightProfileCallAction.bind(this)
        });

        this.heightProfileView.setTrack(trackData.track);

        if (trackData.track.groundHeightProfile !== undefined &&
            trackData.track.groundHeightProfile !== null)
            this.heightProfileView.addGroundProfile(trackData.track.groundHeightProfile);

        this.heightProfileView.updateView();
    }

    /**
     * Closes height profile view again
     */
    closeHeightProfileView() {

        if (this.heightProfileView !== null) {
            const view = this.heightProfileView;
            this.heightProfileView = null;

            view.hide();
            view.destroy();
        }

        this.trackMarker.show = false;

        if (this.trackMarker === this.viewer.trackedEntity)
            this.viewer.trackedEntity = null;

        this.currentHeightProfileTrackId = undefined;

        this.updateScene();
    }

    /**
     * Called for an action of the height profile view
     * @param {string} funcName action function name
     * @param {object} params action params
     */
    heightProfileCallAction(funcName, params) {

        if (funcName === "onHover" || funcName === "onClick") {

            this.updateTrackMarker(this.currentHeightProfileTrackId, params);

            if (funcName === "onClick") {

                this.viewer.flyTo(
                    this.trackMarker,
                    {
                        offset: new HeadingPitchRange(
                            this.viewer.scene.camera.heading,
                            this.viewer.scene.camera.pitch,
                            2000.0)
                    });
            }
        } else if (funcName === "onClose" && this.inOnCloseHandler === false) {
            this.inOnCloseHandler = true;
            this.closeHeightProfileView();
            this.inOnCloseHandler = false;

            this.updateScene();
        }
    }

    /**
     * Updates track marker to be placed on a track point index.
     * @param {string} trackId unique ID of the track
     * @param {object} trackPointIndex index of track point
     */
    updateTrackMarker(trackId, trackPointIndex) {

        const trackData = this.trackIdToTrackDataMap[trackId];
        if (trackData === undefined) {
            console.warn("no track found for track ID " + trackId);
            return;
        }

        const longitude = trackData.track.listOfTrackPoints[trackPointIndex * 3];
        const latitude = trackData.track.listOfTrackPoints[trackPointIndex * 3 + 1];
        const altitude = trackData.track.listOfTrackPoints[trackPointIndex * 3 + 2];

        this.trackMarker.show = true;
        this.trackMarker.position =
            Cartesian3.fromDegrees(longitude, latitude, altitude);

        this.updateScene();
    }

    /**
     * Called by the map ctor when the map is initialized and ready.
     */
    onMapInitialized() {

        MapView.log("map is initialized");

        if (this.options.callback !== undefined)
            this.options.callback("onMapInitialized");
    }

    /**
     * Called by the marker pin link, in order to show details of the location.
     * @param {string} locationId Location ID of location to show
     */
    onShowLocationDetails(locationId) {

        MapView.log("showing details to location with ID:" + locationId);

        if (this.options.callback !== undefined)
            this.options.callback("onShowLocationDetails", locationId);
    }

    /**
     * Called by the marker pin link, in order to show details of the location.
     * @param {string} locationId Location ID of location to show
     */
    onSetLocationAsCompassTarget(locationId) {

        MapView.log("setting location as compass target, with location ID:" + locationId);

        if (locationId === null)
            this.viewer.selectedEntity = undefined;

        if (this.options.callback !== undefined)
            this.options.callback("onSetLocationAsCompassTarget", locationId);
    }

    /**
     * Called by the marker pin link, in order to start navigating to the location.
     * @param {string} locationId Location ID of location to navigate to
     */
    onNavigateToLocation(locationId) {

        MapView.log("navigation to location started, with ID:" + locationId);

        if (this.options.callback !== undefined)
            this.options.callback("onNavigateToLocation", locationId);
    }

    /**
     * Called by the "my position" pin link, in order to share the current location.
     */
    onShareMyLocation() {

        MapView.log("sharing my location started");

        if (this.options.callback !== undefined)
            this.options.callback("onShareMyLocation");
    }

    /**
     * Called by the "add find result" pin link, in order to add the find result as a waypoint.
     * @param {object} [options] An object with the following properties:
     * @param {string} [options.name] Name of the find result
     * @param {number} [options.latitude] Latitude of the find result
     * @param {number} [options.longitude] Longitude of the find result
     */
    onAddFindResult(options) {

        MapView.log("adding find result as waypoint");

        if (this.options.callback !== undefined)
            this.options.callback("onAddFindResult", options);

        this.findResultMarker.show = false;

        // hide the info box
        this.viewer.selectedEntity = undefined;

        this.updateScene();
    }

    /**
     * Called by the "hide" link in the info text of the find result pin.
     */
    hideFindResult() {

        MapView.log("hiding find result pin");

        this.findResultMarker.show = false;

        // also hide the info box
        this.viewer.selectedEntity = undefined;

        this.updateScene();
    }

    /**
     * Called when a long-tap occured on the map.
     * @param {object} [options] An object with the following properties:
     * @param {number} [options.latitude] Latitude of the long tap
     * @param {number} [options.longitude] Longitude of the long tap
     * @param {number} [options.altitude] Altitude of the long tap
     */
    onLongTap(options) {

        MapView.log("long-tap occured: lat=" + options.latitude +
            ", long=" + options.longitude +
            ", alt=" + options.altitude);

        if (this.options.callback !== undefined)
            this.options.callback("onLongTap", options);
    }

    /**
     * Called by the marker pin link, in order to add the location to tour planning.
     * @param {string} locationId Location ID of location to add
     */
    onAddTourPlanLocation(locationId) {

        MapView.log("adding tour planning location: id=" + locationId);

        if (this.options.callback !== undefined)
            this.options.callback("onAddTourPlanLocation", locationId);

        // hide the info box
        this.viewer.selectedEntity = undefined;
    }

    /**
     * Called when a console.error() is called, e.g. for rendering errors from CesiumJS.
     * @param {string} message error message
     */
    onConsoleErrorMessage(message) {

        if (this.options.callback !== undefined)
            this.options.callback("onConsoleErrorMessage", message);
    }

    /**
     * Called to update the last shown location stored in the app.
     * @param {object} [options] An object with the following properties:
     * @param {number} [options.latitude] Latitude of the position
     * @param {number} [options.longitude] Longitude of the position
     * @param {number} [options.altitude] Altitude of the position
     * @param {number} [options.viewingDistance] viewing distance of camera; optional
     */
    onUpdateLastShownLocation(options) {

        options.viewingDistance = Math.floor(options.viewingDistance);

        MapView.log("updating last shown location: " +
            "lat=" + options.latitude +
            ", long=" + options.longitude +
            ", alt=" + options.altitude +
            ", viewingDistance=" + options.viewingDistance);

        if (this.options.callback !== undefined)
            this.options.callback("onUpdateLastShownLocation", options);
    }

    /**
     * Called by the "hide" link in the info text of the flying range cone.
     */
    hideFlyingRangeCone() {

        MapView.log("hiding flying range cone");

        this.viewer.entities.remove(this.flyingRangeCone);

        this.updateScene();
    }

    /**
     * Called when the network connectivity has changed.
     * @param {boolean} isAvailable indicates if network is available now
     */
    onNetworkConnectivityChanged(isAvailable) {

        // retry initializing terrain provider
        if (isAvailable &&
            (this.viewer.terrainProvider === null || this.viewer.terrainProvider instanceof EllipsoidTerrainProvider))
            this.initTerrainProvider();
    }

    /**
     * Called when a layer was successfully exported as KMZ byte stream
     * @param {string} blobData KMZ data, as blob, or null
     */
    onExportLayer(blobData) {

        if (this.options.callback === undefined)
            return;

        // convert to base64
        const reader = new FileReader();
        reader.onloadend = function() {
            const dataUrl = reader.result;
            const base64data = dataUrl.substr(dataUrl.indexOf(",") + 1);

            this.options.callback("onExportLayer", base64data);
        }.bind(this);

        reader.readAsDataURL(blobData);
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

        let baseAirac = new Date(MapView.airacStartDates[currentYear]);
        if (now < baseAirac) {
            currentYear--;
            baseAirac = new Date(MapView.airacStartDates[currentYear]);
        }

        const diffInDays = (now - baseAirac) / 86400000;

        const airacId = ((currentYear - 2000) * 100) + 1 + Math.floor(diffInDays / 28);
        return airacId;
    }
}
