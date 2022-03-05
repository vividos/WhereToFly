const path = require('path');

const HtmlWebpackPlugin = require('html-webpack-plugin');
const MiniCssExtractPlugin = require("mini-css-extract-plugin");
const CopyWebpackPlugin = require('copy-webpack-plugin');

module.exports = {
    mode: 'development',
    entry: {
        heightProfileView: "./src/js/heightProfileView.js",
    },
    output: {
        path: path.resolve(__dirname, 'dist'),
        filename: 'js/WhereToFly.[name].js',
        library: ["WhereToFly", "[name]"],
        libraryTarget: "umd",
        // this fixes the "Automatic publicPath is not supported in this browser"
        // error when using webpack serve
        publicPath: ''
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
    performance: {
        hints: 'warning',
        maxAssetSize: 2097152,
        maxEntrypointSize: 2097152,
    },
    plugins: [
        new HtmlWebpackPlugin({
            filename: "heightProfileView.html",
            template: "src/heightProfileView.html",
            chunks: ["heightProfileView"]
        }),
        new HtmlWebpackPlugin({
            filename: "heightProfileTest.html",
            template: "src/heightProfileTest.html",
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
