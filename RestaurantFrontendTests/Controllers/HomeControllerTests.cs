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

        #region AvailableTables Tests

        [Fact]
        public async Task AvailableTables_Get_ReturnsViewWithViewModel()
        {
            var tables = new List<TableGetDTO>
            {
                new() { TableID = 1, TableSeats = 4, IsAvailable = true },
                new() { TableID = 2, TableSeats = 2, IsAvailable = true }
            };

            var handler = new DelegatingHandlerStub((request, _) =>
            {
                request.Method.Should().Be(HttpMethod.Get);
                request.RequestUri!.AbsoluteUri.Should().Contain("api/Tables/AvailableTables");

                var json = JsonConvert.SerializeObject(tables);
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };
            });

            var controller = new HomeController(NullLogger<HomeController>.Instance, new HttpClient(handler));

            // Use a future date to ensure hours are available
            var futureDate = DateTime.Today.AddDays(1);
            var result = await controller.AvailableTables(futureDate, 2, 10);

            var view = result.Should().BeOfType<ViewResult>().Subject;
            var model = view.Model.Should().BeOfType<AvailableTablesViewModel>().Subject;
            model.SelectedDate.Should().Be(futureDate.Date);
            model.SelectedSeats.Should().Be(2);
            model.SelectedHour.Should().Be(10);
            model.AvailableTables.Should().HaveCount(2);
        }

        [Fact]
        public async Task AvailableTables_Get_DefaultsToTodayAndTwoSeats()
        {
            var tables = new List<TableGetDTO>();

            var handler = new DelegatingHandlerStub((request, _) =>
            {
                var json = JsonConvert.SerializeObject(tables);
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };
            });

            var controller = new HomeController(NullLogger<HomeController>.Instance, new HttpClient(handler));

            var result = await controller.AvailableTables(null, 2, null);

            var view = result.Should().BeOfType<ViewResult>().Subject;
            var model = view.Model.Should().BeOfType<AvailableTablesViewModel>().Subject;
            model.SelectedDate.Should().Be(DateTime.Today);
            model.SelectedSeats.Should().Be(2);
        }

        [Fact]
        public async Task ConfirmAvailableTable_WithValidHour_ShowsBookingForm()
        {
            var tables = new List<TableGetDTO>
            {
                new() { TableID = 1, TableSeats = 4, IsAvailable = true }
            };

            var handler = new DelegatingHandlerStub((request, _) =>
            {
                var json = JsonConvert.SerializeObject(tables);
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };
            });

            var controller = new HomeController(NullLogger<HomeController>.Instance, new HttpClient(handler));
            SetupControllerContext(controller);

            var vm = new AvailableTablesViewModel
            {
                SelectedDate = DateTime.Today.AddDays(1),
                SelectedSeats = 2,
                SelectedHour = 10
            };

            var result = await controller.ConfirmAvailableTable(vm);

            var view = result.Should().BeOfType<ViewResult>().Subject;
            view.ViewName.Should().Be("AvailableTables");
            var model = view.Model.Should().BeOfType<AvailableTablesViewModel>().Subject;
            model.ShowBookingForm.Should().BeTrue();
            model.SelectedTableID.Should().Be(1);
        }

        [Fact]
        public async Task ConfirmAvailableTable_WithNoHour_DoesNotShowBookingForm()
        {
            var handler = new DelegatingHandlerStub((_, __) => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("[]", Encoding.UTF8, "application/json")
            });

            var controller = new HomeController(NullLogger<HomeController>.Instance, new HttpClient(handler));
            SetupControllerContext(controller);

            var vm = new AvailableTablesViewModel
            {
                SelectedDate = DateTime.Today.AddDays(1),
                SelectedSeats = 2,
                SelectedHour = null
            };

            var result = await controller.ConfirmAvailableTable(vm);

            var view = result.Should().BeOfType<ViewResult>().Subject;
            var model = view.Model.Should().BeOfType<AvailableTablesViewModel>().Subject;
            model.ShowBookingForm.Should().BeFalse();
        }

        [Fact]
        public async Task ConfirmAvailableTable_WithNoAvailableTables_DoesNotShowBookingForm()
        {
            var handler = new DelegatingHandlerStub((_, __) => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("[]", Encoding.UTF8, "application/json")
            });

            var controller = new HomeController(NullLogger<HomeController>.Instance, new HttpClient(handler));
            SetupControllerContext(controller);

            var vm = new AvailableTablesViewModel
            {
                SelectedDate = DateTime.Today.AddDays(1),
                SelectedSeats = 2,
                SelectedHour = 10
            };

            var result = await controller.ConfirmAvailableTable(vm);

            var view = result.Should().BeOfType<ViewResult>().Subject;
            var model = view.Model.Should().BeOfType<AvailableTablesViewModel>().Subject;
            model.ShowBookingForm.Should().BeFalse();
            model.SelectedTableID.Should().BeNull();
        }

        [Fact]
        public async Task BookAvailableTable_WithValidData_RedirectsToAvailableTables()
        {
            var tables = new List<TableGetDTO>
            {
                new() { TableID = 1, TableSeats = 4, IsAvailable = true }
            };

            var handler = new DelegatingHandlerStub((request, _) =>
            {
                if (request.Method == HttpMethod.Post)
                {
                    request.RequestUri!.AbsoluteUri.Should().Contain("api/bookings/CreateBooking");
                    return new HttpResponseMessage(HttpStatusCode.Created);
                }

                var json = JsonConvert.SerializeObject(tables);
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };
            });

            var controller = new HomeController(NullLogger<HomeController>.Instance, new HttpClient(handler));
            SetupControllerContext(controller);

            var vm = new AvailableTablesViewModel
            {
                SelectedDate = DateTime.Today.AddDays(1),
                SelectedSeats = 2,
                SelectedHour = 10,
                SelectedTableID = 1,
                Name = "John Doe",
                Phone = "123456789",
                Email = "john@example.com"
            };

            var result = await controller.BookAvailableTable(vm);

            var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("AvailableTables");
        }

        [Fact]
        public async Task BookAvailableTable_WithNoHour_ReturnsViewWithoutBookingForm()
        {
            var handler = new DelegatingHandlerStub((_, __) => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("[]", Encoding.UTF8, "application/json")
            });

            var controller = new HomeController(NullLogger<HomeController>.Instance, new HttpClient(handler));
            SetupControllerContext(controller);

            var vm = new AvailableTablesViewModel
            {
                SelectedDate = DateTime.Today.AddDays(1),
                SelectedSeats = 2,
                SelectedHour = null
            };

            var result = await controller.BookAvailableTable(vm);

            var view = result.Should().BeOfType<ViewResult>().Subject;
            var model = view.Model.Should().BeOfType<AvailableTablesViewModel>().Subject;
            model.ShowBookingForm.Should().BeFalse();
        }

        [Fact]
        public async Task BookAvailableTable_WithInvalidModel_ReturnsViewWithBookingForm()
        {
            var tables = new List<TableGetDTO>
            {
                new() { TableID = 1, TableSeats = 4, IsAvailable = true }
            };

            var handler = new DelegatingHandlerStub((request, _) =>
            {
                var json = JsonConvert.SerializeObject(tables);
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };
            });

            var controller = new HomeController(NullLogger<HomeController>.Instance, new HttpClient(handler));
            SetupControllerContext(controller);
            controller.ModelState.AddModelError("Name", "Required");

            var vm = new AvailableTablesViewModel
            {
                SelectedDate = DateTime.Today.AddDays(1),
                SelectedSeats = 2,
                SelectedHour = 10,
                SelectedTableID = 1
            };

            var result = await controller.BookAvailableTable(vm);

            var view = result.Should().BeOfType<ViewResult>().Subject;
            var model = view.Model.Should().BeOfType<AvailableTablesViewModel>().Subject;
            model.ShowBookingForm.Should().BeTrue();
        }

        [Fact]
        public async Task BookAvailableTable_WithNoTableSelected_AddsModelError()
        {
            var tables = new List<TableGetDTO>
            {
                new() { TableID = 1, TableSeats = 4, IsAvailable = true }
            };

            var handler = new DelegatingHandlerStub((request, _) =>
            {
                var json = JsonConvert.SerializeObject(tables);
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };
            });

            var controller = new HomeController(NullLogger<HomeController>.Instance, new HttpClient(handler));
            SetupControllerContext(controller);

            var vm = new AvailableTablesViewModel
            {
                SelectedDate = DateTime.Today.AddDays(1),
                SelectedSeats = 2,
                SelectedHour = 10,
                SelectedTableID = null,
                Name = "John",
                Phone = "123",
                Email = "a@b.com"
            };

            var result = await controller.BookAvailableTable(vm);

            var view = result.Should().BeOfType<ViewResult>().Subject;
            var model = view.Model.Should().BeOfType<AvailableTablesViewModel>().Subject;
            model.ShowBookingForm.Should().BeTrue();
            controller.ModelState.ContainsKey("SelectedTableID").Should().BeTrue();
        }

        [Fact]
        public async Task Index_SetsViewDataTitle()
        {
            var handler = new DelegatingHandlerStub((_, __) =>
            {
                var json = JsonConvert.SerializeObject(new List<MenuItem>());
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };
            });

            var controller = new HomeController(NullLogger<HomeController>.Instance, new HttpClient(handler));

            await controller.Index();

            controller.ViewData["Title"].Should().Be("Restaurant Kifo - Home");
        }

        [Fact]
        public async Task Menu_SetsViewDataTitle()
        {
            var handler = new DelegatingHandlerStub((_, __) =>
            {
                var json = JsonConvert.SerializeObject(new List<MenuItem>());
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };
            });

            var controller = new HomeController(NullLogger<HomeController>.Instance, new HttpClient(handler));

            await controller.Menu();

            controller.ViewData["Title"].Should().Be("Menu");
        }

        [Fact]
        public async Task AvailableTables_SetsViewDataTitle()
        {
            var handler = new DelegatingHandlerStub((_, __) =>
            {
                var json = JsonConvert.SerializeObject(new List<TableGetDTO>());
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };
            });

            var controller = new HomeController(NullLogger<HomeController>.Instance, new HttpClient(handler));

            await controller.AvailableTables(DateTime.Today.AddDays(1), 2, 10);

            controller.ViewData["Title"].Should().Be("Available Tables");
        }

        [Fact]
        public async Task AvailableTables_WithCustomSeats_UsesProvidedValue()
        {
            var handler = new DelegatingHandlerStub((_, __) =>
            {
                var json = JsonConvert.SerializeObject(new List<TableGetDTO>());
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };
            });

            var controller = new HomeController(NullLogger<HomeController>.Instance, new HttpClient(handler));

            var result = await controller.AvailableTables(DateTime.Today.AddDays(1), 8, 12);

            var view = result.Should().BeOfType<ViewResult>().Subject;
            var model = view.Model.Should().BeOfType<AvailableTablesViewModel>().Subject;
            model.SelectedSeats.Should().Be(8);
        }

        [Fact]
        public async Task AvailableTables_FutureDateWithHour_SetsTimeUtcValues()
        {
            var handler = new DelegatingHandlerStub((_, __) =>
            {
                var json = JsonConvert.SerializeObject(new List<TableGetDTO>());
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };
            });

            var controller = new HomeController(NullLogger<HomeController>.Instance, new HttpClient(handler));

            var futureDate = DateTime.Today.AddDays(1);
            var result = await controller.AvailableTables(futureDate, 2, 14);

            var view = result.Should().BeOfType<ViewResult>().Subject;
            var model = view.Model.Should().BeOfType<AvailableTablesViewModel>().Subject;
            model.SelectedTimeUtc.Should().NotBeNull();
            model.SelectedTimeEndUtc.Should().NotBeNull();
            model.SelectedTimeEndUtc.Should().BeAfter(model.SelectedTimeUtc!.Value);
        }

        [Fact]
        public async Task ConfirmAvailableTable_SelectsSmallestSuitableTable()
        {
            var tables = new List<TableGetDTO>
            {
                new() { TableID = 1, TableSeats = 8, IsAvailable = true },
                new() { TableID = 2, TableSeats = 4, IsAvailable = true },
                new() { TableID = 3, TableSeats = 2, IsAvailable = true }
            };

            var handler = new DelegatingHandlerStub((_, __) =>
            {
                var json = JsonConvert.SerializeObject(tables);
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };
            });

            var controller = new HomeController(NullLogger<HomeController>.Instance, new HttpClient(handler));
            SetupControllerContext(controller);

            var vm = new AvailableTablesViewModel
            {
                SelectedDate = DateTime.Today.AddDays(1),
                SelectedSeats = 4,
                SelectedHour = 10
            };

            var result = await controller.ConfirmAvailableTable(vm);

            var view = result.Should().BeOfType<ViewResult>().Subject;
            var model = view.Model.Should().BeOfType<AvailableTablesViewModel>().Subject;
            model.SelectedTableID.Should().Be(2);
        }

        [Fact]
        public async Task ConfirmAvailableTable_PreservesSelectedTableIfProvided()
        {
            var tables = new List<TableGetDTO>
            {
                new() { TableID = 1, TableSeats = 4, IsAvailable = true },
                new() { TableID = 2, TableSeats = 2, IsAvailable = true }
            };

            var handler = new DelegatingHandlerStub((_, __) =>
            {
                var json = JsonConvert.SerializeObject(tables);
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };
            });

            var controller = new HomeController(NullLogger<HomeController>.Instance, new HttpClient(handler));
            SetupControllerContext(controller);

            var vm = new AvailableTablesViewModel
            {
                SelectedDate = DateTime.Today.AddDays(1),
                SelectedSeats = 2,
                SelectedHour = 10,
                SelectedTableID = 1
            };

            var result = await controller.ConfirmAvailableTable(vm);

            var view = result.Should().BeOfType<ViewResult>().Subject;
            var model = view.Model.Should().BeOfType<AvailableTablesViewModel>().Subject;
            model.SelectedTableID.Should().Be(1);
        }

        [Fact]
        public async Task BookAvailableTable_SendsCorrectBookingData()
        {
            Booking? capturedBooking = null;

            var tables = new List<TableGetDTO>
            {
                new() { TableID = 5, TableSeats = 4, IsAvailable = true }
            };

            var handler = new DelegatingHandlerStub((request, _) =>
            {
                if (request.Method == HttpMethod.Post)
                {
                    var content = request.Content!.ReadAsStringAsync().Result;
                    capturedBooking = JsonConvert.DeserializeObject<Booking>(content);
                    return new HttpResponseMessage(HttpStatusCode.Created);
                }

                var json = JsonConvert.SerializeObject(tables);
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };
            });

            var controller = new HomeController(NullLogger<HomeController>.Instance, new HttpClient(handler));
            SetupControllerContext(controller);

            var vm = new AvailableTablesViewModel
            {
                SelectedDate = DateTime.Today.AddDays(1),
                SelectedSeats = 4,
                SelectedHour = 15,
                SelectedTableID = 5,
                Name = "Test User",
                Phone = "555-1234",
                Email = "test@example.com"
            };

            await controller.BookAvailableTable(vm);

            capturedBooking.Should().NotBeNull();
            capturedBooking!.TableID.Should().Be(5);
            capturedBooking.Name.Should().Be("Test User");
            capturedBooking.Phone.Should().Be("555-1234");
            capturedBooking.Email.Should().Be("test@example.com");
        }

        [Fact]
        public async Task BookAvailableTable_RedirectsWithCorrectRouteValues()
        {
            var tables = new List<TableGetDTO>
            {
                new() { TableID = 1, TableSeats = 4, IsAvailable = true }
            };

            var handler = new DelegatingHandlerStub((request, _) =>
            {
                if (request.Method == HttpMethod.Post)
                {
                    return new HttpResponseMessage(HttpStatusCode.Created);
                }

                var json = JsonConvert.SerializeObject(tables);
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };
            });

            var controller = new HomeController(NullLogger<HomeController>.Instance, new HttpClient(handler));
            SetupControllerContext(controller);

            var selectedDate = DateTime.Today.AddDays(3);
            var vm = new AvailableTablesViewModel
            {
                SelectedDate = selectedDate,
                SelectedSeats = 6,
                SelectedHour = 18,
                SelectedTableID = 1,
                Name = "Jane",
                Phone = "123",
                Email = "jane@test.com"
            };

            var result = await controller.BookAvailableTable(vm);

            var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("AvailableTables");
            redirect.RouteValues.Should().ContainKey("date");
            redirect.RouteValues.Should().ContainKey("seats");
            redirect.RouteValues.Should().ContainKey("hour");
            redirect.RouteValues!["seats"].Should().Be(6);
            redirect.RouteValues["hour"].Should().Be(18);
        }

        [Fact]
        public async Task AvailableTables_PopulatesAvailableHours()
        {
            var handler = new DelegatingHandlerStub((_, __) =>
            {
                var json = JsonConvert.SerializeObject(new List<TableGetDTO>());
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };
            });

            var controller = new HomeController(NullLogger<HomeController>.Instance, new HttpClient(handler));

            var futureDate = DateTime.Today.AddDays(1);
            var result = await controller.AvailableTables(futureDate, 2, null);

            var view = result.Should().BeOfType<ViewResult>().Subject;
            var model = view.Model.Should().BeOfType<AvailableTablesViewModel>().Subject;
            model.AvailableHours.Should().NotBeEmpty();
            model.AvailableHours.Should().OnlyContain(h => h >= 9 && h <= 21);
        }

        [Fact]
        public async Task AvailableTables_WhenNoHourProvided_SelectsFirstAvailableHour()
        {
            var handler = new DelegatingHandlerStub((_, __) =>
            {
                var json = JsonConvert.SerializeObject(new List<TableGetDTO>());
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };
            });

            var controller = new HomeController(NullLogger<HomeController>.Instance, new HttpClient(handler));

            var futureDate = DateTime.Today.AddDays(1);
            var result = await controller.AvailableTables(futureDate, 2, null);

            var view = result.Should().BeOfType<ViewResult>().Subject;
            var model = view.Model.Should().BeOfType<AvailableTablesViewModel>().Subject;
            model.SelectedHour.Should().Be(model.AvailableHours.First());
        }

        [Fact]
        public async Task ConfirmAvailableTable_ClearsModelState()
        {
            var tables = new List<TableGetDTO>
            {
                new() { TableID = 1, TableSeats = 4, IsAvailable = true }
            };

            var handler = new DelegatingHandlerStub((_, __) =>
            {
                var json = JsonConvert.SerializeObject(tables);
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };
            });

            var controller = new HomeController(NullLogger<HomeController>.Instance, new HttpClient(handler));
            SetupControllerContext(controller);
            controller.ModelState.AddModelError("Name", "Required");

            var vm = new AvailableTablesViewModel
            {
                SelectedDate = DateTime.Today.AddDays(1),
                SelectedSeats = 2,
                SelectedHour = 10
            };

            await controller.ConfirmAvailableTable(vm);

            controller.ModelState.IsValid.Should().BeTrue();
        }

        #endregion

        private static void SetupControllerContext(HomeController controller)
        {
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
        }
    }
}
