using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Dvs.Core.IoC
{
    public interface ICleanup
    {
        void Add(IDisposable disposable);
        void DisposeAll();
        void Dispose(IDisposable disposable);
    }

    public class Cleanup : ICleanup
    {
        private List<IDisposable> Disposables = new List<IDisposable>();

        public void Add(IDisposable disposable)
        {
            Disposables.Add(disposable);
        }

        public void Dispose(IDisposable disposable)
        {
            Disposables.Remove(disposable);
            disposable.Dispose();
        }

        public void DisposeAll()
        {
            foreach (var disposable in Disposables)
            {
                try
                {
                    Debug.WriteLine("ThreadCleanup.DisposeAll : " + disposable.GetType().Name);
                    disposable.Dispose();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("ThreadCleanup.DisposeAll Exception : " + ex.GetType());
                    Debug.WriteLine(" - " + ex.Message);
                }
            }
            Disposables.Clear();
        }

        [DiSetup]
        private static void DiSetup()
        {
            //Di.Bind<ICleanup>().AsSingleton<Cleanup>();
            Di<ICleanup>.Singleton.Bind<Cleanup>();
        }
    }
}
