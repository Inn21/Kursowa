using System;
using Core.Model;

namespace Core.Feature
{
    public class BaseFeature : IDisposable
    {
        public virtual void Dispose()
        {
        }
    }
    public abstract class BaseFeature<TModel> : BaseFeature where TModel : BaseModel
    {
        protected TModel model;

        public virtual void Initialize(TModel model)
        {
            this.model = model;
            OnInitialize();
        }

        protected abstract void OnInitialize();
    }
}