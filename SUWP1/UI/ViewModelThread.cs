using SUWP1.Structures;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SUWP1.UI
{
    public class ViewModelThread
    {
        public ObservableCollection<Post> posts = new ObservableCollection<Post>();
        
        public void newPosts(List<Post> lstPosts)
        {
            posts.Clear();
            this.addPosts(lstPosts);
        }

        public void addPosts(List<Post> lstPosts)
        {
            foreach (Post p in lstPosts) posts.Add(p);
        }
    }
}
