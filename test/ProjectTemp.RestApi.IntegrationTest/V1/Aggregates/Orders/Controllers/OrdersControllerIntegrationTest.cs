﻿using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Project.Domain.Aggregates.Orders.Enums;
using Project.RestApi.IntegrationTest.SeedHelpers;
using Project.RestApi.V1.Aggregates.Orders.Models;
using Xunit;

namespace Project.RestApi.IntegrationTest.V1.Aggregates.Orders.Controllers
{
    [Collection(nameof(TestFixtureCollection))]
    public partial class OrdersControllerIntegrationTest : BaseControllerIntegrationTest
    {
        private readonly HttpClient client;

        public OrdersControllerIntegrationTest(TestFixture testFixture)
            : base(testFixture)
        {
            client = testFixture.Client;
        }

        protected override string BaseUrl { get; } = "/rest/api/v1/Orders";

        [Fact]
        public async Task TestAll_WhenEverythingIsOk_StatusMustBeCorrect()
        {
            // Create
            var createRequest = new OrderRequest
            {
                Goods = new[]
                {
                    new OrderGoodRequest {Name = GoodDataSeeder.FirstGoodName, Count = 1},
                    new OrderGoodRequest {Name = GoodDataSeeder.SecondGoodName, Count = 2}
                },
                Description = nameof(OrderRequest.Description)
            };

            var createResponse = await Create<OrderResponse>(createRequest);

            createResponse.Status.Should().Be(OrderStatus.Received.ToString());
            createResponse.PostType.Should().Be(OrderPostType.SpecialPost.ToString());
            createResponse.Address.Should().BeNull();
            createResponse.TotalPrice.Should().Be(106000);
            createResponse.Description.Should().Be(createRequest.Description);

            // Update
            var updateRequest = new OrderRequest
            {
                Goods = new[]
                {
                    new OrderGoodRequest {Name = GoodDataSeeder.FirstGoodName, Count = 1}
                },
                Description = $"{nameof(OrderRequest.Description)}Updated"
            };

            await Update(createResponse.Id.ToString(), updateRequest);

            // GetAll
            var getAllParameters = new
            {
                PageSize = 1,
                PageIndex = 1
            };
            var getAllResponse = await GetAll<OrderResponse>(getAllParameters);

            getAllResponse.Values.Should().HaveCount(1);
            getAllResponse.Values.First().Status.Should().Be(OrderStatus.Received.ToString());
            getAllResponse.Values.First().PostType.Should().Be(OrderPostType.OrdinaryPost.ToString());
            getAllResponse.Values.First().Address.Should().BeNull();
            getAllResponse.Values.First().TotalPrice.Should().Be(90000);
            getAllResponse.Values.First().Description.Should().Be(updateRequest.Description);

            getAllResponse.TotalCount.Should().Be(1);

            // GetItems
            var getItemsResponse = await GetItems(createResponse.Id);

            getItemsResponse.TotalCount.Should().Be(1);
            getItemsResponse.Values.First().Name.Should().Be(GoodDataSeeder.FirstGoodName);
            getItemsResponse.Values.First().Price.Should().Be(GoodDataSeeder.FirstGoodPrice);
            getItemsResponse.Values.First().Discount.Should().Be(GoodDataSeeder.FirstGoodDiscount);
            getItemsResponse.Values.First().IsFragile.Should().Be(GoodDataSeeder.FirstGoodIsFragile);
            getItemsResponse.Values.First().Name.Should().Be(GoodDataSeeder.FirstGoodName);
            getItemsResponse.Values.First().Description.Should().BeNull();

            // ChangeStatusToPacked
            await ChangeStatus(createResponse.Id);

            var orderResponse = await GetByIdentifier<OrderResponse>(createResponse.Id.ToString());
            orderResponse.Status.Should().Be(OrderStatus.Packed.ToString());
            orderResponse.Address.Should().Be(UserDataSeeder.AdminAddress);

            // ChangeStatusToDelivered
            await ChangeStatus(createResponse.Id);

            orderResponse = await GetByIdentifier<OrderResponse>(createResponse.Id.ToString());
            orderResponse.Status.Should().Be(OrderStatus.Delivered.ToString());
            orderResponse.Address.Should().Be(UserDataSeeder.AdminAddress);

            await Delete(createResponse.Id.ToString());
        }

        [Fact]
        public async Task TestPost_WhenTotalPriceLessThanMinimum_StatusMustBeBadRequest()
        {
            var createRequest = new OrderRequest
            {
                Goods = new[]
                {
                    new OrderGoodRequest {Name = GoodDataSeeder.SecondGoodName, Count = 2}
                },
                Description = nameof(OrderRequest.Description)
            };

            await Create<OrderResponse>(createRequest, HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task TestGetOrderByName_WhenDoesNotExist_StatusMustBeNotFound()
        {
            await GetByIdentifier<OrderResponse>("FakeOrderName", HttpStatusCode.NotFound);
        }
    }
}