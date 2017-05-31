using SUWP1.Structures;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SUWP1.UI
{
    public class ViewModelSubForum
    {
        public ObservableCollection<Thread> threads = new ObservableCollection<Thread>();

        public void newThreads(List<Thread> lstThreads)
        {
            threads.Clear();
            this.addThreads(lstThreads);
        }

        public void addThreads(List<Thread> lstThreads)
        {
            foreach (Thread t in lstThreads) threads.Add(t);
        }
    }
}
