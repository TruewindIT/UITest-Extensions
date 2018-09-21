using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.UITest;

namespace Truewind.UITest.Extensions
{
    /// <summary>
    /// Method Extensions for IApp interface.
    /// </summary>
    public static class IAppExtensions
    {
        private static TimeSpan DEFAULT_TIMEOUT = new TimeSpan(0, 1, 30);

        private static bool UseXPath(string s)
        {
            return s.StartsWith("/");
        }

        /// <summary>
        /// Returns application object as an AndroidApp.
        /// </summary>
        /// <param name="app">The application on which the extension method is executed.</param>
        /// <returns>The application object as an AndroidApp. If the object cannot be casted the return value will be null.</returns>
        public static Xamarin.UITest.Android.AndroidApp AsAndroid(this IApp app)
        {
            return (app as Xamarin.UITest.Android.AndroidApp);
        }

        /// <summary>
        /// Returns application object as an iOSApp.
        /// </summary>
        /// <param name="app">The application on which the extension method is executed.</param>
        /// <returns>The application object as an iOSApp. If the object cannot be casted the return value will be null.</returns>
        public static Xamarin.UITest.iOS.iOSApp AsiOS(this IApp app)
        {
            return (app as Xamarin.UITest.iOS.iOSApp);
        }

        /// <summary>
        /// Takes a screenshot of the app in it's current state.
        /// This is used to denote test steps in App Center.
        /// </summary>
        /// <param name="app">The application on which the extension method is executed.</param>
        /// <param name="condition">The condition to be evaluated before the screenshot is taken. If the condition is false, the screenshot will not be taken.</param>
        /// <param name="screenshotTitle">The title of screenshot, used as step name. If not supplied will default to "Screenshot".</param>
        /// <returns>The screenshot file. If the condition is false, the return value will be null.</returns>
        public static FileInfo ScreenshotIf(this IApp app, bool condition = false, string screenshotTitle = null)
        {
            return condition ? app.Screenshot(screenshotTitle ?? "Screenshot") : null;
        }

        /// <summary>
        /// Enters text into a matching element that supports it.
        /// Optionally clears text from the element.
        /// Optionally takes screenshots of the app before and/or after the action.
        /// </summary>
        /// <param name="app">The application on which the extension method is executed.</param>
        /// <param name="selector">XPath or Css selector to specify the element.</param>
        /// <param name="text">The text to enter.</param>
        /// <param name="clear">Option to clear the element before entering the text.</param>
        /// <param name="wait">Option to disable wait for the element after dismiss keyboard (for example if an action is automatically fired when entering the text).</param>
        /// <param name="screenshot">Option to take a screenshot after the action.</param>
        /// <param name="screenshotTitle">Option to specify the screenshot title.</param>
        public static void EnterTextAndDismissKeyboard(this IApp app, string selector, string text, bool clear = false, bool wait = true, bool screenshot = false, string screenshotTitle = null)
        {
            app.Wait(selector);

            if (clear)
            {
                app.ClearText(x => UseXPath(selector) ? x.XPath(selector) : x.Css(selector));
                app.DismissKeyboard();
            }

            app.EnterText(x => UseXPath(selector) ? x.XPath(selector) : x.Css(selector), text);
            app.DismissKeyboard();

            if (wait) app.Wait(selector, screenshot, screenshotTitle ?? $"Entered Text on {selector}");
        }
        
        /// <summary>
        /// Performs a scroll down on the WebView.
        /// </summary>
        /// <param name="app">The application on which the extension method is executed.</param>
        /// <param name="ratioFromY">Relative position on the screen (value between 0 and 1) indicating where to start the scroll from.</param>
        /// <param name="ratioToY">Relative position on the screen (value between 0 and 1) indicating where to end the scroll.</param>
        /// <param name="screenshot">Option to take a screenshot after the action.</param>
        /// <param name="screenshotTitle">Option to specify the screenshot title.</param>
        public static void ManualScroll(this IApp app, float ratioFromY, float ratioToY, bool screenshot = false, string screenshotTitle = null)
        {
            var webview = app.Query(y => y.WebView()).FirstOrDefault();
            if (webview != null)
            {
                var fromX = webview.Rect.CenterX;
                var fromY = (webview.Rect.Y + webview.Rect.Height) * ratioFromY;
                var toX = webview.Rect.CenterX;
                var toY = (webview.Rect.Y + webview.Rect.Height) * ratioToY;
                app.DragCoordinates(fromX, fromY, toX, toY);
                app.ScreenshotIf(screenshot, screenshotTitle ?? $"Scrolled WebView Down");
            }
        }

        /// <summary>
        /// Performs a scroll down on the WebView.
        /// </summary>
        /// <param name="app">The application on which the extension method is executed.</param>
        /// <param name="screenshot">Option to take a screenshot after the action.</param>
        /// <param name="screenshotTitle"> Option to specify the screenshot title.</param>
        public static void ManualScrollDown(this IApp app, bool screenshot = false, string screenshotTitle = null)
        {
            app.ManualScroll(0.66f, 0.33f, screenshot, screenshotTitle);
        }

        /// <summary>
        /// Performs a scroll up on the WebView.
        /// </summary>
        /// <param name="app">The application on which the extension method is executed.</param>
        /// <param name="screenshot">Option to take a screenshot after the action.</param>
        /// <param name="screenshotTitle">Option to specify the screenshot title.</param>
        public static void ManualScrollUp(this IApp app, bool screenshot = false, string screenshotTitle = null)
        {
            app.ManualScroll(0.33f, 0.66f, screenshot, screenshotTitle);
        }

        /// <summary>
        /// Performs a swipe on the WebView.
        /// </summary>
        /// <param name="app">The application on which the extension method is executed.</param>
        /// <param name="coordY">Vertical position on the screen indicating where to preform the scroll.</param>
        /// <param name="ratioFromX">Relative position on the screen (value between 0 and 1) indicating where to start the scroll from.</param>
        /// <param name="ratioToX">Relative position on the screen (value between 0 and 1) indicating where to end the scroll.</param>
        /// <param name="screenshot">Option to take a screenshot after the action.</param>
        /// <param name="screenshotTitle">Option to specify the screenshot title.</param>
        public static void ManualSwipe(this IApp app, float coordY, float ratioFromX, float ratioToX, bool screenshot = false, string screenshotTitle = null)
        {
            var webview = app.Query(y => y.WebView()).FirstOrDefault();
            if (webview != null)
            {
                var fromY = coordY;
                var fromX = (webview.Rect.X + webview.Rect.Width) * ratioFromX;
                var toY = coordY;
                var toX = (webview.Rect.X + webview.Rect.Width) * ratioToX;
                app.DragCoordinates(fromX, fromY, toX, toY);
                app.ScreenshotIf(screenshot, screenshotTitle ?? $"Swiped WebView");
            }
        }

        /// <summary>
        /// Performs a scroll down on the WebView.
        /// If a selector is specified, performs a scroll down on the WebView until an element that matches the selector is shown on the screen.
        /// </summary>
        /// <param name="app">The application on which the extension method is executed.</param>
        /// <param name="selector">XPath or Css selector to specify the element to bring on screen.</param>
        /// <param name="screenshot">Option to take a screenshot after the action.</param>
        /// <param name="screenshotTitle">Option to specify the screenshot title.</param>
        public static void ScrollWebViewDown(this IApp app, string selector = null, bool screenshot = false, string screenshotTitle = null)
        {   
            // Must use Gesture otherwise it won't work on some cases...
            if (selector == null)
                app.ScrollDown(y => y.WebView(), ScrollStrategy.Gesture);
            else 
                app.ScrollDownTo(x => UseXPath(selector) ? x.XPath(selector) : x.Css(selector), y => y.WebView(), ScrollStrategy.Gesture);

            app.ScreenshotIf(screenshot, screenshotTitle ?? $"Scrolled WebView Down To {selector}");
        }

        /// <summary>
        /// Performs a scroll up on the WebView.
        /// If a selector is specified, performs a scroll up on the WebView until an element that matches the selector is shown on the screen.
        /// </summary>
        /// <param name="app">The application on which the extension method is executed.</param>
        /// <param name="selector">XPath or Css selector to specify the element to bring on screen.</param>
        /// <param name="screenshot">Option to take a screenshot after the action.</param>
        /// <param name="screenshotTitle">Option to specify the screenshot title.</param>
        public static void ScrollWebViewUp(this IApp app, string selector = null, bool screenshot = false, string screenshotTitle = null)
        {
            // Must use Gesture otherwise it won't work on some cases...
            if (selector == null)
                app.ScrollUp(y => y.WebView(), ScrollStrategy.Gesture);
            else
                app.ScrollUpTo(x => UseXPath(selector) ? x.XPath(selector) : x.Css(selector), y => y.WebView(), ScrollStrategy.Gesture);

            app.ScreenshotIf(screenshot, screenshotTitle ?? $"Scrolled WebView Up To: {selector}");
        }

        /// <summary>
        /// Wait function that will repeatly query the app until a WebView is found.
        /// Throws a System.TimeoutException if no element is found within the time limit.
        /// </summary>
        /// <param name="app">The application on which the extension method is executed.</param>
        /// <param name="screenshot">Option to take a screenshot after the action.</param>
        /// <param name="screenshotTitle">Option to specify the screenshot title.</param>
        public static void WaitWebView(this IApp app, bool screenshot = false, string screenshotTitle = null)
        {
            app.WaitForElement(a => a.WebView(), "Timed out waiting for element...", DEFAULT_TIMEOUT);
            app.ScreenshotIf(screenshot, screenshotTitle ?? "WebView loaded");
        }

        /// <summary>
        /// Wait function that will repeatly query the app until an element matching the selector is found (works even if the element is not visible on screen).
        /// Throws a System.TimeoutException if no element is found within the time limit.
        /// </summary>
        /// <param name="app">The application on which the extension method is executed.</param>
        /// <param name="selector">XPath or Css selector to specify the element to bring on screen.</param>
        /// <param name="screenshot">Option to take a screenshot after the action.</param>
        /// <param name="screenshotTitle">Option to specify the screenshot title.</param>
        public static void WaitAll(this IApp app, string selector, bool screenshot = false, string screenshotTitle = null)
        {
            app.WaitForElement(x => UseXPath(selector) ? x.All().XPath(selector) : x.All().Css(selector), "Timed out waiting for element...", DEFAULT_TIMEOUT);
            app.ScreenshotIf(screenshot, screenshotTitle ?? $"Waited: {selector}");
        }

        /// <summary>
        /// Queries web view objects using a Css or XPath selector.
        /// Defaults to only return view objects that are visible.
        /// </summary>
        /// <param name="app">The application on which the extension method is executed.</param>
        /// <param name="selector">XPath or Css selector to specify the element to query. If left as null returns all visible view objects.</param>
        /// <returns>An array representing the matched view objects.</returns>
        public static Xamarin.UITest.Queries.AppWebResult[] WebViewQuery(this IApp app, string selector)
        {
            return app.Query(x => UseXPath(selector) ? x.XPath(selector) : x.Css(selector));
        }

        /// <summary>
        /// Wait function that will repeatly query the app until an element matching the selector is found (works only for visible elements).
        /// Throws a System.TimeoutException if no element is found within the time limit.
        /// </summary>
        /// <param name="app">The application on which the extension method is executed.</param>
        /// <param name="selector">XPath or Css selector to specify the element to bring on screen.</param>
        /// <param name="screenshot">Option to take a screenshot after the action.</param>
        /// <param name="screenshotTitle">Option to specify the screenshot title.</param>
        public static void Wait(this IApp app, string selector, bool screenshot = false, string screenshotTitle = null)
        {
            app.WaitForElement(x => UseXPath(selector) ? x.XPath(selector) : x.Css(selector), "Timed out waiting for element...", DEFAULT_TIMEOUT);
            app.ScreenshotIf(screenshot, screenshotTitle ?? $"Waited: {selector}");
        }

        /// <summary>
        /// Wait function that will repeatly query the app until an element matching the selector is no longer
        /// found. Throws a System.TimeoutException if the element is visible at the end of the time limit.
        /// </summary>
        /// <param name="app">The application on which the extension method is executed.</param>
        /// <param name="selector">XPath or Css selector to specify the element to bring on screen.</param>
        /// <param name="screenshot">Option to take a screenshot after the action.</param>
        /// <param name="screenshotTitle">Option to specify the screenshot title.</param>
        public static void WaitNo(this IApp app, string selector, bool screenshot = false, string screenshotTitle = null)
        {
            app.WaitForNoElement(x => UseXPath(selector) ? x.XPath(selector) : x.Css(selector), "Timed out waiting for no element...", DEFAULT_TIMEOUT);
            app.ScreenshotIf(screenshot, screenshotTitle ?? $"Waited no: {selector}");
        }

        /// <summary>
        /// Performs a tap / touch gesture on the element matching the selector.
        /// If multiple elements are matched, the first one will be used.
        /// </summary>
        /// <param name="app">The application on which the extension method is executed.</param>
        /// <param name="selector">XPath or Css selector to specify the element to bring on screen.</param>
        /// <param name="screenshot">Option to take a screenshot after the Wait action.</param>
        /// <param name="screenshotTitle">Option to specify the screenshot title.</param>
        public static void WaitAndTap(this IApp app, string selector, bool screenshot = false, string screenshotTitle = null)
        {
            app.Wait(selector, screenshot, screenshotTitle);
            app.Tap(x => UseXPath(selector) ? x.XPath(selector) : x.Css(selector));
        }

        /// <summary>
        /// Returns the WebView's User Agent string
        /// </summary>
        /// <param name="app">The application on which the extension method is executed.</param>
        public static string WebViewGetUserAgent(this IApp app)
        {
            app.WaitWebView();
            var query = app.Query(a => a.WebView().InvokeJS("return navigator.userAgent;"));
            return (query != null ? String.Join(Environment.NewLine, query) : null);
        }

        /// <summary>
        /// Highlights the HTML element on the WebView by making it flash. Element must be identified by its id.
        /// </summary>
        /// <param name="app">The application on which the extension method is executed.</param>
        /// <param name="htmlElementId">HTML Element Identified (attribute id).</param>
        public static void WebViewFlashElement(this IApp app, string htmlElementId)
        {
            app.WaitWebView();
            app.Query(a =>
                a.WebView().InvokeJS(
                    System.String.Format(@"
var element = document.getElementById('{0}');
element.style.visibility='hidden';
setTimeout(function(){{
    element.style.visibility = 'visible';
    setTimeout(function(){{
        element.style.visibility = 'hidden';
        setTimeout(function(){{
            element.style.visibility = 'visible';
            setTimeout(function(){{
                element.style.visibility = 'hidden';
                setTimeout(function(){{
                    element.style.visibility = 'visible';
                }},({1}));
            }},({1}));
        }},({1}));
    }},({1}));
}},({1}));
", htmlElementId, 500)
                )
            );
        }
    }
}
