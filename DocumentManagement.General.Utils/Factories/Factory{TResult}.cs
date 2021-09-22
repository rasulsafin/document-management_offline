using System;

namespace Brio.Docs.General.Utils.Factories
{
    public class Factory<TResult> : IFactory<TResult>
    {
        private readonly Func<TResult> func;

        public Factory(Func<TResult> func)
            => this.func = func;

        public TResult Create()
            => func();
    }
}
