using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace MediatR.Extensions.Microsoft.DependencyInjection.Tests
{
    public class HomePage : ILayout
    {
        public HomePage()
        {

        }

        public HomePage(string header, string body, string footer)
        {
            Header = header;
            Body = body;
            Footer = footer;
        }

        public string? Header { get; set; }
        public string? Body { get; set; }
        public string? Footer { get; set; }
    }

    public class ChildTable : T_X
    {
        public int IDChild { get; set; }
    }

    public class ParentTable : T_XDomains
    {
        public int IDParent { get; set; }
        public int IDChild { get; set; }
    }

    public class QuestionsFAQDTO: IOutside
    {
        public int IDQuestion { get; set; }
        public string? Question { get; set; }
        public string? Answer { get; set; }
        public int Outside { get; set; }
    }

    public class GenericHandlersTest
    {
        public IServiceProvider _provider { get; set; }
        public GenericHandlersTest()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddMediatR(cfg => cfg.Using<MyCustomMediator>(), typeof(GenericHandler));
            services.RegisterGenericMediatRHandlers(typeof(SimpleGeneric.Handler<>).Assembly);
            _provider = services.BuildServiceProvider();
        }

        [Fact]
        public void ShouldResolveMediator()
        {
            _provider.GetService<IMediator>().ShouldNotBeNull();
            _provider.GetRequiredService<IMediator>().GetType().ShouldBe(typeof(MyCustomMediator));
        }

        [Fact]
        public void ShouldResolveSimpleGenericHandler()
        {
            _provider.GetService<IRequestHandler<SimpleGeneric.Query<HomePage>, Result<HomePage>>>().ShouldNotBeNull();
        }

        [Fact]
        public void ShouldResolveMaybeSimpleGenericHandler()
        {
            _provider.GetService<IRequestHandler<MaybeSimpleGeneric.Query<ChildTable, ParentTable>, List<MaybeSimpleGeneric.Data<ChildTable, ParentTable>>>>().ShouldNotBeNull();
        }

        [Fact]
        public void ShouldResolveComplexGenericHandler()
        {
            _provider.GetService<IRequestHandler<ComplexGeneric.Query<ChildTable, ParentTable, QuestionsFAQDTO>, Result<List<QuestionsFAQDTO>>>>().ShouldNotBeNull();
        }
    }
}
