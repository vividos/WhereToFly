const { merge } = require("webpack-merge");
const common = require("./webpack.common.config.js");

const HtmlWebpackPlugin = require("html-webpack-plugin");

if (process.env.BING_MAPS_API_KEY === "")
    console.warn("environment variable BING_MAPS_API_KEY is not set!");

if (process.env.CESIUM_ION_API_KEY === "")
    console.warn("environment variable CESIUM_ION_API_KEY is not set!");

module.exports = merge(common, {
    mode: "development",
    output: {
        compareBeforeEmit: false,
        // this fixes the "Automatic publicPath is not supported in this browser"
        // error when using webpack serve
        publicPath: ""
    },
    performance: {
        hints: "warning",
        maxAssetSize: 18200000,
        maxEntrypointSize: 18200000
    },
    plugins: [
        new HtmlWebpackPlugin({
            filename: "mapTest.html",
            template: "src/mapTest.html",
            chunks: ["mapView"],
            apiKeys: {
                BING_MAPS_API_KEY: process.env.BING_MAPS_API_KEY,
                CESIUM_ION_API_KEY: process.env.CESIUM_ION_API_KEY
            }
        }),
        new HtmlWebpackPlugin({
            filename: "heightProfileTest.html",
            template: "src/heightProfileTest.html",
            chunks: ["heightProfileView"]
        })
    ]
});
