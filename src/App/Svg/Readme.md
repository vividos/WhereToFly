# Where-to-fly SVG image library

This is the SVG image library project that provides an `SvgImage` Maui control
to dynamically display `.svg` images in the Where-to-fly app.

## Usage

- Add this project to the .NET MAUI project (there's no NuGet project yet)
- Add XML reference in the `xaml` file: 

      xmlns:svg="clr-namespace:WhereToFly.App.Svg;assembly=WhereToFly.App.Svg"

- Add the SvgImage control:

      <svg:SvgImage Source="{Binding ImageSource}"
                    TintColor="Black"
                    WidthRequest="64" HeightRequest="64" />

The `ImageSource` bindable property must be a `string` or an `ImageSource`
object.

The `TintColor` bindable property can also specify an app theme dependent
color or `{x:Null}` to not set a tint color.

The control is automatically redrawn when the image source, tint color or the
control's size change.

Also check out the sample project in the repository for further use cases of
the control.

## License

The library is licensed under the [MIT license](../../../LICENSE.md).
