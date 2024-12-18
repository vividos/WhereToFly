//
// webpack.config.js - webpack configuration for debug builds
//
const { merge } = require("webpack-merge");
const common = require("./webpack.common.config.js");

module.exports = merge(common, {
    mode: "development",
    output: {
        compareBeforeEmit: false,
    }
});
