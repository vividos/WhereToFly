# Where-to-fly MapView library

This is the .NET MAUI MapView library project that provides map view and
height profile content views usable in a .NET MAUI project.

The project uses the Where-to-fly web library and hosts it inside a WebView in
order to render the controls.

## Usage

In `XAML` files, specify the namespace to use and reference the controls:

    <ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
        xmlns:mapview="clr-namespace:WhereToFly.App.MapView;assembly=WhereToFly.App.MapView"
    ...
    <mapview:MapView
        VerticalOptions="Fill"
        HorizontalOptions="Fill" />
    ...
    <mapView:HeightProfileView
       VerticalOptions="Fill"
       HorizontalOptions="Fill"
       Track="{Binding Track}"
       UseDarkTheme="True" />
    
Or use the controls from `C#`:

    Content = new MapView
    {
        VerticalOptions = LayoutOptions.Fill,
        HorizontalOptions = LayoutOptions.Fill,
        NearbyPoiService = nearbyPoiService,
    };

and

    Content = new HeightProfileView
    {
        HorizontalOptions = LayoutOptions.Fill,
        VerticalOptions = LayoutOptions.Fill,
        Track = track,
        UseDarkTheme = true,
    };

## License

The library is licensed under the [MIT license](../../../LICENSE.md).
