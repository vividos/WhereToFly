const path = require('path');

const HtmlWebpackPlugin = require('html-webpack-plugin');
const MiniCssExtractPlugin = require("mini-css-extract-plugin");
const CssMinimizerPlugin = require("css-minimizer-webpack-plugin");
const CopyWebpackPlugin = require('copy-webpack-plugin');
const TerserPlugin = require("terser-webpack-plugin");

module.exports = {
    mode: 'production',
    entry: {
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
        maxAssetSize: 524288,
        maxEntrypointSize: 524288,
    },
    plugins: [
        new HtmlWebpackPlugin({
            filename: "heightProfileView.html",
            template: "src/heightProfileView.html",
            chunks: ["heightProfileView"]
        }),
        new MiniCssExtractPlugin({
            filename: "css/[name].css"
        }),
        new CopyWebpackPlugin({
            patterns: [
                { from: 'src/images', to: 'images' },
            ],
        }),
    ],
};
