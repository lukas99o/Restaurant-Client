using System.Net;
using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ResturangFrontEnd.Controllers;
using ResturangFrontEnd.Models;
using Restaurant_Frontend_Tests.Helpers;

namespace Restaurant_Frontend_Tests.Controllers
{
    public class BookingControllerTests
    {
        [Fact]
        public async Task Index_ReturnsViewWithBookingsFromApi()
        {
            var booking = new Booking
            {
                BookingID = 1,
                TableID = 2,
                Time = DateTime.UtcNow,
                TimeEnd = DateTime.UtcNow.AddHours(2),
                Name = "Guest",
                Email = "guest@example.com",
                Phone = "123456"
            };

            var handler = new DelegatingHandlerStub((request, _) =>
            {
                request.Method.Should().Be(HttpMethod.Get);
                request.RequestUri!.AbsoluteUri.Should().Contain("api/Bookings");

                var json = JsonConvert.SerializeObject(new List<Booking> { booking });
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };
            });

            var controller = new BookingController(new HttpClient(handler));

            var result = await controller.Index();

            var view = result.Should().BeOfType<ViewResult>().Subject;
            var model = view.Model.Should().BeAssignableTo<List<Booking>>().Subject;
            model.Should().ContainSingle().Which.BookingID.Should().Be(booking.BookingID);
        }

        [Fact]
        public void Create_Get_ReturnsView()
        {
            var controller = new BookingController(new HttpClient(new DelegatingHandlerStub((_, __) => new HttpResponseMessage(HttpStatusCode.OK))));

            var result = controller.Create();

            result.Should().BeOfType<ViewResult>();
        }

        [Fact]
        public async Task Create_Post_InvalidModel_ReturnsViewWithModel()
        {
            var controller = new BookingController(new HttpClient(new DelegatingHandlerStub((_, __) => new HttpResponseMessage(HttpStatusCode.OK))));
            controller.ModelState.AddModelError("Name", "Required");

            var booking = new Booking { Name = string.Empty };

            var result = await controller.Create(booking);

            var view = result.Should().BeOfType<ViewResult>().Subject;
            view.Model.Should().BeSameAs(booking);
        }

        [Fact]
        public async Task Create_Post_ValidModel_RedirectsToIndexAndPosts()
        {
            var handler = new DelegatingHandlerStub((request, _) =>
            {
                request.Method.Should().Be(HttpMethod.Post);
                request.RequestUri!.AbsoluteUri.Should().Contain("CreateBooking");
                return new HttpResponseMessage(HttpStatusCode.Created);
            });

            var controller = new BookingController(new HttpClient(handler));

            var booking = new Booking
            {
                BookingID = 1,
                TableID = 2,
                Time = DateTime.UtcNow,
                TimeEnd = DateTime.UtcNow.AddHours(1),
                Name = "Guest",
                Email = "guest@example.com",
                Phone = "123456"
            };

            var result = await controller.Create(booking);

            var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
        }

        [Fact]
        public async Task Edit_Get_ReturnsBookingFromApi()
        {
            var booking = new Booking
            {
                BookingID = 5,
                Name = "Guest",
                TableID = 1
            };

            var handler = new DelegatingHandlerStub((request, _) =>
            {
                request.Method.Should().Be(HttpMethod.Get);
                request.RequestUri!.AbsoluteUri.Should().Contain($"GetSpecificBooking/{booking.BookingID}");

                var json = JsonConvert.SerializeObject(booking);
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };
            });

            var controller = new BookingController(new HttpClient(handler));

            var result = await controller.Edit(booking.BookingID);

            var view = result.Should().BeOfType<ViewResult>().Subject;
            var model = view.Model.Should().BeOfType<Booking>().Subject;
            model.BookingID.Should().Be(booking.BookingID);
        }

        [Fact]
        public async Task Edit_Post_InvalidModel_ReturnsViewWithModel()
        {
            var controller = new BookingController(new HttpClient(new DelegatingHandlerStub((_, __) => new HttpResponseMessage(HttpStatusCode.OK))));
            controller.ModelState.AddModelError("Name", "Required");

            var booking = new Booking { BookingID = 3, Name = string.Empty };

            var result = await controller.Edit(booking);

            var view = result.Should().BeOfType<ViewResult>().Subject;
            view.Model.Should().BeSameAs(booking);
        }

        [Fact]
        public async Task Edit_Post_ValidModel_RedirectsToIndexAndPuts()
        {
            var handler = new DelegatingHandlerStub((request, _) =>
            {
                request.Method.Should().Be(HttpMethod.Put);
                request.RequestUri!.AbsoluteUri.Should().Contain($"UpdateBooking/7");
                return new HttpResponseMessage(HttpStatusCode.OK);
            });

            var controller = new BookingController(new HttpClient(handler));

            var booking = new Booking { BookingID = 7, Name = "Updated" };

            var result = await controller.Edit(booking);

            var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
        }

        [Fact]
        public async Task Delete_Post_RedirectsToIndexAndDeletes()
        {
            var handler = new DelegatingHandlerStub((request, _) =>
            {
                request.Method.Should().Be(HttpMethod.Delete);
                request.RequestUri!.AbsoluteUri.Should().Contain("DeleteBooking/9");
                return new HttpResponseMessage(HttpStatusCode.OK);
            });

            var controller = new BookingController(new HttpClient(handler));

            var result = await controller.Delete(9);

            var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Index");
        }
    }
}
