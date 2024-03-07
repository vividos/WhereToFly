/*!
  * chartjs-plugin-simple-crosshair v1.0.1
  * https://www.chartjs.org
  * (c) Abel Heinsbroek, vividos
  * Released under the MIT license
  * This is a simplified version of the chartjs-plugin-crosshair from Abel
  * Heinsbroek, modified to work under ChartJS 3.x/4.x and to remove zoom and
  * sync features.
  * History:
  * 1.0.0: Initial version, forked and removed zoom and sync features
  * 1.0.1: Set args.changed instead of calling chart.draw(); also cleaned up
  *        code.
  */
import { valueOrDefault } from "chart.js/helpers";

export default {

    id: "crosshair",

    defaults: {
        line: {
            color: "#F66",
            width: 1,
            dashPattern: []
        }
    },

    afterInit: function(chart) {

        if (!chart.config.options.scales.x)
            return;

        const xScaleType = chart.config.options.scales.x.type

        if (xScaleType !== "linear" &&
            xScaleType !== "time" &&
            xScaleType !== "category" &&
            xScaleType !== "logarithmic" &&
            xScaleType !== "timeseries")
            return;

        if (chart.options.plugins.crosshair === undefined)
            chart.options.plugins.crosshair = this.defaults;

        chart.crosshair = {
            enabled: false,
            x: null
        };
    },

    getOption: function(chart, category, name) {
        return valueOrDefault(
            chart.options.plugins.crosshair[category]
                ? chart.options.plugins.crosshair[category][name]
                : undefined,
            this.defaults[category][name]);
    },
    getXScale: function(chart) {
        return chart.data.datasets.length
            ? chart.scales[chart.getDatasetMeta(0).xAxisID]
            : null;
    },
    getYScale: function(chart) {
        return chart.scales[chart.getDatasetMeta(0).yAxisID];
    },

    afterEvent: function(chart, args) {

        if (chart.config.options.scales.x.length === 0)
            return;

        const e = args.event;

        const xScaleType = chart.config.options.scales.x.type;

        if (xScaleType !== "linear" &&
            xScaleType !== "time" &&
            xScaleType !== "category" &&
            xScaleType !== "logarithmic" &&
            xScaleType !== "timeseries")
            return;

        const xScale = this.getXScale(chart);

        if (!xScale)
            return;

        chart.crosshair.enabled = (e.type !== "mouseout" &&
            (e.x > xScale.getPixelForValue(xScale.min) &&
                e.x < xScale.getPixelForValue(xScale.max)));

        if (!chart.crosshair.enabled)
            return;

        chart.crosshair.x = e.x;

        args.changed = true;
    },

    afterDraw: function(chart) {

        if (!chart.crosshair.enabled)
            return;

        this.drawTraceLine(chart);
    },

    drawTraceLine: function(chart) {

        const yScale = this.getYScale(chart);

        const lineWidth = this.getOption(chart, "line", "width");
        const color = this.getOption(chart, "line", "color");
        const dashPattern = this.getOption(chart, "line", "dashPattern");

        const lineX = chart.crosshair.x;

        chart.ctx.beginPath();
        chart.ctx.setLineDash(dashPattern);
        chart.ctx.moveTo(lineX, yScale.getPixelForValue(yScale.max));
        chart.ctx.lineWidth = lineWidth;
        chart.ctx.strokeStyle = color;
        chart.ctx.lineTo(lineX, yScale.getPixelForValue(yScale.min));
        chart.ctx.stroke();
        chart.ctx.setLineDash([]);
    }
};
