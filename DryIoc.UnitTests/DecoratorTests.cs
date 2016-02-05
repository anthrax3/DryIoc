﻿using System;
using NUnit.Framework;

namespace DryIoc.UnitTests
{
    [TestFixture]
    public class DecoratorTests
    {
        [Test]
        public void Should_resolve_decorator()
        {
            var container = new Container();
            container.Register<IOperation, SomeOperation>();
            container.Register<IOperation, MeasureExecutionTimeOperationDecorator>(setup: Setup.Decorator);

            var decorator = container.Resolve<IOperation>();

            Assert.IsInstanceOf<MeasureExecutionTimeOperationDecorator>(decorator);
        }

        [Test]
        public void Should_resolve_decorator_of_decorator()
        {
            var container = new Container();
            container.Register<IOperation, SomeOperation>();
            container.Register<IOperation, MeasureExecutionTimeOperationDecorator>(setup: Setup.Decorator);
            container.Register<IOperation, RetryOperationDecorator>(setup: Setup.Decorator);

            var decorator = (RetryOperationDecorator)container.Resolve<IOperation>();

            Assert.IsInstanceOf<MeasureExecutionTimeOperationDecorator>(decorator.Decorated);
        }

        [Test]
        public void Should_resolve_decorator_for_named_service()
        {
            var container = new Container();
            container.Register<IOperation, SomeOperation>();
            container.Register<IOperation, AnotherOperation>(serviceKey: "Another");
            container.Register<IOperation, RetryOperationDecorator>(setup: Setup.Decorator);

            var decorator = (RetryOperationDecorator)container.Resolve<IOperation>("Another");

            Assert.IsInstanceOf<AnotherOperation>(decorator.Decorated);
        }

        [Test]
        public void Should_NOT_cache_decorator_so_it_could_decorated_another_service()
        {
            var container = new Container();
            container.Register<IOperation, SomeOperation>(serviceKey: "Some");
            container.Register<IOperation, AnotherOperation>(serviceKey: "Another");
            container.Register<IOperation, RetryOperationDecorator>(setup: Setup.Decorator);

            var some = (RetryOperationDecorator)container.Resolve<IOperation>("Some");
            var another = (RetryOperationDecorator)container.Resolve<IOperation>("Another");

            Assert.That(some.Decorated, Is.InstanceOf<SomeOperation>());
            Assert.That(another.Decorated, Is.InstanceOf<AnotherOperation>());
        }

        [Test]
        public void Should_resolve_generic_decorator()
        {
            var container = new Container();
            container.Register(typeof(IOperation<>), typeof(SomeOperation<>));
            container.Register(typeof(IOperation<>), typeof(MeasureExecutionTimeOperationDecorator<>), setup: Setup.Decorator);

            var decorator = container.Resolve<IOperation<string>>();

            Assert.That(decorator, Is.InstanceOf<MeasureExecutionTimeOperationDecorator<string>>());
        }

        [Test]
        public void Should_resolve_closed_service_with_open_generic_decorator()
        {
            var container = new Container();
            container.Register<IOperation<int>, SomeOperation<int>>();
            container.Register(typeof(IOperation<>), typeof(MeasureExecutionTimeOperationDecorator<>), setup: Setup.Decorator);

            var operation = container.Resolve<IOperation<int>>();

            Assert.That(operation, Is.InstanceOf<MeasureExecutionTimeOperationDecorator<int>>());
        }

        [Test]
        public void Should_resolve_generic_decorator_of_decorator()
        {
            var container = new Container();
            container.Register(typeof(IOperation<>), typeof(SomeOperation<>));
            container.Register(typeof(IOperation<>), typeof(MeasureExecutionTimeOperationDecorator<>), setup: Setup.Decorator);
            container.Register(typeof(IOperation<>), typeof(RetryOperationDecorator<>), setup: Setup.Decorator);

            var decorator = (RetryOperationDecorator<int>)container.Resolve<IOperation<int>>();

            Assert.That(decorator.Decorated, Is.InstanceOf<MeasureExecutionTimeOperationDecorator<int>>());
        }

        [Test]
        public void Should_resolve_generic_decorator_of_closed_decorator_of_generic_service()
        {
            var container = new Container();
            container.Register(typeof(IOperation<>), typeof(SomeOperation<>));
            container.Register(typeof(IOperation<int>), typeof(MeasureExecutionTimeOperationDecorator<int>), setup: Setup.Decorator);
            container.Register(typeof(IOperation<>), typeof(RetryOperationDecorator<>), setup: Setup.Decorator);

            var decorator = (RetryOperationDecorator<int>)container.Resolve<IOperation<int>>();

            Assert.That(decorator.Decorated, Is.InstanceOf<MeasureExecutionTimeOperationDecorator<int>>());
        }

        [Test]
        public void Resolve_could_NOT_select_closed_over_generic_decorator_cause_their_are_not_related()
        {
            var container = new Container();
            container.Register(typeof(IOperation<>), typeof(SomeOperation<>));
            container.Register(typeof(IOperation<>), typeof(MeasureExecutionTimeOperationDecorator<>), setup: Setup.Decorator);
            container.Register(typeof(IOperation<int>), typeof(MeasureExecutionTimeOperationDecorator<int>), setup: Setup.Decorator);

            var decorator = (MeasureExecutionTimeOperationDecorator<int>)container.Resolve<IOperation<int>>();

            Assert.That(decorator.Decorated, Is.InstanceOf<MeasureExecutionTimeOperationDecorator<int>>());
        }

        [Test]
        public void Should_resolve_decorator_array()
        {
            var container = new Container();
            container.Register<IOperation, SomeOperation>();
            container.Register<IOperation, AnotherOperation>();
            container.Register<IOperation, RetryOperationDecorator>(setup: Setup.Decorator);

            var ops = container.Resolve<IOperation[]>();

            Assert.That(ops[0], Is.InstanceOf<RetryOperationDecorator>());
            Assert.That(((RetryOperationDecorator)ops[0]).Decorated, Is.InstanceOf<SomeOperation>());
            Assert.That(ops[1], Is.InstanceOf<RetryOperationDecorator>());
            Assert.That(((RetryOperationDecorator)ops[1]).Decorated, Is.InstanceOf<AnotherOperation>());
        }

        [Test]
        public void Should_resolve_wrappers_of_decorator_array()
        {
            var container = new Container();
            container.Register<IOperation, SomeOperation>();
            container.Register<IOperation, AnotherOperation>();
            container.Register<IOperation, RetryOperationDecorator>(setup: Setup.Decorator);

            var ops = container.Resolve<Lazy<IOperation>[]>();

            Assert.That(ops[0].Value, Is.InstanceOf<RetryOperationDecorator>());
            Assert.That(((RetryOperationDecorator)ops[0].Value).Decorated, Is.InstanceOf<SomeOperation>());
            Assert.That(ops[1].Value, Is.InstanceOf<RetryOperationDecorator>());
            Assert.That(((RetryOperationDecorator)ops[1].Value).Decorated, Is.InstanceOf<AnotherOperation>());
        }

        [Test]
        public void Should_support_decorator_implementation_without_decorated_service_argument_in_constructor()
        {
            var container = new Container();
            container.Register<IOperation, SomeOperation>();
            container.Register<IOperation, AnotherOperation>(setup: Setup.Decorator);

            var operation = container.Resolve<IOperation>();
            //var operationExpr = container.Resolve<Container.DebugExpression<IOperation>>();

            Assert.That(operation, Is.InstanceOf<AnotherOperation>());
        }

        [Test]
        public void Replacing_decorator_reuse_may_different_from_decorated_service()
        {
            var container = new Container();
            container.Register<IOperation, SomeOperation>(Reuse.Singleton);
            container.Register<IOperation, AnotherOperation>(setup: Setup.Decorator);

            var first = container.Resolve<IOperation>();
            var second = container.Resolve<IOperation>();

            Assert.That(first, Is.InstanceOf<AnotherOperation>());
            Assert.That(first, Is.Not.SameAs(second));
        }

        [Test]
        public void Replacing_decorator_may_be_non_transient()
        {
            var container = new Container();
            container.Register<IOperation, SomeOperation>(Reuse.Singleton);
            container.Register<IOperation, AnotherOperation>(Reuse.Singleton, setup: Setup.Decorator);

            var first = container.Resolve<IOperation>();
            var second = container.Resolve<IOperation>();

            Assert.That(first, Is.InstanceOf<AnotherOperation>());
            Assert.That(first, Is.SameAs(second));
        }

        [Test]
        public void Normal_decorator_may_be_non_transient()
        {
            var container = new Container();
            container.Register<IOperation, SomeOperation>();
            container.Register<IOperation, RetryOperationDecorator>(Reuse.Singleton, setup: Setup.Decorator);

            var operation = container.Resolve<IOperation>();

            Assert.That(operation, Is.InstanceOf<RetryOperationDecorator>());
            Assert.That(((RetryOperationDecorator)operation).Decorated, Is.InstanceOf<SomeOperation>());
        }

        [Test]
        public void Should_support_decorator_of_decorator_without_decorated_service_argument_in_constructor()
        {
            var container = new Container();
            container.Register<IOperation, SomeOperation>();
            container.Register<IOperation, AnotherOperation>(setup: Setup.Decorator);
            container.Register<IOperation, RetryOperationDecorator>(setup: Setup.Decorator);

            var operation = container.Resolve<IOperation>();

            Assert.That(operation, Is.InstanceOf<RetryOperationDecorator>());
            Assert.That(((RetryOperationDecorator)operation).Decorated, Is.InstanceOf<AnotherOperation>());
        }

        [Test]
        public void Should_support_decorating_of_Lazy_service()
        {
            var container = new Container();
            container.Register<IOperation, SomeOperation>();
            container.Register<IOperation, LazyDecorator>(setup: Setup.Decorator);

            var operation = container.Resolve<IOperation>();

            Assert.That(operation, Is.InstanceOf<LazyDecorator>());
        }

        [Test]
        public void Should_support_decorating_of_Lazy_named_service()
        {
            var container = new Container();
            container.Register<IOperation, SomeOperation>(serviceKey: "some");
            container.Register<IOperation, LazyDecorator>(setup: Setup.Decorator);

            var operation = container.Resolve<IOperation>("some");

            Assert.That(operation, Is.InstanceOf<LazyDecorator>());
        }

        [Test]
        public void Should_apply_decorator_When_resolving_Func_of_decorated_service()
        {
            var container = new Container();
            container.Register<IOperation, SomeOperation>();
            container.Register<IOperation, MeasureExecutionTimeOperationDecorator>(setup: Setup.Decorator);

            var operation = container.Resolve<Func<IOperation>>();

            Assert.That(operation(), Is.InstanceOf<MeasureExecutionTimeOperationDecorator>());
        }

        [Test]
        public void Should_propagate_metadata_to_Meta_wrapper()
        {
            var container = new Container();
            container.Register(typeof(IOperation<>), typeof(SomeOperation<>), setup: Setup.With(metadataOrFuncOfMetadata: "blah"));
            container.Register(typeof(IOperation<>), typeof(SomeOperation<>), setup: Setup.With(metadataOrFuncOfMetadata: "blah"));
            container.Register(typeof(IOperation<>), typeof(MeasureExecutionTimeOperationDecorator<>), setup: Setup.Decorator);
            container.RegisterMany(new[] { typeof(OperationUser<>) });

            var user = container.Resolve<OperationUser<object>>();

            Assert.That(user.GetOperation.Metadata, Is.EqualTo("blah"));
            Assert.That(user.GetOperation.Value(), Is.InstanceOf<MeasureExecutionTimeOperationDecorator<object>>());
        }

        [Test]
        public void Possible_to_register_decorator_as_delegate_of_decorated_service()
        {
            var container = new Container();
            container.Register<IOperation, SomeOperation>();
            container.RegisterDelegateDecorator<IOperation>(_ => op => new MeasureExecutionTimeOperationDecorator(op));

            var operation = container.Resolve<IOperation>();

            Assert.That(operation, Is.InstanceOf<MeasureExecutionTimeOperationDecorator>());
        }

        [Test]
        public void Possible_to_register_decorator_as_delegate_of_decorated_service_with_additional_dependencies_resolved_from_Container()
        {
            var container = new Container();
            container.Register<IOperation, SomeOperation>();
            container.Register<IMeasurer, Measurer>();
            container.RegisterDelegateDecorator<IOperation>(r => 
                op => MeasureExecutionTimeOperationDecorator.MeasureWith(op, r.Resolve<IMeasurer>()));

            var operation = container.Resolve<IOperation>();

            Assert.That(operation, Is.InstanceOf<MeasureExecutionTimeOperationDecorator>());
        }

        [Test]
        public void Should_support_decorator_of_service_registered_with_delegate()
        {
            var container = new Container();
            container.RegisterDelegate<IOperation>(_ => new SomeOperation());
            container.Register<IOperation, MeasureExecutionTimeOperationDecorator>(setup: Setup.Decorator);

            var operation = container.Resolve<IOperation>();

            Assert.That(operation, Is.InstanceOf<MeasureExecutionTimeOperationDecorator>());
        }

        [Test]
        public void Should_support_decorator_of_decorator_registered_with_delegates()
        {
            var container = new Container();
            container.Register<IOperation, SomeOperation>();
            container.RegisterDelegateDecorator<IOperation>(r => op => new RetryOperationDecorator(op));
            container.RegisterDelegateDecorator<IOperation>(r => op => new MeasureExecutionTimeOperationDecorator(op));

            var operation = container.Resolve<IOperation>();

            Assert.That(operation, Is.InstanceOf<MeasureExecutionTimeOperationDecorator>());
            Assert.That(((MeasureExecutionTimeOperationDecorator)operation).Decorated, Is.InstanceOf<RetryOperationDecorator>());
        }

        [Test]
        public void When_mixing_Type_and_Delegate_decorators_the_registration_order_is_preserved()
        {
            var container = new Container();
            container.Register<IOperation, SomeOperation>();
            container.Register<IOperation, RetryOperationDecorator>(setup: Setup.Decorator);
            container.RegisterDelegateDecorator<IOperation>(r => op => new MeasureExecutionTimeOperationDecorator(op));
            container.Register<IOperation, AsyncOperationDecorator>(setup: Setup.Decorator);

            var operation = container.Resolve<IOperation>();

            Assert.IsInstanceOf<AsyncOperationDecorator>(operation);

            var decorated1 = ((AsyncOperationDecorator)operation).Decorated();
            Assert.IsInstanceOf<MeasureExecutionTimeOperationDecorator>(decorated1);

            var decorated2 = ((MeasureExecutionTimeOperationDecorator)decorated1).Decorated;
            Assert.IsInstanceOf<RetryOperationDecorator>(decorated2);
        }

        [Test]
        public void Delegate_decorator_will_use_decoratee_reuse()
        {
            var container = new Container();
            container.Register<IOperation, SomeOperation>(Reuse.Singleton);
            container.RegisterDelegateDecorator<IOperation>(r => 
                op => new MeasureExecutionTimeOperationDecorator(op));

            var operation = container.Resolve<IOperation>();

            Assert.IsInstanceOf<MeasureExecutionTimeOperationDecorator>(operation);
            Assert.AreSame(operation, container.Resolve<IOperation>());
        }

        [Test]
        public void Should_support_resolving_Func_with_parameters_of_decorated_service()
        {
            var container = new Container();
            container.Register<IOperation, ParameterizedOperation>();
            container.Register<IOperation, RetryOperationDecorator>(setup: Setup.Decorator);

            var operation = container.Resolve<Func<object, IOperation>>();

            Assert.That(operation("blah"), Is.InstanceOf<RetryOperationDecorator>());
        }

        [Test]
        public void Should_support_resolving_Func_with_parameters_without_decorated_service_argument_in_constructor()
        {
            var container = new Container();
            container.Register<IOperation, ParameterizedOperation>();
            container.Register<IOperation, AnotherOperation>(setup: Setup.Decorator);

            var operation = container.Resolve<Func<object, IOperation>>();

            Assert.That(operation("blah"), Is.InstanceOf<AnotherOperation>());
        }

        [Test]
        public void Should_allow_Register_and_Resolve_of_two_decorators_of_the_same_type()
        {
            var container = new Container();
            container.RegisterDelegate<IOperation>(_ => new SomeOperation());
            container.Register<IOperation, MeasureExecutionTimeOperationDecorator>(setup: Setup.Decorator);
            container.Register<IOperation, MeasureExecutionTimeOperationDecorator>(setup: Setup.Decorator);

            var operation = container.Resolve<IOperation>();

            Assert.That(operation, Is.InstanceOf<MeasureExecutionTimeOperationDecorator>());
            Assert.That(((MeasureExecutionTimeOperationDecorator)operation).Decorated, Is.InstanceOf<MeasureExecutionTimeOperationDecorator>());
        }

        [Test]
        public void Should_support_multiple_decorator_in_object_graph()
        {
            var container = new Container();
            container.Register(typeof(IOperation<>), typeof(SomeOperation<>), setup: Setup.With(metadataOrFuncOfMetadata: "some"));
            container.Register(typeof(IOperation<>), typeof(RetryOperationDecorator<>), setup: Setup.Decorator);

            container.Register<IOperationUser<int>, OperationUser<int>>();
            container.Register(typeof(IOperationUser<>), typeof(LogUserOps<>), setup: Setup.Decorator);

            var user = container.Resolve<IOperationUser<int>>();
            Assert.That(user, Is.InstanceOf<LogUserOps<int>>());
            Assert.That(((LogUserOps<int>)user).Decorated, Is.InstanceOf<OperationUser<int>>());

            var operation = user.GetOperation.Value();
            Assert.That(operation, Is.InstanceOf<RetryOperationDecorator<int>>());
            Assert.That(((RetryOperationDecorator<int>)operation).Decorated, Is.InstanceOf<SomeOperation<int>>());
        }

        [Test]
        public void Should_support_decorator_of_Func_with_parameters()
        {
            var container = new Container();
            container.Register<IOperation, ParameterizedOperation>();
            container.Register<IOperation, FuncWithArgDecorator>(setup: Setup.Decorator);

            var func = container.Resolve<Func<object, IOperation>>();
            var operation = func("hey");
            Assert.That(operation, Is.InstanceOf<FuncWithArgDecorator>());

            var decoratedFunc = ((FuncWithArgDecorator)operation).DecoratedFunc("hey");
            Assert.That(decoratedFunc, Is.InstanceOf<ParameterizedOperation>());
        }

        [Test]
        public void May_decorate_func_of_service()
        {
            var container = new Container();

            container.Register<IOperation, SomeOperation>(Reuse.Singleton);
            container.Register<IOperation, AsyncOperationDecorator>(setup: Setup.Decorator);

            var a = container.Resolve<IOperation>();
            Assert.IsInstanceOf<AsyncOperationDecorator>(a);

            var decorated = ((AsyncOperationDecorator)a).Decorated();
            Assert.IsInstanceOf<SomeOperation>(decorated);
        }

        [Test]
        public void May_next_func_decorator_inside_other_decorator()
        {
            var container = new Container();

            container.Register<IOperation, SomeOperation>(Reuse.Singleton);
            container.Register<IOperation, AsyncOperationDecorator>(Reuse.Singleton, setup: Setup.Decorator);
            container.Register<IOperation, RetryOperationDecorator>(Reuse.Singleton, setup: Setup.Decorator);

            var a = container.Resolve<IOperation>();
            Assert.IsInstanceOf<RetryOperationDecorator>(a);

            var nestedDecorator = ((RetryOperationDecorator)a).Decorated;
            Assert.IsInstanceOf<AsyncOperationDecorator>(nestedDecorator);

            var getOp = ((AsyncOperationDecorator)nestedDecorator).Decorated;

            var decorated = getOp();
            Assert.IsInstanceOf<SomeOperation>(decorated);
            Assert.AreSame(decorated, getOp());
        }

        [Test]
        public void Removing_decorator_before_chaining_it_with_lazy_decorator()
        {
            var container = new Container();

            container.Register<IOperation, SomeOperation>();
            container.Register<IOperation, RetryOperationDecorator>(setup: Setup.Decorator);
            container.Register<IOperation, LazyDecorator>(setup: Setup.Decorator);
            container.Register<IOperation, AsyncOperationDecorator>(setup: Setup.Decorator);

            var op = container.Resolve<IOperation>();
            op = ((AsyncOperationDecorator)op).Decorated();
            Assert.IsInstanceOf<LazyDecorator>(op);

            container.Unregister<IOperation>(factoryType: FactoryType.Decorator,
                condition: factory => factory.ImplementationType == typeof(LazyDecorator));

            op = ((LazyDecorator)op).Decorated.Value;
            Assert.IsInstanceOf<RetryOperationDecorator>(op);
        }

        [Test]
        public void Can_decorate_service_type_when_required_type_is_different()
        {
            var container = new Container();
            container.Register<IBird, TalkingBirdDecorator>(setup: Setup.Decorator);
            container.Register<Duck>();

            var bird = container.Resolve<IBird>(typeof(Duck));

            Assert.IsInstanceOf<TalkingBirdDecorator>(bird);
        }

        [Test, Ignore]
        public void Can_register_custom_Disposer()
        {
            var container = new Container();
            container.Register<Foo>(Reuse.Singleton);

            container.Register<FooDisposer>(
                setup: Setup.With(useParentReuse: true));

            container.Register(
                Made.Of(() => CustomDisposer.WithDispose(Arg.Of<Foo>(), Arg.Of<FooDisposer>())),
                setup: Setup.DecoratorWith(useDecorateeReuse: true));

            var foo = container.Resolve<Foo>();

            container.Dispose();
            Assert.IsTrue(foo.IsReleased);
        }

        [Test]
        public void Decorator_created_by_factory_should_be_compasable_with_other_decorator()
        {
            var container = new Container();
            container.Register<A>();

            container.Register<FB>();
            container.Register(
                Made.Of(r => ServiceInfo.Of<FB>(),
                f => f.Decorate(Arg.Of<A>())),
                setup: Setup.Decorator);

            container.Register<A, C>(setup: Setup.Decorator);

            var a = container.Resolve<A>();
            Assert.IsInstanceOf<C>(a);

            var c = (C)a;
            Assert.IsInstanceOf<B>(c.A);
        }

        public class A { }

        public class FB : A
        {
            public A Decorate(A a)
            {
                return new B(a);
            }     
        }

        class B : A
        {
            public A A { get; private set; }

            public B(A a)
            {
                A = a;
            }
        }

        class C : A
        {
            public A A { get; private set; }

            public C(A a)
            {
                A = a;
            }
        }


        public class Foo
        {
            public bool IsReleased { get; set; }
        }

        public class FooDisposer : Disposer<Foo>
        {
            public override void Dispose()
            {
                Item.IsReleased = true;
            }
        }

        public static class CustomDisposer
        {
            public static T WithDispose<T>(T foo, Disposer<T> disposer)
            {
                disposer.TrackForDispose(foo);
                return foo;
            }
        }

        public abstract class Disposer<T> : IDisposable
        {
            protected T Item;

            public void TrackForDispose(T item)
            {
                Item = item;
            }

            public abstract void Dispose();
        }

        public interface IBird {}

        public class Duck : IBird {}

        public class TalkingBirdDecorator : IBird
        {
            public IBird Decoratee { get; private set; }

            public TalkingBirdDecorator(IBird bird)
            {
                Decoratee = bird;
            }
        }
    }

    #region CUT

    public interface IOperationUser<T>
    {
        Meta<Func<IOperation<T>>, string> GetOperation { get; }
    }

    public class LogUserOps<T> : IOperationUser<T>
    {
        public readonly IOperationUser<T> Decorated;
        public Meta<Func<IOperation<T>>, string> GetOperation { get { return Decorated.GetOperation; } }

        public LogUserOps(IOperationUser<T> decorated)
        {
            Decorated = decorated;
        }
    }

    public class OperationUser<T> : IOperationUser<T>
    {
        public Meta<Func<IOperation<T>>, string> GetOperation { get; set; }

        public OperationUser(Meta<Func<IOperation<T>>, string> getOperation)
        {
            GetOperation = getOperation;
        }
    }

    public interface IOperation
    {
    }

    public class SomeOperation : IOperation
    {
    }

    public class AnotherOperation : IOperation
    {
    }

    public class ParameterizedOperation : IOperation
    {
        public object Param { get; set; }

        public ParameterizedOperation(object param)
        {
            Param = param;
        }
    }

    public class MeasureExecutionTimeOperationDecorator : IOperation
    {
        public IOperation Decorated;

        public MeasureExecutionTimeOperationDecorator(IOperation operation)
        {
            Decorated = operation;
        }

        public static IOperation MeasureWith(IOperation operation, IMeasurer measurer)
        {
            return new MeasureExecutionTimeOperationDecorator(operation) { Measurer = measurer };
        }

        public IMeasurer Measurer { get; set; }
    }

    public interface IMeasurer
    {
    }

    public class Measurer : IMeasurer
    {
    }

    public class RetryOperationDecorator : IOperation
    {
        public IOperation Decorated;

        public RetryOperationDecorator(IOperation operation)
        {
            Decorated = operation;
        }
    }

    public class AsyncOperationDecorator : IOperation
    {
        public readonly Func<IOperation> Decorated;

        public AsyncOperationDecorator(Func<IOperation> a)
        {
            Decorated = a;
        }
    }

    public interface IOperation<T>
    {
    }

    public class SomeOperation<T> : IOperation<T>
    {
    }

    public class RetryOperationDecorator<T> : IOperation<T>
    {
        public IOperation<T> Decorated;

        public RetryOperationDecorator(IOperation<T> operation)
        {
            Decorated = operation;
        }
    }

    public class MeasureExecutionTimeOperationDecorator<T> : IOperation<T>
    {
        public IOperation<T> Decorated;

        public MeasureExecutionTimeOperationDecorator(IOperation<T> operation)
        {
            Decorated = operation;
        }
    }

    public class LazyDecorator : IOperation
    {
        public Lazy<IOperation> Decorated;

        public LazyDecorator(Lazy<IOperation> decorated)
        {
            Decorated = decorated;
        }
    }

    public class FuncWithArgDecorator : IOperation
    {
        public Func<object, IOperation> DecoratedFunc;

        public FuncWithArgDecorator(Func<object, IOperation> decoratedFunc)
        {
            DecoratedFunc = decoratedFunc;
        }
    }

    #endregion
}
