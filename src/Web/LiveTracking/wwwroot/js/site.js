// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// sidebar
$(document).ready(function () {

    $('#sidebarCollapse').on('click', function () {
        $('#sidebar').toggleClass('sidebar-hidden');
    });

});

// function used to call to C#
function callAction(funcName, params) {
    console.log('call action: ' + funcName + ', params:' + JSON.stringify(params));
}

map = new MapView({
    id: 'mapElement',
    initialCenterPoint: { latitude: 47.083, longitude: 12.178 },
    initialZoomLevel: 5,
    hasMouse: true
});

map.setShadingMode('CurrentTime');

map.addLocationList([{
    id: 'crossingthealps2019-start',
    name: 'Start: Kampenwand',
    description: 'Start of the Crossing the Alps 2019 tour',
    type: 'Turnpoint',
    latitude: 47.754076,
    longitude: 12.352277,
    altitude: 0.0,
    isPlanTourLocation: false
}]);

map.addLocationList([{
    id: 'crossingthealps2019-end',
    name: 'End: Feltre',
    description: 'End of the Crossing the Alps 2019 tour',
    type: 'Turnpoint',
    latitude: 46.017779,
    longitude: 11.900711,
    altitude: 0.0,
    isPlanTourLocation: false
}]);

map.addTrack({
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

$('#findForm').submit(function (event) {

    event.preventDefault();

    geocodeAndShow($('#findValue')[0].value);

});

function geocodeAndShow(input) {

    console.log("geocoding find text: " + input);

    var endpoint = 'https://nominatim.openstreetmap.org/search';
    var resource = new Cesium.Resource({
        url: endpoint,
        queryParameters: {
            format: 'json',
            q: input
        }
    });

    return resource.fetchJson()
        .then(function (results) {
            return results.map(function (resultObject) {
                map.showFindResult({
                    name: input,
                    description: resultObject.display_name,
                    latitude: resultObject.lat,
                    longitude: resultObject.lon,
                    displayLatitude: resultObject.lat,
                    displayLongitude: resultObject.lon
                });
            });
        });
}
