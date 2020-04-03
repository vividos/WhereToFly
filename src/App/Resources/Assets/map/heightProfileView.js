/**
 * Creates a new instance of HeightProfileView
 * @constructor
 * @param {object} [options] Options to use for initializing height profile view
 * @param {Number} [options.id] DOM ID of the canvas element to create height profile view in
 */
function HeightProfileView(options) {

    console.log("creating new height profile view");

    this.options = options || {
        id: 'chartElement',
        callback: {}
    };

    if (this.options.callback === undefined)
        this.options.callback = callAction;

    var ctx = document.getElementById(this.options.id).getContext('2d');

    this.chart = new Chart(ctx, {
        type: 'line',
        data: {},
        options: {
            legend: {
                display: false
            },
            scales: {
                xAxes: [{
                    id: 'time',
                    type: 'time',
                    time: {
                        unit: 'hour',
                        displayFormats: {
                            hour: 'HH:mm'
                        },
                        stepSize: 1,
                        minUnit: 'second'
                    }
                }
                ],
                yAxes: [{
                    id: 'elevation',
                    type: 'linear',
                    position: 'left',
                    offset: true,
                    ticks: {
                        beginAtZero: true
                    }
                }]
            }
        }
    });
}

/**
 * Sets track to display the height profile for
 * @param {object} [track] Track object to add
 * @param {string} [track.id] unique ID of the track
 * @param {string} [track.name] track name to add
 * @param {array} [track.listOfTrackPoints] An array of track points in long, lat, alt, long, lat, alt ... order
 * @param {array} [track.listOfTimePoints] An array of time points in seconds since unix epoch;
 * same length as listOfTrackPoints; may be null
 */
HeightProfileView.prototype.setTrack = function (track) {

    console.log("setting height profile with " + track.listOfTrackPoints.length / 3 + " track points");

    var trackData = [];

    for (var i = 0, len = track.listOfTrackPoints.length; i < len; i += 3) {
        var timePoint = track.listOfTimePoints[i / 3];
        trackData.push({
            x: new Date(timePoint * 1000.0),
            y: track.listOfTrackPoints[i + 2],
        });
    }

    this.chart.data = {
        datasets: [{
            data: trackData,
            fill: false,
            label: 'Track',
            tension: 0.0,
            pointRadius: 0.0,
            borderColor: 'rgba(0,0,255,255)',
        }]
    };

    this.chart.update();
};

/**
 * adds a ground profile for an already added track
 * @param {object} [elevationArray] elevations to add
 */
HeightProfileView.prototype.addGroundProfile = function (elevationArray) {

    console.log("adding ground profile with " + elevationArray.length + " elevation points");

    var trackData = this.chart.data.datasets[0].data;
    var elevationData = [];
    for (var i = 0, len = trackData.length; i < len; i++) {
        elevationData.push({
            x: trackData[i].x,
            y: elevationArray[i]
        });
    }

    this.chart.data.datasets.push({
        data: elevationData,
        showLine: true,
        fill: true,
        label: 'Terrain',
        tension: 0.0,
        pointRadius: 0.0,
        bckgroundColor: 'rgba(128,128,128,0)',
    });

    this.chart.update();
};
