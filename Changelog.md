# Changelog

## Version 1.11.0

- added group headings for list when selecting weather icons

## Version 1.10.0

Released on 2021-03-14.

- support for Cesium OpenStreetMap Buildings layer; can be added to layer list
- for locations that have takeoff directions, the takeoff directions are now
  shown on the map as circle slices
- automatically zoom to newly imported locations
- added exporting CZML layers to .kmz files
- added clustering of Location pins and Layer elements when zooming out
- keep distance to terrain when zooming to different location pins or the
  current location; the viewing distance is also re-used after app restarts
- fixed flight track colors when track contains many duplicate track points
  (e.g. from waiting at takeoff)
- fixed importing GPX files with version 1.0 
- using CesiumJS version 1.79.1

## Version 1.9.0

Released on 2020-12-21.

- added support for Android 11
- added showing height profile when tapping on a track
- added zooming and panning to height profile graph
- added setting app themes "light", "dark" and "same as device"
- added showing sunrise and sunset times on the "Current Position" page
- added dialog when importing airspaces to select which airspace classes to
  import
- using magnetic compass for heading on the "Current Position" page when the
  device has one
- various bugfixes and improvements
- using CesiumJS version 1.76

## Version 1.8.1

Released on 2020-08-19.

- fixed "Sampling track point heights..." not disappearing when device has no
  network connection
- fixed crash when opening files from remote file providers (like OneDrive and
  Google Drive) but the device has no network connection
- using CesiumJS version 1.72

## Version 1.8.0

Released on 2020-08-11.

- fixed displaying the map when there is no mobile network at app startup;
  the terrain is loaded when the mobile network is available again
- added info page containing the manual for all pages in the app
- added importing locations from SeeYou waypoints.cup files
- added exporting track to .gpx file
- importing KML tracks from XC Tracer devices now calculate the time points
  automatically
- long-tapping on images in the weather browser offers downloading that image
- fixed crash when navigating between different weather pages
- fixed crash when deleting a location
- fixed displaying OpenFlightMaps imagery layer
- fixed displaying the Cesium Ion icon on the bottom of the screen
- using CesiumJS version 1.72

## Version 1.7.0

Released on 2020-02-17.

- all data is now stored in an SQLite database; improves speed of location
  list; migrating existing data occurs when upgrading to 1.7.0
- added automatic logon for Alptherm website; can be configured on settings
  page
- updated included ParaglidingSpots locations
- the info page now shows several pages, showing version info, this changelog
  and the credits
- various bug fixes and improvements
- using CesiumJS version 1.66

## Version 1.6.0 "Crossing the Alps 2019" edition

Released on 2019-07-25.

- added new "layer" menu where CesiumJS .czml files can be loaded as layers
  displayed on the map
- added specifying "track point interval" when imported track has no time data
  (e.g. for KML LineStrings); useful for .kml files from XC Tracer II GPS
- flying range cones can now be hidden using an icon in the description
- supported opening .igc files downloaded from xcontest
- updated waypoints for Crossing the Alps 2019
- using CesiumJS version 1.59

## Version 1.5.0

Released on 2019-05-28.

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

## Version 1.4.0

Released on 2018-12-08.

- added importing and showing tracks on map, new track list and track details page
- added opening .igc files as tracks
- added activity indicator in location list when list is being refreshed
- weather links are now shown on a dedicated page with icons to change to other weather pages
- added new map imagery type: OpenTopoMap
- added button on settings page to clear cache used for map view

## Version 1.3.1 "Crossing the alps 2018" bugfix edition

Released on 2018-07-21.

- fixed bug where a newly added location (using find or long-tap to add) wasn't shown on map
- fixed bug displaying distances with many fractional digits
- updated waypoints for Crossing the alps 2018

## Version 1.3.0 "Crossing the alps 2018" edition

Released on 2018-06-26.

- introduced hamburger menu with drawer to navigate between top-level pages
- added weather dashboard, showing weather icons that can be links to external web pages or apps
- redesigned "current position" page to use tiles
- added sharing a location on the "location details" page
- added waiting dialogs when importing locations
- crossing the alps: added waypoints and a fixed red polyline with the planned route
- fixed opening downloaded .kml, .kmz and .gpx files
- fixed crash on Android before 8.0; the "Skyways" overlay is not available on these devices
- other small bugfixes and improvements

## Version 1.2.1

Released on 2018-05-18.

- fixed crash at startup for some Android devices
- fixed loading terrain height data
- fixed displaying large number of pins when many locations are loaded

## Version 1.2.0

Released on 2018-05-04.

- added "Location list" and "Location details" pages, showing list and details of the loaded placemarks
- added map overlay "Thermal Skyways (thermal.kk7.ch)"
- added long-tap gesture on map, in order to add custom waypoint
- added "Find location" function to search for addresses or other place names
- added importing GPX files for locations
- fixed crash when picking files from "Downloads" or other special folders
- many other bugfixes and minor changes

## Version 1.1.0

Released on 2018-02-03.

- added imagery layer "Aerials + Labels (Bing Maps)"
- added map overlays "Contour lines", "Slope + contour lines" and "NASA Black Marble"

## Version 1.0.0 "NZ down under" edition for Jonas and Julian.

Released on 2018-01-29.

- showing 3D map of mountains to hike up and fly down, with shading based on daylight settings
- showing locations and detail infos, based on imported KML placemarks, e.g. take off and landing place locations
- showing the current location, and share it with other apps
