using ProjectLighthouse.Model;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ProjectLighthouse.ViewModel
{
    public class LatheViewModel : BaseViewModel
    {
        private List<Lathe> lathes;
        public List<Lathe> Lathes
        {
            get { return lathes; }
            set 
            { 
                lathes = value;
                OnPropertyChanged();
            }
        }


        public LatheViewModel()
        {
            GetData();
        }

        private void GetData()
        {
            Lathes = DatabaseHelper.Read<Lathe>().ToList();
        }
    }
}
