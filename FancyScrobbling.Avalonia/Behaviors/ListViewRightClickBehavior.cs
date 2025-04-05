using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FancyScrobbling.Avalonia.Behaviors
{
    public class ListViewRightClickBehavior : Behavior<InputElement>
    {


        public ICommand Command
        {
            get => this.GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public static readonly StyledProperty<ICommand> CommandProperty =
            AvaloniaProperty.Register<ListViewRightClickBehavior, ICommand>(nameof(Command));



        protected override void OnAttached()
        {
            AssociatedObject.PointerPressed += OnPointerPressed;
        }

        private void OnPointerPressed(object? sender, global::Avalonia.Input.PointerPressedEventArgs e)
        {
            
            var point = e.GetCurrentPoint(AssociatedObject);
            if (point.Properties.IsRightButtonPressed)
            {
                e.Handled = true;
                Command?.Execute(null);
            }
        }
    }
}
