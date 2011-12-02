using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Microsoft.UI.Xaml.Controls
{
    // TEMPORARY comments denote areas where this has been written in simulated base class form
    // because of runtime issues that prevent custom base classes

    //TEMPORARY
    public interface ISimulatedSubclass
    {
        string DetermineViewStateName(ApplicationViewState viewState);
    }

    public class LayoutAwarePage // TEMPORARY - should be public class LayoutAwarePage : Page
    {
        private Control _simulatedThis; //TEMPORARY
        private List<Control> _layoutAwareControls;

        public LayoutAwarePage(Control simulatedThis) //TEMPORARY parameter
        {
            _simulatedThis = simulatedThis; //TEMPORARY
            _simulatedThis.Loaded += StartLayoutUpdates; //TEMPORARY - should be this.Loaded += StartLayoutUpdates;
            _simulatedThis.Unloaded += StopLayoutUpdates; //TEMPORARY - should be this.Unloaded += StopLayoutUpdates;
        }

        public void StartLayoutUpdates(object sender, RoutedEventArgs e)
        {
            var control = sender as Control;
            if (control == null) return;
            if (_layoutAwareControls == null)
            {
                ApplicationView.GetForCurrentView().ViewStateChanged += ViewStateChanged;
                _layoutAwareControls = new List<Control>();
            }
            _layoutAwareControls.Add(control);
            VisualStateManager.GoToState(control, DetermineViewStateName(ApplicationView.Value), false);
        }

        public void StopLayoutUpdates(object sender, RoutedEventArgs e)
        {
            var control = sender as Control;
            if (control == null || _layoutAwareControls == null) return;
            _layoutAwareControls.Remove(control);
            if (_layoutAwareControls.Count == 0)
            {
                _layoutAwareControls = null;
                ApplicationView.GetForCurrentView().ViewStateChanged -= ViewStateChanged;
            }
        }

        private void ViewStateChanged(ApplicationView sender, ApplicationViewStateChangedEventArgs e)
        {
            if (_layoutAwareControls != null)
            {
                string viewStateName = DetermineViewStateName(e == null ? ApplicationView.Value : e.ViewState);
                foreach (var layoutAwareControl in _layoutAwareControls)
                {
                    VisualStateManager.GoToState(layoutAwareControl, viewStateName, false);
                }
            }
        }

        // Designed to be overridden for custom state management
        protected virtual string DetermineViewStateName(ApplicationViewState viewState)
        {
            var overloaded = _simulatedThis as ISimulatedSubclass; //TEMPORARY
            if (overloaded != null) return overloaded.DetermineViewStateName(viewState); //TEMPORARY
            return this.DetermineDefaultLayoutStateName(viewState);
        }

        public string DetermineDefaultLayoutStateName(ApplicationViewState viewState)
        {
            switch (viewState)
            {
                case ApplicationViewState.FullScreenPortrait: return "Portrait";
                case ApplicationViewState.Snapped: return "Snapped";
                case ApplicationViewState.Filled: return "Filled";
                default: return "FullScreen";
            }
        }

        public void InvalidateViewState()
        {
            ViewStateChanged(null, null);
        }
    }
}
