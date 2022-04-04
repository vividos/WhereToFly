const path = require('path');

const MiniCssExtractPlugin = require("mini-css-extract-plugin");
const CopyWebpackPlugin = require('copy-webpack-plugin');
const TerserPlugin = require("terser-webpack-plugin");

module.exports = {
    mode: 'production',
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
    optimization: {
        usedExports: true,
        minimize: true,
        minimizer: [new TerserPlugin()],
    },
    plugins: [
        new MiniCssExtractPlugin({
            filename: "css/[name].css"
        }),
        // Copy Cesium Assets, Widgets, and Workers to a static directory
        new CopyWebpackPlugin({
            patterns: [
                { from: 'src/favicon.png', to: 'favicon.png' },
                { from: 'src/data', to: 'data' },
                { from: 'src/images', to: 'images' },
                { from: 'node_modules/wheretofly-weblib/dist/images', to: 'images' },
                { from: 'node_modules/cesium/Build/Cesium/Workers', to: 'js/Workers' },
                { from: 'node_modules/cesium/Build/Cesium/Assets', to: 'js/Assets' },
                { from: 'node_modules/cesium/Build/Cesium/Widgets', to: 'js/Widgets' }
            ],
        })
    ]
};
