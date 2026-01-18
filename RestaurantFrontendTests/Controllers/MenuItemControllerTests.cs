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
    public class MenuItemControllerTests
    {
        [Fact]
        public async Task Index_ReturnsViewWithMenuItemsFromApi()
        {
            var menuItem = new MenuItem
            {
                MenuItemID = 1,
                Name = "Dish",
                Description = "Tasty",
                Price = 99
            };

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

            var controller = new MenuItemController(new HttpClient(handler));

            var result = await controller.Index();

            var view = result.Should().BeOfType<ViewResult>().Subject;
            var model = view.Model.Should().BeAssignableTo<List<MenuItem>>().Subject;
            model.Should().ContainSingle().Which.MenuItemID.Should().Be(menuItem.MenuItemID);
        }

        [Fact]
        public void Create_Get_ReturnsView()
        {
            var controller = new MenuItemController(new HttpClient(new DelegatingHandlerStub((_, __) => new HttpResponseMessage(HttpStatusCode.OK))));

            var result = controller.Create();

            result.Should().BeOfType<ViewResult>();
        }

        [Fact]
        public async Task Create_Post_InvalidModel_ReturnsViewWithModel()
        {
            var controller = new MenuItemController(new HttpClient(new DelegatingHandlerStub((_, __) => new HttpResponseMessage(HttpStatusCode.OK))));
            controller.ModelState.AddModelError("Name", "Required");

            var menuItem = new MenuItem { Name = string.Empty };

            var result = await controller.Create(menuItem);

            var view = result.Should().BeOfType<ViewResult>().Subject;
            view.Model.Should().BeSameAs(menuItem);
        }

        [Fact]
        public async Task Create_Post_ValidModel_RedirectsToIndexAndPosts()
        {
            var handler = new DelegatingHandlerStub((request, _) =>
            {
                request.Method.Should().Be(HttpMethod.Post);
                request.RequestUri!.AbsoluteUri.Should().Contain("CreateMenuItem");
                return new HttpResponseMessage(HttpStatusCode.Created);
            });

            var controller = new MenuItemController(new HttpClient(handler));

            var menuItem = new MenuItem
            {
                MenuItemID = 2,
                Name = "Soup",
                Description = "Hot",
                Price = 55
            };

            var result = await controller.Create(menuItem);

            var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
        }

        [Fact]
        public async Task Edit_Get_ReturnsMenuItemFromApi()
        {
            var menuItem = new MenuItem
            {
                MenuItemID = 5,
                Name = "Pasta"
            };

            var handler = new DelegatingHandlerStub((request, _) =>
            {
                request.Method.Should().Be(HttpMethod.Get);
                request.RequestUri!.AbsoluteUri.Should().Contain($"GetSpecificMenuItem/{menuItem.MenuItemID}");

                var json = JsonConvert.SerializeObject(menuItem);
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };
            });

            var controller = new MenuItemController(new HttpClient(handler));

            var result = await controller.Edit(menuItem.MenuItemID);

            var view = result.Should().BeOfType<ViewResult>().Subject;
            var model = view.Model.Should().BeOfType<MenuItem>().Subject;
            model.MenuItemID.Should().Be(menuItem.MenuItemID);
        }

        [Fact]
        public async Task Edit_Post_InvalidModel_ReturnsViewWithModel()
        {
            var controller = new MenuItemController(new HttpClient(new DelegatingHandlerStub((_, __) => new HttpResponseMessage(HttpStatusCode.OK))));
            controller.ModelState.AddModelError("Name", "Required");

            var menuItem = new MenuItem { MenuItemID = 3, Name = string.Empty };

            var result = await controller.Edit(menuItem);

            var view = result.Should().BeOfType<ViewResult>().Subject;
            view.Model.Should().BeSameAs(menuItem);
        }

        [Fact]
        public async Task Edit_Post_ValidModel_RedirectsToIndexAndPuts()
        {
            var handler = new DelegatingHandlerStub((request, _) =>
            {
                request.Method.Should().Be(HttpMethod.Put);
                request.RequestUri!.AbsoluteUri.Should().Contain($"UpdateMenuItem/4");
                return new HttpResponseMessage(HttpStatusCode.OK);
            });

            var controller = new MenuItemController(new HttpClient(handler));

            var menuItem = new MenuItem { MenuItemID = 4, Name = "Updated" };

            var result = await controller.Edit(menuItem);

            var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
        }

        [Fact]
        public async Task Delete_Post_RedirectsToIndexAndDeletes()
        {
            var handler = new DelegatingHandlerStub((request, _) =>
            {
                request.Method.Should().Be(HttpMethod.Delete);
                request.RequestUri!.AbsoluteUri.Should().Contain("DeleteMenuItem/8");
                return new HttpResponseMessage(HttpStatusCode.OK);
            });

            var controller = new MenuItemController(new HttpClient(handler));

            var result = await controller.Delete(8);

            var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
        }
    }
}
