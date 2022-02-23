/******/ (function(modules) { // webpackBootstrap
/******/ 	// The module cache
/******/ 	var installedModules = {};
/******/
/******/ 	// The require function
/******/ 	function __webpack_require__(moduleId) {
/******/
/******/ 		// Check if module is in cache
/******/ 		if(installedModules[moduleId]) {
/******/ 			return installedModules[moduleId].exports;
/******/ 		}
/******/ 		// Create a new module (and put it into the cache)
/******/ 		var module = installedModules[moduleId] = {
/******/ 			i: moduleId,
/******/ 			l: false,
/******/ 			exports: {}
/******/ 		};
/******/
/******/ 		// Execute the module function
/******/ 		modules[moduleId].call(module.exports, module, module.exports, __webpack_require__);
/******/
/******/ 		// Flag the module as loaded
/******/ 		module.l = true;
/******/
/******/ 		// Return the exports of the module
/******/ 		return module.exports;
/******/ 	}
/******/
/******/
/******/ 	// expose the modules object (__webpack_modules__)
/******/ 	__webpack_require__.m = modules;
/******/
/******/ 	// expose the module cache
/******/ 	__webpack_require__.c = installedModules;
/******/
/******/ 	// define getter function for harmony exports
/******/ 	__webpack_require__.d = function(exports, name, getter) {
/******/ 		if(!__webpack_require__.o(exports, name)) {
/******/ 			Object.defineProperty(exports, name, { enumerable: true, get: getter });
/******/ 		}
/******/ 	};
/******/
/******/ 	// define __esModule on exports
/******/ 	__webpack_require__.r = function(exports) {
/******/ 		if(typeof Symbol !== 'undefined' && Symbol.toStringTag) {
/******/ 			Object.defineProperty(exports, Symbol.toStringTag, { value: 'Module' });
/******/ 		}
/******/ 		Object.defineProperty(exports, '__esModule', { value: true });
/******/ 	};
/******/
/******/ 	// create a fake namespace object
/******/ 	// mode & 1: value is a module id, require it
/******/ 	// mode & 2: merge all properties of value into the ns
/******/ 	// mode & 4: return value when already ns object
/******/ 	// mode & 8|1: behave like require
/******/ 	__webpack_require__.t = function(value, mode) {
/******/ 		if(mode & 1) value = __webpack_require__(value);
/******/ 		if(mode & 8) return value;
/******/ 		if((mode & 4) && typeof value === 'object' && value && value.__esModule) return value;
/******/ 		var ns = Object.create(null);
/******/ 		__webpack_require__.r(ns);
/******/ 		Object.defineProperty(ns, 'default', { enumerable: true, value: value });
/******/ 		if(mode & 2 && typeof value != 'string') for(var key in value) __webpack_require__.d(ns, key, function(key) { return value[key]; }.bind(null, key));
/******/ 		return ns;
/******/ 	};
/******/
/******/ 	// getDefaultExport function for compatibility with non-harmony modules
/******/ 	__webpack_require__.n = function(module) {
/******/ 		var getter = module && module.__esModule ?
/******/ 			function getDefault() { return module['default']; } :
/******/ 			function getModuleExports() { return module; };
/******/ 		__webpack_require__.d(getter, 'a', getter);
/******/ 		return getter;
/******/ 	};
/******/
/******/ 	// Object.prototype.hasOwnProperty.call
/******/ 	__webpack_require__.o = function(object, property) { return Object.prototype.hasOwnProperty.call(object, property); };
/******/
/******/ 	// __webpack_public_path__
/******/ 	__webpack_require__.p = "";
/******/
/******/
/******/ 	// Load entry module and return exports
/******/ 	return __webpack_require__(__webpack_require__.s = "./src/content/contextMenuHandler.ts");
/******/ })
/************************************************************************/
/******/ ({

/***/ "./src/content/contextMenuHandler.ts":
/*!*******************************************!*\
  !*** ./src/content/contextMenuHandler.ts ***!
  \*******************************************/
/*! no static exports found */
/***/ (function(module, exports) {

const inputTags = ['input', 'textarea', 'select'];
const labelTags = ['label', 'span'];
const attributes = ['id', 'name', 'label-aria', 'placeholder'];
const invalidElement = chrome.i18n.getMessage('copyCustomFieldNameInvalidElement');
const noUniqueIdentifier = chrome.i18n.getMessage('copyCustomFieldNameNotUnique');
let clickedEl = null;
// Find the best attribute to be used as the Name for an element in a custom field.
function getClickedElementIdentifier() {
    var _a, _b;
    if (clickedEl == null) {
        return invalidElement;
    }
    const clickedTag = clickedEl.nodeName.toLowerCase();
    let inputEl = null;
    // Try to identify the input element (which may not be the clicked element)
    if (labelTags.includes(clickedTag)) {
        let inputId = null;
        if (clickedTag === 'label') {
            inputId = clickedEl.getAttribute('for');
        }
        else {
            inputId = (_a = clickedEl.closest('label')) === null || _a === void 0 ? void 0 : _a.getAttribute('for');
        }
        inputEl = document.getElementById(inputId);
    }
    else {
        inputEl = clickedEl;
    }
    if (inputEl == null || !inputTags.includes(inputEl.nodeName.toLowerCase())) {
        return invalidElement;
    }
    for (const attr of attributes) {
        const attributeValue = inputEl.getAttribute(attr);
        const selector = '[' + attr + '="' + attributeValue + '"]';
        if (!isNullOrEmpty(attributeValue) && ((_b = document.querySelectorAll(selector)) === null || _b === void 0 ? void 0 : _b.length) === 1) {
            return attributeValue;
        }
    }
    return noUniqueIdentifier;
}
function isNullOrEmpty(s) {
    return s == null || s === '';
}
// We only have access to the element that's been clicked when the context menu is first opened.
// Remember it for use later.
document.addEventListener('contextmenu', event => {
    clickedEl = event.target;
});
// Runs when the 'Copy Custom Field Name' context menu item is actually clicked.
chrome.runtime.onMessage.addListener(event => {
    if (event.command === 'getClickedElement') {
        const identifier = getClickedElementIdentifier();
        chrome.runtime.sendMessage({
            command: 'getClickedElementResponse',
            sender: 'contextMenuHandler',
            identifier: identifier,
        });
    }
});


/***/ })

/******/ });