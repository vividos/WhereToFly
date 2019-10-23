/**
 * Creates a new instance of MapView
 * @constructor
 * @param {object} [options] Options to use for initializing map view
 * @param {Number} [options.id] DOM ID of the div element to create map view in
 * @param {object} [options.initialCenterPoint] initial center point of map view
 * @param {double} [options.initialCenterPoint.latitude] latitude of center point
 * @param {double} [options.initialCenterPoint.longitude] longitude of center point
 * @param {Number} [options.initialZoomLevel] initial zoom level
 * @param {Boolean} [options.hasMouse] indicates if the device this is running supports a mouse
 * @param {Function} [options.callback] callback function to use for calling back to C# code
 */
function MapView(options) {

    console.log("creating new 3D map view");

    this.options = options || {
        id: 'mapElement',
        initialCenterPoint: { latitude: 47.67, longitude: 11.88 },
        initialZoomLevel: 14,
        hasMouse: false,
        callback: {}
    };

    if (this.options.callback === undefined)
        this.options.callback = callAction;

    console.log("#1 imagery provider");

    Cesium.Ion.defaultAccessToken = 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJqdGkiOiJiZWMzMjU5NC00MTg4LTQwYmEtYWNhYi01MDYwMWQyZDIxNTUiLCJpZCI6NjM2LCJpYXQiOjE1MjUzNjQ5OTN9.kXik5Mg_-01LBkN-5OTIDpwlMcuE2noRaaHrqjhbaRE';
    Cesium.BingMapsApi.defaultKey = 'AuuY8qZGx-LAeruvajcGMLnudadWlphUWdWb0k6N6lS2QUtURFk3ngCjIXqqFOoe';

    this.openStreetMapImageryLayer = null;
    this.openStreetMapImageryProvider = Cesium.OpenStreetMapImageryProvider({
        url: 'https://{s}.tile.openstreetmap.org/',
        subdomains: 'abc',
        maximumLevel: 18
    });

    this.bingMapsAerialWithLabelsImageryLayer = null;
    this.bingMapsAerialWithLabelsImageryProvider = new Cesium.BingMapsImageryProvider({
        url: 'https://dev.virtualearth.net',
        mapStyle: Cesium.BingMapsStyle.AERIAL_WITH_LABELS_ON_DEMAND
    });

    this.openTopoMapImageryLayer = null;
    this.openTopoMapImageryProvider = Cesium.OpenStreetMapImageryProvider({
        url: 'https://{s}.tile.opentopomap.org/',
        subdomains: 'abc',
        maximumLevel: 18,
        credits: '<code>Kartendaten: &copy; <a href="https://openstreetmap.org/copyright">OpenStreetMap</a>-Mitwirkende, SRTM | Kartendarstellung: &copy; <a href="http://opentopomap.org">OpenTopoMap</a> (<a href="https://creativecommons.org/licenses/by-sa/3.0/">CC-BY-SA</a>)</code>'
    });

    this.sentinel2ImageryLayer = null;
    this.sentinel2ImageryProvider = new Cesium.IonImageryProvider({ assetId: 3954 });

    this.openFlightMapsImageryLayer = null;
    var airacId = calcCurrentAiracId();
    this.openFlightMapsImageryProvider = Cesium.OpenStreetMapImageryProvider({
        url: 'https://snapshots.openflightmaps.org/live/' + airacId + '/tiles/world/noninteractive/epsg3857/merged/512/latest/',
        credit: '(c) <a href="https://openflightmaps.org/" target="_blank">Open Flightmaps association</a>, (c) OpenStreetMap contributors, NASA elevation data',
        maximumLevel: 11
    });

    this.setupSlopeAndContourLines();

    this.thermalSkywaysLayer = null;
    this.thermalSkywaysOverlay = this.createThermalImageryProvider();

    console.log('thermal maps url: ' + this.thermalSkywaysOverlay.url);

    this.blackMarbleLayer = null;
    this.blackMarbleOverlay = new Cesium.TileMapServiceImageryProvider({
        url: 'https://cesiumjs.org/tilesets/imagery/blackmarble',
        maximumLevel: 8,
        credit: 'Black Marble imagery courtesy NASA Earth Observatory'
    });

    console.log("#2 terrain provider");
    var terrainProvider = Cesium.createWorldTerrain({
        requestWaterMask: false,
        requestVertexNormals: true
    });

    console.log("#3 clock");
    var now = Cesium.JulianDate.now();
    var end = Cesium.JulianDate.addDays(now, 1, new Cesium.JulianDate());

    var clock = new Cesium.Clock({
        startTime: now,
        endTime: end,
        currentTime: now.clone(),
        clockStep: Cesium.ClockStep.SYSTEM_CLOCK,
        clockRange: Cesium.ClockRange.CLAMPED
    });

    console.log("#4 viewer");
    this.viewer = new Cesium.Viewer(this.options.id, {
        imageryProvider: this.openStreetMapImageryProvider,
        terrainProvider: terrainProvider,
        clockViewModel: new Cesium.ClockViewModel(clock),
        baseLayerPicker: false,
        sceneModePicker: false,
        animation: false,
        geocoder: false,
        homeButton: false,
        fullscreenButton: false,
        timeline: false,
        skyBox: false,
        scene3DOnly: true,
        requestRenderMode: true
    });

    this.viewer.scene.globe.enableLighting = true;

    // clip walls against terrain
    this.viewer.scene.globe.depthTestAgainstTerrain = true;

    // increase resolution for all image layer
    // https://github.com/AnalyticalGraphicsInc/cesium/issues/3279
    this.viewer.scene.globe.maximumScreenSpaceError = 1.666;

    // allow scripts to run in info box
    this.viewer.infoBox.frame.sandbox = this.viewer.infoBox.frame.sandbox + " allow-scripts";

    if (!options.hasMouse) {
        // switch to Touch instructions, as the control is only used on touch devices
        this.viewer.navigationHelpButton.viewModel.showTouch();
    }

    console.log("#5 setView");
    var longitude = this.options.initialCenterPoint['longitude'];
    var latitude = this.options.initialCenterPoint['latitude'];

    if (longitude !== 0 && latitude !== 0) {

        var initialHeading = 0.0; // north
        var initialPitch = Cesium.Math.toRadians(-35);

        var distance = 600000 - 40000 * this.options.initialZoomLevel;

        this.viewer.camera.setView({
            destination: Cesium.Cartesian3.fromDegrees(longitude, latitude, distance),
            orientation: {
                initialHeading,
                initialPitch,
                roll: 0.0
            }
        });

        var altitude = this.options.initialCenterPoint['altitude'] || 0.0;

        this.zoomToLocation({
            longitude: longitude,
            latitude: latitude,
            altitude: altitude
        });
    }

    console.log("#6 location markers");
    this.myLocationMarker = null;

    this.pinBuilder = new Cesium.PinBuilder();

    var that = this;
    Cesium.when(
        this.createEntity(undefined, 'My Position', '', Cesium.Color.GREEN, 'images/map-marker.svg', 0.0, 0.0),
        function (myLocationEntity) {
            myLocationEntity.show = false;
            that.myLocationMarker = that.viewer.entities.add(myLocationEntity);
        });

    // the zoom entity is invisible and transparent and is used for zoomToLocation() calls
    this.zoomEntity = this.viewer.entities.add({
        id: 'zoomEntity',
        position: Cesium.Cartesian3.fromDegrees(0.0, 0.0),
        point: {
            color: Cesium.Color.TRANSPARENT,
            heightReference: Cesium.HeightReference.CLAMP_TO_GROUND
        },
        show: false
    });

    // the find result entity is initially invisible
    Cesium.when(
        this.createEntity(undefined, 'Find result', '', Cesium.Color.ORANGE, 'images/magnify.svg', 0.0, 0.0),
        function (findResultEntity) {
            findResultEntity.show = false;
            that.findResultMarker = that.viewer.entities.add(findResultEntity);
        });

    console.log("#7 long tap handler");

    this.pickingHandler = new Cesium.ScreenSpaceEventHandler(this.viewer.scene.canvas);

    this.pickingHandler.setInputAction(this.onScreenTouchDown.bind(this), Cesium.ScreenSpaceEventType.LEFT_DOWN);
    this.pickingHandler.setInputAction(this.onScreenTouchUp.bind(this), Cesium.ScreenSpaceEventType.LEFT_UP);

    if (options.hasMouse) {
        this.pickingHandler.setInputAction(this.onScreenRightClick.bind(this), Cesium.ScreenSpaceEventType.RIGHT_CLICK);
    }

    console.log("#8 other stuff");

    // add a dedicated track primitives collection, as we can't call viewer.scene.primitives.removeAll()
    this.trackPrimitivesCollection = new Cesium.PrimitiveCollection({
        show: true,
        destroyPrimitives: true
    });

    this.viewer.scene.primitives.add(this.trackPrimitivesCollection);

    this.trackIdToTrackDataMap = {};

    this.dataSourceMap = {};

    this.onMapInitialized();
}

/**
 * Called when the screen space event handler detected a touch-down event.
 * @param {object} movement movement info object
 */
MapView.prototype.onScreenTouchDown = function (movement) {

    this.currentLeftDownPosition = movement.position;
    this.currentLeftDownTime = new Date().getTime();
};

/**
 * Called when the screen space event handler detected a touch-up event.
 * @param {object} movement movement info object
 */
MapView.prototype.onScreenTouchUp = function (movement) {

    var deltaX = this.currentLeftDownPosition.x - movement.position.x;
    var deltaY = this.currentLeftDownPosition.y - movement.position.y;
    var deltaSquared = deltaX * deltaX + deltaY * deltaY;

    var deltaTime = new Date().getTime() - this.currentLeftDownTime;

    // when tap was longer than 600ms and moved less than 10 pixels
    if (deltaTime > 600 && deltaSquared < 10 * 10) {

        var ray = this.viewer.camera.getPickRay(movement.position);
        var cartesian = this.viewer.scene.globe.pick(ray, this.viewer.scene);
        if (cartesian) {
            var cartographic = Cesium.Cartographic.fromCartesian(cartesian);

            this.onLongTap({
                latitude: Cesium.Math.toDegrees(cartographic.latitude),
                longitude: Cesium.Math.toDegrees(cartographic.longitude),
                altitude: cartographic.height
            });
        }
    }
};

/**
 * Called when the screen space event handler detected a right-click event.
 * @param {object} movement movement info object
 */
MapView.prototype.onScreenRightClick = function (movement) {

    var ray = this.viewer.camera.getPickRay(movement.position);
    var cartesian = this.viewer.scene.globe.pick(ray, this.viewer.scene);
    if (cartesian) {
        var cartographic = Cesium.Cartographic.fromCartesian(cartesian);

        this.onLongTap({
            latitude: Cesium.Math.toDegrees(cartographic.latitude),
            longitude: Cesium.Math.toDegrees(cartographic.longitude),
            altitude: cartographic.height
        });
    }
};

/**
 * Creates a new imagery provider that uses the Thermal Skyways from https://thermal.kk7.ch/.
 * @returns {object} generated imagery provider object
 */
MapView.prototype.createThermalImageryProvider = function () {

    var url = 'https://thermal.kk7.ch/tiles/skyways_all/{z}/{x}/{reverseY}.png?src=https://github.com/vividos/WhereToFly';

    var creditText = 'Skyways &copy; <a href="https://thermal.kk7.ch/">thermal.kk7.ch</a>';

    var tilingScheme = new Cesium.WebMercatorTilingScheme();

    return new Cesium.UrlTemplateImageryProvider({
        url: Cesium.Resource.createIfNeeded(url),
        credit: new Cesium.Credit(creditText, false),
        tilingScheme: tilingScheme,
        tileWidth: 256,
        tileHeight: 256,
        minimumLevel: 0,
        maximumLevel: 12,
        rectangle: tilingScheme.rectangle
    });
};

/**
 * Sets new map imagery type
 * @param {string} imageryType imagery type constant; the following constants currently can be
 * used: 'OpenStreetMap'.
 */
MapView.prototype.setMapImageryType = function (imageryType) {

    console.log("setting new imagery type: " + imageryType);

    var layers = this.viewer.scene.imageryLayers;

    if (this.openStreetMapImageryLayer !== null)
        layers.remove(this.openStreetMapImageryLayer, false);

    if (this.bingMapsAerialWithLabelsImageryLayer !== null)
        layers.remove(this.bingMapsAerialWithLabelsImageryLayer, false);

    if (this.openTopoMapImageryLayer !== null)
        layers.remove(this.openTopoMapImageryLayer, false);

    if (this.sentinel2ImageryLayer !== null)
        layers.remove(this.sentinel2ImageryLayer, false);

    if (this.openFlightMapsImageryLayer !== null)
        layers.remove(this.openFlightMapsImageryLayer, false);

    switch (imageryType) {
        case 'OpenStreetMap':
            if (this.openStreetMapImageryLayer === null)
                this.openStreetMapImageryLayer = layers.addImageryProvider(this.openStreetMapImageryProvider, 1);
            else
                layers.add(this.openStreetMapImageryLayer, 1);
            break;

        case 'BingMapsAerialWithLabels':
            if (this.bingMapsAerialWithLabelsImageryLayer === null)
                this.bingMapsAerialWithLabelsImageryLayer = layers.addImageryProvider(this.bingMapsAerialWithLabelsImageryProvider, 1);
            else
                layers.add(this.bingMapsAerialWithLabelsImageryLayer, 1);
            break;

        case 'OpenTopoMap':
            if (this.openTopoMapImageryLayer === null)
                this.openTopoMapImageryLayer = layers.addImageryProvider(this.openTopoMapImageryProvider, 1);
            else
                layers.add(this.openTopoMapImageryLayer, 1);
            break;

        case 'Sentinel2':
            if (this.sentinel2ImageryLayer === null)
                this.sentinel2ImageryLayer = layers.addImageryProvider(this.sentinel2ImageryProvider, 1);
            else
                layers.add(this.sentinel2ImageryLayer, 1);
            break;

        case 'OpenFlightMaps':
            if (this.openFlightMapsImageryLayer === null)
                this.openFlightMapsImageryLayer = layers.addImageryProvider(this.openFlightMapsImageryProvider, 1);
            else
                layers.add(this.openFlightMapsImageryLayer, 1);
            break;

        default:
            console.log('invalid imagery type: ' + imageryType);
            break;
    }

    this.viewer.scene.requestRender();
};

var slopeRamp = [0.0, 0.29, 0.5, Math.sqrt(2) / 2, 0.87, 0.91, 1.0];

/**
 * Generates a color ramp canvas element and returns it. From
 * https://cesiumjs.org/Cesium/Build/Apps/Sandcastle/index.html?src=Globe%20Materials.html
 * @returns {object} generated canvas object
 */
function getColorRamp() {
    var ramp = document.createElement('canvas');
    ramp.width = 100;
    ramp.height = 1;
    var ctx = ramp.getContext('2d');

    var values = slopeRamp;

    var grd = ctx.createLinearGradient(0, 0, 100, 0);
    grd.addColorStop(values[0], '#000000'); // black
    grd.addColorStop(values[1], '#2747E0'); // blue
    grd.addColorStop(values[2], '#D33B7D'); // pink
    grd.addColorStop(values[3], '#D33038'); // red
    grd.addColorStop(values[4], '#FF9742'); // orange
    grd.addColorStop(values[5], '#ffd700'); // yellow
    grd.addColorStop(values[6], '#ffffff'); // white

    ctx.fillStyle = grd;
    ctx.fillRect(0, 0, 100, 1);

    return ramp;
}

/**
 * Sets up slope and contour lines materials for overlay types 'ContourLines' and
 * 'SlopeAndContourLines'. From
 * https://cesiumjs.org/Cesium/Build/Apps/Sandcastle/index.html?src=Globe%20Materials.html
 */
MapView.prototype.setupSlopeAndContourLines = function () {

    // Creates a material with contour lines only
    this.contourLinesMaterial = Cesium.Material.fromType('ElevationContour');

    this.contourLinesMaterial.uniforms.width = 1; // in pixels
    this.contourLinesMaterial.uniforms.spacing = 100; // in meters
    this.contourLinesMaterial.uniforms.color = Cesium.Color.BLUE.clone();

    // Creates a composite material with both slope shading and contour lines
    var material = new Cesium.Material({
        fabric: {
            type: 'SlopeColorContour',
            materials: {
                contourMaterial: {
                    type: 'ElevationContour'
                },
                slopeRampMaterial: {
                    type: 'SlopeRamp'
                }
            },
            components: {
                diffuse: 'contourMaterial.alpha == 0.0 ? slopeRampMaterial.diffuse : contourMaterial.diffuse',
                alpha: 'max(contourMaterial.alpha, slopeRampMaterial.alpha)'
            }
        },
        translucent: false
    });

    var contourUniforms = material.materials.contourMaterial.uniforms;
    contourUniforms.width = 1; // in pixels
    contourUniforms.spacing = 100; // in meters
    contourUniforms.color = Cesium.Color.BLUE.clone();

    var shadingUniforms = material.materials.slopeRampMaterial.uniforms;
    shadingUniforms.image = getColorRamp();

    this.slopeAndContourLinesMaterial = material;
};

/**
 * Sets new map overlay type
 * @param {string} overlayType overlay type constant; the following constants currently can be
 * used: 'None', 'ContourLines', 'SlopeAndContourLines', 'ThermalSkywaysKk7', 'BlackMarble'.
 */
MapView.prototype.setMapOverlayType = function (overlayType) {

    console.log("setting new map overlay type: " + overlayType);

    var layers = this.viewer.scene.imageryLayers;

    if (this.thermalSkywaysLayer !== null)
        layers.remove(this.thermalSkywaysLayer, false);

    if (this.blackMarbleLayer !== null)
        layers.remove(this.blackMarbleLayer, false);

    this.viewer.scene.globe.material = null;

    switch (overlayType) {
        case 'None':
            break;

        case 'ContourLines':
            this.viewer.scene.globe.material = this.contourLinesMaterial;
            break;

        case 'SlopeAndContourLines':
            this.viewer.scene.globe.material = this.slopeAndContourLinesMaterial;
            break;

        case 'ThermalSkywaysKk7':
            if (this.thermalSkywaysLayer === null) {
                this.thermalSkywaysLayer = layers.addImageryProvider(this.thermalSkywaysOverlay);
                this.thermalSkywaysLayer.alpha = 0.2; // 0.0 is transparent.  1.0 is opaque.
                this.thermalSkywaysLayer.brightness = 2.0; // > 1.0 increases brightness.  < 1.0 decreases.
            }
            else
                layers.add(this.thermalSkywaysLayer);
            break;

        case 'BlackMarble':
            if (this.blackMarbleLayer === null) {
                this.blackMarbleLayer = layers.addImageryProvider(this.blackMarbleOverlay);
                this.blackMarbleLayer.alpha = 0.5; // 0.0 is transparent.  1.0 is opaque.
                this.blackMarbleLayer.brightness = 2.0; // > 1.0 increases brightness.  < 1.0 decreases.
            }
            else
                layers.add(this.blackMarbleLayer);
            break;

        default:
            console.log('invalid map overlay type: ' + overlayType);
            break;
    }

    this.viewer.scene.requestRender();
};

/**
 * Sets new map shading mode
 * @param {string} shadingMode shading mode constant; the following constants currently can be
 * used: 'Fixed10Am', 'Fixed3Pm', 'CurrentTime', 'Ahead6Hours' and 'None'.
 */
MapView.prototype.setShadingMode = function (shadingMode) {

    console.log("setting new shading mode: " + shadingMode);

    var today = new Date();
    var now = Cesium.JulianDate.now();

    switch (shadingMode) {
        case 'Fixed10Am':
        case 'Fixed3Pm':
            today.setHours(shadingMode === 'Fixed10Am' ? 10 : 15, 0, 0, 0);
            var fixedTime = Cesium.JulianDate.fromDate(today);

            this.viewer.clockViewModel.startTime = fixedTime;
            this.viewer.clockViewModel.currentTime = fixedTime.clone();
            this.viewer.clockViewModel.endTime = fixedTime.clone();
            this.viewer.clockViewModel.clockStep = 0;
            this.viewer.clockViewModel.shouldAnimate = false;
            break;

        case 'CurrentTime':
            var end = Cesium.JulianDate.addDays(now, 1, new Cesium.JulianDate());

            this.viewer.clockViewModel.startTime = now;
            this.viewer.clockViewModel.currentTime = now.clone();
            this.viewer.clockViewModel.endTime = end;
            this.viewer.clockViewModel.clockStep = Cesium.ClockStep.SYSTEM_CLOCK;
            this.viewer.clockViewModel.shouldAnimate = true;
            break;

        case 'Ahead6Hours':
            var ahead = Cesium.JulianDate.addHours(now, 6, new Cesium.JulianDate());
            var end2 = Cesium.JulianDate.addDays(ahead, 1, new Cesium.JulianDate());

            this.viewer.clockViewModel.startTime = ahead;
            this.viewer.clockViewModel.currentTime = ahead.clone();
            this.viewer.clockViewModel.endTime = end2;
            this.viewer.clockViewModel.clockStep = Cesium.ClockStep.SYSTEM_CLOCK;
            this.viewer.clockViewModel.shouldAnimate = true;
            break;

        case 'None':
            this.viewer.clockViewModel.shouldAnimate = false;
            break;

        default:
            console.log('invalid shading mode: ' + shadingMode);
    }

    this.viewer.scene.globe.enableLighting = shadingMode !== 'None';

    this.viewer.terrainShadows =
        shadingMode === 'None' ? Cesium.ShadowMode.DISABLED : Cesium.ShadowMode.RECEIVE_ONLY;

    this.viewer.scene.requestRender();
};

/**
 * Updates the "my location" marker on the map
 * @param {object} [options] Options to use for updating "my location" pin.
 * @param {double} [options.latitude] Latitude of position
 * @param {double} [options.longitude] Longitude of position
 * @param {double} [options.displayLatitude] Display text of latitude
 * @param {double} [options.displayLongitude] Display text of longitude
 * @param {double} [options.altitude] Altitude of position
 * @param {double} [options.speed] Current speed, in km/h
 * @param {double} [options.displaySpeed] Display text for current speed
 * @param {string} [options.timestamp] Timestamp of position, as parseable date string
 * @param {string} [options.displayTimestamp] Display text of timestamp
 * @param {Number} [options.positionAccuracy] Accuracy of position, in meter
 * @param {string} [options.positionAccuracyColor] Hex color in format #rrggbb for position accuracy
 * @param {Boolean} [options.zoomToLocation] indicates if view should also zoom to this position
 */
MapView.prototype.updateMyLocation = function (options) {

    if (this.myLocationMarker === null) {
        console.log("warning: myLocationMarker not initialized yet");
        return;
    }

    console.log("updating my location: lat=" + options.latitude + ", long=" + options.longitude);

    this.myLocationMarker.show = true;
    this.myLocationMarker.position = Cesium.Cartesian3.fromDegrees(options.longitude, options.latitude);

    var text = '<h2><img height="48em" width="48em" src="images/map-marker.svg" style="vertical-align:middle" />My Position</h2>' +
        '<div>Latitude: ' + options.displayLatitude + '<br/>' +
        'Longitude: ' + options.displayLongitude + '<br/>' +
        'Accuracy: <span style="color:' + options.positionAccuracyColor + '">+/- ' + options.positionAccuracy + ' m</span><br/>' +
        (options.altitude !== undefined && options.altitude !== 0 ? 'Altitude: ' + options.altitude + 'm<br/>' : '') +
        'Speed: ' + options.displaySpeed + "<br/>" +
        'Time: ' + options.displayTimestamp +
        '</div>';

    text += '<img height="32em" width="32em" src="images/share-variant.svg" style="vertical-align:middle" />' +
        '<a href="javascript:parent.map.onShareMyLocation();">Share position</a></p>';

    this.myLocationMarker.description = text;

    if (options.zoomToLocation) {
        console.log("also zooming to my location");
        this.viewer.flyTo(
            this.myLocationMarker,
            {
                offset: new Cesium.HeadingPitchRange(
                    this.viewer.scene.camera.heading,
                    this.viewer.scene.camera.pitch,
                    5000.0)
            });
    }
};

/**
 * Zooms to given location, by flying to the location
 * @param {object} [options] Options to use for zooming
 * @param {double} [options.latitude] Latitude of zoom target
 * @param {double} [options.longitude] Longitude of zoom target
 * @param {double} [options.altitude] Altitude of zoom target
 */
MapView.prototype.zoomToLocation = function (options) {

    if (this.zoomEntity === undefined) {
        console.log("warning: zoomEntity not initialized yet");
        return;
    }

    console.log("zooming to: latitude=" + options.latitude + ", longitude=" + options.longitude + ", altitude=" + options.altitude);

    var altitude = options.altitude || 0.0;

    // zooming works by assinging the zoom entity a new position, making it
    // visible (but transparent), fly there and hiding it again
    this.zoomEntity.position = Cesium.Cartesian3.fromDegrees(options.longitude, options.latitude, altitude);

    this.zoomEntity.point.heightReference =
        altitude === 0.0 ? Cesium.HeightReference.CLAMP_TO_GROUND : Cesium.HeightReference.NONE;

    this.zoomEntity.show = true;

    console.log("zooming to: start flying");

    this.viewer.flyTo(
        this.zoomEntity,
        {
            offset: new Cesium.HeadingPitchRange(
                this.viewer.camera.heading,
                this.viewer.camera.pitch,
                5000.0)
        }).then(function () {
            this.zoomEntity.show = false;
            console.log("zooming to: flying finished");
        });
};

/**
 * Adds a new layer to the map
 * @param {object} [layer] Layer object to add
 * @param {string} [layer.id] ID of layer
 * @param {string} [layer.name] Layer name
 * @param {string} [layer.type] Layer type
 * @param {boolean} [layer.isVisible] Indicates if layer is visible
 * @param {string} [layer.data] CZML data of layer
 */
MapView.prototype.addLayer = function (layer) {

    console.log("adding layer " + layer.name + ", with data length " + layer.data.length + " bytes");

    var czml = JSON.parse(layer.data);

    var dataSourcePromise = Cesium.CzmlDataSource.load(czml);

    var that = this;

    Cesium.when(dataSourcePromise,
        function (dataSource) {
            that.viewer.dataSources.add(dataSource);
            that.dataSourceMap[layer.id] = dataSource;
        });
};

/**
 * Zooms to layer with given layer ID
 * @param {string} [layerId] ID of layer
 */
MapView.prototype.zoomToLayer = function (layerId) {

    console.log("zooming to layer with id " + layerId);

    var dataSource = this.dataSourceMap[layerId];
    if (dataSource !== undefined)
        this.viewer.flyTo(dataSource);
};

/**
 * Sets layer visibility
 * @param {object} [layer] Layer object to set visibility for
 * @param {string} [layer.id] ID of layer
 * @param {boolean} [layer.isVisible] Indicates if layer is visible
 */
MapView.prototype.setLayerVisibility = function (layer) {

    console.log("setting new visibility for layer with id " + layerId +
        ", visibility: " + layer.isVisible);

    var dataSource = this.dataSourceMap[layerId];
    if (dataSource !== undefined)
        dataSource.show = layer.isVisible;
};

/**
 * Removes layer with given layer ID
 * @param {string} [layerId] ID of layer
 */
MapView.prototype.removeLayer = function (layerId) {

    console.log("removing layer with id " + layerId);

    var dataSource = this.dataSourceMap[layerId];
    if (dataSource !== undefined) {
        this.viewer.dataSources.remove(dataSource);
        this.dataSourceMap[layerId] = undefined;
    }
};

/**
 * Clears list of layers
 */
MapView.prototype.clearLayerList = function () {

    console.log("clearing layer list");

    for (var layerId in this.dataSourceMap) {
        var dataSource = this.dataSourceMap[layerId];
        if (dataSource !== undefined)
            this.viewer.dataSources.remove(dataSource);
    }

    this.dataSourceMap = {};
};

/**
 * Clears list of locations
 */
MapView.prototype.clearLocationList = function () {

    console.log("clearing location list");

    this.viewer.entities.removeAll();

    // re-add the special purpose entities
    if (this.myLocationMarker !== null)
        this.viewer.entities.add(this.myLocationMarker);

    if (this.findResultMarker !== null)
        this.viewer.entities.add(this.findResultMarker);

    if (this.zoomEntity !== null)
        this.viewer.entities.add(this.zoomEntity);

    if (this.trackEntity !== null)
        this.viewer.entities.add(this.trackEntity);

    if (this.flyingRangeCone !== null)
        this.viewer.entities.add(this.flyingRangeCone);
};

/**
 * Formats description text for a location
 * @param {string} [location] Location to format text for
 * @param {string} [location.id] Location ID
 * @param {string} [location.type] Location type
 * @param {string} [location.name] Location name
 * @param {string} [location.description] Location description text
 * @param {Number} [location.altitude] Altitude of the location
 * @param {boolean} [location.isPlanTourLocation] Indicates if it's a tour planning location
 * @returns {string} formatted location text
 */
MapView.prototype.formatLocationText = function (location) {

    var text = '<h2><img height="48em" width="48em" src="' + this.imageUrlFromLocationType(location.type) + '" style="vertical-align:middle" />' +
        location.name +
        (location.altitude !== 0 ? ' ' + location.altitude + 'm' : '') +
        '</h2>';

    text += '<p><img height="32em" width="32em" src="images/information-outline.svg" style="vertical-align:middle" /> ' +
        '<a href="javascript:parent.map.onShowLocationDetails(\'' + location.id + '\');">Show details</a> ';

    text += '<img height="32em" width="32em" src="images/directions.svg" style="vertical-align:middle" /> ' +
        '<a href="javascript:parent.map.onNavigateToLocation(\'' + location.id + '\');">Navigate here</a></p>';

    if (location.isPlanTourLocation === true) {
        text += '<img height="32em" width="32em" src="images/map-marker-plus.svg" style="vertical-align:middle" /> ' +
            '<a href="javascript:parent.map.onAddTourPlanLocation(\'' + location.id + '\');">Plan tour</a></p>';
    }

    text += "<p>" + location.description + "</p>";

    return text;
};

/**
 * Adds list of locations to the map, as marker pins
 * @param {array} locationList An array of location, each with the following object layout:
 * { id:"location-id", name:"Location Name", type:"LocationType", latitude: 123.45678, longitude: 9.87654, altitude:1234.5 }
 */
MapView.prototype.addLocationList = function (locationList) {

    console.log("adding location list, with " + locationList.length + " entries");

    var that = this;
    for (var index in locationList) {

        var location = locationList[index];

        var text = this.formatLocationText(location);

        var imagePath = this.imageUrlFromLocationType(location.type);

        Cesium.when(
            this.createEntity(
                location.id,
                location.name + (location.altitude !== 0 ? ' ' + location.altitude + 'm' : ''),
                text,
                this.pinColorFromLocationType(location.type),
                imagePath,
                location.longitude,
                location.latitude),
            function (entity) {
                that.viewer.entities.add(entity);
            });
    }
};

/**
 * Creates an entity object with given informations that can be placed into
 * the entities list.
 * @param {string} id Unique ID of the entity; may be 'undefined'
 * @param {string} name Name of the entity
 * @param {string} description Longer description text
 * @param {string} pinColor Pin color, one of the Cesium.Color.Xxx constants
 * @param {string} pinImage Relative link URL to SVG image to use in pin
 * @param {double} longitude Longitude of entity
 * @param {double} latitude Latitude of entity
 * @returns {Promise<object>} entity description, usable for viewer.entities.add()
 */
MapView.prototype.createEntity = function (id, name, description, pinColor, pinImage, longitude, latitude) {

    var url = Cesium.getAbsoluteUri(pinImage, window.location.href);

    return Cesium.when(
        this.pinBuilder.fromUrl(url, pinColor, 48),
        function (canvas) {
            return {
                id: id,
                name: name,
                description: description,
                position: Cesium.Cartesian3.fromDegrees(longitude, latitude),
                billboard: {
                    image: canvas.toDataURL(),
                    verticalOrigin: Cesium.VerticalOrigin.BOTTOM,
                    heightReference: Cesium.HeightReference.CLAMP_TO_GROUND
                }
            };
        });
};

/**
 * Returns a relative image Url for given location type
 * @param {string} locationType location type
 * @returns {string} relative image Url
 */
MapView.prototype.imageUrlFromLocationType = function (locationType) {

    switch (locationType) {
        case 'Summit': return 'images/mountain-15.svg';
        //case 'Pass': return '';
        case 'Lake': return 'images/water-15.svg';
        case 'Bridge': return 'images/bridge.svg';
        case 'Viewpoint': return 'images/attraction-15.svg';
        case 'AlpineHut': return 'images/home-15.svg';
        case 'Restaurant': return 'images/restaurant-15.svg';
        case 'Church': return 'images/church.svg';
        case 'Castle': return 'images/castle.svg';
        //case 'Cave': return '';
        case 'Information': return 'images/information-outline.svg';
        case 'PublicTransportBus': return 'images/bus.svg';
        case 'PublicTransportTrain': return 'images/train.svg';
        case 'Parking': return 'images/parking.svg';
        //case 'ViaFerrata': return '';
        case 'CableCar': return 'images/aerialway-15.svg';
        case 'FlyingTakeoff': return 'images/paragliding.svg';
        case 'FlyingLandingPlace': return 'images/paragliding.svg';
        case 'FlyingWinchTowing': return 'images/paragliding.svg';
        //case 'LiveWaypoint': return '';
        //case 'Turnpoint': return '';
        default: return 'images/map-marker.svg';
    }
};

/**
 * Returns a pin color for given location type
 * @param {string} locationType location type
 * @returns {string} Cesium.Color constant
 */
MapView.prototype.pinColorFromLocationType = function (locationType) {

    switch (locationType) {
        case 'FlyingTakeoff': return Cesium.Color.YELLOWGREEN;
        case 'FlyingLandingPlace': return Cesium.Color.ORANGE;
        case 'FlyingWinchTowing': return Cesium.Color.CORNFLOWERBLUE;
        case 'Turnpoint': return Cesium.Color.RED;
        default: return Cesium.Color.BLUE;
    }
};

/**
 * Updates a single location
 * @param {Object} [location] An object with the following properties:
 * @param {String} [location.id] ID of the location to update
 * @param {string} [location.name] Location name
 * @param {string} [location.type] Location type
 * @param {Number} [location.latitude] Latitude of the location to update
 * @param {Number} [location.longitude] Longitude of the location to update
 * @param {Number} [location.altitude] Altitude of the location to update
 * @param {string} [location.description] Location description text
 * @param {boolean} [location.isPlanTourLocation] Indicates if it's a tour planning location
 */
MapView.prototype.updateLocation = function (location) {

    console.log("updating location \"" + location.id +
        "\", new position at at latitude " + location.latitude +
        ", longitude " + location.longitude +
        ", altitude " + location.altitude);

    var entity = this.viewer.entities.getById(location.id);

    if (entity === undefined) {
        console.log("couldn't find entity for id: " + location.id);
        return;
    }

    entity.position = Cesium.Cartesian3.fromDegrees(location.longitude, location.latitude, location.altitude);

    entity.name = location.name;
    entity.description = this.formatLocationText(location);
};

/**
 * Removes a single location from map
 * @param {String} [locationId] ID of the location to remove
 */
MapView.prototype.removeLocation = function (locationId) {

    console.log("removing location with ID \"" + location.id);

    var entity = this.viewer.entities.getById(locationId);

    this.viewer.entities.remove(entity);
};

/**
 * Shows a find result pin, with a link to add a waypoint for this result.
 * @param {Object} [options] An object with the following properties:
 * @param {String} [options.name] Name of the find result
 * @param {String} [options.description] Description text of the find result
 * @param {Number} [options.latitude] Latitude of the find result
 * @param {Number} [options.longitude] Longitude of the find result
 * @param {Number} [options.displayLatitude] Display text for latitude
 * @param {Number} [options.displayLongitude] Display text for longitude
 */
MapView.prototype.showFindResult = function (options) {

    if (this.findResultMarker === undefined) {
        console.log("warning: findResultMarker not initialized yet");
        return;
    }

    console.log("showing find result for \"" + options.name +
        "\", at latitude " + options.latitude + ", longitude " + options.longitude);

    var text = '<h2><img height="48em" width="48em" src="images/magnify.svg" style="vertical-align:middle" />' + options.name + '</h2>' +
        '<div>Latitude: ' + options.displayLatitude + '<br/>' +
        'Longitude: ' + options.displayLongitude +
        '</div>';

    var optionsText = '{ name: \'' + options.name + '\', latitude:' + options.latitude + ', longitude:' + options.longitude + '}';

    text += '<img height="32em" width="32em" src="images/map-marker-plus.svg" style="vertical-align:middle" />' +
        '<a href="javascript:parent.map.onAddFindResult(' + optionsText + ');">Add as waypoint</a></p>';

    if (options.description !== undefined)
        text += '<div>' + options.description + '</div>';

    this.findResultMarker.description = text;
    this.findResultMarker.position = Cesium.Cartesian3.fromDegrees(options.longitude, options.latitude);
    this.findResultMarker.show = true;

    this.viewer.flyTo(
        this.findResultMarker,
        {
            offset: new Cesium.HeadingPitchRange(
                this.viewer.camera.heading,
                this.viewer.camera.pitch,
                5000.0)
        });
};

/**
 * Shows flying range for a given map point and using flying range parameters.
 * @param {Object} [options] Options for showing flying range:
 * @param {Number} [options.latitude] Latitude of the point to show flying range
 * @param {Number} [options.longitude] Longitude of the point to show flying range
 * @param {double} [options.displayLatitude] Display text of latitude
 * @param {double} [options.displayLongitude] Display text of longitude
 * @param {Number} [options.altitude] Altitude of the point to show flying range
 * @param {Number} [options.glideRatio] Glide ratio, in km flying per 1000m sink
 * @param {Number} [options.gliderSpeed] Glider speed, in km/h; must be above wind speed
 * @param {Number} [options.windDirection] Wind direction, in degrees
 * @param {Number} [options.windSpeed] Wind speed, in km/h
 */
MapView.prototype.showFlyingRange = function (options) {

    if (this.flyingRangeCone !== null)
        this.viewer.entities.remove(this.flyingRangeCone);

    // limit wind speed so that we don't get a negative glide angle
    if (options.windSpeed > options.gliderSpeed - 1)
        options.windSpeed = options.gliderSpeed - 1;

    // use wind speed to calculate the angle to pitch the cone
    var glideAngle = Math.atan(options.glideRatio);
    var glideRatioWithWind = options.glideRatio * (options.gliderSpeed - options.windSpeed) / options.gliderSpeed;
    var glideAngleWithWind = Math.atan(glideRatioWithWind);
    var conePitch = glideAngle - glideAngleWithWind;

    var quat = Cesium.Transforms.headingPitchRollQuaternion(
        Cesium.Cartesian3.fromDegrees(options.longitude, options.latitude, options.altitude / 2.0),
        new Cesium.HeadingPitchRoll(
            Cesium.Math.toRadians(options.windDirection + 90.0),
            conePitch, 0.0)
    );

    var text = '<p><img height="32em" width="32em" src="images/close-circle-outline.svg" style="vertical-align:middle" />' +
        '<a href="javascript:parent.map.hideFlyingRangeCone();">Hide</a></p>';

    text += '<p>Flying range for map point at<br/>Latitude: ' + options.displayLatitude + '<br/>' +
        'Longitude: ' + options.displayLongitude + '<br/>' +
        'Altitude: ' + options.altitude + 'm</p>';

    text += '<p>' +
        'Glide ratio: ' + options.glideRatio + '<br/>' +
        'Glide angle: ' + (90.0 - Cesium.Math.toDegrees(glideAngle)).toFixed(1) + ' degrees<br/>' +
        //'Glider speed: ' + options.gliderSpeed + ' km/h<br/>' +
        //'Glide ratio with wind: ' + glideRatioWithWind.toFixed(1) + '<br/>' +
        //'Glide angle with wind: ' + (90.0 - Cesium.Math.toDegrees(glideAngleWithWind)).toFixed(1) + ' degrees<br/>' +
        //'Wind: ' + options.windSpeed + ' km/h from ' + options.windDirection + ' degrees' +
        '</p>';

    this.flyingRangeCone = this.viewer.entities.add({
        name: 'Flying range',
        description: text,
        position: Cesium.Cartesian3.fromDegrees(options.longitude, options.latitude, options.altitude / 2.0),
        orientation: quat,
        cylinder: {
            length: options.altitude,
            topRadius: 0.0,
            bottomRadius: options.altitude * options.glideRatio,
            numberOfVerticalLines: 18,
            material: Cesium.Color.BLUE.withAlpha(0.4),
            outline: true,
            outlineColor: Cesium.Color.WHITE
        }
    });

    this.viewer.flyTo(this.flyingRangeCone,
        {
            offset: new Cesium.HeadingPitchRange(
                this.viewer.camera.heading,
                this.viewer.camera.pitch,
                20 * 1000.0)
        });
};

/**
 * Samples track point heights from actual map and adjusts the track when it goes below terrain
 * height.
 * @param {object} [track] Track object to add
 * @param {string} [track.id] unique ID of the track
 * @param {array} [track.listOfTrackPoints] An array of track points in long, lat, alt, long, lat, alt ... order
 * @param {number} [offsetInMeters] offset in meters to apply to track
 */
MapView.prototype.sampleTrackHeights = function (track, offsetInMeters) {

    console.log("start sampling track point heights for " + track.listOfTrackPoints.length + " points...");

    if (!Cesium.Entity.supportsPolylinesOnTerrain(this.viewer.scene)) {
        return;
    }

    offsetInMeters = offsetInMeters || 0.0;

    var trackPointArray = Cesium.Cartesian3.fromDegreesArrayHeights(track.listOfTrackPoints);

    var cartographicArray = [];
    for (var trackPointIndex = 0; trackPointIndex < trackPointArray.length; ++trackPointIndex) {
        cartographicArray.push(Cesium.Cartographic.fromCartesian(trackPointArray[trackPointIndex]));
    }

    var that = this;
    Cesium.when(Cesium.sampleTerrainMostDetailed(
        this.viewer.terrainProvider,
        cartographicArray),
        function (samples) {

            var trackPointHeightArray = [];

            // NOSONAR
            for (var trackPointIndex = 0; trackPointIndex < trackPointArray.length; ++trackPointIndex) {

                var trackPointHeight = track.listOfTrackPoints[trackPointIndex * 3 + 2] + offsetInMeters;
                var sampledHeight = samples[trackPointIndex].height;

                if (sampledHeight > trackPointHeight)
                    trackPointHeight = sampledHeight;

                trackPointHeightArray.push(trackPointHeight);
            }

            console.log("sampling track point heights finished.");

            that.onSampledTrackHeights(trackPointHeightArray);
        });

    console.log("call to sampleTrackHeights() returns.");
};

/**
 * Called when sampling track points has finished.
 * @param {array} [listOfTrackPointHeights] An array of track point heights for all track points
 * that were passed to sampleTrackHeights().
 */
MapView.prototype.onSampledTrackHeights = function (listOfTrackPointHeights) {
    console.log("sampling track heights has finished");

    if (this.options.callback !== undefined)
        this.options.callback('onSampledTrackHeights', listOfTrackPointHeights);
};

/**
 * Calculates track color from given variometer climb/sink rate value.
 * @param {double} varioValue variometer value, in m/s
 * @returns {Cesium.Color} Track color
 */
MapView.prototype.trackColorFromVarioValue = function (varioValue) {

    var varioColorMapping = [
        5.0, Cesium.Color.RED,
        4.5, Cesium.Color.fromBytes(255, 64, 0),
        4.0, Cesium.Color.fromBytes(255, 128, 0),
        3.5, Cesium.Color.fromBytes(255, 192, 0),
        3.0, Cesium.Color.YELLOW,
        2.5, Cesium.Color.fromBytes(192, 255, 0),
        2.0, Cesium.Color.fromBytes(128, 255, 0),
        1.5, Cesium.Color.fromBytes(64, 255, 128),
        1.0, Cesium.Color.CYAN,
        0.5, Cesium.Color.fromBytes(0, 224, 255),
        0.0, Cesium.Color.fromBytes(0, 192, 255),
        -0.5, Cesium.Color.fromBytes(0, 160, 255),
        -1.0, Cesium.Color.fromBytes(0, 128, 255),
        -1.5, Cesium.Color.fromBytes(0, 96, 224),
        -2.0, Cesium.Color.fromBytes(0, 64, 192),
        -3.0, Cesium.Color.fromBytes(0, 32, 160),
        -3.5, Cesium.Color.fromBytes(0, 0, 128),
        -4.0, Cesium.Color.fromBytes(64, 0, 128)
    ];

    for (var mappingIndex = 0; mappingIndex < varioColorMapping.length; mappingIndex += 2) {
        if (varioValue >= varioColorMapping[mappingIndex])
            return varioColorMapping[mappingIndex + 1];
    }

    return Cesium.Color.fromBytes(128, 0, 128); // smaller than -4.0
};

/**
 * Calculates an array of track colors based on the altitude changes of the given track points.
 * @param {array} listOfTrackPoints An array of track points in long, lat, alt, long, lat, alt ... order
 * @param {array} listOfTimePoints An array of time points in seconds; same length as listOfTrackPoints; may be null
 * @returns {array} Array with same number of entries as track points in the given list
 */
MapView.prototype.calcTrackColors = function (listOfTrackPoints, listOfTimePoints) {

    var trackColors = [];

    trackColors[0] = this.trackColorFromVarioValue(0.0);

    for (var index = 3; index < listOfTrackPoints.length; index += 3) {

        var altitudeDiff = listOfTrackPoints[index + 2] - listOfTrackPoints[index - 1];

        var timeDiff = 1.0;
        if (listOfTimePoints !== null)
            timeDiff = listOfTimePoints[index / 3] - listOfTimePoints[index / 3 - 1];

        var varioValue = altitudeDiff / timeDiff;

        var varioColor = this.trackColorFromVarioValue(varioValue);

        if (varioColor === undefined)
            console.log("undefined color vor vario value " + varioValue);

        trackColors[index / 3] = varioColor;
    }

    return trackColors;
};

/**
 * Creates a primitive for a flight track
 * @param {object} [track] Track object to add
 * @param {array} [track.listOfTrackPoints] An array of track points in long, lat, alt, long, lat, alt ... order
 * @param {array} [track.listOfTimePoints] An array of time points in seconds; same length as listOfTrackPoints; may be null
 * @param {array} [trackPointArray] An array of track points to use
 * @returns {Cesium.Primitive} created primitive object
 */
MapView.prototype.getFlightTrackPrimitive = function (track, trackPointArray) {

    var trackColors = this.calcTrackColors(track.listOfTrackPoints, track.listOfTimePoints);

    var trackPolyline = new Cesium.PolylineGeometry({
        positions: trackPointArray,
        width: 5,
        vertexFormat: Cesium.PolylineColorAppearance.VERTEX_FORMAT,
        colors: trackColors,
        colorsPerVertex: false
    });

    var primitive = new Cesium.Primitive({
        geometryInstances: new Cesium.GeometryInstance({
            geometry: trackPolyline
        }),
        appearance: new Cesium.PolylineColorAppearance({ translucent: false })
    });

    return primitive;
};

/**
 * Creates a wall primitive for a flight track to display relation to ground
 * @param {array} [trackPointArray] An array of track points to use
 * @returns {Cesium.Primitive} created wall primitive object
 */
MapView.prototype.getFlightTrackWallPrimitive = function (trackPointArray) {

    var wallGeometry = new Cesium.WallGeometry({
        positions: trackPointArray
    });

    var wallMaterial = Cesium.Material.fromType('Color');

    wallMaterial.uniforms.color = new Cesium.Color(0.5, 0.5, 1, 0.4);

    var wallPrimitive = new Cesium.Primitive({
        geometryInstances: new Cesium.GeometryInstance({
            geometry: Cesium.WallGeometry.createGeometry(wallGeometry)
        }),
        appearance: new Cesium.MaterialAppearance({
            translucent: true,
            material: wallMaterial,
            faceForward: true
        })
    });

    return wallPrimitive;
};

/**
 * Creates a ground primitive for a non-flight track
 * @param {object} [track] Track object to add
 * @param {string} [track.color] Color as "RRGGBB" string value
 * @param {array} [trackPointArray] An array of track points to use
 * @returns {Cesium.Primitive} created primitive object
 */
MapView.prototype.getGroundTrackPrimitive = function (track, trackPointArray) {

    var groundTrackPolyline = new Cesium.GroundPolylineGeometry({
        positions: trackPointArray,
        width: 5
    });

    var primitive = new Cesium.GroundPolylinePrimitive({
        geometryInstances: new Cesium.GeometryInstance({
            geometry: groundTrackPolyline,
            attributes: {
                color: Cesium.ColorGeometryInstanceAttribute.fromColor(Cesium.Color.fromCssColorString('#' + track.color))
            }
        }),
        appearance: new Cesium.PolylineColorAppearance({ translucent: false })
    });

    return primitive;
};

/**
 * Adds a track to the map
 * @param {object} [track] Track object to add
 * @param {string} [track.id] unique ID of the track
 * @param {string} [track.name] track name to add
 * @param {boolean} [track.isFlightTrack] indicates if track is a flight
 * @param {array} [track.listOfTrackPoints] An array of track points in long, lat, alt, long, lat, alt ... order
 * @param {array} [track.listOfTimePoints] An array of time points in seconds; same length as listOfTrackPoints; may be null
 * @param {string} [track.color] Color as "RRGGBB" string value, or undefined when track should be colored
 *                       according to climb and sink rate.
 */
MapView.prototype.addTrack = function (track) {

    this.removeTrack(track.id);

    console.log("adding list of track points, with ID " + track.id + " and " + track.listOfTrackPoints.length + " track points");

    var trackPointArray = Cesium.Cartesian3.fromDegreesArrayHeights(track.listOfTrackPoints);

    var primitive = undefined;
    var wallPrimitive = undefined;

    if (track.isFlightTrack) {
        primitive = this.getFlightTrackPrimitive(track, trackPointArray);
        wallPrimitive = this.getFlightTrackWallPrimitive(trackPointArray);

        this.trackPrimitivesCollection.add(wallPrimitive);

    } else {
        primitive = this.getGroundTrackPrimitive(track, trackPointArray);
    }

    this.trackPrimitivesCollection.add(primitive);

    var boundingSphere = Cesium.BoundingSphere.fromPoints(trackPointArray, null);

    this.trackIdToTrackDataMap[track.id] = {
        primitive: primitive,
        wallPrimitive: wallPrimitive,
        boundingSphere: boundingSphere
    };
};

/**
 * Zooms to a track on the map
 * @param {string} trackId unique ID of the track to zoom to
 */
MapView.prototype.zoomToTrack = function (trackId) {

    var trackData = this.trackIdToTrackDataMap[trackId];

    if (trackData !== undefined) {

        console.log("zooming to track with ID: " + trackId);

        this.viewer.camera.flyToBoundingSphere(trackData.boundingSphere);
    }
};

/**
 * Removes a track from the map
 * @param {string} trackId unique ID of the track
 */
MapView.prototype.removeTrack = function (trackId) {

    var trackData = this.trackIdToTrackDataMap[trackId];

    if (trackData !== undefined) {

        console.log("removing track with ID: " + trackId);

        this.trackPrimitivesCollection.remove(trackData.primitive);
        if (trackData.wallPrimitive !== undefined)
            this.trackPrimitivesCollection.remove(trackData.wallPrimitive);

        this.trackIdToTrackDataMap[trackId] = undefined;
    }
};

/**
 * Clears all tracks from the map
 */
MapView.prototype.clearAllTracks = function () {

    console.log("clearing all tracks");

    this.trackPrimitivesCollection.removeAll();

    this.trackIdToTrackDataMap = {};
};

/**
 * Called by the map ctor when the map is initialized and ready.
 */
MapView.prototype.onMapInitialized = function () {

    console.log("map is initialized");

    if (this.options.callback !== undefined)
        this.options.callback('onMapInitialized');
};

/**
 * Called by the marker pin link, in order to show details of the location.
 * @param {string} locationId Location ID of location to show
 */
MapView.prototype.onShowLocationDetails = function (locationId) {

    console.log("showing details to location: id=" + locationId);

    if (this.options.callback !== undefined)
        this.options.callback('onShowLocationDetails', locationId);
};

/**
 * Called by the marker pin link, in order to start navigating to the location.
 * @param {string} locationId Location ID of location to navigate to
 */
MapView.prototype.onNavigateToLocation = function (locationId) {

    console.log("navigation to location started: id=" + locationId);

    if (this.options.callback !== undefined)
        this.options.callback('onNavigateToLocation', locationId);
};

/**
 * Called by the "my position" pin link, in order to share the current location.
 */
MapView.prototype.onShareMyLocation = function () {

    console.log("sharing my location started");

    if (this.options.callback !== undefined)
        this.options.callback('onShareMyLocation');
};

/**
 * Called by the "add find result" pin link, in order to add the find result as a waypoint.
 * @param {Object} [options] An object with the following properties:
 * @param {String} [options.name] Name of the find result
 * @param {Number} [options.latitude] Latitude of the find result
 * @param {Number} [options.longitude] Longitude of the find result
 */
MapView.prototype.onAddFindResult = function (options) {

    console.log("adding find result as waypoint");

    if (this.options.callback !== undefined)
        this.options.callback('onAddFindResult', options);

    this.findResultMarker.show = false;

    // hide the info box
    this.viewer.selectedEntity = undefined;
};

/**
 * Called when a long-tap occured on the map.
 * @param {Object} [options] An object with the following properties:
 * @param {Number} [options.latitude] Latitude of the long tap
 * @param {Number} [options.longitude] Longitude of the long tap
 * @param {Number} [options.altitude] Altitude of the long tap
 */
MapView.prototype.onLongTap = function (options) {

    console.log("long-tap occured: lat=" + options.latitude +
        ", long=" + options.longitude +
        ", alt=" + options.altitude);

    if (this.options.callback !== undefined)
        this.options.callback('onLongTap', options);
};

/**
 * Called by the marker pin link, in order to add the location to tour planning.
 * @param {string} locationId Location ID of location to add
 */
MapView.prototype.onAddTourPlanLocation = function (locationId) {

    console.log("adding tour planning location: id=" + locationId);

    if (this.options.callback !== undefined)
        this.options.callback('onAddTourPlanLocation', locationId);
};

/**
 * Called by the "hide" link in the info text are of the flying range cone.
 */
MapView.prototype.hideFlyingRangeCone = function () {

    console.log("hiding flying range cone");

    this.viewer.entities.remove(this.flyingRangeCone);
};

/**
 * Calculates the current airac ID, based on the current date, which is good enough for our
 * purposes.
 * @returns {number} current airac ID
 */
function calcCurrentAiracId() {
    var baseAirac = new Date("2019-01-31"); // 1902 began on that day
    var diffInDays = Math.abs(new Date() - baseAirac) / 86400000;
    return 1902 + Math.floor(diffInDays / 28);
}
