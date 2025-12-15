using ReactiveUI;
using System;

namespace DC1AP.ViewModels
{
    public class ViewModelBase : ReactiveObject, IDisposable
    {
        public virtual void Dispose()
        {
        }
    }
}
