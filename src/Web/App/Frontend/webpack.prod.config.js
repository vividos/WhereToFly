//
// webpack.prod.config.js - webpack configuration for production builds
//
const { merge } = require("webpack-merge");
const common = require("./webpack.common.config.js");

const CssMinimizerPlugin = require("css-minimizer-webpack-plugin");
const TerserPlugin = require("terser-webpack-plugin");

module.exports = merge(common, {
    mode: "production",
    optimization: {
        usedExports: true,
        minimize: true,
        minimizer: [
            new TerserPlugin(),
            new CssMinimizerPlugin()
        ]
    },
    performance: {
        hints: "warning",
        maxAssetSize: 5242880,
        maxEntrypointSize: 5242880
    }
});
