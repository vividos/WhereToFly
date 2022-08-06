const { merge } = require('webpack-merge');
const common = require('./webpack.common.config.js');

const path = require('path');

const CssMinimizerPlugin = require("css-minimizer-webpack-plugin");
const TerserPlugin = require("terser-webpack-plugin");

module.exports = merge(common, {
    mode: 'development',
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
            }
        ]
    },
    performance: {
        hints: 'error',
        maxAssetSize: 19600000,
        maxEntrypointSize: 19600000,
    },
});
