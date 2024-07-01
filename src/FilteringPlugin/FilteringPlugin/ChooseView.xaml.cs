using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FilteringPlugin
{
    /// <summary>
    /// Interaction logic for ChooseView.xaml
    /// </summary>
    public partial class ChooseView : Window
    {
        public bool Continue = true;
        public List<string> ViewName = new List<string>();
        public string Filter = "";
        public bool OverrideLine = true;
        public ChooseView(List<string> ViewNames)
        {
            InitializeComponent();

            #region Create StackPanels
            List<StackPanel> Stacks= new List<StackPanel>();

            foreach(string ViewName in ViewNames)
            {
                StackPanel Stack = new StackPanel();
                Stack.Orientation = Orientation.Horizontal;
                Stack.Margin = new Thickness(0, 1, 0, 1);



                CheckBox View=new CheckBox();
                View.Content=ViewName;
                View.DataContext = ViewName;
                View.Width = 15;
                Stack.Children.Add(View);


                TextBlock ViewText = new TextBlock();
                ViewText.Margin = new Thickness(2, -1.5, 0, 0);
                ViewText.Text = ViewName;
                Stack.Children.Add(ViewText);

                
                Stacks.Add(Stack);    
            }
            #endregion
            ListOfSheetsView.ItemsSource= Stacks;
            Category.IsChecked= true;
            System.Drawing.Image img = Properties.Resources.Colorby;
            this.Icon = GetImageSource(img);
        }

        private BitmapSource GetImageSource(System.Drawing.Image img)
        {
            BitmapImage bmp = new BitmapImage();

            using (MemoryStream ms = new MemoryStream())
            {
                img.Save(ms, ImageFormat.Png);
                ms.Position = 0;
                bmp.BeginInit();
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.UriSource = null;
                bmp.StreamSource = ms;
                bmp.EndInit();
            }
            return bmp;
        }

        private void SelectionSets_Loaded(object sender, RoutedEventArgs e)
        {
           // ViewName = SelectionSets.SelectedValue.ToString();
        }

        private void SelectionSets_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //ViewName = SelectionSets.SelectedValue.ToString();
        }

        private void SelectionSets_DropDownOpened(object sender, EventArgs e)
        {
        }

        private void Category_Checked(object sender, RoutedEventArgs e)
        {
            Filter = "Category";
        }

        private void Family_Checked(object sender, RoutedEventArgs e)
        {
            Filter = "Family";
        }

        private void FamilyType_Checked(object sender, RoutedEventArgs e)
        {
            Filter = "Type";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var StackList = ListOfSheetsView.ItemsSource;
            foreach (StackPanel Stack in StackList)
            {
                foreach(var Item in Stack.Children)
                {
                    if(Item is CheckBox)
                    {
                        bool Checked = (bool)(Item as CheckBox).IsChecked;
                        if(Checked)
                        {
                            foreach(var Item2 in Stack.Children)
                            {
                                if(Item2 is TextBlock)
                                {
                                    TextBlock TextBlock = (TextBlock)Item2;
                                    ViewName.Add(TextBlock.Text);
                                }
                            }
                        }
                    }
                }
            }
            OverrideLine = (bool)OverrideLineCheck.IsChecked;
            Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Continue = false;
            Close();
        }
    }
}
