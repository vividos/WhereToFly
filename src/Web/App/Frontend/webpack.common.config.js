//
// webpack.common.config.js - webpack common configuration for web app
//
const path = require("path");

const MiniCssExtractPlugin = require("mini-css-extract-plugin");
const CopyWebpackPlugin = require("copy-webpack-plugin");

module.exports = {
    entry: {
        app: "./src/js/app.js"
    },
    output: {
        clean: true,
        filename: "js/[name].bundle.js",
        path: path.resolve(__dirname, "..", "wwwroot"),
        library: {
            type: 'module'
        }
    },
    experiments: {
        outputModule: true
    },
    devtool: "source-map",
    module: {
        unknownContextCritical: false,
        rules: [
            {
                test: /\.css$/,
                use: [{ loader: MiniCssExtractPlugin.loader }, "css-loader"],
                sideEffects: true
            },
            { test: /\.eot(\?v=\d+\.\d+\.\d+)?$/, use: ["file-loader"] },
            { test: /\.(woff|woff2)$/, use: ["url-loader?prefix=font/&limit=5000"] },
            { test: /\.ttf(\?v=\d+\.\d+\.\d+)?$/, use: ["url-loader?limit=10000&mimetype=application/octet-stream"] }
        ]
    },
    plugins: [
        new MiniCssExtractPlugin({
            filename: "css/[name].css"
        }),
        // Copy images and Cesium files
        new CopyWebpackPlugin({
            patterns: [
                { from: "src/favicon.png", to: "favicon.png" },
                { from: "node_modules/wheretofly-weblib/dist/images", to: "images" },
                { from: "node_modules/wheretofly-weblib/dist/js/Assets", to: "js/Assets" },
                { from: "node_modules/wheretofly-weblib/dist/js/Widgets", to: "js/Widgets" },
                { from: "node_modules/wheretofly-weblib/dist/js/Workers", to: "js/Workers" }
            ]
        })
    ]
};
