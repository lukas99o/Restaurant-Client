using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using ResturangFrontEnd.Controllers;

namespace Restaurant_Frontend_Tests.Controllers
{
    public class AdminControllerTests
    {
        [Fact]
        public void Index_ReturnsViewWithTitle()
        {
            var controller = new AdminController();

            var result = controller.Index();

            var view = result.Should().BeOfType<ViewResult>().Subject;
            controller.ViewData["Title"].Should().Be("Admin Panel");
            view.Model.Should().BeNull();
        }
    }
}
