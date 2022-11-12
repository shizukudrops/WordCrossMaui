using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordCrossMaui
{
    class MyViewCell : ViewCell
    {
        public MyViewCell() : base()
        {
            var label = new Label();
            label.SetBinding(Label.TextProperty, new Binding("Name"));

            var layout = new StackLayout()
            {
                Children =
                {
                    label
                },
                Padding = new Thickness(10, 5, 0, 5)
            };

            layout.SetBinding(Layout.BackgroundColorProperty, new Binding("BackgroundColor"));

            View = layout;
        }
    }
}
