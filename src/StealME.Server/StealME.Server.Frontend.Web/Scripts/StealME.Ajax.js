function onCommandSuccess(content) {
    alert(content.responseText);
}


function onDeviceUpdateSuccess(content) {

}

function onNavigationUpdateSuccess(content) {
    var obj = jQuery.parseJSON(content.responseText);

    // Clear previous map markers
    clearMarkers();

    // Clear previous line layer
    clearLines();

    // Add line to map vector layer from positions
    drawLine(obj.PositionResult);

    // Add markers from callback result
    for (var pos in obj.PositionResult) {
        addMarker(obj.PositionResult[pos].Longtitude,
                  obj.PositionResult[pos].Latitude, 
                  obj.PositionResult[pos].Longtitude + obj.PositionResult[pos].Latitude + obj.PositionResult[pos].CreationDate);
    }

    // Set device avaliability field
    $('#device-avaliability-value').html(obj.TrackerAvaliability);

    // Set device status field
    $('#device-status-value').html(obj.TrackerStatus);

    // Set device status field
    var date = formatJSONDate(obj.CreationDate);
    var h = date.getHours();
    var m = date.getMinutes();
    var s = date.getSeconds();
    $('#device-lastupdated-value').html(h + ":" + m + ":" + s);

    // Set map center to the first position
    setCenter(obj.PositionResult[0].Longtitude, obj.PositionResult[0].Latitude);

    //alert("form update success");
}

function UpdateDevicePanels() {
    $('*[id*=UpdateDeviceForm]').each(function () {
        $(this).submit();
    });
}

function UpdateTrackingInformation() {
    $('*[id*=NavigationForm]').each(function () {
        $(this).submit();
    });
}

function formatJSONDate(jsonDate) {
    var date = new Date(parseInt(jsonDate.substr(6)));
    return date;

}