﻿using Discord;
using GrillBot.Data.Extensions.Discord;
using GrillBot.Tests.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GrillBot.Tests.Data.Extensions.Discord
{
    [TestClass]
    public class UserExtensionTests
    {
        [TestMethod]
        public void HaveAnimatedAvatar_True()
        {
            var mock = DiscordHelpers.CreateUserMock(0, null);
            mock.Setup(o => o.AvatarId).Returns("a_asdf");
            var user = mock.Object;

            Assert.IsTrue(user.HaveAnimatedAvatar());
        }

        [TestMethod]
        public void HaveAnimatedAvatar_False()
        {
            var mock = DiscordHelpers.CreateUserMock(0, null);
            mock.Setup(o => o.AvatarId).Returns("asdf");
            var user = mock.Object;

            Assert.IsFalse(user.HaveAnimatedAvatar());
        }

        [TestMethod]
        public void IsUser_Webhook_False()
        {
            var mock = DiscordHelpers.CreateUserMock(0, null);

            mock.Setup(o => o.IsBot).Returns(false);
            mock.Setup(o => o.IsWebhook).Returns(true);

            Assert.IsFalse(mock.Object.IsUser());
        }

        [TestMethod]
        public void IsUser_Bot_False()
        {
            var mock = DiscordHelpers.CreateUserMock(0, null);

            mock.Setup(o => o.IsBot).Returns(true);
            mock.Setup(o => o.IsWebhook).Returns(false);

            Assert.IsFalse(mock.Object.IsUser());
        }

        [TestMethod]
        public void IsUser_User_True()
        {
            var mock = DiscordHelpers.CreateUserMock(0, null);

            mock.Setup(o => o.IsBot).Returns(false);
            mock.Setup(o => o.IsWebhook).Returns(false);

            Assert.IsTrue(mock.Object.IsUser());
        }

        [TestMethod]
        public void GetAvatarUri_Default()
        {
            var mock = DiscordHelpers.CreateUserMock(0, null);

            mock.Setup(o => o.GetAvatarUrl(It.IsAny<ImageFormat>(), It.IsAny<ushort>())).Returns((string)null);
            mock.Setup(o => o.GetDefaultAvatarUrl()).Returns("https://discord.com");

            var result = mock.Object.GetAvatarUri();
            Assert.AreEqual("https://discord.com", result);
        }

        [TestMethod]
        public void GetAvatarUri_User()
        {
            var mock = DiscordHelpers.CreateUserMock(0, null);

            mock.Setup(o => o.GetAvatarUrl(It.IsAny<ImageFormat>(), It.IsAny<ushort>())).Returns("https://discord.com/user.jpg");
            mock.Setup(o => o.GetDefaultAvatarUrl()).Returns("https://discord.com");

            var result = mock.Object.GetAvatarUri();
            Assert.AreEqual("https://discord.com/user.jpg", result);
        }

        [TestMethod]
        public void DownloadAvatar()
        {
            var mock = DiscordHelpers.CreateUserMock(0, null);
            mock.Setup(o => o.GetAvatarUrl(It.IsAny<ImageFormat>(), It.IsAny<ushort>())).Returns("https://www.google.cz/images/branding/googlelogo/2x/googlelogo_color_272x92dp.png");

            mock.Object.DownloadAvatarAsync().ContinueWith(data =>
            {
                Assert.IsNotNull(data);
                Assert.IsNotNull(data.Result);
                Assert.IsTrue(data.Result.Length > 0);
            });
        }

        [TestMethod]
        public void GetDisplayName_GuildUser_WithoutNick()
        {
            var mock = DiscordHelpers.CreateGuildUserMock(0, "Test");
            mock.Setup(o => o.Discriminator).Returns("1234");

            var result = mock.Object.GetDisplayName();
            Assert.AreEqual("Test#1234", result);
        }

        [TestMethod]
        public void GetDisplayName_GuildUser_WithNick()
        {
            var mock = DiscordHelpers.CreateGuildUserMock(0, "Test", "Testik");
            mock.Setup(o => o.Discriminator).Returns("1234");

            var result = mock.Object.GetDisplayName();
            Assert.AreEqual("Testik", result);
        }

        [TestMethod]
        public void GetDisplayName_BasicUser()
        {
            var mock = DiscordHelpers.CreateUserMock(0, "Test");
            mock.Setup(o => o.Discriminator).Returns("1234");

            var result = mock.Object.GetDisplayName();
            Assert.AreEqual("Test#1234", result);
        }

        [TestMethod]
        public void GetDisplayName_BasicUser_WithoutDiscriminator()
        {
            var mock = DiscordHelpers.CreateUserMock(0, "Test");
            mock.Setup(o => o.Discriminator).Returns("1234");

            var result = mock.Object.GetDisplayName(false);
            Assert.AreEqual("Test", result);
        }

        [TestMethod]
        public void GetFullName_GuildUser_WithoutNick()
        {
            var mock = DiscordHelpers.CreateGuildUserMock(0, "Test");
            mock.Setup(o => o.Discriminator).Returns("1234");

            var result = mock.Object.GetFullName();
            Assert.AreEqual("Test#1234", result);
        }

        [TestMethod]
        public void GetFullName_GuildUser_WithNick()
        {
            var mock = DiscordHelpers.CreateGuildUserMock(0, "Test", "Testik");
            mock.Setup(o => o.Discriminator).Returns("1234");

            var result = mock.Object.GetFullName();
            Assert.AreEqual("Testik (Test#1234)", result);
        }

        [TestMethod]
        public void GetFullName_BasicUser()
        {
            var mock = DiscordHelpers.CreateUserMock(0, "Test");
            mock.Setup(o => o.Discriminator).Returns("1234");

            var result = mock.Object.GetFullName();
            Assert.AreEqual("Test#1234", result);
        }

        [TestMethod]
        public void CreateProfilePicFilename_DefaultAvatar()
        {
            var user = DiscordHelpers.CreateUserMock(12345, null);
            user.Setup(o => o.AvatarId).Returns((string)null);
            user.Setup(o => o.Discriminator).Returns("1234");

            const string expected = "12345_1234_128.png";

            var result = user.Object.CreateProfilePicFilename(128);
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void CreateProfilePicFilename_PngAvatar()
        {
            var user = DiscordHelpers.CreateUserMock(12345, null);
            user.Setup(o => o.AvatarId).Returns("abcd");
            user.Setup(o => o.Discriminator).Returns("1234");

            const string expected = "12345_abcd_128.png";

            var result = user.Object.CreateProfilePicFilename(128);
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void CreateProfilePicFilename_GifAvatar()
        {
            var user = DiscordHelpers.CreateUserMock(12345, null);
            user.Setup(o => o.AvatarId).Returns("a_abcd");
            user.Setup(o => o.Discriminator).Returns("1234");

            const string expected = "12345_a_abcd_128.gif";

            var result = user.Object.CreateProfilePicFilename(128);
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void TryAddRole_HaveRole()
        {
            var role = new Mock<IRole>();
            role.Setup(o => o.Id).Returns(12345);

            var user = DiscordHelpers.CreateGuildUserMock(0, null, null);
            user.Setup(o => o.RoleIds).Returns(new List<ulong>() { 12345 });
            user.Setup(o => o.AddRoleAsync(It.IsAny<IRole>(), It.IsAny<RequestOptions>())).Returns(Task.CompletedTask).Verifiable();

            user.Object.TryAddRoleAsync(role.Object).ContinueWith(_ => user.Verify(o => o.AddRoleAsync(It.IsAny<IRole>(), It.IsAny<RequestOptions>()), Times.Never()));
        }

        [TestMethod]
        public void TryAddRole_HaventRole()
        {
            var role = new Mock<IRole>();
            role.Setup(o => o.Id).Returns(12345);

            var user = DiscordHelpers.CreateGuildUserMock(0, null, null);
            user.Setup(o => o.RoleIds).Returns(new List<ulong>());
            user.Setup(o => o.AddRoleAsync(It.IsAny<IRole>(), It.IsAny<RequestOptions>())).Returns(Task.CompletedTask).Verifiable();

            user.Object.TryAddRoleAsync(role.Object).ContinueWith(_ => user.Verify(o => o.AddRoleAsync(It.IsAny<IRole>(), It.IsAny<RequestOptions>()), Times.Once()));
        }

        [TestMethod]
        public void TryRemoveRole_HaveRole()
        {
            var role = new Mock<IRole>();
            role.Setup(o => o.Id).Returns(12345);

            var user = DiscordHelpers.CreateGuildUserMock(0, null, null);
            user.Setup(o => o.RoleIds).Returns(new List<ulong>() { 12345 });
            user.Setup(o => o.RemoveRoleAsync(It.IsAny<IRole>(), It.IsAny<RequestOptions>())).Returns(Task.CompletedTask).Verifiable();

            user.Object.TryRemoveRoleAsync(role.Object).ContinueWith(_ => user.Verify(o => o.RemoveRoleAsync(It.IsAny<IRole>(), It.IsAny<RequestOptions>()), Times.Once()));
        }

        [TestMethod]
        public void TryRemoveRole_HaventRole()
        {
            var role = new Mock<IRole>();
            role.Setup(o => o.Id).Returns(12345);

            var user = DiscordHelpers.CreateGuildUserMock(0, null, null);
            user.Setup(o => o.RoleIds).Returns(new List<ulong>());
            user.Setup(o => o.RemoveRoleAsync(It.IsAny<IRole>(), It.IsAny<RequestOptions>())).Returns(Task.CompletedTask).Verifiable();

            user.Object.TryRemoveRoleAsync(role.Object).ContinueWith(_ => user.Verify(o => o.RemoveRoleAsync(It.IsAny<IRole>(), It.IsAny<RequestOptions>()), Times.Never()));
        }
    }
}