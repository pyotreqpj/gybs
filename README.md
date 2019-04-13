# GYBS
GYBS is a set of small but useful things for any .NET Standard project. The idea behind is not to enforce anything - just grab the parts you're intrested in and help yourself.

## How to use it?

### Base
Base library, available at [NuGet](https://www.nuget.org/packages/Gybs), consists of three main parts:
* various extension methods
* `IResult` interface its implementation
* dependency injection utils

#### Extension methods
Example usage:
```
Task<bool> IsPresent(string str) => str.IsPresent().ToCompletedTask()
```

#### `IResult` and `Result`
`IResult` represents the operation result, where `Result` is an implementation of this interface.

Examples:
```
return Result.Success(new Model());
return new Model().ToSuccessfulResult();
```

```
var errors = new ResultErrorsDictionary();
errors.Add("Field1", "value");
errors.Add<Model>(m => m.Field2, "value");
return Result.Failure(errors.ToDictionary()).Map<Model>();
```

#### Dependency injection
Interfaces and attributes for component auto-registration.
```
class DummyScopedService : IScopedService {}

[SingletonService]
class DummySingletonService {}

serviceCollection.AddGybs(builder => {
    builder.AddInterfaceServices();
    builder.AddAttributeServices();
);
```

### Logic.Operations
Operations library, available at [NuGet](https://www.nuget.org/packages/Gybs.Logic.Operations), allows to decouple the operation contracts from the handler implementations. This is achieved by:
* separating `IOperation` from `IOperationHandler`
* putting the `IOperationBus` between, which takes the reponsibility of finding the correct handler for each operation

By default, library provides the `ServiceProviderOperationBus` which resolves the handler from DI container.

```
serviceCollection.AddGybs(builder => {
    builder.AddServiceProviderOperationBus();
    builder.AddOperationFactory();
    builder.AddOperationHandlers();
);

class DummyOperation : IOperation<string> {}

class DummyOperationHandler : IOperationHandler<DummyOperation, string>
{
    public async Task<IResult<string>> HandleAsync(DummyOperation operation) 
    {
        return "success".ToSuccessfulResult();
    }
}

IOperationFactory factory;

var result = await factory
    .Create<DummyOperation>()
    .HandleAsync();
```

### Logic.Cqrs
CQRS is a wrapper around Operations replacing `IOperation` and `IOperationHandler` with `IQuery/ICommand` and `IQueryHandler/ICommandHandler`.

### Logic.Events
Events library, available at [NuGet](https://www.nuget.org/packages/Gybs.Logic.Events), provides two basic interfaces for the events support: `IEvent` and `IEventBus`. Additionally, it provides the `InMemoryEventBus` implementation.

```
serviceCollection.AddGybs(builder => {
    builder.AddInMemoryEventBus();    
);

class Event : IEvent {}

IEventBus eventBus;
await eventBus.SubscribeAsync(e => Task.CompletedTask);
await eventBus.SendAsync(new Event());
```

### Logic.Validation
Validation, available at [NuGet](https://www.nuget.org/packages/Gybs.Logic.Validation), allows to separate validation logic from the rest of the application. This is achived by grouping the implementations of `IValidationRule` interface into the single validator by `IValidatorFactory`.

```
serviceCollection.AddGybs(builder => {
    builder.AddValidatorFactory();
    builder.AddValidationRules();    
);

class StringIsPresentRule : IValidationRule<string>
{
    public async Task<IResult> ValidateAsync(string str)
    {
        return result.IsPresent()
            ? Result.Success()
            : Result.Failure(null);
    }
}

IValidatorFactory factory;

var result = await factory
    .Require<StringIsPresentRule>
        .WithData("")
        .StopIfFailed()
    .Require<StringIsPresentRule>
        .WithData(null)
    .ValidateAsync();
```

### Data.Ef
Ef library, available at [NuGet](https://www.nuget.org/packages/Gybs.Data.Ef), provides the wrappers around `DbContext` and `DbSet<>` for grouping the extensions with the queries.

```
class Context : DbContext
{
    public DbSet<Model> Models { get; set;}
}

static class ModelQueries
{
    public static IQueryable<Model> Active(this DbSetQueries<Model> models)
    {
        return models.Entities.Where(m => m.Active);
    }
}

new Context().Models.Queries().Active();
```

### Data.Repositories
Repositories library, available at [NuGet](https://www.nuget.org/packages/Gybs.Data.Repositories), provides interfaces for repository and unit of work patterns.

## Why to use it?
I don't know. You need to figure it out yourself.