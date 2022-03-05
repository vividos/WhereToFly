const path = require('path');

const MiniCssExtractPlugin = require("mini-css-extract-plugin");
const TerserPlugin = require("terser-webpack-plugin");
const CssMinimizerPlugin = require("css-minimizer-webpack-plugin");

module.exports = {
    mode: 'production',
    entry: "./src/js/index.js",
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
    plugins: [
        new MiniCssExtractPlugin({
            filename: "css/[name].css"
        }),
    ],
};
