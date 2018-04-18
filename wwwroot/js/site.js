function action(tcontroller, taction, tparam, tresult)
{
    //var root = window.location.protocol + "//" + window.location.host;
    $.post("../" + tcontroller + "/" + taction, tparam, tresult);
}

function actionSync(tcontroller, taction, params, tresult)
{
    //openWaitMe(waitMeObj, 'stretch', 'Lütfen Bekleyin ...');
    //var root = window.location.protocol + "//" + window.location.host;
    $.ajax(
    {
        url: root + "../" + tcontroller + "/" + taction,
        type: 'Post',
        data: params, 
        //dataType: "json",
        //contentType: "application/json; charset=utf-8",
        async: false,
        success: tresult,
        error: function (errData) {
            //closeWaitMe(waitMeObj);
            //showSystemError(errData.responseText);
        },
    });
}


function dateToString(tdate)
{
    return tdate.getFullYear().toString() + padLeft('00', tdate.getMonth() + 1) + padLeft('00', tdate.getDate()) + padLeft('00', tdate.getHours()) + padLeft('00', tdate.getMinutes()) + padLeft('00', tdate.getSeconds());
}

function padLeft(pad, user_str) {
    if (typeof user_str === 'undefined')
        return pad;
    return (pad + user_str).slice(-pad.length);
}

function padRight(pad, user_str) {
    if (typeof user_str === 'undefined')
        return pad;
    return (user_str + pad).substring(0, pad.length);
}