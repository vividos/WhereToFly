// eslint flat config
import js from "@eslint/js";
import jsdoc from "eslint-plugin-jsdoc";
import globals from "globals";

export default [
    js.configs.recommended,
    jsdoc.configs["flat/recommended"],
    {
        languageOptions: {
            globals: {
                ...globals.browser
            }
        },
        plugins: {
            jsdoc,
        },
        rules: {
            "indent": [ "error", 4 ],
            "curly": [ "error", "multi-or-nest" ],
            "quotes": [ "error", "double" ],
            "padded-blocks": "off",
            "space-before-function-paren": ["error", "never"],
            "semi": "off",
            "no-extra-semi": "off"
        }
    }
];
