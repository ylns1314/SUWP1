using SUWP1.Structures;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace SUWP1.UI
{
    public class ViewModelForum
    {
        public ObservableCollection<Forum> forums = new ObservableCollection<Forum>();
        private WebClient.JsonHandler jh;
        public ViewModelForum()
        {
            jh = new WebClient.JsonHandler();
        }

        public async void addForums()
        {
            List<Forum> lstForums = await jh.getForumList();
            forums = new ObservableCollection<Forum>(lstForums);
        }
    }
}
