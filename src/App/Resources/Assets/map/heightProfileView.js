/**
 * Creates a new instance of HeightProfileView
 * @constructor
 * @param {object} [options] Options to use for initializing height profile view
 * @param {Number} [options.id] DOM ID of the canvas element to create height profile view in
 * @param {boolean} [options.useDarkTheme] indicates if a dark theme should be used for the chart
 */
function HeightProfileView(options) {

    console.log("HeightProfileView: creating new height profile view");

    this.options = options || {
        id: 'chartElement',
        useDarkTheme: false,
        callback: {}
    };

    if (this.options.callback === undefined)
        this.options.callback = callAction;

    var ctx = document.getElementById(this.options.id).getContext('2d');

    this.backgroundColor = this.options.useDarkTheme ? '#202124' : '#F5F5F5';
    this.axisColor = this.options.useDarkTheme ? '#f5f5f5' : '#202020';
    this.groundProfileColor = this.options.useDarkTheme ? '#404040' : '#808080';

    // also style the parent node, in case it's the standalone view
    var parent = document.getElementById(this.options.id);
    parent.style.backgroundColor = this.backgroundColor;

    var that = this;
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
                    gridLines: {
                        color: this.axisColor,
                        zeroLineColor: this.axisColor
                    },
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
                    gridLines: {
                        color: this.axisColor,
                        zeroLineColor: this.axisColor
                    },
                    ticks: {
                        beginAtZero: true
                    }
                }]
            },
            onHover: function (event, elements) {
                that.onHover(elements);
            },
            onClick: function (event, elements) {
                that.onClick(elements);
            }
        }
    });

    // background color can't be set directly, so use a plugin
    Chart.plugins.register({
        beforeDraw: function (chartInstance) {
            var ctx = chartInstance.chart.ctx;
            ctx.fillStyle = that.backgroundColor;
            ctx.fillRect(0, 0, chartInstance.chart.width, chartInstance.chart.height);
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

    console.log("HeightProfileView: setting height profile with " + track.listOfTrackPoints.length / 3 + " track points");

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

    console.log("HeightProfileView: adding ground profile with " + elevationArray.length + " elevation points");

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
        backgroundColor: this.groundProfileColor,
        borderColor: 'rgba(0,128,0,255)'
    });

    this.chart.update();
};

/**
 * Called by Chart.js when the user hovers over an element in the chart
 * @param {array} [elements] array of elements; may be empty
 */
HeightProfileView.prototype.onHover = function (elements) {

    console.log("HeightProfileView: onHover called, with " + elements.length + " elements");

    if (elements.length > 0 &&
        this.options.callback !== undefined) {
        this.options.callback('onHover', elements[0]._index);
    }
};

/**
 * Called by Chart.js when the user clicked on an element in the chart
 * @param {array} [elements] array of elements; may be empty
 */
HeightProfileView.prototype.onClick = function (elements) {

    console.log("HeightProfileView: onClick called, with " + elements.length + " elements");

    if (elements.length > 0 &&
        this.options.callback !== undefined) {
        this.options.callback('onClick', elements[0]._index);
    }
};
