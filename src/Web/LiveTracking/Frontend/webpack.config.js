const path = require('path');

const MiniCssExtractPlugin = require("mini-css-extract-plugin");
const CopyWebpackPlugin = require('copy-webpack-plugin');

module.exports = {
    mode: 'development',
    entry: {
        site: './src/js/site.js'
    },
    output: {
        filename: 'js/[name].bundle.js',
        path: path.resolve(__dirname, '..', 'wwwroot')
    },
    devtool: 'source-map',
    module: {
        rules: [
            {
                test: /\.css$/,
                use: [{ loader: MiniCssExtractPlugin.loader }, "css-loader"]
            }, {
                test: /\.(png|gif|jpg|jpeg|svg|xml|json|czml)$/,
                use: ['url-loader']
            },
            { test: /\.eot(\?v=\d+\.\d+\.\d+)?$/, use: ['file-loader'] },
            { test: /\.(woff|woff2)$/, use: ["url-loader?prefix=font/&limit=5000"] },
            { test: /\.ttf(\?v=\d+\.\d+\.\d+)?$/, use: ["url-loader?limit=10000&mimetype=application/octet-stream"] }
        ]
    },
    plugins: [
        new MiniCssExtractPlugin({
            filename: "css/[name].css"
        }),
        new CopyWebpackPlugin({
            patterns: [
                { from: 'src/favicon.png', to: 'favicon.png' },
                { from: 'src/data', to: 'data' },
                { from: 'src/images', to: 'images' },
                { from: 'src/js', to: 'js' },
                { from: 'src/css', to: 'css' },
            ],
        })
    ]
};
