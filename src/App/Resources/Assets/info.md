# Where-to-fly

Where-to-fly is an Android app that helps decide where to (hike up and) fly
with a paraglider. It shows the current position on a 3D map, and nearby
point of interests, such as start and landing places, webcams and weather
stations.

## Version infos

Current version: %VERSION%

### Releases

1.6.0 "Crossing the Alps 2019" edition

- added new "layer" menu where CesiumJS .czml files can be loaded as layers
  displayed on the map
- added specifying "track point interval" when imported track has no time data
  (e.g. for KML LineStrings); useful for .kml files from XC Tracer II GPS
- flying range cones can now be hidden using an icon in the description
- updated waypoints for Crossing the Alps 2019

1.5.0

- added support for live waypoints that update themselves periodically;
  supported are live waypoints to Garmin inReach and SPOT devices
- added support for where-to-fly:// URLs for adding live waypoints
- added long-tap menu entry "show flying range" to display a half transparent
  cone that shows the flying range, based on the glide ratio
- when adding a flying track, terrain height is sampled so that track is never
  "under ground"; non-flying tracks are clamped to terrain now
- added Sentinel-2 and OpenFlightMaps.org imagery layer
- added hiking tour planning support for some selected location waypoints in
  the Schliersee area.
- improved retrieving and caching of weather dashboard icons
- fixed opening .igc files from with negative latitudes or longitudes
- added Where-to-fly Windows app (beta)

1.4.0

- added importing and showing tracks on map, new track list and track details page
- added opening .igc files as tracks
- added activity indicator in location list when list is being refreshed
- weather links are now shown on a dedicated page with icons to change to other weather pages
- added new map imagery type: OpenTopoMap
- added button on settings page to clear cache used for map view

1.3.1 "Crossing the alps 2018" bugfix edition

- fixed bug where a newly added location (using find or long-tap to add) wasn't shown on map
- fixed bug displaying distances with many fractional digits
- updated waypoints for Crossing the alps 2018

1.3.0 "Crossing the alps 2018" edition

- introduced hamburger menu with drawer to navigate between top-level pages
- added weather dashboard, showing weather icons that can be links to external web pages or apps
- redesigned "current position" page to use tiles
- added sharing a location on the "location details" page
- added waiting dialogs when importing locations
- crossing the alps: added waypoints and a fixed red polyline with the planned route
- fixed opening downloaded .kml, .kmz and .gpx files
- fixed crash on Android before 8.0; the "Skyways" overlay is not available on these devices
- other small bugfixes and improvements

1.2.1

- fixed crash at startup for some Android devices
- fixed loading terrain height data
- fixed displaying large number of pins when many locations are loaded

1.2.0

- added "Location list" and "Location details" pages, showing list and details of the loaded placemarks
- added map overlay "Thermal Skyways (thermal.kk7.ch)"
- added long-tap gesture on map, in order to add custom waypoint
- added "Find location" function to search for addresses or other place names
- added importing GPX files for locations
- fixed crash when picking files from "Downloads" or other special folders
- many other bugfixes and minor changes

1.1.0

- added imagery layer "Aerials + Labels (Bing Maps)"
- added map overlays "Contour lines", "Slope + contour lines" and "NASA Black Marble"

1.0.0 "NZ down under" edition for Jonas and Julian.

- showing 3D map of mountains to hike up and fly down, with shading based on daylight settings
- showing locations and detail infos, based on imported KML placemarks, e.g. take off and landing place locations
- showing the current location, and share it with other apps
