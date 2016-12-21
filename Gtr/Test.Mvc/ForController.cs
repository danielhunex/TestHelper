using Gtr.Test.Unit;
using Moq;
using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Gtr.Test.Mvc
{
    public class ForController<T> : TestFor<T> where T : Controller
    {
        private Action<RouteCollection> _routeRegistrar;
        private Mock<HttpRequestBase> _mockRequest;

        protected virtual Action<RouteCollection> RouteRegistrar
        {
            get { return _routeRegistrar ?? DefaultRouteRegistrar; }
            set { _routeRegistrar = value; }
        }

        protected Mock<HttpRequestBase> MockRequest
        {
            get
            {
                if (_mockRequest == null)
                {
                    _mockRequest = The<HttpRequestBase>();
                }

                return _mockRequest;
            }
        }

        protected override void TargetSetup()
        {
            var routes = new RouteCollection();
            RouteRegistrar(routes);

            var responseMock = The<HttpResponseBase>();
            responseMock.Setup(x => x.ApplyAppPathModifier(It.IsAny<string>())).Returns((string url) => url);

            var contextMock = The<HttpContextBase>();
            contextMock.SetupGet(x => x.Request).Returns(MockRequest.Object);
            contextMock.SetupGet(x => x.Response).Returns(responseMock.Object);
            contextMock.SetupGet(x => x.Session).Returns(The<HttpSessionStateBase>().Object);

            Target.ControllerContext = new ControllerContext(contextMock.Object, new RouteData(), Target);
            Target.Url = new UrlHelper(new RequestContext(contextMock.Object, new RouteData()), routes);
        }

        protected void DefaultRouteRegistrar(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional });
        }
    }
}
