using Xunit;
using FluentAssertions;
using CrmArcheonzero.ViewModels;

namespace CrmArcheonzero.Tests.ViewModels
{
    public class MainViewModelTests
    {
        [Fact]
        public void MainViewModel_ShouldInitialize()
        {
            var vm = new MainViewModel();
            vm.Should().NotBeNull();
            vm.IsAuthenticated.Should().BeFalse();
        }

        [Fact]
        public void Statuses_ShouldContainDefaultValues()
        {
            var vm = new MainViewModel();
            vm.Statuses.Should().Contain(new[] { "Все", "Active", "Inactive", "Lead" });
        }

        [Fact]
        public void InteractionTypes_ShouldContainDefaultValues()
        {
            var vm = new MainViewModel();
            vm.InteractionTypes.Should().Contain(new[] { "Call", "Email", "Meeting", "Note" });
        }
    }
}