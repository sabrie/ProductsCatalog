/// <reference path="jquery-1.7.1.js" />

var httpRequester = (function () {
    function getJSON(url, success, error) {
        $.ajax({
            url: url,
            type: "GET",
            //timeout: 5000,
            contentType: "application/json",
            dataType: "json",
            AccessControlAllowOrigin: '*',
            success: success,
            error: error
        });
    }
    function postJSON(url, data, success, error) {
        $.ajax({
            url: url,
            type: "POST",
            contentType: "application/json",
            dataType: "json",
            //timeout: 5000,
            data: JSON.stringify(data),
            success: success,
            error: error
        });
    }

    function postJSONFileUpload(url, data, success, error) {
        $.ajax({
            url: url,
            type: "POST",
            contentType: false,
            cache: false,
            dataType: "json",
            processData: false,
            data: data,
            success: success,
            error: error
        });
    }

    function putJSON(url, data, success, error) {
        $.ajax({
            url: url,
            type: "PUT",
            data: JSON.stringify(data),
            dataType: "json",
            contentType: "application/json",
            //timeout: 5000,
            success: success,
            error: error
        });
    }

    function putJSONFileUpload(url, data, success, error) {
        $.ajax({
            url: url,
            type: "PUT",
            contentType: false,
            cache: false,
            dataType: "json",
            processData: false,
            data: data,
            success: success,
            error: error
        });
    }

    function deleteJSON(url, success, error) {
        $.ajax({
            url: url,
            type: "DELETE",
            //data: JSON.stringify(data),
            dataType: "json",
            contentType: "application/json",
            //timeout: 5000,
            success: success,
            error: error
        });
    }

    return {
        getJSON: getJSON,
        postJSON: postJSON,
        putJSON: putJSON,
        deleteJSON: deleteJSON,
        postJSONFileUpload: postJSONFileUpload,
        putJSONFileUpload: putJSONFileUpload
    };
}());