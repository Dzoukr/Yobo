var path = require("path");
var webpack = require("webpack");
var MinifyPlugin = require("terser-webpack-plugin");
var HtmlWebpackPlugin = require('html-webpack-plugin')
var CopyWebpackPlugin = require('copy-webpack-plugin');

function resolve(filePath) {
    return path.join(__dirname, filePath)
}

var CONFIG = {
    fsharpEntry: {
        "app": [
            "whatwg-fetch",
            "@babel/polyfill",
            resolve("./Yobo.Client.fsproj")
        ]
        ,
        "login": [
            "whatwg-fetch",
            "@babel/polyfill",
            resolve("./Login/Yobo.Client.Login.fsproj")
        ]
    },
    devServerProxy: {
        '/api/*': {
            target: 'http://localhost:' + (process.env.SUAVE_FABLE_PORT || "8085"),
            changeOrigin: true
        }
    },
    historyApiFallback: {
        index: resolve("./index.html")
    },
    contentBase: resolve("./public"),
    babel: {
        presets: [
            ["@babel/preset-env", {
                "targets": {
                    "browsers": ["last 2 versions"]
                },
                "modules": false,
                "useBuiltIns": "usage",
            }]
        ],
        plugins: ["@babel/plugin-transform-runtime"]
    }
}

var isProduction = process.argv.indexOf("-p") >= 0;
console.log("Bundling for " + (isProduction ? "production" : "development") + "...");

var commonPlugins = [
    new HtmlWebpackPlugin({
        chunks: ['app'],
        template: resolve('./public/index.html'),
        filename: resolve('./output/index.html')
    }),
    new HtmlWebpackPlugin({
        chunks: ['login'],
        template: resolve('./public/login.html'),
        filename: resolve('./output/login.html')
    })
]

module.exports = {
    entry : CONFIG.fsharpEntry,
    output: {
        path: resolve('./output'),
        filename: isProduction ? '[name].[hash].js' : '[name].js'
    },
    mode: isProduction ? "production" : "development",
    devtool: isProduction ? undefined : "source-map",
    resolve: {
        symlinks: false
    },
    optimization: {
        splitChunks: {
            cacheGroups: {
                commons: {
                    test: /node_modules/,
                    name: "vendors",
                    chunks: "all"
                }
            }
        },
        minimizer: isProduction ? [new MinifyPlugin()] : []
    },
    plugins: isProduction ? commonPlugins.concat ( [
        new CopyWebpackPlugin([
            { from: resolve('./public') }
        ])
    ]) : commonPlugins.concat ([
        new webpack.HotModuleReplacementPlugin(),
        new webpack.NamedModulesPlugin()
    ]),
    devServer: {
        proxy: CONFIG.devServerProxy,
        hot: true,
        inline: true,
        historyApiFallback: CONFIG.historyApiFallback,
        contentBase: CONFIG.contentBase
    },
    module: {
        rules: [
            {
                test: /\.fs(x|proj)?$/,
                use: "fable-loader"
            },
            {
                test: /\.js$/,
                exclude: /node_modules/,
                use: {
                    loader: 'babel-loader',
                    options: CONFIG.babel
                },
            }
        ]
    }
};
