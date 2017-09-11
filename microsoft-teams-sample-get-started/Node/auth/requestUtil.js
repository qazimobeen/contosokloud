/*
 * Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license.
 * See LICENSE in the project root for license information.
 */
var https = require('https');
var authHelper = require('./authHelper.js');

/** 
 * Get data from the specified path, handling errors
 * @param req Incoming user request context
 * @param res Outgoing response context
 * @param {string} getPath the resource path from which to retrieve data
 * @param {callback} callback with (data) successfully retrieved
 */
function executeRequestWithErrorHandling(req, res, requestType, requestPath, requestBody, callback) {
  executeRequest(
    requestType,
    requestPath,
    requestBody,
    req.cookies.ACCESS_TOKEN_CACHE_KEY,
    function (firstRequestError, firstTryUser) {
      if (!firstRequestError) {
        callback(firstTryUser);   
      } else if (hasAccessTokenExpired(firstRequestError)) {
        // Handle the refresh flow
        authHelper.getTokenFromRefreshToken(
          req.cookies.REFRESH_TOKEN_CACHE_KEY,
          function (refreshError, accessToken) {
            res.setCookie(authHelper.ACCESS_TOKEN_CACHE_KEY, accessToken);
            if (accessToken !== null) {
              executeRequest(
                requestType,
                requestPath,
                requestBody,
                req.cookies.ACCESS_TOKEN_CACHE_KEY,
                function (secondRequestError, secondTryUser) {
                  if (!secondRequestError) {
                    callback(secondTryUser);
                  } else {
                    clearCookies(res);
                    renderError(res, secondRequestError);
                  }
                }
              );
            } else {
              renderError(res, refreshError);
            }
          });
      } else {
        renderError(res, firstRequestError);
      }
    }
  );
}

/**
 * Generates a POST request to the SendMail endpoint
 * @param {string} accessToken the access token with which the request should be authenticated
 * @param {string} data the data which will be 'POST'ed
 * @param {callback} callback
 * Per issue #53 for BadRequest when message uses utf-8 characters: Set 'Content-Length': Buffer.byteLength(mailBody,'utf8')
 */
function executeRequest(requestType, requestPath, requestBody, accessToken, callback) {
  var options = {
    host: 'graph.microsoft.com',
    path: requestPath,
    method: requestType,
    headers: {
      'Content-Type': 'application/json',
      Authorization: 'Bearer ' + accessToken,
      'Content-Length': (requestBody == null) ? 0 : requestBody.length  // **Check
    }
  };

  // Set up the request
  var request = https.request(options, function (response) {
    var body = '';
    response.on('data', function (d) {
      body += d;
    });
    response.on('end', function () {
      var error;
            if (response.statusCode === 200) {
        callback(null, JSON.parse(body));
      } else if (response.statusCode === 202) {
        callback(null, null);
      } else {
        error = new Error();
        error.code = response.statusCode;
        error.message = response.statusMessage;
        // The error body sometimes includes an empty space
        // before the first character, remove it or it causes an error.
        body = body.trim();
        error.innerError = JSON.parse(body).error;
        // Note: If you receive a 500 - Internal Server Error
        // while using a Microsoft account (outlok.com, hotmail.com or live.com),
        // it's possible that your account has not been migrated to support this flow.
        // Check the inner error object for code 'ErrorInternalServerTransientError'.
        // You can try using a newly created Microsoft account or contact support.
        callback(error, null);
      }
    });
  });

  // write the outbound data to it
  if (requestBody != null) {
    request.write(requestBody);
  }
  // we're done!
  request.end();

  request.on('error', function (e) {
    callback(e);
  });
}

function hasAccessTokenExpired(e) {
  var expired;
  if (!e.innerError) {
    expired = false;
  } else {
    expired = e.code === 401 &&
      e.innerError.code === 'InvalidAuthenticationToken' &&
      e.innerError.message === 'Access token has expired.';
  }
  return expired;
}

function clearCookies(res) {
  res.clearCookie(authHelper.ACCESS_TOKEN_CACHE_KEY);
  res.clearCookie(authHelper.REFRESH_TOKEN_CACHE_KEY);
}

function renderError(res, e) {
  res.send({
    message: e.message,
    error: e
  });
  res.end();
}

exports.executeRequestWithErrorHandling = executeRequestWithErrorHandling;
exports.clearCookies = clearCookies;