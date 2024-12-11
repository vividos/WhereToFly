/**
 * function used to call to C# WebView
 * @param {string} action callback action name
 * @param {object} params parameter as object
 */
export function callAction(action, params) {
    console.log("callAction: called with " + action);
    if (typeof callback === "object") {
        callback.postMessage(
            JSON.stringify({
                action: action,
                data: JSON.stringify(params)
            }));
    }
    else if (typeof window.chrome.webview === "object")
        window.chrome.webview.postMessage({
            action: action,
            data: JSON.stringify(params)
        });
    else
        console.warn("unhandled callAction() case!");
}
