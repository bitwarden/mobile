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
/******/ 	return __webpack_require__(__webpack_require__.s = "./src/notification/bar.js");
/******/ })
/************************************************************************/
/******/ ({

/***/ "./src/notification/bar.js":
/*!*********************************!*\
  !*** ./src/notification/bar.js ***!
  \*********************************/
/*! no static exports found */
/***/ (function(module, exports, __webpack_require__) {

__webpack_require__(/*! ./bar.scss */ "./src/notification/bar.scss");

document.addEventListener('DOMContentLoaded', () => {
    var i18n = {};
    var lang = window.navigator.language;

    i18n.appName = chrome.i18n.getMessage('appName');
    i18n.close = chrome.i18n.getMessage('close');
    i18n.never = chrome.i18n.getMessage('never');
    i18n.folder = chrome.i18n.getMessage('folder');
    i18n.notificationAddSave = chrome.i18n.getMessage('notificationAddSave');
    i18n.notificationAddDesc = chrome.i18n.getMessage('notificationAddDesc');
    i18n.notificationChangeSave = chrome.i18n.getMessage('notificationChangeSave');
    i18n.notificationChangeDesc = chrome.i18n.getMessage('notificationChangeDesc');
    lang = chrome.i18n.getUILanguage();

    // delay 50ms so that we get proper body dimensions
    setTimeout(load, 50);

    function load() {
        const isVaultLocked = getQueryVariable('isVaultLocked') == 'true';
        document.getElementById('logo').src = isVaultLocked
            ? chrome.runtime.getURL('images/icon38_locked.png')
            : chrome.runtime.getURL('images/icon38.png');

        document.getElementById('logo-link').title = i18n.appName;

        var neverButton = document.querySelector('#template-add .never-save');
        neverButton.textContent = i18n.never;

        var selectFolder = document.querySelector('#template-add .select-folder');
        selectFolder.setAttribute('aria-label', i18n.folder);
        selectFolder.setAttribute('isVaultLocked', isVaultLocked.toString());

        var addButton = document.querySelector('#template-add .add-save');
        addButton.textContent = i18n.notificationAddSave;

        var changeButton = document.querySelector('#template-change .change-save');
        changeButton.textContent = i18n.notificationChangeSave;

        var closeIcon = document.getElementById('close');
        closeIcon.src = chrome.runtime.getURL('images/close.png');
        closeIcon.alt = i18n.close;

        var closeButton = document.getElementById('close-button')
        closeButton.title = i18n.close;
        closeButton.setAttribute('aria-label', i18n.close);

        document.querySelector('#template-add .add-text').textContent = i18n.notificationAddDesc;
        document.querySelector('#template-change .change-text').textContent = i18n.notificationChangeDesc;

        if (getQueryVariable('add')) {
            setContent(document.getElementById('template-add'));

            var addButton = document.querySelector('#template-add-clone .add-save'),
                neverButton = document.querySelector('#template-add-clone .never-save');

            addButton.addEventListener('click', (e) => {
                e.preventDefault();

                const folderId = document.querySelector('#template-add-clone .select-folder').value;

                const bgAddSaveMessage = {
                    command: 'bgAddSave',
                    folder: folderId,
                };
                sendPlatformMessage(bgAddSaveMessage);
            });

            neverButton.addEventListener('click', (e) => {
                e.preventDefault();
                sendPlatformMessage({
                    command: 'bgNeverSave'
                });
            });

            if (!isVaultLocked) {
                const responseFoldersCommand = 'notificationBarGetFoldersList';
                chrome.runtime.onMessage.addListener((msg) => {
                    if (msg.command === responseFoldersCommand && msg.data) {
                        fillSelectorWithFolders(msg.data.folders);
                    }
                });
                sendPlatformMessage({
                    command: 'bgGetDataForTab',
                    responseCommand: responseFoldersCommand
                });
            }
        } else if (getQueryVariable('change')) {
            setContent(document.getElementById('template-change'));
            var changeButton = document.querySelector('#template-change-clone .change-save');
            changeButton.addEventListener('click', (e) => {
                e.preventDefault();

                const bgChangeSaveMessage = {
                    command: 'bgChangeSave'
                };
                sendPlatformMessage(bgChangeSaveMessage);
            });
        }

        closeButton.addEventListener('click', (e) => {
            e.preventDefault();
            sendPlatformMessage({
                command: 'bgCloseNotificationBar'
            });
        });

        window.addEventListener("resize", adjustHeight);
        adjustHeight();
    }

    function getQueryVariable(variable) {
        var query = window.location.search.substring(1);
        var vars = query.split('&');

        for (var i = 0; i < vars.length; i++) {
            var pair = vars[i].split('=');
            if (pair[0] === variable) {
                return pair[1];
            }
        }

        return null;
    }

    function setContent(element) {
        const content = document.getElementById('content');
        while (content.firstChild) {
            content.removeChild(content.firstChild);
        }

        var newElement = element.cloneNode(true);
        newElement.id = newElement.id + '-clone';
        content.appendChild(newElement);
    }

    function sendPlatformMessage(msg) {
        chrome.runtime.sendMessage(msg);
    }

    function fillSelectorWithFolders(folders) {
        const select = document.querySelector('#template-add-clone .select-folder');
        select.appendChild(new Option(chrome.i18n.getMessage('selectFolder'), null, true));
        folders.forEach((folder) => {
            //Select "No Folder" (id=null) folder by default
            select.appendChild(new Option(folder.name, folder.id || '', false));
        });
    }

    function adjustHeight() {
        sendPlatformMessage({
            command: 'bgAdjustNotificationBar',
            data: {
                height: document.querySelector('body').scrollHeight
            }
        });
    }
});


/***/ }),

/***/ "./src/notification/bar.scss":
/*!***********************************!*\
  !*** ./src/notification/bar.scss ***!
  \***********************************/
/*! no exports provided */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
// extracted by mini-css-extract-plugin


/***/ })

/******/ });