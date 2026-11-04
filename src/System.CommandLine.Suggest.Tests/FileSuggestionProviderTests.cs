using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace System.CommandLine.Suggest.Tests
{
    public class FileSuggestionProviderTests : SuggestionRegistrationTest, IDisposable
    {
        protected override ISuggestionRegistration GetSuggestionRegistration() => new FileSuggestionRegistration(_filePath);

        private readonly string _filePath;

        public FileSuggestionProviderTests()
        {
            _filePath = Path.GetFullPath(Path.GetRandomFileName());
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Repeated_registrations_are_not_duplicated(bool useRelativePath)
        {
            var provider = GetSuggestionRegistration();
            var fullPath = Path.GetFullPath("test-command");
            var path = useRelativePath ? "test-command" : fullPath;

            provider.AddSuggestionRegistration(new Registration(path));
            provider.AddSuggestionRegistration(new Registration(path));
            provider.AddSuggestionRegistration(new Registration(fullPath));

            provider.FindAllRegistrations().Should().ContainSingle()
                    .Which.ExecutablePath.Should().Be(fullPath);
        }

        [Fact]
        public void A_relative_registration_can_be_found()
        {
            var provider = GetSuggestionRegistration();
            provider.AddSuggestionRegistration(new Registration("test-command"));

            var registration = provider.FindRegistration(new FileInfo("test-command"));

            registration.Should().NotBeNull();
            registration.ExecutablePath.Should().Be(Path.GetFullPath("test-command"));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void An_exact_match_is_found_regardless_of_registration_order(bool exactMatchFirst)
        {
            var path = Path.GetFullPath("test-command");
            var otherPath = path + "-other";
            File.WriteAllLines(_filePath, exactMatchFirst ? new[] { path, otherPath } : new[] { otherPath, path });

            var registration = GetSuggestionRegistration().FindRegistration(new FileInfo(path));

            registration.Should().NotBeNull();
            registration.ExecutablePath.Should().Be(path);
        }

        [Fact]
        public void A_path_prefix_is_not_a_registration()
        {
            var path = Path.GetFullPath("test-command");
            File.WriteAllText(_filePath, path + "-other");

            var registration = GetSuggestionRegistration().FindRegistration(new FileInfo(path));

            registration.Should().BeNull();
        }

        [Fact]
        public void Matching_respects_platform_path_case_sensitivity()
        {
            var provider = GetSuggestionRegistration();
            var path = Path.GetFullPath("test-command");
            provider.AddSuggestionRegistration(new Registration(path));

            var registration = provider.FindRegistration(new FileInfo(Path.GetFullPath("TEST-COMMAND")));

            if (OperatingSystem.IsWindows())
            {
                registration.Should().NotBeNull();
                registration.ExecutablePath.Should().Be(path);
            }
            else
            {
                registration.Should().BeNull();
            }
        }

        [Fact]
        public void Duplicate_detection_respects_platform_path_case_sensitivity()
        {
            var provider = GetSuggestionRegistration();
            var path = Path.GetFullPath("test-command");
            var otherPath = Path.GetFullPath("TEST-COMMAND");
            provider.AddSuggestionRegistration(new Registration(path));
            provider.AddSuggestionRegistration(new Registration(otherPath));

            provider.FindAllRegistrations().Should().HaveCount(OperatingSystem.IsWindows() ? 1 : 2);
            provider.FindRegistration(new FileInfo(path)).ExecutablePath.Should().Be(path);
        }

        [Fact]
        public async Task Register_command_does_not_duplicate_relative_paths()
        {
            var provider = GetSuggestionRegistration();
            var dispatcher = new SuggestionDispatcher(provider);
            dispatcher.Configuration.Output = new StringWriter();
            var args = new[] { "register", "--command-path", "test-command" };

            (await dispatcher.InvokeAsync(args)).Should().Be(0);
            (await dispatcher.InvokeAsync(args)).Should().Be(0);

            provider.FindAllRegistrations().Should().ContainSingle()
                    .Which.ExecutablePath.Should().Be(Path.GetFullPath("test-command"));
        }

        public void Dispose()
        {
            if (File.Exists(_filePath))
            {
                File.Delete(_filePath);
            }
        }
    }
}
