using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3DModel
{
    public class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propName)
        {
            if(this.PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }

        protected bool SetValue<T>(ref T fieldValue, T newValue, string propName)
        {
            if (object.Equals(fieldValue, newValue))
                return false;

            fieldValue = newValue;

            OnPropertyChanged(propName);
            return true;
        }
    }
}
