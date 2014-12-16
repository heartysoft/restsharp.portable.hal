/// <reference path="jquery-1.10.2.intellisense.js" />
/// <reference path="traverson.js" />

var submission = function() {

    var submitter = function (state) {

        state.requestOptions = state.requestOptions || {
            headers: {
                'Accept': 'application/hal+json',
                'Content-Type': 'application/json'
            }
        };

        state.templateParameters = state.templateParameters || {};
        state.newData = state.newData || {};
        state.handler = state.handler || {};

        var result = {
        };

        result.requestOptions = function (opt) {
            jQuery.extend(state.requestOptions, opt);
            return submitter(state);
        };

        result.templateParameters = function withTemplateParams(params) {
            jQuery.extend(state.templateParameters, params);
            return submitter(state);
        }

        result.newData = function withData(data) {
            jQuery.extend(state.newData, data);
            return submitter(state);
        }

        result.handler = function (handler) {
            state.handler = handler;
            return submitter(state);
        }

        result.execute = function (handlerOverride) {
            return function (error, doc) {
                var callback = handlerOverride ? handlerOverride : state.handler;

                if (error) {
                    callback(error, doc);
                } else {
                    var getDataForSubmit = function getData(docu, currentState) {
                        var newData = currentState.newData;
                        var merged = jQuery.extend(docu, newData);

                        delete merged._links;
                        delete merged._embedded;

                        return merged;
                    };

                    var path = doc._links.self.href;
                    var api = traverson.jsonHal.from(path);

                    api.newRequest()
                        .withRequestOptions(state.requestOptions)
                        .follow('self')
                        .withTemplateParameters(state.templateParameters)
                        .post(getDataForSubmit(doc, state), callback);
                }
            };
        };
        return result;
    }
    return submitter({});
};


