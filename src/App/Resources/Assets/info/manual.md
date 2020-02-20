# Manual

Welcome to the manual page for the Where-to-fly app!

## 3D Map

## Layers

## Location list

### Live waypoints

## Tracks

### Track colors

If a track was imported as a flight, the track segments are colored according
to the climb or sink rate of that segment. The following colors are used:

<table border="0px"><tbody>
<tr><th width="50px">Color</th><th>Climb / Sink rate</th></tr>
<tr><td bgcolor="#FF0000"></td><td>+5.0 m/s</td></tr>
<tr><td bgcolor="#FF4000"></td><td>+4.5 m/s</td></tr>
<tr><td bgcolor="#FF8000"></td><td>+4.0 m/s</td></tr>
<tr><td bgcolor="#FFC000"></td><td>+3.5 m/s</td></tr>
<tr><td bgcolor="#FFFF00"></td><td>+3.0 m/s</td></tr>
<tr><td bgcolor="#C0FF00"></td><td>+2.5 m/s</td></tr>
<tr><td bgcolor="#80FF00"></td><td>+2.0 m/s</td></tr>
<tr><td bgcolor="#40FF80"></td><td>+1.5 m/s</td></tr>
<tr><td bgcolor="#00FFFF"></td><td>+1.0 m/s</td></tr>
<tr><td bgcolor="#00E0FF"></td><td>+0.5 m/s</td></tr>
<tr><td bgcolor="#00C0FF"></td><td>0.0 m/s</td></tr>
<tr><td bgcolor="#00A0FF"></td><td>-0.5 m/s</td></tr>
<tr><td bgcolor="#0080FF"></td><td>-1.0 m/s</td></tr>
<tr><td bgcolor="#0060E0"></td><td>-1.5 m/s</td></tr>
<tr><td bgcolor="#0040C0"></td><td>-2.0 m/s</td></tr>
<tr><td bgcolor="#0020A0"></td><td>-3.0 m/s</td></tr>
<tr><td bgcolor="#000080"></td><td>-3.5 m/s</td></tr>
<tr><td bgcolor="#400080"></td><td>-4.0 m/s</td></tr>
</tbody></table>

The colors are interpolated when rates between two colors were calculated.
This can happen for tracks with sub-second track point resolution.

## Current position

## Weather

## Settings

The settings page shows two tabs.

### General tab

![Settings](images/settings-general.png)

The General tab lets you specify an account for the Alptherm weather service.
When opening one of the Alptherm links, the app automatically logs you in. The
password is stored using
[secure storage](https://docs.microsoft.com/en-us/xamarin/essentials/secure-storage?tabs=android).

### Map tab

![Settings](images/settings-map.png)

The Map tab has several settings:

- Map Imagery: Determines the base imagery layer. The following layers are
  available:
  * OpenStreetMap: Tiles from the well known mapping project
  * Aerials + Labels (Bing Maps): Bing maps tiles, showing aerial photography
    and labels
  * Sentinel-2 Cloudless: Tiles showing terrain from Sentinel-2 satellite
    imagery
  * OpenTopoMap: Tiles showing topographical maps, from the open source
    project [opentopomap.org](https://opentopomap.org/)
  * OpenFlightMaps: Tiles showing flight related infos like airspaces, etc.,
    from the project [openflightmaps.org](https://www.openflightmaps.org/)

- Map Overlay: Shows a second half-transparent layer over the imagery layer:
  * None: No further layer
  * Thermal Skyways (thermal.kk7.ch): Displays thermal skyways from
    [thermal.kk7.ch](https://thermal.kk7.ch)
  * Contour lines: Displays contour lines with a distance of 100 meters
  * Slope + contour lines: Displays contour lines and additionally colors the
    ground based on the slope
  * NASA Black Marble: Displays the night imagery from NASA

- Map Shading: Determines how the map is shaded, based on the sun's position:
  * Fixed at 10 am: The sunlight shines on early SE slopes
  * Fixed at 3 pm: The sunlight shines on afternooon SW slopes
  * Follow current time: The sunlight follows the current time
  * Current time + 6 hours: The sun is positioned 6 hours into the future
  * No shading: No shading from sunlight

- Coordinates display format: Determines how latitude and longitude are
  displayed in the app:
  * dd.dddddd&deg;
  * dd&deg; mm.mmm'
  * dd&deg; mm' sss"

The "Clear map view cache" button clears the app's browser cache that stores
tile map images and terrain height infos.
