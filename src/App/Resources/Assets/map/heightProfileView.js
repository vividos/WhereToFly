/**
 * Creates a new instance of HeightProfileView
 * @constructor
 * @param {object} [options] Options to use for initializing height profile view
 * @param {Number} [options.id] DOM ID of the canvas element to create height profile view in
 * @param {boolean} [options.useDarkTheme] indicates if a dark theme should be used for the chart
 * @param {boolean} [options.showCloseButton] indicates if a close button should be shown
 */
function HeightProfileView(options) {

    console.log("HeightProfileView: creating new height profile view");

    this.isZoomAndPanActive = true;

    this.options = options || {
        id: 'chartElement',
        useDarkTheme: false,
        showCloseButton: false,
        callback: {}
    };

    if (this.options.callback === undefined)
        this.options.callback = callAction;

    var chartElement = document.getElementById(this.options.id);

    chartElement.parentElement.style.display = 'block';

    var ctx = chartElement.getContext('2d');

    this.backgroundColor = this.options.useDarkTheme ? '#202124' : '#F5F5F5';
    this.axisColor = this.options.useDarkTheme ? '#f5f5f5' : '#202020';
    this.groundProfileColor = this.options.useDarkTheme ? '#404040' : '#808080';

    // also style the parent node, in case it's the standalone view
    chartElement.style.backgroundColor = this.backgroundColor;

    var that = this;
    this.chart = new Chart(ctx, {
        type: 'line',
        data: {},
        options: {
            responsive: true,
            maintainAspectRatio: true,
            aspectRatio: 2,
            legend: {
                display: false
            },
            tooltips: {
                enabled: false,
                mode: 'nearest',
                axis: 'x',
                intersect: false,
                custom: function (tooltipModel) {
                    that.updateTooltipElement(tooltipModel);
                }
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
            plugins: {
                // the crosshair plugin is only used for the line, not for zooming
                crosshair: {
                    line: {
                        color: '#ffe666',
                        width: 2
                    },
                    sync: {
                        enabled: false
                    },
                    zoom: {
                        enabled: false
                    },
                },
                // the zoom plugin is used for panning and zooming
                zoom: {
                    pan: {
                        enabled: true,
                        mode: 'x',
                        rangeMin: {
                            x: null, // to be set later
                            y: null
                        },
                        rangeMax: {
                            x: null, // to be set later
                            y: null
                        },
                        speed: 20,
                        threshold: 10
                    },
                    zoom: {
                        enabled: true,
                        drag: false, // don't use drag-to-zoom
                        mode: 'x',
                        rangeMin: {
                            x: null, // to be set later
                            y: null
                        },
                        rangeMax: {
                            x: null, // to be set later
                            y: null
                        },
                        speed: 0.1, // mouse wheel zoom speed (in percent)
                        threshold: 2,
                        sensitivity: 3
                    }
                }
            },
            hover: {
                mode: 'nearest',
                intersect: false,
                axis: 'x'
            },
            onHover: function (event, elements) {
                if (!that.isZoomAndPanActive)
                    that.onHover(elements);
            },
            onClick: function (event, elements) {
                if (!that.isZoomAndPanActive)
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

    var chartButtonClose = document.getElementById('chartButtonClose');
    chartButtonClose.style.display = this.options.showCloseButton ? 'block' : 'none';
}

/**
 * Sets 'hover' mode where zoom and pan is disabled and the user can click or
 * touch on the graph to see the crosshair and to update the track marker, if
 * displayed.
 */
HeightProfileView.prototype.setModeHover = function () {

    this.isZoomAndPanActive = false;

    this.chart.options.plugins.zoom.zoom.enabled = false;
    this.chart.options.plugins.zoom.pan.enabled = false;

    this.chart.update();

    // update buttons
    var chartButtonModeHover = document.getElementById('chartButtonModeHover');
    chartButtonModeHover.classList.remove('chartToolbarButtonDisabled');

    var chartButtonModeZoomAndPan = document.getElementById('chartButtonModeZoomAndPan');
    chartButtonModeZoomAndPan.classList.add('chartToolbarButtonDisabled');
};

/**
 * Sets 'zoom-and-pan' mode where zoom and pan is enabled and clicks or hover
 * events are not given to the callback.
 */
HeightProfileView.prototype.setModeZoomAndPan = function () {

    this.isZoomAndPanActive = true;

    this.chart.options.plugins.zoom.zoom.enabled = true;
    this.chart.options.plugins.zoom.pan.enabled = true;

    this.chart.update();

    // update buttons
    var chartButtonModeHover = document.getElementById('chartButtonModeHover');
    chartButtonModeHover.classList.add('chartToolbarButtonDisabled');

    var chartButtonModeZoomAndPan = document.getElementById('chartButtonModeZoomAndPan');
    chartButtonModeZoomAndPan.classList.remove('chartToolbarButtonDisabled');
};

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

    // need this to update the scales
    this.chart.update();

    var scale = this.chart.scales["time"];
    var zoomPanOptions = this.chart.options.plugins.zoom;
    zoomPanOptions.pan.rangeMin.x = scale.min.valueOf(); // left value
    zoomPanOptions.pan.rangeMax.x = scale.max.valueOf(); // right value
    zoomPanOptions.zoom.rangeMin.x = 60; // seconds of min. zoom level
    zoomPanOptions.zoom.rangeMax.x = (scale.max - scale.min).valueOf(); // the whole time range

    // need this to update the zoom and pan options
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
 * Resets zoom level
 */
HeightProfileView.prototype.resetZoom = function () {

    this.chart.resetZoom();
};

/**
 * Hides the height profile view
 */
HeightProfileView.prototype.hide = function () {

    var chartElement = document.getElementById(this.options.id);

    chartElement.parentElement.style.display = 'none';

    if (this.options.callback !== undefined)
        this.options.callback('onClose', null);
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

/**
 * @typedef {Object} TooltipTrackPointInfo
 * @property {Date} timePoint time point of track; may be undefined
 * @property {number} elapsedTime elapsed time, in seconds since track start
 * @property {number} trackHeight track altitude, in m
 * @property {number} groundHeight ground height, in m; may be undefined
 * @property {number} varioValue variometer climb or sink value; in m/s
 */

/**
 * Returns data about a tooltip track point. Depending on the track point,
 * various infos are returned.
 * @param {object} tooltipModel tooltip model
 * @returns {TooltipTrackPointInfo} track point info
 */
HeightProfileView.prototype.getTrackTooltipInfos = function (tooltipModel) {

    var values = {};

    var timePoint = this.chart.data.datasets[0].data[tooltipModel.dataPoints[0].index].x;

    if (timePoint.getFullYear() === 1970)
        values.elapsedTime = timePoint.valueOf() / 1000.0;
    else {
        values.timePoint = timePoint;
        var startTime = this.chart.data.datasets[0].data[0].x;
        values.elapsedTime = (values.timePoint - startTime).valueOf() / 1000.0;
    }

    var that = this;
    tooltipModel.dataPoints.forEach(function (tooltipItem) {
        if (tooltipItem.datasetIndex === 0) {

            var currentDataPoint = that.chart.data.datasets[tooltipItem.datasetIndex].data[tooltipItem.index];
            values.trackHeight = currentDataPoint.y;

            if (tooltipItem.index === 0) {
                values.varioValue = 0.0;
            }
            else {
                var lastDataPoint = that.chart.data.datasets[tooltipItem.datasetIndex].data[tooltipItem.index - 1];

                var lastTrackHeight = lastDataPoint.y;

                var deltaTimeMs = currentDataPoint.x.valueOf() - lastDataPoint.x.valueOf();
                values.varioValue = (values.trackHeight - lastTrackHeight) / deltaTimeMs * 1000.0;
            }
        }

        if (tooltipItem.datasetIndex === 1) {
            values.groundHeight = that.chart.data.datasets[tooltipItem.datasetIndex].data[tooltipItem.index].y;
        }
    });

    return values;
};

/**
 * Formats the tooltip text from given tooltip model
 * @param {object} tooltipModel tooltip model
 */
HeightProfileView.prototype.formatTooltipText = function (tooltipModel) {

    var values = this.getTrackTooltipInfos(tooltipModel);

    var text = "";

    if (values.timePoint !== undefined)
        text += "<div>Time: " + values.timePoint.toLocaleTimeString() + "</div>";

    text += "<div>Elapsed: " + new Date((values.elapsedTime - 60.0 * 60.0) * 1000.0).toLocaleTimeString() + "</div>";

    if (values.trackHeight !== undefined)
        text += "<div>Altitude: " + values.trackHeight.toFixed(1) + "m</div>";

    if (values.groundHeight !== undefined) {
        text += "<div>Ground: " + values.groundHeight.toFixed(1) + "m</div>";
        text += "<div>AGL: " + (values.trackHeight - values.groundHeight).toFixed(1) + "m</div>";
    }

    if (values.varioValue !== undefined) {
        text += "<div>Vario: " + values.varioValue.toFixed(1) + "m/s</div>";
    }

    return text;
};

/**
 * Updates the tooltip DOM element
 * @param {object} tooltipModel tooltip model
 */
HeightProfileView.prototype.updateTooltipElement = function (tooltipModel) {

    var tooltipElement = document.getElementById('chartjs-tooltip');

    if (!tooltipElement) {
        tooltipElement = document.createElement('div');
        tooltipElement.id = 'chartjs-tooltip';
        this.chart.canvas.parentNode.appendChild(tooltipElement);
    }

    // hide if no tooltip
    if (tooltipModel.opacity === 0) {
        tooltipElement.style.opacity = 0;
        return;
    }

    // set caret position
    tooltipElement.classList.remove('above', 'below', 'no-transform');
    if (tooltipModel.yAlign)
        tooltipElement.classList.add(tooltipModel.yAlign);
    else
        tooltipElement.classList.add('no-transform');

    // set text
    tooltipElement.innerHTML = this.formatTooltipText(tooltipModel);

    var positionY = this.chart.canvas.offsetTop;
    var positionX = this.chart.canvas.offsetLeft;

    // display, position, and set styles for font
    tooltipElement.style.opacity = 1;
    tooltipElement.style.left = positionX + tooltipModel.caretX + 'px';
    tooltipElement.style.top = positionY + tooltipModel.caretY + 'px';
    tooltipElement.style.fontFamily = tooltipModel._bodyFontFamily;
    tooltipElement.style.fontSize = tooltipModel.bodyFontSize + 'px';
    tooltipElement.style.fontStyle = tooltipModel._bodyFontStyle;
    tooltipElement.style.padding = tooltipModel.yPadding + 'px ' + tooltipModel.xPadding + 'px';
};
