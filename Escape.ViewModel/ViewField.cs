using System;
using System.ComponentModel;
using System.Windows.Media;
using System.Runtime.CompilerServices;

namespace Escape.ViewModel
{
    public class ViewField : ViewModelBase
    {
        Color color;
        public Color Color
        {
            get { return color; }
            set
            {
                if (color != value)
                {
                    color = value;
                    OnPropertyChanged();
                }
            }
        }
    }
}
