const path = require('path');

const webpack = require('webpack');
const MiniCssExtractPlugin = require("mini-css-extract-plugin");
const CopyWebpackPlugin = require('copy-webpack-plugin');

module.exports = {
    mode: 'development',
    entry: {
        site: './src/js/site.js'
    },
    output: {
        filename: 'js/[name].bundle.js',
        path: path.resolve(__dirname, '..', 'wwwroot')
    },
    devtool: 'source-map',
    resolve: {
        // resolve rules for some require() calls in CesiumJS
        fallback: {
            // these modules are used in loadWithHttpRequest(), which is never used
            http: false,
            https: false,
            url: false,
            zlib: false,
        },
    },
    module: {
        rules: [
            {
                test: /\.css$/,
                use: [{ loader: MiniCssExtractPlugin.loader }, "css-loader"],
                sideEffects: true
            },
            { test: /\.eot(\?v=\d+\.\d+\.\d+)?$/, use: ['file-loader'] },
            { test: /\.(woff|woff2)$/, use: ["url-loader?prefix=font/&limit=5000"] },
            { test: /\.ttf(\?v=\d+\.\d+\.\d+)?$/, use: ["url-loader?limit=10000&mimetype=application/octet-stream"] }
        ]
    },
    plugins: [
        new MiniCssExtractPlugin({
            filename: "css/[name].css"
        }),
        // Copy images and data files
        new CopyWebpackPlugin({
            patterns: [
                { from: 'src/favicon.png', to: 'favicon.png' },
                { from: 'src/data', to: 'data' },
                { from: 'node_modules/wheretofly-weblib/dist/images', to: 'images' },
                { from: 'node_modules/wheretofly-weblib/dist/js/Assets', to: 'js/Assets' },
                { from: 'node_modules/wheretofly-weblib/dist/js/ThirdParty', to: 'js/ThirdParty' },
                { from: 'node_modules/wheretofly-weblib/dist/js/Widgets', to: 'js/Widgets' },
                { from: 'node_modules/wheretofly-weblib/dist/js/Workers', to: 'js/Workers' }
            ]
        })
    ],
};
