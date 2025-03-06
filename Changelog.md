# Changelog

## Version 1.16.0

Released on ?.

✨ New Features / Improvements

- Android: `geo:` links can now be opened with the app and adds a new location
- Improved recognizing more geo coordinate formats
- Minimum Android version is now Android 12
- Using CesiumJS version 1.126.0

🐛 Bugfixes

- Fixed displaying sunrise and sunset when altitude is below 0 m

## Version 1.15.0

Released on 2024-11-20.

✨ New Features / Improvements

- Planning tour is now possible with any location pin and by long-tapping a
  point on the map
- Height profile now shows track name and an info button to go to the track
  details page
- Improved parsing coordinates, recognizing more formats
- Using CesiumJS version 1.123.0
- Upgraded user interface to use .NET MAUI

🐛 Bugfixes

- Fixed exporting layers
- Hide color selection when editing a flight track
- Fixed clicking on links on the layer, location and track details pages
- Fixed "show details" and other functions for nearby POIs

## Version 1.14.4

Released on 2024-05-01.

✨ New Features / Improvements

- Made popup dialogs a bit lighter in dark mode
- Using CesiumJS version 1.116.0

🐛 Bugfixes

- Hide track pin when closing track height profile view

## Version 1.14.3

Released on 2024-02-09.

✨ New Features / Improvements

- Finding locations can now also parse coordinates in various formats
- Using CesiumJS version 1.114.0

🐛 Bugfixes

- Fixed saving added weather web links
- When adding a weather web link, pre-select the Group field
- When zoomed out far, fly nearer to the ground when flying to current position

## Version 1.14.2

Released on 2023-11-23.

✨ New Features / Improvements

- Improved performance of track height profile view
- Moved OpenFlightMaps.org layer to map overlays
- Corrected Sunset angle heading on the current position page
- Using CesiumJS version 1.111.0

🐛 Bugfixes

- Fixed crash when track height profile can't be sampled at import
- Fixed crash on location list page when location permission was not given yet

## Version 1.14.1

Released on 2023-04-17.

✨ New Features / Improvements

- Using CesiumJS version 1.103

🐛 Bugfixes

- Fixed getting position updates when pressing the "Locate me" toolbar button
- Use correct location pin icon when adding a find result as location
- Determine altitude for "find result" location instead of showing 0 m
- Improved resolution of location pin images
- Fixed Alptherm auto login
- Fixed downloading images from weather pages that need a login
- Disable swiping in track details when hovering over the track's height
  profile
- Fixed displaying time scale in track height profile
- Fixed context menu icon color of Delete menu items

## Version 1.14.0 "New Zealand 2022/2023" edition

Released on 2022-12-01.

✨ New Features / Improvements

- Added new "Find nearby POIs" button in the map view that shows temporary
  point of interest markers on the map
- Added the "Waymarked Trails Hiking" map overlay
- The "find result" pin can now be hidden again
- Added "altitude offset" to Flying Range dialog
- Using CesiumJS version 1.99

🐛 Bugfixes

- Fixed a crash when opening the Current Position page directly after app
  startup
- Fixed crash when deleting the location list while the list is currently
  being filtered
- Fixed "Add new..." weather icon color in dark mode

## Version 1.13.0 "FJällräven Classic 2022" edition

Released on 2022-08-11.

✨ New Features / Improvements

- The Current Position page now has a second tab with a compass that rotates
  with the devices' compass, showing sunset and sunrise angles and set target
  direction
- Locations and points in the map can now be set as compass target, displaying
  a line on the ground to the target and a target endpoint pin
- In the Compass tab, a target direction angle can be set (without showing
  the target endpoint pin)
- Split heading into magnetic-north and geographic, true-north compass
  directions
- Improved performance when adding many locations
- Using CesiumJS version 1.96

## Version 1.12.0

Released on 2022-07-17.

✨ New Features / Improvements

- Layers, tracks and locations now have a 3-dot-button to show the context menu
- The context menus now have icons and are displayed as popup dialogs
- Updated included Paraglidingspots European Alps locations to version 2.02
- Display name and description from CZML layers
- Using CesiumJS version 1.95

🐛 Bugfixes

- Fixed loading weather icons for some pages
- Fixed map starting location, using last zoomed-to location
- Fixed opening websites in external browser window
- Fixed finding current position after minimizing app
- Fixed showing toast messages and tour plan dialogs multiple times

## Version 1.11.0

Released on 2021-11-09.

✨ New Features / Improvements

- Added support for live tracking tracks using `where-to-fly://` links
- Added group headings for list when selecting weather icons
- Improved track handling by using a memory cache for tracks
- When importing tracks from KML files, display track names based on KML tree
  structure
- Using CesiumJS version 1.87

🐛 Bugfixes

- Fixed saving images in weather web pages on Android 10 and above
- Fixed zooming to newly added locations
- Fixed displaying OpenFlightMaps layer
- Various other minor bugfixes

## Version 1.10.0

Released on 2021-03-14.

✨ New Features / Improvements

- Support for Cesium OpenStreetMap Buildings layer; can be added to layer list
- For locations that have takeoff directions, the takeoff directions are now
  shown on the map as circle slices
- Automatically zoom to newly imported locations
- Added exporting CZML layers to .kmz files
- Added clustering of Location pins and Layer elements when zooming out
- Keep distance to terrain when zooming to different location pins or the
  current location; the viewing distance is also re-used after app restarts
- Using CesiumJS version 1.79.1

🐛 Bugfixes

- Fixed flight track colors when track contains many duplicate track points
  (e.g. from waiting at takeoff)
- Fixed importing GPX files with version 1.0 

## Version 1.9.0

Released on 2020-12-21.

✨ New Features / Improvements

- Added support for Android 11
- Added showing height profile when tapping on a track
- Added zooming and panning to height profile graph
- Added setting app themes "light", "dark" and "same as device"
- Added showing sunrise and sunset times on the "Current Position" page
- Added dialog when importing airspaces to select which airspace classes to
  import
- Using magnetic compass for heading on the "Current Position" page when the
  device has one
- Using CesiumJS version 1.76

🐛 Bugfixes

- Various bugfixes and improvements

## Version 1.8.1

Released on 2020-08-19.

✨ New Features / Improvements

- Using CesiumJS version 1.72

🐛 Bugfixes

- Fixed "Sampling track point heights..." not disappearing when device has no
  network connection
- Fixed crash when opening files from remote file providers (like OneDrive and
  Google Drive) but the device has no network connection

## Version 1.8.0

Released on 2020-08-11.

✨ New Features / Improvements

- Added info page containing the manual for all pages in the app
- Added importing locations from SeeYou waypoints.cup files
- Added exporting track to .gpx file
- Importing KML tracks from XC Tracer devices now calculate the time points
  automatically
- Long-tapping on images in the weather browser offers downloading that image
- Using CesiumJS version 1.72

🐛 Bugfixes

- Fixed displaying the map when there is no mobile network at app startup;
  the terrain is loaded when the mobile network is available again
- Fixed crash when navigating between different weather pages
- Fixed crash when deleting a location
- Fixed displaying OpenFlightMaps imagery layer
- Fixed displaying the Cesium Ion icon on the bottom of the screen

## Version 1.7.0

Released on 2020-02-17.

✨ New Features / Improvements

- All data is now stored in an SQLite database; improves speed of location
  list; migrating existing data occurs when upgrading to 1.7.0
- Added automatic logon for Alptherm website; can be configured on settings
  page
- Updated included ParaglidingSpots locations
- The info page now shows several pages, showing version info, this changelog
  and the credits
- Using CesiumJS version 1.66

🐛 Bugfixes

- Various bug fixes and improvements

## Version 1.6.0 "Crossing the Alps 2019" edition

Released on 2019-07-25.

✨ New Features / Improvements

- Added new "layer" menu where CesiumJS .czml files can be loaded as layers
  displayed on the map
- Added specifying "track point interval" when imported track has no time data
  (e.g. for KML LineStrings); useful for .kml files from XC Tracer II GPS
- Flying range cones can now be hidden using an icon in the description
- Supported opening .igc files downloaded from xcontest
- Updated waypoints for Crossing the Alps 2019
- Using CesiumJS version 1.59

## Version 1.5.0

Released on 2019-05-28.

✨ New Features / Improvements

- Added support for live waypoints that update themselves periodically;
  supported are live waypoints to Garmin inReach and SPOT devices
- Added support for `where-to-fly://` URLs for adding live waypoints
- Added long-tap menu entry "show flying range" to display a half transparent
  cone that shows the flying range, based on the glide ratio
- When adding a flying track, terrain height is sampled so that track is never
  "under ground"; non-flying tracks are clamped to terrain now
- Added Sentinel-2 and OpenFlightMaps.org imagery layer
- Added hiking tour planning support for some selected location waypoints in
  the Schliersee area.
- Improved retrieving and caching of weather dashboard icons
- Added Where-to-fly Windows app (beta)

🐛 Bugfixes

- Fixed opening .igc files from with negative latitudes or longitudes

## Version 1.4.0

Released on 2018-12-08.

✨ New Features / Improvements

- Added importing and showing tracks on map, new track list and track details page
- Added opening .igc files as tracks
- Added activity indicator in location list when list is being refreshed
- Weather links are now shown on a dedicated page with icons to change to other weather pages
- Added new map imagery type: OpenTopoMap
- Added button on settings page to clear cache used for map view

## Version 1.3.1 "Crossing the alps 2018" bugfix edition

Released on 2018-07-21.

✨ New Features / Improvements

- Updated waypoints for Crossing the alps 2018

🐛 Bugfixes

- Fixed bug where a newly added location (using find or long-tap to add) wasn't shown on map
- Fixed bug displaying distances with many fractional digits

## Version 1.3.0 "Crossing the alps 2018" edition

Released on 2018-06-26.

✨ New Features / Improvements

- Introduced hamburger menu with drawer to navigate between top-level pages
- Added weather dashboard, showing weather icons that can be links to external web pages or apps
- Redesigned "current position" page to use tiles
- Added sharing a location on the "location details" page
- Added waiting dialogs when importing locations
- Crossing the alps: added waypoints and a fixed red polyline with the planned route

🐛 Bugfixes

- Fixed opening downloaded .kml, .kmz and .gpx files
- Fixed crash on Android before 8.0; the "Skyways" overlay is not available on these devices
- Other small bugfixes and improvements

## Version 1.2.1

Released on 2018-05-18.

🐛 Bugfixes

- Fixed crash at startup for some Android devices
- Fixed loading terrain height data
- Fixed displaying large number of pins when many locations are loaded

## Version 1.2.0

Released on 2018-05-04.

✨ New Features / Improvements

- Added "Location list" and "Location details" pages, showing list and details of the loaded placemarks
- Added map overlay "Thermal Skyways (thermal.kk7.ch)"
- Added long-tap gesture on map, in order to add custom waypoint
- Added "Find location" function to search for addresses or other place names
- Added importing GPX files for locations

🐛 Bugfixes

- Fixed crash when picking files from "Downloads" or other special folders
- Many other bugfixes and minor changes

## Version 1.1.0

Released on 2018-02-03.

✨ New Features / Improvements

- Added imagery layer "Aerials + Labels (Bing Maps)"
- Added map overlays "Contour lines", "Slope + contour lines" and "NASA Black Marble"

## Version 1.0.0 "NZ down under" edition for Jonas and Julian.

Released on 2018-01-29.

✨ New Features / Improvements

- Showing 3D map of mountains to hike up and fly down, with shading based on daylight settings
- Showing locations and detail infos, based on imported KML placemarks, e.g. take off and landing place locations
- Showing the current location, and share it with other apps
