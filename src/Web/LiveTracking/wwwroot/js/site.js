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

mapping = [];
timeoutMapping = [];

mapping['where-to-fly://TestPos/data'] = 'testpos';

map.addLocationList([{
    id: 'where-to-fly://TestPos/data',
    name: 'Live Waypoint',
    description: '',
    type: 'LiveWaypoint',
    latitude: 0.0,
    longitude: 0.0,
    altitude: 0.0,
    isPlanTourLocation: false
}]);

updateLiveWaypoint('where-to-fly://TestPos/data');

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

function updateLiveWaypoint(liveWaypointUri) {

    console.log('updating live waypoint ' + liveWaypointUri);
    $.ajax({
        url: '/?handler=UpdateLiveWaypoint',
        data: {
            Uri: liveWaypointUri
        }
    })
        .done(function (result) {

            console.log('update result: ' + JSON.stringify(result));

            if (result.data !== undefined) {
                result.data.id = liveWaypointUri;
                map.updateLocation(result.data);

                if (mapping[liveWaypointUri] !== undefined) {
                    var idDesc = '#' + mapping[liveWaypointUri] + 'Description';

                    $(idDesc)[0].textContent = result.data.description;

                    var idLastUpdate = '#' + mapping[liveWaypointUri] + 'LastUpdate';

                    $(idLastUpdate)[0].textContent = 'Last update: ' + new Date().toLocaleTimeString();
                }
            }

            if (typeof result === 'string') {

                if (mapping[liveWaypointUri] !== undefined) {
                    var idDesc2 = '#' + mapping[liveWaypointUri] + 'Description';

                    $(idDesc2)[0].textContent = 'Error: ' + result;
                }
            }

            // schedule next update based on reported date
            if (result.nextRequestDate !== undefined)
                scheduleNextUpdate(liveWaypointUri, result.nextRequestDate);
        });
}

function scheduleNextUpdate(liveWaypointUri, nextRequestDate) {
    console.log('scheduling next update for live waypoint ' + liveWaypointUri);

    if (timeoutMapping[liveWaypointUri] !== undefined)
        clearTimeout(timeoutMapping[liveWaypointUri]);

    var now = new Date();
    var nextRequest = new Date(nextRequestDate);

    var millisTillUpdate = nextRequest - now;
    if (millisTillUpdate < 0)
        millisTillUpdate = 10 * 1000; // schedule in 10 seconds

    console.log("scheduling update in " + millisTillUpdate + " milliseconds");

    var myTimeout = setTimeout(function () {
        console.log("next update for " + liveWaypointUri + " is due!");
        updateLiveWaypoint(liveWaypointUri);
    }, millisTillUpdate);

    timeoutMapping[liveWaypointUri] = myTimeout;
}
