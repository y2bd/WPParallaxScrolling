using Microsoft.Phone.Controls;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace StretchTest
{
  public partial class MainPage : PhoneApplicationPage
  {
    // this is the height of the empty space above the content
    // that we created in the XAML
    private const double EmptySpace = 500;

    // this will represent the vertical position of the image
    // we're storing it in a variable because we'll need it later
    private double _startPosition;

    // this will be our backing property for the binding
    private double _verticalOffset;
    public double VerticalOffset
    {
      get { return _verticalOffset; }
      set
      {
        _verticalOffset = value;

        // update our header image position accordingly
        onVerticalOffsetChanged();
      }
    }

    public MainPage()
    {
      InitializeComponent();

      // we're putting our code in a Loaded event handler
      // because we have to wait until the image control is loaded
      // before getting its height
      Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
      // Here's the magic
      // To get the image where we want, we will shift it up by its height
      // Then we'll shift it down by the height of the empty space
      // Last, we'll divide that by two so we'll be directly between the top 
      // of the page and the bottom of the empty space
      _startPosition = (-Image.ActualHeight + EmptySpace) / 2;

      // set the TranslateY of the CompositeTransform we created in the XAML
      // to our calculated start position
      ImageTransform.TranslateY = _startPosition;

      var scrollbar =
        ((FrameworkElement) VisualTreeHelper.GetChild(Scroller, 0)).FindName("VerticalScrollBar") as ScrollBar;

      if (scrollbar != null)
      {
        scrollbar.ValueChanged += OnScrollbarValueChanged;
      }

      // when the mouse (the user tapping his screen) moves
      // within the ScrollViewer, there's a chance that
      // the ScrollViewer's Content's RenderTransform will be set to
      // a new CompositeTransform that we need to grab a hold of
      Scroller.MouseMove += ScrollerOnMouseMove;
    }

    private void OnScrollbarValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
      // if we're in positive scrolling territory
      // where positive means down
      if (e.NewValue > 0)
      {
        // we're going to scroll our Image along with the content
        // but only half as fast
        // try removing the /2.0 and see what happens
        ImageTransform.TranslateY = _startPosition - e.NewValue/2.0;

        // instead of using the scrollbar's value, you can also use the VerticalOffset
        // of the ScrollViewer itself
        // try it, see which one works better for you performance-wise
        // ImageTransform.TranslateY = _startPosition - Scroller.VerticalOffset/2.0;
      }
    }
    
    private void onVerticalOffsetChanged()
    {
      // now let's do the same thing we did with the scrollbar

      // we only want to handle the squishes here, as the 
      // scrollbar events handle the normal scrolling
      // so we'll only respond to the squishes, when the content
      // is being moved down
      if (VerticalOffset >= 0)
      {
        ImageTransform.TranslateY = _startPosition + VerticalOffset/2.0;
      }
    }

    private void ScrollerOnMouseMove(object sender, MouseEventArgs mouseEventArgs)
    {
      // grab the content element
      var uiElement = Scroller.Content as UIElement;
      if (uiElement != null)
      {
        // try to grab its transform as a CompositeTransform
        var transform = uiElement.RenderTransform as CompositeTransform;

        // if it's actually a CompositeTransform
        if (transform != null)
        {
          // we're good, let's go to town!

          // let's set up the binding in a standard manner
          var binding = new Binding("VerticalOffset");

          // in a perfect world, we use a reverse OneWay binding, where
          // the DP we're binding to can set the backing property
          // as this doesn't exist in the world of Windows Phone
          // we're going to cheat and use TwoWay
          binding.Mode = BindingMode.TwoWay;
          binding.Source = this;

          BindingOperations.SetBinding(transform, CompositeTransform.TranslateYProperty, binding);

          // we're going to release the event handler
          // since we only need to bind once
          // i recommend detaching the other event
          // handlers we've used at some point as well, as it's good form
          Scroller.MouseMove -= ScrollerOnMouseMove;
        }
      }
    }
    
  }
}