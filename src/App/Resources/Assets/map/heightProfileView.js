/**
 * Creates a new instance of HeightProfileView
 * @constructor
 * @param {object} [options] Options to use for initializing height profile view
 * @param {Number} [options.id] DOM ID of the canvas element to create height profile view in
 * @param {Number} [options.containerId] DOM ID of the container that contains the chart and the
 * toolbar
 * @param {boolean} [options.useDarkTheme] indicates if a dark theme should be used for the chart
 * @param {boolean} [options.setBodyBackgroundColor] indicates if body background should be themed
 * @param {boolean} [options.showCloseButton] indicates if a close button should be shown
 * @param {Function} [options.colorFromVarioValue] function to get a color from vario value; may
 * be undefined
 * @param {Function} [options.callback] action function callback
 */
function HeightProfileView(options) {

    console.log("HeightProfileView: creating new height profile view");

    this.isZoomAndPanActive = true;

    this.options = options || {
        id: 'chartElement',
        containerId: 'chartContainer',
        setBodyBackgroundColor: true,
        useDarkTheme: false,
        showCloseButton: false,
        callback: {}
    };

    if (this.options.callback === undefined)
        this.options.callback = callAction;

    this.trackColor = this.options.useDarkTheme ? '#00ffff' : '#0000ff';
    this.backgroundColor = this.options.useDarkTheme ? '#202124' : '#F5F5F5';
    this.axisColor = this.options.useDarkTheme ? '#f5f5f5' : '#202020';
    this.groundProfileColor = this.options.useDarkTheme ? '#404040C0' : '#808080C0';

    if (this.options.setBodyBackgroundColor)
        document.body.style.backgroundColor = this.backgroundColor;

    var chartContainer = document.getElementById(this.options.containerId);
    chartContainer.style.display = 'block';

    // also style the parent node, in case it's the standalone view
    chartContainer.style.backgroundColor = this.backgroundColor;

    var chartElement = document.getElementById(this.options.id);
    var ctx = chartElement.getContext('2d');

    var that = this;
    this.chart = new Chart(ctx, {
        type: 'line',
        data: {},
        options: {
            responsive: true,
            maintainAspectRatio: true,
            aspectRatio: 2,
            scales: {
                x: {
                    id: 'time',
                    type: 'time',
                    grid: {
                        color: this.axisColor,
                        zeroLineColor: this.axisColor
                    },
                    time: {
                        unit: 'hour',
                        displayFormats: {
                            hour: 'HH:mm'
                        },
                        stepSize: 0.25, // every 15 minutes
                        minUnit: 'second'
                    }
                },
                y: {
                    id: 'elevation',
                    type: 'linear',
                    position: 'left',
                    offset: true,
                    grid: {
                        color: this.axisColor,
                        zeroLineColor: this.axisColor
                    },
                    ticks: {
                        beginAtZero: false
                    }
                }
            },
            plugins: {
                legend: {
                    display: false
                },
                tooltip: {
                    enabled: false,
                    mode: 'nearest',
                    axis: 'x',
                    intersect: false,
                    external: function (context) {
                        that.updateTooltipElement(context.tooltip);
                    }
                },
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
                        mode: 'x'
                    },
                    zoom: {
                        wheel: {
                            enabled: true,
                            speed: 0.1 // mouse wheel zoom speed (in percent)
                        },
                        drag: {
                            enabled: false // don't use drag-to-zoom
                        },
                        pinch: {
                            enabled: true
                        },
                        mode: 'x'
                    },
                    limits: {
                        // to be set later
                        x: { min: null, max: null },
                        y: { min: null, max: null }
                    }
                }
            },
            hover: {
                mode: 'nearest',
                intersect: false,
                axis: 'x'
            },
            onHover: function (event, elements, chart) {
                that.onHover(elements);
            },
            onClick: function (event, elements, chart) {
                if (!that.isZoomAndPanActive)
                    that.onClick(elements);
            }
        }
    });

    var chartButtonClose = document.getElementById('chartButtonClose');
    chartButtonClose.style.display = this.options.showCloseButton ? 'block' : 'none';

    this.setModeZoomAndPan();
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

    this.chart.update(0);

    // update buttons
    var chartButtonModeHover = document.getElementById('chartButtonModeHover');
    chartButtonModeHover.classList.remove('chart-toolbar-button-disabled');

    var chartButtonModeZoomAndPan = document.getElementById('chartButtonModeZoomAndPan');
    chartButtonModeZoomAndPan.classList.add('chart-toolbar-button-disabled');
};

/**
 * Sets 'zoom-and-pan' mode where zoom and pan is enabled and clicks or hover
 * events are not given to the callback.
 */
HeightProfileView.prototype.setModeZoomAndPan = function () {

    this.isZoomAndPanActive = true;

    this.chart.options.plugins.zoom.zoom.enabled = true;
    this.chart.options.plugins.zoom.pan.enabled = true;

    this.chart.update(0);

    // update buttons
    var chartButtonModeHover = document.getElementById('chartButtonModeHover');
    chartButtonModeHover.classList.add('chart-toolbar-button-disabled');

    var chartButtonModeZoomAndPan = document.getElementById('chartButtonModeZoomAndPan');
    chartButtonModeZoomAndPan.classList.remove('chart-toolbar-button-disabled');
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

    if (track.listOfTimePoints == null) {
        // create time points from 0 to length, in seconds
        track.listOfTimePoints = [];
        for (var i = 0, len = track.listOfTrackPoints.length / 3; i < len; i++)
            track.listOfTimePoints[i] = i;
    }

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
            backgroundColor: this.backgroundColor,
            fill: false,
            label: 'Track',
            tension: 0.0,
            pointRadius: 0.0,
            borderColor: this.trackColor,
        }]
    };

    // need this to update the scales
    this.chart.update(0);

    var scale = this.chart.scales.x;
    var zoomPanLimits = this.chart.options.plugins.zoom.limits;
    zoomPanLimits.x.min = scale.min.valueOf(); // left value
    zoomPanLimits.x.max = scale.max.valueOf(); // right value
    zoomPanLimits.y.min = 60; // seconds of min. zoom level
    zoomPanLimits.y.max = (scale.max - scale.min).valueOf(); // the whole time range

    // need this to update the zoom and pan options
    this.chart.update(0);
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
        fill: 'start',
        label: 'Terrain',
        tension: 0.0,
        pointRadius: 0.0,
        backgroundColor: this.groundProfileColor,
        borderColor: 'rgba(0,128,0,255)'
    });

    this.chart.update(0);
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

    var chartContainer = document.getElementById(this.options.containerId);

    chartContainer.style.display = 'none';

    if (this.options.callback !== undefined)
        this.options.callback('onClose', null);
};

/**
 * Destroys the height profile view
 */
HeightProfileView.prototype.destroy = function () {

    this.chart.stop();
    this.chart.destroy();
    this.options = undefined;
    this.chart = undefined;
};

/**
 * Called by Chart.js when the user hovers over an element in the chart
 * @param {array} [elements] array of elements; may be empty
 */
HeightProfileView.prototype.onHover = function (elements) {

    //console.log("HeightProfileView: onHover called, with " + elements.length + " elements");

    if (elements.length > 0 &&
        this.options.callback !== undefined) {
        this.options.callback('onHover', elements[0].index);
    }
};

/**
 * Called by Chart.js when the user clicked on an element in the chart
 * @param {array} [elements] array of elements; may be empty
 */
HeightProfileView.prototype.onClick = function (elements) {

    //console.log("HeightProfileView: onClick called, with " + elements.length + " elements");

    if (elements.length > 0 &&
        this.options.callback !== undefined) {
        this.options.callback('onClick', elements[0].index);
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

    var timePoint = this.chart.data.datasets[0].data[tooltipModel.dataPoints[0].dataIndex].x;

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

            var currentDataPoint = that.chart.data.datasets[tooltipItem.datasetIndex].data[tooltipItem.dataIndex];
            values.trackHeight = currentDataPoint.y;

            if (tooltipItem.dataIndex === 0) {
                values.varioValue = 0.0;
            }
            else {
                var lastDataPoint = that.chart.data.datasets[tooltipItem.datasetIndex].data[tooltipItem.dataIndex - 1];

                var lastTrackHeight = lastDataPoint.y;

                var deltaTimeMs = currentDataPoint.x.valueOf() - lastDataPoint.x.valueOf();
                values.varioValue = (values.trackHeight - lastTrackHeight) / deltaTimeMs * 1000.0;
            }
        }

        if (tooltipItem.datasetIndex === 1) {
            values.groundHeight = that.chart.data.datasets[tooltipItem.datasetIndex].data[tooltipItem.dataIndex].y;
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

        text += "<div>Vario: " + values.varioValue.toFixed(1) + "m/s ";

        if (this.options.colorFromVarioValue !== undefined) {
            var varioColor = this.options.colorFromVarioValue(values.varioValue);
            text += "<div style='width:12px; height:12px; border:1px white solid; background-color:" + varioColor + "'></div>";
        }

        text += "</div>";
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

    var position = this.chart.canvas.getBoundingClientRect();

    var showLeft = tooltipModel.caretX > position.width / 2;

    var bodyFont = Chart.helpers.toFont(tooltipModel.options.bodyFont);

    // display, position, and set styles for font
    tooltipElement.style.opacity = 1;
    tooltipElement.style.left = showLeft ? (position.x + window.pageXOffset + 50) + 'px' : '';
    tooltipElement.style.right = !showLeft ? '10px' : '';
    tooltipElement.style.top = '60px';
    tooltipElement.style.font = bodyFont.string;
    tooltipElement.style.padding = tooltipModel.padding + 'px ' + tooltipModel.padding + 'px';
};
