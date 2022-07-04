import * as Utils from './utils.js';

// polyfill for UWP
if (CanvasRenderingContext2D.prototype.resetTransform === undefined) {
    CanvasRenderingContext2D.prototype.resetTransform = function() {
        this.setTransform(1, 0, 0, 1, 0, 0);
    }
}

// Chart.js
import {
    Chart,
    LineController,
    LineElement,
    LinearScale,
    PointElement,
    TimeScale,
    Tooltip
} from 'chart.js';

import * as helpers from 'chart.js/helpers';

// Chart.js plugins
import 'chartjs-adapter-moment';

import CrosshairPlugin from 'chartjs-plugin-crosshair';
import ZoomPlugin from 'chartjs-plugin-zoom';

// local
import '../css/heightProfileView.css';

Chart.register(
    LineController, LineElement, LinearScale, PointElement,
    TimeScale, Tooltip,
    CrosshairPlugin, ZoomPlugin);

export class HeightProfileView {

    /**
     * Console log style
     */
    static consoleLogStyle = "background: darkblue; color: yellow; padding: 1px 3px; border-radius: 3px;";

    /**
     * Logs a message to the console, just like console.log, but with styled output.
     */
    static log(message) {
        console.log("%cHeightProfileView%c" + message, HeightProfileView.consoleLogStyle);
    }

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
    constructor(options) {

        HeightProfileView.log("creating new height profile view");

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
            this.options.callback = Utils.callAction;

        this.trackColor = this.options.useDarkTheme ? '#00ffff' : '#0000ff';
        this.backgroundColor = this.options.useDarkTheme ? '#202124' : '#F5F5F5';
        this.axisColor = this.options.useDarkTheme ? '#f5f5f5' : '#202020';
        this.groundProfileColor = this.options.useDarkTheme ? '#404040C0' : '#808080C0';

        if (this.options.setBodyBackgroundColor)
            document.body.style.backgroundColor = this.backgroundColor;

        let chartContainer = document.getElementById(this.options.containerId);
        chartContainer.style.display = 'block';

        // also style the parent node, in case it's the standalone view
        chartContainer.style.backgroundColor = this.backgroundColor;

        let chartElement = document.getElementById(this.options.id);
        let ctx = chartElement.getContext('2d');

        let that = this;
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
                onHover: function (_event, elements, _chart) {
                    that.onHover(elements);
                },
                onClick: function (_event, elements, _chart) {
                    if (!that.isZoomAndPanActive)
                        that.onClick(elements);
                }
            }
        });

        let chartButtonClose = document.getElementById('chartButtonClose');
        chartButtonClose.style.display = this.options.showCloseButton ? 'block' : 'none';

        this.setModeZoomAndPan();
    }

    /**
     * Sets 'hover' mode where zoom and pan is disabled and the user can click or
     * touch on the graph to see the crosshair and to update the track marker, if
     * displayed.
     */
    setModeHover() {

        this.isZoomAndPanActive = false;

        this.chart.options.plugins.zoom.zoom.enabled = false;
        this.chart.options.plugins.zoom.pan.enabled = false;

        this.chart.update(0);

        // update buttons
        let chartButtonModeHover = document.getElementById('chartButtonModeHover');
        chartButtonModeHover.classList.remove('chart-toolbar-button-disabled');

        let chartButtonModeZoomAndPan = document.getElementById('chartButtonModeZoomAndPan');
        chartButtonModeZoomAndPan.classList.add('chart-toolbar-button-disabled');
    }

    /**
     * Sets 'zoom-and-pan' mode where zoom and pan is enabled and clicks or hover
     * events are not given to the callback.
     */
    setModeZoomAndPan() {

        this.isZoomAndPanActive = true;

        this.chart.options.plugins.zoom.zoom.enabled = true;
        this.chart.options.plugins.zoom.pan.enabled = true;

        this.chart.update(0);

        // update buttons
        let chartButtonModeHover = document.getElementById('chartButtonModeHover');
        chartButtonModeHover.classList.add('chart-toolbar-button-disabled');

        let chartButtonModeZoomAndPan = document.getElementById('chartButtonModeZoomAndPan');
        chartButtonModeZoomAndPan.classList.remove('chart-toolbar-button-disabled');
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
    setTrack(track) {

        HeightProfileView.log("setting height profile with " +
            track.listOfTrackPoints.length / 3 + " track points");

        let trackData = [];

        if (track.listOfTimePoints === null) {
            // create time points from 0 to length, in seconds
            track.listOfTimePoints = [];
            for (let timePointIndex = 0, maxTimePointIndex = track.listOfTrackPoints.length / 3; timePointIndex < maxTimePointIndex; timePointIndex++)
                track.listOfTimePoints[timePointIndex] = timePointIndex;
        }

        for (let trackPointIndex = 0, len = track.listOfTrackPoints.length; trackPointIndex < len; trackPointIndex += 3) {
            let timePoint = track.listOfTimePoints[trackPointIndex / 3];
            trackData.push({
                x: new Date(timePoint * 1000.0),
                y: track.listOfTrackPoints[trackPointIndex + 2],
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

        this.updateZoomPanLimits();
    }

    /**
     * Updates the current zoom and pan limits, based on the current X axis scale.
     * After adding data, be sure to call chart.update() so that scales are current.
     * */
    updateZoomPanLimits() {

        let scale = this.chart.scales.x;
        let zoomPanLimits = this.chart.options.plugins.zoom.limits;
        zoomPanLimits.x.min = scale.min.valueOf(); // left value
        zoomPanLimits.x.max = scale.max.valueOf(); // right value
        zoomPanLimits.y.min = 60; // seconds of min. zoom level
        zoomPanLimits.y.max = (scale.max - scale.min).valueOf(); // the whole time range

        // need this to update the zoom and pan options
        this.chart.update(0);
    }

    /**
     * adds a ground profile for an already added track
     * @param {object} [elevationArray] elevations to add
     */
    addGroundProfile(elevationArray) {

        HeightProfileView.log("adding ground profile with " +
            elevationArray.length + " elevation points");

        let trackData = this.chart.data.datasets[0].data;
        let elevationData = [];
        for (let i = 0, len = trackData.length; i < len; i++) {
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
    }

    /**
     * Adds new track points to the height profile view, for the currently displayed track.
     * @param {object} [track] Track data to add
     * @param {string} [track.id] unique ID of the track
     * @param {array} [track.listOfTrackPoints] An array of additional track points in long, lat, alt,
     * long, lat, alt ... order
     * @param {array} [track.listOfTimePoints] An array of additional time points in seconds since
     * unix epoch; * same length as listOfTrackPoints; may be null
     */
    addTrackPoints(track) {

        if (track.listOfTrackPoints.length === 0 ||
            track.listOfTrackPoints.length !== track.listOfTimePoints.length * 3)
            return;

        let trackData = this.chart.data.datasets[0].data;
        if (trackData.length === 0) {
            this.setTrack(track);
            return;
        }

        let lastDate = trackData[trackData.length - 1].x;
        let lastTimePoint = lastDate.getTime() / 1000;

        let newStartTimePos = track.listOfTimePoints.indexOf(lastTimePoint) + 1;
        if (newStartTimePos === 0 ||
            newStartTimePos >= track.listOfTimePoints.length)
            return;

        HeightProfileView.log("adding " +
            (track.listOfTimePoints.length - newStartTimePos) +
            " track points to height profile");

        for (let trackPointIndex = newStartTimePos * 3, len = track.listOfTrackPoints.length; trackPointIndex < len; trackPointIndex += 3) {
            let timePoint = track.listOfTimePoints[trackPointIndex / 3];
            trackData.push({
                x: new Date(timePoint * 1000.0),
                y: track.listOfTrackPoints[trackPointIndex + 2],
            });
        }

        this.chart.update(0);

        this.updateZoomPanLimits();
    }

    /**
     * Resets zoom level
     */
    resetZoom() {

        this.chart.resetZoom();
    }

    /**
     * Hides the height profile view
     */
    hide() {

        let chartContainer = document.getElementById(this.options.containerId);

        chartContainer.style.display = 'none';

        if (this.options.callback !== undefined)
            this.options.callback('onClose', null);
    }

    /**
     * Destroys the height profile view
     */
    destroy() {

        this.chart.stop();
        this.chart.destroy();
        this.options = undefined;
        this.chart = undefined;
    }

    /**
     * Called by Chart.js when the user hovers over an element in the chart
     * @param {array} [elements] array of elements; may be empty
     */
    onHover(elements) {

        if (elements.length > 0 &&
            this.options.callback !== undefined) {
            this.options.callback('onHover', elements[0].index);
        }
    }

    /**
     * Called by Chart.js when the user clicked on an element in the chart
     * @param {array} [elements] array of elements; may be empty
     */
    onClick(elements) {

        if (elements.length > 0 &&
            this.options.callback !== undefined) {
            this.options.callback('onClick', elements[0].index);
        }
    }

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
    getTrackTooltipInfos(tooltipModel) {

        let values = {};

        let timePoint = this.chart.data.datasets[0].data[tooltipModel.dataPoints[0].dataIndex].x;

        if (timePoint.getFullYear() === 1970)
            values.elapsedTime = timePoint.valueOf() / 1000.0;
        else {
            values.timePoint = timePoint;
            let startTime = this.chart.data.datasets[0].data[0].x;
            values.elapsedTime = (values.timePoint - startTime).valueOf() / 1000.0;
        }

        let that = this;
        tooltipModel.dataPoints.forEach(function (tooltipItem) {
            if (tooltipItem.datasetIndex === 0) {

                let currentDataPoint = that.chart.data.datasets[tooltipItem.datasetIndex].data[tooltipItem.dataIndex];
                values.trackHeight = currentDataPoint.y;

                if (tooltipItem.dataIndex === 0) {
                    values.varioValue = 0.0;
                }
                else {
                    let lastDataPoint = that.chart.data.datasets[tooltipItem.datasetIndex].data[tooltipItem.dataIndex - 1];

                    let lastTrackHeight = lastDataPoint.y;

                    let deltaTimeMs = currentDataPoint.x.valueOf() - lastDataPoint.x.valueOf();
                    values.varioValue = (values.trackHeight - lastTrackHeight) / deltaTimeMs * 1000.0;
                }
            }

            if (tooltipItem.datasetIndex === 1) {
                values.groundHeight = that.chart.data.datasets[tooltipItem.datasetIndex].data[tooltipItem.dataIndex].y;
            }
        });

        return values;
    }

    /**
     * Formats an elapsed time value as time span, in the format h:mm:ss; when the
     * elapsed time is greater than a day, the format is d.hh:mm:ss.
     * @param {any} elapsedTime elapsed time in milliseconds
     * @returns formatted time span text
     */
    static formatTimeSpan(elapsedTime) {

        const elapsed = new Date((elapsedTime - 60.0 * 60.0) * 1000.0);

        let hour = ('0' + elapsed.getHours()).slice(-2);
        let minutes = ('0' + elapsed.getMinutes()).slice(-2);
        let seconds = ('0' + elapsed.getSeconds()).slice(-2);

        if (elapsed.getDate() === 1)
            return (hour + ':' + minutes + ':' + seconds).replace(/^0/, '');

        let days = elapsed.getDate();

        return days + "." + hour + ':' + minutes + ':' + seconds;
    }

    /**
     * Formats the tooltip text from given tooltip model
     * @param {object} tooltipModel tooltip model
     */
    formatTooltipText(tooltipModel) {

        let values = this.getTrackTooltipInfos(tooltipModel);

        let text = "";

        if (values.timePoint !== undefined)
            text += "<div>Time: " + values.timePoint.toLocaleTimeString() + "</div>";

        text += "<div>Elapsed: " + HeightProfileView.formatTimeSpan(values.elapsedTime) + "</div>";

        if (values.trackHeight !== undefined)
            text += "<div>Altitude: " + values.trackHeight.toFixed(1) + "m</div>";

        if (values.groundHeight !== undefined) {
            text += "<div>Ground: " + values.groundHeight.toFixed(1) + "m</div>";
            text += "<div>AGL: " + (values.trackHeight - values.groundHeight).toFixed(1) + "m</div>";
        }

        if (values.varioValue !== undefined) {

            text += "<div>Vario: " + values.varioValue.toFixed(1) + "m/s ";

            if (this.options.colorFromVarioValue !== undefined) {
                const varioColor = this.options.colorFromVarioValue(values.varioValue);
                text += "<div style='width:12px; height:12px; border:1px white solid; background-color:" + varioColor + "'></div>";
            }

            text += "</div>";
        }

        return text;
    }

    /**
     * Updates the tooltip DOM element
     * @param {object} tooltipModel tooltip model
     */
    updateTooltipElement(tooltipModel) {

        let tooltipElement = document.getElementById('chartjs-tooltip');

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

        let position = this.chart.canvas.getBoundingClientRect();

        let showLeft = tooltipModel.caretX > position.width / 2;

        let bodyFont = helpers.toFont(tooltipModel.options.bodyFont);

        // display, position, and set styles for font
        tooltipElement.style.opacity = 1;
        tooltipElement.style.left = showLeft ? (position.x + window.scrollX + 50) + 'px' : '';
        tooltipElement.style.right = !showLeft ? '10px' : '';
        tooltipElement.style.top = '60px';
        tooltipElement.style.font = bodyFont.string;
        tooltipElement.style.padding = tooltipModel.padding + 'px ' + tooltipModel.padding + 'px';
    }
}
