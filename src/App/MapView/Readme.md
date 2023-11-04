# Where-to-fly MapView library

This is the Xamarin.Forms MapView library project that provides map view and
height profile content views usable in a Xamarin.Forms project.

The project uses the Where-to-fly web library and hosts it inside a WebView in
order to render the controls.

## Usage

In `XAML` files, specify the namespace to use and reference the controls:

    <ContentPage xmlns="http://xamarin.com/schemas/2014/forms"
        xmlns:mapview="clr-namespace:WhereToFly.App.MapView;assembly=WhereToFly.App.MapView"
    ...
    <mapview:MapView 
        VerticalOptions="FillAndExpand"
        HorizontalOptions="FillAndExpand" />
    ...
    <mapView:HeightProfileView
       VerticalOptions="FillAndExpand"
       HorizontalOptions="FillAndExpand"
       Track="{Binding Track}"
       UseDarkTheme="True" />
    
Or use the controls from `C#`:

    Content = new MapView
    {
        VerticalOptions = LayoutOptions.FillAndExpand,
        HorizontalOptions = LayoutOptions.FillAndExpand,
        NearbyPoiService = nearbyPoiService,
    };

and

    Content = new HeightProfileView
    {
        HorizontalOptions = LayoutOptions.FillAndExpand,
        VerticalOptions = LayoutOptions.FillAndExpand,
        Track = track,
        UseDarkTheme = true,
    };

## License

The library is licensed under the [MIT license](../../../LICENSE.md).
