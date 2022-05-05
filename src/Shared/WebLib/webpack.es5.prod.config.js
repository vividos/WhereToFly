const { merge } = require('webpack-merge');
const common = require('./webpack.common.config.js');

const path = require('path');

const CssMinimizerPlugin = require("css-minimizer-webpack-plugin");
const TerserPlugin = require("terser-webpack-plugin");

module.exports = merge(common, {
    mode: 'production',
    target: ['web', 'es5'],
    output: {
        path: path.resolve(__dirname, 'dist-es5'),
    },
    module: {
        rules: [
            {
                test: /\.js$/,
                exclude: {
                    and: [/node_modules/],
                    not: [
                        /cesium/,
                        /chart.js/
                    ]
                },
                use: {
                    loader: 'babel-loader',
                    options: {
                        cacheDirectory: true
                    }
                }
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
});
