using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Xaml.Interactivity;
using FancyScrobbling.Avalonia.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FancyScrobbling.Avalonia.Behaviors
{
    public class ListBixBehaviors : Behavior<ListBox>
    {


        /// <summary>
        /// SelectedItems StyledProperty definition
        /// </summary>
        public static readonly StyledProperty<IEnumerable<ScrobbleTrackViewModel>> SelectedItemsProperty =
            AvaloniaProperty.Register<ListBixBehaviors, IEnumerable<ScrobbleTrackViewModel>>(nameof(SelectedItems));

        /// <summary>
        /// Gets or sets the SelectedItems property. This StyledProperty
        /// indicates ....
        /// </summary>
        public IEnumerable<ScrobbleTrackViewModel> SelectedItems
        {
            get => this.GetValue(SelectedItemsProperty);
            set => SetValue(SelectedItemsProperty, value);
        }


        protected override void OnAttached()
        {
            AssociatedObject.SelectionChanged += OnSelectionChanged;
        }

        private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            SelectedItems = ((AvaloniaList<object>)AssociatedObject.SelectedItems).Cast<ScrobbleTrackViewModel>().ToList();
        }
    }
}
