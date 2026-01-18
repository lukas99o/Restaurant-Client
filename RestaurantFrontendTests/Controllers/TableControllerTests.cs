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
    public class TableControllerTests
    {
        [Fact]
        public async Task Index_Get_ReturnsViewWithTablesFromApi()
        {
            var tables = new List<Table>
            {
                new() { TableID = 1, TableSeats = 4, IsAvailable = true },
                new() { TableID = 2, TableSeats = 2, IsAvailable = false }
            };

            var handler = new DelegatingHandlerStub((request, _) =>
            {
                request.Method.Should().Be(HttpMethod.Get);
                request.RequestUri!.AbsoluteUri.Should().Contain("api/Tables");

                var json = JsonConvert.SerializeObject(tables);
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };
            });

            var controller = new TableController(new HttpClient(handler));

            var result = await controller.Index();

            var view = result.Should().BeOfType<ViewResult>().Subject;
            var model = view.Model.Should().BeOfType<List<Table>>().Subject;
            model.Should().HaveCount(2);
        }

        [Fact]
        public async Task Create_Post_InvalidModel_ReturnsViewWithSameModel()
        {
            var controller = new TableController(
                new HttpClient(new DelegatingHandlerStub((_, __) => new HttpResponseMessage(HttpStatusCode.OK))));

            controller.ModelState.AddModelError("TableSeats", "Required");

            var table = new Table { TableSeats = 0, IsAvailable = true };

            var result = await controller.Create(table);

            var view = result.Should().BeOfType<ViewResult>().Subject;
            view.Model.Should().BeSameAs(table);
        }

        [Fact]
        public async Task Create_Post_ValidModel_RedirectsToIndex()
        {
            var handler = new DelegatingHandlerStub((request, _) =>
            {
                request.Method.Should().Be(HttpMethod.Post);
                request.RequestUri!.AbsoluteUri.Should().Contain("api/Tables/CreateTable");
                return new HttpResponseMessage(HttpStatusCode.Created);
            });

            var controller = new TableController(new HttpClient(handler));

            var table = new Table { TableSeats = 4, IsAvailable = true };

            var result = await controller.Create(table);

            var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
        }

        [Fact]
        public async Task Edit_Get_ReturnsViewWithTableFromApi()
        {
            var table = new Table { TableID = 5, TableSeats = 4, IsAvailable = true };

            var handler = new DelegatingHandlerStub((request, _) =>
            {
                request.Method.Should().Be(HttpMethod.Get);
                request.RequestUri!.AbsoluteUri.Should().Contain("GetSpecificTable/5");

                var json = JsonConvert.SerializeObject(table);
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };
            });

            var controller = new TableController(new HttpClient(handler));

            var result = await controller.Edit(5);

            var view = result.Should().BeOfType<ViewResult>().Subject;
            var model = view.Model.Should().BeOfType<Table>().Subject;
            model.TableID.Should().Be(5);
        }

        [Fact]
        public async Task Edit_Post_InvalidModel_ReturnsViewWithSameModel()
        {
            var controller = new TableController(
                new HttpClient(new DelegatingHandlerStub((_, __) => new HttpResponseMessage(HttpStatusCode.OK))));

            controller.ModelState.AddModelError("TableSeats", "Required");

            var table = new Table { TableID = 1, TableSeats = 0, IsAvailable = true };

            var result = await controller.Edit(table);

            var view = result.Should().BeOfType<ViewResult>().Subject;
            view.Model.Should().BeSameAs(table);
        }

        [Fact]
        public async Task Edit_Post_ValidModel_RedirectsToIndex()
        {
            var handler = new DelegatingHandlerStub((request, _) =>
            {
                request.Method.Should().Be(HttpMethod.Put);
                request.RequestUri!.AbsoluteUri.Should().Contain("api/Tables/UpdateTable/7");
                return new HttpResponseMessage(HttpStatusCode.NoContent);
            });

            var controller = new TableController(new HttpClient(handler));

            var table = new Table { TableID = 7, TableSeats = 4, IsAvailable = true };

            var result = await controller.Edit(table);

            var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
        }

        [Fact]
        public async Task Delete_Post_RedirectsToIndex()
        {
            var handler = new DelegatingHandlerStub((request, _) =>
            {
                request.Method.Should().Be(HttpMethod.Delete);
                request.RequestUri!.AbsoluteUri.Should().Contain("api/Tables/DeleteTable/3");
                return new HttpResponseMessage(HttpStatusCode.NoContent);
            });

            var controller = new TableController(new HttpClient(handler));

            var result = await controller.Delete(3);

            var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
        }
    }
}
