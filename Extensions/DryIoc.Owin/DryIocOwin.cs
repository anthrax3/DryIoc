﻿/*
The MIT License (MIT)

Copyright (c) 2014 Maksim Volkau

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

namespace DryIoc.Owin
{
    using System.Threading.Tasks;
    using global::Owin;
    using Microsoft.Owin;

    public static class DryIocOwin
    {
        public static void UseDryIocMiddleware(this IAppBuilder app, IContainer container)
        {
            container = container.With(scopeContext: new AsyncExecutionFlowScopeContext());
            app.Use<DryIocMiddleware>(container);
            //var registeredMiddleware = container.Resolve<Func<OwinMiddleware, OwinMiddleware>[]>();
            //foreach (var getMiddleware in registeredMiddleware)
            //{
            //    app.Use((context, next) =>
            //    {
            //        getMiddleware(context.)
            //    })
            //}
        }
    }

    public class DryIocMiddleware : OwinMiddleware
    {
        public DryIocMiddleware(OwinMiddleware next, IContainer container)
            : base(next)
        {
            _container = container;
        }

        public async override Task Invoke(IOwinContext context)
        {
            using (_container.OpenScope())
                await Next.Invoke(context);
        }

        private readonly IContainer _container;
    }

    public static class WebReuse
    {
        public static readonly IReuse InRequest = Reuse.InCurrentNamedScope(AsyncExecutionFlowScopeContext.ROOT_SCOPE_NAME);
    }
}
