# Where-to-fly web library

This is the npm library project that is used as web page components hosted in
a WebView inside the Where-to-fly app.

## General

The web library consists of the following classes:

- MapView: A 3D map view displaying location pins, tracks and CZML layer
  objects using [Cesium.js](https://github.com/CesiumGS/cesium).
- HeightProfileView: A chart view displaying the height profile of a track;
  the view is also used in MapView when a track is tapped. The view uses the
  [Chart.js](https://github.com/chartjs/Chart.js) library.

## Build

The library can be built using npm; run ...
1. `npm install` to get all dependent npm packages
2. `npm run build` to build an unoptimized library
3. `npm run build-release` to build an optimized and minified library
4. `npm run serve` run the development webserver; open
   http://localhost:8080/mapTest.html or http://localhost:8080/heightProfileTest.html

The app builds the library using the MSBuild project
`WhereToFly.Shared.WebLib.csproj` that is a dependency of the app projects.

## Usage

Include one of the following .js files:
- `js/WhereToFly.mapView.js` and `css/mapView.css`
- `js/WhereToFly.heightProfileView.js` and `css/heightProfileView.css`

Or use the `mapView.html` and `heightProfileView.html` files directly.

Create the `MapView` control:

    <div id="mapElement" class="map-element-fullscreen"></div>

    let map = new WhereToFly.mapView.MapView({
        id: 'mapElement',
        initialCenterPoint: { latitude: 47.6764385, longitude: 11.8710533 },
        initialViewingDistance: 5000.0
    });

## License

The library is licensed under the [BSD 2-clause license](../../../LICENSE.md).
