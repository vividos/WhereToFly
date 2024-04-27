/**
 *  function used to call to C# WebView
 * @param {string} funcName callback function name
 * @param {object} params parameter as object
 */
export function callAction(funcName, params) {
    const ios = /iphone|ipod|ipad/.test(window.navigator.userAgent.toLowerCase());
    if (ios)
        window.location.href = "callback://" + funcName + "/" + JSON.stringify(params);
    else if (typeof callback === "object")
        window.callback.call(funcName, JSON.stringify(params));
    else
        window.location.href = "callback://" + funcName + "/" + JSON.stringify(params);
}
