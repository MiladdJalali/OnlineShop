﻿using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Project.RestApi.V1.Aggregates.Users.Models;
using Xunit;

namespace Project.RestApi.IntegrationTest.V1.Aggregates.Users.Controllers
{
    [Collection(nameof(TestFixtureCollection))]
    public class UsersControllerIntegrationTest : BaseControllerIntegrationTest
    {
        public UsersControllerIntegrationTest(TestFixture testFixture)
            : base(testFixture)
        {
        }

        protected override string BaseUrl { get; } = "/rest/api/v1/Users";

        [Fact]
        public async Task TestAll_WhenEverythingIsOk_StatusMustBeCorrect()
        {
            // Create
            var createRequest = new UserRequest
            {
                Username = nameof(UserRequest.Username),
                Address = nameof(UserRequest.Address),
                Password = nameof(UserRequest.Password),
                ConfirmPassword = nameof(UserRequest.Password),
                Description = nameof(UserRequest.Description)
            };

            var createResponse = await Create<UserResponse>(createRequest);

            createResponse.Username.Should().Be(createRequest.Username);
            createResponse.Address.Should().Be(createRequest.Address);
            createResponse.Description.Should().Be(createRequest.Description);

            // Update
            var updateRequest = new UserRequest
            {
                Username = $"{nameof(UserRequest.Username)}Updated",
                Address = $"{nameof(UserRequest.Address)}Updated",
                Password = $"{nameof(UserRequest.Password)}Updated",
                ConfirmPassword = $"{nameof(UserRequest.Password)}Updated",
                Description = $"{nameof(UserRequest.Description)}Updated"
            };

            await Update(createRequest.Username, updateRequest);

            // GetAll
            var getAllParameters = new
            {
                PageSize = 1,
                PageIndex = 1
            };
            var getAllResponse = await GetAll<UserResponse>(getAllParameters);

            getAllResponse.Values.Should().HaveCount(1);
            getAllResponse.TotalCount.Should().Be(1);

            // GetByUsername
            var getByUsernameResponse = await GetByIdentifier<UserResponse>(updateRequest.Username);

            getByUsernameResponse.Username.Should().Be(updateRequest.Username);
            getByUsernameResponse.Address.Should().Be(updateRequest.Address);
            getByUsernameResponse.Description.Should().Be(updateRequest.Description);

            // Delete
            await Delete(updateRequest.Username);
        }

        [Fact]
        public async Task TestCreate_WhenPasswordsDoNotMatch_StatusMustBeBadRequest()
        {
            // Create
            var createRequest = new UserRequest
            {
                Username = nameof(UserRequest.Username),
                Address = nameof(UserRequest.Address),
                Password = nameof(UserRequest.Password),
                ConfirmPassword = nameof(UserRequest.ConfirmPassword)
            };

            await Create<UserResponse>(createRequest, HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task TestGetByUsername_WhenDoesNotExist_StatusMustBeNotFound()
        {
            await GetByIdentifier<UserResponse>("FakeUsername", HttpStatusCode.NotFound);
        }
    }
}