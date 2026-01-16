using System.Net;
using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using ResturangFrontEnd.Controllers;
using ResturangFrontEnd.Models;
using Restaurant_Frontend_Tests.Helpers;

namespace Restaurant_Frontend_Tests.Controllers
{
    public class HomeControllerTests
    {
        [Fact]
        public async Task Index_ReturnsMenuItemsFromApi()
        {
            var menuItem = new MenuItem { MenuItemID = 1, Name = "Dish" };

            var handler = new DelegatingHandlerStub((request, _) =>
            {
                request.Method.Should().Be(HttpMethod.Get);
                request.RequestUri!.AbsoluteUri.Should().Contain("api/MenuItems");

                var json = JsonConvert.SerializeObject(new List<MenuItem> { menuItem });
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };
            });

            var controller = new HomeController(NullLogger<HomeController>.Instance, new HttpClient(handler));

            var result = await controller.Index();

            var view = result.Should().BeOfType<ViewResult>().Subject;
            var model = view.Model.Should().BeAssignableTo<List<MenuItem>>().Subject;
            model.Should().ContainSingle().Which.MenuItemID.Should().Be(menuItem.MenuItemID);
        }

        [Fact]
        public async Task Menu_ReturnsMenuItemsFromApi()
        {
            var menuItem = new MenuItem { MenuItemID = 2, Name = "Soup" };

            var handler = new DelegatingHandlerStub((request, _) =>
            {
                request.Method.Should().Be(HttpMethod.Get);
                request.RequestUri!.AbsoluteUri.Should().Contain("api/MenuItems");

                var json = JsonConvert.SerializeObject(new List<MenuItem> { menuItem });
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };
            });

            var controller = new HomeController(NullLogger<HomeController>.Instance, new HttpClient(handler));

            var result = await controller.Menu();

            var view = result.Should().BeOfType<ViewResult>().Subject;
            var model = view.Model.Should().BeAssignableTo<List<MenuItem>>().Subject;
            model.Should().ContainSingle().Which.MenuItemID.Should().Be(menuItem.MenuItemID);
        }

        [Fact]
        public void Privacy_ReturnsView()
        {
            var controller = new HomeController(NullLogger<HomeController>.Instance, new HttpClient(new DelegatingHandlerStub((_, __) => new HttpResponseMessage(HttpStatusCode.OK))));

            var result = controller.Privacy();

            result.Should().BeOfType<ViewResult>();
        }

        [Fact]
        public void Error_ReturnsErrorViewModelWithRequestId()
        {
            var controller = new HomeController(NullLogger<HomeController>.Instance, new HttpClient(new DelegatingHandlerStub((_, __) => new HttpResponseMessage(HttpStatusCode.OK))));
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            var result = controller.Error();

            var view = result.Should().BeOfType<ViewResult>().Subject;
            var model = view.Model.Should().BeOfType<ErrorViewModel>().Subject;
            model.RequestId.Should().NotBeNullOrEmpty();
        }
    }
}
