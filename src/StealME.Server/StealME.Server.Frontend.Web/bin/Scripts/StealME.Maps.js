var map, markers, zoom, center, currentPopup, lineLayer,lineFeature;
var size = new OpenLayers.Size(30, 20);
var offset = new OpenLayers.Pixel(-(size.w / 2), -(size.h / 2));
var icon = new OpenLayers.Icon('http://hackneyplaybus.org/wp-content/themes/paper_made/images/icon_pin_blue.png', size, offset);

var popupClass = OpenLayers.Class(OpenLayers.Popup.FramedCloud, {
    "autoSize": true,
    "minSize": new OpenLayers.Size(300, 50),
    "maxSize": new OpenLayers.Size(500, 300),
    "keepInMap": true
});

function init() {
    if (typeof map === 'undefined') {

        // Create Map object with default controls
        map = new OpenLayers.Map("map");
        map.addControl(new OpenLayers.Control.DragPan());

        // Add Base layer
        layer = new OpenLayers.Layer.OSM();
        map.addLayer(layer);

        // Add line vector layer
        lineLayer = new OpenLayers.Layer.Vector("Line Layer");
        map.addLayer(lineLayer);

        // Add markers layer
        markers = new OpenLayers.Layer.Markers('Markers');
        markers.id = "Markers";
        map.addLayer(markers);
    }
}

function drawLine(positionResult){
var points = new Array();

for (i = 0; i < positionResult.length; i++)
{
    points[i] = new OpenLayers.Geometry.Point(positionResult[i].Longtitude, positionResult[i].Latitude).transform(new OpenLayers.Projection("EPSG:4326"), map.getProjectionObject());
}

var line = new OpenLayers.Geometry.LineString(points);

var style = { 
  strokeColor: '#0000ff', 
  strokeOpacity: 0.5,
  strokeWidth: 5
};

lineFeature = new OpenLayers.Feature.Vector(line, null, style);
lineLayer.addFeatures([lineFeature]);
}

function clearLines() {
    if (typeof lineFeature === 'undefined')
        return;

    lineLayer.removeFeatures([lineFeature]);
}

function clearMarkers() {
    markers.clearMarkers();
}

function addMarker(lng, lat, info) {
    
    // Create projected position from GPS Latitude/Longtitude
    var pos = new OpenLayers.LonLat(lng, lat).transform(new OpenLayers.Projection("EPSG:4326"), map.getProjectionObject());

    // Create popup instance
    var feature = new OpenLayers.Feature(markers, pos);
    feature.closeBox = true;
    feature.popupClass = popupClass;
    feature.data.popupContentHTML = info;
    feature.data.overflow = "auto";

    // Create Marker object
    var marker = new OpenLayers.Marker(pos, icon.clone());
    
    // Create Marker Click function
    var markerClick = function (evt) {
        if (currentPopup != null && currentPopup.visible()) {
            currentPopup.hide();
        }
        if (this.popup == null) {
            this.popup = this.createPopup(this.closeBox);
            map.addPopup(this.popup);
            this.popup.show();
        } else {
            this.popup.toggle();
        }
        currentPopup = this.popup;
        OpenLayers.Event.stop(evt);
    };
    marker.events.register("mousedown", feature, markerClick);

    // If ever needed...onmouse/onleave events
    /*marker.events.register('mouseover', marker, function (evt) {
        document.getElementById('info').innerHTML = infoText;
        OpenLayers.Event.stop(evt);
    });

    marker.events.register('mouseout', marker, function (evt) {
        document.getElementById('info').innerHTML = '&nbsp;';
        OpenLayers.Event.stop(evt);
    });*/

    // Add marker to Markers layer
    markers.addMarker(marker);
}

function setCenter(longtitude, latitude) {
    map.setCenter(
        new OpenLayers.LonLat(longtitude, latitude).transform(
            new OpenLayers.Projection("EPSG:4326"),
            map.getProjectionObject()
            ), 16
         );
}