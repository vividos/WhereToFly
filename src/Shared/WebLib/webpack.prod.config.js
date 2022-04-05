const { merge } = require('webpack-merge');
const common = require('./webpack.common.config.js');

const path = require('path');

const CssMinimizerPlugin = require("css-minimizer-webpack-plugin");
const TerserPlugin = require("terser-webpack-plugin");

module.exports = merge(common, {
    mode: 'production',
    module: {
        rules: [
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
});
