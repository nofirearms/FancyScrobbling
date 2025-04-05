using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FancyScrobbling.Avalonia;

public partial class DialogView : UserControl
{


    public object CustomContent
    {
        get => GetValue(CustomContentProperty);
        set => SetValue(CustomContentProperty, value);
    }
    public static readonly StyledProperty<object> CustomContentProperty =
        AvaloniaProperty.Register<DialogView, object>(nameof(CustomContent), default);

    public DialogView()
    {
        InitializeComponent();
    }
}