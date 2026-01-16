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
    public class BookTableControllerTests
    {
        [Fact]
        public async Task Book_Get_ReturnsViewWithBookingFromApi()
        {
            var table = new Table
            {
                TableID = 5,
                TableSeats = 4,
                IsAvailable = true
            };

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

            var controller = new BookTableController(new HttpClient(handler));

            var result = await controller.Book(5);

            var view = result.Should().BeOfType<ViewResult>().Subject;
            var model = view.Model.Should().BeOfType<Booking>().Subject;
            model.TableID.Should().Be(table.TableID);
            model.MaxSeats.Should().Be(table.TableSeats);
        }

        [Fact]
        public async Task Book_Post_InvalidModel_ReturnsViewWithSameModel()
        {
            var controller = new BookTableController(new HttpClient(new DelegatingHandlerStub((_, __) => new HttpResponseMessage(HttpStatusCode.OK))));
            controller.ModelState.AddModelError("Name", "Required");

            var booking = new Booking
            {
                TableID = 1,
                MaxSeats = 2,
                AmountOfPeople = 2,
                Time = DateTime.UtcNow,
                TimeEnd = DateTime.UtcNow.AddHours(2),
                Name = "",
                Email = "",
                Phone = "",
                CustomerID = 1
            };

            var result = await controller.Book(booking);

            var view = result.Should().BeOfType<ViewResult>().Subject;
            view.Model.Should().BeSameAs(booking);
        }

        [Fact]
        public async Task Book_Post_ValidModel_RedirectsToIndex()
        {
            var handler = new DelegatingHandlerStub((request, _) =>
            {
                request.Method.Should().Be(HttpMethod.Post);
                request.RequestUri!.AbsoluteUri.Should().Contain("CreateBooking");
                return new HttpResponseMessage(HttpStatusCode.Created);
            });

            var controller = new BookTableController(new HttpClient(handler));

            var booking = new Booking
            {
                TableID = 2,
                MaxSeats = 4,
                AmountOfPeople = 3,
                Time = DateTime.UtcNow,
                TimeEnd = DateTime.UtcNow.AddHours(2),
                Name = "Guest",
                Email = "guest@example.com",
                Phone = "123456",
                CustomerID = 10
            };

            var result = await controller.Book(booking);

            var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
        }
    }
}
