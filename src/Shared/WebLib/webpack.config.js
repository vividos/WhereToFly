const { merge } = require('webpack-merge');
const common = require('./webpack.common.config.js');

const webpack = require('webpack');
const HtmlWebpackPlugin = require('html-webpack-plugin');

module.exports = merge(common, {
    mode: 'development',
    output: {
        compareBeforeEmit: false,
        // this fixes the "Automatic publicPath is not supported in this browser"
        // error when using webpack serve
        publicPath: ''
    },
    performance: {
        hints: 'warning',
        maxAssetSize: 18200000,
        maxEntrypointSize: 18200000,
    },
    plugins: [
        new HtmlWebpackPlugin({
            filename: "mapTest.html",
            template: "src/mapTest.html",
            chunks: ["mapView"]
        }),
        new HtmlWebpackPlugin({
            filename: "heightProfileTest.html",
            template: "src/heightProfileTest.html",
            chunks: ["heightProfileView"]
        }),
    ]
});
