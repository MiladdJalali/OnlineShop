﻿using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Project.Application.Aggregates.Users.Commands.CreateUser;
using Project.Application.Properties;
using Project.Domain.Exceptions;
using Xunit;

namespace Project.Application.UnitTest.Aggregates.Users.Commands.CreateUser
{
    public class CreateUserCommandHandlerUnitTest
    {
        [Fact]
        public async Task TestHandle_WhenEverythingIsOk_UsernameMustBeReturned()
        {
            var command = CreateUserCommandBuilder.Build();
            var commandHandler = new CreateUserCommandHandlerBuilder().Build();

            var username = await commandHandler
                .Handle(command, CancellationToken.None);

            username.Should().Be(command.Username);
        }

        [Fact]
        public void TestHandle_WhenNameIsSystemEntity_ThrowsException()
        {
            var systemEntityDetectorMock = new Mock<ISystemEntityDetector>();
            var command = CreateUserCommandBuilder.Build();
            var commandHandler = new CreateUserCommandHandlerBuilder()
                .WithSystemEntityDetector(systemEntityDetectorMock.Object)
                .Build();
            var func = new Func<Task>(async () => await commandHandler
                .Handle(command, CancellationToken.None));

            systemEntityDetectorMock.Setup(i => i.IsSystemEntity(command.Username!)).Returns(true);

            func.Should().ThrowAsync<DomainException>()
                .WithMessage(ApplicationResources.User_UsernameCannotStartWithUnderscore);
        }

        [Fact]
        public void TestHandle_WhenPasswordAndConfirmPasswordDoesNotMatch_ThrowsException()
        {
            var command = new CreateUserCommand
            {
                Username = "UserUsername",
                Password = "UserPassword",
                ConfirmPassword = "AnotherPassword",
                Description = "UserDescription"
            };
            var commandHandler = new CreateUserCommandHandlerBuilder().Build();
            var func = new Func<Task>(async () => await commandHandler
                .Handle(command, CancellationToken.None));

            func.Should().ThrowAsync<DomainException>()
                .WithMessage(ApplicationResources.User_PasswordAndConfirmPasswordDoesNotMatch);
        }
    }
}