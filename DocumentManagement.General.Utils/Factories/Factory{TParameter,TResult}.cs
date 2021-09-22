using System;

namespace Brio.Docs.General.Utils.Factories
{
    public class Factory<TParameter, TResult> : IFactory<TParameter, TResult>
    {
        private readonly Func<TParameter, TResult> func;

        public Factory(Func<TParameter, TResult> func)
            => this.func = func;

        public TResult Create(TParameter parameter)
            => func(parameter);
    }
}
