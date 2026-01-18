using System.Net;
using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ResturangFrontEnd.Controllers;
using ResturangFrontEnd.Models;
using Restaurant_Frontend_Tests.Helpers;

namespace Restaurant_Frontend_Tests.Controllers
{
    public class MenusControllerTests
    {
        [Fact]
        public async Task Index_Get_ReturnsViewWithMenusFromApi()
        {
            var menus = new List<Menu>
            {
                new() { MenuID = 1, Name = "Food" },
                new() { MenuID = 2, Name = "Drinks" }
            };

            var handler = new DelegatingHandlerStub((request, _) =>
            {
                request.Method.Should().Be(HttpMethod.Get);
                request.RequestUri!.AbsoluteUri.Should().Contain("api/Menus");

                var json = JsonConvert.SerializeObject(menus);
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };
            });

            var controller = new MenuController(new HttpClient(handler));

            var result = await controller.Index();

            var view = result.Should().BeOfType<ViewResult>().Subject;
            view.ViewName.Should().Be("~/Views/Menu/Index.cshtml");
            var model = view.Model.Should().BeOfType<List<Menu>>().Subject;
            model.Should().HaveCount(2);
        }

        [Fact]
        public async Task Create_Post_InvalidModel_ReturnsViewWithSameModel()
        {
            var controller = new MenuController(
                new HttpClient(new DelegatingHandlerStub((_, __) => new HttpResponseMessage(HttpStatusCode.OK))));

            controller.ModelState.AddModelError("Name", "Required");

            var menu = new Menu { Name = "" };

            var result = await controller.Create(menu);

            var view = result.Should().BeOfType<ViewResult>().Subject;
            view.ViewName.Should().Be("~/Views/Menu/Create.cshtml");
            view.Model.Should().BeSameAs(menu);
        }

        [Fact]
        public async Task Create_Post_ValidModel_RedirectsToIndex()
        {
            var handler = new DelegatingHandlerStub((request, _) =>
            {
                request.Method.Should().Be(HttpMethod.Post);
                request.RequestUri!.AbsoluteUri.Should().Contain("api/Menus/CreateMenu");
                return new HttpResponseMessage(HttpStatusCode.Created);
            });

            var controller = new MenuController(new HttpClient(handler));

            var menu = new Menu { Name = "Desserts" };

            var result = await controller.Create(menu);

            var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
        }

        [Fact]
        public async Task Edit_Get_ReturnsViewWithMenuFromApi()
        {
            var menu = new Menu { MenuID = 5, Name = "Food" };

            var handler = new DelegatingHandlerStub((request, _) =>
            {
                request.Method.Should().Be(HttpMethod.Get);
                request.RequestUri!.AbsoluteUri.Should().Contain("GetSpecificMenu/5");

                var json = JsonConvert.SerializeObject(menu);
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };
            });

            var controller = new MenuController(new HttpClient(handler));

            var result = await controller.Edit(5);

            var view = result.Should().BeOfType<ViewResult>().Subject;
            view.ViewName.Should().Be("~/Views/Menu/Edit.cshtml");
            var model = view.Model.Should().BeOfType<Menu>().Subject;
            model.MenuID.Should().Be(5);
        }

        [Fact]
        public async Task Edit_Post_InvalidModel_ReturnsViewWithSameModel()
        {
            var controller = new MenuController(
                new HttpClient(new DelegatingHandlerStub((_, __) => new HttpResponseMessage(HttpStatusCode.OK))));

            controller.ModelState.AddModelError("Name", "Required");

            var menu = new Menu { MenuID = 7, Name = "" };

            var result = await controller.Edit(menu);

            var view = result.Should().BeOfType<ViewResult>().Subject;
            view.ViewName.Should().Be("~/Views/Menu/Edit.cshtml");
            view.Model.Should().BeSameAs(menu);
        }

        [Fact]
        public async Task Edit_Post_ValidModel_RedirectsToIndex()
        {
            var handler = new DelegatingHandlerStub((request, _) =>
            {
                request.Method.Should().Be(HttpMethod.Put);
                request.RequestUri!.AbsoluteUri.Should().Contain("api/Menus/UpdateMenu/7");
                return new HttpResponseMessage(HttpStatusCode.NoContent);
            });

            var controller = new MenuController(new HttpClient(handler));

            var menu = new Menu { MenuID = 7, Name = "Drinks" };

            var result = await controller.Edit(menu);

            var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
        }

        [Fact]
        public async Task Delete_Post_RedirectsToIndex()
        {
            var handler = new DelegatingHandlerStub((request, _) =>
            {
                request.Method.Should().Be(HttpMethod.Delete);
                request.RequestUri!.AbsoluteUri.Should().Contain("api/Menus/DeleteMenu/3");
                return new HttpResponseMessage(HttpStatusCode.NoContent);
            });

            var controller = new MenuController(new HttpClient(handler));

            var result = await controller.Delete(3);

            var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
        }
    }
}
