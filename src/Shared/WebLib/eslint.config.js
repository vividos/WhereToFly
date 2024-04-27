// eslint flat config
import js from "@eslint/js";
import globals from "globals";

export default [
    js.configs.recommended,
    {
        languageOptions: {
            globals: {
                ...globals.browser
            }
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
