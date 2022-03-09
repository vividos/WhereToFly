﻿const path = require('path');

const webpack = require('webpack');
const HtmlWebpackPlugin = require('html-webpack-plugin');
const MiniCssExtractPlugin = require("mini-css-extract-plugin");
const CssMinimizerPlugin = require("css-minimizer-webpack-plugin");
const CopyWebpackPlugin = require('copy-webpack-plugin');
const TerserPlugin = require("terser-webpack-plugin");

module.exports = {
    mode: 'production',
    entry: {
        mapView: './src/js/mapView.js',
        heightProfileView: "./src/js/heightProfileView.js",
    },
    output: {
        clean: true,
        path: path.resolve(__dirname, 'dist'),
        filename: 'js/WhereToFly.[name].js',
        library: ["WhereToFly", "[name]"],
        libraryTarget: "umd",
    },
    devtool: 'source-map',
    module: {
        rules: [
            {
                test: /\.css$/,
                use: [{ loader: MiniCssExtractPlugin.loader }, "css-loader"],
                sideEffects: true
            }, {
                test: /\.(png|gif|jpg|jpeg|svg|xml|json|czml)$/,
                use: ['url-loader']
            },
            {
                // Strip cesium pragmas
                test: /\.js$/,
                enforce: 'pre',
                include: path.resolve(__dirname, 'node_modules/cesium/Source'),
                sideEffects: false,
                use: [{
                    loader: 'strip-pragma-loader',
                    options: {
                        pragmas: {
                            debug: false
                        }
                    }
                }]
            }
        ]
    },
    optimization: {
        usedExports: true,
        minimize: true,
        minimizer: [
            new TerserPlugin(),
            new CssMinimizerPlugin(),
        ]
    },
    performance: {
        hints: 'error',
        maxAssetSize: 4194304,
        maxEntrypointSize: 4194304,
    },
    plugins: [
        new HtmlWebpackPlugin({
            filename: "mapView.html",
            template: "src/mapView.html",
            chunks: ["mapView"]
        }),
        new HtmlWebpackPlugin({
            filename: "heightProfileView.html",
            template: "src/heightProfileView.html",
            chunks: ["heightProfileView"]
        }),
        new MiniCssExtractPlugin({
            filename: "css/[name].css"
        }),
        // Copy images and Cesium files
        new CopyWebpackPlugin({
            patterns: [
                { from: 'src/images', to: 'images' },
                { from: 'node_modules/cesium/Build/Cesium/Workers', to: 'js/Workers' },
                { from: 'node_modules/cesium/Build/Cesium/Assets', to: 'js/Assets' },
                { from: 'node_modules/cesium/Build/Cesium/Widgets', to: 'js/Widgets' }
            ],
        }),
        new webpack.DefinePlugin({
            // Define relative base path in cesium for loading assets
            CESIUM_BASE_URL: JSON.stringify('./js/')
        })
    ],
    amd: {
        // Enable webpack-friendly use of require in Cesium
        toUrlUndefined: true
    },
    resolve: {
        // Needed when using webpack 5 until this bug is fixed in CesiumJS:
        // https://github.com/CesiumGS/cesium/issues/9212
        exportsFields: []
    }
};