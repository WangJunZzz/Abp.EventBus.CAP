using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Uow;

namespace EasyAbp.Abp.EventBus.Cap;

[Dependency(ReplaceServices = true)]
[ExposeServices(typeof(CapUnitOfWork), typeof(UnitOfWork), typeof(IUnitOfWork))]
public class CapUnitOfWork : UnitOfWork
{
    public CapUnitOfWork(IServiceProvider serviceProvider, IUnitOfWorkEventPublisher unitOfWorkEventPublisher,
        IOptions<AbpUnitOfWorkDefaultOptions> options) : base(serviceProvider, unitOfWorkEventPublisher, options)
    {
    }

    public override void AddTransactionApi(string key, ITransactionApi api)
    {
        var factories = ServiceProvider.GetServices<ICapTransactionApiFactory>();

        var factory = factories.FirstOrDefault(x => x.TransactionApiType == api.GetType());

        if (factory is not null)
        {
            api = factory.Create(api);
        }
        
        base.AddTransactionApi(key, api);
    }

    public override ITransactionApi GetOrAddTransactionApi(string key, Func<ITransactionApi> factory)
    {
        Check.NotNull(key, nameof(key));
        Check.NotNull(factory, nameof(factory));

        var transactionApi = FindTransactionApi(key);

        if (transactionApi is not null)
        {
            return transactionApi;
        }
        
        AddTransactionApi(key, factory());

        return FindTransactionApi(key);
    }
}