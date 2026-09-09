//
// webpack.prod.config.js - webpack configuration for production builds
//
const { merge } = require("webpack-merge");
const common = require("./webpack.common.config.js");

const path = require("path");

const MinimizerPlugin = require("minimizer-webpack-plugin");

module.exports = merge(common, {
    mode: "production",
    module: {
        rules: [
            {
                // Strip cesium pragmas
                test: /\.js$/,
                enforce: "pre",
                include: path.resolve(__dirname, "node_modules/cesium/Source"),
                sideEffects: false,
                use: [{
                    loader: "strip-pragma-loader",
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
            new MinimizerPlugin()
        ]
    },
    performance: {
        hints: "warning",
        maxAssetSize: 4700000,
        maxEntrypointSize: 4700000
    },
});
