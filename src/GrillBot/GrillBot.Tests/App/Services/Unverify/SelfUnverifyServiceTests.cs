﻿using GrillBot.App.Services.Unverify;
using GrillBot.Database.Services;
using GrillBot.Tests.TestHelper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrillBot.Tests.App.Services.Unverify
{
    [TestClass]
    public class SelfUnverifyServiceTests
    {
        private ServiceProvider CreateService(out SelfunverifyService service)
        {
            service = null;
            var container = DIHelpers.CreateContainer();

            if (container.GetService<GrillBotContextFactory>() is not TestingGrillBotContextFactory dbFactory)
            {
                Assert.Fail("DbFactory není TestingGrillBotContextFactory.");
                return null;
            }

            service = new SelfunverifyService(null, dbFactory);
            return container;
        }

        [TestMethod]
        [ExpectedException(typeof(AggregateException))]
        public void AddKeepable_ValidationErrorAfterSuccess()
        {
            using var container = CreateService(out var service);

            const string group = "group";
            const string name = "name";

            service.AddKeepableAsync(group, name).Wait();
            service.AddKeepableAsync(group, name).Wait();
        }

        [TestMethod]
        [ExpectedException(typeof(AggregateException))]
        public void RemoveKeepable_Groups_ValidationError()
        {
            using var container = CreateService(out var service);

            string group = $"group{new Random().Next(int.MinValue, int.MaxValue)}";
            service.RemoveKeepableAsync(group).Wait();
        }

        [TestMethod]
        [ExpectedException(typeof(AggregateException))]
        public void RemoveKeepable_Item_ValidationError()
        {
            using var container = CreateService(out var service);

            string group = $"group{new Random().Next(int.MinValue, int.MaxValue)}";
            const string name = "name";

            service.RemoveKeepableAsync(group, name).Wait();
        }

        [TestMethod]
        public void RemoveKeepable_Item_Success()
        {
            using var container = CreateService(out var service);

            string group = $"group{new Random().Next(int.MinValue, int.MaxValue)}";
            const string name = "name";

            service.AddKeepableAsync(group, name).Wait();
            service.RemoveKeepableAsync(group).Wait();
        }

        [TestMethod]
        public void RemoveKeepable_Group_Success()
        {
            using var container = CreateService(out var service);

            string group = $"group{new Random().Next(int.MinValue, int.MaxValue)}";
            const string name = "name";

            service.AddKeepableAsync(group, name).Wait();
            service.RemoveKeepableAsync(group, name).Wait();
        }
    }
}
